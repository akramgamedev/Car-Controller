using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class CustomerFeedbackPopup : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI encouragementText;
    public TextMeshProUGUI moneyText;
    public Image iconImage;
    public CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    public float floatSpeed = 1.5f;
    public float lifetime = 2.5f;
    public float fadeStartTime = 1.5f;

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
        // Set random encouragement message
        if (encouragementMessages.Length > 0)
        {
            string randomMessage = encouragementMessages[Random.Range(0, encouragementMessages.Length)];
            if (encouragementText != null)
                encouragementText.text = randomMessage;
        }

        // Set money amount
        if (moneyText != null)
            moneyText.text = $"+${moneyEarned}";

        // Set random icon if available
        if (iconImage != null && iconSprites != null && iconSprites.Length > 0)
        {
            iconImage.sprite = iconSprites[Random.Range(0, iconSprites.Length)];
        }

        // Reset position and alpha
        startLocalPosition = transform.localPosition;
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        // Show and animate
        gameObject.SetActive(true);
        StartCoroutine(AnimatePopup());
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

    IEnumerator AnimatePopup()
    {
        float elapsed = 0;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;

            // Float upward (local space so it moves with car)
            transform.localPosition = startLocalPosition + Vector3.up * (elapsed * floatSpeed);

            // Fade out after fadeStartTime
            if (elapsed > fadeStartTime && canvasGroup != null)
            {
                float fadeProgress = (elapsed - fadeStartTime) / (lifetime - fadeStartTime);
                canvasGroup.alpha = 1 - fadeProgress;
            }

            yield return null;
        }

        // Hide when done
        gameObject.SetActive(false);
    }
}