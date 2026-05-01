using UnityEngine;

namespace CMF
{
	[DefaultExecutionOrder(100)]
	[RequireComponent(typeof(AdvancedWalkerController))]
	[RequireComponent(typeof(Mover))]
	public class LedgeHangController : MonoBehaviour
	{
		struct LedgeData
		{
			public Vector3 hangPoint;
			public Vector3 wallNormal;
		}

		struct DebugCastInfo
		{
			public bool isValid;
			public bool hasHit;
			public Vector3 origin;
			public Vector3 direction;
			public float radius;
			public float distance;
			public float hitDistance;
			public Vector3 hitPoint;
			public Vector3 hitNormal;
			public Color color;
		}

		[Header("Detection")]
		public LayerMask ledgeLayers = ~0;
		public bool requireLedgeMarker = false;
		public Transform forwardReference;
		public float maxUpwardSpeedToGrab = 2f;
		public float regrabCooldown = 0.2f;
		public float topCheckHeight = 1.8f;
		public float topCheckForwardOffset = 0.45f;
		public float topCheckRadius = 0.18f;
		public float topCheckDistance = 0.75f;
		public float topSurfaceMaxAngle = 25f;
		public float frontCheckHeight = 1.25f;
		public float frontCheckRadius = 0.18f;
		public float frontCheckDistance = 0.6f;
		public float wallFacingMaxAngle = 25f;
		public float wallVerticalAngleTolerance = 20f;
		public float minimumLedgeHeight = 0.35f;
		public float maxSnapDistance = 1.35f;

		[Header("Hang")]
		public float hangDistanceFromWall = 0.45f;
		public float hangVerticalOffset = 1.2f;
		public float hangSnapSpeed = 14f;
		public float faceWallTurnSpeed = 1080f;
		public float ledgeMoveSpeed = 2.5f;
		public float horizontalInputDeadZone = 0.15f;
		public float sameWallNormalTolerance = 25f;
		public float hangPositionSnapDistance = 0.02f;

		[Header("Release")]
		public KeyCode dropKey = KeyCode.LeftShift;
		public float jumpInputBufferDuration = 0.15f;
		public float dropAwaySpeed = 1.5f;
		public float jumpOffUpwardSpeed = 7f;
		public float jumpOffAwaySpeed = 2.5f;

		[Header("Debug")]
		public bool drawDebug = false;

		AdvancedWalkerController controller;
		Mover mover;
		CharacterInput characterInput;
		Rigidbody rig;
		Transform tr;
		TurnTowardControllerVelocity[] turnTowardVelocityComponents;
		bool[] turnTowardVelocityStates;

		bool isHanging = false;
		bool jumpKeyIsPressed = false;
		bool jumpKeyWasPressed = false;
		bool dropKeyIsPressed = false;
		bool dropKeyWasPressed = false;

		Vector3 targetHangPoint = Vector3.zero;
		Vector3 currentWallNormal = Vector3.zero;
		Vector3 lastProbeForward = Vector3.zero;
		float lastJumpPressedTime = Mathf.NegativeInfinity;
		float nextAllowedGrabTime = 0f;
		DebugCastInfo topDebugCast;
		DebugCastInfo frontDebugCast;

		void Awake()
		{
			controller = GetComponent<AdvancedWalkerController>();
			mover = GetComponent<Mover>();
			characterInput = GetComponent<CharacterInput>();
			rig = GetComponent<Rigidbody>();
			tr = transform;
			turnTowardVelocityComponents = GetComponentsInChildren<TurnTowardControllerVelocity>(true);
			turnTowardVelocityStates = new bool[turnTowardVelocityComponents.Length];
		}

		void OnValidate()
		{
			topCheckHeight = Mathf.Max(0f, topCheckHeight);
			topCheckForwardOffset = Mathf.Max(0f, topCheckForwardOffset);
			topCheckRadius = Mathf.Max(0.01f, topCheckRadius);
			topCheckDistance = Mathf.Max(0.01f, topCheckDistance);
			frontCheckHeight = Mathf.Max(0f, frontCheckHeight);
			frontCheckRadius = Mathf.Max(0.01f, frontCheckRadius);
			frontCheckDistance = Mathf.Max(0.01f, frontCheckDistance);
			minimumLedgeHeight = Mathf.Max(0f, minimumLedgeHeight);
			maxSnapDistance = Mathf.Max(0.01f, maxSnapDistance);
			hangDistanceFromWall = Mathf.Max(0.01f, hangDistanceFromWall);
			hangVerticalOffset = Mathf.Max(0.01f, hangVerticalOffset);
			hangSnapSpeed = Mathf.Max(0.01f, hangSnapSpeed);
			faceWallTurnSpeed = Mathf.Max(0f, faceWallTurnSpeed);
			ledgeMoveSpeed = Mathf.Max(0f, ledgeMoveSpeed);
			horizontalInputDeadZone = Mathf.Clamp01(horizontalInputDeadZone);
			hangPositionSnapDistance = Mathf.Max(0.001f, hangPositionSnapDistance);
			regrabCooldown = Mathf.Max(0f, regrabCooldown);
			jumpInputBufferDuration = Mathf.Max(0f, jumpInputBufferDuration);
		}

