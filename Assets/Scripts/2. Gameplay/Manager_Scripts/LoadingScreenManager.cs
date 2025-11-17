using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class LoadingScreenManager : MonoBehaviour
{
    public GameObject loadingScreenPanel;
    public Image loadingBarFill;
    public TextMeshProUGUI loadingText;
    public float loadingDuration = 5f;

    private void Start()
    {
        StartCoroutine(ShowLoadingScreen());
    }

    private IEnumerator ShowLoadingScreen()
    {
        loadingScreenPanel.SetActive(true);

        float progress = 0f;
        float dotTimer = 0f;
        int dotCount = 0;

        while (progress < 1f)
        {
            progress += Time.deltaTime / loadingDuration;
            dotTimer += Time.deltaTime;

            if (loadingBarFill != null)
                loadingBarFill.fillAmount = progress;

            if (dotTimer >= 0.5f)
            {
                dotTimer = 0f;
                dotCount = (dotCount + 1) % 4;

                if (loadingText != null)
                    loadingText.text = $"Loading" + new string('.', dotCount);
            }

            yield return null;
        }

        if (loadingBarFill != null)
            loadingBarFill.fillAmount = 1f;

        if (loadingText != null)
            loadingText.text = "Loading...";

        yield return new WaitForSeconds(0.5f);

        loadingScreenPanel.SetActive(false);
    }
}