using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLib.Protocol
{
    public class ProtocolTypes
    {
        //GameStart : GS
        public static byte[] GameStart = Encoding.ASCII.GetBytes("GS");
        //GameEnd : GE
        public static byte[] GameEnd = Encoding.ASCII.GetBytes("GE");
        //UseCard : UC
        public static byte[] UseCard = Encoding.ASCII.GetBytes("UC");
        //OK : OK
        public static byte[] OK = Encoding.ASCII.GetBytes("OK");
        //Error : ER
        public static byte[] Error = Encoding.ASCII.GetBytes("ER");
        //PlayerCards : PC
        public static byte[] PlayerCards = Encoding.ASCII.GetBytes("PC");
        //GameInfo : GI
        public static byte[] GameInfo = Encoding.ASCII.GetBytes("GI");
        //JoinGame : JG
        public static byte[] JoinGame = Encoding.ASCII.GetBytes("JG");
        //CreateGame : CG
        public static byte[] CreateGame = Encoding.ASCII.GetBytes("CG");
        //IsAlive : IA
        //public static byte[] IsAlive = Encoding.ASCII.GetBytes("IA");
        //GameList : GL
        public static byte[] GameList = Encoding.ASCII.GetBytes("GL");
        //DrawCard : DC
        public static byte[] DrawCard = Encoding.ASCII.GetBytes("DC");
    }
}
