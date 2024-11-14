using Google.Protobuf;
using Google.Protobuf.Reflection;
using System;
using System.IO;
using static GamePacket;

namespace Ironcow.WebSocketPacket
{
    // 최상위 GamePacket 메시지
    // 패킷 타입 상수 정의 (서버와 동일해야 함)
    public enum ePacketType
    {
        NONE = 0,
        // 회원가입 및 로그인
        REGISTER_REQUEST = 1,
        REGISTER_RESPONSE = 2,
        LOGIN_REQUEST = 3,
        LOGIN_RESPONSE = 4,

        // 매칭
        MATCH_REQUEST = 5,
        MATCH_START_NOTIFICATION = 6,

        // 상태 동기화
        STATE_SYNC_NOTIFICATION = 7,

        // 타워 구입 및 배치
        TOWER_PURCHASE_REQUEST = 8,
        TOWER_PURCHASE_RESPONSE = 9,
        ADD_ENEMY_TOWER_NOTIFICATION = 10,

        // 몬스터 생성
        SPAWN_MONSTER_REQUEST = 11,
        SPAWN_MONSTER_RESPONSE = 12,
        SPAWN_ENEMY_MONSTER_NOTIFICATION = 13,

        // 전투 액션
        TOWER_ATTACK_REQUEST = 14,
        ENEMY_TOWER_ATTACK_NOTIFICATION = 15,
        MONSTER_ATTACK_BASE_REQUEST = 16,

        // 기지 HP 업데이트 및 게임 오버
        UPDATE_BASE_HP_NOTIFICATION = 17,
        GAME_OVER_NOTIFICATION = 18,

        // 게임 종료
        GAME_END_REQUEST = 19,

        // 몬스터 사망 통지
        MONSTER_DEATH_NOTIFICATION = 20,
        ENEMY_MONSTER_DEATH_NOTIFICATION = 21,
    }

    public class Packet
    {
        public PayloadOneofCase type;
        public string version;
        public int sequence;
        public byte[] payloadBytes;
        public GamePacket gamePacket
        {
            get
            {
                GamePacket gamePacket = new GamePacket();
                gamePacket.MergeFrom(payloadBytes);
                return gamePacket;
            }
        }

        public MessageDescriptor Descriptor => throw new NotImplementedException();

        public Packet(byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            var reader = new BinaryReader(stream);
            var data = reader.ReadBytes(2);
            Array.Reverse(data);
            type = (PayloadOneofCase)BitConverter.ToInt16(data);
            data = reader.ReadBytes(1);
            var length = data[0] & 0xff;
            data = reader.ReadBytes(length);
            version = BitConverter.ToString(data);
            data = reader.ReadBytes(4);
            Array.Reverse(data);
            sequence = BitConverter.ToInt32(data);
            data = reader.ReadBytes(4);
            Array.Reverse(data);
            var payloadLength = BitConverter.ToInt32(data);
            payloadBytes = reader.ReadBytes(payloadLength);
        }

        public Packet(PayloadOneofCase type, string version, int sequence, byte[] payload)
        {
            this.type = type;
            this.version = version;
            this.sequence = sequence;

            payloadBytes = payload;
        }

        public ArraySegment<byte> ToByteArray()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            byte[] bytes = new byte[1024];
            var fields = GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(int))
                {
                    bytes = BitConverter.GetBytes((int)field.GetValue(this));
                    Array.Reverse(bytes);
                    writer.Write(bytes);
                }
                else if (field.FieldType == typeof(string))
                {
                    var str = (string)field.GetValue(this);
                    //writer.Write((char)UTF8Encoding.UTF8.GetBytes(str).Length);
                    writer.Write(str);
                }
                else if (field.FieldType == typeof(bool))
                {
                    writer.Write((bool)field.GetValue(this));
                }
                else if (field.FieldType == typeof(short))
                {
                    bytes = BitConverter.GetBytes((short)field.GetValue(this));
                    Array.Reverse(bytes);
                    writer.Write(bytes);
                }
                else if (field.FieldType == typeof(float))
                {
                    bytes = BitConverter.GetBytes((float)field.GetValue(this));
                    Array.Reverse(bytes);
                    writer.Write(bytes);
                }
                else if (field.FieldType == typeof(double))
                {
                    bytes = BitConverter.GetBytes((double)field.GetValue(this));
                    Array.Reverse(bytes);
                    writer.Write(bytes);
                }
                else if (field.FieldType.IsEnum)
                {
                    bytes = BitConverter.GetBytes((short)(int)field.GetValue(this));
                    Array.Reverse(bytes);
                    writer.Write(bytes);
                }
                else
                {
                    using (MemoryStream memory = new MemoryStream())
                    {
                        var array = (byte[])field.GetValue(this);
                        bytes = BitConverter.GetBytes(array.Length);
                        Array.Reverse(bytes);
                        writer.Write(bytes);
                        writer.Write(new ArraySegment<byte>(array));
                        memory.Dispose();
                    }
                }
            }
            writer.Flush();
            stream.Dispose();
            return stream.ToArray();
        }
        
    }
}