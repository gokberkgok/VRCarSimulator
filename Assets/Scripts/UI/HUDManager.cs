using UnityEngine;
using UnityEngine.UI;
using VRCarSimulator.Core;

namespace VRCarSimulator.UI
{
    /// <summary>
    /// Manages all HUD elements – speedometer, gear display, signals, handbrake indicator.
    /// Works in both Screen-space and World-space (VR) Canvas.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        [Header("Speed")]
        [SerializeField] private Text speedText;
        [SerializeField] private Image speedNeedle;
        [SerializeField] private float maxSpeedDisplay = 200f;

        [Header("RPM")]
        [SerializeField] private Image rpmNeedle;
        [SerializeField] private float maxRPMAngle = 270f;

        [Header("Gear")]
        [SerializeField] private Text gearText;

        [Header("Signals")]
        [SerializeField] private Image leftSignalIcon;
        [SerializeField] private Image rightSignalIcon;
        [SerializeField] private Color signalActiveColor = Color.green;
        [SerializeField] private Color signalInactiveColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        [Header("Handbrake")]
        [SerializeField] private Image handbrakeIcon;
        [SerializeField] private Color handbrakeActiveColor = Color.red;
        [SerializeField] private Color handbrakeInactiveColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        [Header("ABS")]
        [SerializeField] private Image absIcon;

        private void OnEnable()
        {
            EventBus.OnSpeedChanged += UpdateSpeed;
            EventBus.OnGearChanged += UpdateGear;
            EventBus.OnHandbrakeToggled += UpdateHandbrake;
        }

        private void OnDisable()
        {
            EventBus.OnSpeedChanged -= UpdateSpeed;
            EventBus.OnGearChanged -= UpdateGear;
            EventBus.OnHandbrakeToggled -= UpdateHandbrake;
        }

        private void UpdateSpeed(float speedKmh)
        {
            if (speedText != null)
                speedText.text = Mathf.RoundToInt(speedKmh).ToString();

            if (speedNeedle != null)
            {
                float normalized = Mathf.Clamp01(speedKmh / maxSpeedDisplay);
                speedNeedle.transform.localRotation = Quaternion.Euler(0f, 0f, -normalized * 270f);
            }
        }

        public void UpdateRPM(float normalizedRPM)
        {
            if (rpmNeedle != null)
            {
                float angle = normalizedRPM * maxRPMAngle;
                rpmNeedle.transform.localRotation = Quaternion.Euler(0f, 0f, -angle);
            }
        }

        private void UpdateGear(int gear)
        {
            if (gearText == null) return;
            if (gear == -1) gearText.text = "R";
            else if (gear == 0) gearText.text = "N";
            else gearText.text = gear.ToString();
        }

        public void UpdateSignals(bool left, bool right)
        {
            if (leftSignalIcon != null)
                leftSignalIcon.color = left ? signalActiveColor : signalInactiveColor;
            if (rightSignalIcon != null)
                rightSignalIcon.color = right ? signalActiveColor : signalInactiveColor;
        }

        private void UpdateHandbrake(bool active)
        {
            if (handbrakeIcon != null)
                handbrakeIcon.color = active ? handbrakeActiveColor : handbrakeInactiveColor;
        }

        public void SetABSActive(bool active)
        {
            if (absIcon != null)
                absIcon.gameObject.SetActive(active);
        }
    }
}
