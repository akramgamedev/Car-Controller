using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class LevelUnlockData
{
    public int unlockedLevelNumber;
    public int levelAttemptRate;
    public bool unlockAllLevels;

    public LevelUnlockData()
    {
        unlockAllLevels = false;
        levelAttemptRate = 1;
        unlockedLevelNumber = 1;
    }

    public void SetUnlockAllLevel()
    {
        unlockAllLevels = true;
    }

    public bool GetUnlockAllLevels()
    {
        return unlockAllLevels;
    }

    public void SetUnlockedLevelNumber(int value)
    {
        unlockedLevelNumber = value;
    }

    public int GetUnlockedLevelNumber()
    {
        return unlockedLevelNumber;
    }

    public void SetLevelAttemptRate(int count)
    {
        levelAttemptRate = count;
    }

    public int GetLevelAttemptRate()
    {
        return levelAttemptRate;
    }
}
