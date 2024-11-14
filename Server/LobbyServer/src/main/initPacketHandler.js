import { Socket } from 'net';

export const onConnection = (socket) => {
  console.log('새로운 연결이 감지되었습니다:', socket.remoteAddress, socket.remotePort);

  let buffer = Buffer.alloc(0);

  socket.on('data', (data) => {
    buffer = Buffer.concat([buffer, data]);

    if (buffer.length < config.packet.sizeOfHeader) {
      return;
    }

    let header = PacketUtils.readPacketHeader(buffer);
    if (buffer.length < header.size) {
      console.log('파싱X', buffer.length, header.size);
      return;
    }

    const packet = buffer.subarray(config.packet.sizeOfHeader, header.size);

    if (header.id === ePacketId.C2L_Init) {
      console.log('클라 접속');
      initialHandler(packet, socket, ePacketId.C2L_Init);
    } else if (header.id === ePacketId.B2L_Init) {
      console.log('배틀 서버 접속');
      initialHandler(packet, socket, ePacketId.B2L_Init);
    } else {
      console.log('먼지 모르겥는거 두두등장');
      socket.destroy();
    }
  });
};

/*---------------------------------------------
    [초기화 핸들러]
---------------------------------------------*/
const initialHandler = async (buffer, socket, packetId) => {
  console.log('initialHandler: called');
  socket.removeAllListeners('data'); 

  if (packetId === ePacketId.C2L_Init) {
    let packet;
    try {
      packet = fromBinary(C2L_InitialPacketSchema, buffer);
    } catch (error) {
      throw new CustomError(ErrorCodes.PACKET_DECODE_ERROR, '패킷 디코딩 중 오류가 발생했습니다');
    }

    if (packet.meta?.clientVersion !== lobbyConfig.client.version) {
      throw new CustomError(
        ErrorCodes.CLIENT_VERSION_MISMATCH,
        '클라이언트 버전이 일치하지 않습니다.'
      );
    }

    let user = await UserDb.findUserByDeviceID(packet.meta.userId);
    if (!user) {
      user = await UserDb.createUser(packet.meta.userId);
    } else {
      UserDb.updateUserLogin(user.id);
    }

    sessionManager.addSession(packet.meta.userId, socket);
    sessionManager.getSessionOrNull(packet.meta.userId)?.setNickname(packet.nickname);

    const initPacket = create(L2C_InitSchema, {
      meta: ResponseUtils.createMetaResponse(RESPONSE_SUCCESS_CODE),
      userId: packet.meta.userId,
    });

    const sendBuffer = PacketUtils.SerializePacket(
      initPacket,
      L2C_InitSchema,
      ePacketId.L2C_Init,
      sessionManager.getSessionOrNull(packet.meta.userId)?.getNextSequence() || 0
    );
    console.log('Serialized sendBuffer length:', sendBuffer.length);
    sessionManager.getSessionOrNull(packet.meta.userId)?.send(sendBuffer);
  } else if (packetId === ePacketId.B2L_Init) {
    let packet;
    try {
      packet = fromBinary(B2L_InitialPacketSchema, buffer);
    } catch (error) {
      throw new CustomError(ErrorCodes.PACKET_DECODE_ERROR, '패킷 디코딩 중 오류가 발생했습니다');
    }
    battleSessionManager.addSession(packet.serverId, socket);
  }
};

export default initialHandler;
