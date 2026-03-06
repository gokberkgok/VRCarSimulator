using UnityEngine;

namespace VRCarSimulator.Analytics
{
    /// <summary>
    /// Measures reaction times for specific events (brake response, hazard detection, etc.).
    /// </summary>
    public class ReactionTimeMeasurer : MonoBehaviour
    {
        private float eventTriggerTime;
        private bool measuring;
        private string currentEventId;

        public bool IsMeasuring => measuring;
        public float ElapsedTime => measuring ? Time.time - eventTriggerTime : 0f;

        public event System.Action<string, float> OnReactionMeasured; // eventId, reactionTimeSeconds

        /// <summary>
        /// Start measuring reaction time for an event.
        /// </summary>
        public void StartMeasurement(string eventId)
        {
            currentEventId = eventId;
            eventTriggerTime = Time.time;
            measuring = true;
        }

        /// <summary>
        /// Stop measurement and record the reaction time.
        /// </summary>
        public float StopMeasurement()
        {
            if (!measuring) return -1f;

            float reactionTime = Time.time - eventTriggerTime;
            measuring = false;

            OnReactionMeasured?.Invoke(currentEventId, reactionTime);
            Core.EventBus.TriggerMetricRecorded($"reactionTime_{currentEventId}", reactionTime);

            if (AnalyticsTracker.Instance != null)
            {
                AnalyticsTracker.Instance.RecordMetric($"reactionTime_{currentEventId}", reactionTime);
                AnalyticsTracker.Instance.RecordTimeSample("reactionTimes", reactionTime);
            }

            return reactionTime;
        }

        /// <summary>
        /// Cancel without recording.
        /// </summary>
        public void Cancel()
        {
            measuring = false;
            currentEventId = null;
        }
    }
}
