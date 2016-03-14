using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebServiceExample
{
    class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    internal class Program
    {
        private static void Main(string[] args)
        {
            var persondata = new []
            {
                new { Id = 1, Name = "Peter"},
                new { Id = 2, Name = "Joe"},
                new { Id = 3, Name = "Sue"}
            };

            var service = new TcpListener(IPAddress.Any, 51234);
            service.Start();
            while (true)
            {
                TcpClient c = service.AcceptTcpClient();
                var stream = c.GetStream();
                var bytes = new byte[1024];
                int i = stream.Read(bytes, 0, bytes.Length);
                string data = Encoding.ASCII.GetString(bytes, 0, i);

                string msg = "Hello, use http://localhost:51234/api/persons";

                if (data.Contains("api/persons"))
                {
                    var match = Regex.Match(data, "api/persons/([0-9]+)");
                    if (match.Success)
                    {
                        var id = int.Parse(match.Groups[1].Value);
                        var p = persondata.FirstOrDefault(x => x.Id == id);
                        if(p != null)
                            msg = JsonConvert.SerializeObject(p);
                        else
                        {
                            msg = "Person with id = " + id + " not found";
                        }
                    }
                    else
                    {
                        string person = "";
                        msg = JsonConvert.SerializeObject(persondata);
                        JsonConvert.DeserializeObject<Person>(person);
                    }
                }

                var byteCount = Encoding.UTF8.GetByteCount(msg);

                var response = Encoding.ASCII.GetBytes(
                    "HTTP/1.1 200 OK\r\n" +
                    "Content-Length: " + byteCount + "\r\n" +
                    "\r\n" +
                    msg + "\r\n");
                stream.Write(response, 0, response.Length);
                c.Close();
            }
        }
    }
}
