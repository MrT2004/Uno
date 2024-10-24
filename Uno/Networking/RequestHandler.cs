using System.Net.Sockets;

namespace Uno_Server.Networking
{
    public class RequestHandler
    {
        private NetworkStream clientStream;
        private Thread readThread;
        private bool isReading;
        private static readonly object locker = new();

        private const int TimeoutMilliseconds = 300000; // Set the timeout duration in milliseconds

        public event EventHandler<RequestEventArgs>? RequestEvent;
        public RequestHandler(TcpClient client)
        {
            Client = client;
            clientStream = null!;
            readThread = null!;
        }
        public TcpClient Client { get; private set; }
        public void Start()
        {
            try
            {
                clientStream = Client.GetStream();
                isReading = true;
                readThread = new Thread(Read);
                readThread.Start();
            }
            catch
            {
                Console.WriteLine("Server RequestHandler could not be started.");
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
                        if (!WaitForData(TimeoutMilliseconds)) // Wait for data with a timeout
                        {
                            Stop(); // Stop the request handler if timeout occurs
                            break;
                        }
                        continue;
                    }

                    List<byte> receivedBytes = [];
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

                    EventRequestReceived(receivedBytes.ToArray());
                }
                catch
                {
                    Console.WriteLine("Server error!");
                }
            }
        }
        public void Stop()
        {
            try
            {
                isReading = false;
                clientStream.Close();
                Client.Close();
            }
            catch
            {
                Console.WriteLine("Connection could not be closed!");
            }
        }
        public void Send(Protocol.Protocol protocol)
        {
            lock (locker)
            {
                try
                {
                    byte[] sendBytes = protocol.ToBytes();
                    if (clientStream.CanWrite)
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

        private bool WaitForData(int timeoutMilliseconds)
        {
            int elapsedMilliseconds = 0;
            const int sleepInterval = 10;
            while (!clientStream.DataAvailable && elapsedMilliseconds < timeoutMilliseconds)
            {
                Thread.Sleep(sleepInterval);
                elapsedMilliseconds += sleepInterval;
            }
            return clientStream.DataAvailable;
        }
    }
}
