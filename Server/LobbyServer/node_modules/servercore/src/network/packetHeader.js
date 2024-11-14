import { ePacketId } from "./packetId.js";

/**---------------------------------------------
 * @typedef {Object} PacketHeader
 * @property {number} size - 패킷의 크기
 * @property {ePacketId} id - 패킷의 ID
 * @property {number} sequence - 시퀀스 번호
 ---------------------------------------------*/

/**---------------------------------------------
 * @type {PacketHeader}
 ---------------------------------------------*/
export const packetHeader = {
    size,
    id,
    sequence
  };
