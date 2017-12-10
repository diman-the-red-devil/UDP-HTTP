using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using UDPHttp.Server; 

namespace UDPHttpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Запускаем сервер
            IPAddress serverIPAddress = IPAddress.Parse("127.0.0.1");
            int serverPort = 5001;
            IPAddress clientIPAddress = IPAddress.Parse("127.0.0.1");
            int clientPort = 5002;
            HttpServer udpServer = new HttpServer(serverIPAddress, serverPort, clientIPAddress, clientPort);
            try
            {
                udpServer.Run();          
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.ReadKey();
        }
    }
}

