using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager_Temporary : MonoBehaviour
{
    public static UIManager_Temporary Instance;

    [Header("UI Elements")]
    [SerializeField] private GameObject resetPanel;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (resetPanel != null)
            resetPanel.SetActive(false);
    }

    public void ShowResetPanelDelayed(float delay)
    {
        Invoke(nameof(ShowResetPanel), delay);
    }

    public void ShowResetPanel()
    {
        if (resetPanel != null)
            resetPanel.SetActive(true);
    }

    public void OnResetButtonPressed()
    {
        SceneManager.LoadScene(0);
    }
}

