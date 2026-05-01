using UnityEngine;

public class SlipperySurface : MonoBehaviour
{
	[Header("Ice Movement")]
	[Tooltip("Horizontal acceleration added by movement input while grounded on this surface.")]
	public float inputAcceleration = 18f;

	[Tooltip("How quickly horizontal momentum is reduced while grounded on this surface.")]
	public float groundFriction = 1.25f;

	[Tooltip("Additional downhill acceleration applied while standing on an angled ice surface.")]
	public float downhillAcceleration = 16f;

	[Tooltip("Project momentum onto the ice surface normal to keep sliding aligned with the floor.")]
	public bool projectMomentumToSurface = true;
}
