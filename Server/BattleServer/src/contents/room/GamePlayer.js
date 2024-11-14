
import { ObjectInfo } from 'src/protocol/struct_pb';

export class GamePlayer {
  /**
   * @param {BattleSession} session - 플레이어의 세션 정보
   * @param {ObjectInfo} playerInfo - 플레이어의 정보 객체
   */
  constructor(session, playerInfo) {
    this.session = session;
    this.playerInfo = playerInfo;
  }
}
