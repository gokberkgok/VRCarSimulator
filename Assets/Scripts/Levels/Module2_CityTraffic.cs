using UnityEngine;
using VRCarSimulator.Vehicle;
using VRCarSimulator.Analytics;

namespace VRCarSimulator.Levels
{
    /// <summary>
    /// Module 2: City Traffic – traffic lights, intersection priority, sign tracking.
    /// Measures brake reaction time, following distance, speed limit compliance.
    /// </summary>
    public class Module2_CityTraffic : ModuleBase
    {
        [Header("Module 2 References")]
        [SerializeField] private VehicleController vehicle;
        [SerializeField] private ReactionTimeMeasurer reactionMeasurer;
        [SerializeField] private AI.AdaptiveDifficultySystem difficultySystem;

        [Header("Traffic Settings")]
        [SerializeField] private float speedLimitKmh = 50f;
        [SerializeField] private float minFollowingDistanceMeters = 5f;
        [SerializeField] private float redLightCheckInterval = 0.5f;

        [Header("Traffic Light Zones")]
        [SerializeField] private TrafficLightZone[] trafficLightZones;

        // Analytics
        private int redLightViolations;
        private int speedLimitViolations;
        private float speedViolationDuration;
        private int followingDistanceViolations;
        private float totalBrakeReactionTime;
        private int brakeReactionCount;

        private float checkTimer;

        [System.Serializable]
        public class TrafficLightZone
        {
            public Collider zoneCollider;
            public TrafficLightState currentState;
            public enum TrafficLightState { Red, Yellow, Green }
        }

        protected override void OnModuleStart()
        {
            redLightViolations = 0;
            speedLimitViolations = 0;
            speedViolationDuration = 0f;
            followingDistanceViolations = 0;
            totalBrakeReactionTime = 0f;
            brakeReactionCount = 0;
        }

        protected override void OnModuleUpdate()
        {
            if (vehicle == null) return;

            CheckSpeedLimit();
            CheckFollowingDistance();

            checkTimer += Time.deltaTime;
            if (checkTimer >= redLightCheckInterval)
            {
                checkTimer = 0f;
                CheckTrafficLights();
            }
        }

        protected override void OnModuleEnd()
        {
            var tracker = AnalyticsTracker.Instance;
            if (tracker != null)
            {
                tracker.RecordMetric("redLightViolations", redLightViolations);
                tracker.RecordMetric("speedLimitViolations", speedLimitViolations);
                tracker.RecordMetric("speedViolationDuration", speedViolationDuration);
                tracker.RecordMetric("followingDistanceViolations", followingDistanceViolations);
                float avgReaction = brakeReactionCount > 0 ? totalBrakeReactionTime / brakeReactionCount : 0f;
                tracker.RecordMetric("avgBrakeReactionTime", avgReaction);
            }

            if (scoringSystem != null)
            {
                scoringSystem.RecordMetric("redLightViolations", redLightViolations);
                scoringSystem.RecordMetric("speedViolationDuration", speedViolationDuration);
                float avgReaction = brakeReactionCount > 0 ? totalBrakeReactionTime / brakeReactionCount : 0f;
                scoringSystem.RecordMetric("avgBrakeReactionTime", avgReaction);
            }
        }

        private void CheckSpeedLimit()
        {
            if (vehicle.SpeedKmh > speedLimitKmh + 5f) // 5 km/h tolerance
            {
                speedViolationDuration += Time.deltaTime;
                if (vehicle.SpeedKmh > speedLimitKmh + 10f)
                {
                    speedLimitViolations++;
                    Core.EventBus.TriggerTrafficViolation("speedLimit");
                }
            }
        }

        private void CheckFollowingDistance()
        {
            // Raycast forward to detect vehicle ahead
            Ray ray = new Ray(vehicle.transform.position + Vector3.up * 0.5f, vehicle.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 50f))
            {
                if (hit.collider.CompareTag("Vehicle") || hit.collider.CompareTag("TrafficAI"))
                {
                    float distance = hit.distance;
                    float safeDistance = minFollowingDistanceMeters * (vehicle.SpeedKmh / 50f);
                    if (distance < safeDistance)
                    {
                        followingDistanceViolations++;
                        Core.EventBus.TriggerTrafficViolation("followingDistance");
                    }
                }
            }
        }

        private void CheckTrafficLights()
        {
            if (trafficLightZones == null) return;

            foreach (var zone in trafficLightZones)
            {
                if (zone.zoneCollider == null) continue;
                if (zone.currentState == TrafficLightZone.TrafficLightState.Red)
                {
                    if (zone.zoneCollider.bounds.Contains(vehicle.transform.position))
                    {
                        if (vehicle.SpeedKmh > 3f) // Not stopped
                        {
                            redLightViolations++;
                            Core.EventBus.TriggerTrafficViolation("redLight");
                        }
                    }
                }
            }
        }

        /// <summary>Called when a brake-test event triggers (e.g., sudden hazard).</summary>
        public void OnBrakeTestEvent(string eventId)
        {
            reactionMeasurer?.StartMeasurement(eventId);
        }

        /// <summary>Called when the player presses the brake.</summary>
        public void OnBrakePressed()
        {
            if (reactionMeasurer != null && reactionMeasurer.IsMeasuring)
            {
                float reaction = reactionMeasurer.StopMeasurement();
                if (reaction >= 0f)
                {
                    totalBrakeReactionTime += reaction;
                    brakeReactionCount++;
                }
            }
        }

        public void SetSpeedLimit(float newLimit)
        {
            speedLimitKmh = newLimit;
        }
    }
}
