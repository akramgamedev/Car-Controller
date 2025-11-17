using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EconomyData
{
    public int cash;
    public int keys;

    public EconomyData()
    {
        SetCash(0);
        SetKeys(0);
    }

    public void SetCash(int value)
    {
        cash = value;
    }

    public int GetCash()
    {
        return cash;
    }

    public void SetKeys(int value)
    {
        keys = value;
    }

    public int GetKeys()
    {
        return keys;
    }
}
