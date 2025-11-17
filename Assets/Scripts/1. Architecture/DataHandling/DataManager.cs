using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DataManager : MonoBehaviour
{
    [SerializeField] public Scriptable_GameValues persistantValues;
    public GameData gameData { get; private set; }
    SaveData saveData = new SaveData();

    private void Awake()
    {
        if (saveData.SaveFileExists())
            LoadGameData();
        else
            SaveWithDefaultValues();
    }

    void SaveWithDefaultValues()
    {
        gameData = new GameData(
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
            Debug.LogError("DataNotSaved");
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
