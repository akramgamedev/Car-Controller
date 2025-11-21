using System;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    [Header("Ads Settings")]
    [SerializeField] private bool useTestAds = true;
    [SerializeField] private float testAdDuration = 2f;

    private Action onAdClosedCallback;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {


        LogHelper.Log("Ads Initialized!");

    }

    public void ShowInterstitialAd(Action onAdClosed)
    {
        onAdClosedCallback = onAdClosed;

        if (useTestAds)
        {
            LogHelper.Log("Show TEST interstitial ad...");
            Invoke(nameof(OnAdClosed), testAdDuration);
        }
        else
        {


            LogHelper.Log("Show TEST interstitial ad...");
            Invoke(nameof(OnAdClosed), testAdDuration);
        }
    }


    void OnAdClosed()
    {
        LogHelper.Log("Ad closed! Giving reward...");
        onAdClosedCallback?.Invoke();
        onAdClosedCallback = null;
    }
}
