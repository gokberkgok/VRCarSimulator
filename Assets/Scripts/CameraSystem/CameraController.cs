using UnityEngine;

namespace VRCarSimulator.CameraSystem
{
    /// <summary>
    /// Modular camera controller – supports VR cockpit view, optional third-person,
    /// adjustable FOV, and head movement smoothing.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Mode")]
        [SerializeField] private CameraMode currentMode = CameraMode.Cockpit;

        [Header("Target")]
        [SerializeField] private Transform vehicle;
        [SerializeField] private Transform cockpitAnchor;  // inside the vehicle
        [SerializeField] private Transform thirdPersonAnchor;

        [Header("Cockpit Settings")]
        [SerializeField] private float headSmoothing = 8f;
        [SerializeField] private float maxHeadOffset = 0.1f; // subtle sway

        [Header("Third-Person Settings")]
        [SerializeField] private float followDistance = 6f;
        [SerializeField] private float followHeight = 2.5f;
        [SerializeField] private float followSmoothing = 5f;
        [SerializeField] private float lookAtSmoothing = 8f;

        [Header("FOV")]
        [SerializeField] private float baseFOV = 60f;
        [SerializeField] private float maxFOV = 80f;
        [SerializeField] private float fovSpeedScale = 200f; // km/h at which FOV is max

        private UnityEngine.Camera cam;
        private Vector3 smoothVelocity;
        private float currentFOV;

        public CameraMode CurrentMode => currentMode;

        private void Awake()
        {
            cam = GetComponent<UnityEngine.Camera>();
            if (cam == null) cam = UnityEngine.Camera.main;
            currentFOV = baseFOV;
        }

        private void LateUpdate()
        {
            if (vehicle == null) return;

            switch (currentMode)
            {
                case CameraMode.Cockpit:
                    UpdateCockpitView();
                    break;
                case CameraMode.ThirdPerson:
                    UpdateThirdPersonView();
                    break;
                case CameraMode.VR:
                    // VR camera is controlled by headset – only apply anchor parenting
                    UpdateVRView();
                    break;
            }

            UpdateFOV();
        }

        private void UpdateCockpitView()
        {
            if (cockpitAnchor == null) return;

            // Smooth head movement (simulates body inertia)
            Vector3 targetPos = cockpitAnchor.position;
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * headSmoothing);
            transform.rotation = Quaternion.Slerp(transform.rotation, cockpitAnchor.rotation, Time.deltaTime * headSmoothing);
        }

        private void UpdateThirdPersonView()
        {
            Vector3 targetPos = vehicle.position
                - vehicle.forward * followDistance
                + Vector3.up * followHeight;

            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref smoothVelocity,
                1f / followSmoothing);

            Quaternion targetRot = Quaternion.LookRotation(vehicle.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * lookAtSmoothing);
        }

        private void UpdateVRView()
        {
            if (cockpitAnchor == null) return;
            // Parent the VR camera rig to the cockpit anchor so it moves with the car
            // The actual head rotation is handled by the XR system
        }

        private void UpdateFOV()
        {
            if (cam == null || currentMode == CameraMode.VR) return;

            // Speed-based FOV increase
            float speed = 0f;
            var vehicleCtrl = vehicle?.GetComponent<Vehicle.VehicleController>();
            if (vehicleCtrl != null) speed = vehicleCtrl.SpeedKmh;

            float targetFOV = Mathf.Lerp(baseFOV, maxFOV, speed / fovSpeedScale);
            currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * 3f);
            cam.fieldOfView = currentFOV;
        }

        public void SetMode(CameraMode mode)
        {
            currentMode = mode;
        }

        public void ToggleMode()
        {
            currentMode = currentMode == CameraMode.Cockpit
                ? CameraMode.ThirdPerson
                : CameraMode.Cockpit;
        }

        public void SetFOV(float fov)
        {
            baseFOV = fov;
        }
    }

    public enum CameraMode
    {
        Cockpit,
        ThirdPerson,
        VR
    }
}
