using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CloseCallEmojiSystem : MonoBehaviour
{
    [System.Serializable]
    public class EmojiConfig
    {
        public string messageType;
        public Sprite emojiSprite;
    }

    [Header("Emoji Configurations")]
    [SerializeField] private List<EmojiConfig> emojiConfigs = new List<EmojiConfig>();

    [Header("UI References")]
    [SerializeField] private Image emojiImage;
    [SerializeField] private CanvasGroup emojiCanvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float popInScale = 1.3f;
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float popInDuration = 0.25f;
    [SerializeField] private Ease popInEase = Ease.OutBack;

    [SerializeField] private float floatUpDistance = 100f;
    [SerializeField] private float floatDuration = 1.2f;
    [SerializeField] private Ease floatEase = Ease.OutCubic;

    [SerializeField] private float fadeOutDelay = 0.6f;
    [SerializeField] private float fadeOutDuration = 0.6f;

    [Header("Optional Rotation Animation")]
    [SerializeField] private bool enableRotation = false;
    [SerializeField] private float rotationAmount = 15f;
    [SerializeField] private float rotationDuration = 0.4f;

    private Dictionary<string, Sprite> emojiSpriteDict = new Dictionary<string, Sprite>();
    private Vector2 originalPosition;
    private bool isAnimating = false;

    private void Awake()
    {
        InitializeEmojiDictionary();

        if (emojiImage != null)
        {

            originalPosition = emojiImage.rectTransform.anchoredPosition;
            emojiImage.gameObject.SetActive(false);
        }
        else
        {
            LogHelper.Log("Emoji Image not assigned");
        }

        if (emojiCanvasGroup == null && emojiImage != null)
        {
            emojiCanvasGroup = emojiImage.GetComponent<CanvasGroup>();
            if (emojiCanvasGroup == null)
            {
                emojiCanvasGroup = emojiImage.gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    private void InitializeEmojiDictionary()
    {
        foreach (var config in emojiConfigs)
        {
            if (!string.IsNullOrEmpty(config.messageType) && config.emojiSprite != null)
            {
                emojiSpriteDict[config.messageType] = config.emojiSprite;
            }
        }
    }

    public void ShowEmoji(string messageType)
    {
        if (emojiImage == null) return;

        if (!emojiSpriteDict.ContainsKey(messageType))
        {
            LogHelper.LogWarning($"No emoji configured for message type: {messageType}");
            return;
        }

        if (isAnimating)
        {
            emojiImage.rectTransform.DOKill();
            emojiCanvasGroup.DOKill();
        }

        if (isAnimating)
        {
            emojiImage.rectTransform.DOKill();
            emojiCanvasGroup.DOKill();
        }

        isAnimating = true;

        emojiImage.sprite = emojiSpriteDict[messageType];

        emojiImage.rectTransform.anchoredPosition = originalPosition;
        emojiImage.rectTransform.localScale = Vector3.zero;
        emojiImage.rectTransform.rotation = Quaternion.identity;


        if (emojiCanvasGroup != null)
        {
            emojiCanvasGroup.alpha = 1f;
        }

        emojiImage.gameObject.SetActive(true);

        Sequence emojiSequence = DOTween.Sequence();

        emojiSequence.Append(
            emojiImage.rectTransform.DOScale(popInScale, popInDuration * 0.6f)
            .SetEase(popInEase)
        );
        emojiSequence.Append(
          emojiImage.rectTransform.DOScale(normalScale, popInDuration * 0.4f)
              .SetEase(Ease.InOutQuad)
      );

        Vector2 targetPos = originalPosition + Vector2.up * floatUpDistance;
        emojiSequence.Insert(
            popInDuration * 0.5f,
            emojiImage.rectTransform.DOAnchorPos(targetPos, floatDuration)
                .SetEase(floatEase)
        );

        if (enableRotation)
        {
            emojiSequence.Insert(
                popInDuration,
                emojiImage.rectTransform.DORotate(new Vector3(0, 0, rotationAmount), rotationDuration)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
            );
        }

        // 4. Fade out
        if (emojiCanvasGroup != null)
        {
            emojiSequence.Insert(
                fadeOutDelay,
                emojiCanvasGroup.DOFade(0f, fadeOutDuration)
            );
        }

        // 5. Deactivate when done
        emojiSequence.OnComplete(() =>
        {
            emojiImage.gameObject.SetActive(false);
            isAnimating = false;
        });
    }

    private void OnDestroy()
    {
        if (emojiImage != null)
        {
            emojiImage.rectTransform.DOKill();
        }
        if (emojiCanvasGroup != null)
        {
            emojiCanvasGroup.DOKill();
        }
    }

    // Optional: Manual test in editor
    // [ContextMenu("Test PRO Emoji")]
    // private void TestProEmoji() => ShowEmoji("PRO");

    // [ContextMenu("Test GREAT Emoji")]
    // private void TestGreatEmoji() => ShowEmoji("GREAT");

    // [ContextMenu("Test WHOAH Emoji")]
    // private void TestWhoahEmoji() => ShowEmoji("WHOAH");

    // [ContextMenu("Test DANGER Emoji")]
    // private void TestDangerEmoji() => ShowEmoji("DANGER");
}
