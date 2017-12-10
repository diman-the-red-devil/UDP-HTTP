using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UDPHttp.Server
{
    class HttpServer
    {
        // Адрес сервера
        private IPAddress serverIPAddress = null;
        private int serverPort;
        // Адрес клиента       
        private IPAddress clientIPAddress = null;
        private int clientPort;
        
        IPEndPoint clientEndPoint = null;
        private UdpClient receiver = null;  // приемник сообщений
        private UdpClient sender = null;    // передатчик сообщений
        
        const string serverDirectory = "C:\\Server\\";      // корневая папка сервера
        private Dictionary<string, string> contentTypes;    // поддерживаемый контент 
        // Запрос клиента
        private string request = null;  // запрос 
        private string[] startLine;     // стартовая строка запроса
        private string httpMethod;      //   метод              
        private string httpUrl;         //   ресурс
        private string httpVersion;     //   версия протокола    
        private string[] headers;       // заголовки запроса
        private string[] body;          // тело запроса
        private string path;            // полный путь
        private string contentType;         // тип контента
        private FileStream file;
        // Переменные необходимые для парсинга запроса
        private int index1;         
        private int index2;
        private string sub1, sub2, sub3;
        // Статусы проверки запроса
        private string statusMethod;
        private string statusUrl;
        private string statusVersion;
        private string statusPath;
        private string statusFile;
        private bool flag = false;

        // Конструкторы класса
        public HttpServer(IPAddress serverIPAddress, int serverPort)
        {
            this.serverIPAddress = serverIPAddress;
            this.serverPort = serverPort;         
            CreateContentType();
        }

        public HttpServer(IPAddress serverIPAddress, int serverPort, int clientPort)
        {
            this.serverIPAddress = serverIPAddress;
            this.serverPort = serverPort;
            this.clientPort = clientPort;
            CreateContentType();
        }

        public HttpServer(IPAddress serverIPAddress, int serverPort, IPAddress clientIPAddress, int clientPort)
        {
            this.serverIPAddress = serverIPAddress;
            this.serverPort = serverPort;
            this.clientIPAddress = clientIPAddress;
            this.clientPort = clientPort;
            CreateContentType();
        }

        // Поддерживаемые типы контента
        private void CreateContentType()
        {
            contentTypes = new Dictionary<string, string>()
            { 
                { ".htm", "text/html" },
                { ".html", "text/html" },
                { ".xml", "text/xml" },
                { ".txt", "text/plain" },
                { ".css", "text/css" },
                { ".png", "image/png" },
                { ".gif", "image/gif" },
                { ".jpg", "image/jpg" },
                { ".jpeg", "image/jpeg" },
                { ".zip", "application/zip"}
            };
        }

        // Метод запуска взаимодествия клиент-сервер
        public void Run()
        {                  
            try
            {
                // Создаем объект приемник
                receiver = new UdpClient(serverPort);
                // В цикле принимаем запросы от клиента
                while (true)
                {
                    Recv();
                    ParseRequest();
                    flag = true;
                    Send();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
        }

        // Метод приема запросов от клиента
        public void Recv()
        {
            byte[] receiveBytes = receiver.Receive(ref clientEndPoint); // буфер для хранения данных с клиента  
            //clientIPAddress = clientEndPoint.Address;
            //clientPort = clientEndPoint.Port;
            //Console.WriteLine(clientIPAddress + "\n");
            //Console.WriteLine(clientPort + "\n");
            string returnData = Encoding.UTF8.GetString(receiveBytes); // данные преобразованные в строковый тип
            request = returnData.ToString(); // запрос клиента
        }

        // Метод разбора строки запроса
        private void ParseRequest()
        {
            index1 = request.IndexOf("\r\n");
            index2 = request.LastIndexOf("\r\n");
            GetStartLine();     // разбираем стартовую строку
            GetMethod();        // проверяем http-метода
            GetUrl();           // проверяем url
            GetVersion();       // проверяем версию http
            GetHeaders();       // разбираем заголовки
            GetBody();          // проверяем тело запроса
            GetPath();          // проверяем полный путь к запрашиваемому ресурсу
            GetContentType();   // проверяем тип контента
            GetFile();          // проверяем запрашиваемый ресурс
        }

        // Разбор стартовой строки
        private void GetStartLine()
        {
            sub1 = request.Substring(0, index1);
            startLine = sub1.Split(' ');
        }

        // Проверка http-метода
        private void GetMethod()
        {  
            httpMethod = startLine[0];                    
            if (httpMethod.Equals("GET") || httpMethod.Equals("POST"))
            {
                statusMethod = "OK";
            }
            else 
            {
                statusMethod = "NOTIMPL";
            }
            Console.WriteLine("HttpMethod: " + httpMethod);             
        }

        // Проверка url 
        private void GetUrl()
        {
            httpUrl = startLine[1];
            if(httpUrl.Equals("") || httpUrl.Equals("/"))
            {
                httpUrl = "index.html";
                statusUrl = "OK";
            }
            else if (httpUrl.Contains("..") == false)
            {
                statusUrl = "OK";              
            }
            else 
            { 
                statusUrl = "BADREQUEST";
            }
            Console.WriteLine("HttpUrl: " + httpUrl + httpUrl.Contains(".."));
        }

        // Проверка версии протокола
        private void GetVersion()
        {
            httpVersion = startLine[2];
            if (httpVersion.IndexOf("HTTP") >= 0)
            {
                statusVersion = "OK";
            }
            else
            {
                statusVersion = "BADREQUEST";
            }
            Console.WriteLine("HttpVersion: " + httpVersion);
        }

        // Разбор заголовков запроса
        private void GetHeaders()
        {
            sub2 = request.Substring(index1, index2 - index1);
            headers = sub2.Split('\n');
            Console.WriteLine("\nHeaders:");
            foreach (string s in headers)
            {
                Console.WriteLine(s);
            }
        }

        // Проверка тела запроса
        private void GetBody()
        {
            if(httpMethod.Equals("POST"))
            { 
                sub3 = request.Substring(index2, request.Length - index2);
                body = sub3.Split('&');
                Console.WriteLine("\nBody:");
                foreach (string s in body)
                {
                    Console.WriteLine(s + "\n");
                }
            }
            else
            {
                body = null;
            }
        }

        // Проверка полныого пути к запрашиваемому ресурсу
        private void GetPath()
        {
            path = serverDirectory + httpUrl;
            if (File.Exists(path))
            {
                statusPath = "OK";
            }
            else
            {
                statusPath = "NOTFOUND";
            }
            Console.WriteLine("\nPath: " + path);
        }

        // Проверка типа контента
        private void GetContentType()
        {
            string extension = null;
            int dot = path.LastIndexOf(".");           
            if(dot >= 0)
            {
                extension = path.Substring(dot, path.Length - dot).ToLower();
            }
            if(extension != null)
            {
                if(contentTypes.ContainsKey(extension) == false)
                {
                    contentType = "application/" + extension;
                }
                else
                {
                    contentType = contentTypes[extension];
                }
            }
            Console.WriteLine("\nContent Type: " + contentType);
        }

        // Проверка запрашиваемого ресурса
        private void GetFile()
        {
            try
            {
                file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                statusFile = "OK";
            }
            catch (Exception)
            {
                statusFile = "INTERNALSERVERERROR";
            }
        }

        // Метод отправки ответа клиенту
        private void Send()
        {
            if(flag == true)
            {
                if(statusMethod.Equals("NOTIMPL"))
                {
                    // Не поддерживается http-метод
                    SendNotImplemented();
                }
                else if (statusUrl.Equals("BADREQUEST")||statusVersion.Equals("BADREQUEST"))
                {
                    // Некорректный запрос
                    SendBadRequest();
                }
                else if (statusPath.Equals("NOTFOUND"))
                {
                    // Не найден ресурс
                    SendNotFound();
                }
                else if(statusFile.Equals("INTERNALSERVERERROR"))
                {
                    // Внутренняя ошибка сервера
                    SendInternalServerError();
                }
                else
                {
                    // Успешный запрос
                    SendOk();
                }
                flag = false;
            }
        }

        // Метод отправки массива байтов
        private void SendResponse(byte[] content, string responseCode, string contentType)
        {
            // Создаем обьект передатчик
            sender = new UdpClient();   
            clientEndPoint = new IPEndPoint(clientIPAddress, clientPort);
            try
            {
                // Формируем заголовки ответа
                string headers = "HTTP/1.1 " + responseCode + "\r\n" +
                                 "Content-Type: " + contentType + "\r\n" +
                                 "Content-Length: " + content.Length.ToString() + "\r\n";
                // Рассчитываем хэш для заголовков ответа
                int hash1 = headers.GetHashCode();
                headers += "Hash: " + hash1 + "\r\n\r\n";
                //Console.WriteLine(headers);
                // Преобразуем заголовки ответа в байты
                byte[] bHeader = Encoding.UTF8.GetBytes(headers);
                //Console.WriteLine("Hash: " + hash1);
                // Отправляем байты клиенту
                sender.Send(bHeader, bHeader.Length, clientEndPoint);
                //Console.WriteLine(Encoding.UTF8.GetString(bHeader));
                // Формируем данные для клиента
                string data = Encoding.UTF8.GetString(content);
                // Рассчитываем хэш для данных в ответе
                int index = data.LastIndexOf('>');
                data = data.Remove(index + 1);
                int hash2 = data.GetHashCode();
                data += "Hash: " + hash2 + "\r\n\r\n";
                //Console.WriteLine(data);
                // Преобразуем данные в ответе в байты
                byte[] bContent = Encoding.UTF8.GetBytes(data);
                //Console.WriteLine("Hash: " + hash2);
                // Отправляем байты серверу
                sender.Send(bContent, bContent.Length, clientEndPoint);
                //Console.WriteLine(Encoding.UTF8.GetString(content));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
            finally
            {
                sender.Close();
            }
        }

        // Посылаем строку
        private void SendStrings(string dataString, string responseCode, string contentType)
        {
            // Преобразуем данные в ответе в байты
            byte[] bytes = Encoding.UTF8.GetBytes(dataString);
            // Отправляем байты серверу
            SendResponse(bytes, responseCode, contentType);
        }

        // Метод для отправки ответа об ошибках
        private void SendError(string errorString, string responseCode, string contentType)
        {
            // Шаблон для ответа об ошибке
            string response =
                "<html>" +
                "<head>" +
                "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">" +
                "</head>" +
                "<body>" +
                "<h2>Simple Web Server</h2>" +
                "<div><b>" + errorString + "</b></div>" +
                "</body>" +
                "</html>";
            SendStrings(response, responseCode, contentType);
        }

        // 501 - Not Implemented
        private void SendNotImplemented()
        {
            SendError("501 - Not Implemented", "501 Not Implemented", "text/html");
        }

        // 400 - Bad Request
        private void SendBadRequest()
        {
            SendError("400 - Bad Request", "400 Bad Request", "text/html");
        }

        // 404 - Not Found
        private void SendNotFound()
        {
            SendError("404 - Not Found", "404 Not Found", "text/html");
        }

        // 500 - Internal Server Error
        private void SendInternalServerError()
        {
            SendError("500 - Internal Server Error", "500 Internal Server Error", "text/html");
        }

        // 200 - OK
        private void SendOk()
        {
            byte[] buffer = new byte[1024];
            file.Read(buffer, 0, buffer.Length);
            SendResponse(buffer, "200 OK", "text/html");
        }
    }
}
