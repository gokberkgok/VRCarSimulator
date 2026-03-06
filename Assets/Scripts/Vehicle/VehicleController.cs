using UnityEngine;
using VRCarSimulator.Core;

namespace VRCarSimulator.Vehicle
{
    /// <summary>
    /// Main vehicle controller – orchestrates all subsystems (steering, gear, brake, handbrake).
    /// Uses WheelCollider-based physics for realistic behaviour.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SteeringSystem steering;
        [SerializeField] private GearSystem gearSystem;
        [SerializeField] private BrakeSystem brakeSystem;
        [SerializeField] private HandbrakeSystem handbrakeSystem;

        [Header("Wheel Colliders")]
        [SerializeField] private WheelCollider frontLeft;
        [SerializeField] private WheelCollider frontRight;
        [SerializeField] private WheelCollider rearLeft;
        [SerializeField] private WheelCollider rearRight;

        [Header("Wheel Meshes")]
        [SerializeField] private Transform frontLeftMesh;
        [SerializeField] private Transform frontRightMesh;
        [SerializeField] private Transform rearLeftMesh;
        [SerializeField] private Transform rearRightMesh;

        [Header("Engine")]
        [SerializeField] private float maxMotorTorque = 350f;
        [SerializeField] private float maxRPM = 7000f;
        [SerializeField] private float idleRPM = 800f;
        [SerializeField] private AnimationCurve torqueCurve;

        [Header("Physics")]
        [SerializeField] private Vector3 centerOfMass = new Vector3(0f, -0.5f, 0.3f);

        private Rigidbody rb;
        private float currentRPM;
        private float throttleInput;
        private float steerInput;
        private float brakeInput;
        private float clutchInput;

        // Public accessors
        public float SpeedKmh => rb.velocity.magnitude * 3.6f;
        public float CurrentRPM => currentRPM;
        public float NormalizedRPM => Mathf.InverseLerp(idleRPM, maxRPM, currentRPM);
        public int CurrentGear => gearSystem != null ? gearSystem.CurrentGear : 0;
        public Rigidbody Rigidbody => rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.centerOfMass = centerOfMass;

            if (torqueCurve == null || torqueCurve.length == 0)
            {
                torqueCurve = new AnimationCurve(
                    new Keyframe(0f, 0.5f),
                    new Keyframe(0.4f, 1f),
                    new Keyframe(0.7f, 1f),
                    new Keyframe(1f, 0.6f)
                );
            }
        }

        private void Update()
        {
            UpdateWheelMeshes();
        }

        private void FixedUpdate()
        {
            CalculateEngineRPM();
            ApplyMotor();
            ApplySteering();
            ApplyBrakes();
            BroadcastState();
        }

        #region Input Setters (called by VR or keyboard input)

        public void SetThrottleInput(float value) => throttleInput = Mathf.Clamp01(value);
        public void SetSteerInput(float value) => steerInput = Mathf.Clamp(value, -1f, 1f);
        public void SetBrakeInput(float value) => brakeInput = Mathf.Clamp01(value);
        public void SetClutchInput(float value) => clutchInput = Mathf.Clamp01(value);

        #endregion

        private void CalculateEngineRPM()
        {
            float wheelRPM = (rearLeft.rpm + rearRight.rpm) * 0.5f;
            float gearRatio = gearSystem != null ? gearSystem.GetCurrentGearRatio() : 1f;

            if (gearRatio == 0f) gearRatio = 1f;

            float engineRPMFromWheels = Mathf.Abs(wheelRPM * gearRatio);
            float clutchFactor = 1f - clutchInput; // 0 = fully pressed, 1 = released

            currentRPM = Mathf.Lerp(
                idleRPM + (throttleInput * (maxRPM - idleRPM)),
                Mathf.Clamp(engineRPMFromWheels, idleRPM, maxRPM),
                clutchFactor
            );

            currentRPM = Mathf.Clamp(currentRPM, idleRPM, maxRPM);
        }

        private void ApplyMotor()
        {
            bool handbrakeActive = handbrakeSystem != null && handbrakeSystem.IsActive;
            if (handbrakeActive) return;

            float normalizedRPM = NormalizedRPM;
            float torqueMultiplier = torqueCurve.Evaluate(normalizedRPM);
            float gearRatio = gearSystem != null ? gearSystem.GetCurrentGearRatio() : 1f;
            float clutchFactor = 1f - clutchInput;

            float motorTorque = throttleInput * maxMotorTorque * torqueMultiplier * gearRatio * clutchFactor;

            rearLeft.motorTorque = motorTorque;
            rearRight.motorTorque = motorTorque;
        }

        private void ApplySteering()
        {
            float steerAngle = steering != null
                ? steering.GetSteerAngle(steerInput, SpeedKmh)
                : steerInput * 35f;

            frontLeft.steerAngle = steerAngle;
            frontRight.steerAngle = steerAngle;
        }

        private void ApplyBrakes()
        {
            float brake = 0f;

            if (brakeSystem != null)
                brake = brakeSystem.GetBrakeTorque(brakeInput, SpeedKmh);
            else
                brake = brakeInput * 3000f;

            bool handbrakeActive = handbrakeSystem != null && handbrakeSystem.IsActive;

            frontLeft.brakeTorque = brake;
            frontRight.brakeTorque = brake;
            rearLeft.brakeTorque = handbrakeActive ? 5000f : brake;
            rearRight.brakeTorque = handbrakeActive ? 5000f : brake;
        }

        private void UpdateWheelMeshes()
        {
            UpdateWheelMesh(frontLeft, frontLeftMesh);
            UpdateWheelMesh(frontRight, frontRightMesh);
            UpdateWheelMesh(rearLeft, rearLeftMesh);
            UpdateWheelMesh(rearRight, rearRightMesh);
        }

        private void UpdateWheelMesh(WheelCollider collider, Transform mesh)
        {
            if (collider == null || mesh == null) return;
            collider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            mesh.position = pos;
            mesh.rotation = rot;
        }

        private void BroadcastState()
        {
            EventBus.TriggerSpeedChanged(SpeedKmh);
        }

        /// <summary>
        /// Sets wheel friction for weather / surface conditions.
        /// </summary>
        public void SetWheelFriction(float forwardStiffness, float sidewaysStiffness)
        {
            SetFriction(frontLeft, forwardStiffness, sidewaysStiffness);
            SetFriction(frontRight, forwardStiffness, sidewaysStiffness);
            SetFriction(rearLeft, forwardStiffness, sidewaysStiffness);
            SetFriction(rearRight, forwardStiffness, sidewaysStiffness);
        }

        private void SetFriction(WheelCollider wheel, float fwd, float side)
        {
            WheelFrictionCurve fwdCurve = wheel.forwardFriction;
            fwdCurve.stiffness = fwd;
            wheel.forwardFriction = fwdCurve;

            WheelFrictionCurve sideCurve = wheel.sidewaysFriction;
            sideCurve.stiffness = side;
            wheel.sidewaysFriction = sideCurve;
        }
    }
}
