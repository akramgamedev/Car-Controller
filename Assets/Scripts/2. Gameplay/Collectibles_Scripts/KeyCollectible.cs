using UnityEngine;

public class KeyCollectible : MonoBehaviour
{
    [SerializeField] private int keyValue = 10;

    private bool isCollected = false;

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Car"))
        {
            isCollected = true;
            StaticEvents.GameEconomy.OnCurrencyChange?.Invoke(keyValue, GlobalEnums.CurrencyType.Key);
            AudioManager.Instance?.PlaySFX("CashRegister");
            gameObject.SetActive(false);
        }
    }
}