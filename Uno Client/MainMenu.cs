using System.Net;

namespace Uno_Client
{
    internal class MainMenu
    {
        public static void Menu()
        {
            Networking.RequestHandler requesthandler = new(IPAddress.Loopback);
            Console.Clear();
            string[] options = { "Create Game", "Join Game", "Exit" };
            int choice = 0;
            Game.Game game = new(requesthandler);
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                ShowTitle();
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == choice)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    Console.WriteLine(options[i]);
                }
                ConsoleKeyInfo key = Console.ReadKey();
                if (key.Key == ConsoleKey.UpArrow)
                {
                    if (choice > 0)
                    {
                        choice--;
                    }
                    else
                    {
                        choice = options.Length - 1;
                    }
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    if (choice < options.Length - 1)
                    {
                        choice++;
                    }
                    else
                    {
                        choice = 0;
                    }
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
                Console.Clear();
            }
            switch (choice)
            {
                case 0:
                    game.CreateGamePage();
                    break;
                case 1:
                    game.JoinGamePage();
                    break;
                case 2:
                    Environment.Exit(0);
                    break;
            }

        }
        public static void ShowTitle()
        {
            Console.WriteLine(@"
                                _    _
                _______________| |  | |  
               / __ | |  |_  __| |  | |_ __   ___  
              | /   | |   | |  | |  | | '_ \ / _ \
              | \__ | |___| |__| |..| | | | | (_) |
               \____|___________\____/|_| |_|\___/ 
                ");
            Console.ResetColor();
        }
    }
}
