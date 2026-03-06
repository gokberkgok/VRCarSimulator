using UnityEngine;

namespace VRCarSimulator.Data
{
    /// <summary>
    /// ScriptableObject definition for module configuration.
    /// Each module (1-5) has its own asset defining rules, scoring weights, and descriptions.
    /// </summary>
    [CreateAssetMenu(fileName = "ModuleConfig", menuName = "VRCarSim/Module Config")]
    public class ModuleConfig : ScriptableObject
    {
        [Header("Info")]
        public int moduleIndex;
        public string moduleNameTR;    // Turkish name
        public string moduleNameEN;    // English name
        [TextArea] public string description;

        [Header("Scene")]
        public string sceneName;

        [Header("Scoring")]
        public float passingScore = 70f;
        public float timeLimit = 300f;  // seconds, 0 = no limit
        public ScoringMetric[] scoringMetrics;

        [Header("Difficulty")]
        public float baseDifficulty = 1f;
        public bool adaptiveDifficultyEnabled = true;

        [Header("Checklist")]
        public string[] requiredCheckpoints;
    }

    [System.Serializable]
    public class ScoringMetric
    {
        public string metricKey;
        public string displayNameTR;
        public float weight = 1f;
        public float idealValue;
        public float worstValue;
        public bool lowerIsBetter = true;
    }
}