		void Update()
		{
			bool _newJumpKeyState = (characterInput != null) && characterInput.IsJumpKeyPressed();
			jumpKeyWasPressed = !jumpKeyIsPressed && _newJumpKeyState;
			if(jumpKeyWasPressed)
				lastJumpPressedTime = Time.time;
			jumpKeyIsPressed = _newJumpKeyState;

			bool _newDropKeyState = Input.GetKey(dropKey);
			dropKeyWasPressed = !dropKeyIsPressed && _newDropKeyState;
			dropKeyIsPressed = _newDropKeyState;
		}

		void FixedUpdate()
		{
			if(isHanging)
			{
				HandleLedgeHang();
				return;
			}

			TryBeginLedgeHang();
		}

		void OnDisable()
		{
			if(!isHanging)
				return;

			isHanging = false;

			if(gameObject.activeInHierarchy && controller != null)
			{
				controller.SetMomentum(Vector3.zero);
				controller.ResetJumpInputState();
				controller.enabled = true;
			}

			if(rig != null)
			{
				rig.linearVelocity = Vector3.zero;
				rig.angularVelocity = Vector3.zero;
			}

			RestoreTurnTowardComponents();
		}

		void OnDrawGizmos()
		{
			if(!drawDebug)
				return;

			DrawCastGizmo(topDebugCast);
			DrawCastGizmo(frontDebugCast);

			if(isHanging || targetHangPoint != Vector3.zero)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere(targetHangPoint, 0.06f);
			}
		}

		void TryBeginLedgeHang()
		{
			if(controller == null || mover == null || rig == null)
				return;

			if(!controller.enabled || controller.IsGrounded())
				return;

			if(Time.time < nextAllowedGrabTime)
				return;

			float _verticalSpeed = VectorMath.GetDotProduct(controller.GetVelocity(), tr.up);
			if(_verticalSpeed > maxUpwardSpeedToGrab)
				return;

			if(!TryGetLedgeFromAvailableDirections(tr.position, out LedgeData _ledgeData))
				return;

			if((_ledgeData.hangPoint - tr.position).sqrMagnitude > (maxSnapDistance * maxSnapDistance))
				return;

			BeginLedgeHang(_ledgeData);
		}

		void BeginLedgeHang(LedgeData _ledgeData)
		{
			isHanging = true;
			targetHangPoint = _ledgeData.hangPoint;
			currentWallNormal = _ledgeData.wallNormal;
			lastProbeForward = -currentWallNormal;
			lastJumpPressedTime = Mathf.NegativeInfinity;

			controller.SetMomentum(Vector3.zero);
			controller.ResetJumpInputState();
			controller.enabled = false;
			DisableTurnTowardComponents();

			rig.linearVelocity = Vector3.zero;
			rig.angularVelocity = Vector3.zero;
			SnapToWallRotation();
		}

		void HandleLedgeHang()
		{
			if(dropKeyWasPressed)
			{
				ReleaseFromLedge(currentWallNormal * dropAwaySpeed);
				return;
			}

			if(HasBufferedJumpRequest())
			{
				lastJumpPressedTime = Mathf.NegativeInfinity;
				ReleaseFromLedge((tr.up * jumpOffUpwardSpeed) + (currentWallNormal * jumpOffAwaySpeed));
				return;
			}

			if(!TryGetLedge(rig.position, -currentWallNormal, out LedgeData _currentLedge))
			{
				ReleaseFromLedge(Vector3.zero);
				return;
			}

			currentWallNormal = _currentLedge.wallNormal;
			targetHangPoint = _currentLedge.hangPoint;

			UpdateSideMovementTarget();
			MoveTowardHangPoint();
			UpdateHangRotation();
		}

