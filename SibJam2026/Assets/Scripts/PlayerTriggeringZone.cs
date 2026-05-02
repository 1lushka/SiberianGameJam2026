using System.Collections.Generic;
using CMF;
using UnityEngine;

public class PlayerTriggeringZone : MonoBehaviour
{
    [SerializeField] private float bounceRetriggerDelay = 0.1f;

    private readonly HashSet<Bouncy> activeBounceOverlaps = new HashSet<Bouncy>();
    private readonly Dictionary<Bouncy, float> lastBounceTimes = new Dictionary<Bouncy, float>();

    private AdvancedWalkerController controller;
    private Collider playerBodyCollider;

    private void Awake()
    {
        controller = GetComponentInParent<AdvancedWalkerController>();

        if (controller != null)
            playerBodyCollider = controller.GetComponent<Collider>();

        if (playerBodyCollider == null)
            playerBodyCollider = GetComponentInParent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var disaperObj = other.GetComponent<Disappearing>();
        if (disaperObj == null)
            disaperObj = other.GetComponentInParent<Disappearing>();

        if (disaperObj != null)
            disaperObj.TriggerDisappearance();
    }

    private void OnTriggerStay(Collider other)
    {
        if (controller == null)
            return;

        if (!TryGetBouncy(other, out Bouncy bouncy))
            return;

        if (activeBounceOverlaps.Contains(bouncy))
            return;

        float lastBounceTime;
        if (lastBounceTimes.TryGetValue(bouncy, out lastBounceTime) &&
            Time.time - lastBounceTime < bounceRetriggerDelay)
            return;

        if (bouncy.TriggerBounce(controller, playerBodyCollider))
        {
            activeBounceOverlaps.Add(bouncy);
            lastBounceTimes[bouncy] = Time.time;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!TryGetBouncy(other, out Bouncy bouncy))
            return;

        activeBounceOverlaps.Remove(bouncy);
    }

    private void OnDisable()
    {
        activeBounceOverlaps.Clear();
        lastBounceTimes.Clear();
    }

    private bool TryGetBouncy(Collider other, out Bouncy bouncy)
    {
        bouncy = other.GetComponent<Bouncy>();
        if (bouncy == null)
            bouncy = other.GetComponentInParent<Bouncy>();

        return bouncy != null;
    }
}
