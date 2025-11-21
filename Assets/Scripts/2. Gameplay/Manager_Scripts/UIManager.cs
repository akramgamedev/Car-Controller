using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelSuccessScreen;
    [SerializeField] private GameObject levelFailScreen;
    [SerializeField] private GameObject hud;
    [SerializeField] private GameObject cashBar;
    [SerializeField] private GameObject loadingScreenPanel;
    [SerializeField] private GameObject settingsScreen;
    [SerializeField] private GameObject selectionScreen;
    [SerializeField] private GameObject VIPScreen;

    [Header("Selected Car")]
    [SerializeField] private GameObject selectedCar;

    [Header("Car Stage")]
    [SerializeField] private GameObject stage;

    [Header("Camera References")]
    [SerializeField] private Camera worldCanvasCam;
    [SerializeField] private Camera mainCamera;

    [Header("Loading Screen")]
    [SerializeField] private Image loadingBarFill;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private float loadingDuration = 5f;

    [Header("Level Fail Settings")]
    [SerializeField] private float failScreenDelay = 1.5f;

    [Header("Currency UI")]
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI KeysText;

    private bool isGameStarted = false;
    public bool IsGameStarted => isGameStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnEnable()
    {
        StaticEvents.GameEconomy.OnCurrencyChange += OnCurrencyChanged;
    }

    void OnDisable()
    {
        StaticEvents.GameEconomy.OnCurrencyChange -= OnCurrencyChanged;
    }

    private void Start()
    {
        StartCoroutine(ShowLoadingScreen());
        UpdateCoinsUI(StaticEvents.GameEconomy.OnGetCurrency(GlobalEnums.CurrencyType.Coin));
        UpdateKeysUI(StaticEvents.GameEconomy.OnGetCurrency(GlobalEnums.CurrencyType.Key));
    }

    private void OnCurrencyChanged(int amount, GlobalEnums.CurrencyType type)
    {
        if (type == GlobalEnums.CurrencyType.Coin)
        {
            UpdateCoinsUI(StaticEvents.GameEconomy.OnGetCurrency(GlobalEnums.CurrencyType.Coin));
        }
        else if (type == GlobalEnums.CurrencyType.Key)
        {
            UpdateKeysUI(StaticEvents.GameEconomy.OnGetCurrency(GlobalEnums.CurrencyType.Key));
        }
    }

    private IEnumerator ShowLoadingScreen()
    {
        loadingScreenPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        hud.SetActive(false);
        levelSuccessScreen.SetActive(false);
        levelFailScreen.SetActive(false);
        settingsScreen.SetActive(false);
        selectionScreen.SetActive(false);
        VIPScreen.SetActive(false);
        stage.SetActive(false);
        selectedCar.SetActive(false);

        UpdateCameraState();

        float progress = 0f;
        float dotTimer = 0f;
        int dotCount = 0;

        while (progress < 1f)
        {
            progress += Time.deltaTime / loadingDuration;
            dotTimer += Time.deltaTime;

            if (loadingBarFill)
                loadingBarFill.fillAmount = progress;

            if (dotTimer >= 0.5f)
            {
                dotTimer = 0f;
                dotCount = (dotCount + 1) % 4;

                if (loadingText)
                    loadingText.text = "Loading" + new string('.', dotCount);
            }

            yield return null;
        }

        if (loadingBarFill) loadingBarFill.fillAmount = 1f;
        if (loadingText) loadingText.text = "Loading...";

        yield return new WaitForSeconds(0.5f);

        loadingScreenPanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        UpdateCameraState();
    }

    public void HideMainMenu()
    {
        if (mainMenuPanel == null) return;

        CanvasGroup cg = mainMenuPanel.GetComponent<CanvasGroup>();
        RectTransform rect = mainMenuPanel.GetComponent<RectTransform>();

        if (cg == null)
            cg = mainMenuPanel.AddComponent<CanvasGroup>();

        rect.DOKill();
        rect.localScale = Vector3.one;
        rect.anchoredPosition3D = Vector3.zero;
        cg.alpha = 1;

        Sequence seq = DOTween.Sequence();
        seq.Append(rect.DOScale(0.7f, 0.6f).SetEase(Ease.InOutBack));
        seq.Join(rect.DOLocalMoveZ(-600f, 0.6f).SetEase(Ease.InOutCubic));
        seq.Join(cg.DOFade(0, 0.65f));

        seq.OnComplete(() => { mainMenuPanel.SetActive(false); });

        hud.SetActive(true);
        UpdateCameraState();
    }

    public void ShowLevelSuccess()
    {
        UpdateCoinsUI(StaticEvents.GameEconomy.OnGetCurrency(GlobalEnums.CurrencyType.Coin));
        UpdateKeysUI(StaticEvents.GameEconomy.OnGetCurrency(GlobalEnums.CurrencyType.Key));
        //hud.SetActive(false);
        levelSuccessScreen.SetActive(true);
        UpdateCameraState();
    }
    public void HideLevelSuccessScreen()
    {
        levelSuccessScreen.SetActive(false);
        UpdateCameraState();
    }

    public void ShowLevelFail()
    {
        levelFailScreen.SetActive(true);
        UpdateCameraState();
    }

    public void ShowLevelFailDelay(float delay)
    {
        Invoke(nameof(ShowLevelFail), delay);
    }

    public void ShowSettingsScreen()
    {
        settingsScreen.SetActive(true);
        UpdateCameraState();
    }

    public void HideSettingScreen()
    {
        settingsScreen.SetActive(false);
        UpdateCameraState();
    }

    public void ShowSelectionScreen()
    {
        selectionScreen.SetActive(true);
        stage.SetActive(true);
        selectedCar.SetActive(true);
        mainMenuPanel.SetActive(false);

        UpdateCameraState();
    }

    public void HideSelectionScreen()
    {
        selectionScreen.SetActive(false);
        stage.SetActive(false);
        selectedCar.SetActive(false);
        mainMenuPanel.SetActive(true);

        UpdateCameraState();
    }

    public void ShowVIPScreen()
    {
        VIPScreen.SetActive(true);
        mainMenuPanel.SetActive(false);
        cashBar.SetActive(false);

        UpdateCameraState();
    }

    public void HideVIPScreen()
    {
        VIPScreen.SetActive(false);
        mainMenuPanel.SetActive(true);
        cashBar.SetActive(true);

        UpdateCameraState();

    }
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Optional
    }

    public void OnRestartButtonPressed()
    {
        RestartLevel();
    }

    private void UpdateCameraState()
    {
        if (worldCanvasCam == null || mainCamera == null) return;

        bool shouldEnableCamera =
        (selectionScreen != null && selectionScreen.activeSelf) ||
            (VIPScreen != null && VIPScreen.activeSelf) ||
            (stage != null && stage.activeSelf) ||
            (selectedCar != null && selectedCar.activeSelf);

        worldCanvasCam.gameObject.SetActive(shouldEnableCamera);
        mainCamera.gameObject.SetActive(!shouldEnableCamera);
    }

    public void UpdateCoinsUI(int amount)
    {
        if (coinsText != null)
            coinsText.text = amount.ToString();
    }

    public void UpdateKeysUI(int amount)
    {
        if (KeysText != null)
            KeysText.text = amount.ToString();
    }
}