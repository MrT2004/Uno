using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLib.Protocol
{
    public class Protocol
    {
        private readonly byte[] header = Encoding.ASCII.GetBytes("UNO");
        private readonly byte[] tail = Encoding.ASCII.GetBytes("!");
        //write constructor
        public Protocol(byte[] type, byte[] content)
        {
            this.Type = type;
            this.Content = content;
        }
        //read constructor
        public Protocol(byte[] bytes)
        {
            if (header[0] == bytes[0] && header[1] == bytes[1] && header[2] == bytes[2])
            {
                this.Type = new byte[] { bytes[3], bytes[4] };
                this.Content = new byte[bytes.Length - 5];
                Array.Copy(bytes, 5, this.Content, 0, this.Content.Length);
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
            protocol.AddRange(this.header);
            protocol.AddRange(this.Type);
            protocol.AddRange(this.Content);
            protocol.AddRange(this.tail);
            return protocol.ToArray();
        }
    }
}
