using UnityEngine;
using DG.Tweening;

public class Disappearing : BaseProperty
{
    [Header("Настройки анимации")]
    public float shakeDuration = 0.8f;
    public float shakeStrength = 0.15f;
    public int vibrato = 30;
    public float shrinkDuration = 0.4f;
    public float delayBeforeShrink = 0.1f;

    [Header("Звуки")]
    public AudioClip[] shakeSounds;   
    public AudioClip[] shrinkSounds;  

    private GameObject visualObject;
    private Vector3 originalScale;
    private bool triggered;

    private void Start()
    {
        if (visualObject == null)
            visualObject = gameObject;
        originalScale = visualObject.transform.localScale;
    }

    private void OnEnable()
    {
        triggered = false;
        if (visualObject != null)
            visualObject.transform.localScale = originalScale;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        // Твой вызов (раскомментируй если нужно)
        // TriggerDisappearance();
    }

    public void TriggerDisappearance()
    {
        if (triggered) return;
        triggered = true;
        StartDisappearing();
    }

    private void StartDisappearing()
    {
        if (!isActiveAndEnabled) return;

        PlayRandomSound(shakeSounds);

        Sequence seq = DOTween.Sequence();

        seq.Append(visualObject.transform.DOShakePosition(shakeDuration, shakeStrength, vibrato));

        seq.AppendInterval(delayBeforeShrink);

        seq.AppendCallback(() => PlayRandomSound(shrinkSounds));

        seq.Append(visualObject.transform.DOScale(Vector3.zero, shrinkDuration).SetEase(Ease.InBack));

        seq.OnComplete(() => visualObject.SetActive(false));
    }

    private void PlayRandomSound(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, visualObject.transform.position);
    }

    public void ResetObject()
    {
        triggered = false;
        visualObject.transform.localScale = originalScale;
        visualObject.SetActive(true);
    }
}