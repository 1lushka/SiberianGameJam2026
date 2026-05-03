using CMF;
using UnityEngine;

public class Bouncy : MonoBehaviour
{
    public enum BounceDirectionMode
    {
        ColliderNormal,
        ObjectUp,
        CustomTransformUp
    }

    public float bounceForce = 10f;
    [SerializeField] private BounceDirectionMode bounceDirectionMode = BounceDirectionMode.ColliderNormal;
    [SerializeField] private Transform bounceDirectionReference;
    [SerializeField] private bool preserveLateralMomentum = true;

    private Collider cachedBounceCollider;

    private void Awake()
    {
        cachedBounceCollider = FindBounceCollider();
    }
    private void OnDisable()
    {
        //Destroy(this);
    }
    public bool TriggerBounce(AdvancedWalkerController controller, Collider playerCollider)
    {
        if (isActiveAndEnabled == false)
            return false;
        if (controller == null)
            return false;

        Collider bounceCollider = GetBounceCollider();
        if (bounceCollider == null)
            return false;

        Vector3 playerPosition = playerCollider != null
            ? playerCollider.bounds.center
            : controller.transform.position;

        Vector3 bounceDirection = GetBounceDirection(bounceCollider, playerPosition);
        Vector3 currentMomentum = controller.GetMomentum();

        if (preserveLateralMomentum)
        {
            // Keep sideways motion, but make the bounce speed along the launch direction deterministic.
            Vector3 lateralMomentum = Vector3.ProjectOnPlane(currentMomentum, bounceDirection);
            controller.SetMomentum(lateralMomentum + bounceDirection * bounceForce);
        }
        else
        {
            // Full override for consistent launch angle regardless of the incoming velocity.
            controller.SetMomentum(bounceDirection * bounceForce);
        }

        AudioControl audioCtrl = controller.GetComponent<AudioControl>();
        if (audioCtrl != null)
            audioCtrl.PlayBounceSound();

        return true;
    }

    private Collider GetBounceCollider()
    {
        if (cachedBounceCollider != null && cachedBounceCollider.enabled)
            return cachedBounceCollider;

        cachedBounceCollider = FindBounceCollider();
        return cachedBounceCollider;
    }

    private Collider FindBounceCollider()
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
        switch (bounceDirectionMode)
        {
            case BounceDirectionMode.ObjectUp:
                return transform.up.normalized;

            case BounceDirectionMode.CustomTransformUp:
                if (bounceDirectionReference != null)
                    return bounceDirectionReference.up.normalized;
                return transform.up.normalized;
        }

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
