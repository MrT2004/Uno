using System.Net;
using System.Net.Sockets;

namespace Uno_Client.Networking
{
    public class RequestHandler
    {
        private TcpClient client;
        private NetworkStream clientStream;
        private readonly IPEndPoint serverSocket;
        private Thread? readThread;
        private bool isReading;
        private static readonly object locker = new();

        public bool Connected { get; private set; }

        public event EventHandler<RequestEventArgs>? RequestEvent;

        public RequestHandler(IPAddress serverIP)
        {
            serverSocket = new IPEndPoint(serverIP, 8080);
            client = new TcpClient();
            clientStream = null!;
            Connected = false;
        }

        public void Start()
        {
            try
            {
                if (!client.ConnectAsync(serverSocket.Address, serverSocket.Port).Wait(1000))
                {
                    throw new Exception("Failed to connect to server");
                }
                else
                {
                    Connected = true;
                }
            }
            catch (Exception e)
            {
                Console.Clear();
                MainMenu.ShowTitle();
                Console.WriteLine(e.Message);
                Console.WriteLine("Press any key to return to menu");
                Console.ReadKey(true);
                Console.Clear();
                MainMenu.Menu();
            }
            if (Connected)
            {
                try
                {
                    this.clientStream = client.GetStream();
                    readThread = new Thread(Read);
                    isReading = true;
                    readThread.Start();
                }
                catch
                {
                    Console.WriteLine("Server RequestHandler could not be started.");
                }
            }
        }

        public void Read()
        {
            while (isReading)
            {
                try
                {
                    if (!clientStream.DataAvailable)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    List<byte> receivedBytes = new List<byte>();
                    byte[] buffer = new byte[1];

                    while (true)
                    {
                        clientStream.Read(buffer, 0, 1);
                        if (buffer[0] == (byte)'!')
                        {
                            break;
                        }
                        receivedBytes.Add(buffer[0]);
                    }

                    EventRequestReceived([.. receivedBytes]);
                }
                catch
                {
                    Console.WriteLine("Client error!");
                }
            }
        }

        public void Stop()
        {
            try
            {
                isReading = false;
                Connected = false;
                clientStream?.Close();
                client.Close();
                client = new TcpClient();
            }
            catch
            {
                Console.WriteLine("Server could not stop!");
            }
        }

        public void Send(Protocol.Protocol protocol)
        {
            lock (locker)
            {
                try
                {
                    byte[] sendBytes = protocol.ToBytes();
                    if (clientStream?.CanWrite == true)
                    {
                        clientStream.Write(sendBytes, 0, sendBytes.Length);
                    }
                }
                catch
                {
                    Console.WriteLine("Server could not send that response!");
                }
            }
        }

        protected virtual void EventRequestReceived(byte[] bytes)
        {
            RequestEvent?.Invoke(this, new RequestEventArgs(bytes, this));
        }
    }
}
