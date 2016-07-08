using SpeechTransport;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Speech.Recognition;

namespace SpeechServer
{
    public class SocketSpeechServer : AbstractSpeechServer
    {
        public int Port { get; private set; }
        public IPAddress Host { get; private set; }

        private Socket _Socket;
        private IPEndPoint _IPEndPoint;

        public SocketSpeechServer(IPAddress host, int port)
        {
            Port = port;
            Host = host;

            Connect();
        }

        private void Connect()
        {
            _Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _IPEndPoint = new IPEndPoint(Host, Port);
        }

        protected override void Send(ref SpeechResult result)
        {
            var data = Encoding.UTF8.GetBytes(result.ToString());
            _Socket.SendTo(data, _IPEndPoint);
        }

        public override void Dispose()
        {
            base.Dispose();
            _Socket.Dispose();
        }
    }
}
