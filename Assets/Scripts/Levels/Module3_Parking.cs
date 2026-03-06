using UnityEngine;
using VRCarSimulator.Vehicle;
using VRCarSimulator.Analytics;

namespace VRCarSimulator.Levels
{
    /// <summary>
    /// Module 3: L-Park & Parallel Park – reverse parking, parallel parking, tight maneuvering.
    /// Uses park zone colliders, wheel angle analysis, and ghost ideal route overlay.
    /// </summary>
    public class Module3_Parking : ModuleBase
    {
        [Header("Module 3 References")]
        [SerializeField] private VehicleController vehicle;
        [SerializeField] private AI.IdealRouteCalculator idealRoute;

        [Header("Park Zones")]
        [SerializeField] private ParkingZone[] parkingZones;

        [Header("Evaluation")]
        [SerializeField] private float maxParkingTime = 120f; // seconds
        [SerializeField] private float sampleInterval = 0.3f;
        [SerializeField] private float curbDistanceTolerance = 0.3f; // meters

        private int currentZoneIndex;
        private int totalManeuvers;
        private int gearChangeCount;
        private float parkingStartTime;
        private float sampleTimer;
        private int lastGear;
        private bool parkedSuccessfully;

        [System.Serializable]
        public class ParkingZone
        {
            public string zoneName;
            public Collider zoneCollider;
            public Transform idealPosition;
            public ParkType parkType;
            public Transform curbReference;

            public enum ParkType { LPark, ParallelPark, ReversePark }
        }

        protected override void OnModuleStart()
        {
            currentZoneIndex = 0;
            totalManeuvers = 0;
            gearChangeCount = 0;
            parkedSuccessfully = false;
            parkingStartTime = Time.time;
            lastGear = vehicle != null ? vehicle.CurrentGear : 0;

            Core.EventBus.OnGearChanged += OnGearChanged;
        }

        protected override void OnModuleUpdate()
        {
            if (vehicle == null) return;

            sampleTimer += Time.deltaTime;
            if (sampleTimer >= sampleInterval)
            {
                sampleTimer = 0f;
                idealRoute?.SampleDeviation(vehicle.transform.position);
            }

            // Check if vehicle is in the current parking zone and stopped
            CheckParkingCompletion();

            // Time limit per zone
            if (Time.time - parkingStartTime > maxParkingTime)
            {
                AdvanceZone(false);
            }
        }

        protected override void OnModuleEnd()
        {
            Core.EventBus.OnGearChanged -= OnGearChanged;

            var tracker = AnalyticsTracker.Instance;
            if (tracker != null)
            {
                tracker.RecordMetric("totalManeuvers", totalManeuvers);
                tracker.RecordMetric("gearChangeCount", gearChangeCount);
                tracker.RecordMetric("parkingDuration", Time.time - moduleStartTime);
                tracker.RecordMetric("routeAccuracy", idealRoute != null ? idealRoute.GetRouteAccuracy() : 0f);
                tracker.RecordMetric("curbDistance", GetCurbDistance());
            }

            if (scoringSystem != null)
            {
                scoringSystem.RecordMetric("totalManeuvers", totalManeuvers);
                scoringSystem.RecordMetric("parkingDuration", Time.time - moduleStartTime);
                scoringSystem.RecordMetric("routeAccuracy", idealRoute != null ? idealRoute.GetRouteAccuracy() : 0f);
                scoringSystem.RecordMetric("curbDistance", GetCurbDistance());
            }
        }

        private void OnGearChanged(int gear)
        {
            if (gear != lastGear)
            {
                gearChangeCount++;
                // Count direction changes as maneuvers
                if ((lastGear > 0 && gear < 0) || (lastGear < 0 && gear > 0))
                    totalManeuvers++;
                lastGear = gear;
            }
        }

        private void CheckParkingCompletion()
        {
            if (currentZoneIndex >= parkingZones.Length) return;

            var zone = parkingZones[currentZoneIndex];
            if (zone.zoneCollider == null) return;

            // Check if car is within the zone and nearly stopped
            bool inZone = zone.zoneCollider.bounds.Contains(vehicle.transform.position);
            bool stopped = vehicle.SpeedKmh < 1f;

            if (inZone && stopped)
            {
                float curbDist = GetCurbDistance();
                parkedSuccessfully = curbDist <= curbDistanceTolerance;
                AdvanceZone(parkedSuccessfully);
            }
        }

        private void AdvanceZone(bool success)
        {
            Core.EventBus.TriggerCheckpointReached(
                parkingZones[currentZoneIndex].zoneName + (success ? "_pass" : "_fail"));

            currentZoneIndex++;
            parkingStartTime = Time.time;
            idealRoute?.ResetDeviation();

            if (currentZoneIndex >= parkingZones.Length)
            {
                CompleteModule();
            }
        }

        private float GetCurbDistance()
        {
            if (currentZoneIndex >= parkingZones.Length) return 999f;
            var zone = parkingZones[currentZoneIndex];
            if (zone.curbReference == null || vehicle == null) return 999f;

            return Vector3.Distance(
                new Vector3(vehicle.transform.position.x, 0f, vehicle.transform.position.z),
                new Vector3(zone.curbReference.position.x, 0f, zone.curbReference.position.z));
        }
    }
}
