using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CountryData
{
    [Header("Country Info")]
    public string countryName;
    public GameObject countryContainer;     // Parent GameObject containing flag + bars for this country
    public Image flagImage;                 // The flag image
    
    [Header("Level Bars")]
    public List<Image> levelBars = new List<Image>();  // All bars for levels in THIS country
    
    [HideInInspector] public int totalLevels => levelBars.Count;
}

public class CountryLevelIndicator : MonoBehaviour
{
    [Header("All Countries (In Order)")]
    [SerializeField] private List<CountryData> countries = new List<CountryData>();
    
    [Header("Bar Colors")]
    [SerializeField] private Color lockedBarColor = new Color(0.5f, 0.5f, 0.5f, 1f);      
    [SerializeField] private Color completedBarColor = new Color(0.2f, 0.8f, 0.2f, 1f);   
    [SerializeField] private Color currentBarColor = new Color(1f, 0.9f, 0.2f, 1f);       
    
    [Header("Flag States")]
    [SerializeField] private Color completedFlagColor = new Color(1f, 1f, 1f, 0.6f);      
    [SerializeField] private Color currentFlagColor = new Color(1f, 1f, 1f, 1f);          
    [SerializeField] private Color upcomingFlagColor = new Color(1f, 1f, 1f, 0.4f);       
    [SerializeField] private Color lockedFlagColor = new Color(1f, 1f, 1f, 0.2f);         
    
    [Header("Bar Scales")]
    [SerializeField] private Vector3 normalBarScale = new Vector3(1f, 1f, 1f);
    [SerializeField] private Vector3 currentBarScale = new Vector3(1.2f, 1.2f, 1f);
    
    [Header("Animation")]
    [SerializeField] private bool useAnimation = true;
    [SerializeField] private float animationSpeed = 8f;
    [SerializeField] private float colorTransitionSpeed = 5f;
    
    [Header("Visibility Settings")]
    [SerializeField] private int showCountriesAhead = 1;  // How many upcoming countries to show
    
    private int currentCountryIndex = 0;
    private int currentLevelInCountry = 0;
    private int totalLevels = 0;
    
    private Dictionary<Transform, Vector3> targetScales = new Dictionary<Transform, Vector3>();
    private Dictionary<Image, Color> targetColors = new Dictionary<Image, Color>();

    private void Start()
    {
        ValidateSetup();
        CalculateTotalLevels();
        InitializeTargets();
        
        if (LevelManager.Instance != null)
        {
            UpdateIndicator(LevelManager.Instance.GetCurrentLevelIndex());
        }
        else
        {
            UpdateIndicator(0);
        }
    }

