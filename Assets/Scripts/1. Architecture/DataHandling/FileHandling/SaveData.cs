using System.IO;
using UnityEngine;

public class SaveData
{
    private string filePath;

    public SaveData()
    {
        if (Application.isEditor)
        {
            filePath = Path.Combine(Application.dataPath, "pickMeUpGD.json");
        }
        else
        {
            filePath = Path.Combine(Application.persistentDataPath, "pickMeUpGD.json");
        }
    }

    public void Save<T>(T data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
    }

    public T Load<T>()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<T>(json);
        }
        return default(T);
    }

    public bool SaveFileExists()
    {
        return File.Exists(filePath);
    }
}
