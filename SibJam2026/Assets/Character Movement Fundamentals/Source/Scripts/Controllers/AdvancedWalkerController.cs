using System.Collections;
using UnityEngine;

namespace CMF
{
	//Advanced walker controller script;
	//This controller is used as a basis for other controller types ('SidescrollerController');
	//Custom movement input can be implemented by creating a new script that inherits 'AdvancedWalkerController' and overriding the 'CalculateMovementDirection' function;
	public class AdvancedWalkerController : Controller {

		//References to attached components;
		protected Transform tr;
		protected Mover mover;
		protected CharacterInput characterInput;
		protected CeilingDetector ceilingDetector;

        //Jump key variables;
        bool jumpInputIsLocked = false;
        bool jumpKeyWasPressed = false;
		bool jumpKeyWasLetGo = false;
		bool jumpKeyIsPressed = false;

		//Movement speed;
		public float movementSpeed = 7f;
		[Tooltip("Ground acceleration in units per second. Lower values produce a slower ramp-up to full speed.")]
		public float acceleration = 60f;
		[Tooltip("Ground deceleration in units per second. Lower values produce a longer slide before stopping.")]
		public float deceleration = 80f;

		//How fast the controller can change direction while in the air;
		//Higher values result in more air control;
		public float airControlRate = 2f;

		//Jump speed;
		public float jumpSpeed = 10f;

		//Jump duration variables;
		public float jumpDuration = 0.2f;
		float currentJumpStartTime = 0f;

		[SerializeField]
		private float jumpBufferDuration = 0.2f;
		[SerializeField]
		private float coyoteTimeDuration = 0.1f;
		float lastJumpButtonPressedTime = Mathf.NegativeInfinity;
		float lastTimeGrounded = Mathf.NegativeInfinity;

		//'AirFriction' determines how fast the controller loses its momentum while in the air;
		//'GroundFriction' is used instead, if the controller is grounded;
		public float airFriction = 0.5f;
		public float groundFriction = 100f;

		//Current momentum;
		protected Vector3 momentum = Vector3.zero;

		//Saved velocity from last frame;
		Vector3 savedVelocity = Vector3.zero;

		//Saved horizontal movement velocity from last frame;
		Vector3 savedMovementVelocity = Vector3.zero;
		Vector3 currentMovementVelocity = Vector3.zero;
		bool acceleratingFromRest = false;
		SlipperySurface currentSlipperySurface = null;

		//Amount of downward gravity;
		public float gravity = 30f;
		[Tooltip("How fast the character will slide down steep slopes.")]
		public float slideGravity = 5f;
		
		//Acceptable slope angle limit;
		public float slopeLimit = 80f;

		[Tooltip("Whether to calculate and apply momentum relative to the controller's transform.")]
		public bool useLocalMomentum = false;

		//Enum describing basic controller states; 
		public enum ControllerState
		{
			Grounded,
			Sliding,
			Falling,
			Rising,
			Jumping
		}
		
		ControllerState currentControllerState = ControllerState.Falling;

		[Tooltip("Optional camera transform used for calculating movement direction. If assigned, character movement will take camera view into account.")]
		public Transform cameraTransform;
		
		//Get references to all necessary components;
		void Awake () {
			mover = GetComponent<Mover>();
			tr = transform;
			characterInput = GetComponent<CharacterInput>();
			ceilingDetector = GetComponent<CeilingDetector>();

			if(characterInput == null)
				Debug.LogWarning("No character input script has been attached to this gameobject", this.gameObject);

			Setup();
		}

		//This function is called right after Awake(); It can be overridden by inheriting scripts;
		protected virtual void Setup()
		{
		}

		void Update()
		{
			HandleJumpKeyInput();
		}

		void OnDisable()
		{
			ResetJumpInputState();
		}

