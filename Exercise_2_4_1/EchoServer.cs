using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Echo
{
    public class EchoServer : IDisposable
    {
        private TcpListener _server;
        public int Port { get; set; }

        public EchoServer(int port)
        {
            Port = port;
        }

        public void Start()
        {
            _server = new TcpListener(IPAddress.Any, Port);
            _server.Start();

            using (var client = _server.AcceptTcpClient())
            using (var stream = client.GetStream())
            {
                var message = new byte[1024];
                int bytesRead = stream.Read(message, 0, message.Length);

                var msg = Encoding.UTF8.GetString(message, 0, bytesRead);

                var bytes = Encoding.UTF8.GetBytes("FROM SERVER: " + msg.ToUpper());

                stream.Write(bytes, 0, bytes.Length);
            }
        }

        public void Dispose()
        {
            _server.Stop();
        }
    }
}