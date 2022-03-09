using System;

public class Breakthrough
{
    private static Random RNoGen = new Random();
    private CardCollection Deck;
    private CardCollection Hand;
    private CardCollection Sequence;
    private CardCollection Discard;
    private List<Lock> Locks = new List<Lock>();
    private int Score;
    private bool GameOver;
    private Lock CurrentLock;
    private bool LockSolved;

    public Breakthrough()
    {
        Deck = new CardCollection("DECK");
        Hand = new CardCollection("HAND");
        Sequence = new CardCollection("SEQUENCE");
        Discard = new CardCollection("DISCARD");
        Score = 0;
        LoadLocks();
    }

    public void PlayGame()
    {
        string MenuChoice;
        if (Locks.Count > 0)
        {
            GameOver = false;
            CurrentLock = new Lock();
            SetupGame();
            while (!GameOver)
            {
                LockSolved = false;
                while (!LockSolved && !GameOver)
                {
                    Console.WriteLine();
                    Console.WriteLine("Current score: " + Score);
                    Console.WriteLine(CurrentLock.GetLockDetails());
                    Console.WriteLine(Sequence.GetCardDisplay());
                    Console.WriteLine(Hand.GetCardDisplay());
                    MenuChoice = GetChoice();
                    switch (MenuChoice)
                    {
                        case "D":
                            {
                                Console.WriteLine(Discard.GetCardDisplay());
                                break;
                            }
                        case "U":
                            {
                                int CardChoice = GetCardChoice();
                                string DiscardOrPlay = GetDiscardOrPlayChoice();
                                if (DiscardOrPlay == "D")
                                {
                                    MoveCard(Hand, Discard, Hand.GetCardNumberAt(CardChoice - 1));
                                    GetCardFromDeck(CardChoice);
                                }
                                else if (DiscardOrPlay == "P")
                                    PlayCardToSequence(CardChoice);
                                break;
                            }
                    }
                    if (CurrentLock.GetLockSolved())
                    {
                        LockSolved = true;
                        ProcessLockSolved();
                    }
                }
                GameOver = CheckIfPlayerHasLost();
            }
        }
        else
            Console.WriteLine("No locks in file.");
    }

    private void ProcessLockSolved()
    {
        Score += 10;
        Console.WriteLine("Lock has been solved.  Your score is now: " + Score);
        while (Discard.GetNumberOfCards() > 0)
        {
            MoveCard(Discard, Deck, Discard.GetCardNumberAt(0));
        }
        Deck.Shuffle();
        CurrentLock = GetRandomLock();
    }

    private bool CheckIfPlayerHasLost()
    {
        if (Deck.GetNumberOfCards() == 0)
        {
            Console.WriteLine("You have run out of cards in your deck.  Your final score is: " + Score);
            return true;
        }
        else
        {
            return false;
        }
    }

    private void SetupGame()
    {
        string Choice;
        Console.Write("Enter L to load a game from a file, anything else to play a new game:> ");
        Choice = Console.ReadLine().ToUpper();
        if (Choice == "L")
        {
            if (!LoadGame("game1.txt"))
            {
                GameOver = true;
            }
        }
        else
        {
            CreateStandardDeck();
            Deck.Shuffle();
            for (int Count = 1; Count <= 5; Count++)
            {
                MoveCard(Deck, Hand, Deck.GetCardNumberAt(0));
            }
            AddDifficultyCardsToDeck();
            Deck.Shuffle();
            CurrentLock = GetRandomLock();
        }
    }

    private void PlayCardToSequence(int cardChoice)
    {
        if (Sequence.GetNumberOfCards() > 0)
        {
            if (Hand.GetCardDescriptionAt(cardChoice - 1)[0] != Sequence.GetCardDescriptionAt(Sequence.GetNumberOfCards() - 1)[0])
            {
                Score += MoveCard(Hand, Sequence, Hand.GetCardNumberAt(cardChoice - 1));
                GetCardFromDeck(cardChoice);
            }
        }
        else
        {
            Score += MoveCard(Hand, Sequence, Hand.GetCardNumberAt(cardChoice - 1));
            GetCardFromDeck(cardChoice);
        }
        if (CheckIfLockChallengeMet())
        {
            Console.WriteLine();
            Console.WriteLine("A challenge on the lock has been met.");
            Console.WriteLine();
            Score += 5;
        }
    }

