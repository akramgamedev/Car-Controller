using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEconomyManager : MonoBehaviour
{
    [SerializeField] DataManager dataManager;
    #region subscription
    private void OnEnable()
    {
        StaticEvents.GameEconomy.OnCurrencyChange += ChangeCurrency;
        StaticEvents.GameEconomy.OnGetCurrency += GetCurrency;
    }
    private void OnDisable()
    {
        StaticEvents.GameEconomy.OnCurrencyChange -= ChangeCurrency;
        StaticEvents.GameEconomy.OnGetCurrency -= GetCurrency;
    }
    #endregion

    void ChangeCurrency(int amount, GlobalEnums.CurrencyType type)
    {
        switch (type)
        {
            case GlobalEnums.CurrencyType.Coin:
                {
                    int Coins = dataManager.gameData.economy.GetCash() + amount;
                    if (Coins < 0)
                        Coins = 0;
                    dataManager.gameData.economy.SetCash(Coins);
                    LogHelper.Log($"[GameEconomy] Coins updated to :{Coins}");

                    UIManager.Instance?.UpdateCoinsUI(Coins);
                    break;
                }
            case GlobalEnums.CurrencyType.Key:
                {
                    int Keys = dataManager.gameData.economy.GetKeys() + amount;
                    if (Keys < 0)
                        Keys = 0;
                    dataManager.gameData.economy.SetKeys(Keys);

                    UIManager.Instance?.UpdateCoinsUI(Keys);
                    break;
                }
            default:
                {
                    int Coins = dataManager.gameData.economy.GetCash() + amount;
                    if (Coins < 0)
                        Coins = 0;
                    dataManager.gameData.economy.SetCash(Coins);

                    UIManager.Instance?.UpdateCoinsUI(Coins);
                    break;
                }
        }
    }
    int GetCurrency(GlobalEnums.CurrencyType type)
    {
        int value;
        switch (type)
        {
            case GlobalEnums.CurrencyType.Coin:
                {
                    value = dataManager.gameData.economy.GetCash();
                    break;
                }
            case GlobalEnums.CurrencyType.Key:
                {
                    value = dataManager.gameData.economy.GetKeys();
                    break;
                }
            default:
                {
                    value = dataManager.gameData.economy.GetCash();
                    break;
                }
        }
        return value;
    }
}
