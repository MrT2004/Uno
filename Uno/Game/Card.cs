namespace Uno_Server.Game
{
    public class Card
    {
        public Card(Color color, Value value)
        {
            Color = color;
            Value = value;
        }

        public Color Color { get; set; }
        public Value Value { get; set; }

        public bool CanPlayOn(Card other)
        {
            return Color == other.Color || Value == other.Value || Color == Color.Wild;
        }

        public override string ToString()
        {
            return $"{Color} {Value}";
        }
    }
}
