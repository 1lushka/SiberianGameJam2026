using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!CheckpointTrigger.IsPlayerCollider(other))
            return;

        CheckpointTrigger.RestartLevel();
    }
}
