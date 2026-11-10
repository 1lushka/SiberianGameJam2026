using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.UI;

// UI pointer IDs are per finger. A zone keeps its first finger until release/cancel.
public struct MobileTouchOwner
{
    public PointerEventData data;
    int id;
    TouchControl touch;
    public bool Held => data != null;
    public bool Released => touch != null && !touch.press.isPressed;
    public bool Begin(PointerEventData e)
    {
        if (Held) return false;
        data = e; id = e.pointerId;
        touch = (e as ExtendedPointerEventData)?.control?.parent as TouchControl;
        return true;
    }
    public bool Owns(PointerEventData e) => Held && id == e.pointerId;
    public void Clear() { data = null; touch = null; }
}

public class MobileLookField : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IInitializePotentialDragHandler, ICancelHandler
{
    MobileTouchOwner owner;
    Vector2 previous, pending, sample;
    int frame = -1;
    public bool Held => owner.Held;
    public Vector2 Delta
    {
        get
        {
            if (!Held) return Vector2.zero;
            if (frame != Time.frameCount) { sample = pending; pending = Vector2.zero; frame = Time.frameCount; }
            return sample;
        }
    }
    public void OnInitializePotentialDrag(PointerEventData e) { e.useDragThreshold = false; }
    public void OnPointerDown(PointerEventData e) { if (owner.Begin(e)) { previous = e.position; pending = sample = Vector2.zero; frame = -1; } }
    public void OnDrag(PointerEventData e)
    {
        if (!owner.Owns(e)) return;
        pending += e.position - previous; previous = e.position;
    }
    public void OnPointerUp(PointerEventData e) { if (owner.Owns(e)) Release(); }
    public void OnCancel(BaseEventData e) { Release(); }
    public void Release() { owner.Clear(); pending = sample = Vector2.zero; frame = -1; }
    void Update() { if (owner.Released) Release(); }
    void OnDisable() { Release(); }
}
