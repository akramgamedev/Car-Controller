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
        public string alertOnString;
        public string alertOffString;
    }

    [SerializeField] DataManager dataManager;
    [SerializeField] VibrationController vibrationController;

    [SerializeField] SettingObject music;
    [SerializeField] SettingObject sound;
    [SerializeField] SettingObject vibration;
    [Header("Alert Animation")]
    [SerializeField] private TextMeshProUGUI alertText;
    [SerializeField] private Transform alertTargetPos;
    [SerializeField] private CanvasGroup alertTextCanvasGroup;
    private Vector3 alertStartPos;
    private int musicValue, soundValue;
    private Tween alertTween;

    private void Start()
    {
        alertText.gameObject.SetActive(false);
        alertStartPos = alertText.transform.position;
        InitializeSettings();
    }
    private void InitializeSettings()
    {
        SetSetting(music, dataManager.gameData.setting.IsMusicSettingON());
        SoundsManager.instance.MusicSetting(dataManager.gameData.setting.IsMusicSettingON());
     
        SetSetting(sound, dataManager.gameData.setting.IsSoundSettingON());
        SoundsManager.instance.SoundSetting(dataManager.gameData.setting.IsSoundSettingON());

        SetSetting(vibration, dataManager.gameData.setting.IsVibrationSettingON());
        vibrationController.VibrationSetting(dataManager.gameData.setting.IsVibrationSettingON());
    }

    private void SetSetting(SettingObject setting, bool value)
    {
        SetImageSprite(setting.image, value ? setting.onSprite : setting.offSprite);
        alertText.text = value ? setting.alertOnString : setting.alertOffString;
        PlayAlertAnimation();
    }
    private void SetImageSprite(Image image, Sprite sprite)
    {
        if (image != null && sprite != null)
        {
            image.sprite = sprite;
        }
    }
    private void PlayAlertAnimation()
    {
        alertText.gameObject.SetActive(false);
        alertTween?.Kill();
        alertTextCanvasGroup.alpha = 1;
        alertText.gameObject.SetActive(true);

        float duration = 1;
        alertText.transform.position = alertStartPos;

        alertTween = alertText.transform.DOMove(alertTargetPos.position, duration).SetUpdate(true).OnComplete(() =>
        {
            alertTextCanvasGroup.DOFade(0, duration * 0.3f).SetUpdate(true).OnComplete(() =>
            {
                alertText.gameObject.SetActive(false);
            });
        });
    }
    public void MusicToggle()
    {
        dataManager.gameData.setting.MusicSettingChange(!dataManager.gameData.setting.IsMusicSettingON());
        SetSetting(music, dataManager.gameData.setting.IsMusicSettingON());
        SoundsManager.instance.MusicSetting(dataManager.gameData.setting.IsMusicSettingON());
    }

    public void SoundToggle()
    {
        dataManager.gameData.setting.SoundSettingChange(!dataManager.gameData.setting.IsSoundSettingON());
        SetSetting(sound, dataManager.gameData.setting.IsSoundSettingON());
        SoundsManager.instance.SoundSetting(dataManager.gameData.setting.IsSoundSettingON());
    }

    public void VibrationToggle()
    {
        dataManager.gameData.setting.VibrationSettingChange(!dataManager.gameData.setting.IsVibrationSettingON());
        SetSetting(vibration, dataManager.gameData.setting.IsVibrationSettingON());
        vibrationController.VibrationSetting(dataManager.gameData.setting.IsVibrationSettingON());
    }
}
