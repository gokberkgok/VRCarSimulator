using UnityEngine;

namespace VRCarSimulator.VR
{
    /// <summary>
    /// Makes a VR-interactive steering wheel that can be grabbed and rotated.
    /// Attach to the steering wheel object; output goes to VRInputManager.
    /// </summary>
    public class VRSteeringWheel : MonoBehaviour
    {
        [Header("References")]
       // [SerializeField] private VRInputManager inputManager;

        [Header("Settings")]
        [SerializeField] private float maxRotationAngle = 450f;
        [SerializeField] private float returnSpeed = 5f;
        [SerializeField] private Transform wheelPivot;

        private bool isGrabbed;
        private float currentAngle;
        private Vector3 grabStartDirection;
        private Transform grabbingHand;

        public float NormalizedValue => currentAngle / maxRotationAngle; // -1 to 1

        private void Update()
        {
            if (isGrabbed && grabbingHand != null)
            {
                UpdateGrabbedRotation();
            }
            else
            {
                // Return to center
                currentAngle = Mathf.Lerp(currentAngle, 0f, Time.deltaTime * returnSpeed);
            }

            ApplyVisualRotation();
            SendInput();
        }

        /// <summary>
        /// Called when VR hand grabs the steering wheel.
        /// </summary>
        public void OnGrab(Transform hand)
        {
            isGrabbed = true;
            grabbingHand = hand;
            grabStartDirection = GetLocalDirection(hand.position);
        }

        /// <summary>
        /// Called when VR hand releases the steering wheel.
        /// </summary>
        public void OnRelease()
        {
            isGrabbed = false;
            grabbingHand = null;
        }

        private void UpdateGrabbedRotation()
        {
            Vector3 currentDirection = GetLocalDirection(grabbingHand.position);
            float angle = Vector3.SignedAngle(grabStartDirection, currentDirection, transform.forward);

            currentAngle = Mathf.Clamp(currentAngle + angle, -maxRotationAngle, maxRotationAngle);
            grabStartDirection = currentDirection;
        }

        private Vector3 GetLocalDirection(Vector3 worldPos)
        {
            Vector3 localPos = transform.InverseTransformPoint(worldPos);
            localPos.z = 0f;
            return localPos.normalized;
        }

        private void ApplyVisualRotation()
        {
            if (wheelPivot != null)
                wheelPivot.localRotation = Quaternion.Euler(0f, 0f, -currentAngle);
        }

        private void SendInput()
        {
            //if (inputManager != null)
              //  inputManager.SetSteeringValue(NormalizedValue);
        }
    }
}
