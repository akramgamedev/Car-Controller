[System.Serializable]
public class GameData
{
    public EconomyData economy;
    public LevelUnlockData level;
    public SettingData setting;
    public ProfileData profile;

    public InAppData inApp;

    public GameData(EconomyData _economy, LevelUnlockData _level, SettingData _setting,ProfileData _profile, InAppData _inapp)
    {
        economy = _economy;
        level = _level;
        setting = _setting;
        profile = _profile;
        inApp =_inapp;
    }
}
