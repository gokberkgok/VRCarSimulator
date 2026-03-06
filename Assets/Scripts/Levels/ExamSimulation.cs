using UnityEngine;
using System.Collections.Generic;
using VRCarSimulator.Analytics;

namespace VRCarSimulator.Levels
{
    /// <summary>
    /// Full exam simulation – runs all modules sequentially with strict scoring.
    /// Mimics the real driving license exam format.
    /// </summary>
    public class ExamSimulation : MonoBehaviour
    {
        [Header("Modules (in exam order)")]
        [SerializeField] private ModuleBase[] examModules;

        [Header("Exam Settings")]
        [SerializeField] private float passingScore = 70f;
        [SerializeField] private float totalTimeLimit = 1800f; // 30 min
        [SerializeField] private int maxCriticalErrors = 3;

        private int currentModuleIndex;
        private float examStartTime;
        private float[] moduleScores;
        private int criticalErrors;
        private bool examActive;

        public bool ExamActive => examActive;
        public float ExamTime => Time.time - examStartTime;
        public float TotalScore => CalculateTotalScore();
        public int CriticalErrors => criticalErrors;

        public event System.Action<float, bool> OnExamComplete; // totalScore, passed

        private void OnEnable()
        {
            Core.EventBus.OnCollision += OnCriticalCollision;
        }

        private void OnDisable()
        {
            Core.EventBus.OnCollision -= OnCriticalCollision;
        }

        public void StartExam()
        {
            if (examModules == null || examModules.Length == 0)
            {
                Debug.LogError("[ExamSimulation] No modules assigned.");
                return;
            }

            examActive = true;
            examStartTime = Time.time;
            currentModuleIndex = 0;
            moduleScores = new float[examModules.Length];
            criticalErrors = 0;

            StartCurrentModule();
        }

        private void Update()
        {
            if (!examActive) return;

            // Global time check
            if (ExamTime >= totalTimeLimit)
            {
                FinishExam();
            }

            // Critical error limit
            if (criticalErrors >= maxCriticalErrors)
            {
                FinishExam();
            }
        }

        private void StartCurrentModule()
        {
            if (currentModuleIndex >= examModules.Length)
            {
                FinishExam();
                return;
            }

            var module = examModules[currentModuleIndex];
            module.OnModuleComplete += OnModuleFinished;
            module.StartModule();
        }

        private void OnModuleFinished(float score)
        {
            var module = examModules[currentModuleIndex];
            module.OnModuleComplete -= OnModuleFinished;

            moduleScores[currentModuleIndex] = score;
            currentModuleIndex++;
            StartCurrentModule();
        }

        private void FinishExam()
        {
            examActive = false;
            float total = TotalScore;
            bool passed = total >= passingScore && criticalErrors < maxCriticalErrors;

            // Save exam result
            var metrics = new Dictionary<string, float>
            {
                ["examDuration"] = ExamTime,
                ["criticalErrors"] = criticalErrors,
                ["moduleCount"] = examModules.Length
            };

            for (int i = 0; i < moduleScores.Length; i++)
                metrics[$"module{i + 1}_score"] = moduleScores[i];

            Data.SaveSystem.Instance?.SaveModuleResult("exam", total, metrics);

            OnExamComplete?.Invoke(total, passed);
        }

        private float CalculateTotalScore()
        {
            if (moduleScores == null || moduleScores.Length == 0) return 0f;

            float sum = 0f;
            int count = 0;
            foreach (float s in moduleScores)
            {
                if (s > 0f) { sum += s; count++; }
            }
            return count > 0 ? sum / count : 0f;
        }

        private void OnCriticalCollision(Core.CollisionType type, float force)
        {
            if (!examActive) return;

            // Pedestrian collision or high-force impact = critical
            if (type == Core.CollisionType.Pedestrian || force > 30f)
            {
                criticalErrors++;
            }
        }
    }
}
