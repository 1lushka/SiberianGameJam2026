using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMF
{
	//This script handles and plays audio cues like footsteps, jump and land audio clips based on character movement speed and events; 
	public class AudioControl : MonoBehaviour {


        public static AudioControl Instance { get; private set; }
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

        [Header("Disappearing Platforms")]
        public AudioClip[] shakeClips;
        public AudioClip[] shrinkClips;
        [Header("Platform Volume")]
        [Range(0f, 1f)]
        public float platformSoundVolume = 0.4f;

        public bool isLedgeClimbing { get; set; }
        public Vector3 ledgeMoveVelocity { get; set; }

        //Setup;

        void Awake()
        {
            // Синглтон (если ещё не настроен)
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }
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

            // Use a dedicated traversal distance so ledge movement cadence can be tuned separately.
            ledgeClimbDistanceCounter += Time.deltaTime * _speed;

            float _ledgeStepDistance = ledgeClimbDistance > 0f ? ledgeClimbDistance : footstepDistance;
            if (ledgeClimbDistanceCounter >= _ledgeStepDistance)
            {
                PlayFootstepSound(_speed);
                ledgeClimbDistanceCounter = 0f;
            }
        }

        public void PlayFootstepSound(float _movementSpeed)
		{
			AudioClip[] _clips = ResolveFootstepClips();
			if(_clips == null || _clips.Length == 0)
				return;

			int _footStepClipIndex = Random.Range(0, _clips.Length);
			PlayClip(_clips[_footStepClipIndex], audioClipVolume + audioClipVolume * Random.Range(-relativeRandomizedVolumeRange, relativeRandomizedVolumeRange));
		}

		void OnLand(Vector3 _v)
		{
			//Only trigger sound if downward velocity exceeds threshold;
			if(VectorMath.GetDotProduct(_v, tr.up) > -landVelocityThreshold)
				return;

			//Play land audio clip;
			PlayRandomClip(ResolveLandClips(), audioClipVolume);
		}

		void OnJump(Vector3 _v)
		{
			PlayJumpSound();
		}

        public void PlayJumpSound()
        {
            //Play jump audio clip;
            PlayRandomClip(ResolveJumpClips(), audioClipVolume);
        }

        AudioClip[] ResolveFootstepClips()
        {
            CharacterMovementPreset _preset = GetActiveMovementPreset();
            if(_preset != null && _preset.footstepClips != null && _preset.footstepClips.Length > 0)
                return _preset.footstepClips;

            return footStepClips;
        }

        AudioClip[] ResolveJumpClips()
        {
            CharacterMovementPreset _preset = GetActiveMovementPreset();
            if(_preset != null && _preset.jumpClips != null && _preset.jumpClips.Length > 0)
                return _preset.jumpClips;

            return jumpClips;
        }

        AudioClip[] ResolveLandClips()
        {
            CharacterMovementPreset _preset = GetActiveMovementPreset();
            if(_preset != null && _preset.landClips != null && _preset.landClips.Length > 0)
                return _preset.landClips;

            return landClips;
        }

        CharacterMovementPreset GetActiveMovementPreset()
        {
            if(controller is AdvancedWalkerController _walkerController)
                return _walkerController.CurrentMovementPreset;

            return null;
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

        private void PlayRandomAtPoint(AudioClip[] clips, Vector3 position, float volume)
        {
            if (clips == null || clips.Length == 0) return;
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            if (clip != null)
            {
                // Создаём временный AudioSource, чтобы задать громкость
                GameObject tempGO = new GameObject("TempPlatformSound");
                tempGO.transform.position = position;
                AudioSource tempSource = tempGO.AddComponent<AudioSource>();
                tempSource.clip = clip;
                tempSource.volume = volume;
                tempSource.spatialBlend = 1f;   // 3D-звук
                tempSource.Play();
                Destroy(tempGO, clip.length);
            }
        }

        public void PlayShakeSound(Vector3 position)
        {
            PlayRandomAtPoint(shakeClips, position, platformSoundVolume);
        }

        public void PlayShrinkSound(Vector3 position)
        {
            PlayRandomAtPoint(shrinkClips, position, platformSoundVolume);
        }
    }
}

