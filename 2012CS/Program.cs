// Skeleton Program code for the AQA A Level Paper 1 Summer 2022 examination
//this code should be used in conjunction with the Preliminary Material
//written by the AQA Programmer Team
//developed in the Visual Studio Community Edition programming environment

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Breakthrough
{
    class Program
    {
        static void Main(string[] args)
        {
            Breakthrough ThisGame = new Breakthrough();
            ThisGame.PlayGame();
            Console.ReadLine();
        }
    }

    class Breakthrough
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

    class Challenge
    {
        protected List<string> Condition;
        protected bool Met;

        public Challenge()
        {
            Met = false;
        }

        public bool GetMet()
        {
            return Met;
        }

        public List<string> GetCondition()
        {
            return Condition;
        }

        public void SetMet(bool newValue)
        {
            Met = newValue;
        }

        public void SetCondition(List<string> newCondition)
        {
            Condition = newCondition;
        }
    }

    class Lock
    {
        protected List<Challenge> Challenges = new List<Challenge>();

        public virtual void AddChallenge(List<string> condition)
        {
            Challenge C = new Challenge();
            C.SetCondition(condition);
            Challenges.Add(C);
        }

        private string ConvertConditionToString(List<string> c)
        {
            string ConditionAsString = "";
            for (int Pos = 0; Pos <= c.Count - 2; Pos++)
            {
                ConditionAsString += c[Pos] + ", ";
            }
            ConditionAsString += c[c.Count - 1];
            return ConditionAsString;
        }

        public virtual string GetLockDetails()
        {
            string LockDetails = Environment.NewLine + "CURRENT LOCK" + Environment.NewLine + "------------" + Environment.NewLine;
            foreach (var C in Challenges)
            {
                if (C.GetMet())
                {
                    LockDetails += "Challenge met: ";
                }
                else
                {
                    LockDetails += "Not met:       ";
                }
                LockDetails += ConvertConditionToString(C.GetCondition()) + Environment.NewLine;
            }
            LockDetails += Environment.NewLine;
            return LockDetails;
        }

        public virtual bool GetLockSolved()
        {
            foreach (var C in Challenges)
            {
                if (!C.GetMet())
                {
                    return false;
                }
            }
            return true;
        }

        public virtual bool CheckIfConditionMet(string sequence)
        {
            foreach (var C in Challenges)
            {
                if (!C.GetMet() && sequence == ConvertConditionToString(C.GetCondition()))
                {
                    C.SetMet(true);
                    return true;
                }
            }
            return false;
        }

        public virtual void SetChallengeMet(int pos, bool value)
        {
            Challenges[pos].SetMet(value);
        }

        public virtual bool GetChallengeMet(int pos)
        {
            return Challenges[pos].GetMet();
        }

        public virtual int GetNumberOfChallenges()
        {
            return Challenges.Count;
        }
    }

    class Card
    {
        protected int CardNumber, Score;
        protected static int NextCardNumber = 1;

        public Card()
        {
            CardNumber = NextCardNumber;
            NextCardNumber += 1;
            Score = 0;
        }

        public virtual int GetScore()
        {
            return Score;
        }

        public virtual void Process(CardCollection deck, CardCollection discard,
            CardCollection hand, CardCollection sequence, Lock currentLock,
            string choice, int cardChoice)
        {
        }

        public virtual int GetCardNumber()
        {
            return CardNumber;
        }

        public virtual string GetDescription()
        {
            if (CardNumber < 10)
            {
                return " " + CardNumber.ToString();
            }
            else
            {
                return CardNumber.ToString();
            }
        }
    }

    class ToolCard : Card
    {
        protected string ToolType;
        protected string Kit;

        public ToolCard(string t, string k) : base()
        {
            ToolType = t;
            Kit = k;
            SetScore();
        }

        public ToolCard(string t, string k, int cardNo)
        {
            ToolType = t;
            Kit = k;
            CardNumber = cardNo;
            SetScore();
        }

        private void SetScore()
        {
            switch (ToolType)
            {
                case "K":
                    {
                        Score = 3;
                        break;
                    }
                case "F":
                    {
                        Score = 2;
                        break;
                    }
                case "P":
                    {
                        Score = 1;
                        break;
                    }
            }
        }

        public override string GetDescription()
        {
            return ToolType + " " + Kit;
        }
    }

    class DifficultyCard : Card
    {
        protected string CardType;

        public DifficultyCard()
            : base()
        {
            CardType = "Dif";
        }

        public DifficultyCard(int cardNo)
        {
            CardType = "Dif";
            CardNumber = cardNo;
        }

        public override string GetDescription()
        {
            return CardType;
        }

        public override void Process(CardCollection deck, CardCollection discard, CardCollection hand, CardCollection sequence, Lock currentLock, string choice, int cardChoice)
        {
            int ChoiceAsInteger;
            if (int.TryParse(choice, out ChoiceAsInteger))
            {
                if (ChoiceAsInteger >= 1 && ChoiceAsInteger <= 5)
                {
                    if (ChoiceAsInteger >= cardChoice)
                    {
                        ChoiceAsInteger -= 1;
                    }
                    if (ChoiceAsInteger > 0)
                    {
                        ChoiceAsInteger -= 1;
                    }
                    if (hand.GetCardDescriptionAt(ChoiceAsInteger)[0] == 'K')
                    {
                        Card CardToMove = hand.RemoveCard(hand.GetCardNumberAt(ChoiceAsInteger));
                        discard.AddCard(CardToMove);
                        return;
                    }
                }
            }
            int Count = 0;
            while (Count < 5 && deck.GetNumberOfCards() > 0)
            {
                Card CardToMove = deck.RemoveCard(deck.GetCardNumberAt(0));
                discard.AddCard(CardToMove);
                Count += 1;
            }
        }
    }

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
}