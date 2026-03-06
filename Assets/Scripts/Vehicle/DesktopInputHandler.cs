using UnityEngine;

namespace VRCarSimulator.Vehicle
{
    /// <summary>
    /// Keyboard/gamepad input fallback when VR is not active.
    /// Maps standard Unity Input to VehicleController.
    /// </summary>
    [RequireComponent(typeof(VehicleController))]
    public class DesktopInputHandler : MonoBehaviour
    {
        private VehicleController vehicle;
        private GearSystem gearSystem;
        private HandbrakeSystem handbrakeSystem;
        private SignalSystem signalSystem;

        [Header("Input Axes")]
        [SerializeField] private string throttleAxis = "Vertical";
        [SerializeField] private string steerAxis = "Horizontal";
        [SerializeField] private string brakeKey = "space";
        [SerializeField] private string handbrakeKey = "h";
        [SerializeField] private string shiftUpKey = "e";
        [SerializeField] private string shiftDownKey = "q";
        [SerializeField] private string leftSignalKey = "z";
        [SerializeField] private string rightSignalKey = "x";

        private void Awake()
        {
            vehicle = GetComponent<VehicleController>();
            gearSystem = GetComponent<GearSystem>();
            handbrakeSystem = GetComponent<HandbrakeSystem>();
            signalSystem = GetComponent<SignalSystem>();
        }

        private void Update()
        {
            float vertical = Input.GetAxis(throttleAxis);
            float horizontal = Input.GetAxis(steerAxis);

            vehicle.SetThrottleInput(Mathf.Clamp01(vertical));
            vehicle.SetBrakeInput(Mathf.Clamp01(-vertical));
            vehicle.SetSteerInput(horizontal);

            if (Input.GetKey(brakeKey))
                vehicle.SetBrakeInput(1f);

            if (Input.GetKeyDown(shiftUpKey) && gearSystem != null)
                gearSystem.ShiftUp();

            if (Input.GetKeyDown(shiftDownKey) && gearSystem != null)
                gearSystem.ShiftDown();

            if (Input.GetKeyDown(handbrakeKey) && handbrakeSystem != null)
                handbrakeSystem.Toggle();

            if (signalSystem != null)
            {
                if (Input.GetKeyDown(leftSignalKey)) signalSystem.ToggleLeft();
                if (Input.GetKeyDown(rightSignalKey)) signalSystem.ToggleRight();
            }
        }
    }
}