		void UpdateSideMovementTarget()
		{
			if(characterInput == null)
				return;

			float _horizontalInput = characterInput.GetHorizontalMovementInput();
			if(Mathf.Abs(_horizontalInput) < horizontalInputDeadZone)
				return;

			Vector3 _ledgeTangent = Vector3.Cross(currentWallNormal, tr.up).normalized;
			if(_ledgeTangent.sqrMagnitude <= 0.0001f)
				return;

			float _sideDistance = ledgeMoveSpeed * Mathf.Abs(_horizontalInput) * Time.fixedDeltaTime;
			Vector3 _candidatePosition = targetHangPoint + (_ledgeTangent * Mathf.Sign(_horizontalInput) * _sideDistance);

			if(!TryGetLedge(_candidatePosition, -currentWallNormal, out LedgeData _sideLedge))
				return;

			if(Vector3.Angle(_sideLedge.wallNormal, currentWallNormal) > sameWallNormalTolerance)
				return;

			targetHangPoint = _sideLedge.hangPoint;
			currentWallNormal = _sideLedge.wallNormal;
		}

		void MoveTowardHangPoint()
		{
			Vector3 _toTarget = targetHangPoint - rig.position;
			float _snapDistance = hangPositionSnapDistance * hangPositionSnapDistance;

			if(_toTarget.sqrMagnitude <= _snapDistance)
			{
				rig.position = targetHangPoint;
				rig.linearVelocity = Vector3.zero;
				rig.angularVelocity = Vector3.zero;
				return;
			}

			Vector3 _desiredVelocity = _toTarget / Time.fixedDeltaTime;
			rig.linearVelocity = Vector3.ClampMagnitude(_desiredVelocity, hangSnapSpeed);
			rig.angularVelocity = Vector3.zero;
		}

		void UpdateHangRotation()
		{
			Vector3 _lookDirection = Vector3.ProjectOnPlane(-currentWallNormal, tr.up);
			if(_lookDirection.sqrMagnitude <= 0.0001f)
				return;

			Quaternion _targetRotation = Quaternion.LookRotation(_lookDirection.normalized, tr.up);
			if(faceWallTurnSpeed <= 0f)
			{
				tr.rotation = _targetRotation;
				return;
			}

			tr.rotation = Quaternion.RotateTowards(tr.rotation, _targetRotation, faceWallTurnSpeed * Time.fixedDeltaTime);
		}

		void SnapToWallRotation()
		{
			Vector3 _lookDirection = Vector3.ProjectOnPlane(-currentWallNormal, tr.up);
			if(_lookDirection.sqrMagnitude <= 0.0001f)
				return;

			tr.rotation = Quaternion.LookRotation(_lookDirection.normalized, tr.up);
		}

		bool HasBufferedJumpRequest()
		{
			if(jumpInputBufferDuration <= 0f)
				return jumpKeyWasPressed;

			return (Time.time - lastJumpPressedTime) <= jumpInputBufferDuration;
		}

		void ReleaseFromLedge(Vector3 _releaseVelocity)
		{
			isHanging = false;
			nextAllowedGrabTime = Time.time + regrabCooldown;

			controller.SetMomentum(_releaseVelocity);
			controller.ResetJumpInputState();
			controller.enabled = true;
			RestoreTurnTowardComponents();

			rig.linearVelocity = _releaseVelocity;
			rig.angularVelocity = Vector3.zero;

			if(_releaseVelocity.sqrMagnitude > 0.0001f)
				lastProbeForward = Vector3.ProjectOnPlane(_releaseVelocity, tr.up).normalized;
		}

		bool TryGetLedgeFromAvailableDirections(Vector3 _rootPosition, out LedgeData _ledgeData)
		{
			_ledgeData = default(LedgeData);

			Vector3 _velocityDirection = Vector3.ProjectOnPlane(controller.GetVelocity(), tr.up);
			if(TryGetLedge(_rootPosition, _velocityDirection, out _ledgeData))
			{
				lastProbeForward = _velocityDirection.normalized;
				return true;
			}

			Vector3 _inputDirection = CalculateInputDirection();
			if(TryGetLedge(_rootPosition, _inputDirection, out _ledgeData))
			{
				lastProbeForward = _inputDirection.normalized;
				return true;
			}

			if(forwardReference != null)
			{
				Vector3 _referenceForward = Vector3.ProjectOnPlane(forwardReference.forward, tr.up);
				if(TryGetLedge(_rootPosition, _referenceForward, out _ledgeData))
				{
					lastProbeForward = _referenceForward.normalized;
					return true;
				}
			}

			Vector3 _transformForward = Vector3.ProjectOnPlane(tr.forward, tr.up);
			if(TryGetLedge(_rootPosition, _transformForward, out _ledgeData))
			{
				lastProbeForward = _transformForward.normalized;
				return true;
			}

			if(lastProbeForward.sqrMagnitude > 0.0001f && TryGetLedge(_rootPosition, lastProbeForward, out _ledgeData))
				return true;

			return false;
		}

