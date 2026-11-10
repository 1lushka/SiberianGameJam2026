using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;

public class MobileJumpButton : OnScreenButton, IPointerDownHandler, IPointerUpHandler, ICancelHandler
{
    MobileTouchOwner owner;
    public bool Held => owner.Held;
    void IPointerDownHandler.OnPointerDown(PointerEventData e) { if (owner.Begin(e)) base.OnPointerDown(e); }
    void IPointerUpHandler.OnPointerUp(PointerEventData e) { if (owner.Owns(e)) Release(); }
    public void OnCancel(BaseEventData e) { Release(); }
    public void Release() { if (Held) base.OnPointerUp(owner.data); owner.Clear(); }
    void Update() { if (owner.Released) Release(); }
    protected override void OnDisable() { Release(); base.OnDisable(); }
}
