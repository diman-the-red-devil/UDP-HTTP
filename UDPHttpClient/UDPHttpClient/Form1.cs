using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UDPHttpClient.Client;

namespace UDPHttpClient
{
    public partial class Form1 : Form
    {
        private HttpClient udpClient;

        public Form1()
        {        
            InitializeComponent();
            //webBrowser.Url = new Uri("http://google.com");
            // Запускаем в отдельном потоке взаимолествие с сервером
            Thread newThread = new Thread(new ThreadStart(runClient));
            newThread.Start();
        }

        // Запуск клиента
        public void runClient()
        {
            // Подключаемся к серверу
            IPAddress serverIPAddress = IPAddress.Parse("127.0.0.1");
            int serverPort = 5001;
            int localPort = 5002;
            udpClient = new HttpClient(serverIPAddress, serverPort, localPort);
            udpClient.Run(ref rtbLog, ref webBrowser); // передаем в параметры элементы формы
        }

        // Обработка нажатия кнопки
        private void goToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Считываем с текстового поля url-адрес
            string url = tstbUrl.Text; 

            // Формируем http-запрос
            string query = "GET " + url + " HTTP/1.1\r\n" +
                                "Host: web.net\r\n" +
                                "User-Agent: Mozilla/5.0 (Windows NT 6.1; rv:14.0) Gecko/20100101 Firefox/14.0.1\r\n" +
                                "Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8\r\n" +
                                "Accept-Language: tr-tr,tr;q=0.8,en-us;q=0.5,en;q=0.3\r\n" +
                                "Accept-Encoding: gzip, deflate\r\n" +
                                "Connection: keep-alive\r\n\r\n";         
            // Посылаем запрос
            udpClient.Send(query);
        }
    }
}
