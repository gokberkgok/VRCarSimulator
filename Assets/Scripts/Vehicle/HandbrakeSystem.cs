using UnityEngine;
using VRCarSimulator.Core;

namespace VRCarSimulator.Vehicle
{
    /// <summary>
    /// Handbrake system with VR pull interaction support.
    /// </summary>
    public class HandbrakeSystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float engageThreshold = 0.7f;
        [SerializeField] private float releaseThreshold = 0.3f;

        [Header("Visual")]
        [SerializeField] private Transform handbrakeHandle;
        [SerializeField] private float handleRotationRange = 45f; // degrees

        private float currentValue;
        private bool isActive;

        public bool IsActive => isActive;
        public float CurrentValue => currentValue;

        /// <summary>
        /// Set handbrake value from VR controller or input (0-1).
        /// </summary>
        public void SetHandbrakeValue(float value)
        {
            currentValue = Mathf.Clamp01(value);

            if (!isActive && currentValue >= engageThreshold)
            {
                isActive = true;
                EventBus.TriggerHandbrakeToggled(true);
            }
            else if (isActive && currentValue <= releaseThreshold)
            {
                isActive = false;
                EventBus.TriggerHandbrakeToggled(false);
            }

            UpdateVisual();
        }

        public void Toggle()
        {
            isActive = !isActive;
            currentValue = isActive ? 1f : 0f;
            EventBus.TriggerHandbrakeToggled(isActive);
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (handbrakeHandle == null) return;
            float angle = currentValue * handleRotationRange;
            handbrakeHandle.localRotation = Quaternion.Euler(-angle, 0f, 0f);
        }
    }
}
