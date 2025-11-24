// ========================================
// 1. SettingsManager.cs - UPDATED TO WORK WITH YOUR AUDIOMANAGER
// ========================================
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class SettingsManager : MonoBehaviour
{
    [Serializable]
    public struct SettingObject
    {
        public Sprite onSprite;
        public Sprite offSprite;
        public Image image;
    }

    [SerializeField] private DataManager dataManager;
    [SerializeField] private VibrationController vibrationController;

    [Header("Setting Toggles")]
    [SerializeField] private SettingObject vibration;

    [Header("Volume Sliders (Optional)")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider effectsVolumeSlider;

    private Vector3 alertStartPos;
    private Tween alertTween;

    private void Start()
    {
        InitializeSettings();
        InitializeSliders();
    }

    private void InitializeSettings()
    {
        // Music Setting
        bool isMusicOn = dataManager.gameData.setting.IsMusicSettingON();
        ApplyMusicSetting(isMusicOn, false); // false = don't show alert on init

        // Sound Setting
        bool isSoundOn = dataManager.gameData.setting.IsSoundSettingON();

        ApplySoundSetting(isSoundOn, false);

        // Vibration Setting
        bool isVibrationOn = dataManager.gameData.setting.IsVibrationSettingON();
        SetSettingVisual(vibration, isVibrationOn);
        if (vibrationController != null)
            vibrationController.VibrationSetting(isVibrationOn);
    }

    private void InitializeSliders()
    {
        // Setup Music Volume Slider
        if (musicVolumeSlider != null)
        {
            float savedMusicVolume = dataManager.gameData.setting.musicVolume;
            musicVolumeSlider.value = savedMusicVolume;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        // Setup Effects Volume Slider
        if (effectsVolumeSlider != null)
        {
            float savedEffectsVolume = dataManager.gameData.setting.effectsVolume;
            effectsVolumeSlider.value = savedEffectsVolume;
            effectsVolumeSlider.onValueChanged.AddListener(OnEffectsVolumeChanged);
        }
    }

    private void SetSettingVisual(SettingObject setting, bool value)
    {
        SetImageSprite(setting.image, value ? setting.onSprite : setting.offSprite);
    }

    private void SetImageSprite(Image image, Sprite sprite)
    {
        if (image != null && sprite != null)
        {
            image.sprite = sprite;
        }
    }
    // ========== PUBLIC METHODS FOR UI BUTTONS ==========

    public void VibrationToggle()
    {
        bool newValue = !dataManager.gameData.setting.IsVibrationSettingON();
        dataManager.gameData.setting.VibrationSettingChange(newValue);
        SetSettingVisual(vibration, newValue);

        if (vibrationController != null)
            vibrationController.VibrationSetting(newValue);


        // Save the data
        dataManager.SaveGameData();

        // Play button click sound
        AudioManager.Instance?.PlayUI("ButtonClick");
    }

    public void OnMusicVolumeChanged(float volume)
    {
        dataManager.gameData.setting.SetMusicVolume(volume);

        if (dataManager.gameData.setting.IsMusicSettingON())
        {
            AudioManager.Instance?.SetMusicVolume(volume);
        }

        // Save the data
        dataManager.SaveGameData();
    }

    public void OnEffectsVolumeChanged(float volume)
    {
        dataManager.gameData.setting.SetEffectsVolume(volume);

        if (dataManager.gameData.setting.IsSoundSettingON())
        {
            AudioManager.Instance?.SetSFXVolume(volume);
            AudioManager.Instance?.SetUIVolume(volume);
        }

        // Save the data
        dataManager.SaveGameData();
    }

    // ========== PRIVATE HELPER METHODS ==========

    private void ApplyMusicSetting(bool isOn, bool showAlert)
    {
        if (AudioManager.Instance == null) return;

        if (isOn)
        {
            // Turn music ON with saved volume
            float savedVolume = dataManager.gameData.setting.musicVolume;
            AudioManager.Instance.SetMusicVolume(savedVolume);

        }
        else
        {
            // Turn music OFF (mute)
            AudioManager.Instance.SetMusicVolume(0f);

        }
    }

    private void ApplySoundSetting(bool isOn, bool showAlert)
    {
        if (AudioManager.Instance == null) return;

        if (isOn)
        {
            // Turn sound ON with saved volume
            float savedVolume = dataManager.gameData.setting.effectsVolume;
            AudioManager.Instance.SetSFXVolume(savedVolume);
            AudioManager.Instance.SetUIVolume(savedVolume);

        }
        else
        {
            // Turn sound OFF (mute)
            AudioManager.Instance.SetSFXVolume(0f);
            AudioManager.Instance.SetUIVolume(0f);

        }
    }
}


// *********** original setting manager script ******************
// using System;
// using UnityEngine;
// using TMPro;
// using UnityEngine.UI;
// using DG.Tweening;

// public class SettingsManager : MonoBehaviour
// {
//     [Serializable]
//     public struct SettingObject
//     {
//         public Sprite onSprite;
//         public Sprite offSprite;
//         public Image image;
//         public string alertOnString;
//         public string alertOffString;
//     }

//     [SerializeField] DataManager dataManager;
//     [SerializeField] VibrationController vibrationController;

//     [SerializeField] SettingObject music;
//     [SerializeField] SettingObject sound;
//     [SerializeField] SettingObject vibration;
//     [Header("Alert Animation")]
//     [SerializeField] private TextMeshProUGUI alertText;
//     [SerializeField] private Transform alertTargetPos;
//     [SerializeField] private CanvasGroup alertTextCanvasGroup;
//     private Vector3 alertStartPos;
//     private int musicValue, soundValue;
//     private Tween alertTween;

//     private void Start()
//     {
//         alertText.gameObject.SetActive(false);
//         alertStartPos = alertText.transform.position;
//         InitializeSettings();
//     }
//     private void InitializeSettings()
//     {
//         SetSetting(music, dataManager.gameData.setting.IsMusicSettingON());
//         SoundsManager.instance.MusicSetting(dataManager.gameData.setting.IsMusicSettingON());

//         SetSetting(sound, dataManager.gameData.setting.IsSoundSettingON());
//         SoundsManager.instance.SoundSetting(dataManager.gameData.setting.IsSoundSettingON());

//         SetSetting(vibration, dataManager.gameData.setting.IsVibrationSettingON());
//         vibrationController.VibrationSetting(dataManager.gameData.setting.IsVibrationSettingON());
//     }

//     private void SetSetting(SettingObject setting, bool value)
//     {
//         SetImageSprite(setting.image, value ? setting.onSprite : setting.offSprite);
//         alertText.text = value ? setting.alertOnString : setting.alertOffString;
//         PlayAlertAnimation();
//     }
//     private void SetImageSprite(Image image, Sprite sprite)
//     {
//         if (image != null && sprite != null)
//         {
//             image.sprite = sprite;
//         }
//     }
//     private void PlayAlertAnimation()
//     {
//         alertText.gameObject.SetActive(false);
//         alertTween?.Kill();
//         alertTextCanvasGroup.alpha = 1;
//         alertText.gameObject.SetActive(true);

//         float duration = 1;
//         alertText.transform.position = alertStartPos;

//         alertTween = alertText.transform.DOMove(alertTargetPos.position, duration).SetUpdate(true).OnComplete(() =>
//         {
//             alertTextCanvasGroup.DOFade(0, duration * 0.3f).SetUpdate(true).OnComplete(() =>
//             {
//                 alertText.gameObject.SetActive(false);
//             });
//         });
//     }
//     public void MusicToggle()
//     {
//         dataManager.gameData.setting.MusicSettingChange(!dataManager.gameData.setting.IsMusicSettingON());
//         SetSetting(music, dataManager.gameData.setting.IsMusicSettingON());
//         SoundsManager.instance.MusicSetting(dataManager.gameData.setting.IsMusicSettingON());
//     }

//     public void SoundToggle()
//     {
//         dataManager.gameData.setting.SoundSettingChange(!dataManager.gameData.setting.IsSoundSettingON());
//         SetSetting(sound, dataManager.gameData.setting.IsSoundSettingON());
//         SoundsManager.instance.SoundSetting(dataManager.gameData.setting.IsSoundSettingON());
//     }

//     public void VibrationToggle()
//     {
//         dataManager.gameData.setting.VibrationSettingChange(!dataManager.gameData.setting.IsVibrationSettingON());
//         SetSetting(vibration, dataManager.gameData.setting.IsVibrationSettingON());
//         vibrationController.VibrationSetting(dataManager.gameData.setting.IsVibrationSettingON());
//     }
// }