    private bool CheckIfLockChallengeMet()
    {
        string SequenceAsString = "";
        for (int Count = Sequence.GetNumberOfCards() - 1; Count >= Math.Max(0, Sequence.GetNumberOfCards() - 3); Count--)
        {
            if (SequenceAsString.Length > 0)
            {
                SequenceAsString = ", " + SequenceAsString;
            }
            SequenceAsString = Sequence.GetCardDescriptionAt(Count) + SequenceAsString;
            if (CurrentLock.CheckIfConditionMet(SequenceAsString))
            {
                return true;
            }
        }
        return false;
    }

    private void SetupCardCollectionFromGameFile(string lineFromFile, CardCollection cardCol)
    {
        List<string> SplitLine;
        int CardNumber;
        if (lineFromFile.Length > 0)
        {
            SplitLine = lineFromFile.Split(',').ToList();
            foreach (var Item in SplitLine)
            {
                if (Item.Length == 5)
                {
                    CardNumber = Convert.ToInt32(Item[4]);
                }
                else
                {
                    CardNumber = Convert.ToInt32(Item.Substring(4, 2));
                }
                if (Item.Substring(0, 3) == "Dif")
                {
                    DifficultyCard CurrentCard = new DifficultyCard(CardNumber);
                    cardCol.AddCard(CurrentCard);
                }
                else
                {
                    ToolCard CurrentCard = new ToolCard(Item[0].ToString(), Item[2].ToString(), CardNumber);
                    cardCol.AddCard(CurrentCard);
                }
            }
        }
    }

    private void SetupLock(string line1, string line2)
    {
        List<string> SplitLine;
        SplitLine = line1.Split(';').ToList();
        foreach (var Item in SplitLine)
        {
            List<string> Conditions;
            Conditions = Item.Split(',').ToList();
            CurrentLock.AddChallenge(Conditions);
        }
        SplitLine = line2.Split(';').ToList();
        for (int Count = 0; Count < SplitLine.Count; Count++)
        {
            if (SplitLine[Count] == "Y")
            {
                CurrentLock.SetChallengeMet(Count, true);
            }
        }
    }

    private bool LoadGame(string fileName)
    {
        string LineFromFile;
        string LineFromFile2;
        try
        {
            using (StreamReader MyStream = new StreamReader(fileName))
            {
                LineFromFile = MyStream.ReadLine();
                Score = Convert.ToInt32(LineFromFile);
                LineFromFile = MyStream.ReadLine();
                LineFromFile2 = MyStream.ReadLine();
                SetupLock(LineFromFile, LineFromFile2);
                LineFromFile = MyStream.ReadLine();
                SetupCardCollectionFromGameFile(LineFromFile, Hand);
                LineFromFile = MyStream.ReadLine();
                SetupCardCollectionFromGameFile(LineFromFile, Sequence);
                LineFromFile = MyStream.ReadLine();
                SetupCardCollectionFromGameFile(LineFromFile, Discard);
                LineFromFile = MyStream.ReadLine();
                SetupCardCollectionFromGameFile(LineFromFile, Deck);
            }
            return true;
        }
        catch
        {
            Console.WriteLine("File not loaded");
            return false;
        }
    }

    private void LoadLocks()
    {
        string FileName = "locks.txt";
        string LineFromFile;
        List<string> Challenges;
        Locks = new List<Lock>();
        try
        {
            using (StreamReader MyStream = new StreamReader(FileName))
            {
                LineFromFile = MyStream.ReadLine();
                while (LineFromFile != null)
                {
                    Challenges = LineFromFile.Split(';').ToList();
                    Lock LockFromFile = new Lock();
                    foreach (var C in Challenges)
                    {
                        List<string> Conditions = new List<string>();
                        Conditions = C.Split(',').ToList();
                        LockFromFile.AddChallenge(Conditions);
                    }
                    Locks.Add(LockFromFile);
                    LineFromFile = MyStream.ReadLine();
                }
            }
        }
        catch
        {
            Console.WriteLine("File not loaded");
        }
    }

