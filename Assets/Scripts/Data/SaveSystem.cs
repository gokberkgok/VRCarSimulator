using UnityEngine;
using System.IO;

namespace VRCarSimulator.Data
{
    /// <summary>
    /// JSON-based save/load system for PlayerProgress.
    /// Stores data in Application.persistentDataPath.
    /// Ready for future backend integration (swap SaveToFile/LoadFromFile with HTTP calls).
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private string saveFileName = "player_progress.json";
        [SerializeField] private bool prettyPrint = true;

        private PlayerProgress currentProgress;

        public PlayerProgress CurrentProgress => currentProgress;
        public string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Load();
        }

        /// <summary>
        /// Save current progress to JSON file.
        /// </summary>
        public void Save()
        {
            if (currentProgress == null)
            {
                Debug.LogWarning("[SaveSystem] No progress data to save.");
                return;
            }

            currentProgress.UpdateLastPlayed();
            currentProgress.RecalculateTotalScore();

            string json = JsonUtility.ToJson(new PlayerProgressWrapper(currentProgress), prettyPrint);
            File.WriteAllText(SavePath, json);
            Debug.Log($"[SaveSystem] Saved to {SavePath}");
        }

        /// <summary>
        /// Load progress from JSON file, or create new if not found.
        /// </summary>
        public void Load()
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                var wrapper = JsonUtility.FromJson<PlayerProgressWrapper>(json);
                currentProgress = wrapper?.ToPlayerProgress() ?? new PlayerProgress();
                Debug.Log($"[SaveSystem] Loaded player: {currentProgress.playerId}");
            }
            else
            {
                currentProgress = new PlayerProgress();
                Debug.Log("[SaveSystem] Created new player progress.");
                Save();
            }
        }

        /// <summary>
        /// Delete save data.
        /// </summary>
        public void DeleteSave()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log("[SaveSystem] Save file deleted.");
            }
            currentProgress = new PlayerProgress();
        }

        /// <summary>
        /// Record module results and save.
        /// </summary>
        public void SaveModuleResult(string moduleKey, float score,
            System.Collections.Generic.Dictionary<string, float> metrics = null)
        {
            if (currentProgress == null) currentProgress = new PlayerProgress();

            var module = currentProgress.GetOrCreateModule(moduleKey);
            module.RecordAttempt(score, metrics);
            Save();
        }

        /// <summary>
        /// Returns raw JSON string for backend upload.
        /// </summary>
        public string GetProgressJSON()
        {
            if (currentProgress == null) return "{}";
            return JsonUtility.ToJson(new PlayerProgressWrapper(currentProgress), prettyPrint);
        }
    }

    /// <summary>
    /// JsonUtility wrapper since it doesn't support Dictionary directly.
    /// Serializes PlayerProgress into flat arrays.
    /// </summary>
    [System.Serializable]
    public class PlayerProgressWrapper
    {
        public string playerId;
        public float totalScore;
        public string lastPlayed;
        public string[] moduleKeys;
        public ModuleProgressData[] moduleData;

        public PlayerProgressWrapper() { }

        public PlayerProgressWrapper(PlayerProgress progress)
        {
            playerId = progress.playerId;
            totalScore = progress.totalScore;
            lastPlayed = progress.lastPlayed;

            int count = progress.modules.Count;
            moduleKeys = new string[count];
            moduleData = new ModuleProgressData[count];

            int i = 0;
            foreach (var kvp in progress.modules)
            {
                moduleKeys[i] = kvp.Key;
                moduleData[i] = new ModuleProgressData(kvp.Value);
                i++;
            }
        }

        public PlayerProgress ToPlayerProgress()
        {
            var progress = new PlayerProgress(playerId)
            {
                totalScore = totalScore,
                lastPlayed = lastPlayed
            };

            if (moduleKeys != null && moduleData != null)
            {
                for (int i = 0; i < moduleKeys.Length && i < moduleData.Length; i++)
                {
                    progress.modules[moduleKeys[i]] = moduleData[i].ToModuleProgress();
                }
            }

            return progress;
        }
    }

    [System.Serializable]
    public class ModuleProgressData
    {
        public bool completed;
        public float score;
        public int attempts;
        public float bestScore;
        public string[] metricKeys;
        public float[] metricValues;

        public ModuleProgressData() { }

        public ModuleProgressData(ModuleProgress module)
        {
            completed = module.completed;
            score = module.score;
            attempts = module.attempts;
            bestScore = module.bestScore;

            int count = module.metrics.Count;
            metricKeys = new string[count];
            metricValues = new float[count];

            int i = 0;
            foreach (var kvp in module.metrics)
            {
                metricKeys[i] = kvp.Key;
                metricValues[i] = kvp.Value;
                i++;
            }
        }

        public ModuleProgress ToModuleProgress()
        {
            var module = new ModuleProgress
            {
                completed = completed,
                score = score,
                attempts = attempts,
                bestScore = bestScore
            };

            if (metricKeys != null && metricValues != null)
            {
                for (int i = 0; i < metricKeys.Length && i < metricValues.Length; i++)
                {
                    module.metrics[metricKeys[i]] = metricValues[i];
                }
            }

            return module;
        }
    }
}
