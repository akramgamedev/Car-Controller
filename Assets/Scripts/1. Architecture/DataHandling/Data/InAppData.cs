using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class InAppData
{
   public bool isRemoveAds;
   public bool isUnlockLevels;
   public bool isRestored;

    public InAppData()
    {
        SetRemoveAds(false);
        SetUnlockLevels(false);
        SetRestored(false);
    }
    public void SetRemoveAds(bool value)
    {
        isRemoveAds = value;
    }
    public bool GetRemoveAds()
    {
        return isRemoveAds;
    }
    public void SetUnlockLevels(bool value)
    {
        isUnlockLevels = value;
    }
    public bool GetUnlockLevels()
    {
        return isUnlockLevels;
    }
    public void SetRestored(bool value)
    {
        isRestored = value;
    }
    public bool GetRestored()
    {
        return isRestored;
    }
}
