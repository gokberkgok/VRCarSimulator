using UnityEngine;
using VRCarSimulator.Vehicle;
using VRCarSimulator.Analytics;

namespace VRCarSimulator.Levels
{
    /// <summary>
    /// Module 5: Free Drive – open-world free driving with background performance analysis.
    /// No pass/fail, but analytics still record everything.
    /// </summary>
    public class Module5_FreeDrive : ModuleBase
    {
        [Header("Module 5 References")]
        [SerializeField] private VehicleController vehicle;

        [Header("Background Analysis")]
        [SerializeField] private float analysisInterval = 5f; // sample every N seconds

        private float analysisTimer;
        private float totalDistanceDriven;
        private Vector3 lastPosition;
        private float maxSpeedReached;
        private float smoothDrivingScore; // lower lateral acceleration = smoother

        protected override void OnModuleStart()
        {
            totalDistanceDriven = 0f;
            maxSpeedReached = 0f;
            smoothDrivingScore = 100f;
            analysisTimer = 0f;
            if (vehicle != null)
                lastPosition = vehicle.transform.position;
        }

        protected override void OnModuleUpdate()
        {
            if (vehicle == null) return;

            // Track distance
            float frameDist = Vector3.Distance(vehicle.transform.position, lastPosition);
            totalDistanceDriven += frameDist;
            lastPosition = vehicle.transform.position;

            // Track max speed
            if (vehicle.SpeedKmh > maxSpeedReached)
                maxSpeedReached = vehicle.SpeedKmh;

            // Smooth driving analysis
            AnalyzeSmoothness();

            // Periodic metric snapshot
            analysisTimer += Time.deltaTime;
            if (analysisTimer >= analysisInterval)
            {
                analysisTimer = 0f;
                RecordSnapshot();
            }
        }

        protected override void OnModuleEnd()
        {
            var tracker = AnalyticsTracker.Instance;
            if (tracker != null)
            {
                tracker.RecordMetric("totalDistanceKm", totalDistanceDriven / 1000f);
                tracker.RecordMetric("maxSpeedKmh", maxSpeedReached);
                tracker.RecordMetric("smoothDrivingScore", smoothDrivingScore);
                tracker.RecordMetric("freeDriveDuration", Time.time - moduleStartTime);
            }
        }

        private void AnalyzeSmoothness()
        {
            if (vehicle.Rigidbody == null) return;
            Vector3 localVelocity = vehicle.transform.InverseTransformDirection(vehicle.Rigidbody.velocity);
            float lateralG = Mathf.Abs(localVelocity.x);

            // Penalize aggressive lateral movement
            if (lateralG > 3f)
            {
                smoothDrivingScore -= Time.deltaTime * (lateralG - 3f) * 2f;
                smoothDrivingScore = Mathf.Max(0f, smoothDrivingScore);
            }
        }

        private void RecordSnapshot()
        {
            AnalyticsTracker.Instance?.RecordTimeSample("speedSnapshots", vehicle.SpeedKmh);
        }

        /// <summary>Exit free drive (called by UI button).</summary>
        public void ExitFreeDrive()
        {
            CompleteModule();
        }
    }
}
