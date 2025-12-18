using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TapToPlayFadeAnimation : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float minAlpha = 0.3f;
    [SerializeField] private float maxAlpha = 1f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Ease easeType = Ease.InOutSine;

    private Graphic targetGraphic;
    private Tween fadeTween;

    private void Awake()
    {
        // Try to get Text or Image component
        targetGraphic = GetComponent<TextMeshProUGUI>();

        if (targetGraphic == null)
            targetGraphic = GetComponent<Text>();

        if (targetGraphic == null)
            targetGraphic = GetComponent<Image>();

        if (targetGraphic == null)
        {
            LogHelper.LogError("TapToPlayFadeAnimation: No Text, TextMeshProUGUI, or Image component found!");
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (targetGraphic != null)
        {
            StartFadeLoop();
        }
    }

    private void OnDisable()
    {
        StopFadeLoop();
    }

    private void StartFadeLoop()
    {
        fadeTween?.Kill();

        // Set initial alpha
        Color col = targetGraphic.color;
        col.a = maxAlpha;
        targetGraphic.color = col;

        // Create looping fade animation
        fadeTween = targetGraphic.DOFade(minAlpha, fadeDuration)
            .SetEase(easeType)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true); // Use unscaled time
    }

    private void StopFadeLoop()
    {
        fadeTween?.Kill();

        if (targetGraphic != null)
        {
            Color col = targetGraphic.color;
            col.a = maxAlpha;
            targetGraphic.color = col;
        }
    }

    private void OnDestroy()
    {
        fadeTween?.Kill();
    }
}