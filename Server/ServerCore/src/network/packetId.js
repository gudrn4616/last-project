/*---------------------------------------------
    [접두사]
    - S2C: 서버->클라

    - C2L / L2C
        클라<->로비 서버
    - C2B / B2C
        클라->배틀 서버
    - L2B / B2L
        로비 서버 <-> 배틀 서버
---------------------------------------------*/
//export enum ePacketId

export const ePacketId = {
    S2C_Error: 0,
    C2L_Init: 1,
    C2L_EnterRoom: 2,
    C2L_LeaveRoom: 3,
    C2L_GetRooms: 4,
    C2L_GameStart: 5,
    C2B_Init: 21,
    C2B_Move: 22,
    L2C_Init: 51,
    L2C_EnterRoomMe: 52,
    L2C_EnterRoomOther: 53,
    L2C_LeaveRoomMe: 54,
    L2C_LeaveRoomOther: 55,
    L2C_GetRoom: 56,
    L2C_GameStart: 57,
    L2B_Init: 61,
    L2B_CreateRoom: 62,
    B2C_Enter: 101,
    B2C_GameStart: 102,
    B2C_Move: 103,
    B2L_Init: 121,
    B2L_CreateRoom: 122,
  };
  