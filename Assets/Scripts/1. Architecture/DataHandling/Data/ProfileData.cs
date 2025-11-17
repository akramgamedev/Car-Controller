using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProfileData
{
    public int profileIndex;
    public string profileName;

    public ProfileData()
    {
        profileIndex = 0;
        profileName = "PlayerName";
    }

    public void SetProfileIndex(int value)
    {
        profileIndex = value;
    }
    public int GetProfileIndex()
    {
        return profileIndex;
    }
    public void SetProfileName(string value)
    {
        profileName = value;
    }
    public string GetProfileName()
    {
        return profileName;
    }
}
