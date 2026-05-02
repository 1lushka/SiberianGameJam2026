using UnityEngine;

[CreateAssetMenu(fileName = "CharacterMovementPreset", menuName = "Game/Character Movement Preset")]
public class CharacterMovementPreset : ScriptableObject
{
	[Header("Ground Movement")]
	public float movementSpeed = 7f;
	public float acceleration = 60f;
	public float deceleration = 80f;

	[Header("Air Movement")]
	public float airControlRate = 2f;
	public float airFriction = 0.5f;

	[Header("Jump")]
	public float jumpSpeed = 10f;
	public float jumpDuration = 0.2f;
	public float jumpBufferDuration = 0.2f;
	public float coyoteTimeDuration = 0.1f;

	[Header("Physics")]
	public float groundFriction = 100f;
	public float gravity = 30f;
	public float slideGravity = 5f;
	public float slopeLimit = 80f;

	void OnValidate()
	{
		movementSpeed = Mathf.Max(0f, movementSpeed);
		acceleration = Mathf.Max(0f, acceleration);
		deceleration = Mathf.Max(0f, deceleration);
		airControlRate = Mathf.Max(0f, airControlRate);
		airFriction = Mathf.Max(0f, airFriction);
		jumpSpeed = Mathf.Max(0f, jumpSpeed);
		jumpDuration = Mathf.Max(0f, jumpDuration);
		jumpBufferDuration = Mathf.Max(0f, jumpBufferDuration);
		coyoteTimeDuration = Mathf.Max(0f, coyoteTimeDuration);
		groundFriction = Mathf.Max(0f, groundFriction);
		gravity = Mathf.Max(0f, gravity);
		slideGravity = Mathf.Max(0f, slideGravity);
		slopeLimit = Mathf.Clamp(slopeLimit, 0f, 89.9f);
	}
}
