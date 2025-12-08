using UnityEngine;
using DG.Tweening;

public class MarkerBounce : MonoBehaviour
{
    [Header("Bounce Settings")]
    [SerializeField] private float bounceHeight = 0.3f;
    [SerializeField] private float bounceDuration = 0.5f;
    [SerializeField] private Ease bounceEase = Ease.OutQuad;
    [SerializeField] private int bounceLoops = -1; // -1 for infinite
    [SerializeField] private bool playOnStart = true;

    private Vector3 startPosition;
    private Tween bounceTween;

    private void Start()
    {
        startPosition = transform.localPosition;
        
        if (playOnStart)
        {
            StartBouncing();
        }
    }

    public void StartBouncing()
    {
        StopBouncing();
        
        bounceTween = transform.DOLocalMoveY(startPosition.y + bounceHeight, bounceDuration)
            .SetEase(bounceEase)
            .SetLoops(bounceLoops, LoopType.Yoyo);
    }

    public void StopBouncing()
    {
        if (bounceTween != null && bounceTween.IsActive())
        {
            bounceTween.Kill();
        }
        
        transform.localPosition = startPosition;
    }

    private void OnDisable()
    {
        StopBouncing();
    }

    private void OnDestroy()
    {
        if (bounceTween != null && bounceTween.IsActive())
        {
            bounceTween.Kill();
        }
    }
}