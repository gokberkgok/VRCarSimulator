using UnityEngine;
using VRCarSimulator.Core;

namespace VRCarSimulator.Vehicle
{
    /// <summary>
    /// Brake system with ABS simulation and brake force distribution.
    /// </summary>
    public class BrakeSystem : MonoBehaviour
    {
        [Header("Brake Settings")]
        [SerializeField] private float maxBrakeTorque = 4000f;
        [SerializeField] private float brakeResponseCurve = 2f; // exponential feel

        [Header("ABS")]
        [SerializeField] private bool absEnabled = true;
        [SerializeField] private float absSlipThreshold = 0.3f;
        [SerializeField] private float absCycleRate = 15f; // Hz

        [Header("References")]
        [SerializeField] private WheelCollider[] monitoredWheels;

        private bool absActive;
        private float absTimer;

        public bool ABSActive => absActive;

        /// <summary>
        /// Returns calculated brake torque considering ABS.
        /// </summary>
        public float GetBrakeTorque(float input, float speedKmh)
        {
            if (input < 0.01f)
            {
                absActive = false;
                return 0f;
            }

            float baseBrake = Mathf.Pow(input, brakeResponseCurve) * maxBrakeTorque;

            if (absEnabled && speedKmh > 5f)
            {
                baseBrake = ApplyABS(baseBrake);
            }

            return baseBrake;
        }

        private float ApplyABS(float brakeTorque)
        {
            bool anyWheelSlipping = false;

            if (monitoredWheels != null)
            {
                foreach (var wheel in monitoredWheels)
                {
                    if (wheel == null) continue;
                    wheel.GetGroundHit(out WheelHit hit);
                    if (Mathf.Abs(hit.forwardSlip) > absSlipThreshold)
                    {
                        anyWheelSlipping = true;
                        break;
                    }
                }
            }

            if (anyWheelSlipping)
            {
                absActive = true;
                absTimer += Time.fixedDeltaTime;
                float cycle = Mathf.Sin(absTimer * absCycleRate * Mathf.PI * 2f);
                return brakeTorque * Mathf.Lerp(0.3f, 1f, (cycle + 1f) * 0.5f);
            }

            absActive = false;
            absTimer = 0f;
            return brakeTorque;
        }

        /// <summary>
        /// Returns the current braking distance estimate in meters.
        /// </summary>
        public float EstimateBrakingDistance(float speedKmh, float friction = 0.7f)
        {
            float speedMs = speedKmh / 3.6f;
            // v^2 / (2 * mu * g)
            return (speedMs * speedMs) / (2f * friction * 9.81f);
        }
    }
}
