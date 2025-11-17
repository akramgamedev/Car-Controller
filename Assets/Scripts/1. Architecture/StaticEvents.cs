using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class StaticEvents
{
    public static class GameEvents
    {
        public static Action OnMainMenu;
        public static Action OnGameplay;
        public static Action OnGameWin;
        public static Action OnGameLoose;
        public static Action OnGamePause; 
        public static Action OnGameResume; 
        // public static Action<int> OnLevelTaskComplete;
    }
    public static class GameEconomy
    {
        public static Action<int,GlobalEnums.CurrencyType> OnCurrencyChange;
    }
    public static class Loading
    {
        public static Action<float> OnShowLoading;
        public static Action OnHideLoading;

    }    
}