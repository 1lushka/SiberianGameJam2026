using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.OnScreen;

public class MobileStick : OnScreenStick, IPointerDownHandler, IDragHandler, IPointerUpHandler, IInitializePotentialDragHandler, ICancelHandler
{
    MobileTouchOwner owner;
    public bool Held => owner.Held;
    public Vector2 Value(float deadZone)
    {
        if (!Held || !(control is Vector2Control axis)) return Vector2.zero;
        var v = Vector2.ClampMagnitude(axis.ReadUnprocessedValue(), 1);
        float length = v.magnitude;
        return length <= deadZone ? Vector2.zero : v / length * Mathf.Clamp01((length - deadZone) / (1 - deadZone));
    }
    public void OnInitializePotentialDrag(PointerEventData e) { e.useDragThreshold = false; }
    void IPointerDownHandler.OnPointerDown(PointerEventData e) { if (owner.Begin(e)) base.OnPointerDown(e); }
    void IDragHandler.OnDrag(PointerEventData e) { if (owner.Owns(e)) base.OnDrag(e); }
    void IPointerUpHandler.OnPointerUp(PointerEventData e) { if (owner.Owns(e)) Release(); }
    public void OnCancel(BaseEventData e) { Release(); }
    public void Release() { if (Held) base.OnPointerUp(owner.data); owner.Clear(); }
    void Update() { if (owner.Released) Release(); }
    protected override void OnDisable() { Release(); base.OnDisable(); }
}
