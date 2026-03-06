using UnityEngine;

namespace VRCarSimulator.VR
{
    /// <summary>
    /// Tracks the player's gaze direction for mirror checks, blind spot detection,
    /// and cockpit interaction analysis.
    /// Uses the VR headset forward direction (or camera forward as fallback).
    /// </summary>
    public class GazeTracker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera vrCamera;

        [Header("Gaze Zones")]
        [SerializeField] private GazeZone[] gazeZones;

        [Header("Settings")]
        [SerializeField] private float gazeRayLength = 10f;
        [SerializeField] private LayerMask gazeLayerMask = ~0;
        [SerializeField] private float dwellTimeThreshold = 0.5f; // seconds to count as "looked"

        private GazeZone currentZone;
        private float currentZoneDwellTime;
        private string lastGazeTarget;

        public GazeZone CurrentGazeZone => currentZone;
        public float CurrentDwellTime => currentZoneDwellTime;
        public string LastGazeTarget => lastGazeTarget;

        public event System.Action<string, float> OnGazeDwell; // zoneName, dwellTime
        public event System.Action<string> OnGazeEnter;
        public event System.Action<string> OnGazeExit;

        private void Update()
        {
            if (vrCamera == null) return;

            Ray gazeRay = new Ray(vrCamera.transform.position, vrCamera.transform.forward);
            EvaluateGazeZones(gazeRay);
            CheckRaycast(gazeRay);
        }

        private void EvaluateGazeZones(Ray gazeRay)
        {
            GazeZone hitZone = null;

            if (gazeZones != null)
            {
                foreach (var zone in gazeZones)
                {
                    if (zone == null || zone.zoneCollider == null) continue;

                    float angle = Vector3.Angle(gazeRay.direction,
                        (zone.zoneCollider.bounds.center - gazeRay.origin).normalized);

                    if (angle <= zone.detectionAngle)
                    {
                        hitZone = zone;
                        break;
                    }
                }
            }

            if (hitZone != currentZone)
            {
                if (currentZone != null)
                    OnGazeExit?.Invoke(currentZone.zoneName);

                currentZone = hitZone;
                currentZoneDwellTime = 0f;

                if (currentZone != null)
                    OnGazeEnter?.Invoke(currentZone.zoneName);
            }

            if (currentZone != null)
            {
                currentZoneDwellTime += Time.deltaTime;

                if (currentZoneDwellTime >= dwellTimeThreshold)
                {
                    OnGazeDwell?.Invoke(currentZone.zoneName, currentZoneDwellTime);
                }
            }
        }

        private void CheckRaycast(Ray gazeRay)
        {
            if (Physics.Raycast(gazeRay, out RaycastHit hit, gazeRayLength, gazeLayerMask))
            {
                lastGazeTarget = hit.collider.gameObject.name;
            }
            else
            {
                lastGazeTarget = null;
            }
        }

        /// <summary>
        /// Check if the player has looked at a specific zone for at least the required duration.
        /// </summary>
        public bool HasLookedAt(string zoneName, float requiredDuration = -1f)
        {
            if (requiredDuration < 0) requiredDuration = dwellTimeThreshold;
            return currentZone != null
                && currentZone.zoneName == zoneName
                && currentZoneDwellTime >= requiredDuration;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (vrCamera == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(vrCamera.transform.position, vrCamera.transform.forward * gazeRayLength);
        }
#endif
    }

    [System.Serializable]
    public class GazeZone
    {
        public string zoneName;          // e.g., "LeftMirror", "RearMirror", "BlindSpotLeft"
        public Collider zoneCollider;
        public float detectionAngle = 15f;

        [Tooltip("Minimum dwell time to register as a valid check (seconds)")]
        public float requiredDwellTime = 0.5f;
    }
}
