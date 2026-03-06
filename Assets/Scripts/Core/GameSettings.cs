using UnityEngine;

namespace VRCarSimulator.Core
{
    /// <summary>
    /// Global settings via ScriptableObject – simulation fidelity, VR settings, etc.
    /// </summary>
    [CreateAssetMenu(fileName = "GameSettings", menuName = "VRCarSim/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        [Header("Simulation")]
        public SimulationLevel simulationLevel = SimulationLevel.Realistic;
        public bool enableClutchSimulation = true;
        public bool enableABS = true;

        [Header("VR")]
        public bool vrEnabled = true;
        public float hapticIntensity = 0.5f;
        public float hapticDuration = 0.1f;

        [Header("Graphics")]
        public int targetFrameRate = 90;
        public bool enableLOD = true;
        public bool enableOcclusionCulling = true;

        [Header("Audio")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float engineVolume = 0.8f;
        [Range(0f, 1f)] public float uiVolume = 0.7f;

        [Header("Difficulty")]
        public float baseDifficultyMultiplier = 1f;
        public bool adaptiveDifficulty = true;
    }

    public enum SimulationLevel
    {
        Casual,
        Standard,
        Realistic
    }
}
