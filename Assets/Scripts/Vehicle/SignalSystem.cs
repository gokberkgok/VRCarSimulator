using UnityEngine;

namespace VRCarSimulator.Vehicle
{
    /// <summary>
    /// Turn signal system with left/right indicators and auto-cancel.
    /// </summary>
    public class SignalSystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float blinkRate = 0.5f; // seconds per blink
        [SerializeField] private float autoCancelSteerAngle = 5f;

        [Header("Light References")]
        [SerializeField] private GameObject[] leftSignalLights;
        [SerializeField] private GameObject[] rightSignalLights;

        private bool leftActive;
        private bool rightActive;
        private float blinkTimer;
        private bool blinkState;

        public bool LeftActive => leftActive;
        public bool RightActive => rightActive;

        private void Update()
        {
            if (!leftActive && !rightActive) return;

            blinkTimer += Time.deltaTime;
            if (blinkTimer >= blinkRate)
            {
                blinkTimer = 0f;
                blinkState = !blinkState;
                UpdateLights();
            }
        }

        public void ToggleLeft()
        {
            leftActive = !leftActive;
            rightActive = false;
            ResetBlink();
        }

        public void ToggleRight()
        {
            rightActive = !rightActive;
            leftActive = false;
            ResetBlink();
        }

        public void CancelAll()
        {
            leftActive = false;
            rightActive = false;
            SetLights(leftSignalLights, false);
            SetLights(rightSignalLights, false);
        }

        /// <summary>
        /// Call from steering system to auto-cancel after turn completion.
        /// </summary>
        public void CheckAutoCancel(float steerAngle)
        {
            if ((leftActive && steerAngle > autoCancelSteerAngle) ||
                (rightActive && steerAngle < -autoCancelSteerAngle))
            {
                // Auto-cancel when wheel returns past center
                CancelAll();
            }
        }

        private void ResetBlink()
        {
            blinkTimer = 0f;
            blinkState = true;
            UpdateLights();
        }

        private void UpdateLights()
        {
            SetLights(leftSignalLights, leftActive && blinkState);
            SetLights(rightSignalLights, rightActive && blinkState);
        }

        private void SetLights(GameObject[] lights, bool on)
        {
            if (lights == null) return;
            foreach (var light in lights)
            {
                if (light != null) light.SetActive(on);
            }
        }
    }
}
