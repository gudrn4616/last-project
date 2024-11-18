import { fromBinary, create } from '@bufbuild/protobuf';
import { ePacketId } from 'ServerCore/src/network/packetId.js';
import { Session } from 'ServerCore/src/network/session.js';
import { CustomError } from 'ServerCore/src/utils/error/customError.js';
import { ErrorCodes } from 'ServerCore/src/utils/error/errorCodes.js';
import { PacketUtils } from 'ServerCore/src/utils/packetUtils.js';
import { C2L_GameStartSchema, L2B_CreateGameRoomRequestSchema, C2L_JoinRoomRequestSchema, C2L_CreateRoomRequestSchema, C2L_LeaveRoomRequestSchema, C2L_GetRoomListRequestSchema, L2C_GetRoomListResponseSchema, L2C_CreateRoomResponseSchema, L2C_JoinRoomResponseSchema, L2C_LeaveRoomResponseSchema, L2C_LeaveRoomNotificationSchema, L2C_JoinRoomNotificationSchema, B2L_CreateGameRoomResponeSchema } from 'src/protocol/room_pb.js';
import { battleSessionManager } from 'src/server.js';


const MAX_ROOMS_SIZE = 10000;

class RoomManager {
  constructor() {
    this.rooms = new Map();
    this.availableRoomIds = Array.rom({ length: MAX_ROOMS_SIZE }, (_, i) => i + 1);
  }

  createRoomHandler(buffer, session) {
    console.log('createRoomHandler');
    // 클라이언트가 보낸 패킷 역직렬화
    const packet = fromBinary(C2L_CreateRoomRequestSchema, buffer);
    let tmpRoomId = this.availableRoomIds.shift() || 0;

    if (packet.maxUserNum > 4) {
      throw new CustomError(ErrorCodes.CREATE_ROOM_FAILED, '방 인원수는 최대 4명 이상이어야 합니다.');
    }

    const newRoom = new Room(tmpRoomId, packet.name, packet.maxUserNum);
    this.rooms.set(tmpRoomId, newRoom);

    // 응답 정보 생성
    const responsePacket = create(L2C_CreateRoomResponseSchema, { isSuccess: true, room: newRoom, failCode: 0 });
    const response = PacketUtils.SerializePacket(
      responsePacket,
      ePacketId.S2CCreateRoomResponse,//수정할 부분
      session.getId(),
      session.getSequence()
    );
    session.send(response);
  }

  /**---------------------------------------------
    [방 입장]
    * @param {Buffer} buffer
    * @param {LobbySession} session
---------------------------------------------*/
  enterRoomHandler(buffer, session) {
    console.log('enterRoomHandler');

    // 클라이언트가 보낸 패킷 역직렬화
    const packet = fromBinary(C2L_JoinRoomRequestSchema, buffer);

    // 방 ID를 통해 해당 방을 가져오기
    const room = this.rooms.get(packet.roomId);
    if (!room) {
      throw new CustomError(ErrorCodes.ROOM_NOT_FOUND, '방을 찾을 수 없습니다.');
    }

    if (room.getCurrentUsersCount() >= room.getMaxUsersCount()) {
      throw new CustomError(ErrorCodes.ROOM_FULL, '방이 가득 찼습니다.');
    }

    const success = room.enterRoom(session);
    const responsePacket = create(L2C_JoinRoomResponseSchema, { isSuccess: success, room, failCode: 0 });
    // 유저에게 방 입장 성공 알림
    const response = PacketUtils.SerializePacket(
      responsePacket,
      L2C_JoinRoomResponseSchema,
      ePacketId.L2C_EnterRoomMe,
      session.getNextSequence()
    );

    const otherResponsePacket = create(L2C_JoinRoomNotificationSchema, { joinUser: session.getNickname() });

    const otherResponse = PacketUtils.SerializePacket(
      otherResponsePacket,
      L2C_JoinRoomNotificationSchema,
      ePacketId.L2C_EnterRoomOther,
      session.getNextSequence()
    );

    room.broadcast(otherResponse);
  }

  /**---------------------------------------------
    [방 퇴장]

    * @param {Buffer} buffer
   * @param {LobbySession} session
---------------------------------------------*/
  leaveRoomHandler(buffer, session) {
    console.log('leaveRoomHandler');
    const packet = fromBinary(C2L_LeaveRoomRequestSchema, buffer);
    const room = this.rooms.get(packet.roomId);
    if (!room) {
      throw new CustomError(ErrorCodes.ROOM_NOT_FOUND, '방을 찾을 수 없습니다.');
    }

    const success = room.leaveRoom(session);
    if (!success) {
      throw new CustomError(ErrorCodes.LEAVE_ROOM_FAILED, '방 퇴장에 실패했습니다.');
    } else {
      // 퇴장한 유저에게 퇴장 응답
      const responsePacket = create(L2C_LeaveRoomResponseSchema, { isSuccess: true, failCode: 0 });
      const response = PacketUtils.SerializePacket(
        responsePacket,
        L2C_LeaveRoomResponseSchema,
        ePacketId.L2C_LeaveRoom,
        session.getNextSequence()
      );

      // 퇴장한 유저를 다른 유저에게 통지
      const otherResponsePacket = create(L2C_LeaveRoomNotificationSchema, { userId: session.getId() });
      const otherResponse = PacketUtils.SerializePacket(
        otherResponsePacket,
        L2C_LeaveRoomNotificationSchema,
        ePacketId.L2C_LeaveRoomOther,
        session.getNextSequence()
      );
      room.broadcast(otherResponse, session);
    }
  }

