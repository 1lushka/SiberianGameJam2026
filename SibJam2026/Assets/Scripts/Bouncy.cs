using CMF;
using UnityEngine;

public class Bouncy : MonoBehaviour
{
    public float bounceForce = 10f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        

        AdvancedWalkerController controller = other.GetComponent<AdvancedWalkerController>();
        if (controller == null)
            controller = other.GetComponentInParent<AdvancedWalkerController>();

        if (controller == null) return;

        

        // Берём коллайдер самого батута (на этом же объекте)
        Collider bounceCollider = GetComponent<Collider>();
        if (bounceCollider == null) return;
        print("asdasd");


        // Ближайшая точка на коллайдере к центру игрока (или его ногам)
        Vector3 playerPos = other.bounds.center;   // можно other.transform.position
        Vector3 closestPoint = bounceCollider.ClosestPoint(playerPos);

        // Нормаль: от поверхности к игроку (аналог collision.contacts[0].normal)
        Vector3 normal = (playerPos - closestPoint).normalized;

        controller.AddMomentum(normal * bounceForce);
    }
}