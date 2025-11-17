using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioSource uiSource;

    [Header("Audio Clips - UI")]
    [SerializeField] private AudioClip buttonClick;
    [SerializeField] private AudioClip messageAlert;
    [SerializeField] private AudioClip messageFlyOut;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip dropOffSound;
    [SerializeField] private AudioClip cashRegister;
    [SerializeField] private AudioClip moneyCounter;

    [Header("Audio Clips - Car")]
    [SerializeField] private AudioClip carCrash;
    [SerializeField] private AudioClip carBeep;
    [SerializeField] private AudioClip carUnlock;
    [SerializeField] private AudioClip spinWheelTick;

    [Header("Audio Clips - Environment")]
    [SerializeField] private AudioClip openDoor;
    [SerializeField] private AudioClip closeDoor;
    [SerializeField] private AudioClip trainHorn;
    [SerializeField] private AudioClip firework;

    [Header("Audio Clips - Misc")]
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip reviveCountdown;
    [SerializeField] private AudioClip videoRewardButton;
    [SerializeField] private AudioClip interactionSound1;
    [SerializeField] private AudioClip bgLoop;

    [Header("Settings")]
    [SerializeField] private float sfxVolume = 0.7f;
    [SerializeField] private float musicVolume = 0.5f;
    [SerializeField] private float uiVolume = 0.8f;

    private Dictionary<string, AudioClip> audioClips;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioClips();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeAudioClips()
    {
        audioClips = new Dictionary<string, AudioClip>
        {
            // UI Sounds
            {"ButtonClick", buttonClick},
            {"MessageAlert", messageAlert},
            {"MessageFlyOut", messageFlyOut},
            {"Pickup", pickupSound},
            {"DropOff", dropOffSound},
            {"CashRegister", cashRegister},
            {"MoneyCounter", moneyCounter},
            
            // Car Sounds
            {"CarCrash", carCrash},
            {"CarBeep", carBeep},
            {"CarUnlock", carUnlock},
            {"SpinWheel", spinWheelTick},
            
            // Environment
            {"OpenDoor", openDoor},
            {"CloseDoor", closeDoor},
            {"TrainHorn", trainHorn},
            {"Firework", firework},
            
            // Misc
            {"WinSound", winSound},
            {"ReviveCountdown", reviveCountdown},
            {"VideoReward", videoRewardButton},
            {"Interaction", interactionSound1},
            {"BGMusic", bgLoop}
        };
    }

    public void PlayUI(string soundName)
    {
        if (audioClips.TryGetValue(soundName, out AudioClip clip))
        {
            uiSource.PlayOneShot(clip, uiVolume);
        }
    }

    public void PlaySFX(string soundName)
    {
        if (audioClips.TryGetValue(soundName, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    public void PlayLoop(string soundName)
    {
        if (audioClips.TryGetValue(soundName, out AudioClip clip))
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    public void StopLoop()
    {
        musicSource.Stop();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }

    public void SetUIVolume(float volume)
    {
        uiVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("UIVolume", uiVolume);
    }
}
