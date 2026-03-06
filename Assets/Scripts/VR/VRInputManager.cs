using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using VRCarSimulator.Vehicle;
using System.Collections.Generic;

namespace VRCarSimulator.VR
{
    /// <summary>
    /// Central VR input manager – reads XR controller inputs via Input System Actions
    /// and maps them to vehicle systems. Supports haptic feedback on both hands.
    /// </summary>
   /* public class VRInputManager : MonoBehaviour
    {
        [Header("Vehicle Reference")]
        [SerializeField] private VehicleController vehicleController;
        [SerializeField] private GearSystem gearSystem;
        [SerializeField] private HandbrakeSystem handbrakeSystem;
        [SerializeField] private SignalSystem signalSystem;

        [Header("VR Settings")]
        [SerializeField] private bool vrActive = true;
        [SerializeField] private float steeringDeadzone = 0.05f;
        [SerializeField] private float throttleSensitivity = 1f;
        [SerializeField] private float brakeSensitivity = 1f;

        [Header("Input Action References")]
        [SerializeField] private InputActionReference rightTriggerAction;  // Gas
        [SerializeField] private InputActionReference leftTriggerAction;   // Brake
        [SerializeField] private InputActionReference leftGripAction;      // Clutch
        [SerializeField] private InputActionReference rightGripAction;     // Handbrake
        [SerializeField] private InputActionReference leftPrimaryButtonAction;   // Left signal (X/A)
        [SerializeField] private InputActionReference rightPrimaryButtonAction;  // Right signal (A/X)
        [SerializeField] private InputActionReference leftSecondaryButtonAction; // Gear down (Y/B)
        [SerializeField] private InputActionReference rightSecondaryButtonAction;// Gear up (B/Y)
        [SerializeField] private InputActionReference leftThumbstickAction;      // Optional steering

        [Header("Haptic Feedback")]
        [SerializeField] private float hapticIntensity = 0.3f;
        [SerializeField] private float hapticDuration = 0.05f;
        [SerializeField] private float collisionHapticIntensity = 0.8f;
        [SerializeField] private float collisionHapticDuration = 0.2f;

        // Runtime values
        private float leftTrigger;
        private float rightTrigger;
        private float steerValue;
        private float clutchValue;
        private float handbrakeValue;

        // XR devices for haptics
        private InputDevice leftHandDevice;
        private InputDevice rightHandDevice;
        private float deviceRefreshTimer;

        public bool VRActive => vrActive;

        private void OnEnable()
        {
            Core.EventBus.OnCollision += HandleCollisionHaptic;
            EnableActions();
            BindButtonCallbacks();
        }

        private void OnDisable()
        {
            Core.EventBus.OnCollision -= HandleCollisionHaptic;
            UnbindButtonCallbacks();
        }

        private void Update()
        {
            if (!vrActive || vehicleController == null) return;

            PollXRInputs();
            ApplyInputsToVehicle();
            RefreshDevices();
        }

        // ──────────────────── INPUT READING ────────────────────

        private void PollXRInputs()
        {
            // Analog axes – read every frame
            if (rightTriggerAction != null && rightTriggerAction.action != null)
                rightTrigger = rightTriggerAction.action.ReadValue<float>();

            if (leftTriggerAction != null && leftTriggerAction.action != null)
                leftTrigger = leftTriggerAction.action.ReadValue<float>();

            if (leftGripAction != null && leftGripAction.action != null)
                clutchValue = leftGripAction.action.ReadValue<float>();

            if (rightGripAction != null && rightGripAction.action != null)
                handbrakeValue = rightGripAction.action.ReadValue<float>();

            // Thumbstick steering (fallback when not using VR steering wheel grab)
            if (leftThumbstickAction != null && leftThumbstickAction.action != null)
            {
                Vector2 thumbstick = leftThumbstickAction.action.ReadValue<Vector2>();
                // Only use thumbstick if no external steering value is being set
                if (Mathf.Abs(steerValue) < 0.01f)
                    steerValue = thumbstick.x;
            }
        }

        private void ApplyInputsToVehicle()
        {
            vehicleController.SetThrottleInput(rightTrigger * throttleSensitivity);
            vehicleController.SetBrakeInput(leftTrigger * brakeSensitivity);

            float steer = Mathf.Abs(steerValue) > steeringDeadzone ? steerValue : 0f;
            vehicleController.SetSteerInput(steer);

            vehicleController.SetClutchInput(clutchValue);

            if (handbrakeSystem != null)
                handbrakeSystem.SetHandbrakeValue(handbrakeValue);

            // Reset external steer so thumbstick can take over next frame
            steerValue = 0f;
        }

        // ──────────────────── BUTTON CALLBACKS ────────────────────

        private void EnableActions()
        {
            EnableAction(rightTriggerAction);
            EnableAction(leftTriggerAction);
            EnableAction(leftGripAction);
            EnableAction(rightGripAction);
            EnableAction(leftPrimaryButtonAction);
            EnableAction(rightPrimaryButtonAction);
            EnableAction(leftSecondaryButtonAction);
            EnableAction(rightSecondaryButtonAction);
            EnableAction(leftThumbstickAction);
        }

        private void EnableAction(InputActionReference actionRef)
        {
            if (actionRef != null && actionRef.action != null)
                actionRef.action.Enable();
        }

        private void BindButtonCallbacks()
        {
            BindPerformed(leftPrimaryButtonAction, OnLeftSignalPressed);
            BindPerformed(rightPrimaryButtonAction, OnRightSignalPressed);
            BindPerformed(leftSecondaryButtonAction, OnGearDownPressed);
            BindPerformed(rightSecondaryButtonAction, OnGearUpPressed);
        }

        private void UnbindButtonCallbacks()
        {
            UnbindPerformed(leftPrimaryButtonAction, OnLeftSignalPressed);
            UnbindPerformed(rightPrimaryButtonAction, OnRightSignalPressed);
            UnbindPerformed(leftSecondaryButtonAction, OnGearDownPressed);
            UnbindPerformed(rightSecondaryButtonAction, OnGearUpPressed);
        }

        private void BindPerformed(InputActionReference actionRef, System.Action<InputAction.CallbackContext> callback)
        {
            if (actionRef != null && actionRef.action != null)
                actionRef.action.performed += callback;
        }

        private void UnbindPerformed(InputActionReference actionRef, System.Action<InputAction.CallbackContext> callback)
        {
            if (actionRef != null && actionRef.action != null)
                actionRef.action.performed -= callback;
        }

        private void OnLeftSignalPressed(InputAction.CallbackContext ctx)
        {
            if (signalSystem != null) signalSystem.ToggleLeft();
            TriggerHaptic(HapticHand.Left, hapticIntensity, hapticDuration);
        }

        private void OnRightSignalPressed(InputAction.CallbackContext ctx)
        {
            if (signalSystem != null) signalSystem.ToggleRight();
            TriggerHaptic(HapticHand.Right, hapticIntensity, hapticDuration);
        }

        private void OnGearDownPressed(InputAction.CallbackContext ctx)
        {
            if (gearSystem != null) gearSystem.ShiftDown();
            TriggerHaptic(HapticHand.Left, hapticIntensity, hapticDuration);
        }

        private void OnGearUpPressed(InputAction.CallbackContext ctx)
        {
            if (gearSystem != null) gearSystem.ShiftUp();
            TriggerHaptic(HapticHand.Right, hapticIntensity, hapticDuration);
        }

        // ──────────────────── EXTERNAL SETTERS ────────────────────
        // (VRSteeringWheel, VRGearLever gibi grab interactable'lardan çağrılır)

        public void SetSteeringValue(float value) => steerValue = Mathf.Clamp(value, -1f, 1f);
        public void SetThrottleValue(float value) => rightTrigger = Mathf.Clamp01(value);
        public void SetBrakeValue(float value) => leftTrigger = Mathf.Clamp01(value);
        public void SetClutchValue(float value) => clutchValue = Mathf.Clamp01(value);

        public void OnGearSetDirect(int gearIndex)
        {
            if (gearSystem != null) gearSystem.SetGearDirect(gearIndex);
            TriggerHaptic(HapticHand.Both, hapticIntensity, hapticDuration);
        }

        // ──────────────────── HAPTIC FEEDBACK ────────────────────

        public enum HapticHand { Left, Right, Both }

        private void HandleCollisionHaptic(Core.CollisionType type, float force)
        {
            float intensity = Mathf.Lerp(hapticIntensity, collisionHapticIntensity, Mathf.Clamp01(force / 50f));
            TriggerHaptic(HapticHand.Both, intensity, collisionHapticDuration);
        }

        public void TriggerHaptic(float intensity, float duration)
        {
            TriggerHaptic(HapticHand.Both, intensity, duration);
        }

        public void TriggerHaptic(HapticHand hand, float intensity, float duration)
        {
            if (hand == HapticHand.Left || hand == HapticHand.Both)
                SendHaptic(leftHandDevice, intensity, duration);
            if (hand == HapticHand.Right || hand == HapticHand.Both)
                SendHaptic(rightHandDevice, intensity, duration);
        }

        private void SendHaptic(InputDevice device, float intensity, float duration)
        {
            if (device.isValid)
            {
                device.SendHapticImpulse(0, intensity, duration);
            }
        }

        private void RefreshDevices()
        {
            deviceRefreshTimer += Time.deltaTime;
            if (deviceRefreshTimer < 2f) return; // her 2 saniyede bir
            deviceRefreshTimer = 0f;

            var leftDevices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftDevices);
            if (leftDevices.Count > 0) leftHandDevice = leftDevices[0];

            var rightDevices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightDevices);
            if (rightDevices.Count > 0) rightHandDevice = rightDevices[0];
        }
    }*/
}
