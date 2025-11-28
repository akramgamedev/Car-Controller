using UnityEngine;

public class CoinCollectible : MonoBehaviour
{
    [SerializeField] private int coinValue = 10;
    [SerializeField] private ParticleSystem cashParticle;

    private bool isCollected = false;

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Car"))
        {
            isCollected = true;
            StaticEvents.GameEconomy.OnCurrencyChange?.Invoke(coinValue, GlobalEnums.CurrencyType.Coin);
            UIManager.Instance.AddCoinsEarned(coinValue);
            AudioManager.Instance?.PlaySFX("CashRegister");
            cashParticle.Play();
            gameObject.SetActive(false);
        }
    }
}
