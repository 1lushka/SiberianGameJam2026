using System;
using UnityEngine;

namespace Project.Code.Gameplay.Common
{
    public class SpeedParticle : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particleSystem;
        [Space]
        [SerializeField] private float _rotationSpeed = 15f;
        [SerializeField] private float _emissionMultiplier;
        [SerializeField] private float _maxEmission;

        private ParticleSystem.EmissionModule _emissionModule;
        private Vector3 _lastPos;

        private float _setLastPosTimer;

        private void Start()
        {
            _emissionModule = _particleSystem.emission;
        }

        private void Update()
        {
            var moveVector = transform.position - _lastPos;
            var speed = moveVector.magnitude / Time.deltaTime;
            var emission = Mathf.Clamp(speed * _emissionMultiplier, 0f, _maxEmission);
            
            _emissionModule.rateOverDistance = emission;

            if (moveVector.magnitude > 0.01f)
            {
                var targetRotation = Quaternion.LookRotation(moveVector);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
            }

            _setLastPosTimer += Time.deltaTime;

            if (_setLastPosTimer >= 0.05f)
            {
                _setLastPosTimer = 0f;
                _lastPos = transform.position;
            }
        }
    }
}
