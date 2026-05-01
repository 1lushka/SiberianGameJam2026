using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public class Disappearing : MonoBehaviour
{
    [Header("Настройки")]
    [Tooltip("Сколько секунд трястись")]
    public float shakeDuration = 0.8f;

    [Tooltip("Сила тряски")]
    public float shakeStrength = 0.15f;

    [Tooltip("Насколько сильно дрожать (вибрато)")]
    public int vibrato = 30;

    [Tooltip("Сколько секунд сжиматься")]
    public float shrinkDuration = 0.4f;

    [Tooltip("Задержка перед сжатием после тряски")]
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
        print("asdas");
        if (triggered) return;

        
        triggered = true;
        StartDisappearing();
    }

    private void StartDisappearing()
    {
        DG.Tweening.Sequence seq = DOTween.Sequence();

        // Тряска
        seq.Append(visualObject.transform.DOShakePosition(shakeDuration, shakeStrength, vibrato));

        // Небольшая пауза
        seq.AppendInterval(delayBeforeShrink);

        // Сжатие до нуля
        seq.Append(visualObject.transform.DOScale(Vector3.zero, shrinkDuration).SetEase(Ease.InBack));

        // Отключаем объект в конце
        seq.OnComplete(() =>
        {
            visualObject.SetActive(false);
        });
    }

    // Публичный метод для сброса (вызови при рестарте уровня)
    public void ResetObject()
    {
        triggered = false;
        visualObject.transform.localScale = originalScale;
        visualObject.SetActive(true);
    }
}