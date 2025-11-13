using System.Collections.Generic;
using System;

[Serializable]
public class GameData
{
    public int coins = 0;
    public int keys = 0;
    public int highScore = 0;
    public int selectedCarIndex = 0;
    public List<int> unlockedCarIndices = new List<int> { 0 };

    public GameData()
    {
        coins = 0;
        keys = 0;
        highScore = 0;
        selectedCarIndex = 0;
        unlockedCarIndices = new List<int> { 0 };
    }
}
