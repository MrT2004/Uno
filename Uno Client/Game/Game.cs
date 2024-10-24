using System.Text;
using Uno_Client.Networking;
using Uno_Client.Protocol;

namespace Uno_Client.Game
{
    public class Game
    {
        private readonly Networking.RequestHandler requestHandler;
        private int playerID;
        private int gameID;
        private int winnerID;
        private int currentPlayer;
        private Card? lastCard;
        private string availableGames = "";
        private List<Card> hand = [];
        private string[] playerHandLengths = [];
        private bool OK = false;
        private bool gameOver = false;
        private bool responseRecieved = false;
        private bool reload = false;

        public Game(Networking.RequestHandler requestHandler)
        {
            this.requestHandler = requestHandler;
            this.requestHandler.RequestEvent += HandleRequest;
        }
        public void CreateGamePage()
        {
            int playerCount = 2;
            Console.Clear();

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Chose number of players with UP and DOWN arrow keys");
                Console.WriteLine("Press ENTER to create game (M to return to Menu) ");
                Console.Write("Players: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(playerCount);

                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.UpArrow)
                {
                    if (playerCount < 4)
                    {
                        playerCount++;
                    }
                    else
                    {
                        playerCount = 2;
                    }
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    if (playerCount > 2)
                    {
                        playerCount--;
                    }
                    else
                    {
                        playerCount = 4;
                    }
                }
                else if (key.Key == ConsoleKey.M)
                {
                    MainMenu.Menu();
                    break;
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    Console.Clear();
                    requestHandler.Start();
                    if (requestHandler.Connected)
                    {
                        requestHandler.Send(ProtocolManager.CreateGame(playerCount.ToString()));
                        WaitForResponse();
                        if (OK)
                        {
                            WaitForGameStart();
                            GamePage();
                            break;
                        }
                        else
                        {
                            Console.WriteLine("There was an error creating the game. Press any key to return to menu.");
                            Console.ReadKey(true);
                        }
                    }
                    break;
                }
                Console.Clear();
            }
            Console.Clear();
            MainMenu.Menu();
        }
        public void JoinGamePage()
        {
            requestHandler.Start();
            if (requestHandler.Connected)
            {
                int choice = 0;

                requestHandler.Send(ProtocolManager.RequestGameList());
                WaitForResponse();
                string[] gameList = availableGames.Split(',');
                PaintJoinGamePage(gameList);
                while (true)
                {
                    ConsoleKeyInfo key = Console.ReadKey();
                    if (key.Key == ConsoleKey.R)
                    {
                        Console.Clear();
                        requestHandler.Send(ProtocolManager.RequestGameList());
                        WaitForResponse();
                        PaintJoinGamePage(gameList);
                    }
                    else if (key.Key == ConsoleKey.UpArrow)
                    {
                        if (choice > 0)
                        {
                            choice--;
                        }
                        else
                        {
                            choice = gameList.Length - 1;
                        }
                        PaintJoinGamePage(gameList, choice);
                    }
                    else if (key.Key == ConsoleKey.DownArrow)
                    {
                        if (choice < gameList.Length - 1)
                        {
                            choice++;
                        }
                        else
                        {
                            choice = 0;
                        }
                        PaintJoinGamePage(gameList, choice);
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        requestHandler.Send(ProtocolManager.JoinGame(gameList[choice].Split('-')[0]));
                        WaitForResponse();
                        if (OK)
                        {
                            WaitForGameStart();
                            GamePage();
                            break;
                        }
                        else
                        {
                            Console.WriteLine("There was an error joining the game. Press any key to return to return to menu.");
                            Console.ReadKey(true);
                            MainMenu.Menu();
                            break;
                        }

                    }
                    else if (key.Key == ConsoleKey.M)
                    {
                        MainMenu.Menu();
                        break;
                    }
                }
            }
        }
        public void PaintJoinGamePage(string[] gameList, int currentChoice = 0)
        {
            Console.Clear();
            Console.WriteLine("[R]efresh [M]enu\n");
            if (availableGames == null || availableGames == string.Empty)
            {
                Console.WriteLine("No games available. You'll need to create one.");
            }
            else
            {
                for (int i = 0; i < gameList.Length; i++)
                {
                    if (i == currentChoice)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    string[] gameInfo = gameList[i].Split('-');
                    Console.WriteLine($@"
                            Game ID: {gameInfo[0]}
                            Players: {gameInfo[1]}/{gameInfo[2]}

                        ");
                }
            }
        }
        public void GamePage()
        {

            while (!gameOver)
            {
                PaintGamePage();
                if (playerID == currentPlayer)
                {
                    ChooseCard();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\nWaiting for player {0}...", currentPlayer + 1);
                }
                while (!reload)
                {
                    WaitForResponse();
                }
                reload = false;
            }
            EndGame();
        }
        public void PaintGamePage()
        {
            Console.Clear();
            //stats
            if (playerID == currentPlayer)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("It's your turn!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("It's player {0}'s turn!", currentPlayer + 1);
            }
            for (int i = 0; i < playerHandLengths.Length; i++)
            {
                if (i == playerID)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                }
                Console.WriteLine("Player {0}: {1} cards", i + 1, playerHandLengths[i]);
            }
            Console.WriteLine();
            Console.ResetColor();

            //draw and discard piles
            Card drawPile = new(Color.Wild, Value.DrawPile);
            if (lastCard != null)
            {
                Card discardPile = lastCard;
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(8, playerHandLengths.Length + 4);
                Console.Write("[D]RAW  DISCARD");
                drawPile.Paint(7, playerHandLengths.Length + 5);
                discardPile.Paint(15, playerHandLengths.Length + 5);
            }

        }
        private void ChooseCard()
        {
            int lastVisibleCard = hand.Count > 5 ? 4 : hand.Count - 1;
            int choice = 0;
            int firstVisibleCard = 0;
            while (!gameOver)
            {
                if (firstVisibleCard > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }
                //Draw left arrow
                Console.SetCursorPosition(1, playerHandLengths.Length + 17);
                Console.WriteLine(" /|_ ");
                Console.SetCursorPosition(1, playerHandLengths.Length + 18);
                Console.WriteLine("|  _|");
                Console.SetCursorPosition(1, playerHandLengths.Length + 19);
                Console.WriteLine(@" \|  ");
                int posX = 7;
                for (int i = firstVisibleCard; i <= lastVisibleCard; i++, posX += 8)
                {
                    if (i == choice)
                    {
                        //draw arrow pointing to choice
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.SetCursorPosition(posX + 1, playerHandLengths.Length + 11);
                        Console.WriteLine("  _  ");
                        Console.SetCursorPosition(posX + 1, playerHandLengths.Length + 12);
                        Console.WriteLine("_| |_");
                        Console.SetCursorPosition(posX + 1, playerHandLengths.Length + 13);
                        Console.WriteLine(@"\   /");
                        Console.SetCursorPosition(posX + 1, playerHandLengths.Length + 14);
                        Console.WriteLine(@" \_/ ");
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    else
                    {
                        Console.SetCursorPosition(posX + 1, playerHandLengths.Length + 11);
                        Console.WriteLine("     ");
                        Console.SetCursorPosition(posX + 1, playerHandLengths.Length + 12);
                        Console.WriteLine("     ");
                        Console.SetCursorPosition(posX + 1, playerHandLengths.Length + 13);
                        Console.WriteLine("     ");
                        Console.SetCursorPosition(posX + 1, playerHandLengths.Length + 14);
                        Console.WriteLine("     ");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    hand[i].Paint(posX, playerHandLengths.Length + 15);
                }
                if (lastVisibleCard < hand.Count - 1)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }
                //Draw right arrow
                Console.SetCursorPosition(posX, playerHandLengths.Length + 17);
                Console.WriteLine(@" _|\ ");
                Console.SetCursorPosition(posX, playerHandLengths.Length + 18);
                Console.WriteLine("|_  |");
                Console.SetCursorPosition(posX, playerHandLengths.Length + 19);
                Console.WriteLine("  |/ ");
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.LeftArrow)
                {
                    if (choice == 0)
                    { }
                    else if (choice == firstVisibleCard)
                    {
                        choice--;
                        firstVisibleCard--;
                        lastVisibleCard--;
                    }
                    else
                    {
                        choice--;
                    }
                }
                else if (key.Key == ConsoleKey.RightArrow)
                {
                    if (choice == hand.Count-1)
                    { }
                    else if (choice == lastVisibleCard)
                    {
                        choice++;
                        firstVisibleCard++;
                        lastVisibleCard++;
                    }
                    else
                    {
                        choice++;
                    }
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    string? newColor = null;

                    if (hand[choice].Color == Color.Wild)
                    {
                        newColor = ChooseColor();
                    }
                    Card card = hand[choice];
                    requestHandler.Send(ProtocolManager.UseCard(gameID.ToString(), playerID.ToString(), card.Color.ToString(), card.Value.ToString(), newColor));
                    WaitForResponse();
                    //If you draw all the cards and then try to play a draw 4 card, OK will be false when debugging and true otherwise
                    if (OK)
                    {
                        break;
                    }
                    else
                    {
                        Console.SetCursorPosition(0, playerHandLengths.Length + 22);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("You can't play that card.                      \n                                                         ");
                    }
                }
                else if (key.Key == ConsoleKey.D)
                {
                    requestHandler.Send(ProtocolManager.DrawCard(gameID.ToString(), playerID.ToString()));
                    WaitForResponse();
                    if (OK)
                    {
                        break;
                    }
                    else
                    {
                        Console.SetCursorPosition(0, playerHandLengths.Length + 22);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("All cards are in a player's hand.               \n                                                         ");
                    }
                }
            
            }
        }
        private string ChooseColor()
        {
            Console.SetCursorPosition(0, playerHandLengths.Length + 22);
            Console.WriteLine("          Choose a color:                       ");
            Console.WriteLine("[R]ed [Y]ellow [G]reen [B]lue");
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.R)
                {
                    return Color.Red.ToString();
                }
                else if (key.Key == ConsoleKey.B)
                {
                    return Color.Blue.ToString();
                }
                else if (key.Key == ConsoleKey.G)
                {
                    return Color.Green.ToString();
                }
                else if (key.Key == ConsoleKey.Y)
                {
                    return Color.Yellow.ToString();
                }
            }
        }
        private void WaitForResponse()
        {
            while (!responseRecieved)
            {
                Thread.Sleep(10);
            }
            responseRecieved = false;
        }
        private void EndGame()
        {
            requestHandler.Stop();
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Game Over!");
            if (playerID == winnerID)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("You won!\n\n");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Player {0} won!\n\n", winnerID + 1);
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Press any key to return to menu");
            Console.ReadKey(true);
            MainMenu.Menu();
        }
        private void WaitForGameStart()
        {
            Console.Clear();
            MainMenu.ShowTitle();
            Console.WriteLine("Waiting for players to join.");
            while (!reload)
            {
                WaitForResponse();
            }
            reload = false;

        }
        private void HandleRequest(object? sender, RequestEventArgs args)
        {
            if (args.Protocol != null)
            {
                if (args.Protocol.Type.SequenceEqual(ProtocolTypes.OK))
                {
                    OK = true;
                    responseRecieved = true;
                }
                else if (args.Protocol.Type.SequenceEqual(ProtocolTypes.Error))
                {
                    OK = false;
                    responseRecieved = true;
                }
                else if (args.Protocol.Type.SequenceEqual(ProtocolTypes.GameList))
                {
                    availableGames = Encoding.ASCII.GetString(args.Protocol.Content);
                    OK = true;
                    responseRecieved = true;
                }
                else if (args.Protocol.Type.SequenceEqual(ProtocolTypes.GameInfo))
                {
                    string[] gameInfo = Encoding.ASCII.GetString(args.Protocol.Content).Split('-');
                    if (Enum.TryParse(gameInfo[0], out Color color) && Enum.TryParse(gameInfo[1], out Value value))
                    {
                        lastCard = new(color, value);
                    }
                    else
                    {
                        Console.WriteLine($"Error parsing card. Color: {gameInfo[0]}, Value: {gameInfo[1]}");
                    }
                    currentPlayer = int.Parse(gameInfo[2]);
                    playerHandLengths = gameInfo[3].Split(',');
                    OK = true;
                    responseRecieved = true;
                    reload = true;
                }
                else if (args.Protocol.Type.SequenceEqual(ProtocolTypes.PlayerCards))
                {
                    string[] cards = Encoding.ASCII.GetString(args.Protocol.Content).Split(',');
                    hand = [];
                    if (cards.Length > 0)
                    {
                        foreach (string card in cards)
                        {
                            string[] cardAttributes = card.Split('-');
                            if (Enum.TryParse(cardAttributes[0], out Color color) && Enum.TryParse(cardAttributes[1], out Value value))
                            {
                                hand.Add(new Card(color, value));
                            }
                            else
                            {
                                Console.WriteLine($"Error parsing card: {card}. Color: {cardAttributes[0]}, Value: {cardAttributes[1]}");
                            }
                        }
                    }
                    OK = true;
                    responseRecieved = true;
                }
                else if (args.Protocol.Type.SequenceEqual(ProtocolTypes.GameStart))
                {
                    string[] gameStartInfo = Encoding.ASCII.GetString(args.Protocol.Content).Split('-');
                    gameID = int.Parse(gameStartInfo[0]);
                    playerID = int.Parse(gameStartInfo[1]);
                    OK = true;
                    responseRecieved = true;
                }
                else if (args.Protocol.Type.SequenceEqual(ProtocolTypes.GameEnd))
                {
                    gameOver = true;
                    winnerID = int.Parse(args.Protocol.Content);
                    OK = true;
                    responseRecieved = true;
                    reload = true;
                }
                else
                {
                    OK = false;
                    responseRecieved = true;
                }
            }
        }
    }
}
