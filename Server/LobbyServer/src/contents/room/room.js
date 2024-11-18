/**
 * @enum {number}
 */
export const eRoomStateId = {
  WAITING: 0,
  IN_PROGRESS: 1,
};

export class Room {
  /**---------------------------------------------
   * [생성자]
   * @param {number} id - 방의 고유 ID
   * @param {string} roomName - 방 이름
   * @param {number} [maxPlayerCount=2] - 최대 플레이어 수
  ---------------------------------------------*/
  constructor(id, roomName, maxPlayerCount = 2) {
    this.id = id;
    this.roomName = roomName;
    this.users = [];
    this.state = eRoomStateId.WAITING;
    this.maxPlayerCount = maxPlayerCount;
  }

  /**---------------------------------------------
   *  [방 입장]
   * @param {LobbySession} newUser - 새로운 유저 세션
   * @returns {boolean} - 입장 성공 여부
  
       1. 방이 가득 찼는지 확인
       2. 기존 플레이어 목록을 유저에게 보내기
       3.  유저 추가
       4. 새 유저 입장 정보를 다른 유저들에게 알리기
  ---------------------------------------------*/
  enterRoom(newUser) {
    this.users.push(newUser);
    return true;
  }

  /**---------------------------------------------
      [방 퇴장]
  ---------------------------------------------*/
  leaveRoom(player) {
    if (this.users.includes(player)) {
      this.users = this.users.filter(user => user !== player);
      return true;
    }
    return false;
  }

  /**---------------------------------------------
    [broadcast]
---------------------------------------------*/
  broadcast(buffer) {
    for (const user of this.users) {
      user.send(buffer);
    }
  }

  /**---------------------------------------------
     * 현재 방 이름 반환
     * @returns {string}
   ---------------------------------------------*/
  getRoomName() {
    return this.roomName;
  }

  /**---------------------------------------------
     * 현재 유저 수 반환
     * @returns {number}
   ---------------------------------------------*/
  getCurrentUsersCount() {
    return this.users.length;
  }

  /**---------------------------------------------
     * 최대 유저 수 반환
     * @returns {number}
   ---------------------------------------------*/
  getMaxUsersCount() {
    return this.maxPlayerCount;
  }
}
