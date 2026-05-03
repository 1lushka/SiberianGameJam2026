using UnityEngine;
using DG.Tweening;
using CMF;

public class Disappearing : BaseProperty
{
    [Header("Настройки анимации")]
    public float shakeDuration = 0.8f;
    public float shakeStrength = 0.15f;
    public int vibrato = 30;
    public float shrinkDuration = 0.4f;
    public float delayBeforeShrink = 0.1f;

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
        // Твой вызов
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

        // Звук тряски – через AudioControl
        if (AudioControl.Instance != null)
            AudioControl.Instance.PlayShakeSound(visualObject.transform.position);

        Sequence seq = DOTween.Sequence();
        seq.Append(visualObject.transform.DOShakePosition(shakeDuration, shakeStrength, vibrato));
        seq.AppendInterval(delayBeforeShrink);

        // Звук сжатия
        seq.AppendCallback(() => {
            if (AudioControl.Instance != null)
                AudioControl.Instance.PlayShrinkSound(visualObject.transform.position);
        });

        seq.Append(visualObject.transform.DOScale(Vector3.zero, shrinkDuration).SetEase(Ease.InBack));
        seq.OnComplete(() => visualObject.SetActive(false));
    }

    public void ResetObject()
    {
        triggered = false;
        visualObject.transform.localScale = originalScale;
        visualObject.SetActive(true);
    }
}