using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Echo
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 3333;
            // create the server
            var server = new EchoServer(port);
            // run the server in its own task (thread)
            Task.Run(() => server.Start());

            var client = new EchoClient("127.0.0.1", port);

            // just to be sure that the server is started
            // we give it 50 milliseconds
            // This means: pause for 50 milliseconds and then run the 
            // task. We save the task in a variable ....
            var task = Task.Delay(50).ContinueWith((x) =>
            {
                Console.WriteLine(client.Send("hello"));
            });
            // so we can wait for it to finish
            task.Wait();
            // before the main thread is finishing.

            server.Dispose();
        }
    }
}
