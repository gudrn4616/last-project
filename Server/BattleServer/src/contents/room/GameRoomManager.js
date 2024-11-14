
const MAX_ROOMS_SIZE = 10000;

class GameRoomManager {
  constructor() {
    this.rooms = new Map();
    this.availableRoomIds = Array.from({ length: MAX_ROOMS_SIZE }, (_, i) => i + 1);
  }

  /**---------------------------------------------
   * [방 입장] - 클라이언트에게 B2C_EnterRoom 패킷 전송
   * @param {number} roomId - 입장할 방 ID
   * @param {GamePlayer} player - 입장할 플레이어 정보
   ---------------------------------------------*/
  enterRoomHandler(roomId, player) {
    console.log('enterRoomHandler');

    const room = this.rooms.get(roomId);
    if (!room) {
      console.log('유효하지 않은 roomId입니다.');
      return;
    }

    room.enterRoom(player);
  }

  /**---------------------------------------------
   * [방 생성] - 배틀서버에게 게임 방 생성 요청을 보내고, 클라이언트에게 방 ID 전송
   * @param {Buffer} buffer - 방 생성 요청 버퍼
   * @param {LobbySession | BattleSession} session - 요청한 세션
   ---------------------------------------------*/
  createGameRoomHandler(buffer, session) {
    console.log('createGameRoom', session.getId());
    console.log('--------------------');
    const L2B_CreateRoomPacket = fromBinary(L2B_CreateRoomSchema, buffer);
    const roomId = L2B_CreateRoomPacket.roomId;
    const maxPlayerCount = L2B_CreateRoomPacket.maxPlayers;

    // roomId가 이미 존재하는지 검증
    if (this.rooms.has(roomId)) {
      console.error(`방 ID ${roomId}가 이미 존재합니다.`);
      console.log(this.rooms.get(roomId));
      throw new CustomError(ErrorCodes.SOCKET_ERROR, '방 ID가 이미 존재합니다.');
    }

    // 최대 인원 수가 유효한지 검증
    if (maxPlayerCount <= 0 || maxPlayerCount > MAX_ROOMS_SIZE) {
      console.error(`유효하지 않은 최대 인원 수: ${maxPlayerCount}`);
      throw new CustomError(ErrorCodes.SOCKET_ERROR, '유효하지 않은 최대 인원 수입니다.');
    }

    // 게임 방 생성
    const newGameRoom = new GameRoom(roomId, maxPlayerCount);
    this.rooms.set(roomId, newGameRoom);

    console.log('방 생성', L2B_CreateRoomPacket.roomId);

    const B2L_CreateRoomPacket = create(B2L_CreateRoomSchema, {
      isCreated: true,
      roomId: L2B_CreateRoomPacket.roomId,
    });

    const sendBuffer = PacketUtils.SerializePacket(
      B2L_CreateRoomPacket,
      B2L_CreateRoomSchema,
      ePacketId.B2L_CreateRoom,
      session.getNextSequence()
    );
    session.send(sendBuffer);
    console.log('send B2L_CreateRoom');
  }

  /**---------------------------------------------
   * [이동 동기화]
   * @param {Buffer} buffer - 이동 데이터 버퍼
   * @param {BattleSession} session - 이동 요청을 보낸 세션
   ---------------------------------------------*/
  moveHandler(buffer, session) {
    const packet = fromBinary(C2B_MoveSchema, buffer);

    const room = this.rooms.get(packet.roomId);
    if (!room) {
      console.log('유효하지 않은 roomId');
      throw new CustomError(ErrorCodes.SOCKET_ERROR, '유효하지 않은 roomId');
    }

    room.handleMove(buffer);
  }
}

export const gameRoomManager = new GameRoomManager();
