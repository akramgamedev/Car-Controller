using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class CustomerFeedbackPopup : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI encouragementText;
    public TextMeshProUGUI moneyText;
    public Image iconImage;
    public CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    public float floatDistance = 2f;
    public float totalDuration = 2.5f;
    public Ease floatEase = Ease.OutQuad;

    [Header("Scale Animation")]
    public float scaleInDuration = 0.2f;
    public float scaleOutDuration = 0.3f;
    public float startScale = 0.3f;
    public float maxScale = 1.2f;
    public Ease scaleInEase = Ease.OutBack;
    public Ease scaleOutEase = Ease.InBack;

    [Header("Fade Animation")]
    public float fadeOutDuration = 0.4f;

    [Header("Encouragement Messages")]
    public string[] encouragementMessages = new string[]
    {
        "Excellent Service",
        "Great Job",
        "Perfect Delivery",
        "Amazing Work",
        "Well Done",
        "Fantastic",
        "Outstanding",
        "Superb"
    };

    [Header("Icon Sprites (Optional)")]
    public Sprite[] iconSprites;

    private Vector3 startLocalPosition;
    private Transform mainCamera;
    private Sequence popupSequence;

    void Awake()
    {
        mainCamera = Camera.main.transform;

        // Make sure canvas group exists
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // Start hidden
        gameObject.SetActive(false);
    }

    public void Show(int moneyEarned)
    {
        // Kill any existing animation
        if (popupSequence != null && popupSequence.IsActive())
        {
            popupSequence.Kill();
        }

        // Set random encouragement message
        if (encouragementMessages.Length > 0)
        {
            string randomMessage = encouragementMessages[Random.Range(0, encouragementMessages.Length)];
            if (encouragementText != null)
                encouragementText.text = randomMessage;
        }

        // Set money amount
        if (moneyText != null)
            moneyText.text = $"+{moneyEarned}";

        // Set random icon if available
        if (iconImage != null && iconSprites != null && iconSprites.Length > 0)
        {
            iconImage.sprite = iconSprites[Random.Range(0, iconSprites.Length)];
        }

        // Reset position and alpha
        startLocalPosition = transform.localPosition;
        transform.localPosition = startLocalPosition;
        transform.localScale = Vector3.one * startScale;

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        // Show and animate
        gameObject.SetActive(true);
        AnimatePopup();
    }

    void LateUpdate()
    {
        // Always face camera
        if (mainCamera != null)
        {
            transform.LookAt(mainCamera);
            transform.Rotate(0, 180, 0);
        }
    }

    void AnimatePopup()
    {
        popupSequence = DOTween.Sequence();

        // Scale pop IN (0 to 0 seconds)
        popupSequence.Append(transform.DOScale(maxScale, scaleInDuration).SetEase(scaleInEase));

        // Float upward animation (runs throughout)
        Vector3 targetPos = startLocalPosition + Vector3.up * floatDistance;
        popupSequence.Join(transform.DOLocalMove(targetPos, totalDuration).SetEase(floatEase));

        // Calculate when to start scale out and fade
        float scaleOutStartTime = totalDuration - scaleOutDuration;
        float fadeOutStartTime = totalDuration - fadeOutDuration;

        // Scale OUT before disappearing
        popupSequence.Insert(scaleOutStartTime, transform.DOScale(0f, scaleOutDuration).SetEase(scaleOutEase));

        // Fade out animation
        if (canvasGroup != null)
        {
            popupSequence.Insert(fadeOutStartTime, canvasGroup.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad));
        }

        // Hide when complete
        popupSequence.OnComplete(() =>
        {
            gameObject.SetActive(false);

            // Reset for next use
            transform.localPosition = startLocalPosition;
            transform.localScale = Vector3.one;
            if (canvasGroup != null)
                canvasGroup.alpha = 1f;
        });
    }

    void OnDisable()
    {
        // Clean up animation if object is disabled
        if (popupSequence != null && popupSequence.IsActive())
        {
            popupSequence.Kill();
        }
    }

    void OnDestroy()
    {
        // Clean up animation on destroy
        if (popupSequence != null && popupSequence.IsActive())
        {
            popupSequence.Kill();
        }
    }
}


// using UnityEngine;
// using TMPro;
// using UnityEngine.UI;
// using System.Collections;
// using DG.Tweening;

