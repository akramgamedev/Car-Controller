using UnityEngine;

public class LogHelper : MonoBehaviour
{
    [Header("Logging Settings")]
    [Tooltip("Enable or disable all logs globally.")]
    public bool enableLogs = true;

    private static LogHelper instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void Log(string message)
    {
        if (instance != null && instance.enableLogs)
            Debug.Log(message);
    }

    public static void LogError(string message)
    {
        if (instance != null && instance.enableLogs)
            Debug.LogError(message);
    }

    public static void LogWarning(string message)
    {
        if (instance != null && instance.enableLogs)
            Debug.LogWarning(message);
    }
    // Add different Categories of Log here 
    // Say ,
    //  ads log
    //  Game Logic Log
}
