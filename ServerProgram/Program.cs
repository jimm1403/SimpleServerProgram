using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;

namespace ServerProgram
{
    class ServerProgram
    {
        static void Main(string[] args)
        {
            TcpListener serverSocket = new TcpListener(IPAddress.Parse("127.0.0.1"), 11000);
            
            TcpClient clientSocket;
            ServerProgram serverP = new ServerProgram();

            serverSocket.Start();
            Console.WriteLine("Server started");
            Game serverGame = new Game();
            
            while (true)
            {
                clientSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine("Client: " + clientSocket.Client.RemoteEndPoint + " Connected!");
                HandleClient client = new HandleClient();       //Hjemmelavet klasse
                client.StartClient(clientSocket);
                client.game = serverGame;
            }
        }
    }
    public class Game
    {
        UdpClient publisher = new UdpClient(11001);
        //NetworkStream stream;
        //StreamWriter pubSW;

        int tries;
        bool gameRun;
        Random rNm = new Random();
        int randomNr;
        public bool GameRun { get { return gameRun; } set { gameRun = value; } }

        public Game()
        {
            //pubSW = new StreamWriter(stream);
            tries = 0;
            gameRun = true;
            randomNr = rNm.Next(1, 2);
        }
        public string Guess(int guess)
        {            
            tries++;
            if (guess == randomNr)
            {
                Console.WriteLine("Caster til alle clients");
                byte[] sdata = Encoding.ASCII.GetBytes("Spillet er vundet!");
                publisher.Send(sdata, sdata.Length);
                Reset();
                return "Sådan verdensmand!";
            }
            else if(tries <= 8)
            {
                return "Gæt igen, " + tries + " forsøg brugt!";   
            }
            else
            {
                Reset();
                
                return "Du har tabt, ikke flere gæt!";
            }
        }
        private void Reset()
        {
            tries = 0;
            gameRun = false;
            randomNr = rNm.Next(1, 21);
        }

    }
    public class HandleClient
    {
        private TcpClient clientSocket; //Holder den client der er connected.
        private Thread thread;
        public Game game { get; set; }

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
                        Game(sw, sr);
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
        
        public void Game(StreamWriter sw, StreamReader sr)
        {
            sw.WriteLine("Guess a number from 1 to 20");
            sw.Flush();
            game.GameRun = true;

            while (game.GameRun)
            {
                sw.WriteLine(game.Guess(int.Parse(sr.ReadLine())));
                sw.Flush();    
            }
        }    
    }
}
