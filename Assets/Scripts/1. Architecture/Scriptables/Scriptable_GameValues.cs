using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "GameValues_SO", menuName = "Archanite/GameValues")]
public class Scriptable_GameValues : ScriptableObject
{
    [HideInInspector] public int currentLevelIndex=0;
    [HideInInspector] public int playableLevel=10;
    [Header("Loading Screen")]
    public float loadingMoveInDuration = 1;
    public float loadingMoveOutDuration = 1;
    public bool isRetry = false;
    public bool isNext = false;

}
