using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    public event Action<int> OnCoinsChanged;
    public event Action<int> OnKeysChanged;

    private int currentCoins;
    private int currentKeys;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        LoadCurrency();
    }

    void LoadCurrency()
    {
        if (DataManager.Instance != null)
        {
            currentCoins = DataManager.Instance.GetCoins();
            currentKeys = DataManager.Instance.GetKeys();

            OnCoinsChanged?.Invoke(currentCoins);
            OnKeysChanged?.Invoke(currentKeys);
        }
    }


    public int GetCoins()
    {
        return currentCoins;
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;

        currentCoins += amount;
        DataManager.Instance?.SetCoins(currentCoins);
        OnCoinsChanged?.Invoke(currentCoins);

        LogHelper.Log($"Added {amount} coins. Total: {currentCoins}");
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0)
        {
            LogHelper.LogWarning("Cannot spend 0 or negative coins");
            return false;
        }

        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            DataManager.Instance?.SetCoins(currentCoins);
            OnCoinsChanged?.Invoke(currentCoins);

            LogHelper.Log($"Spent {amount} coins. Remaining: {currentCoins}");
            return true;
        }
        else
        {
            LogHelper.LogWarning($"Not enough coins! Need {amount}, have {currentCoins}");
            return false;
        }
    }
    public bool HasEnoughCoins(int amount)
    {
        return currentCoins >= amount;
    }

    public int GetKeys()
    {
        return currentKeys;
    }

    public void AddKeys(int amount)
    {
        if (amount <= 0) return;

        currentKeys += amount;
        DataManager.Instance?.SetKeys(currentKeys);
        OnKeysChanged?.Invoke(currentKeys);

        LogHelper.Log($"Added {amount} keys. Total: {currentKeys}");
    }

    public bool SpendKeys(int amount)
    {
        if (amount <= 0)
        {
            LogHelper.LogWarning("Cannot spend 0 or negative keys");
            return false;
        }

        if (currentKeys >= amount)
        {
            currentKeys -= amount;
            DataManager.Instance?.SetKeys(currentKeys);
            OnKeysChanged?.Invoke(currentKeys);

            LogHelper.Log($"Spent {amount} keys. Remaining: {currentKeys}");
            return true;
        }
        else
        {
            LogHelper.LogWarning($"Not enough keys! Need {amount}, have {currentKeys}");
            return false;
        }
    }

    public bool HasEnoughKeys(int amount)
    {
        return currentKeys >= amount;
    }

    // ==================== TESTING ====================
    [ContextMenu("Add 100 Coins")]
    void TestAddCoins()
    {
        AddCoins(100);
    }

    [ContextMenu("Add 10 Keys")]
    void TestAddKeys()
    {
        AddKeys(10);
    }
}
