using UnityEngine;

public class GravityTargetZone : MonoBehaviour
{
    [SerializeField] private Transform gravityTarget;
    private void OnDisable()
    {
        Destroy(this);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (isActiveAndEnabled == false)
            return;
        AlignRigidbodyToTarget align = FindAlignComponent(other);
        if (align == null || gravityTarget == null)
            return;

        align.target = gravityTarget;

        if (!align.enabled)
            align.enabled = true;
    }

    private AlignRigidbodyToTarget FindAlignComponent(Collider other)
    {
        if (other.attachedRigidbody != null &&
            other.attachedRigidbody.TryGetComponent(out AlignRigidbodyToTarget alignOnRigidbody))
        {
            return alignOnRigidbody;
        }

        if (other.TryGetComponent(out AlignRigidbodyToTarget alignOnCollider))
            return alignOnCollider;

        return other.GetComponentInParent<AlignRigidbodyToTarget>();
    }
}