  /**---------------------------------------------
    [방 목록 조회] - 방 목록을 순회하면서 RoomInfo 메시지 생성

    * @param {Buffer} buffer
    * @param {LobbySession | BattleSession} session
---------------------------------------------*/
  getRoomsHandler(buffer, session) {
    console.log('getRoomsHandler');

    const packet = fromBinary(C2L_GetRoomListRequestSchema, buffer);

    // 방 목록 정보 생성
    const roomsData = [];
    this.rooms.forEach((room, roomId) => {
      const roomData = {
        id: roomId,
        ownerId: room.getOwnerId(),
        name: room.getName(),
        maxUserNum: room.getMaxUsersCount(),
        state: room.getState(),
        users: room.getUsers().map(user => ({
          id: user.getId(),
          nickname: user.getNickname(),
        })),
      };
      roomsData.push(roomData);
    });

    // 방 목록 응답
    const responsePacket = create(L2C_GetRoomListResponseSchema, { rooms: roomsData });
    const response = PacketUtils.SerializePacket(
      responsePacket,
      L2C_GetRoomListResponseSchema,
      ePacketId.L2C_RoomList,
      session.getNextSequence()
    );
    session.send(response);
  }

  /**---------------------------------------------
   * [게임 시작] - 배틀서버에게 게임 방 생성 요청

    // 1. 클라 -> 로비: 게임 시작 요청
    // 2. 로비 -> 배틀: 방 생성 요청
    // 3. 배틀 -> 로비: 방 생성 완료 통지

    // 4. 로비 -> 클라: 게임 시작 응답(배틀 서버의 주소, port, gameRoomID)
    
    // 5. 클라 -> 배틀: 접속
    // 6. 클라 -> 배틀: initialPacket전송 
    // 7. 클라 -> 배틀: 방 입장 요청
    // 8. 배틀 -> 클라: 모든 유저가 접속 시 게임 시작 통지

   * @param {Buffer} buffer
   * @param {LobbySession | BattleSession} session
   ---------------------------------------------*/
  gameStartHandler(buffer, session) {
    console.log('gameStartHandler');

    //로비서버와 배틀서버가 1개씩만 존재(임시)
    const battleSession = battleSessionManager.getSessionOrNull('battleServerSession'); //임시. 로드 밸런서에서 배운 것처럼 여러 배틀세션에 나눠서 요청하기

    if (!battleSession) {
      console.log('!BattleServerSession을 찾을 수 없습니다.');
      throw new CustomError(ErrorCodes.SOCKET_ERROR, 'BattleServerSession을 찾을 수 없습니다.');
    }

    //클라가 보낸 패킷 역직렬화(decoding)
    const packet = fromBinary(C2L_GameStartSchema, buffer);

    //생성되어 있는 방들 중 roomId를 통해 해당 room을 가져오기
    const room = this.rooms.get(packet.roomId);
    if (room == undefined) {
      console.log('방을 찾을 수 없습니다.');
      throw new CustomError(ErrorCodes.SOCKET_ERROR, 'invalid roomId.');
    }

    //배틀 서버에게 방 생성 요청하기
    const L2BPacket = create(L2B_CreateGameRoomRequestSchema, {
      roomId: packet.roomId,
      maxPlayers: room.getCurrentUsersCount(),
    });

    //패킷 직렬화
    const sendBuffer = PacketUtils.SerializePacket(
      L2BPacket,
      L2B_CreateGameRoomRequestSchema,
      ePacketId.L2B_CreateRoom,
      session.getNextSequence(),
    );

    //배틀 서버에게 전송
    battleSession.send(sendBuffer);
  }

  /**---------------------------------------------
   * [게임 시작2] - 클라에게 배틀 서버의 주소와 포트번호, 게임 방ID 전송
   * @param {Buffer} buffer
   * @param {LobbySession | BattleSession} session
   * // 4. 로비 -> 클라: 게임 시작 응답(배틀 서버의 주소, port, gameRoomID)
   * 
   * // 이것 왜 있는건지 모르겠음
   ---------------------------------------------*/
  onGameStartHandler(buffer, session) {
    //B2L_CreateRoomSchema(수신받은  packetId)
    const packet = fromBinary(B2L_CreateGameRoomResponeSchema, buffer);
    const { ip, port, Id } = packet;

    //L2C_GameStartSchema(송신할 packetId)
    //무엇인지 몰라 일딴 위 주석에 있는것으로 만들어 놈
    const responsePacket = create(L2C_GameStartSchema, { ip, port, Id });
    const response = PacketUtils.SerializePacket(
      responsePacket,
      L2C_GameStartSchema,
      ePacketId.L2C_GameStart,
      session.getNextSequence()
    );
    this.rooms.get(Id).broadcast(response);
  }

  /**---------------------------------------------
   * [방 ID 해제] - 사용하지 않는 방 ID를 큐에 반환하여 재사용 가능하게 만듦
   * @param {number} roomId
   ---------------------------------------------*/
  freeRoomId(roomId) {
    if (!this.rooms.has(roomId)) {
      console.log('유효하지 않은 roomID');
      throw new CustomError(ErrorCodes.SOCKET_ERROR, '유효하지 않은 roomID');
    }

    this.rooms.delete(roomId);
    this.availableRoomIds.push(roomId);
  }
}

export const roomManager = new RoomManager();
