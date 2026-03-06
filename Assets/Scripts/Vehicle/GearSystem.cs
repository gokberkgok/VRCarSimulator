using UnityEngine;
using VRCarSimulator.Core;

namespace VRCarSimulator.Vehicle
{
    /// <summary>
    /// Manual gear system with configurable ratios, reverse, and neutral.
    /// </summary>
    [System.Serializable]
    public class GearSystem : MonoBehaviour
    {
        [Header("Gear Ratios")]
        [SerializeField] private float[] gearRatios = { -3.2f, 0f, 3.5f, 2.15f, 1.4f, 1f, 0.77f };
        // Index:  0=Reverse, 1=Neutral, 2=1st, 3=2nd, 4=3rd, 5=4th, 6=5th

        [SerializeField] private float finalDriveRatio = 3.7f;

        [Header("Shift Settings")]
        [SerializeField] private float shiftCooldown = 0.3f;

        private int currentGearIndex = 1; // Start in Neutral
        private float lastShiftTime;

        public int CurrentGear => currentGearIndex - 1; // -1=R, 0=N, 1-5=gears
        public int GearCount => gearRatios.Length - 2; // Excludes R and N
        public bool IsNeutral => currentGearIndex == 1;
        public bool IsReverse => currentGearIndex == 0;

        public float GetCurrentGearRatio()
        {
            return gearRatios[currentGearIndex] * finalDriveRatio;
        }

        public string GetGearDisplayName()
        {
            if (currentGearIndex == 0) return "R";
            if (currentGearIndex == 1) return "N";
            return (currentGearIndex - 1).ToString();
        }

        public bool ShiftUp()
        {
            if (Time.time - lastShiftTime < shiftCooldown) return false;
            if (currentGearIndex >= gearRatios.Length - 1) return false;

            currentGearIndex++;
            lastShiftTime = Time.time;
            EventBus.TriggerGearChanged(CurrentGear);
            return true;
        }

        public bool ShiftDown()
        {
            if (Time.time - lastShiftTime < shiftCooldown) return false;
            if (currentGearIndex <= 0) return false;

            currentGearIndex--;
            lastShiftTime = Time.time;
            EventBus.TriggerGearChanged(CurrentGear);
            return true;
        }

        /// <summary>
        /// Directly set gear by VR lever position. gearIndex: 0=R, 1=N, 2+=forward gears.
        /// </summary>
        public void SetGearDirect(int gearIndex)
        {
            gearIndex = Mathf.Clamp(gearIndex, 0, gearRatios.Length - 1);
            if (gearIndex == currentGearIndex) return;

            currentGearIndex = gearIndex;
            lastShiftTime = Time.time;
            EventBus.TriggerGearChanged(CurrentGear);
        }

        public void SetNeutral()
        {
            currentGearIndex = 1;
            EventBus.TriggerGearChanged(CurrentGear);
        }
    }
}
