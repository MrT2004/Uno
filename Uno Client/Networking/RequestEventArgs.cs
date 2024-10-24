namespace Uno_Client.Networking
{
    public class RequestEventArgs(byte[] bytes, RequestHandler requestHandler) : EventArgs
    {
        public Protocol.Protocol Protocol { get; set; } = new Protocol.Protocol(bytes);
        public RequestHandler RequestHandler { get; set; } = requestHandler;
    }
}
