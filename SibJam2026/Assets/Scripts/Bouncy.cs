using UnityEngine;

public class Bouncy : MonoBehaviour
{
    [Tooltip("Фиксированная сила отскока")]
    public float bounceForce = 10f;

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        Rigidbody playerRb = collision.rigidbody;
        if (playerRb == null) return;

        Vector3 normal = collision.contacts[0].normal;

        playerRb.AddForce(normal * bounceForce, ForceMode.Impulse);
    }
}