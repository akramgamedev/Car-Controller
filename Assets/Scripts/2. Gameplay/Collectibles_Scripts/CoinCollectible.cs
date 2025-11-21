using UnityEngine;

public class CoinCollectible : MonoBehaviour
{
    [SerializeField] private int coinValue = 10;

    private bool isCollected = false;

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Car"))
        {
            isCollected = true;
            StaticEvents.GameEconomy.OnCurrencyChange?.Invoke(coinValue, GlobalEnums.CurrencyType.Coin);
            AudioManager.Instance?.PlaySFX("CashRegister");
            gameObject.SetActive(false);
        }
    }
}