		bool TryGetLedge(Vector3 _rootPosition, Vector3 _probeForward, out LedgeData _ledgeData)
		{
			_ledgeData = default(LedgeData);

			_probeForward = Vector3.ProjectOnPlane(_probeForward, tr.up);
			if(_probeForward.sqrMagnitude <= 0.0001f)
				return false;

			_probeForward.Normalize();

			Vector3 _topCheckOrigin = _rootPosition + (tr.up * topCheckHeight) + (_probeForward * topCheckForwardOffset);
			if(!SphereCastFiltered(_topCheckOrigin, topCheckRadius, -tr.up, topCheckDistance, out RaycastHit _topHit))
			{
				RecordDebugCast(ref topDebugCast, _topCheckOrigin, -tr.up, topCheckRadius, topCheckDistance, false, default(RaycastHit), Color.red);
				return false;
			}

			RecordDebugCast(ref topDebugCast, _topCheckOrigin, -tr.up, topCheckRadius, topCheckDistance, true, _topHit, Color.green);

			if(Vector3.Angle(_topHit.normal, tr.up) > topSurfaceMaxAngle)
				return false;

			Vector3 _frontCheckOrigin = _rootPosition + (tr.up * frontCheckHeight);
			if(!SphereCastFiltered(_frontCheckOrigin, frontCheckRadius, _probeForward, frontCheckDistance, out RaycastHit _wallHit))
			{
				RecordDebugCast(ref frontDebugCast, _frontCheckOrigin, _probeForward, frontCheckRadius, frontCheckDistance, false, default(RaycastHit), Color.red);
				return false;
			}

			RecordDebugCast(ref frontDebugCast, _frontCheckOrigin, _probeForward, frontCheckRadius, frontCheckDistance, true, _wallHit, Color.cyan);

			if(!IsWallSurfaceValid(_rootPosition, _wallHit.point, _wallHit.normal, _probeForward))
				return false;

			float _ledgeHeight = VectorMath.GetDotProduct(_topHit.point - _wallHit.point, tr.up);
			if(_ledgeHeight < minimumLedgeHeight)
				return false;

			Vector3 _topAlignedPoint = _topHit.point - (tr.up * hangVerticalOffset);
			Vector3 _wallAlignedPoint = _wallHit.point + (_wallHit.normal * hangDistanceFromWall);
			Vector3 _horizontalOffset = VectorMath.RemoveDotVector(_wallAlignedPoint - _topAlignedPoint, tr.up);

			_ledgeData.hangPoint = _topAlignedPoint + _horizontalOffset;
			_ledgeData.wallNormal = _wallHit.normal.normalized;
			return true;
		}

		bool IsWallSurfaceValid(Vector3 _rootPosition, Vector3 _wallPoint, Vector3 _wallNormal, Vector3 _probeForward)
		{
			float _verticalAngle = Vector3.Angle(_wallNormal, tr.up);
			if(Mathf.Abs(_verticalAngle - 90f) > wallVerticalAngleTolerance)
				return false;

			Vector3 _toCharacter = Vector3.ProjectOnPlane(_rootPosition - _wallPoint, tr.up);
			if(_toCharacter.sqrMagnitude > 0.0001f && Vector3.Angle(_wallNormal, _toCharacter.normalized) <= wallFacingMaxAngle)
				return true;

			return Vector3.Angle(_wallNormal, -_probeForward) <= wallFacingMaxAngle;
		}

		Vector3 GetProbeForward()
		{
			Vector3 _inputDirection = CalculateInputDirection();
			if(_inputDirection.sqrMagnitude > 0.0001f)
			{
				lastProbeForward = _inputDirection.normalized;
				return lastProbeForward;
			}

			Vector3 _horizontalVelocity = VectorMath.RemoveDotVector(controller.GetVelocity(), tr.up);
			if(_horizontalVelocity.sqrMagnitude > 0.0001f)
			{
				lastProbeForward = _horizontalVelocity.normalized;
				return lastProbeForward;
			}

			if(lastProbeForward.sqrMagnitude > 0.0001f)
				return lastProbeForward;

			if(forwardReference != null)
			{
				Vector3 _forward = Vector3.ProjectOnPlane(forwardReference.forward, tr.up);
				if(_forward.sqrMagnitude > 0.0001f)
				{
					lastProbeForward = _forward.normalized;
					return lastProbeForward;
				}
			}

			Vector3 _fallbackForward = Vector3.ProjectOnPlane(tr.forward, tr.up);
			if(_fallbackForward.sqrMagnitude > 0.0001f)
			{
				lastProbeForward = _fallbackForward.normalized;
				return lastProbeForward;
			}

			return Vector3.zero;
		}

