using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CarGridUI : MonoBehaviour
{
    [Header("Page Settings")]
    [SerializeField] private int pageIndex;
    [SerializeField] private GlobalEnums.CarUnlockType unlockType;

    [Header("UI Elements")]
    [SerializeField] private Button unlockButton;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI buttonText;

    [Header("UI Elements - Ads Button")]
    [SerializeField] private Button adsButton;
    [SerializeField] private TextMeshProUGUI adsRewardText;
    [SerializeField] private int adsRewardAmount = 100;

    [Header("Progression")]
    [SerializeField] private Image progressionFillImage;

    [Header("Car Grid Items")]
    [SerializeField] private Image[] carImages;

    void Start()
    {
        SetupButton();
        RefreshUI();
    }

    void OnEnable()
    {
        StaticEvents.CarUnlockEvents.OnCarUnlocked += OnCarUnlocked;
        StaticEvents.GameEvents.OnGameWin += OnLevelComplete;
        StaticEvents.GameEconomy.OnCurrencyChange += OnCurrencyChanged;


        RefreshUI();

    }

    void OnDisable()
    {
        StaticEvents.CarUnlockEvents.OnCarUnlocked -= OnCarUnlocked;
        StaticEvents.GameEvents.OnGameWin -= OnLevelComplete;
        StaticEvents.GameEconomy.OnCurrencyChange -= OnCurrencyChanged;
    }

    void OnCurrencyChanged(int amount, GlobalEnums.CurrencyType type)
    {
        RefreshButtonState();
    }

    void SetupButton()
    {
        if (unlockButton != null)
        {
            unlockButton.onClick.AddListener(OnUnlockButtonPressed);
        }

        if (adsButton != null)
        {
            adsButton.onClick.AddListener(OnAdsButtonPressed);
        }

        UpdatePriceText();
        UpdateAdsRewardText();

        if (unlockType == GlobalEnums.CarUnlockType.ProgressionUnlock)
        {
            if (unlockButton != null)

                unlockButton.gameObject.SetActive(false);


            if (adsButton != null)

                adsButton.gameObject.SetActive(false);

        }
        else if (unlockType == GlobalEnums.CarUnlockType.VIPUnlock)
        {
            if (buttonText != null)
            {
                buttonText.text = "Purchase VIP";
            }

            if (adsButton != null)
            {
                adsButton.gameObject.SetActive(false);
            }
        }

    }

    void UpdatePriceText()
    {
        if (priceText == null) return;

        if (unlockType == GlobalEnums.CarUnlockType.CashUnlock)
        {
            int cost = GetCashCostForPage();
            priceText.text = cost.ToString();
        }
        else if (unlockType == GlobalEnums.CarUnlockType.ChestUnlock)
        {
            int keyCost = 3;
            priceText.text = keyCost.ToString() + "Keys";
        }
        else if (unlockType == GlobalEnums.CarUnlockType.VIPUnlock)
        {
            priceText.text = "";
        }
    }

    void UpdateAdsRewardText()
    {
        if (adsRewardText == null) return;

        adsRewardText.text = $"+${adsRewardAmount}";
    }

    int GetCashCostForPage()
    {
        if (unlockType == GlobalEnums.CarUnlockType.CashUnlock)
        {
            if (pageIndex == 0) return 1000;
            if (pageIndex == 1) return 1000;
            if (pageIndex == 2) return 1500;

        }
        return 0;
    }

    void OnUnlockButtonPressed()
    {
        if (unlockType == GlobalEnums.CarUnlockType.CashUnlock)
        {
            CarUnlockManager.Instance.OnCashUnlockButtonPressed(pageIndex);
        }
        else if (unlockType == GlobalEnums.CarUnlockType.ChestUnlock)
        {
            CarUnlockManager.Instance.OnChestUnlockButtonPressed();
        }
        else if (unlockType == GlobalEnums.CarUnlockType.VIPUnlock)
        {
            CarUnlockManager.Instance.OnVIPPurchased();
        }
    }

    void OnAdsButtonPressed()
    {
        AdsManager.Instance?.ShowInterstitialAd(() =>
        {
            GiveAdsReward();
        });
    }

    void GiveAdsReward()
    {
        StaticEvents.GameEconomy.OnCurrencyChange?.Invoke(adsRewardAmount, GlobalEnums.CurrencyType.Coin);

        LogHelper.Log($"Player earned {adsRewardAmount} coins from ad on page {pageIndex}!");

    }

    void RefreshButtonState()
    {
        if (unlockButton == null) return;

        bool canUnlock = false;

        if (unlockType == GlobalEnums.CarUnlockType.CashUnlock)
        {
            int cost = GetCashCostForPage();
            int currentCoins = StaticEvents.GameEconomy.OnGetCurrency?.Invoke(GlobalEnums.CurrencyType.Coin) ?? 0;
            canUnlock = currentCoins >= cost;

        }
        else if (unlockType == GlobalEnums.CarUnlockType.ChestUnlock)
        {
            int currentKeys = StaticEvents.GameEconomy.OnGetCurrency?.Invoke(GlobalEnums.CurrencyType.Key) ?? 0;
            canUnlock = currentKeys >= 3;
        }
        else if (unlockType == GlobalEnums.CarUnlockType.VIPUnlock)
        {
            canUnlock = true;
        }
        unlockButton.interactable = canUnlock;

        if (adsButton != null)
        {
            adsButton.interactable = true;
        }
    }

    void RefreshUI()
    {
        if (unlockType == GlobalEnums.CarUnlockType.ProgressionUnlock && progressionFillImage != null)
        {
            float fillAmount = CarUnlockManager.Instance.GetProgressionFillAmount();
            progressionFillImage.fillAmount = fillAmount;
        }

        RefreshButtonState();

        RefreshCarGridVisuals();
    }

    void RefreshCarGridVisuals()
    {
        if(CarUnlockManager.Instance == null) return;

        if (carImages == null) return;

        for (int i = 0; i < carImages.Length; i++)
        {
            if (carImages[i] != null)
            {
                bool isUnlocked = CarUnlockManager.Instance.IsCarUnlocked(i);
                carImages[i].color = isUnlocked ? carImages[i].color : Color.gray;
            }
        }

    }
    void OnCarUnlocked(int carIndex, GlobalEnums.CarUnlockType type)
    {
        if (type == unlockType)
        {
            RefreshUI();
        }
    }

    void OnLevelComplete()
    {
        if (unlockType == GlobalEnums.CarUnlockType.ProgressionUnlock)
        {
            RefreshUI();
        }
    }
}
