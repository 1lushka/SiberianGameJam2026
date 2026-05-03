using UnityEngine;
using UnityEngine.Events;

public class Collectible : BaseProperty
{
    public UnityEvent onCollected;
    public AudioClip[] collectSounds;   

    void OnTriggerEnter(Collider other)
    {
        if (!isActiveAndEnabled) return;
        if (!other.CompareTag("Player")) return;

        if (CoinCounter.Instance != null)
            CoinCounter.Instance.AddCoin(1);

        // Проигрываем случайный звук
        PlayRandomCollectSound();

        onCollected?.Invoke();
        gameObject.SetActive(false);
    }

    private void PlayRandomCollectSound()
    {
        if (collectSounds == null || collectSounds.Length == 0) return;

        AudioClip clip = collectSounds[Random.Range(0, collectSounds.Length)];
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, transform.position);
    }
}