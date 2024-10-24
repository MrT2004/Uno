using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLib.Protocol
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
        public static Protocol GameStart(string gameID, string playerID)
        {
            return new Protocol(ProtocolTypes.GameStart, Encoding.ASCII.GetBytes(gameID + '-' + playerID));
        }
        public static Protocol GameEnd(string playerID)
        {
            return new Protocol(ProtocolTypes.GameEnd, Encoding.ASCII.GetBytes(playerID));
        }
        public static Protocol PlayerCards(Player player)
        {
            string playerCards = string.Join(",", player.Hand.Cards.Select(card => $"{card.Color}-{card.Value}"));
            return new Protocol(ProtocolTypes.PlayerCards, Encoding.ASCII.GetBytes(playerCards));
        }
        public static Protocol GameInfo(Card lastCard, Player curentPlayer, List<Player> players)
        {
            string handLengths = string.Join(",", players.Select(player => player.Hand.Cards.Count));
            string gameInfo = $"{lastCard.Color}-{lastCard.Value}-{curentPlayer.PlayerID}-{handLengths}";
            return new Protocol(ProtocolTypes.GameInfo, Encoding.ASCII.GetBytes(gameInfo));
        }
        //public static Protocol IsAlive()
        //{
        //    return new Protocol(ProtocolTypes.IsAlive, Array.Empty<byte>());
        //}
        public static Protocol GameList(List<Game.Game> games)
        {
            string gameList = string.Join(",", games.Where(game => game.ActivePlayers < game.PlayerCount)
                                                    .Select(game => $"{game.GameID}-{game.ActivePlayers}-{game.PlayerCount}"));
            return new Protocol(ProtocolTypes.GameList, Encoding.ASCII.GetBytes(gameList));
        }
    }
}
