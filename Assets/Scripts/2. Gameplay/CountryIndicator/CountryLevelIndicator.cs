using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CountryData
{
    public string countryName;
    public Sprite flagSprite;
    public int levelCount = 5;
}

public class CountryLevelIndicator : MonoBehaviour
{
    [Header("Global Bar Prefab")]
    [SerializeField] private GameObject barPrefab;

    [Header("All Countries (In Order)")]
    [SerializeField] private List<CountryData> countries = new List<CountryData>();

    [Header("UI References")]
    [SerializeField] private Image currentFlagImage;
    [SerializeField] private Image upcomingFlagImage;
    [SerializeField] private Transform barsParent;

    [Header("Bar Colors")]
    [SerializeField] private Color lockedBarColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private Color completedBarColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    [SerializeField] private Color currentBarColor = new Color(1f, 0.9f, 0.2f, 1f);

    [Header("Bar Scales")]
    [SerializeField] private Vector3 normalBarScale = new Vector3(1f, 1f, 1f);
    [SerializeField] private Vector3 currentBarScale = new Vector3(1.2f, 1.2f, 1f);

    private int currentCountryIndex = 0;
    private int currentLevelInCountry = 0;
    private List<Image> currentBars = new List<Image>();

    private void Start()
    {
        if (currentFlagImage != null)
            currentFlagImage.gameObject.SetActive(true);
        if (upcomingFlagImage != null)
            upcomingFlagImage.gameObject.SetActive(true);

        if (LevelManager.Instance != null)
        {
            UpdateIndicator(LevelManager.Instance.GetCurrentLevelIndex());
        }
        else
        {
            UpdateIndicator(0);
        }
    }

    private void OnEnable()
    {
        StaticEvents.GameEvents.OnGameWin += OnLevelComplete;
    }

    private void OnDisable()
    {
        StaticEvents.GameEvents.OnGameWin -= OnLevelComplete;
    }

    private void OnLevelComplete()
    {
        if (LevelManager.Instance != null)
        {
            UpdateIndicator(LevelManager.Instance.GetCurrentLevelIndex());
        }
    }

    public void UpdateIndicator(int globalLevelIndex)
    {
        if (countries.Count == 0) return;

        GetCountryAndLevel(globalLevelIndex, out int countryIndex, out int levelInCountry);

        currentCountryIndex = countryIndex;
        currentLevelInCountry = levelInCountry;

        UpdateFlags();

        UpdateBars();

        LogHelper.Log($"[Country Indicator] Country: {countries[currentCountryIndex].countryName} | " +
                  $"Level: {currentLevelInCountry + 1}/{countries[currentCountryIndex].levelCount}");
    }

    private void UpdateFlags()
    {
        if (currentFlagImage != null && currentCountryIndex < countries.Count)
        {
            currentFlagImage.sprite = countries[currentCountryIndex].flagSprite;
        }

        if (upcomingFlagImage != null)
        {
            int upcomingIndex = currentCountryIndex + 1;

            if (upcomingIndex < countries.Count)
            {
                upcomingFlagImage.sprite = countries[upcomingIndex].flagSprite;
                upcomingFlagImage.gameObject.SetActive(true);
            }
            else
            {
                upcomingFlagImage.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateBars()
    {
        if (barsParent == null || barPrefab == null) return;

        CountryData currentCountry = countries[currentCountryIndex];

        ClearAllBars();

        for (int i = 0; i < currentCountry.levelCount; i++)
        {
            GameObject barObj = Instantiate(barPrefab, barsParent);
            barObj.name = $"Bar_{i + 1}";

            Image barImage = barObj.GetComponent<Image>();
            if (barImage != null)
            {
                currentBars.Add(barImage);

                if (i < currentLevelInCountry)
                {
                    barImage.color = completedBarColor;
                    barImage.transform.localScale = normalBarScale;
                }
                else if (i == currentLevelInCountry)
                {
                    barImage.color = currentBarColor;
                    barImage.transform.localScale = currentBarScale;
                }
                else
                {
                    barImage.color = lockedBarColor;
                    barImage.transform.localScale = normalBarScale;
                }
            }
        }
    }

    private void ClearAllBars()
    {
        foreach (Transform child in barsParent)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }

        currentBars.Clear();
    }

    private void GetCountryAndLevel(int globalLevelIndex, out int countryIndex, out int levelInCountry)
    {
        int levelsAccumulated = 0;
        countryIndex = 0;
        levelInCountry = 0;

        for (int i = 0; i < countries.Count; i++)
        {
            if (globalLevelIndex < levelsAccumulated + countries[i].levelCount)
            {
                countryIndex = i;
                levelInCountry = globalLevelIndex - levelsAccumulated;
                return;
            }
            levelsAccumulated += countries[i].levelCount;
        }

        countryIndex = countries.Count - 1;
        levelInCountry = countries[countryIndex].levelCount - 1;
    }

    public int GetCurrentCountryIndex() => currentCountryIndex;
    public int GetCurrentLevelInCountry() => currentLevelInCountry;
    public string GetCurrentCountryName() => countries[currentCountryIndex].countryName;
}