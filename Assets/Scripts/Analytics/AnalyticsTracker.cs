using UnityEngine;
using System.Collections.Generic;

namespace VRCarSimulator.Analytics
{
    /// <summary>
    /// Central analytics tracker – records all performance metrics per module session.
    /// Feeds into ErrorScoringSystem and JSON save system.
    /// </summary>
    public class AnalyticsTracker : MonoBehaviour
    {
        public static AnalyticsTracker Instance { get; private set; }

        private Dictionary<string, float> sessionMetrics = new Dictionary<string, float>();
        private Dictionary<string, List<float>> sessionTimeSeries = new Dictionary<string, List<float>>();
        private float sessionStartTime;

        public float SessionDuration => Time.time - sessionStartTime;
        public IReadOnlyDictionary<string, float> Metrics => sessionMetrics;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            Core.EventBus.OnMetricRecorded += RecordMetric;
            Core.EventBus.OnCollision += OnCollision;
            Core.EventBus.OnTrafficViolation += OnViolation;
        }

        private void OnDisable()
        {
            Core.EventBus.OnMetricRecorded -= RecordMetric;
            Core.EventBus.OnCollision -= OnCollision;
            Core.EventBus.OnTrafficViolation -= OnViolation;
        }

        public void StartSession()
        {
            sessionStartTime = Time.time;
            sessionMetrics.Clear();
            sessionTimeSeries.Clear();
            RecordMetric("collisionCount", 0f);
            RecordMetric("violationCount", 0f);
        }

        public void RecordMetric(string key, float value)
        {
            sessionMetrics[key] = value;
        }

        public void IncrementMetric(string key, float amount = 1f)
        {
            if (sessionMetrics.ContainsKey(key))
                sessionMetrics[key] += amount;
            else
                sessionMetrics[key] = amount;
        }

        public void RecordTimeSample(string key, float value)
        {
            if (!sessionTimeSeries.ContainsKey(key))
                sessionTimeSeries[key] = new List<float>();
            sessionTimeSeries[key].Add(value);
        }

        public float GetMetric(string key, float defaultValue = 0f)
        {
            return sessionMetrics.ContainsKey(key) ? sessionMetrics[key] : defaultValue;
        }

        public float GetTimeSeriesAverage(string key)
        {
            if (!sessionTimeSeries.ContainsKey(key) || sessionTimeSeries[key].Count == 0)
                return 0f;

            float sum = 0f;
            foreach (var v in sessionTimeSeries[key]) sum += v;
            return sum / sessionTimeSeries[key].Count;
        }

        private void OnCollision(Core.CollisionType type, float force)
        {
            IncrementMetric("collisionCount");
            IncrementMetric($"collision_{type}");
        }

        private void OnViolation(string violation)
        {
            IncrementMetric("violationCount");
            IncrementMetric($"violation_{violation}");
        }

        /// <summary>
        /// Returns all metrics as a serializable dictionary for JSON save.
        /// </summary>
        public Dictionary<string, float> GetAllMetrics()
        {
            var result = new Dictionary<string, float>(sessionMetrics)
            {
                ["sessionDuration"] = SessionDuration
            };
            return result;
        }
    }
}
