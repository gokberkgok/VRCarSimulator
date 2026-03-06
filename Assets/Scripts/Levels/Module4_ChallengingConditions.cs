using UnityEngine;
using VRCarSimulator.Vehicle;
using VRCarSimulator.Analytics;

namespace VRCarSimulator.Levels
{
    /// <summary>
    /// Module 4: Challenging Conditions – night driving, rain/slippery roads, sudden pedestrians.
    /// Dynamic friction, ABS response, brake distance graph.
    /// </summary>
    public class Module4_ChallengingConditions : ModuleBase
    {
        [Header("Module 4 References")]
        [SerializeField] private VehicleController vehicle;
        [SerializeField] private BrakeSystem brakeSystem;
        [SerializeField] private ReactionTimeMeasurer reactionMeasurer;

        [Header("Weather")]
        [SerializeField] private WeatherCondition currentWeather = WeatherCondition.Clear;
        [SerializeField] private Light directionalLight;
        [SerializeField] private ParticleSystem rainParticles;
        [SerializeField] private ParticleSystem fogParticles;

        [Header("Friction Settings")]
        [SerializeField] private float dryFriction = 1.0f;
        [SerializeField] private float wetFriction = 0.5f;
        [SerializeField] private float nightVisibilityRange = 40f;

        [Header("Pedestrian Hazards")]
        [SerializeField] private PedestrianHazard[] pedestrianHazards;

        [Header("Night Settings")]
        [SerializeField] private float nightAmbientIntensity = 0.1f;
        [SerializeField] private GameObject[] vehicleHeadlights;

        // Analytics
        private int collisionCount;
        private float controlLossDuration;
        private bool wasSliding;

        public enum WeatherCondition { Clear, Rain, Night, NightRain }

        [System.Serializable]
        public class PedestrianHazard
        {
            public GameObject pedestrianObject;
            public float triggerDistance = 30f;
            public Transform spawnPoint;
            public Transform destination;
            public float walkSpeed = 1.5f;
            public bool triggered;
        }

        protected override void OnModuleStart()
        {
            collisionCount = 0;
            controlLossDuration = 0f;

            ApplyWeather(currentWeather);
            Core.EventBus.OnCollision += OnCollisionEvent;
        }

        protected override void OnModuleUpdate()
        {
            if (vehicle == null) return;

            CheckControlLoss();
            CheckPedestrianHazards();
            UpdatePedestrianMovement();
        }

        protected override void OnModuleEnd()
        {
            Core.EventBus.OnCollision -= OnCollisionEvent;

            var tracker = AnalyticsTracker.Instance;
            if (tracker != null)
            {
                tracker.RecordMetric("collisionCount", collisionCount);
                tracker.RecordMetric("controlLossDuration", controlLossDuration);
                tracker.RecordMetric("controlLossRate", controlLossDuration / Mathf.Max(1f, Time.time - moduleStartTime));
            }

            if (scoringSystem != null)
            {
                scoringSystem.RecordMetric("collisionCount", collisionCount);
                scoringSystem.RecordMetric("controlLossRate", controlLossDuration / Mathf.Max(1f, Time.time - moduleStartTime));
            }
        }

        public void ApplyWeather(WeatherCondition weather)
        {
            currentWeather = weather;

            bool isRain = weather == WeatherCondition.Rain || weather == WeatherCondition.NightRain;
            bool isNight = weather == WeatherCondition.Night || weather == WeatherCondition.NightRain;

            // Friction
            float friction = isRain ? wetFriction : dryFriction;
            vehicle?.SetWheelFriction(friction, friction);

            // Rain particles
            if (rainParticles != null)
            {
                if (isRain) rainParticles.Play();
                else rainParticles.Stop();
            }

            // Night lighting
            if (directionalLight != null)
                directionalLight.intensity = isNight ? nightAmbientIntensity : 1f;

            // Headlights
            if (vehicleHeadlights != null)
            {
                foreach (var light in vehicleHeadlights)
                    if (light != null) light.SetActive(isNight);
            }

            // Fog
            if (fogParticles != null)
            {
                if (isRain) fogParticles.Play();
                else fogParticles.Stop();
            }

            // Render distance
            if (isNight && Camera.main != null)
                Camera.main.farClipPlane = nightVisibilityRange;
        }

        private void CheckControlLoss()
        {
            // Detect significant lateral slip
            if (vehicle.Rigidbody == null) return;
            Vector3 localVelocity = vehicle.transform.InverseTransformDirection(vehicle.Rigidbody.velocity);
            float lateralSlip = Mathf.Abs(localVelocity.x);

            bool sliding = lateralSlip > 2f && vehicle.SpeedKmh > 10f;
            if (sliding)
            {
                controlLossDuration += Time.deltaTime;
            }

            if (sliding && !wasSliding)
            {
                Core.EventBus.TriggerTrafficViolation("controlLoss");
            }
            wasSliding = sliding;
        }

        private void CheckPedestrianHazards()
        {
            if (pedestrianHazards == null || vehicle == null) return;

            foreach (var hazard in pedestrianHazards)
            {
                if (hazard.triggered || hazard.pedestrianObject == null || hazard.spawnPoint == null) continue;

                float dist = Vector3.Distance(vehicle.transform.position, hazard.spawnPoint.position);
                if (dist <= hazard.triggerDistance)
                {
                    hazard.triggered = true;
                    hazard.pedestrianObject.SetActive(true);
                    hazard.pedestrianObject.transform.position = hazard.spawnPoint.position;

                    // Start reaction time measurement
                    reactionMeasurer?.StartMeasurement("pedestrian");
                }
            }
        }

        private void UpdatePedestrianMovement()
        {
            if (pedestrianHazards == null) return;

            foreach (var hazard in pedestrianHazards)
            {
                if (!hazard.triggered || hazard.pedestrianObject == null || hazard.destination == null) continue;

                hazard.pedestrianObject.transform.position = Vector3.MoveTowards(
                    hazard.pedestrianObject.transform.position,
                    hazard.destination.position,
                    hazard.walkSpeed * Time.deltaTime);
            }
        }

        private void OnCollisionEvent(Core.CollisionType type, float force)
        {
            collisionCount++;
        }
    }
}