        //Handle jump booleans for later use in FixedUpdate;
        void HandleJumpKeyInput()
        {
            bool _newJumpKeyPressedState = IsJumpKeyPressed();

            if (jumpKeyIsPressed == false && _newJumpKeyPressedState == true)
                jumpKeyWasPressed = true;

            if (jumpKeyIsPressed == true && _newJumpKeyPressedState == false)
            {
                jumpKeyWasLetGo = true;
                jumpInputIsLocked = false;
            }

            jumpKeyIsPressed = _newJumpKeyPressedState;
        }

        void FixedUpdate()
		{
			ControllerUpdate();
		}

		//Update controller;
		//This function must be called every fixed update, in order for the controller to work correctly;
		void ControllerUpdate()
		{
			//Check if mover is grounded;
			mover.CheckForGround();

			//Determine controller state;
			currentControllerState = DetermineControllerState();

			if(currentControllerState == ControllerState.Grounded)
				lastTimeGrounded = Time.time;

			UpdateGroundSurfaceState();

			UpdateMovementVelocity();

			//Apply friction and gravity to 'momentum';
			HandleMomentum();

			//Check if the player has initiated a jump;
			HandleJumping();

			//Calculate movement velocity;
			Vector3 _velocity = Vector3.zero;
			if(currentControllerState == ControllerState.Grounded)
				_velocity = currentMovementVelocity;
			
			//If local momentum is used, transform momentum into world space first;
			Vector3 _worldMomentum = momentum;
			if(useLocalMomentum)
				_worldMomentum = tr.localToWorldMatrix * momentum;

			//Add current momentum to velocity;
			_velocity += _worldMomentum;
			
			//If player is grounded or sliding on a slope, extend mover's sensor range;
			//This enables the player to walk up/down stairs and slopes without losing ground contact;
			mover.SetExtendSensorRange(IsGrounded());

			//Set mover velocity;		
			mover.SetVelocity(_velocity);

			//Store velocity for next frame;
			savedVelocity = _velocity;
		
			//Save controller movement velocity;
			if(IsOnSlipperySurface())
				savedMovementVelocity = VectorMath.RemoveDotVector(_velocity, tr.up);
			else if(currentControllerState == ControllerState.Grounded)
				savedMovementVelocity = currentMovementVelocity;
			else
				savedMovementVelocity = CalculateMovementVelocity();

			//Reset jump key booleans;
			jumpKeyWasLetGo = false;
			jumpKeyWasPressed = false;

			//Reset ceiling detector, if one is attached to this gameobject;
			if(ceilingDetector != null)
				ceilingDetector.ResetFlags();
		}

		//Calculate and return movement direction based on player input;
		//This function can be overridden by inheriting scripts to implement different player controls;
		protected virtual Vector3 CalculateMovementDirection()
		{
			//If no character input script is attached to this object, return;
			if(characterInput == null)
				return Vector3.zero;

			Vector3 _velocity = Vector3.zero;

			//If no camera transform has been assigned, use the character's transform axes to calculate the movement direction;
			if(cameraTransform == null)
			{
				_velocity += tr.right * characterInput.GetHorizontalMovementInput();
				_velocity += tr.forward * characterInput.GetVerticalMovementInput();
			}
			else
			{
				//If a camera transform has been assigned, use the assigned transform's axes for movement direction;
				//Project movement direction so movement stays parallel to the ground;
				_velocity += Vector3.ProjectOnPlane(cameraTransform.right, tr.up).normalized * characterInput.GetHorizontalMovementInput();
				_velocity += Vector3.ProjectOnPlane(cameraTransform.forward, tr.up).normalized * characterInput.GetVerticalMovementInput();
			}

			//If necessary, clamp movement vector to magnitude of 1f;
			if(_velocity.magnitude > 1f)
				_velocity.Normalize();

			return _velocity;
		}

