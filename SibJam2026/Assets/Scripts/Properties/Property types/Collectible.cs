using UnityEngine;
using UnityEngine.Events;

public class Collectible : BaseProperty
{
    public UnityEvent onCollected;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (CoinCounter.Instance != null)
            CoinCounter.Instance.AddCoin(1);

        onCollected?.Invoke();
        gameObject.SetActive(false);
    }
}