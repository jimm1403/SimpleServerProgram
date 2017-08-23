using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace ServerProgram
{
    class ServerProgram
    {
        static void Main(string[] args)
        {
            TcpListener serverSocket = new TcpListener(IPAddress.Any, 11000);
            TcpClient clientSocket;

            serverSocket.Start();
            Console.WriteLine("Server started");

            while (true)
            {
                clientSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine("Client: " + clientSocket.Client.RemoteEndPoint + " Connected!");
                HandleClient client = new HandleClient();       //Hjemmelavet klasse
                client.StartClient(clientSocket);
            }
        }
    }
    public class HandleClient
    {
        private TcpClient clientSocket; //Holder den client der er connected.
        private Thread thread;
        
        internal void StartClient(TcpClient client)
        {
            clientSocket = client;

            IPEndPoint remoteIp = (IPEndPoint)clientSocket.Client.RemoteEndPoint;
            NetworkStream stream = new NetworkStream(clientSocket.Client);      //Kablet/røret fra server til client.
            StreamReader sr = new StreamReader(stream);                         //Står for at læse hvad der kommer fra clienten.
            StreamWriter sw = new StreamWriter(stream);                         //Står for at skrive til clienten.
            
            thread = new Thread(() => ClientHandler(stream, sr, sw, remoteIp));
            thread.Start();
            
        }

        internal void ClientHandler(NetworkStream NWS, StreamReader sr, StreamWriter sw, IPEndPoint remoteIp)
        {
            bool run = true;
            
            while (run)
            {
                try
                {
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

                    else if (message[0]=="game")
                    {
                        Guessing(sw, sr);

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
                        sw.WriteLine("Goodbye");
                        run = false;
                    }
                    else
                    {
                        sw.WriteLine("Unknown command");
                    }
                    sw.Flush();

                    string messageReceived = remoteIp + ": Command >>";
                    for (int i = 0; i < message.Length; i++)
                    {
                        messageReceived += " " + message[i];
                        
                    }
                    if (message[0] == "exit")
                    {
                        Console.WriteLine(remoteIp + ": Command >> " + message[0]);
                        Console.WriteLine("Client disconnected");
                    }
                    else
                    {
                        Console.WriteLine(messageReceived);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ups...");
                    Console.WriteLine(e);
                    run = false;
                }
            }
            thread.Abort();
        }



        public void Guessing(StreamWriter sw, StreamReader sr)
        {
            Random rNm = new Random();
            int number = rNm.Next(1, 10);

            sw.WriteLine("Guess a number from 1 to 9");
            sw.Flush();

            while (true)
            {
                
                if (number == int.Parse(sr.ReadLine()))
                {
                    sw.WriteLine("Correct!");
                    sw.Flush();
                }
                else
                {
                    sw.WriteLine("Guess again loser!");
                    sw.Flush();
                }
            }
            
        }
    }
}
