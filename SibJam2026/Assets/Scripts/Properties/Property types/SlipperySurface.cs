using UnityEngine;

public class SlipperySurface : BaseProperty
{
	[Tooltip("Optional movement preset override for this surface. If empty, the controller will use its default ice preset.")]
	public CharacterMovementPreset movementPresetOverride;
    private void OnDisable()
    {
        Destroy(this);
    }
}
