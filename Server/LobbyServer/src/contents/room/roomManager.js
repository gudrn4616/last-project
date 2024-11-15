import { fromBinary } from '@bufbuild/protobuf';
import { ePacketId } from 'ServerCore/src/network/packetId.js';
import { Session } from 'ServerCore/src/network/session.js';
import { CustomError } from 'ServerCore/src/utils/error/customError.js';
import { ErrorCodes } from 'ServerCore/src/utils/error/errorCodes.js';
import { PacketUtils } from 'ServerCore/src/utils/packetUtils.js';
import { C2L_GameStartSchema, L2B_CreateGameRoomRequestSchema } from 'src/protocol/room_pb.js';
import { battleSessionManager } from 'src/server.js';

const MAX_ROOMS_SIZE = 10000;

class RoomManager {
  constructor() {
    this.rooms = new Map();
    this.availableRoomIds = Array.rom({ length: MAX_ROOMS_SIZE }, (_, i) => i + 1);

    let tmpRoomId = this.availableRoomIds.shift() || 0;
    this.rooms.set(tmpRoomId, new Room(tmpRoomId, '테스트 방', 2));
  }
  /**---------------------------------------------
    [방 입장]
    * @param {Buffer} buffer
    * @param {LobbySession} session
---------------------------------------------*/
  enterRoomHandler(buffer, session) {}

  /**---------------------------------------------
    [방 퇴장]

    * @param {Buffer} buffer
   * @param {LobbySession} session
---------------------------------------------*/
  leaveRoomHandler(buffer, session) {}

  /**---------------------------------------------
    [방 목록 조회] - 방 목록을 순회하면서 RoomInfo 메시지 생성

    * @param {Buffer} buffer
    * @param {LobbySession | BattleSession} session
---------------------------------------------*/
  getRoomsHandler(buffer, session) {}

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
      Session.getNextSequence(),
    );

    //배틀 서버에게 전송
    battleSession.send(sendBuffer);
  }

  /**---------------------------------------------
   * [게임 시작2] - 클라에게 배틀 서버의 주소와 포트번호, 게임 방ID 전송
   * @param {Buffer} buffer
   * @param {LobbySession | BattleSession} session
   * // 4. 로비 -> 클라: 게임 시작 응답(배틀 서버의 주소, port, gameRoomID)
   ---------------------------------------------*/
  onGameStartHandler(buffer, session) {
    //B2L_CreateRoomSchema(수신받은  packetId)
    //L2C_GameStartSchema(송신할 packetId)
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
