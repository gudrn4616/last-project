using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Ironcow
{
    public class TCPSocket : Socket
    {
        public TCPSocket(SocketInformation socketInformation) : base(socketInformation)
        {
        }

        public TCPSocket(SocketType socketType, ProtocolType protocolType) : base(socketType, protocolType)
        {
        }

        public TCPSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) : base(addressFamily, socketType, protocolType)
        {
        }
    }
}