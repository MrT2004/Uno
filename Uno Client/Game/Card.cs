

namespace Uno_Client.Game
{
    public class Card
    {
        public Card(Color color, Value value)
        {
            Color = color;
            Value = value;
        }

        public Color Color { get; private set; }
        public Value Value { get; private set; }


        public override string ToString()
        {
            return $"{Color} {Value}";
        }
        public void Paint(int x, int y)
        {
            switch (Color)
            {
                case Color.Red:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case Color.Blue:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case Color.Green:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case Color.Yellow:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case Color.Wild:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
            Console.SetCursorPosition(x, y);
            Console.WriteLine(" ┌─────┐");
            Console.SetCursorPosition(x, y + 1);
            Console.WriteLine(" │     │");
            Console.SetCursorPosition(x, y + 2);
            Console.WriteLine(" │     │");
            Console.SetCursorPosition(x, y + 3);
            switch (Value)
            {
                case Value.Skip:
                    Console.WriteLine(" │  X  │");
                    break;
                case Value.Reverse:
                    Console.WriteLine(" │ <-> │");
                    break;
                case Value.DrawTwo:
                    Console.WriteLine(" │ +2  │");
                    break;
                case Value.Wild:
                    Console.WriteLine(" │ WILD│");
                    break;
                case Value.DrawFour:
                    Console.WriteLine(" │WILD4│");
                    break;
                case Value.DrawPile:
                    Console.WriteLine(" │ DRAW│");
                    break;
                default:
                    Console.WriteLine(" │  {0}  │", ((int)Value).ToString());
                    break;
            }
            Console.SetCursorPosition(x, y + 4);
            Console.WriteLine(" │     │");
            Console.SetCursorPosition(x, y + 5);
            Console.WriteLine(" └─────┘");
            Console.ResetColor();
        }
    }
}
