using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint; // если не задан, берётся сам объект

    private void OnTriggerEnter(Collider other)
    {
        // Проверка: коллайдер принадлежит игроку?
        // Можно проверить по ссылке из менеджера (сравнить корневые трансформы)
        if (RespawnManager.Instance == null || RespawnManager.Instance.player == null)
            return;

        Transform root = other.transform.root;
        if (root == RespawnManager.Instance.player)
        {
            Transform point = respawnPoint ? respawnPoint : transform;
            RespawnManager.Instance.SaveCheckpoint(point.position, point.rotation);
        }
    }
}