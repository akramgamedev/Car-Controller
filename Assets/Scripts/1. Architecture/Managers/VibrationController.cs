using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lofelt.NiceVibrations;
public class VibrationController : MonoBehaviour
{
    [SerializeField] DataManager dataManager;
    bool canVibrate = true;
    private void Start()
    {
        canVibrate = dataManager.gameData.setting.IsVibrationSettingON();
    }
    public void VibrationSetting(bool value)
    {
        canVibrate = value;
    }
    public virtual void ButtonVibration()
    {
        if (!canVibrate) return;
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.Selection);
    }
    public virtual void SuccessVibration()
    {
        if (!canVibrate) return;
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.Success);
    }
    public virtual void LooseVibration()
    {
        if (!canVibrate) return;
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.Failure);
    }
}
