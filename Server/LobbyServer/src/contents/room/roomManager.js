const MAX_ROOMS_SIZE = 10000;

class RoomManager {
  constructor() {
    this.rooms = new Map();
    this.availableRoomIds = Array.from({ length: MAX_ROOMS_SIZE }, (_, i) => i + 1);

    let tmpRoomId = this.availableRoomIds.shift() || 0;
    this.rooms.set(tmpRoomId, new Room(tmpRoomId, '테스트 방', 2));
  }

  /**---------------------------------------------
    [방 입장]
    * @param {Buffer} buffer
    * @param {LobbySession} session
---------------------------------------------*/
  enterRoomHandler(buffer, session) {
    
  }

/**---------------------------------------------
    [방 퇴장]

    * @param {Buffer} buffer
   * @param {LobbySession} session
---------------------------------------------*/
  leaveRoomHandler(buffer, session) {
    
  }

  /**---------------------------------------------
    [방 목록 조회] - 방 목록을 순회하면서 RoomInfo 메시지 생성

    * @param {Buffer} buffer
    * @param {LobbySession | BattleSession} session
---------------------------------------------*/
  getRoomsHandler(buffer, session) {

  }

  /**---------------------------------------------
   * [게임 시작] - 배틀서버에게 게임 방 생성 요청

   * @param {Buffer} buffer
   * @param {LobbySession | BattleSession} session
   ---------------------------------------------*/
  gameStartHandler(buffer, session) {
   
  }

  /**---------------------------------------------
   * [게임 시작2] - 클라에게 배틀 서버의 주소와 포트번호, 게임 방ID 전송
   * @param {Buffer} buffer
   * @param {LobbySession | BattleSession} session
   ---------------------------------------------*/
  onGameStartHandler(buffer, session) {
  
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
