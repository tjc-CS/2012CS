using System;

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
