using UnityEngine;

namespace VRCarSimulator.Levels
{
    /// <summary>
    /// Base class for all module controllers.
    /// Handles lifecycle (start, evaluate, complete) and analytics integration.
    /// </summary>
    public abstract class ModuleBase : MonoBehaviour
    {
        [Header("Module Config")]
        [SerializeField] protected Data.ModuleConfig config;

        [Header("References")]
        [SerializeField] protected AI.ErrorScoringSystem scoringSystem;

        protected bool isActive;
        protected float moduleStartTime;
        protected float moduleScore;

        public bool IsActive => isActive;
        public float Score => moduleScore;
        public Data.ModuleConfig Config => config;

        public event System.Action<float> OnModuleComplete; // score

        public virtual void StartModule()
        {
            isActive = true;
            moduleStartTime = Time.time;
            moduleScore = 0f;

            Analytics.AnalyticsTracker.Instance?.StartSession();

            if (scoringSystem != null) scoringSystem.Reset();

            OnModuleStart();
        }

        public virtual void CompleteModule()
        {
            isActive = false;

            moduleScore = scoringSystem != null ? scoringSystem.CalculateScore() : 0f;

            // Save results
            var metrics = Analytics.AnalyticsTracker.Instance?.GetAllMetrics();
            string moduleKey = config != null ? $"module{config.moduleIndex}" : "unknown";
            Data.SaveSystem.Instance?.SaveModuleResult(moduleKey, moduleScore, metrics);

            OnModuleComplete?.Invoke(moduleScore);
            Core.GameManager.Instance?.CompleteModule();

            OnModuleEnd();
        }

        protected virtual void Update()
        {
            if (!isActive) return;

            // Time limit check
            if (config != null && config.timeLimit > 0f)
            {
                float elapsed = Time.time - moduleStartTime;
                if (elapsed >= config.timeLimit)
                {
                    CompleteModule();
                    return;
                }
            }

            OnModuleUpdate();
        }

        /// <summary>Override in subclass for module-specific initialization.</summary>
        protected abstract void OnModuleStart();

        /// <summary>Override in subclass for module-specific per-frame logic.</summary>
        protected abstract void OnModuleUpdate();

        /// <summary>Override in subclass for module-specific cleanup.</summary>
        protected abstract void OnModuleEnd();
    }
}