// public class CustomerFeedbackPopup : MonoBehaviour
// {
//     [Header("UI References")]
//     public TextMeshProUGUI encouragementText;
//     public TextMeshProUGUI moneyText;
//     public Image iconImage;
//     public CanvasGroup canvasGroup;

//     [Header("Animation Settings")]
//     public float floatDistance = 2f;
//     public float floatDuration = 2.5f;
//     public float fadeDuration = 1f;
//     public float fadeDelay = 1.5f;
//     public Ease floatEase = Ease.OutQuad;
//     public Ease fadeEase = Ease.InQuad;

//     [Header("Scale Animation")]
//     public bool useScaleAnimation = true;
//     public float scaleInDuration = 0.3f;
//     public float startScale = 0.5f;
//     public Ease scaleEase = Ease.OutBack;

//     //[Header("Animation Settings")]


//     // public float floatSpeed = 1.5f;
//     // public float lifetime = 2.5f;
//     // public float fadeStartTime = 1.5f;

//     [Header("Encouragement Messages")]
//     public string[] encouragementMessages = new string[]
//     {
//         "Excellent Service",
//         "Great Job",
//         "Perfect Delivery",
//         "Amazing Work",
//         "Well Done",
//         "Fantastic",
//         "Outstanding",
//         "Superb"
//     };

//     [Header("Icon Sprites (Optional)")]
//     public Sprite[] iconSprites;

//     private Vector3 startLocalPosition;
//     private Transform mainCamera;
//     private Sequence popupSequence;

//     void Awake()
//     {
//         mainCamera = Camera.main.transform;

//         // Make sure canvas group exists
//         if (canvasGroup == null)
//             canvasGroup = GetComponent<CanvasGroup>();

//         // Start hidden
//         gameObject.SetActive(false);
//     }

//     public void Show(int moneyEarned)
//     {
//         if (popupSequence != null && popupSequence.IsActive())
//         {
//             popupSequence.Kill();
//         }

//         // Set random encouragement message
//         if (encouragementMessages.Length > 0)
//         {
//             string randomMessage = encouragementMessages[Random.Range(0, encouragementMessages.Length)];
//             if (encouragementText != null)
//                 encouragementText.text = randomMessage;
//         }

//         // Set money amount
//         if (moneyText != null)
//             moneyText.text = $"+{moneyEarned}";

//         // Set random icon if available
//         if (iconImage != null && iconSprites != null && iconSprites.Length > 0)
//         {
//             iconImage.sprite = iconSprites[Random.Range(0, iconSprites.Length)];
//         }

//         // Reset position and alpha
//         startLocalPosition = transform.localPosition;
//         transform.localPosition = startLocalPosition;


//         if (canvasGroup != null)
//             canvasGroup.alpha = 1f;

//         // Show and animate
//         gameObject.SetActive(true);
//         AnimatePopup();
//     }

//     void LateUpdate()
//     {
//         // Always face camera
//         if (mainCamera != null)
//         {
//             transform.LookAt(mainCamera);
//             transform.Rotate(0, 180, 0);
//         }
//     }

//     void AnimatePopup()
//     {
//         popupSequence = DOTween.Sequence();

//         // Optional scale-in animation
//         if (useScaleAnimation)
//         {
//             transform.localScale = Vector3.one * startScale;
//             popupSequence.Append(transform.DOScale(Vector3.one, scaleInDuration).SetEase(scaleEase));
//         }

//         // Float upward animation
//         Vector3 targetPos = startLocalPosition + Vector3.up * floatDistance;
//         popupSequence.Join(transform.DOLocalMove(targetPos, floatDuration).SetEase(floatEase));

//         // Fade out animation (starts after delay)
//         if (canvasGroup != null)
//         {
//             popupSequence.Insert(fadeDelay, canvasGroup.DOFade(0f, fadeDuration).SetEase(fadeEase));
//         }

//         // Hide when complete
//         popupSequence.OnComplete(() =>
//         {
//             gameObject.SetActive(false);

//             // Reset for next use
//             transform.localPosition = startLocalPosition;
//             transform.localScale = Vector3.one;
//             if (canvasGroup != null)
//                 canvasGroup.alpha = 1f;
//         });
//     }

//     void OnDisable()
//     {
//         if (popupSequence != null && popupSequence.IsActive())
//         {
//             popupSequence.Kill();
//         }
//     }

//     void OnDestroy()
//     {
//         // Clean up animation on destroy
//         if (popupSequence != null && popupSequence.IsActive())
//         {
//             popupSequence.Kill();
//         }
//     }
// }