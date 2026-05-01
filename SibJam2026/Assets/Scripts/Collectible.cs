using UnityEngine.Events;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public UnityEvent onCollected;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        onCollected?.Invoke();
        gameObject.SetActive(false);
    }
}