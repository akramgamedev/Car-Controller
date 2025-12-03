using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    [SerializeField] public Scriptable_GameValues persistantValues;
    public GameData gameData { get; private set; }
    SaveData saveData = new SaveData();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (saveData.SaveFileExists())
            {
                LoadGameData();
            }
            else
            {
                SaveWithDefaultValues();
            }
            LogHelper.Log("DataManager initialized successfully!");
        }
        else
        {
        LogHelper.LogWarning("Duplicate DataManager found! Destroying...");
           Destroy(gameObject);
        }
    }

    void SaveWithDefaultValues()
    {
        gameData = new GameData(
            new CarData(),
            new EconomyData(),
            new LevelUnlockData(),
            new SettingData(),
            new ProfileData(),
            new InAppData()
            );
        saveData.Save(gameData);
    }

    void LoadGameData()
    {
        gameData = saveData.Load<GameData>();
        if (gameData == null)
        {
            LogHelper.LogError("DataNotSaved");
            SaveWithDefaultValues();
            return;
        }
    }

    private void OnApplicationQuit()
    {
        SaveGameData();
    }

    public void SaveGameData()
    {
        saveData.Save(gameData);
    }
}
