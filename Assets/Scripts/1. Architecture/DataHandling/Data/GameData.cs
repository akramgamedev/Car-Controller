[System.Serializable]
public class GameData
{
    public CarData carData;
    public EconomyData economy;
    public LevelUnlockData level;
    public SettingData setting;
    public ProfileData profile;
    public InAppData inApp;

    public GameData(CarData _carData,EconomyData _economy, LevelUnlockData _level, SettingData _setting,ProfileData _profile, InAppData _inapp)
    {
        carData=_carData;
        economy = _economy;
        level = _level;
        setting = _setting;
        profile = _profile;
        inApp =_inapp;
    }
}
