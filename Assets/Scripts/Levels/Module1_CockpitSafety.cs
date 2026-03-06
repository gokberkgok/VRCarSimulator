using UnityEngine;
using VRCarSimulator.VR;
using VRCarSimulator.Analytics;

namespace VRCarSimulator.Levels
{
    /// <summary>
    /// Module 1: Cockpit & Safety – seat adjustment, mirror check, seatbelt, dashboard intro.
    /// Evaluates gaze tracking (mirror), correct sequence, blind spot checks.
    /// </summary>
    public class Module1_CockpitSafety : ModuleBase
    {
        [Header("Module 1 References")]
        [SerializeField] private GazeTracker gazeTracker;
        [SerializeField] private Transform seatAdjustHandle;

        [Header("Gaze Zones")]
        [SerializeField] private string leftMirrorZone = "LeftMirror";
        [SerializeField] private string rearMirrorZone = "RearMirror";
        [SerializeField] private string rightMirrorZone = "RightMirror";
        [SerializeField] private string blindSpotLeftZone = "BlindSpotLeft";
        [SerializeField] private string blindSpotRightZone = "BlindSpotRight";

        [Header("Required Gaze Durations (seconds)")]
        [SerializeField] private float mirrorGazeRequired = 1.5f;
        [SerializeField] private float blindSpotGazeRequired = 0.8f;

        [Header("Checklist")]
        [SerializeField] private bool requireSeatAdjustment = true;
        [SerializeField] private bool requireSeatbelt = true;

        // State tracking
        private bool seatAdjusted;
        private bool seatbeltFastened;
        private bool leftMirrorChecked;
        private bool rearMirrorChecked;
        private bool rightMirrorChecked;
        private bool blindSpotLeftChecked;
        private bool blindSpotRightChecked;
        private float seatAdjustStartTime;
        private int correctSequenceCount;
        private int expectedStepIndex;

        // Expected order: Seat → Mirrors → Seatbelt
        private readonly string[] expectedSequence = { "Seat", "LeftMirror", "RearMirror", "RightMirror", "Seatbelt" };

        protected override void OnModuleStart()
        {
            seatAdjusted = false;
            seatbeltFastened = false;
            leftMirrorChecked = false;
            rearMirrorChecked = false;
            rightMirrorChecked = false;
            blindSpotLeftChecked = false;
            blindSpotRightChecked = false;
            correctSequenceCount = 0;
            expectedStepIndex = 0;
            seatAdjustStartTime = Time.time;

            if (gazeTracker != null)
            {
                gazeTracker.OnGazeDwell += OnGazeDwell;
            }
        }

        protected override void OnModuleUpdate()
        {
            CheckCompletion();
        }

        protected override void OnModuleEnd()
        {
            if (gazeTracker != null)
            {
                gazeTracker.OnGazeDwell -= OnGazeDwell;
            }

            // Record final metrics
            var tracker = AnalyticsTracker.Instance;
            if (tracker != null)
            {
                tracker.RecordMetric("seatAdjustmentTime", seatAdjusted ? Time.time - seatAdjustStartTime : -1f);
                tracker.RecordMetric("mirrorCheckAccuracy", CalculateMirrorAccuracy());
                tracker.RecordMetric("sequenceAccuracy", (float)correctSequenceCount / expectedSequence.Length * 100f);
                tracker.RecordMetric("blindSpotChecked", (blindSpotLeftChecked && blindSpotRightChecked) ? 1f : 0f);
            }

            if (scoringSystem != null)
            {
                scoringSystem.RecordMetric("mirrorCheckAccuracy", CalculateMirrorAccuracy());
                scoringSystem.RecordMetric("sequenceAccuracy", (float)correctSequenceCount / expectedSequence.Length * 100f);
            }
        }

        private void OnGazeDwell(string zoneName, float dwellTime)
        {
            if (zoneName == leftMirrorZone && dwellTime >= mirrorGazeRequired)
            {
                leftMirrorChecked = true;
                TryAdvanceSequence("LeftMirror");
            }
            else if (zoneName == rearMirrorZone && dwellTime >= mirrorGazeRequired)
            {
                rearMirrorChecked = true;
                TryAdvanceSequence("RearMirror");
            }
            else if (zoneName == rightMirrorZone && dwellTime >= mirrorGazeRequired)
            {
                rightMirrorChecked = true;
                TryAdvanceSequence("RightMirror");
            }
            else if (zoneName == blindSpotLeftZone && dwellTime >= blindSpotGazeRequired)
            {
                blindSpotLeftChecked = true;
            }
            else if (zoneName == blindSpotRightZone && dwellTime >= blindSpotGazeRequired)
            {
                blindSpotRightChecked = true;
            }
        }

        /// <summary>Called by VR interaction when seat is adjusted.</summary>
        public void OnSeatAdjusted()
        {
            seatAdjusted = true;
            TryAdvanceSequence("Seat");
        }

        /// <summary>Called by VR interaction when seatbelt is fastened.</summary>
        public void OnSeatbeltFastened()
        {
            seatbeltFastened = true;
            TryAdvanceSequence("Seatbelt");
        }

        private void TryAdvanceSequence(string step)
        {
            if (expectedStepIndex < expectedSequence.Length &&
                expectedSequence[expectedStepIndex] == step)
            {
                correctSequenceCount++;
                expectedStepIndex++;
            }
        }

        private float CalculateMirrorAccuracy()
        {
            int checked_ = 0;
            if (leftMirrorChecked) checked_++;
            if (rearMirrorChecked) checked_++;
            if (rightMirrorChecked) checked_++;
            return (checked_ / 3f) * 100f;
        }

        private void CheckCompletion()
        {
            bool allDone = true;
            if (requireSeatAdjustment && !seatAdjusted) allDone = false;
            if (requireSeatbelt && !seatbeltFastened) allDone = false;
            if (!leftMirrorChecked || !rearMirrorChecked || !rightMirrorChecked) allDone = false;

            if (allDone)
            {
                CompleteModule();
            }
        }
    }
}
