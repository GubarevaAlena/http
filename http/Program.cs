using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace SimpleHttpServerWithSocketsGetPost
{
    class Program
    {
        static void Main(string[] args)
        {
            StartServer();
        }

        static void StartServer()
        {
            // Устанавливаем IP-адрес и порт, на котором будет слушать сервер
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            int port = 8080;

            // Создаем сокет
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(ipAddress, port));
            listener.Listen(10);

            Console.WriteLine("Сервер запущен. Ожидание подключений...");

            while (true)
            {
                // Принимаем входящее соединение
                Socket clientSocket = listener.Accept();

                // Создаем поток для обработки запроса
                Thread clientThread = new Thread(() => HandleClient(clientSocket));
                clientThread.Start();
            }
        }

        static void HandleClient(Socket clientSocket)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = clientSocket.Receive(buffer);
            string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Console.WriteLine("Получен запрос: ");
            Console.WriteLine(request);


            string[] tokens = request.Split(' ');
            string type = tokens[0];
            string url = tokens[1];

            if (type == "GET")
            {
                if (url.Contains("image")) SendImage(clientSocket);
                else HandleGetRequest(clientSocket);
            }
            else if (type == "POST")
            {
                HandlePostRequest(clientSocket, request);
            }
            else
            {
                SendBadRequest(clientSocket);
            }

            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }

        static void HandleGetRequest(Socket clientSocket)
        {
            string html = File.ReadAllText("post.html");
            string response = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\n\r\n" + html;
            byte[] responseBuffer = Encoding.UTF8.GetBytes(response);
            clientSocket.Send(responseBuffer);
        }

        static void HandlePostRequest(Socket clientSocket, string request)
        {
            string postData = request.Substring(request.LastIndexOf("\r\n\r\n") + 4);
            Dictionary<string, string> formValues = new Dictionary<string, string>();

            string[] pairs = postData.Split('&');
            foreach (string pair in pairs)
            {
                string[] keyValue = pair.Split('=');
                formValues.Add(keyValue[0], keyValue[1]);
            }

            string response = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\n\r\n" + "<html><body><h1>Hello from HTTP Server! (POST request)</h1><h2>Received data: " + postData + "</h2></body></html>";
            byte[] responseBuffer = Encoding.UTF8.GetBytes(response);
            clientSocket.Send(responseBuffer);
        }

        static void SendBadRequest(Socket clientSocket)
        {
            string response = "HTTP/1.1 400 Bad Request\r\nContent-Type: text/html\r\n\r\n" + "<html><body><h1>400 Bad Request</h1></body></html>";
            byte[] responseBuffer = Encoding.UTF8.GetBytes(response);
            clientSocket.Send(responseBuffer);
        }

        static void SendImage(Socket clientSocket)
        {
            string imagePath = "C:\\Users\\gubar\\Pictures\\img.jpg";
            byte[] imageBytes = File.ReadAllBytes(imagePath);
            string imageHeader = "HTTP/1.1 200 OK\r\nContent-Type: image/jpeg\r\n\r\n";

            clientSocket.Send(Encoding.ASCII.GetBytes(imageHeader));
            clientSocket.Send(imageBytes);
        }
    }
}
