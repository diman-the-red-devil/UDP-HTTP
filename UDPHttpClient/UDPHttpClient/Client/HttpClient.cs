using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UDPHttpClient.Client
{
    // Класс http-клиента
    class HttpClient
    {
        // Адрес сервера
        private IPAddress serverIPAddress = null;
        private int serverPort;
        // Адрес клиента   
        private IPAddress clientIPAddress = null;
        private int clientPort;

        private IPEndPoint serverPoint = null;  
        private UdpClient receiver = null;      // приемник сообщений
        private UdpClient sender = null;        // передатчик сообщений

        string response = null;                 // ответ сервера
        private RichTextBox rtb = null;
        private WebBrowser wb = null;

        // Делегаты позволяют обратится к элементам формы из неосновного потока
        private delegate void SetRTBCallback(string text);  // делегат обращения к элементам формы 
        private delegate void SetWBCallback(string text);   // делегат обращения к элементам формы 

        private string datagram;                // сообщение клиента

        // Конструктор класса
        public HttpClient(IPAddress serverIPAddress, int serverPort, int clientPort)
        {
            this.serverIPAddress = serverIPAddress;
            this.serverPort = serverPort;
            this.clientPort = clientPort;           
        }

        // Метод запуска взаимодествия клиент-сервер
        public void Run(ref RichTextBox rtb, ref WebBrowser wb)
        {
            this.rtb = rtb;
            this.wb = wb;
            try
            {
                // Создаем объект приемник
                receiver = new UdpClient(clientPort);  
                // В цикле принимаем ответы от сервера
                while (true)
                {
                    Recv();                                                     
                }
            }
            catch (Exception ex)
            {
                PrintLog("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
        }

        // Метод приема ответов от сервера
        public void Recv()
        {
            byte[] receiveBytes = receiver.Receive(ref serverPoint);    // буфер для хранения данных с сервера               
            string returnData = Encoding.UTF8.GetString(receiveBytes);  // данные преобразованные в строковый тип
            response = returnData.ToString();   // ответ сервера
            // Пишем в лог ответ сервера
            PrintLog(response);
            // Проверяем данные на целостность            
            if(response.Contains("Hash"))
            {
                // Ищем в ответе подстроку "Hash"
                int index1 = response.IndexOf("Hash");
                string s1 = response.Substring(index1 + 4);
                s1 = s1.Remove(0, 2);
                int index2 = s1.IndexOf("\n");
                s1 = s1.Remove(index2);
                // Читаем хэш из данных
                int hash1 = Int32.Parse(s1);
                // Пишем в лог значение хэша рассчитанного на сервере 
                PrintLog("Hash1: " + hash1.ToString() + "\n");
                string s2 = response.Remove(index1);
                // Рассчитываем хэш из данных
                int hash2 = s2.GetHashCode();
                // Пишем в лог значение хэша рассчитанного на клиенте 
                PrintLog("Hash2: " + hash2.ToString() + "\n");
                // Если целостность нарушена то повторяем запрос на сервер
                if(hash1 != hash2)
                {
                    Send(datagram);
                }
            }
            // Если пришел html-код
            if(response.Contains("<html>"))
            {
                int index1 = response.IndexOf("Hash");
                string web = response.Remove(index1);
                // Пишем в лог html-код
                PrintLog("\n********Web Part********\n" + web + 
                         "\n************************\n");
                // Отправляем браузеру
                SetToBrowser(web);              
            }        
        }

        // Метод отправки запроса на сервер
        public void Send(string datagram)
        {
            this.datagram = datagram;
            // Пишем в лог запрос клиента
            PrintLog(datagram);
            // Создаем обьект передатчик
            sender = new UdpClient();
            serverPoint = new IPEndPoint(serverIPAddress, serverPort);           
            try
            {
                // Преобразуем строку запроса в байты
                byte[] bytes = Encoding.UTF8.GetBytes(datagram);
                // Отправляем байты серверу
                sender.Send(bytes, bytes.Length, serverPoint);
            }
            catch (Exception ex)
            {
                PrintLog("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
            finally
            {
                sender.Close();
            }
        }

        // Метод записи в лог
        public void PrintLog(string text)
        {          
            // Если элемент управления вызывается из другого потока
            if (rtb.InvokeRequired)
            {
                // Создаем делегат
                SetRTBCallback rtbCallback = new SetRTBCallback(PrintLog);
                // Выполняем делегат в основном потоке
                rtb.Parent.Invoke(rtbCallback, new object[] { text });
            }
            else
            {
                rtb.AppendText(text + "\n");
            }
        }

        // Метод отправки в браузер полученных html данных
        public void SetToBrowser(string text)
        {
            if (wb.InvokeRequired)
            {
                SetWBCallback wbCallback = new SetWBCallback(SetToBrowser);
                wb.Parent.Invoke(wbCallback, new object[] { text });
            }
            else
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                wb.DocumentText = text;
            }
        }
    }
}