    private void Update()
    {
        if (useAnimation)
        {
            AnimateScales();
            AnimateColors();
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
        StartCoroutine(DelayedUpdate());
    }

    private System.Collections.IEnumerator DelayedUpdate()
    {
        yield return new WaitForSeconds(0.1f);
        if (LevelManager.Instance != null)
        {
            UpdateIndicator(LevelManager.Instance.GetCurrentLevelIndex());
        }
    }

    private void InitializeTargets()
    {
        foreach (var country in countries)
        {
            // Initialize bar targets
            foreach (var bar in country.levelBars)
            {
                if (bar != null)
                {
                    targetScales[bar.transform] = bar.transform.localScale;
                    targetColors[bar] = bar.color;
                }
            }
            
            // Initialize flag targets
            if (country.flagImage != null)
            {
                targetColors[country.flagImage] = country.flagImage.color;
            }
        }
    }

    private void CalculateTotalLevels()
    {
        totalLevels = 0;
        foreach (var country in countries)
        {
            totalLevels += country.totalLevels;
        }
        
        Debug.Log($"[Country Indicator] Total levels: {totalLevels}");
        
        for (int i = 0; i < countries.Count; i++)
        {
            Debug.Log($"[Country Indicator] {countries[i].countryName}: {countries[i].totalLevels} levels");
        }
    }

    private void ValidateSetup()
    {
        if (countries.Count == 0)
        {
            Debug.LogError("[Country Indicator] No countries assigned!");
            return;
        }
        
        for (int i = 0; i < countries.Count; i++)
        {
            if (countries[i].levelBars.Count == 0)
            {
                Debug.LogWarning($"[Country Indicator] '{countries[i].countryName}' has no bars!");
            }
            
            if (countries[i].countryContainer == null)
            {
                Debug.LogWarning($"[Country Indicator] '{countries[i].countryName}' has no container!");
            }
        }
    }

    public void UpdateIndicator(int globalLevelIndex)
    {
        if (countries.Count == 0) return;

        GetCountryAndLevel(globalLevelIndex, out int countryIndex, out int levelInCountry);
        
        currentCountryIndex = countryIndex;
        currentLevelInCountry = levelInCountry;

        UpdateAllCountries(countryIndex, levelInCountry);

        Debug.Log($"[Country Indicator] {countries[currentCountryIndex].countryName} | " +
                  $"Level {currentLevelInCountry + 1}/{countries[currentCountryIndex].totalLevels} | " +
                  $"Global: {globalLevelIndex + 1}/{totalLevels}");
    }

    private void UpdateAllCountries(int currentCountryIdx, int currentLevelInCountry)
    {
        for (int i = 0; i < countries.Count; i++)
        {
            CountryData country = countries[i];
            
            // Determine visibility
            bool shouldShow = ShouldShowCountry(i, currentCountryIdx);
            if (country.countryContainer != null)
            {
                country.countryContainer.SetActive(shouldShow);
            }
            
            if (!shouldShow) continue;

            // Update based on state
            if (i < currentCountryIdx)
            {
                // COMPLETED COUNTRY
                UpdateCountryBars(country, -1, true);  // All completed
                UpdateFlag(country.flagImage, completedFlagColor);
            }
            else if (i == currentCountryIdx)
            {
                // CURRENT COUNTRY
                UpdateCountryBars(country, currentLevelInCountry, false);
                UpdateFlag(country.flagImage, currentFlagColor);
            }
            else if (i == currentCountryIdx + 1)
            {
                // NEXT UPCOMING COUNTRY
                UpdateCountryBars(country, -2, false);  // All locked
                UpdateFlag(country.flagImage, upcomingFlagColor);
            }
            else
            {
                // FAR FUTURE COUNTRIES
                UpdateCountryBars(country, -2, false);  // All locked
                UpdateFlag(country.flagImage, lockedFlagColor);
            }
        }
    }

    private bool ShouldShowCountry(int countryIndex, int currentCountryIndex)
    {
        // Show completed countries
        if (countryIndex < currentCountryIndex) return true;
        
        // Show current country
        if (countryIndex == currentCountryIndex) return true;
        
        // Show X countries ahead
        if (countryIndex <= currentCountryIndex + showCountriesAhead) return true;
        
        // Hide far future countries
        return false;
    }

    private void UpdateCountryBars(CountryData country, int currentLevel, bool allCompleted)
    {
        for (int i = 0; i < country.levelBars.Count; i++)
        {
            Image bar = country.levelBars[i];
            if (bar == null) continue;

            if (allCompleted || i < currentLevel)
            {
                // Completed - green, normal
                SetBarState(bar, completedBarColor, normalBarScale);
            }
            else if (i == currentLevel)
            {
                // Current - yellow, bigger
                SetBarState(bar, currentBarColor, currentBarScale);
            }
            else
            {
                // Locked - grey, normal
                SetBarState(bar, lockedBarColor, normalBarScale);
            }
        }
    }

    private void UpdateFlag(Image flagImage, Color targetColor)
    {
        if (flagImage == null) return;
        
        targetColors[flagImage] = targetColor;
        
        if (!useAnimation)
        {
            flagImage.color = targetColor;
        }
    }

    private void SetBarState(Image bar, Color color, Vector3 scale)
    {
        if (bar == null) return;

        // Set color
        targetColors[bar] = color;
        if (!useAnimation)
        {
            bar.color = color;
        }

        // Set scale
        targetScales[bar.transform] = scale;
        if (!useAnimation)
        {
            bar.transform.localScale = scale;
        }
    }

    private void AnimateScales()
    {
        foreach (var kvp in targetScales)
        {
            if (kvp.Key != null)
            {
                kvp.Key.localScale = Vector3.Lerp(
                    kvp.Key.localScale,
                    kvp.Value,
                    Time.deltaTime * animationSpeed
                );
            }
        }
    }

    private void AnimateColors()
    {
        foreach (var kvp in targetColors)
        {
            if (kvp.Key != null)
            {
                kvp.Key.color = Color.Lerp(
                    kvp.Key.color,
                    kvp.Value,
                    Time.deltaTime * colorTransitionSpeed
                );
            }
        }
    }

    private void GetCountryAndLevel(int globalLevelIndex, out int countryIndex, out int levelInCountry)
    {
        int levelsAccumulated = 0;
        countryIndex = 0;
        levelInCountry = 0;

        for (int i = 0; i < countries.Count; i++)
        {
            if (globalLevelIndex < levelsAccumulated + countries[i].totalLevels)
            {
                countryIndex = i;
                levelInCountry = globalLevelIndex - levelsAccumulated;
                return;
            }
            levelsAccumulated += countries[i].totalLevels;
        }

        countryIndex = countries.Count - 1;
        levelInCountry = countries[countryIndex].totalLevels - 1;
    }

    public int GetCurrentCountryIndex() => currentCountryIndex;
    public int GetCurrentLevelInCountry() => currentLevelInCountry;
    public int GetTotalLevels() => totalLevels;
    public string GetCurrentCountryName()
    {
        if (currentCountryIndex >= 0 && currentCountryIndex < countries.Count)
            return countries[currentCountryIndex].countryName;
        return "Unknown";
    }

#if UNITY_EDITOR
    [ContextMenu("Test - Level 0")]
    private void TestLevel0() { UpdateIndicator(0); }

    [ContextMenu("Test - Level 5")]
    private void TestLevel5() { UpdateIndicator(5); }

    [ContextMenu("Test - Level 10")]
    private void TestLevel10() { UpdateIndicator(10); }
#endif
}



// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// [System.Serializable]
// public class CountryInfo
// {
//     public string countryName;
//     public GameObject flagObject;           // The circular flag image
//     public int totalLevels;                 // How many levels in this country (5, 6, etc.)
// }

// [System.Serializable]
// public class LevelBar
// {
//     public GameObject barObject;            // The bar indicator GameObject
//     public Image barImage;                  // The bar's Image component
// }

// public class CountryLevelIndicator : MonoBehaviour
// {
//     [Header("Country Flags")]
//     [SerializeField] private List<CountryInfo> countries = new List<CountryInfo>();
    
//     [Header("Level Bar Indicators")]
//     [SerializeField] private List<LevelBar> levelBars = new List<LevelBar>();
    
//     [Header("Bar Colors")]
//     [SerializeField] private Color greyBarColor = new Color(0.5f, 0.5f, 0.5f, 1f);      // For upcoming/locked levels
//     [SerializeField] private Color greenBarColor = new Color(0.2f, 0.8f, 0.2f, 1f);     // For completed levels
//     [SerializeField] private Color yellowBarColor = new Color(1f, 0.9f, 0.2f, 1f);      // For current level
    
//     [Header("Bar Scales")]
//     [SerializeField] private Vector3 normalBarScale = new Vector3(1f, 1f, 1f);
//     [SerializeField] private Vector3 currentBarScale = new Vector3(1.2f, 1.2f, 1f); // Slightly bigger
    
//     [Header("Animation")]
//     [SerializeField] private bool useAnimation = true;
//     [SerializeField] private float animationSpeed = 8f;
//     [SerializeField] private float colorTransitionSpeed = 5f;
    
//     private int currentCountryIndex = 0;
//     private int currentLevelInCountry = 0;
//     private int totalLevels = 0;
//     private Dictionary<GameObject, Vector3> targetScales = new Dictionary<GameObject, Vector3>();
//     private Dictionary<Image, Color> targetColors = new Dictionary<Image, Color>();

//     private void Start()
//     {
//         ValidateSetup();
//         CalculateTotalLevels();
        
//         // Initialize scales and colors
//         foreach (var bar in levelBars)
//         {
//             if (bar.barObject != null)
//             {
//                 targetScales[bar.barObject] = bar.barObject.transform.localScale;
//             }
//             if (bar.barImage != null)
//             {
//                 targetColors[bar.barImage] = bar.barImage.color;
//             }
//         }
        
//         if (LevelManager.Instance != null)
//         {
//             UpdateIndicator(LevelManager.Instance.GetCurrentLevelIndex());
//         }
//         else
//         {
//             UpdateIndicator(0);
//         }
//     }

//     private void Update()
//     {
//         if (useAnimation)
//         {
//             AnimateScales();
//             AnimateColors();
//         }
//     }

//     private void OnEnable()
//     {
//         StaticEvents.GameEvents.OnGameWin += OnLevelComplete;
//     }

//     private void OnDisable()
//     {
//         StaticEvents.GameEvents.OnGameWin -= OnLevelComplete;
//     }

//     private void OnLevelComplete()
//     {
//         StartCoroutine(DelayedUpdate());
//     }

//     private System.Collections.IEnumerator DelayedUpdate()
//     {
//         yield return new WaitForSeconds(0.1f);
//         if (LevelManager.Instance != null)
//         {
//             UpdateIndicator(LevelManager.Instance.GetCurrentLevelIndex());
//         }
//     }

//     private void CalculateTotalLevels()
//     {
//         totalLevels = 0;
//         foreach (var country in countries)
//         {
//             totalLevels += country.totalLevels;
//         }
        
//         Debug.Log($"Total levels across all countries: {totalLevels}");
//     }

//     private void ValidateSetup()
//     {
//         if (countries.Count == 0)
//         {
//             Debug.LogError("No countries assigned!");
//             return;
//         }
        
//         if (levelBars.Count == 0)
//         {
//             Debug.LogError("No level bars assigned!");
//             return;
//         }
//     }

//     public void UpdateIndicator(int globalLevelIndex)
//     {
//         if (countries.Count == 0 || levelBars.Count == 0) return;

//         // Find which country and level we're on
//         int countryIndex, levelInCountry;
//         GetCountryAndLevel(globalLevelIndex, out countryIndex, out levelInCountry);
        
//         currentCountryIndex = countryIndex;
//         currentLevelInCountry = levelInCountry;

//         // Update flags visibility/state
//         UpdateFlags(countryIndex);
        
//         // Update bars based on global progress
//         UpdateBars(globalLevelIndex);

//         Debug.Log($"Country: {countries[currentCountryIndex].countryName} | " +
//                   $"Level {currentLevelInCountry + 1}/{countries[currentCountryIndex].totalLevels} | " +
//                   $"Global Level: {globalLevelIndex + 1}/{totalLevels}");
//     }

//     private void UpdateFlags(int currentCountryIndex)
//     {
//         for (int i = 0; i < countries.Count; i++)
//         {
//             CountryInfo country = countries[i];
            
//             if (country.flagObject == null) continue;

//             if (i < currentCountryIndex)
//             {
//                 // Previous countries - could dim or mark as completed
//                 country.flagObject.SetActive(true);
//             }
//             else if (i == currentCountryIndex)
//             {
//                 // Current country - highlight/show
//                 country.flagObject.SetActive(true);
//             }
//             else if (i == currentCountryIndex + 1)
//             {
//                 // Next upcoming country - show
//                 country.flagObject.SetActive(true);
//             }
//             else
//             {
//                 // Far future countries - you can hide or keep visible
//                 country.flagObject.SetActive(true);
//             }
//         }
//     }

//     private void UpdateBars(int globalLevelIndex)
//     {
//         // Each bar represents one level across all countries
//         for (int i = 0; i < levelBars.Count; i++)
//         {
//             LevelBar bar = levelBars[i];
            
//             if (bar.barObject == null) continue;

//             if (i < globalLevelIndex)
//             {
//                 // COMPLETED LEVEL - Green color, normal size
//                 SetBarState(bar, greenBarColor, normalBarScale);
//             }
//             else if (i == globalLevelIndex)
//             {
//                 // CURRENT LEVEL - Yellow color, slightly bigger
//                 SetBarState(bar, yellowBarColor, currentBarScale);
//             }
//             else
//             {
//                 // UPCOMING LEVEL - Grey color, normal size
//                 SetBarState(bar, greyBarColor, normalBarScale);
//             }
            
//             // Show only relevant bars (optional - hide far future bars)
//             bar.barObject.SetActive(true);
//         }
//     }

//     private void SetBarState(LevelBar bar, Color color, Vector3 scale)
//     {
//         // Set color target
//         if (bar.barImage != null)
//         {
//             targetColors[bar.barImage] = color;
            
//             if (!useAnimation)
//             {
//                 bar.barImage.color = color;
//             }
//         }

//         // Set scale target
//         if (bar.barObject != null)
//         {
//             targetScales[bar.barObject] = scale;
            
//             if (!useAnimation)
//             {
//                 bar.barObject.transform.localScale = scale;
//             }
//         }
//     }

//     private void AnimateScales()
//     {
//         foreach (var kvp in targetScales)
//         {
//             if (kvp.Key != null)
//             {
//                 kvp.Key.transform.localScale = Vector3.Lerp(
//                     kvp.Key.transform.localScale,
//                     kvp.Value,
//                     Time.deltaTime * animationSpeed
//                 );
//             }
//         }
//     }

//     private void AnimateColors()
//     {
//         foreach (var kvp in targetColors)
//         {
//             if (kvp.Key != null)
//             {
//                 kvp.Key.color = Color.Lerp(
//                     kvp.Key.color,
//                     kvp.Value,
//                     Time.deltaTime * colorTransitionSpeed
//                 );
//             }
//         }
//     }

//     private void GetCountryAndLevel(int globalLevelIndex, out int countryIndex, out int levelInCountry)
//     {
//         int levelsAccumulated = 0;
//         countryIndex = 0;
//         levelInCountry = 0;

//         for (int i = 0; i < countries.Count; i++)
//         {
//             if (globalLevelIndex < levelsAccumulated + countries[i].totalLevels)
//             {
//                 countryIndex = i;
//                 levelInCountry = globalLevelIndex - levelsAccumulated;
//                 return;
//             }
//             levelsAccumulated += countries[i].totalLevels;
//         }

//         // Exceeded total - stay at last country, last level
//         countryIndex = countries.Count - 1;
//         levelInCountry = countries[countryIndex].totalLevels - 1;
//     }

//     // Public getters
//     public int GetCurrentCountryIndex() => currentCountryIndex;
//     public int GetCurrentLevelInCountry() => currentLevelInCountry;
//     public string GetCurrentCountryName()
//     {
//         if (currentCountryIndex >= 0 && currentCountryIndex < countries.Count)
//             return countries[currentCountryIndex].countryName;
//         return "Unknown";
//     }

// #if UNITY_EDITOR
//     [ContextMenu("Test - Level 0")]
//     private void TestLevel0() { UpdateIndicator(0); }

//     [ContextMenu("Test - Level 3")]
//     private void TestLevel3() { UpdateIndicator(3); }

//     [ContextMenu("Test - Level 6")]
//     private void TestLevel6() { UpdateIndicator(6); }

//     [ContextMenu("Test - Level 10")]
//     private void TestLevel10() { UpdateIndicator(10); }
// #endif
// }