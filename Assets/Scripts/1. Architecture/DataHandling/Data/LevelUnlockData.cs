using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class LevelUnlockData
{
    public int unlockedLevelIndex;
    public int levelAttemptRate;
    public bool unlockAllLevels;

    public LevelUnlockData()
    {
        unlockAllLevels = false;
        levelAttemptRate = 1;
        unlockedLevelIndex = 0;
    }

    public void SetUnlockAllLevel()
    {
        unlockAllLevels = true;
    }

    public bool GetUnlockAllLevels()
    {
        return unlockAllLevels;
    }

    public void SetUnlockedLevelIndex(int value)
    {
        unlockedLevelIndex = value;
    }

    public int GetUnlockedLevelIndex()
    {
        return unlockedLevelIndex;
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
