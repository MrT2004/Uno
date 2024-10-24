namespace Uno_Server.Game
{
    public class Deck
    {
        public List<Card> Cards { get; } = new List<Card>();
        // pop first card
        public Card DrawCard()
        {
            Card card = Cards[0];
            if (card.Value == Value.Wild || card.Value == Value.DrawFour)
            {
                card.Color = Color.Wild;
            }
            Cards.RemoveAt(0);
            return card;
        }
        public void Shuffle()
        {
            Random rnd = new Random();
            for (int i = 0; i < Cards.Count; i++)
            {
                int r = i + rnd.Next(Cards.Count - i);
                (Cards[i], Cards[r]) = (Cards[r], Cards[i]);
            }
        }
        //insert card in sorted order
        public void InsertCard(Card card)
        {
            int index = Cards.BinarySearch(card, new CardComparer());
            if (index < 0)
            {
                index = ~index;
            }
            Cards.Insert(index, card);
        }
        //add card to end of deck
        public void Add(Card card)
        {
            Cards.Add(card);
        }
        //add card to top of deck
        public void Push(Card card)
        {
            Cards.Insert(0, card);
        }
        public void Sort()
        {
            Cards.Sort(new CardComparer());
        }
    }
    internal class CardComparer : IComparer<Card>
    {
        public int Compare(Card? x, Card? y)
        {
            if (x == null || y == null)
            {
                throw new ArgumentException("Arguments cannot be null");
            }

            if (x.Color != y.Color)
            {
                return x.Color.CompareTo(y.Color);
            }
            else
            {
                return x.Value.CompareTo(y.Value);
            }
        }
    }
}
