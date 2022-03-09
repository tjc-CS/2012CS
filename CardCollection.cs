using System;

class CardCollection
{
    protected List<Card> Cards = new List<Card>();
    protected string Name;

    public CardCollection(string n)
    {
        Name = n;
    }

    public string GetName()
    {
        return Name;
    }

    public int GetCardNumberAt(int x)
    {
        return Cards[x].GetCardNumber();
    }

    public string GetCardDescriptionAt(int x)
    {
        return Cards[x].GetDescription();
    }

    public void AddCard(Card c)
    {
        Cards.Add(c);
    }

    public int GetNumberOfCards()
    {
        return Cards.Count;
    }

    public void Shuffle()
    {
        Random RNoGen = new Random();
        Card TempCard;
        int RNo1, RNo2;
        for (int Count = 1; Count <= 10000; Count++)
        {
            RNo1 = RNoGen.Next(0, Cards.Count);
            RNo2 = RNoGen.Next(0, Cards.Count);
            TempCard = Cards[RNo1];
            Cards[RNo1] = Cards[RNo2];
            Cards[RNo2] = TempCard;
        }
    }

    public Card RemoveCard(int cardNumber)
    {
        bool CardFound = false;
        int Pos = 0;
        Card CardToGet = null;
        while (Pos < Cards.Count && !CardFound)
        {
            if (Cards[Pos].GetCardNumber() == cardNumber)
            {
                CardToGet = Cards[Pos];
                CardFound = true;
                Cards.RemoveAt(Pos);
            }
            Pos++;
        }
        return CardToGet;
    }

    private string CreateLineOfDashes(int size)
    {
        string LineOfDashes = "";
        for (int Count = 1; Count <= size; Count++)
        {
            LineOfDashes += "------";
        }
        return LineOfDashes;
    }

    public string GetCardDisplay()
    {
        string CardDisplay = Environment.NewLine + Name + ":";
        if (Cards.Count == 0)
        {
            return CardDisplay + " empty" + Environment.NewLine + Environment.NewLine;
        }
        else
        {
            CardDisplay += Environment.NewLine + Environment.NewLine;
        }
        string LineOfDashes;
        const int CardsPerLine = 10;
        if (Cards.Count > CardsPerLine)
        {
            LineOfDashes = CreateLineOfDashes(CardsPerLine);
        }
        else
        {
            LineOfDashes = CreateLineOfDashes(Cards.Count);
        }
        CardDisplay += LineOfDashes + Environment.NewLine;
        bool Complete = false;
        int Pos = 0;
        while (!Complete)
        {
            CardDisplay += "| " + Cards[Pos].GetDescription() + " ";
            Pos++;
            if (Pos % CardsPerLine == 0)
            {
                CardDisplay += "|" + Environment.NewLine + LineOfDashes + Environment.NewLine;
            }
            if (Pos == Cards.Count)
            {
                Complete = true;
            }
        }
        if (Cards.Count % CardsPerLine > 0)
        {
            CardDisplay += "|" + Environment.NewLine;
            if (Cards.Count > CardsPerLine)
            {
                LineOfDashes = CreateLineOfDashes(Cards.Count % CardsPerLine);
            }
            CardDisplay += LineOfDashes + Environment.NewLine;
        }
        return CardDisplay;
    }
}
