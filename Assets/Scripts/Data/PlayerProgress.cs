using System;
using System.Collections.Generic;

namespace VRCarSimulator.Data
{
    /// <summary>
    /// Serializable player progress data – saved/loaded as JSON.
    /// </summary>
    [Serializable]
    public class PlayerProgress
    {
        public string playerId;
        public Dictionary<string, ModuleProgress> modules = new Dictionary<string, ModuleProgress>();
        public float totalScore;
        public string lastPlayed;

        public PlayerProgress()
        {
            playerId = Guid.NewGuid().ToString().Substring(0, 8);
            lastPlayed = DateTime.Now.ToString("yyyy-MM-dd");
        }

        public PlayerProgress(string id)
        {
            playerId = id;
            lastPlayed = DateTime.Now.ToString("yyyy-MM-dd");
        }

        public void UpdateLastPlayed()
        {
            lastPlayed = DateTime.Now.ToString("yyyy-MM-dd");
        }

        public ModuleProgress GetOrCreateModule(string moduleKey)
        {
            if (!modules.ContainsKey(moduleKey))
                modules[moduleKey] = new ModuleProgress();
            return modules[moduleKey];
        }

        public void RecalculateTotalScore()
        {
            if (modules.Count == 0) { totalScore = 0f; return; }

            float sum = 0f;
            int count = 0;
            foreach (var m in modules.Values)
            {
                if (m.completed)
                {
                    sum += m.score;
                    count++;
                }
            }
            totalScore = count > 0 ? sum / count : 0f;
        }
    }

    [Serializable]
    public class ModuleProgress
    {
        public bool completed;
        public float score;
        public int attempts;
        public float bestScore;
        public Dictionary<string, float> metrics = new Dictionary<string, float>();

        public void RecordAttempt(float attemptScore, Dictionary<string, float> attemptMetrics = null)
        {
            attempts++;
            score = attemptScore;
            if (attemptScore > bestScore) bestScore = attemptScore;
            if (attemptScore >= 70f) completed = true;

            if (attemptMetrics != null)
            {
                foreach (var kvp in attemptMetrics)
                    metrics[kvp.Key] = kvp.Value;
            }
        }
    }
}
