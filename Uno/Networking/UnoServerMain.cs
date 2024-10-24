using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Uno_Server.Networking
{
    public class UnoServerMain
    {
        private TcpListener? serverSocket;
        private Thread? listenerThread;
        private List<Game.Game> games = [];
        private int nextGameID;
        public bool isRunning = false;
        public UnoServerMain()
        {
            nextGameID = 1;
        }
        public void Start()
        {
            isRunning = true;
            games = [];
            serverSocket = new TcpListener(IPAddress.Any, Constants.PORT);
            listenerThread = new Thread(StartListening);
            listenerThread.Start();
        }
        public void Stop()
        {
            isRunning = false;
            serverSocket?.Stop();
        }
        public void StartListening()
        {
            if (serverSocket == null) return;
            serverSocket.Start();
            while (isRunning)
            {
                if (!serverSocket.Pending())
                {
                    Thread.Sleep(100);
                    continue;
                }

                var client = serverSocket.AcceptTcpClient();
                Thread connection = new(new ParameterizedThreadStart(HandleConnection));
                connection.Start(client);
            }
        }
        public void HandleConnection(object? data)
        {
            if (data == null) return;
            TcpClient client = (TcpClient)data;
            RequestHandler requestHandler = new RequestHandler(client);
            requestHandler.RequestEvent += HandleRequest;
            requestHandler.Start();
        }
        public void HandleRequest(object? sender, RequestEventArgs args)
        {
            if (args.Protocol != null)
            {
                if (args.Protocol.Type.SequenceEqual(Protocol.ProtocolTypes.CreateGame))
                {
                    bool parseInt = int.TryParse(args.Protocol.Content, out int playerCount);
                    if (parseInt)
                    {
                        CreateGame(playerCount, args.RequestHandler);
                    }
                }
                else if (args.Protocol.Type.SequenceEqual(Protocol.ProtocolTypes.JoinGame))
                {
                    bool parseInt = int.TryParse(args.Protocol.Content, out int gameID);
                    if (parseInt)
                    {
                        JoinGame(gameID, args.RequestHandler);
                    }
                }
                else if (args.Protocol.Type.SequenceEqual(Protocol.ProtocolTypes.RequestGameList))
                {
                    SendGameList(args.RequestHandler);
                }
                else if (args.Protocol.Type.SequenceEqual(Protocol.ProtocolTypes.UseCard))
                {
                    string content = Encoding.ASCII.GetString(args.Protocol.Content);
                    string[] cardAttributes = content.Split('-');
                    Game.Game? game = games.Find(game => game.GameID == int.Parse(cardAttributes[0]));
                    if (game == null)
                    {
                        Console.WriteLine("Game not found.");
                        return;
                    }
                    if (Enum.TryParse(cardAttributes[2], out Game.Color color) && Enum.TryParse(cardAttributes[3], out Game.Value value))
                    {
                        if (color == Game.Color.Wild)
                        {
                            if (Enum.TryParse(cardAttributes[4], out Game.Color newColor))
                            {
                                game.ActionUseCard(int.Parse(cardAttributes[1]), new Game.Card(color, value), newColor);
                            }
                            else
                            {
                                Console.WriteLine("Error sending new color to server");
                            }
                        }
                        else
                        {
                            game.ActionUseCard(int.Parse(cardAttributes[1]), new Game.Card(color, value), null);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Error parsing card. Color: {cardAttributes[2]}, Value: {cardAttributes[3]}");
                    }
                    
                }
                else if (args.Protocol.Type.SequenceEqual(Protocol.ProtocolTypes.DrawCard))
                {
                    string content = Encoding.ASCII.GetString(args.Protocol.Content);
                    string[] playerInfo = content.Split('-');
                    Game.Game? game = games.Find(game => game.GameID == int.Parse(playerInfo[0]));
                    if (game == null)
                    {
                        Console.WriteLine("Game not found.");
                        return;
                    }
                    game.ActionDrawCard(int.Parse(playerInfo[1]));
                }
                else
                {
                    args.RequestHandler.Send(Protocol.ProtocolManager.Error());

                }
            }
        }
        private void CreateGame(int playerCount, RequestHandler requestHandler)
        {
            Game.Game game = new(nextGameID, playerCount);
            games.Add(game);
            nextGameID++;
            game.Players.Add(new Game.Player(0, requestHandler));
            if (playerCount > 1 && playerCount < 5)
            {
                requestHandler.Send(Protocol.ProtocolManager.OK());
                Console.WriteLine("{0} created a new game (GameID: {1}, Players: {2}/{3})!", ((IPEndPoint)requestHandler.Client.Client.RemoteEndPoint).Address.ToString(), game.GameID, game.ActivePlayers, game.PlayerCount);
            }
            else
            {
                requestHandler.Send(Protocol.ProtocolManager.Error());
            }
        }
        private void JoinGame(int gameID, RequestHandler requestHandler)
        {
            Game.Game? game = games.Find(game => game.GameID == gameID);
            if (game != null)
            {
                if (game.ActivePlayers < game.PlayerCount)
                {
                    game.Players.Add(new Game.Player(game.ActivePlayers, requestHandler));
                    game.ActivePlayers++;
                    requestHandler.Send(Protocol.ProtocolManager.OK());
                    Console.WriteLine("{0} joined game {1} (Players: {2}/{3})!", ((IPEndPoint)requestHandler.Client.Client.RemoteEndPoint).Address.ToString(), game.GameID, game.ActivePlayers, game.PlayerCount);
                    if (game.ActivePlayers == game.PlayerCount)
                    {
                        StartGame(game);
                    }
                }
                else
                {
                    requestHandler.Send(Protocol.ProtocolManager.Error());
                }
            }
            else
            {
                requestHandler.Send(Protocol.ProtocolManager.Error());
            }
        }
        private static void StartGame(Game.Game game)
        {
            game.PrepGame();
            foreach (Game.Player player in game.Players)
            {
                player.RequestHandler.Send(Protocol.ProtocolManager.GameStart(game.GameID.ToString(), player.PlayerID.ToString()));
            }
            Console.WriteLine("A game started (GameID: {0}, Players: {1}/{2})!", game.GameID, game.ActivePlayers, game.PlayerCount);
        }
        private void SendGameList(RequestHandler requestHandler)
        {
            requestHandler.Send(Protocol.ProtocolManager.GameList(games));
        }
    }
}
