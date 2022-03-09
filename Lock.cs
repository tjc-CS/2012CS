using System;

public class Lock
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
