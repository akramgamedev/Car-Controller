using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalEnums
{
    [System.Serializable]
    public enum GameState
    {
        MainMenu, //Reset
        GamePlay, //GameStart
        GameLoose,
        GameWin,
        Pause,
        Resume,
    }
    [System.Serializable]
    public enum AudioSfx
    {
        GameWin,
        GameLoose,
        CoinCollect,
        BonusCollect,
        GemCollect,
        ButtonClick,
        Selection,
        Hit,
        Achievement

    }
    [System.Serializable]
    public enum CurrencyType
    {
        Coin,
        Gem
    }
}