		//Calculate and return movement velocity based on player input, controller state, ground normal [...];
		protected virtual Vector3 CalculateMovementVelocity()
		{
			//Calculate (normalized) movement direction;
			Vector3 _velocity = CalculateMovementDirection();

			//Multiply (normalized) velocity with movement speed;
			_velocity *= movementSpeed;

			return _velocity;
		}

		void UpdateMovementVelocity()
		{
			Vector3 _targetMovementVelocity = CalculateMovementVelocity();

			if(IsOnSlipperySurface())
			{
				currentMovementVelocity = Vector3.zero;
				acceleratingFromRest = false;
				return;
			}

			if(currentControllerState == ControllerState.Grounded)
			{
				if(_targetMovementVelocity.sqrMagnitude <= 0f)
				{
					acceleratingFromRest = false;

					if(deceleration <= 0f)
					{
						return;
					}

					currentMovementVelocity = VectorMath.IncrementVectorTowardTargetVector(
						currentMovementVelocity,
						deceleration,
						Time.deltaTime,
						Vector3.zero
					);
					return;
				}

				if(!acceleratingFromRest && IsStartingFromRest())
					acceleratingFromRest = true;

				if(acceleratingFromRest)
				{
					if(acceleration <= 0f)
						return;

					currentMovementVelocity = VectorMath.IncrementVectorTowardTargetVector(
						currentMovementVelocity,
						acceleration,
						Time.deltaTime,
						_targetMovementVelocity
					);

					if((currentMovementVelocity - _targetMovementVelocity).sqrMagnitude <= 0.0001f)
					{
						currentMovementVelocity = _targetMovementVelocity;
						acceleratingFromRest = false;
					}

					return;
				}

				currentMovementVelocity = _targetMovementVelocity;
				acceleratingFromRest = false;
			}
			else
			{
				acceleratingFromRest = false;
			}
		}

		void UpdateGroundSurfaceState()
		{
			SlipperySurface _previousSlipperySurface = currentSlipperySurface;

			if(currentControllerState != ControllerState.Grounded && currentControllerState != ControllerState.Sliding)
			{
				currentSlipperySurface = null;
				return;
			}

			Collider _groundCollider = mover.GetGroundCollider();
			if(_groundCollider == null)
			{
				currentSlipperySurface = null;
				return;
			}

			currentSlipperySurface = _groundCollider.GetComponentInParent<SlipperySurface>();

			if(_previousSlipperySurface == null && currentSlipperySurface != null && currentControllerState == ControllerState.Grounded)
			{
				if(currentMovementVelocity.sqrMagnitude > 0.0001f)
					AddMomentum(currentMovementVelocity);

				currentMovementVelocity = Vector3.zero;
				acceleratingFromRest = false;
			}
		}

		bool IsOnSlipperySurface()
		{
			if(currentSlipperySurface == null)
				return false;

			return (currentControllerState == ControllerState.Grounded || currentControllerState == ControllerState.Sliding);
		}

		bool IsStartingFromRest()
		{
			float _movementThreshold = 0.01f;

			if(currentMovementVelocity.sqrMagnitude > (_movementThreshold * _movementThreshold))
				return false;

			Vector3 _horizontalMomentum = VectorMath.RemoveDotVector(GetMomentum(), tr.up);
			if(_horizontalMomentum.sqrMagnitude > (_movementThreshold * _movementThreshold))
				return false;

			return true;
		}

		//Returns 'true' if the player presses the jump key;
		protected virtual bool IsJumpKeyPressed()
		{
			//If no character input script is attached to this object, return;
			if(characterInput == null)
				return false;

			return characterInput.IsJumpKeyPressed();
		}

		public void ResetJumpInputState()
		{
			jumpInputIsLocked = false;
			jumpKeyWasPressed = false;
			jumpKeyWasLetGo = false;
			jumpKeyIsPressed = IsJumpKeyPressed();
			lastJumpButtonPressedTime = Mathf.NegativeInfinity;
		}

