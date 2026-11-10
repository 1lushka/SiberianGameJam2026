using System;
using System.Collections;
using UnityEngine;

public class UI_CheckPointMessage : MonoBehaviour
{
    [SerializeField] private float messageDuration = 2f;
    [SerializeField] GameObject checkpointCanvas;

    private void Awake()
    {
        checkpointCanvas.SetActive(false);
        CheckpointTrigger.StartTrigger += ShowCheckpointMessage;
    }

    private void OnDestroy()
    {
        CheckpointTrigger.StartTrigger -= ShowCheckpointMessage;
    }

    private void ShowCheckpointMessage()
    {
        StartCoroutine("ShowMessageCoroutine");
    }

    private IEnumerator ShowMessageCoroutine()
    {
        checkpointCanvas.SetActive(true);
        yield return new WaitForSeconds(messageDuration); // Показываем сообщение 2 секунды
        checkpointCanvas.SetActive(false);
    }
}
