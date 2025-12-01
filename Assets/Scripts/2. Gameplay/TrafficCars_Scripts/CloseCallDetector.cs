using TMPro;
using UnityEngine;

public class CloseCallDetector : MonoBehaviour
{
    [Header("Settings")]
    public float closeCallDistance = 3f;
    public float veryCloseDistance = 1.5f;

    [Header("UI References")]
    public TextMeshProUGUI messageText;
    public float messageDuration = 1f;

    [Header("Sounds")]
    public AudioSource hornAudio;

    private float messageTimer = 0f;

    private void Start()
    {
        if (messageText = null)
        {
            messageText.enabled = false;
        }
    }

    void Update()
    {
        if (messageTimer > 0)
        {
            messageTimer -= Time.deltaTime;

            if (messageTimer <= 0 && messageText != null)
            {
                messageText.enabled = false;
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Car")) return;

        float distance = Vector3.Distance(transform.position, other.transform.position);
        if (distance <= veryCloseDistance)
        {
            ShowMessage("Very Close!");
            PlayHorn();
        }
        else if (distance <= closeCallDistance)
        {
            ShowMessage("Close Call!");
            PlayHorn();
        }
    }

    private void ShowMessage(string msg)
    {
        if (messageText == null) return;

        messageText.text = msg;
        messageText.enabled = true;
        messageTimer = messageDuration;
    }

    private void PlayHorn()
    {
        if (hornAudio != null && !hornAudio.isPlaying)
        {
            hornAudio.Play();
        }
    }
}
