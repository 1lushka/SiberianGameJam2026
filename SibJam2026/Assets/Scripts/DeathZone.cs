using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (RespawnManager.Instance == null || RespawnManager.Instance.player == null)
            return;

        // ѕровер€ем, что коллайдер принадлежит игроку
        if (other.transform.root == RespawnManager.Instance.player)
            RespawnManager.Instance.Respawn();
    }
}