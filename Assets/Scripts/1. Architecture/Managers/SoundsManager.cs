using System;
using UnityEngine;

public class SoundsManager : MonoBehaviour
{
    [SerializeField] AudioSource sfxAudioSource = null;
    [SerializeField] AudioSource bgmAudioSource = null;

    [SerializeField] AudioClip backGroundSound = null;

    [SerializeField] AudioClip levelWinSound = null;
    [SerializeField] AudioClip levelFailSound = null;
    [SerializeField] AudioClip[] coinCollectSounds = null;
    [SerializeField] AudioClip[] bonusCollectSounds = null;
    [SerializeField] AudioClip gemCollectSound = null;
    [SerializeField] AudioClip buttonClickSound = null;
    [SerializeField] AudioClip selectionSound = null;
    [SerializeField] AudioClip hitSound = null;
    [SerializeField] AudioClip achivementSound = null;

    bool isSoundOn = true;
    public static SoundsManager instance = null;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }
    void OnEnable()
    {
        
    }
    void OnDisable()
    {
        
    }
    public void PlaySound(GlobalEnums.AudioSfx audioSfx)
    {
        if (sfxAudioSource == null || sfxAudioSource.mute)
            return;

        switch (audioSfx)
        {
            case GlobalEnums.AudioSfx.GameWin:
                PlaySoundClip(levelWinSound);
                break;
            case GlobalEnums.AudioSfx.GameLoose:
                PlaySoundClip(levelFailSound);
                break;
            case GlobalEnums.AudioSfx.CoinCollect:
                PlayRandomSound(coinCollectSounds);
                break;
            case GlobalEnums.AudioSfx.BonusCollect:
                PlayRandomSound(bonusCollectSounds);
                break;
            case GlobalEnums.AudioSfx.ButtonClick:
                PlaySoundClip(buttonClickSound);
                break;
            case GlobalEnums.AudioSfx.GemCollect:
                PlaySoundClip(gemCollectSound);
                break;
            case GlobalEnums.AudioSfx.Hit:
                PlaySoundClip(hitSound);
                break;
            case GlobalEnums.AudioSfx.Achievement:
                PlaySoundClip(achivementSound);
                break;
            default:
                {
                    PlaySound(GlobalEnums.AudioSfx.ButtonClick);
                    break;
                }
        }
    }
    void PlaySoundClip(AudioClip clip)
    {
        if (!isSoundOn) return;
        if (clip != null)
        {
            sfxAudioSource.clip = clip;
            sfxAudioSource.volume = 1f;
            sfxAudioSource.loop = false;
            sfxAudioSource.Play();
        }
    }
    void PlayRandomSound(AudioClip[] clips)
    {
        if (clips != null && clips.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, clips.Length);
            PlaySoundClip(clips[randomIndex]);
        }
    }
    public void MusicSetting(bool isOn)
    {
        if (isOn)
        {
            bgmAudioSource.mute = false;
        }
        else
        {
            bgmAudioSource.mute = true;
        }
    }
    public void SoundSetting(bool isOn)
    {
        isSoundOn = isOn;
    }
}
