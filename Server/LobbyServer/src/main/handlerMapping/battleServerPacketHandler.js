import { ePacketId } from 'ServerCore/src/network/packetId.js';
import { roomManager } from 'src/contents/room/roomManager.js';

/**
 * 패킷 ID에 따른 배틀 핸들러 매핑
 * @type {Object.<ePacketId, Function>}
 */
const battleHandlerMappings = {
  [ePacketId.B2L_Init]: (buffer, session) => defaultHandler(buffer, session),
  [ePacketId.B2L_CreateRoom]: (buffer, session) => roomManager.onGameStartHandler(buffer, session),
};

export default battleHandlerMappings;
