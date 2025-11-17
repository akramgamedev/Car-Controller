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
    }
    private void OnDisable()
    {
        StaticEvents.GameEconomy.OnCurrencyChange -= ChangeCurrency;
    }
    #endregion
    
    public void ChangeCurrency(int amount, GlobalEnums.CurrencyType type)
    {
        switch (type)
        {
            case GlobalEnums.CurrencyType.Coin:
                {
                    int Coins = dataManager.gameData.economy.GetCash() + amount;
                    if (Coins < 0)
                        Coins = 0;
                    dataManager.gameData.economy.SetCash(Coins);
                    break;
                }
            default:
                {
                    int Coins = dataManager.gameData.economy.GetCash() + amount;
                    if (Coins < 0)
                        Coins = 0;
                    dataManager.gameData.economy.SetCash(Coins);
                    break;
                }
        }
    }
}
