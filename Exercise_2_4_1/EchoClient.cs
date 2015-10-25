using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Echo
{
    public class EchoClient
    {
        private readonly string _address;
        private readonly int _port;

        private int port;


        public EchoClient(string address, int port)
        {
            _address = address;
            _port = port;
        }

        public string Send(string message)
        {
            Console.WriteLine("FROM CLIENT: {0}", message);
            using (var client = new TcpClient(_address, _port))
            using(var stream = client.GetStream())
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                stream.Write(bytes, 0, bytes.Length);

                var response = new byte[1024];
                var readCount = stream.Read(response, 0, response.Length);
                return Encoding.UTF8.GetString(response, 0, readCount);
            }
        }
    }
}