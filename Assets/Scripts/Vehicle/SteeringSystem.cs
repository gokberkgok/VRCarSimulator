using UnityEngine;

namespace VRCarSimulator.Vehicle
{
    /// <summary>
    /// Speed-sensitive steering with configurable max angle and damping.
    /// </summary>
    public class SteeringSystem : MonoBehaviour
    {
        [Header("Steering Settings")]
        [SerializeField] private float maxSteerAngle = 35f;
        [SerializeField] private float minSteerAngleAtSpeed = 10f;
        [SerializeField] private float speedSensitivityKmh = 100f;

        [Header("Smoothing")]
        [SerializeField] private float steerSmoothing = 8f;

        [Header("Visual")]
        [SerializeField] private Transform steeringWheelMesh;
        [SerializeField] private float steeringWheelRotationRange = 450f; // degrees

        private float currentSteerAngle;
        private float smoothedInput;

        public float CurrentSteerAngle => currentSteerAngle;

        /// <summary>
        /// Returns the desired wheel steer angle based on input and speed.
        /// </summary>
        public float GetSteerAngle(float input, float speedKmh)
        {
            smoothedInput = Mathf.Lerp(smoothedInput, input, Time.fixedDeltaTime * steerSmoothing);

            float speedFactor = Mathf.InverseLerp(0f, speedSensitivityKmh, speedKmh);
            float effectiveMaxAngle = Mathf.Lerp(maxSteerAngle, minSteerAngleAtSpeed, speedFactor);

            currentSteerAngle = smoothedInput * effectiveMaxAngle;
            return currentSteerAngle;
        }

        private void Update()
        {
            UpdateSteeringWheelVisual();
        }

        private void UpdateSteeringWheelVisual()
        {
            if (steeringWheelMesh == null) return;

            float normalizedAngle = currentSteerAngle / maxSteerAngle;
            float visualRotation = normalizedAngle * (steeringWheelRotationRange * 0.5f);
            steeringWheelMesh.localRotation = Quaternion.Euler(0f, 0f, -visualRotation);
        }
    }
}
