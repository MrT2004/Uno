using Uno_Server.Networking;

namespace Uno
{
    public class Program
    {
        public static void Main(string[] args)
        {
            UnoServerMain server = new UnoServerMain();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(@"
                                _    _
                _______________| |  | |  
               / __ | |  |_  __| |  | |_ __   ___  
              | /   | |   | |  | |  | | '_ \ / _ \
              | \__ | |___| |__| |..| | | | | (_) |
               \____|___________\____/|_| |_|\___/ 
                ");
            Console.ResetColor();
            Console.WriteLine("\n[S] Start server [E] Stop server [Q] Quit\n");
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey();
                if (key.Key == ConsoleKey.S)
                {
                    if (server.isRunning)
                    {
                        Console.WriteLine("\nServer is already running.\n");
                    }
                    else
                    {
                        server.Start();
                        Console.WriteLine("\nServer started.");
                    }

                }
                else if (key.Key == ConsoleKey.E)
                {
                    if (!server.isRunning)
                    {
                        Console.WriteLine("\nServer is not running.\n");
                    }
                    else
                    {
                        server.Stop();
                        Console.WriteLine("\nServer stopped.");
                    }
                }
                else if (key.Key == ConsoleKey.Q)
                {
                    if (server.isRunning == true)
                    {
                        server.Stop();
                    }

                    Environment.Exit(0);
                    break;
                }
            }
        }
    }
}
