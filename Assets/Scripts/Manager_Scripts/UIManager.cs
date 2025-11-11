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

    [Header("Loading Screen")]
    [SerializeField] private Image loadingBarFill;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private float loadingDuration = 5f;

    [Header("Level Fail Settings")]
    [SerializeField] private float failScreenDelay = 1.5f;

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

    private void Start()
    {
        StartCoroutine(ShowLoadingScreen());
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
    }

    public void ShowLevelSuccess()
    {
        //hud.SetActive(false);
        levelSuccessScreen.SetActive(true);
    }
    public void HideLevelSuccessScreen()
    {
        levelSuccessScreen.SetActive(false);
    }

    public void ShowLevelFail()
    {
        //hud.SetActive(false);
        levelFailScreen.SetActive(true);
    }

    public void ShowLevelFailDelay(float delay)
    {
        Invoke(nameof(ShowLevelFail), delay);
    }

    public void ShowSettingsScreen()
    {
        settingsScreen.SetActive(true);
    }

    public void HideSettingScreen()
    {
        settingsScreen.SetActive(false);
    }

    public void ShowSelectionScreen()
    {
        selectionScreen.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    public void HideSelectionScreen()
    {
        selectionScreen.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void ShowVIPScreen()
    {
        VIPScreen.SetActive(true);
        mainMenuPanel.SetActive(false);
        cashBar.SetActive(false);
    }

    public void HideVIPScreen()
    {
        VIPScreen.SetActive(false);
        mainMenuPanel.SetActive(true);
        cashBar.SetActive(true);

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
}


// using UnityEngine;
// using UnityEngine.UI;
// using System.Collections;
// using System.Collections.Generic;
// using TMPro;
// using UnityEngine.SceneManagement;

// public class UIManager : MonoBehaviour
// {
//     public static UIManager Instance { get; private set; }

//     [Header("UI Panels")]
//     [SerializeField] private GameObject mainMenuPanel;
//     [SerializeField] private GameObject levelSuccessScreen;
//     [SerializeField] private GameObject levelFailScreen;
//     [SerializeField] private GameObject hud;
//     [SerializeField] private GameObject loadingScreenPanel;
//     [SerializeField] private GameObject SettingsScreen;

//     [Header("Loading Screen")]
//     [SerializeField] private Image loadingBarFill;
//     [SerializeField] private TextMeshProUGUI loadingText;
//     [SerializeField] private float loadingDuration = 5f;

//     private Dictionary<string, GameObject> panels;
//     private bool isGameStarted = false;

//     public bool IsGameStarted => isGameStarted;

//     [Header("Level Fail Setting")]
//     [SerializeField] private float failScreenDelay = 1.5f;

//     private void Awake()
//     {
//         // Singleton pattern
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }
//         Instance = this;

//         InitializePanels();
//     }

//     private void Start()
//     {
//         // Start with loading screen
//         StartCoroutine(ShowLoadingScreen());
//     }

//     // private void Update()
//     //{
//     // // Check for screen touch to start game (only when main menu is active)
//     // if (!isGameStarted && mainMenuPanel.activeSelf)
//     // {
//     //     if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
//     //     {
//     //         StartGame();
//     //     }
//     // }
//     // }

//     private void InitializePanels()
//     {
//         panels = new Dictionary<string, GameObject>
//         {
//             { "MainMenu", mainMenuPanel },
//             { "LevelSuccess", levelSuccessScreen },
//             { "LevelFail", levelFailScreen },
//             { "HUD", hud },
//             { "Loading", loadingScreenPanel },
//             {"Settings",SettingsScreen}
//         };

//         // Hide all panels at start
//         foreach (var panel in panels.Values)
//         {
//             if (panel != null)
//             {
//                 panel.SetActive(false);
//             }
//         }
//     }

//     private IEnumerator ShowLoadingScreen()
//     {
//         loadingScreenPanel.SetActive(true);

//         float progress = 0f;
//         float dotTimer = 0f;
//         int dotCount = 0;

//         while (progress < 1f)
//         {
//             progress += Time.deltaTime / loadingDuration;
//             dotTimer += Time.deltaTime;

//             if (loadingBarFill != null)
//                 loadingBarFill.fillAmount = progress;

//             if (dotTimer >= 0.5f)
//             {
//                 dotTimer = 0f;
//                 dotCount = (dotCount + 1) % 4;

//                 if (loadingText != null)
//                     loadingText.text = $"Loading" + new string('.', dotCount);
//             }

//             yield return null;
//         }

//         if (loadingBarFill != null)
//             loadingBarFill.fillAmount = 1f;

//         if (loadingText != null)
//             loadingText.text = "Loading...";

//         yield return new WaitForSeconds(0.5f);

//         // Hide loading and show main menu
//         loadingScreenPanel.SetActive(false);
//         mainMenuPanel.SetActive(true);
//     }

//     // public void StartGame()
//     // {
//     //     isGameStarted = true;
//     //     mainMenuPanel.SetActive(false);
//     //     hud.SetActive(true);

//     //     // Notify other systems that game has started
//     //     OnGameStarted();

//     //     Debug.Log("Game Started!");
//     // }

//     public void ShowPanel(string panelName)
//     {
//         if (panels.ContainsKey(panelName))
//         {
//             panels[panelName].SetActive(true);
//             Debug.Log($"Showing panel: {panelName}");
//         }
//         else
//         {
//             Debug.LogWarning($"Panel '{panelName}' not found in UIManager!");
//         }
//     }

//     public void HidePanel(string panelName)
//     {
//         if (panels.ContainsKey(panelName))
//         {
//             panels[panelName].SetActive(false);
//             Debug.Log($"Hiding panel: {panelName}");
//         }
//         else
//         {
//             Debug.LogWarning($"Panel '{panelName}' not found in UIManager!");
//         }
//     }

//     public void HideAllPanels()
//     {
//         foreach (var panel in panels.Values)
//         {
//             if (panel != null)
//             {
//                 panel.SetActive(false);
//             }
//         }
//     }

//     public bool IsPanelActive(string panelName)
//     {
//         if (panels.ContainsKey(panelName))
//         {
//             return panels[panelName].activeSelf;
//         }
//         return false;
//     }

//     // Level completion methods
//     public void ShowLevelSuccess()
//     {
//         HidePanel("HUD");
//         ShowPanel("LevelSuccess");
//     }

//     public void ShowLevelFailDelay(float delay)
//     {
//         Invoke(nameof(ShowLevelFail), delay);
//     }

//     public void ShowLevelFail()
//     {
//         HidePanel("HUD");
//         ShowPanel("LevelFail");
//     }

//     public void RestartLevel()
//     {
//         // Reset game state
//         isGameStarted = false;

//         // Hide all panels
//         HideAllPanels();

//         // Show loading screen again
//         StartCoroutine(ShowLoadingScreen());
//     }

//     public void ReturnToMainMenu()
//     {
//         // Reset game state
//         isGameStarted = false;

//         // Hide all and show main menu
//         HideAllPanels();
//         mainMenuPanel.SetActive(true);
//     }

//     public void OnRestartButtonPressed()
//     {
//         SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
//     }


//     // Override for game started event - other scripts can subscribe to this
//     private void OnGameStarted()
//     {
//         // You can add additional logic here or use events
//         // For example: enable player input, start spawning, etc.
//     }
// }