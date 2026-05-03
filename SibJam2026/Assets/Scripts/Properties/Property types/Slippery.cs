using CMF;
using UnityEngine;

public class SlipperyZone : MonoBehaviour
{
    [Header("Множители")]
    [Tooltip("Ускорение")]
    public float accelerationMult = 0.3f;

    [Tooltip("Торможение")]
    public float decelerationMult = 0.2f;

    private AdvancedWalkerController controller;
    private float origAccel, origDecel;

    private void OnTriggerEnter(Collider other)
    {
        if (isActiveAndEnabled == false)
            return;
        if (!other.CompareTag("Player")) return;

        controller = other.GetComponent<AdvancedWalkerController>();
        if (controller == null)
            controller = other.GetComponentInParent<AdvancedWalkerController>();
        if (controller == null) return;

        origAccel = controller.acceleration;
        origDecel = controller.deceleration;

        controller.acceleration = origAccel * accelerationMult;
        controller.deceleration = origDecel * decelerationMult;
    }
    private void OnDisable()
    {
        Destroy(this);
    }
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (controller == null) return;

        controller.acceleration = origAccel;
        controller.deceleration = origDecel;

        controller = null;
    }
}