		//Determine current controller state based on current momentum and whether the controller is grounded (or not);
		//Handle state transitions;
		ControllerState DetermineControllerState()
		{
			//Check if vertical momentum is pointing upwards;
			bool _isRising = IsRisingOrFalling() && (VectorMath.GetDotProduct(GetMomentum(), tr.up) > 0f);
			//Check if controller is sliding;
			bool _isSliding = mover.IsGrounded() && IsGroundTooSteep();
			
			//Grounded;
			if(currentControllerState == ControllerState.Grounded)
			{
				if(_isRising){
					OnGroundContactLost();
					return ControllerState.Rising;
				}
				if(!mover.IsGrounded()){
					OnGroundContactLost();
					return ControllerState.Falling;
				}
				if(_isSliding){
					OnGroundContactLost();
					return ControllerState.Sliding;
				}
				return ControllerState.Grounded;
			}

			//Falling;
			if(currentControllerState == ControllerState.Falling)
			{
				if(_isRising){
					return ControllerState.Rising;
				}
				if(mover.IsGrounded() && !_isSliding){
					OnGroundContactRegained();
					return ControllerState.Grounded;
				}
				if(_isSliding){
					return ControllerState.Sliding;
				}
				return ControllerState.Falling;
			}
			
			//Sliding;
			if(currentControllerState == ControllerState.Sliding)
			{	
				if(_isRising){
					OnGroundContactLost();
					return ControllerState.Rising;
				}
				if(!mover.IsGrounded()){
					OnGroundContactLost();
					return ControllerState.Falling;
				}
				if(mover.IsGrounded() && !_isSliding){
					OnGroundContactRegained();
					return ControllerState.Grounded;
				}
				return ControllerState.Sliding;
			}

			//Rising;
			if(currentControllerState == ControllerState.Rising)
			{
				if(!_isRising){
					if(mover.IsGrounded() && !_isSliding){
						OnGroundContactRegained();
						return ControllerState.Grounded;
					}
					if(_isSliding){
						return ControllerState.Sliding;
					}
					if(!mover.IsGrounded()){
						return ControllerState.Falling;
					}
				}

				//If a ceiling detector has been attached to this gameobject, check for ceiling hits;
				if(ceilingDetector != null)
				{
					if(ceilingDetector.HitCeiling())
					{
						OnCeilingContact();
						return ControllerState.Falling;
					}
				}
				return ControllerState.Rising;
			}

			//Jumping;
			if(currentControllerState == ControllerState.Jumping)
			{
				//Check for jump timeout;
				if((Time.time - currentJumpStartTime) > jumpDuration)
					return ControllerState.Rising;

				//Stop extending the jump as soon as the key is no longer held;
				if(jumpKeyWasLetGo || !jumpKeyIsPressed)
					return ControllerState.Rising;

				//If a ceiling detector has been attached to this gameobject, check for ceiling hits;
				if(ceilingDetector != null)
				{
					if(ceilingDetector.HitCeiling())
					{
						OnCeilingContact();
						return ControllerState.Falling;
					}
				}
				return ControllerState.Jumping;
			}
			
			return ControllerState.Falling;
		}

		//Check if player has initiated a jump;
		void HandleJumping()
		{
			if(jumpKeyWasPressed && !jumpInputIsLocked)
				lastJumpButtonPressedTime = Time.time;

			if(!HasBufferedJumpRequest())
				return;

			if(currentControllerState == ControllerState.Grounded)
			{
				StartJump(true);
				return;
			}

			if(CanUseCoyoteJump())
				StartJump(false);
		}

		bool HasBufferedJumpRequest()
		{
			if(jumpInputIsLocked)
				return false;

			return (Time.time - lastJumpButtonPressedTime) <= jumpBufferDuration;
		}

		bool CanUseCoyoteJump()
		{
			if(currentControllerState != ControllerState.Falling)
				return false;

			return (Time.time - lastTimeGrounded) <= coyoteTimeDuration;
		}

