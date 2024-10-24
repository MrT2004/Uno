namespace Uno_Server.Game
{
    public class Game(int gameID, int playerCount)
    {
        private Deck drawPile = new();
        private readonly Deck discardPile = new();
        private int currentPlayerIndex = 0;
        private Flow flow = Flow.Forward;

        public int GameID { get; set; } = gameID;
        public int PlayerCount { get; set; } = playerCount;
        public List<Player> Players { get; set; } = [];
        public int ActivePlayers { get; set; } = 1;

        public void PrepGame()
        {
            InitDeck();
            discardPile.Add(drawPile.DrawCard());
            for (int i = 0; i < 7; i++)
            {
                foreach (Player player in Players)
                {
                    player.Hand.Add(drawPile.DrawCard());
                }
            }
            foreach (Player player in Players)
            {
                player.Hand.Sort();
                SendPlayerHand(player);
            }
            SendGameInfo();
            

        }
        public void InitDeck()
        {
            drawPile = new Deck();
            foreach (Color color in Enum.GetValues(typeof(Color)).Cast<Color>().Where(c => (int)c >= 0 && (int)c <= 3))
            {

                drawPile.Add(new Card(color, Value.Zero));
                foreach (Value value in Enum.GetValues(typeof(Value)).Cast<Value>().Where(c => (int)c >= 1 && (int)c <= 12))
                {
                    drawPile.Add(new Card(color, value));
                    drawPile.Add(new Card(color, value));
                }
            }
            for (int i = 0; i < 4; i++)
            {
                drawPile.Add(new Card(Color.Wild, Value.Wild));
                drawPile.Add(new Card(Color.Wild, Value.DrawFour));
            }
         
            do
            {
                drawPile.Shuffle();
            }
            while (drawPile.Cards[0].Color == Color.Wild);
        }
        private bool Reshuffle(int minCards = 1)
        {
            if (discardPile.Cards.Count <= minCards)
            {
                return false;
            }
            Card tempCard = discardPile.DrawCard();
            
            drawPile.Cards.AddRange(discardPile.Cards);
            discardPile.Cards.Clear();
            discardPile.Cards.Add(tempCard);
            drawPile.Shuffle();
            return true;
        }
        private static void SendPlayerHand(Player player)
        {
            player.RequestHandler.Send(Protocol.ProtocolManager.PlayerCards(player));


        }
        private void SendGameInfo()
        {
            foreach (Player player in Players)
            {
                player.RequestHandler.Send(Protocol.ProtocolManager.GameInfo(discardPile.Cards[0], Players[currentPlayerIndex], Players));
            }
        }
        public void ActionDrawCard(int playerID)
        {
            Player curPlayer = Players[currentPlayerIndex];
            if (playerID != currentPlayerIndex)
            {
                curPlayer.RequestHandler.Send(Protocol.ProtocolManager.Error());
            }
            else
            {
                if (drawPile.Cards.Count < 1)
                {
                    if (!Reshuffle())
                    {
                        curPlayer.RequestHandler.Send(Protocol.ProtocolManager.Error());
                        return;
                    }
                }
                curPlayer.Hand.InsertCard(drawPile.DrawCard());
                curPlayer.RequestHandler.Send(Protocol.ProtocolManager.OK());
                foreach (Player player in Players)
                {
                    SendPlayerHand(player);
                }
                SendGameInfo();
            }
        }
        public void ActionUseCard(int playerID, Card card, Color? newColor = null)
        {
            Player curPlayer = Players[playerID];
            if (playerID != currentPlayerIndex)
            {
                curPlayer.RequestHandler.Send(Protocol.ProtocolManager.Error());
            }
            else
            {
                if (!card.CanPlayOn(discardPile.Cards[0]))
                {
                    curPlayer.RequestHandler.Send(Protocol.ProtocolManager.Error());
                }
                else
                {
                    discardPile.Push(card);
                    curPlayer.RequestHandler.Send(Protocol.ProtocolManager.OK());
                    switch (card.Value)
                    {
                        case Value.Skip:
                            currentPlayerIndex = NextPlayer(2);
                            break;
                        case Value.Reverse:
                            flow = flow == Flow.Forward ? Flow.Backwards : Flow.Forward;
                            if (Players.Count > 2)
                            {
                                currentPlayerIndex = NextPlayer();
                            }
                            break;
                        case Value.DrawTwo:
                            if (drawPile.Cards.Count < 2)
                            {
                                if (!Reshuffle(2))
                                {
                                    discardPile.Cards.RemoveAt(0);
                                    curPlayer.RequestHandler.Send(Protocol.ProtocolManager.Error());
                                    return;
                                }
                            }
                            Players[NextPlayer()].Hand.InsertCard(drawPile.DrawCard());
                            Players[NextPlayer()].Hand.InsertCard(drawPile.DrawCard());
                            currentPlayerIndex =NextPlayer(2);
                            break;
                        case Value.Wild:
                            if (newColor == null)
                            {
                                curPlayer.RequestHandler.Send(Protocol.ProtocolManager.Error());
                            }
                            else
                            {
                                card = new(card.Color, card.Value);
                                discardPile.Cards[0].Color = newColor.Value;
                            }
                            currentPlayerIndex = NextPlayer();
                            break;
                        case Value.DrawFour:
                            if (drawPile.Cards.Count < 4)
                            {
                                if (!Reshuffle(4))
                                {
                                    discardPile.Cards.RemoveAt(0);
                                    curPlayer.RequestHandler.Send(Protocol.ProtocolManager.Error());
                                    return;
                                }
                            }
                            if (newColor == null)
                            {
                                curPlayer.RequestHandler.Send(Protocol.ProtocolManager.Error());
                            }
                            else
                            {
                                card = new(card.Color, card.Value);
                                discardPile.Cards[0].Color = newColor.Value;
                            }
                            Players[NextPlayer()].Hand.InsertCard(drawPile.DrawCard());
                            Players[NextPlayer()].Hand.InsertCard(drawPile.DrawCard());
                            Players[NextPlayer()].Hand.InsertCard(drawPile.DrawCard());
                            Players[NextPlayer()].Hand.InsertCard(drawPile.DrawCard());
                            currentPlayerIndex = NextPlayer(2);
                            break;
                        default:
                            currentPlayerIndex = NextPlayer();
                            break;
                    }

                    
                    Card? cardInstance = curPlayer.Hand.Cards.Find(c => c.Color == card.Color && c.Value == card.Value);
                    if (cardInstance == null)
                    {
                        Console.WriteLine("Error finding card in player's hand.");
                    }
                    else
                    {
                        if (!Players[playerID].Hand.Cards.Remove(cardInstance))
                        {
                            Console.WriteLine("Card could not be removed from hand.");
                        }
                        else
                        {
                            foreach (Player player in Players)
                            {
                                SendPlayerHand(player);
                            }

                            if (curPlayer.Hand.Cards.Count == 0)
                            {
                                EndGame(playerID);
                            }
                            else
                            {
                                SendGameInfo();
                            }
                        }
                    }
                }
            }
        }
        public int NextPlayer(int repeat = 1)
        {
            int nextIndex = currentPlayerIndex;
            for (int i = 0; i < repeat; i++)
            {
                if (flow == Flow.Forward)
                {
                    nextIndex++;
                    if (nextIndex >= Players.Count)
                    {
                        nextIndex = 0;
                    }
                }
                else
                {
                    nextIndex--;
                    if (nextIndex < 0)
                    {
                        nextIndex = Players.Count - 1;
                    }
                }
            }
            return nextIndex;
        }
        private void EndGame(int playerID)
        {
            foreach (Player p in Players)
            {
                p.RequestHandler.Send(Protocol.ProtocolManager.GameEnd(playerID.ToString()));
            }
        }
    }
}
