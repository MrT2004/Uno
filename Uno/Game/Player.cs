namespace Uno_Server.Game
{
    public class Player
    {
        public Player(int playerID, Networking.RequestHandler requestHandler)
        {
            PlayerID = playerID;
            RequestHandler = requestHandler;
        }

        public int PlayerID { get; set; }
        public Networking.RequestHandler RequestHandler { get; set; }
        public Deck Hand { get; set; } = new Deck();
    }
}
