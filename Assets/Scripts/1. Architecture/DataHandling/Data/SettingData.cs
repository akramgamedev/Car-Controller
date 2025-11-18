using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SettingData
{
    public bool musicSetting;
    public bool soundSetting;
    public bool vibrationSetting;

    public SettingData()
    {
        musicSetting = true;
        soundSetting = true;
        vibrationSetting = true;
    }

    public void MusicSettingChange(bool value)
    {
        musicSetting = value;
    }

    public bool IsMusicSettingON()
    {
        return musicSetting;
    }

    public void SoundSettingChange(bool value)
    {
        soundSetting = value;
    }

    public bool IsSoundSettingON()
    {
        return soundSetting;
    }

    public void VibrationSettingChange(bool value)
    {
        vibrationSetting = value;
    }

    public bool IsVibrationSettingON()
    {
        return vibrationSetting;
    }
}
