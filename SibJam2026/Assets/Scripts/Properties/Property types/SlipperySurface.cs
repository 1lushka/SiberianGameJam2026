using UnityEngine;

public class SlipperySurface : MonoBehaviour
{
	[Tooltip("Optional movement preset override for this surface. If empty, the controller will use its default ice preset.")]
	public CharacterMovementPreset movementPresetOverride;
}