		void StartJump(bool notifyGroundContactLost)
		{
			if(notifyGroundContactLost)
				OnGroundContactLost();

			lastJumpButtonPressedTime = Mathf.NegativeInfinity;
			lastTimeGrounded = Mathf.NegativeInfinity;
			OnJumpStart();
			currentControllerState = ControllerState.Jumping;
		}

        //Apply friction to both vertical and horizontal momentum based on 'friction' and 'gravity';
        //Handle movement in the air;
        //Handle sliding down steep slopes;
		void HandleMomentum()
		{
			//If local momentum is used, transform momentum into world coordinates first;
			if(useLocalMomentum)
				momentum = tr.localToWorldMatrix * momentum;

			bool _isOnSlipperySurface = IsOnSlipperySurface();

			Vector3 _verticalMomentum = Vector3.zero;
			Vector3 _horizontalMomentum = Vector3.zero;

			//Split momentum into vertical and horizontal components;
			if(momentum != Vector3.zero)
			{
				_verticalMomentum = VectorMath.ExtractDotVector(momentum, tr.up);
				_horizontalMomentum = momentum - _verticalMomentum;
			}

			//Add gravity to vertical momentum;
			_verticalMomentum -= tr.up * gravity * Time.deltaTime;

			//Remove any downward force if the controller is grounded;
			if(currentControllerState == ControllerState.Grounded && VectorMath.GetDotProduct(_verticalMomentum, tr.up) < 0f)
				_verticalMomentum = Vector3.zero;

			//Manipulate momentum to steer controller in the air (if controller is not grounded or sliding);
			if(!IsGrounded())
			{
				Vector3 _movementVelocity = CalculateMovementVelocity();

				//If controller has received additional momentum from somewhere else;
				if(_horizontalMomentum.magnitude > movementSpeed)
				{
					//Prevent unwanted accumulation of speed in the direction of the current momentum;
					if(VectorMath.GetDotProduct(_movementVelocity, _horizontalMomentum.normalized) > 0f)
						_movementVelocity = VectorMath.RemoveDotVector(_movementVelocity, _horizontalMomentum.normalized);
					
					//Lower air control slightly with a multiplier to add some 'weight' to any momentum applied to the controller;
					float _airControlMultiplier = 0.25f;
					_horizontalMomentum += _movementVelocity * Time.deltaTime * airControlRate * _airControlMultiplier;
				}
				//If controller has not received additional momentum;
				else
				{
					//Clamp _horizontal velocity to prevent accumulation of speed;
					_horizontalMomentum += _movementVelocity * Time.deltaTime * airControlRate;
					_horizontalMomentum = Vector3.ClampMagnitude(_horizontalMomentum, movementSpeed);
				}
			}

			//Steer controller on slopes;
			if(currentControllerState == ControllerState.Sliding)
			{
				//Calculate vector pointing away from slope;
				Vector3 _pointDownVector = Vector3.ProjectOnPlane(mover.GetGroundNormal(), tr.up).normalized;

				//Calculate movement velocity;
				Vector3 _slopeMovementVelocity = CalculateMovementVelocity();
				//Remove all velocity that is pointing up the slope;
				_slopeMovementVelocity = VectorMath.RemoveDotVector(_slopeMovementVelocity, _pointDownVector);

				//Add movement velocity to momentum;
				_horizontalMomentum += _slopeMovementVelocity * Time.fixedDeltaTime;
			}

			if(_isOnSlipperySurface)
			{
				Vector3 _groundNormal = mover.GetGroundNormal();
				Vector3 _iceMovementDirection = Vector3.ProjectOnPlane(CalculateMovementDirection(), _groundNormal);
				if(_iceMovementDirection.sqrMagnitude > 0.0001f)
				{
					_horizontalMomentum += _iceMovementDirection.normalized * currentSlipperySurface.inputAcceleration * Time.deltaTime;
				}

				if(currentControllerState == ControllerState.Grounded)
				{
					Vector3 _downSlopeDirection = Vector3.ProjectOnPlane(-tr.up, _groundNormal);
					if(_downSlopeDirection.sqrMagnitude > 0.0001f)
						_horizontalMomentum += _downSlopeDirection.normalized * currentSlipperySurface.downhillAcceleration * Time.deltaTime;
				}

				if(currentSlipperySurface.projectMomentumToSurface)
					_horizontalMomentum = Vector3.ProjectOnPlane(_horizontalMomentum, _groundNormal);
			}

			//Apply friction to horizontal momentum based on whether the controller is grounded;
			if(currentControllerState == ControllerState.Grounded)
			{
				float _effectiveGroundFriction = _isOnSlipperySurface ? currentSlipperySurface.groundFriction : groundFriction;
				_horizontalMomentum = VectorMath.IncrementVectorTowardTargetVector(_horizontalMomentum, _effectiveGroundFriction, Time.deltaTime, Vector3.zero);
			}
			else
				_horizontalMomentum = VectorMath.IncrementVectorTowardTargetVector(_horizontalMomentum, airFriction, Time.deltaTime, Vector3.zero); 

			//Add horizontal and vertical momentum back together;
			momentum = _horizontalMomentum + _verticalMomentum;

			//Additional momentum calculations for sliding;
			if(currentControllerState == ControllerState.Sliding)
			{
				//Project the current momentum onto the current ground normal if the controller is sliding down a slope;
				momentum = Vector3.ProjectOnPlane(momentum, mover.GetGroundNormal());

				//Remove any upwards momentum when sliding;
				if(VectorMath.GetDotProduct(momentum, tr.up) > 0f)
					momentum = VectorMath.RemoveDotVector(momentum, tr.up);

				//Apply additional slide gravity;
				Vector3 _slideDirection = Vector3.ProjectOnPlane(-tr.up, mover.GetGroundNormal()).normalized;
				momentum += _slideDirection * slideGravity * Time.deltaTime;
			}
			
			//If controller is jumping, override vertical velocity with jumpSpeed;
			if(currentControllerState == ControllerState.Jumping)
			{
				momentum = VectorMath.RemoveDotVector(momentum, tr.up);
				momentum += tr.up * jumpSpeed;
			}

			if(useLocalMomentum)
				momentum = tr.worldToLocalMatrix * momentum;
		}

