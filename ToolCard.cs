using System;

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
