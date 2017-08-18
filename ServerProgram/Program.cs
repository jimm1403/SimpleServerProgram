using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace ServerProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpListener serverSocket = new TcpListener(IPAddress.Any, 11000);
            TcpClient clientSocket;
            int clientIP = 0;

            serverSocket.Start();
            Console.WriteLine("Server started");

            while (true)
            {
                clientSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine("Client: " + clientSocket.Client.RemoteEndPoint + " Connected!");
                HandleClient client = new HandleClient();       //Hjemmelavet klasse
                client.StartClient(clientSocket, clientIP);
            }
        }
    }
    public class HandleClient
    {
        private TcpClient clientSocket; //Holder den client der er connected.
        private string clientNo;

        internal void StartClient(TcpClient client, int clientNo)
        {
            clientSocket = client;
            this.clientNo = clientNo.ToString();
            Thread newThread = new Thread(ClientHandler);
            newThread.Start();
        }

        internal void ClientHandler()
        {
            NetworkStream stream = new NetworkStream(clientSocket.Client);      //Kablet/røret fra server til client.
            StreamReader sr = new StreamReader(stream);                         //Står for at læse hvad der kommer fra clienten.
            StreamWriter sw = new StreamWriter(stream);                         //Står for at skrive til clienten.
            while (true)
            {
                try
                {
                    IPEndPoint remoteIp = (IPEndPoint)clientSocket.Client.RemoteEndPoint;
                    //IPEndPoint localIp = (IPEndPoint)clientSocket.Client.LocalEndPoint;

                    if (remoteIp != null)
                    {
                        Console.WriteLine("IP and Port connected: " + remoteIp.Address + ":" + remoteIp.Port);
                    }

                    //if (localIp != null)
                    //{
                    //    Console.WriteLine("Local IP and Port is: " + localIp.Address + ":" + localIp.Port);
                    //}

                    sw.WriteLine("Ready");
                    sw.Flush();

                    string[] message = sr.ReadLine().Split(' ');        //splitter modtaget string op i dele.
                    message[0] = message[0].ToLower();


                    if (message[0] == "time")
                    {
                        sw.WriteLine("The time right now is " + DateTime.Now.Hour + ":" +
                            DateTime.Now.Minute + ":" + DateTime.Now.Second);
                    }
                    else if (message[0] == "date")
                    {
                        sw.WriteLine("The date today is " + DateTime.Today.Day + "-" +
                            DateTime.Today.Month + "-" + DateTime.Today.Year);
                    }
                    else if (message[0] == "add")
                    {
                            sw.WriteLine("sum " + (int.Parse(message[1]) + int.Parse(message[2])));
                    }
                    else if (message[0] == "sub")
                    {
                        sw.WriteLine("difference " + (int.Parse(message[1]) - int.Parse(message[2])));
                    }
                    else if (message[0] == "exit")
                    {
                        Console.WriteLine("Client disconnected");
                        sw.WriteLine("Goodbye");
                    }
                    else
                    {
                        sw.WriteLine("Unknown command");
                    }
                    sw.Flush();

                    //Console.WriteLine(client.RemoteEndPoint);
                    for (int i = 0; i < message.Length; i++)
                    {
                        Console.WriteLine(message[i]);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ups...");
                    sw.WriteLine("Der er sket en fejl på serveren :(");
                    sw.Flush();
                }

            }
        }
    }
}