		//Events;

		//This function is called when the player has initiated a jump;
		void OnJumpStart()
		{
			//If local momentum is used, transform momentum into world coordinates first;
			if(useLocalMomentum)
				momentum = tr.localToWorldMatrix * momentum;

			//Add jump force to momentum;
			momentum += tr.up * jumpSpeed;

			//Set jump start time;
			currentJumpStartTime = Time.time;

            //Only keep jump input locked while the key is physically held down;
            jumpInputIsLocked = jumpKeyIsPressed;

            //Call event;
            if (OnJump != null)
				OnJump(momentum);

			if(useLocalMomentum)
				momentum = tr.worldToLocalMatrix * momentum;
		}

		//This function is called when the controller has lost ground contact, i.e. is either falling or rising, or generally in the air;
		void OnGroundContactLost()
		{
			//If local momentum is used, transform momentum into world coordinates first;
			if(useLocalMomentum)
				momentum = tr.localToWorldMatrix * momentum;

			//Get current movement velocity;
			Vector3 _velocity = GetMovementVelocity();

			//Check if the controller has both momentum and a current movement velocity;
			if(_velocity.sqrMagnitude >= 0f && momentum.sqrMagnitude > 0f)
			{
				//Project momentum onto movement direction;
				Vector3 _projectedMomentum = Vector3.Project(momentum, _velocity.normalized);
				//Calculate dot product to determine whether momentum and movement are aligned;
				float _dot = VectorMath.GetDotProduct(_projectedMomentum.normalized, _velocity.normalized);

				//If current momentum is already pointing in the same direction as movement velocity,
				//Don't add further momentum (or limit movement velocity) to prevent unwanted speed accumulation;
				if(_projectedMomentum.sqrMagnitude >= _velocity.sqrMagnitude && _dot > 0f)
					_velocity = Vector3.zero;
				else if(_dot > 0f)
					_velocity -= _projectedMomentum;	
			}

			//Add movement velocity to momentum;
			momentum += _velocity;

			if(useLocalMomentum)
				momentum = tr.worldToLocalMatrix * momentum;
		}

