using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WorldSpaceEmojiPool : MonoBehaviour
{
    [System.Serializable]
    public class EmojiData
    {
        public string emojiType;
        public Sprite emojiSprite;
        public Color emojiColor = Color.white;
    }

    [Header("Emoji Settings")]
    [SerializeField] private EmojiData[] emojiTypes;
    [SerializeField] private GameObject emojiPrefab;
    [SerializeField] private int poolSize = 10;

    [Header("Animation Settings")]
    [SerializeField] private float popUpHeight = 2f;
    [SerializeField] private float popUpDuration = 1.5f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] private float floatSpeed = 1f;

    private Queue<GameObject> emojiPool = new Queue<GameObject>();
    private List<ActiveEmoji> activeEmojis = new List<ActiveEmoji>();
    private Camera mainCamera;
    private Dictionary<string, EmojiData> emojiDictionary;

    private class ActiveEmoji
    {
        public GameObject gameObject;
        public Image image;
        public CanvasGroup canvasGroup;
        public float timer;
        public Vector3 startPosition;
        public float startTime;
    }

    private void Awake()
    {
        mainCamera = Camera.main;
        InitializeEmojiDictionary();
        InitializePool();
    }

    private void InitializeEmojiDictionary()
    {
        emojiDictionary = new Dictionary<string, EmojiData>();
        foreach (var emoji in emojiTypes)
        {
            emojiDictionary[emoji.emojiType] = emoji;
        }
    }

    private void InitializePool()
    {
        if (emojiPrefab == null)
        {
            Debug.LogError("Emoji Prefab not assigned!");
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject emoji = Instantiate(emojiPrefab, transform);
            emoji.SetActive(false);
            emojiPool.Enqueue(emoji);
        }
    }

    public void ShowEmoji(string emojiType, Vector3 worldPosition, Vector3 carForward)
    {
        if (!emojiDictionary.ContainsKey(emojiType))
        {
            Debug.LogWarning($"Emoji type '{emojiType}' not found!");
            return;
        }

        GameObject emojiObj = GetPooledEmoji();
        if (emojiObj == null) return;

        EmojiData data = emojiDictionary[emojiType];
        
        Vector3 offsetPosition = worldPosition + Vector3.up * 1.5f;
        
        Vector3 rightOffset = Vector3.Cross(Vector3.up, carForward).normalized * 0.5f;
        offsetPosition += rightOffset;

        emojiObj.transform.position = offsetPosition;
        emojiObj.SetActive(true);

        Image image = emojiObj.GetComponent<Image>();
        CanvasGroup canvasGroup = emojiObj.GetComponent<CanvasGroup>();

        if (image != null)
        {
            image.sprite = data.emojiSprite;
            image.color = data.emojiColor;
        }

        ActiveEmoji activeEmoji = new ActiveEmoji
        {
            gameObject = emojiObj,
            image = image,
            canvasGroup = canvasGroup,
            timer = popUpDuration,
            startPosition = offsetPosition,
            startTime = Time.time
        };

        activeEmojis.Add(activeEmoji);
    }

    private GameObject GetPooledEmoji()
    {
        if (emojiPool.Count > 0)
        {
            return emojiPool.Dequeue();
        }

        GameObject emoji = Instantiate(emojiPrefab, transform);
        return emoji;
    }

    private void Update()
    {
        UpdateActiveEmojis();
    }

    private void UpdateActiveEmojis()
    {
        for (int i = activeEmojis.Count - 1; i >= 0; i--)
        {
            ActiveEmoji emoji = activeEmojis[i];
            emoji.timer -= Time.deltaTime;

            if (emoji.timer <= 0)
            {
                ReturnToPool(emoji);
                activeEmojis.RemoveAt(i);
                continue;
            }

            float progress = 1f - (emoji.timer / popUpDuration);
            
            float yOffset = popUpHeight * progress;
            Vector3 targetPos = emoji.startPosition + Vector3.up * yOffset;
            emoji.gameObject.transform.position = targetPos;

            if (mainCamera != null)
            {
                emoji.gameObject.transform.LookAt(mainCamera.transform);
                emoji.gameObject.transform.Rotate(0, 180, 0);
            }

            float scale = scaleCurve.Evaluate(progress);
            emoji.gameObject.transform.localScale = Vector3.one * scale * 0.8f;

            if (emoji.canvasGroup != null)
            {
                emoji.canvasGroup.alpha = alphaCurve.Evaluate(progress);
            }
        }
    }

    private void ReturnToPool(ActiveEmoji emoji)
    {
        emoji.gameObject.SetActive(false);
        emojiPool.Enqueue(emoji.gameObject);
    }
}