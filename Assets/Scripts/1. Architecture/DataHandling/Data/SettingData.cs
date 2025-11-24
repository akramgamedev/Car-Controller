using UnityEngine;

[System.Serializable]
public class SettingData
{
    public bool musicSetting;
    public bool soundSetting;
    public bool vibrationSetting;
    public float musicVolume;
    public float effectsVolume;

    public SettingData()
    {
        musicSetting = true;
        soundSetting = true;
        vibrationSetting = true;
        musicVolume = 0.5f;
        effectsVolume = 0.7f;
    }

    // Music Toggle
    public void MusicSettingChange(bool value)
    {
        musicSetting = value;
    }

    public bool IsMusicSettingON()
    {
        return musicSetting;
    }

    // Sound Toggle
    public void SoundSettingChange(bool value)
    {
        soundSetting = value;
    }

    public bool IsSoundSettingON()
    {
        return soundSetting;
    }

    // Vibration Toggle
    public void VibrationSettingChange(bool value)
    {
        vibrationSetting = value;
    }

    public bool IsVibrationSettingON()
    {
        return vibrationSetting;
    }

    // Volume Controls
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
    }

    public void SetEffectsVolume(float volume)
    {
        effectsVolume = Mathf.Clamp01(volume);
    }
}

//************* original working code ******************
// using System.Collections;
// using System.Collections.Generic;

// [System.Serializable]
// public class SettingData
// {
//     public bool musicSetting;
//     public bool soundSetting;
//     public bool vibrationSetting;

//     public SettingData()
//     {
//         musicSetting = true;
//         soundSetting = true;
//         vibrationSetting = true;
//     }

//     public void MusicSettingChange(bool value)
//     {
//         musicSetting = value;
//     }

//     public bool IsMusicSettingON()
//     {
//         return musicSetting;
//     }

//     public void SoundSettingChange(bool value)
//     {
//         soundSetting = value;
//     }

//     public bool IsSoundSettingON()
//     {
//         return soundSetting;
//     }

//     public void VibrationSettingChange(bool value)
//     {
//         vibrationSetting = value;
//     }

//     public bool IsVibrationSettingON()
//     {
//         return vibrationSetting;
//     }
// }
