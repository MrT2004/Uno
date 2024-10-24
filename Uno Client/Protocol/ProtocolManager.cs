using System.Text;

namespace Uno_Client.Protocol
{
    public class ProtocolManager
    {
        public static Protocol OK()
        {
            return new Protocol(ProtocolTypes.OK, Array.Empty<byte>());
        }
        public static Protocol Error()
        {
            return new Protocol(ProtocolTypes.Error, Array.Empty<byte>());
        }
        public static Protocol CreateGame(string playerCount)
        {
            return new Protocol(ProtocolTypes.CreateGame, Encoding.ASCII.GetBytes(playerCount));
        }
        public static Protocol JoinGame(string gameID)
        {
            return new Protocol(ProtocolTypes.JoinGame, Encoding.ASCII.GetBytes(gameID));
        }
        public static Protocol RequestGameList()
        {
            return new Protocol(ProtocolTypes.RequestGameList, Array.Empty<byte>());
        }
        public static Protocol DrawCard(string gameID, string playerID)
        {
            return new Protocol(ProtocolTypes.DrawCard, Encoding.ASCII.GetBytes($"{gameID}-{playerID}"));
        }
        public static Protocol UseCard(string gameID, string playerID, string cardColor, string cardValue, string? newColor = null)
        {
            return new Protocol(ProtocolTypes.UseCard, Encoding.ASCII.GetBytes($"{gameID}-{playerID}-{cardColor}-{cardValue}-{newColor}"));
        }

        //public static Protocol IsAlive()
        //{
        //    return new Protocol(ProtocolTypes.IsAlive, Array.Empty<byte>());
        //}
    }
}
