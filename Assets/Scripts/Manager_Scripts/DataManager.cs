// ==========================================
// DataManager.cs - Save/Load System
// ==========================================
using UnityEngine;
using System.IO;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    private GameData gameData;
    private string saveFilePath;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        saveFilePath = Path.Combine(Application.persistentDataPath, "gamedata.json");
        LoadData();
    }

    public GameData GetGameData()
    {
        return gameData;
    }

    public void LoadData()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                gameData = JsonUtility.FromJson<GameData>(json);
                LogHelper.Log("Game data loaded successfully");
            }
            catch (System.Exception e)
            {
                LogHelper.LogError($"Failed to load game data: {e.Message}");
                gameData = new GameData();
            }
        }
        else
        {
            LogHelper.Log("No save file found. Creating new game data.");
            gameData = new GameData();
            SaveData();
        }
    }

    public void SaveData()
    {
        try
        {
            string json = JsonUtility.ToJson(gameData, true);
            File.WriteAllText(saveFilePath, json);
            LogHelper.Log("Game data saved successfully");
        }
        catch (System.Exception e)
        {
            LogHelper.LogError($"Failed to save game data: {e.Message}");
        }
    }

    // Coins
    public int GetCoins()
    {
        return gameData.coins;
    }

    public void SetCoins(int amount)
    {
        gameData.coins = amount;
        SaveData();
    }

    // Keys
    public int GetKeys()
    {
        return gameData.keys;
    }

    public void SetKeys(int amount)
    {
        gameData.keys = amount;
        SaveData();
    }

    // High Score
    public int GetHighScore()
    {
        return gameData.highScore;
    }

    public void SetHighScore(int score)
    {
        if (score > gameData.highScore)
        {
            gameData.highScore = score;
            SaveData();
        }
    }

    // Selected Car
    public int GetSelectedCarIndex()
    {
        return gameData.selectedCarIndex;
    }

    public void SetSelectedCarIndex(int index)
    {
        gameData.selectedCarIndex = index;
        SaveData();
    }

    // Car Unlocks
    public bool IsCarUnlocked(int carIndex)
    {
        return gameData.unlockedCarIndices.Contains(carIndex);
    }

    public void UnlockCar(int carIndex)
    {
        if (!gameData.unlockedCarIndices.Contains(carIndex))
        {
            gameData.unlockedCarIndices.Add(carIndex);
            SaveData();
            LogHelper.Log($"Car {carIndex} unlocked!");
        }
    }

    // Reset all data
    public void ResetAllData()
    {
        gameData = new GameData();
        SaveData();
        LogHelper.Log("All game data reset");
    }
}