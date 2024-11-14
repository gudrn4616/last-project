

export class GameRoom {
  /**---------------------------------------------
   * @param {number} id - 방의 고유 ID
   * @param {number} maxPlayerCount - 최대 플레이어 수
   ---------------------------------------------*/
  constructor(id, maxPlayerCount) {
    this.users = [];
    this.id = id;
    this.maxPlayerCount = maxPlayerCount;
  }

  /**---------------------------------------------
   * [방 입장]
   * @param {GamePlayer} player - 입장할 플레이어 정보
   ---------------------------------------------*/
  enterRoom(player) {
    // 1. 방이 가득 찼는지 확인

    // 2. 유저 추가
    
    // 3. 해당 유저에게 B2C_EnterRoomMe 패킷 전송

    // 4. 모든 인원이 들어왔다면 B2C_GameStart 패킷 전송
  }

  /**---------------------------------------------
   * [이동 동기화]
   * @param {Buffer} buffer - 이동 패킷 데이터
   ---------------------------------------------*/
  handleMove(buffer) {

  }

  /**---------------------------------------------
   * [broadcast] - 모든 유저에게 패킷 전송
   * @param {Buffer} buffer - 전송할 데이터 버퍼
   ---------------------------------------------*/
  broadcast(buffer) {
    for (const user of this.users) {
      user.session.send(buffer);
    }
  }
}
