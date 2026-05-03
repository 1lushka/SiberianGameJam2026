using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMF
{
	//This script handles and plays audio cues like footsteps, jump and land audio clips based on character movement speed and events; 
	public class AudioControl : MonoBehaviour {

		//References to components;
		Controller controller;
		Animator animator;
		Mover mover;
		Transform tr;
		public AudioSource audioSource;

		//Whether footsteps will be based on the currently playing animation or calculated based on walked distance (see further below);
		public bool useAnimationBasedFootsteps = true;

		//Velocity threshold for landing sound effect;
		//Sound effect will only be played if downward velocity exceeds this threshold;
		public float landVelocityThreshold = 5f;

		//Footsteps will be played every time the traveled distance reaches this value (if 'useAnimationBasedFootsteps' is set to 'true');
		public float footstepDistance = 0.2f;
		float currentFootstepDistance = 0f;

		private float currentFootStepValue = 0f;

		//Volume of all audio clips;
		[Range(0f, 1f)]
		public float audioClipVolume = 0.1f;

		//Range of random pitch deviation used for short character sounds;
		public float randomPitchRange = 0.05f;

		//Range of random volume deviation used for footsteps;
		//Footstep audio clips will be played at different volumes for a more "natural sounding" result;
		public float relativeRandomizedVolumeRange = 0.2f;

        [Header("Ledge Climb")]
        public float ledgeClimbDistance = 0.25f;                     
        private float ledgeClimbDistanceCounter;

        //Audio clips;
        public AudioClip[] footStepClips;
		public AudioClip[] jumpClips;
		public AudioClip[] landClips;
        public AudioClip[] bounceClips;
        public AudioClip[] ledgeClimbClips;

        public bool isLedgeClimbing { get; set; }
        public Vector3 ledgeMoveVelocity { get; set; }

        //Setup;
        void Start () {
			//Get component references;
			controller = GetComponent<Controller>();
			animator = GetComponentInChildren<Animator>();
			mover = GetComponent<Mover>();
			tr = transform;

			//Connecting events to controller events;
			controller.OnLand += OnLand;
			controller.OnJump += OnJump;

			if(!animator)
				useAnimationBasedFootsteps = false;
		}
		
		//Update;
		void Update () {

			//Get controller velocity;
			Vector3 _velocity = controller.GetVelocity();

			//Calculate horizontal velocity;
			Vector3 _horizontalVelocity = VectorMath.RemoveDotVector(_velocity, tr.up);

			FootStepUpdate(_horizontalVelocity.magnitude);
            LedgeClimbUpdate();
        }

		void FootStepUpdate(float _movementSpeed)
		{
			float _speedThreshold = 0.05f;

			if(useAnimationBasedFootsteps)
			{
				//Get current foot step value from animator;
				float _newFootStepValue = animator.GetFloat("FootStep");

				//Play a foot step audio clip whenever the foot step value changes its sign;
				if((currentFootStepValue <= 0f && _newFootStepValue > 0f) || (currentFootStepValue >= 0f && _newFootStepValue < 0f))
				{
					//Only play footstep sound if mover is grounded and movement speed is above the threshold;
					if(mover.IsGrounded() && _movementSpeed > _speedThreshold)
						PlayFootstepSound(_movementSpeed);
				}
				currentFootStepValue = _newFootStepValue;
			}
			else
			{
				currentFootstepDistance += Time.deltaTime * _movementSpeed;

				//Play foot step audio clip if a certain distance has been traveled;
				if(currentFootstepDistance > footstepDistance)
				{
					//Only play footstep sound if mover is grounded and movement speed is above the threshold;
					if(mover.IsGrounded() && _movementSpeed > _speedThreshold)
						PlayFootstepSound(_movementSpeed);
					currentFootstepDistance = 0f;
				}
			}
		}

        void LedgeClimbUpdate()
        {
            if (!isLedgeClimbing)
            {
                // Если не висим – сбрасываем счётчик
                ledgeClimbDistanceCounter = 0f;
                return;
            }

            float _speed = ledgeMoveVelocity.magnitude;
            float _speedThreshold = 0.05f;
            if (_speed <= _speedThreshold)
                return;

            // Накапливаем расстояние (как в шагах, когда useAnimationBasedFootsteps = false)
            ledgeClimbDistanceCounter += Time.deltaTime * _speed;

            if (ledgeClimbDistanceCounter >= ledgeClimbDistance)
            {
                PlayLedgeClimbSound();
                ledgeClimbDistanceCounter = 0f;
            }
        }

        public void PlayFootstepSound(float _movementSpeed)
		{
			int _footStepClipIndex = Random.Range(0, footStepClips.Length);
			PlayClip(footStepClips[_footStepClipIndex], audioClipVolume + audioClipVolume * Random.Range(-relativeRandomizedVolumeRange, relativeRandomizedVolumeRange));
		}

		void OnLand(Vector3 _v)
		{
			//Only trigger sound if downward velocity exceeds threshold;
			if(VectorMath.GetDotProduct(_v, tr.up) > -landVelocityThreshold)
				return;

			//Play land audio clip;
			PlayRandomClip(landClips, audioClipVolume);
		}

		void OnJump(Vector3 _v)
		{
			//Play jump audio clip;
			PlayRandomClip(jumpClips, audioClipVolume);
		}

		void PlayRandomClip(AudioClip[] _clips, float _volume)
		{
			if(_clips == null || _clips.Length == 0)
				return;

			int _clipIndex = Random.Range(0, _clips.Length);
			PlayClip(_clips[_clipIndex], _volume);
		}

		void PlayClip(AudioClip _clip, float _volume)
		{
			if(_clip == null || audioSource == null)
				return;

			audioSource.pitch = 1f + Random.Range(-randomPitchRange, randomPitchRange);
			audioSource.PlayOneShot(_clip, _volume);
			audioSource.pitch = 1f;
		}

        public void PlayBounceSound()
        {
            PlayRandomClip(bounceClips, audioClipVolume);
        }

        public void PlayLedgeClimbSound()
        {
            PlayRandomClip(ledgeClimbClips, audioClipVolume);
        }
    }
}

