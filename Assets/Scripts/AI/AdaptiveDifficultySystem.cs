using UnityEngine;
using System.Collections.Generic;

namespace VRCarSimulator.AI
{
    /// <summary>
    /// Tracks player error patterns and adjusts difficulty dynamically.
    /// Records error types, frequencies, and generates adaptive difficulty multipliers.
    /// </summary>
    public class AdaptiveDifficultySystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float difficultyAdjustInterval = 30f; // seconds
        [SerializeField] private float difficultyStep = 0.1f;
        [SerializeField] private float minDifficulty = 0.5f;
        [SerializeField] private float maxDifficulty = 2.0f;

        [Header("Error Thresholds")]
        [SerializeField] private int errorsToDecrease = 5;  // lower difficulty
        [SerializeField] private int cleanRunToIncrease = 3; // raise difficulty

        private float currentDifficulty = 1f;
        private float timer;
        private int recentErrors;
        private int consecutiveCleanIntervals;
        private Dictionary<string, int> errorPatterns = new Dictionary<string, int>();

        public float CurrentDifficulty => currentDifficulty;
        public IReadOnlyDictionary<string, int> ErrorPatterns => errorPatterns;

        public event System.Action<float> OnDifficultyChanged;

        private void OnEnable()
        {
            Core.EventBus.OnCollision += RecordCollision;
            Core.EventBus.OnTrafficViolation += RecordViolation;
        }

        private void OnDisable()
        {
            Core.EventBus.OnCollision -= RecordCollision;
            Core.EventBus.OnTrafficViolation -= RecordViolation;
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= difficultyAdjustInterval)
            {
                EvaluateAndAdjust();
                timer = 0f;
            }
        }

        private void RecordCollision(Core.CollisionType type, float force)
        {
            string key = $"collision_{type}";
            RecordError(key);
        }

        private void RecordViolation(string violation)
        {
            RecordError(violation);
        }

        private void RecordError(string errorType)
        {
            recentErrors++;
            if (errorPatterns.ContainsKey(errorType))
                errorPatterns[errorType]++;
            else
                errorPatterns[errorType] = 1;
        }

        private void EvaluateAndAdjust()
        {
            float previousDifficulty = currentDifficulty;

            if (recentErrors >= errorsToDecrease)
            {
                currentDifficulty -= difficultyStep;
                consecutiveCleanIntervals = 0;
            }
            else if (recentErrors == 0)
            {
                consecutiveCleanIntervals++;
                if (consecutiveCleanIntervals >= cleanRunToIncrease)
                {
                    currentDifficulty += difficultyStep;
                    consecutiveCleanIntervals = 0;
                }
            }
            else
            {
                consecutiveCleanIntervals = 0;
            }

            currentDifficulty = Mathf.Clamp(currentDifficulty, minDifficulty, maxDifficulty);
            recentErrors = 0;

            if (!Mathf.Approximately(previousDifficulty, currentDifficulty))
            {
                OnDifficultyChanged?.Invoke(currentDifficulty);
            }
        }

        /// <summary>
        /// Returns the most frequent error type for targeted feedback.
        /// </summary>
        public string GetMostFrequentError()
        {
            string worst = null;
            int maxCount = 0;
            foreach (var kvp in errorPatterns)
            {
                if (kvp.Value > maxCount)
                {
                    maxCount = kvp.Value;
                    worst = kvp.Key;
                }
            }
            return worst;
        }

        public void Reset()
        {
            currentDifficulty = 1f;
            errorPatterns.Clear();
            recentErrors = 0;
            consecutiveCleanIntervals = 0;
            timer = 0f;
        }
    }
}
