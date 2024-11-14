
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

  }

  /**---------------------------------------------
   * [방 생성] - 배틀서버에게 게임 방 생성 요청을 보내고, 클라이언트에게 방 ID 전송
   * @param {Buffer} buffer - 방 생성 요청 버퍼
   * @param {LobbySession | BattleSession} session - 요청한 세션
   ---------------------------------------------*/
  createGameRoomHandler(buffer, session) {

  }

  /**---------------------------------------------
   * [이동 동기화]
   * @param {Buffer} buffer - 이동 데이터 버퍼
   * @param {BattleSession} session - 이동 요청을 보낸 세션
   ---------------------------------------------*/
  moveHandler(buffer, session) {

  }
}

export const gameRoomManager = new GameRoomManager();
