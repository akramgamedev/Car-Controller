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
        public static Action OnLevelComplete;
        // public static Action<int> OnLevelTaskComplete;
    }
    public static class GameEconomy
    {
        public static Action<int, GlobalEnums.CurrencyType> OnCurrencyChange;
        public static Func<GlobalEnums.CurrencyType, int> OnGetCurrency;
    }
    public static class Loading
    {
        public static Action<float> OnShowLoading;
        public static Action OnHideLoading;

    }
    public static class CarUnlockEvents
    {
        public static Action<int, GlobalEnums.CarUnlockType> OnCarUnlocked;
        public static Action<int> OnCarSelected;
        public static Action <int> OnCarGridRefreshNeeded;

    }
}