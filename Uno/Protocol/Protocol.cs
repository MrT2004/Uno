using System.Text;

namespace Uno_Server.Protocol
{
    public class Protocol
    {
        private readonly byte[] header = Encoding.ASCII.GetBytes("UNO");
        private readonly byte[] tail = Encoding.ASCII.GetBytes("!");
        //write constructor
        public Protocol(byte[] type, byte[] content)
        {
            Type = type;
            Content = content;
        }
        //read constructor
        public Protocol(byte[] bytes)
        {
            if (header[0] == bytes[0] && header[1] == bytes[1] && header[2] == bytes[2])
            {
                Type = new byte[] { bytes[3], bytes[4] };
                Content = new byte[bytes.Length - 5];
                Array.Copy(bytes, 5, Content, 0, Content.Length);
            }
            else
            {
                throw new Exception("Invalid Protocol");
            }
        }
        public byte[] Type { get; set; }
        public byte[] Content { get; set; }
        public byte[] ToBytes()
        {
            var protocol = new List<byte>();
            protocol.AddRange(header);
            protocol.AddRange(Type);
            protocol.AddRange(Content);
            protocol.AddRange(tail);
            return protocol.ToArray();
        }
    }
}
