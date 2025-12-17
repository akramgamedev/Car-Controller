using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideInDelayHandler : MonoBehaviour
{
    [SerializeField] SlideInAnim[] animObjects;
    float time = 0.3f;
    private void OnEnable()
    {
        float delay = 0;
        for (int i =0; i< animObjects.Length; i++)
        {
            delay += time * 0.7f;
            animObjects[i].Init();
            animObjects[i].PlayAnimationWithDelay(delay);
        }
    }
    
}
