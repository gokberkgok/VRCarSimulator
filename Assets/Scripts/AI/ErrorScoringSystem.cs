using UnityEngine;
using System.Collections.Generic;

namespace VRCarSimulator.AI
{
    /// <summary>
    /// Calculates an error score from module analytics, weighted by importance.
    /// Outputs a final 0-100 score and detailed breakdown.
    /// </summary>
    public class ErrorScoringSystem : MonoBehaviour
    {
        [Header("Scoring Weights")]
        [SerializeField] private ScoringWeight[] weights;

        private Dictionary<string, float> rawMetrics = new Dictionary<string, float>();

        [System.Serializable]
        public class ScoringWeight
        {
            public string metricKey;
            public float weight = 1f;
            public float idealValue;
            public float worstValue;
            public bool lowerIsBetter = true; // e.g., reaction time, violations
        }

        /// <summary>
        /// Record a metric value for scoring.
        /// </summary>
        public void RecordMetric(string key, float value)
        {
            rawMetrics[key] = value;
        }

        /// <summary>
        /// Calculate final weighted score (0-100).
        /// </summary>
        public float CalculateScore()
        {
            if (weights == null || weights.Length == 0) return 0f;

            float totalWeight = 0f;
            float weightedSum = 0f;

            foreach (var w in weights)
            {
                if (!rawMetrics.ContainsKey(w.metricKey)) continue;

                float raw = rawMetrics[w.metricKey];
                float normalized;

                if (w.lowerIsBetter)
                    normalized = 1f - Mathf.InverseLerp(w.idealValue, w.worstValue, raw);
                else
                    normalized = Mathf.InverseLerp(w.worstValue, w.idealValue, raw);

                normalized = Mathf.Clamp01(normalized);
                weightedSum += normalized * w.weight;
                totalWeight += w.weight;
            }

            return totalWeight > 0f ? (weightedSum / totalWeight) * 100f : 0f;
        }

        /// <summary>
        /// Returns per-metric scores as a breakdown.
        /// </summary>
        public Dictionary<string, float> GetBreakdown()
        {
            var breakdown = new Dictionary<string, float>();
            if (weights == null) return breakdown;

            foreach (var w in weights)
            {
                if (!rawMetrics.ContainsKey(w.metricKey)) continue;

                float raw = rawMetrics[w.metricKey];
                float normalized;

                if (w.lowerIsBetter)
                    normalized = 1f - Mathf.InverseLerp(w.idealValue, w.worstValue, raw);
                else
                    normalized = Mathf.InverseLerp(w.worstValue, w.idealValue, raw);

                breakdown[w.metricKey] = Mathf.Clamp01(normalized) * 100f;
            }

            return breakdown;
        }

        public void Reset()
        {
            rawMetrics.Clear();
        }
    }
}
