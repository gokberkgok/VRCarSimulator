using UnityEngine;

namespace VRCarSimulator.Core
{
    /// <summary>
    /// Lightweight event bus for decoupled communication between systems.
    /// </summary>
    public static class EventBus
    {
        // Vehicle Events
        public static event System.Action<float> OnSpeedChanged;
        public static event System.Action<int> OnGearChanged;
        public static event System.Action<bool> OnHandbrakeToggled;
        public static event System.Action<float, float> OnSignalActivated; // left, right

        // Collision Events
        public static event System.Action<CollisionType, float> OnCollision;
        public static event System.Action<string> OnTrafficViolation;

        // Module Events
        public static event System.Action<int, float> OnModuleScoreUpdated;
        public static event System.Action<string> OnCheckpointReached;

        // Analytics Events
        public static event System.Action<string, float> OnMetricRecorded;

        // Trigger methods
        public static void TriggerSpeedChanged(float speed) => OnSpeedChanged?.Invoke(speed);
        public static void TriggerGearChanged(int gear) => OnGearChanged?.Invoke(gear);
        public static void TriggerHandbrakeToggled(bool active) => OnHandbrakeToggled?.Invoke(active);
        public static void TriggerSignalActivated(float left, float right) => OnSignalActivated?.Invoke(left, right);
        public static void TriggerCollision(CollisionType type, float force) => OnCollision?.Invoke(type, force);
        public static void TriggerTrafficViolation(string violation) => OnTrafficViolation?.Invoke(violation);
        public static void TriggerModuleScoreUpdated(int module, float score) => OnModuleScoreUpdated?.Invoke(module, score);
        public static void TriggerCheckpointReached(string checkpointId) => OnCheckpointReached?.Invoke(checkpointId);
        public static void TriggerMetricRecorded(string key, float value) => OnMetricRecorded?.Invoke(key, value);

        /// <summary>
        /// Clears all listeners – call on scene unload to prevent leaks.
        /// </summary>
        public static void ClearAll()
        {
            OnSpeedChanged = null;
            OnGearChanged = null;
            OnHandbrakeToggled = null;
            OnSignalActivated = null;
            OnCollision = null;
            OnTrafficViolation = null;
            OnModuleScoreUpdated = null;
            OnCheckpointReached = null;
            OnMetricRecorded = null;
        }
    }

    public enum CollisionType
    {
        TrafficCone,
        Pedestrian,
        Vehicle,
        Curb,
        Barrier,
        Other
    }
}
