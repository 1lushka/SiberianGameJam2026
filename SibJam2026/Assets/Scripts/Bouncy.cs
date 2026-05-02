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

        Collider bounceCollider = GetBounceCollider();
        if (bounceCollider == null) return;

        Vector3 bounceDirection = GetBounceDirection(bounceCollider, other.bounds.center);
        Vector3 currentMomentum = controller.GetMomentum();

        // Preserve sideways motion, but keep bounce speed along the hit side deterministic.
        Vector3 lateralMomentum = Vector3.ProjectOnPlane(currentMomentum, bounceDirection);
        controller.SetMomentum(lateralMomentum + bounceDirection * bounceForce);
    }

    private Collider GetBounceCollider()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        Collider fallbackTrigger = null;

        foreach (Collider collider in colliders)
        {
            if (collider == null || !collider.enabled)
                continue;

            if (!collider.isTrigger)
                return collider;

            if (fallbackTrigger == null)
                fallbackTrigger = collider;
        }

        return fallbackTrigger;
    }

    private Vector3 GetBounceDirection(Collider bounceCollider, Vector3 playerPosition)
    {
        if (bounceCollider is BoxCollider boxCollider)
            return GetBoxBounceDirection(boxCollider, playerPosition);

        Vector3 closestPoint = bounceCollider.ClosestPoint(playerPosition);
        Vector3 bounceDirection = playerPosition - closestPoint;

        if (bounceDirection.sqrMagnitude < 0.0001f)
            bounceDirection = playerPosition - bounceCollider.bounds.center;

        if (bounceDirection.sqrMagnitude < 0.0001f)
            bounceDirection = transform.up;

        return bounceDirection.normalized;
    }

    private Vector3 GetBoxBounceDirection(BoxCollider boxCollider, Vector3 playerPosition)
    {
        Vector3 localPoint = boxCollider.transform.InverseTransformPoint(playerPosition) - boxCollider.center;
        Vector3 halfSize = boxCollider.size * 0.5f;

        float xDistance = halfSize.x - Mathf.Abs(localPoint.x);
        float yDistance = halfSize.y - Mathf.Abs(localPoint.y);
        float zDistance = halfSize.z - Mathf.Abs(localPoint.z);

        Vector3 localNormal;
        if (xDistance <= yDistance && xDistance <= zDistance)
            localNormal = new Vector3(GetAxisSign(localPoint.x), 0f, 0f);
        else if (yDistance <= zDistance)
            localNormal = new Vector3(0f, GetAxisSign(localPoint.y), 0f);
        else
            localNormal = new Vector3(0f, 0f, GetAxisSign(localPoint.z));

        return boxCollider.transform.TransformDirection(localNormal).normalized;
    }

    private float GetAxisSign(float value)
    {
        return value >= 0f ? 1f : -1f;
    }
}