		//This function is called when the controller has landed on a surface after being in the air;
		void OnGroundContactRegained()
		{
			//Call 'OnLand' event;
			if(OnLand != null)
			{
				Vector3 _collisionVelocity = momentum;
				//If local momentum is used, transform momentum into world coordinates first;
				if(useLocalMomentum)
					_collisionVelocity = tr.localToWorldMatrix * _collisionVelocity;

				OnLand(_collisionVelocity);
			}
				
		}

		//This function is called when the controller has collided with a ceiling while jumping or moving upwards;
		void OnCeilingContact()
		{
			//If local momentum is used, transform momentum into world coordinates first;
			if(useLocalMomentum)
				momentum = tr.localToWorldMatrix * momentum;

			//Remove all vertical parts of momentum;
			momentum = VectorMath.RemoveDotVector(momentum, tr.up);

			if(useLocalMomentum)
				momentum = tr.worldToLocalMatrix * momentum;
		}

		//Helper functions;

		//Returns 'true' if vertical momentum is above a small threshold;
		private bool IsRisingOrFalling()
		{
			//Calculate current vertical momentum;
			Vector3 _verticalMomentum = VectorMath.ExtractDotVector(GetMomentum(), tr.up);

			//Setup threshold to check against;
			//For most applications, a value of '0.001f' is recommended;
			float _limit = 0.001f;

			//Return true if vertical momentum is above '_limit';
			return(_verticalMomentum.magnitude > _limit);
		}

		//Returns true if angle between controller and ground normal is too big (> slope limit), i.e. ground is too steep;
		private bool IsGroundTooSteep()
		{
			if(!mover.IsGrounded())
				return true;

			return (Vector3.Angle(mover.GetGroundNormal(), tr.up) > slopeLimit);
		}

		//Getters;

		//Get last frame's velocity;
		public override Vector3 GetVelocity ()
		{
			return savedVelocity;
		}

		//Get last frame's movement velocity (momentum is ignored);
		public override Vector3 GetMovementVelocity()
		{
			return savedMovementVelocity;
		}

		//Get current momentum;
		public Vector3 GetMomentum()
		{
			Vector3 _worldMomentum = momentum;
			if(useLocalMomentum)
				_worldMomentum = tr.localToWorldMatrix * momentum;

			return _worldMomentum;
		}

		//Returns 'true' if controller is grounded (or sliding down a slope);
		public override bool IsGrounded()
		{
			return(currentControllerState == ControllerState.Grounded || currentControllerState == ControllerState.Sliding);
		}

		//Returns 'true' if controller is sliding;
		public bool IsSliding()
		{
			return(currentControllerState == ControllerState.Sliding);
		}

		//Add momentum to controller;
		public void AddMomentum (Vector3 _momentum)
		{
			if(useLocalMomentum)
				momentum = tr.localToWorldMatrix * momentum;

			momentum += _momentum;	

			if(useLocalMomentum)
				momentum = tr.worldToLocalMatrix * momentum;
		}

		//Set controller momentum directly;
		public void SetMomentum(Vector3 _newMomentum)
		{
			if(useLocalMomentum)
				momentum = tr.worldToLocalMatrix * _newMomentum;
			else
				momentum = _newMomentum;
		}
	}
}
