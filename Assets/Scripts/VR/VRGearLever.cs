using UnityEngine;

namespace VRCarSimulator.VR
{
    /// <summary>
    /// VR-interactive gear lever. Maps lever position to gear index.
    /// Designed for XR Grab Interactable.
    /// </summary>
    public class VRGearLever : MonoBehaviour
    {
       /* [Header("References")]
       // [SerializeField] private VRInputManager inputManager;

        [Header("Gear Positions (local Y offsets)")]
        [SerializeField] private float[] gearPositions = { -0.15f, 0f, 0.05f, 0.10f, 0.15f, 0.20f, 0.25f };
        // Index: 0=R, 1=N, 2=1st, 3=2nd, 4=3rd, 5=4th, 6=5th

        [Header("Settings")]
        [SerializeField] private float snapDistance = 0.03f;
        [SerializeField] private float snapSpeed = 15f;

        [Header("Visual")]
        [SerializeField] private Transform leverMesh;

        private int currentGearIndex = 1; // Start in Neutral
        private bool isGrabbed;
        private float targetY;

        public int CurrentGearIndex => currentGearIndex;

        private void Start()
        {
            targetY = gearPositions[currentGearIndex];
        }

        private void Update()
        {
            if (!isGrabbed)
            {
                SnapToGear();
            }
            else
            {
                DetectGearFromPosition();
            }

            UpdateVisual();
        }

        public void OnGrab() => isGrabbed = true;

        public void OnRelease()
        {
            isGrabbed = false;
            SnapToNearestGear();
        }

        /// <summary>
        /// Set lever position from VR hand tracking (local Y).
        /// </summary>
        public void SetLeverPosition(float localY)
        {
            if (!isGrabbed) return;
            targetY = localY;
        }

        private void DetectGearFromPosition()
        {
            int nearest = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i < gearPositions.Length; i++)
            {
                float dist = Mathf.Abs(targetY - gearPositions[i]);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = i;
                }
            }

            if (minDistance <= snapDistance && nearest != currentGearIndex)
            {
                currentGearIndex = nearest;
                inputManager?.OnGearSetDirect(currentGearIndex);
            }
        }

        private void SnapToNearestGear()
        {
            targetY = gearPositions[currentGearIndex];
        }

        private void SnapToGear()
        {
            targetY = Mathf.Lerp(targetY, gearPositions[currentGearIndex], Time.deltaTime * snapSpeed);
        }

        private void UpdateVisual()
        {
            if (leverMesh == null) return;
            Vector3 pos = leverMesh.localPosition;
            pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime * snapSpeed);
            leverMesh.localPosition = pos;
        }*/
    }
}