    private Lock GetRandomLock()
    {
        return Locks[RNoGen.Next(0, Locks.Count)];
    }

    private void GetCardFromDeck(int cardChoice)
    {
        if (Deck.GetNumberOfCards() > 0)
        {
            if (Deck.GetCardDescriptionAt(0) == "Dif")
            {
                Card CurrentCard = Deck.RemoveCard(Deck.GetCardNumberAt(0));
                Console.WriteLine();
                Console.WriteLine("Difficulty encountered!");
                Console.WriteLine(Hand.GetCardDisplay());
                Console.Write("To deal with this you need to either lose a key ");
                Console.Write("(enter 1-5 to specify position of key) or (D)iscard five cards from the deck:> ");
                string Choice = Console.ReadLine();
                Console.WriteLine();
                Discard.AddCard(CurrentCard);
                CurrentCard.Process(Deck, Discard, Hand, Sequence, CurrentLock, Choice, cardChoice);
            }
        }
        while (Hand.GetNumberOfCards() < 5 && Deck.GetNumberOfCards() > 0)
        {
            if (Deck.GetCardDescriptionAt(0) == "Dif")
            {
                MoveCard(Deck, Discard, Deck.GetCardNumberAt(0));
                Console.WriteLine("A difficulty card was discarded from the deck when refilling the hand.");
            }
            else
            {
                MoveCard(Deck, Hand, Deck.GetCardNumberAt(0));
            }
        }
        if (Deck.GetNumberOfCards() == 0 && Hand.GetNumberOfCards() < 5)
        {
            GameOver = true;
        }
    }

    private int GetCardChoice()
    {
        string Choice;
        int Value;
        do
        {
            Console.Write("Enter a number between 1 and 5 to specify card to use:> ");
            Choice = Console.ReadLine();
        }
        while (!int.TryParse(Choice, out Value));
        return Value;
    }

    private string GetDiscardOrPlayChoice()
    {
        string Choice;
        Console.Write("(D)iscard or (P)lay?:> ");
        Choice = Console.ReadLine().ToUpper();
        return Choice;
    }

    private string GetChoice()
    {
        Console.WriteLine();
        Console.Write("(D)iscard inspect, (U)se card:> ");
        string Choice = Console.ReadLine().ToUpper();
        return Choice;
    }

    private void AddDifficultyCardsToDeck()
    {
        for (int Count = 1; Count <= 5; Count++)
        {
            Deck.AddCard(new DifficultyCard());
        }
    }

    private void CreateStandardDeck()
    {
        Card NewCard;
        for (int Count = 1; Count <= 5; Count++)
        {
            NewCard = new ToolCard("P", "a");
            Deck.AddCard(NewCard);
            NewCard = new ToolCard("P", "b");
            Deck.AddCard(NewCard);
            NewCard = new ToolCard("P", "c");
            Deck.AddCard(NewCard);
        }
        for (int Count = 1; Count <= 3; Count++)
        {
            NewCard = new ToolCard("F", "a");
            Deck.AddCard(NewCard);
            NewCard = new ToolCard("F", "b");
            Deck.AddCard(NewCard);
            NewCard = new ToolCard("F", "c");
            Deck.AddCard(NewCard);
            NewCard = new ToolCard("K", "a");
            Deck.AddCard(NewCard);
            NewCard = new ToolCard("K", "b");
            Deck.AddCard(NewCard);
            NewCard = new ToolCard("K", "c");
            Deck.AddCard(NewCard);
        }
    }

    private int MoveCard(CardCollection fromCollection, CardCollection toCollection, int cardNumber)
    {
        int Score = 0;
        if (fromCollection.GetName() == "HAND" && toCollection.GetName() == "SEQUENCE")
        {
            Card CardToMove = fromCollection.RemoveCard(cardNumber);
            if (CardToMove != null)
            {
                toCollection.AddCard(CardToMove);
                Score = CardToMove.GetScore();
            }
        }
        else
        {
            Card CardToMove = fromCollection.RemoveCard(cardNumber);
            if (CardToMove != null)
            {
                toCollection.AddCard(CardToMove);
            }
        }
        return Score;
    }
}
