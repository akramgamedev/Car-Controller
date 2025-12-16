using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CloseCallDevBoard : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CloseCallSystem closeCallSystem;

    [Header("UI References")]
    [SerializeField] private GameObject devBoardPanel;
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;

    [Header("Close Call Sliders")]
    [SerializeField] private Slider proSlider;
    [SerializeField] private Slider greatSlider;
    [SerializeField] private Slider whoahSlider;
    [SerializeField] private Slider dangerSlider;

    [Header("Traffic Control Sliders")]
    [SerializeField] private Slider slowDistanceSlider;
    [SerializeField] private Slider stopDistanceSlider;
    [SerializeField] private Slider resumeDistanceSlider;
    [SerializeField] private Slider slowSpeedSlider;

    [Header("Value Text Labels")]
    [SerializeField] private TMP_Text proValueText;
    [SerializeField] private TMP_Text greatValueText;
    [SerializeField] private TMP_Text whoahValueText;
    [SerializeField] private TMP_Text dangerValueText;
    [SerializeField] private TMP_Text slowDistanceValueText;
    [SerializeField] private TMP_Text stopDistanceValueText;
    [SerializeField] private TMP_Text resumeDistanceValueText;
    [SerializeField] private TMP_Text slowSpeedValueText;

    private void Start()
    {
        if (closeCallSystem == null)
        {
            closeCallSystem = FindObjectOfType<CloseCallSystem>();
        }

        // if (devBoardPanel != null)
        // {
        //     devBoardPanel.SetActive(false);
        // }

        SetupSliders();
        LoadCurrentValues();
    }

    private void Update()
    {
        // if (Input.GetKeyDown(toggleKey))
        // {
        //     ToggleDevBoard();
        // }
    }

    private void SetupSliders()
    {
        // Close Call Sliders
        SetupSlider(proSlider, 1f, 30f, 5.58f, OnProChanged);
        SetupSlider(greatSlider, 1f, 30f, 4.9f, OnGreatChanged);
        SetupSlider(whoahSlider, 0.5f, 30f, 4.04f, OnWhoahChanged);
        SetupSlider(dangerSlider, 0.5f, 30f, 3.49f, OnDangerChanged);

        // Traffic Control Sliders
        SetupSlider(slowDistanceSlider, 2f, 30f, 8f, OnSlowDistanceChanged);
        SetupSlider(stopDistanceSlider, 1f, 30f, 5f, OnStopDistanceChanged);
        SetupSlider(resumeDistanceSlider, 5f, 30f, 6.2f, OnResumeDistanceChanged);
        SetupSlider(slowSpeedSlider, 0.5f, 30f, 3f, OnSlowSpeedChanged);
    }

    private void SetupSlider(Slider slider, float min, float max, float defaultValue, UnityEngine.Events.UnityAction<float> callback)
    {
        if (slider == null) return;

        slider.minValue = min;
        slider.maxValue = max;
        slider.value = defaultValue;
        slider.onValueChanged.AddListener(callback);
    }

    // private void LoadCurrentValues()
    // {
    //     if (closeCallSystem == null) return;

    //     // Use reflection to get current values
    //     var type = closeCallSystem.GetType();
    //     var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

    //     if (proSlider != null)
    //         proSlider.value = (float)type.GetField("proDistance", flags)?.GetValue(closeCallSystem);
    //     if (greatSlider != null)
    //         greatSlider.value = (float)type.GetField("greatDistance", flags)?.GetValue(closeCallSystem);
    //     if (whoahSlider != null)
    //         whoahSlider.value = (float)type.GetField("whoahDistance", flags)?.GetValue(closeCallSystem);
    //     if (dangerSlider != null)
    //         dangerSlider.value = (float)type.GetField("dangerDistance", flags)?.GetValue(closeCallSystem);

    //     if (slowDistanceSlider != null)
    //         slowDistanceSlider.value = (float)type.GetField("trafficSlowDistance", flags)?.GetValue(closeCallSystem);
    //     if (stopDistanceSlider != null)
    //         stopDistanceSlider.value = (float)type.GetField("trafficStopDistance", flags)?.GetValue(closeCallSystem);
    //     if (resumeDistanceSlider != null)
    //         resumeDistanceSlider.value = (float)type.GetField("trafficResumeDistance", flags)?.GetValue(closeCallSystem);
    //     if (slowSpeedSlider != null)
    //         slowSpeedSlider.value = (float)type.GetField("slowSpeed", flags)?.GetValue(closeCallSystem);
    // }

    private void LoadCurrentValues()
    {
        if (closeCallSystem == null) return;

        // Use reflection to get current values
        var type = closeCallSystem.GetType();
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        if (proSlider != null)
        {
            float value = (float)type.GetField("proDistance", flags)?.GetValue(closeCallSystem);
            proSlider.value = value;
            if (proValueText != null) proValueText.text = value.ToString("F2");
        }
        if (greatSlider != null)
        {
            float value = (float)type.GetField("greatDistance", flags)?.GetValue(closeCallSystem);
            greatSlider.value = value;
            if (greatValueText != null) greatValueText.text = value.ToString("F2");
        }
        if (whoahSlider != null)
        {
            float value = (float)type.GetField("whoahDistance", flags)?.GetValue(closeCallSystem);
            whoahSlider.value = value;
            if (whoahValueText != null) whoahValueText.text = value.ToString("F2");
        }
        if (dangerSlider != null)
        {
            float value = (float)type.GetField("dangerDistance", flags)?.GetValue(closeCallSystem);
            dangerSlider.value = value;
            if (dangerValueText != null) dangerValueText.text = value.ToString("F2");
        }

        if (slowDistanceSlider != null)
        {
            float value = (float)type.GetField("trafficSlowDistance", flags)?.GetValue(closeCallSystem);
            slowDistanceSlider.value = value;
            if (slowDistanceValueText != null) slowDistanceValueText.text = value.ToString("F2");
        }
        if (stopDistanceSlider != null)
        {
            float value = (float)type.GetField("trafficStopDistance", flags)?.GetValue(closeCallSystem);
            stopDistanceSlider.value = value;
            if (stopDistanceValueText != null) stopDistanceValueText.text = value.ToString("F2");
        }
        if (resumeDistanceSlider != null)
        {
            float value = (float)type.GetField("trafficResumeDistance", flags)?.GetValue(closeCallSystem);
            resumeDistanceSlider.value = value;
            if (resumeDistanceValueText != null) resumeDistanceValueText.text = value.ToString("F2");
        }
        if (slowSpeedSlider != null)
        {
            float value = (float)type.GetField("slowSpeed", flags)?.GetValue(closeCallSystem);
            slowSpeedSlider.value = value;
            if (slowSpeedValueText != null) slowSpeedValueText.text = value.ToString("F2");
        }
    }

    private void ToggleDevBoard()
    {
        if (devBoardPanel != null)
        {
            devBoardPanel.SetActive(!devBoardPanel.activeSelf);
        }
    }

    // Close Call Callbacks
    private void OnProChanged(float value)
    {
        UpdateValue("proDistance", value, proValueText);
    }

    private void OnGreatChanged(float value)
    {
        UpdateValue("greatDistance", value, greatValueText);
    }

    private void OnWhoahChanged(float value)
    {
        UpdateValue("whoahDistance", value, whoahValueText);
    }

    private void OnDangerChanged(float value)
    {
        UpdateValue("dangerDistance", value, dangerValueText);
    }

    // Traffic Control Callbacks
    private void OnSlowDistanceChanged(float value)
    {
        UpdateValue("trafficSlowDistance", value, slowDistanceValueText);
    }

    private void OnStopDistanceChanged(float value)
    {
        UpdateValue("trafficStopDistance", value, stopDistanceValueText);
    }

    private void OnResumeDistanceChanged(float value)
    {
        UpdateValue("trafficResumeDistance", value, resumeDistanceValueText);
    }

    private void OnSlowSpeedChanged(float value)
    {
        UpdateValue("slowSpeed", value, slowSpeedValueText);
    }

    private void UpdateValue(string fieldName, float value, TMP_Text valueText)
    {
        if (closeCallSystem == null) return;

        // Update the field using reflection
        var type = closeCallSystem.GetType();
        var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(closeCallSystem, value);
        if (valueText != null)
        {
            valueText.text = value.ToString("F2"); // Shows 2 decimal places
        }
    }

    public void ResetToDefaults()
    {
        if (proSlider != null) proSlider.value = 5.58f;
        if (greatSlider != null) greatSlider.value = 4.9f;
        if (whoahSlider != null) whoahSlider.value = 4.04f;
        if (dangerSlider != null) dangerSlider.value = 3.49f;
        if (slowDistanceSlider != null) slowDistanceSlider.value = 8f;
        if (stopDistanceSlider != null) stopDistanceSlider.value = 5f;
        if (resumeDistanceSlider != null) resumeDistanceSlider.value = 6.2f;
        if (slowSpeedSlider != null) slowSpeedSlider.value = 3f;
    }

    public void SaveValues()
    {
        // Save current values to PlayerPrefs
        PlayerPrefs.SetFloat("DevBoard_ProDistance", proSlider.value);
        PlayerPrefs.SetFloat("DevBoard_GreatDistance", greatSlider.value);
        PlayerPrefs.SetFloat("DevBoard_WhoahDistance", whoahSlider.value);
        PlayerPrefs.SetFloat("DevBoard_DangerDistance", dangerSlider.value);
        PlayerPrefs.SetFloat("DevBoard_SlowDistance", slowDistanceSlider.value);
        PlayerPrefs.SetFloat("DevBoard_StopDistance", stopDistanceSlider.value);
        PlayerPrefs.SetFloat("DevBoard_ResumeDistance", resumeDistanceSlider.value);
        PlayerPrefs.SetFloat("DevBoard_SlowSpeed", slowSpeedSlider.value);
        PlayerPrefs.Save();

        LogHelper.Log("Dev Board values saved!");
    }

    public void LoadValues()
    {
        if (PlayerPrefs.HasKey("DevBoard_ProDistance"))
        {
            proSlider.value = PlayerPrefs.GetFloat("DevBoard_ProDistance");
            greatSlider.value = PlayerPrefs.GetFloat("DevBoard_GreatDistance");
            whoahSlider.value = PlayerPrefs.GetFloat("DevBoard_WhoahDistance");
            dangerSlider.value = PlayerPrefs.GetFloat("DevBoard_DangerDistance");
            slowDistanceSlider.value = PlayerPrefs.GetFloat("DevBoard_SlowDistance");
            stopDistanceSlider.value = PlayerPrefs.GetFloat("DevBoard_StopDistance");
            resumeDistanceSlider.value = PlayerPrefs.GetFloat("DevBoard_ResumeDistance");
            slowSpeedSlider.value = PlayerPrefs.GetFloat("DevBoard_SlowSpeed");

            LogHelper.Log("Dev Board values loaded!");
        }
    }
}