		Vector3 CalculateInputDirection()
		{
			if(characterInput == null)
				return Vector3.zero;

			float _horizontalInput = characterInput.GetHorizontalMovementInput();
			float _verticalInput = characterInput.GetVerticalMovementInput();

			Vector3 _direction = Vector3.zero;
			if(controller.cameraTransform == null)
			{
				_direction += tr.right * _horizontalInput;
				_direction += tr.forward * _verticalInput;
			}
			else
			{
				_direction += Vector3.ProjectOnPlane(controller.cameraTransform.right, tr.up).normalized * _horizontalInput;
				_direction += Vector3.ProjectOnPlane(controller.cameraTransform.forward, tr.up).normalized * _verticalInput;
			}

			if(_direction.sqrMagnitude > 1f)
				_direction.Normalize();

			return _direction;
		}

		bool SphereCastFiltered(Vector3 _origin, float _radius, Vector3 _direction, float _distance, out RaycastHit _closestHit)
		{
			_closestHit = default(RaycastHit);
			RaycastHit[] _hits = Physics.SphereCastAll(
				_origin,
				_radius,
				_direction,
				_distance,
				ledgeLayers,
				QueryTriggerInteraction.Ignore
			);

			bool _hasHit = false;
			float _closestDistance = float.MaxValue;

			for(int i = 0; i < _hits.Length; i++)
			{
				Collider _collider = _hits[i].collider;
				if(_collider == null)
					continue;

				if(_collider.transform == tr || _collider.transform.IsChildOf(tr))
					continue;

				if(!IsLedgeColliderAllowed(_collider))
					continue;

				if(_hits[i].distance >= _closestDistance)
					continue;

				_closestDistance = _hits[i].distance;
				_closestHit = _hits[i];
				_hasHit = true;
			}

			return _hasHit;
		}

		bool IsLedgeColliderAllowed(Collider _collider)
		{
			if(!requireLedgeMarker)
				return true;

			LedgeHangSurface _surface = _collider.GetComponentInParent<LedgeHangSurface>();
			return _surface != null && _surface.allowHanging;
		}

		void DisableTurnTowardComponents()
		{
			for(int i = 0; i < turnTowardVelocityComponents.Length; i++)
			{
				if(turnTowardVelocityComponents[i] == null)
					continue;

				turnTowardVelocityStates[i] = turnTowardVelocityComponents[i].enabled;
				turnTowardVelocityComponents[i].enabled = false;
				turnTowardVelocityComponents[i].transform.localRotation = Quaternion.identity;
			}
		}

		void RestoreTurnTowardComponents()
		{
			for(int i = 0; i < turnTowardVelocityComponents.Length; i++)
			{
				if(turnTowardVelocityComponents[i] == null)
					continue;

				turnTowardVelocityComponents[i].enabled = turnTowardVelocityStates[i];
			}
		}

		void RecordDebugCast(ref DebugCastInfo _info, Vector3 _origin, Vector3 _direction, float _radius, float _distance, bool _hasHit, RaycastHit _hit, Color _color)
		{
			_info.isValid = true;
			_info.hasHit = _hasHit;
			_info.origin = _origin;
			_info.direction = _direction.normalized;
			_info.radius = _radius;
			_info.distance = _distance;
			_info.hitDistance = _hasHit ? _hit.distance : _distance;
			_info.hitPoint = _hit.point;
			_info.hitNormal = _hit.normal;
			_info.color = _color;

			if(!drawDebug)
				return;

			Debug.DrawLine(_origin, _origin + (_direction.normalized * _info.hitDistance), _color);
		}

		void DrawCastGizmo(DebugCastInfo _info)
		{
			if(!_info.isValid)
				return;

			Vector3 _direction = _info.direction.sqrMagnitude > 0f ? _info.direction.normalized : Vector3.forward;
			Vector3 _end = _info.origin + (_direction * _info.distance);
			Vector3 _hitEnd = _info.origin + (_direction * _info.hitDistance);

			Gizmos.color = _info.color;
			Gizmos.DrawWireSphere(_info.origin, _info.radius);
			Gizmos.DrawLine(_info.origin, _end);
			Gizmos.DrawWireSphere(_end, _info.radius);

			if(!_info.hasHit)
				return;

			Gizmos.DrawWireSphere(_hitEnd, _info.radius);
			Gizmos.DrawSphere(_info.hitPoint, 0.035f);
			Gizmos.DrawLine(_info.hitPoint, _info.hitPoint + (_info.hitNormal.normalized * 0.3f));
		}
	}
}
