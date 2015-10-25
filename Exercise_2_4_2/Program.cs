using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebServerExample
{
    class Request
    {
        private Dictionary<string, string> _info = new Dictionary<string, string>();

        public string this[string key]
        {
            get
            {
                string val = null;
                _info.TryGetValue(key, out val);
                return val;
            }

            set { _info[key] = value; }
        }

        public Request(string httpRrequest)
        {
            var lines = httpRrequest.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.StartsWith("GET"))
                {
                    var req = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    this["HTTP-REQUEST"] = req[0];
                    this["URL"] = req[1];
                    this["HTTP"] = req[1].Split('/')[1];
                }
                else
                {
                    int splitPos = line.IndexOf(':');
                    this[line.Substring(0, splitPos)] = line.Substring(splitPos + 1).Trim();
                }
            }
        }
    }

    class WebServer
    {
        private TcpListener _server;
        private bool _stop = false;
        public WebServer()
        {
            var ipAddress = IPAddress.Parse("127.0.0.1");
            _server = new TcpListener(ipAddress, 51234);
        }

        public void Start()
        {
            _server.Start();

            var bytes = new byte[1024];
            string data = null;

            while (true)
            {
                Console.Write("Waiting for a connection... ");
                var client = _server.AcceptTcpClient();
                
                var stream = client.GetStream();

                int i = stream.Read(bytes, 0, bytes.Length);
                var request = new Request(Encoding.ASCII.GetString(bytes, 0, i));

                var httpRequest = request["HTTP-REQUEST"];
                var url = request["URL"];

                // ignore request for favicon
                if (url != null && url.Contains("/favicon.ico"))
                {
                    client.Close();
                    continue;
                }

                if (httpRequest != null && httpRequest == "GET")
                {
                    if (url == "/")
                    {
                        var msg = "Hello from the web server! @" + DateTime.Now.ToLongTimeString();
                        SendResponse(stream, 200, "Ok", msg);
                    }
                    else if (url == "/html")
                    {
                        var reader = new StreamReader("index.html");
                        var htmlFile = reader.ReadToEnd();
                        var contentLength = Encoding.UTF8.GetByteCount(htmlFile);
                        SendResponse(stream, 200, "Ok", htmlFile, contentLength);
                    }
                    else if (url.Contains("greeting"))
                    {
                        var match = Regex.Match(url, "/greeting[?]name=(.+)$");
                        if (match.Success)
                        {
                            var value = match.Groups[1].Value;
                            SendResponse(stream, 200, "Ok", "Hi, " + value);
                        }
                        else
                        {
                            Error404(stream);
                        }
                    }
                    else
                    {
                        Error404(stream);
                    }
                }


                client.Close();
            }

        }

        public void Stop() {  _server.Stop(); }

        private void Error404(NetworkStream stream)
        {
            SendResponse(
                stream,
                404,
                "Page not found",
                "Page not found! @" + DateTime.Now.ToLongTimeString());
        }


        private void SendResponse(NetworkStream stream, int statusCode, string statusMessage, string message, int contentLength = 0)
        {
            var response = Encoding.ASCII.GetBytes(
                "HTTP/1.1 " + statusCode + " " + statusMessage + "\r\n" +
                (contentLength == 0 ? "" : "Content-Length: " + contentLength + "\r\n") +
                "\r\n" +
                 message + "\r\n");
            stream.Write(response, 0, response.Length);
        }

    }


    class Program
    {
        static void Main(string[] args)
        {
            var server = new WebServer();
            server.Start();
        }
    }
}
