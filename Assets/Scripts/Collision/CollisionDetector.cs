using UnityEngine;
using VRCarSimulator.Core;

namespace VRCarSimulator.Collision
{
    /// <summary>
    /// Attach to the vehicle. Detects collisions with tagged objects
    /// and broadcasts via EventBus with type classification and force.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CollisionDetector : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float minImpactForce = 2f; // ignore tiny bumps

        [Header("Tag Mapping")]
        [SerializeField] private string pedestrianTag = "Pedestrian";
        [SerializeField] private string vehicleTag = "Vehicle";
        [SerializeField] private string trafficConeTag = "TrafficCone";
        [SerializeField] private string curbTag = "Curb";
        [SerializeField] private string barrierTag = "Barrier";

        private float lastCollisionTime;
        private const float COLLISION_COOLDOWN = 0.5f; // prevent spam

        private void OnCollisionEnter(UnityEngine.Collision collision)
        {
            if (Time.time - lastCollisionTime < COLLISION_COOLDOWN) return;

            float force = collision.impulse.magnitude / Time.fixedDeltaTime;
            if (force < minImpactForce) return;

            lastCollisionTime = Time.time;

            CollisionType type = ClassifyCollision(collision.gameObject);
            EventBus.TriggerCollision(type, force);

            Debug.Log($"[Collision] Type={type} Force={force:F1} Object={collision.gameObject.name}");
        }

        private CollisionType ClassifyCollision(GameObject obj)
        {
            if (obj.CompareTag(pedestrianTag)) return CollisionType.Pedestrian;
            if (obj.CompareTag(vehicleTag)) return CollisionType.Vehicle;
            if (obj.CompareTag(trafficConeTag)) return CollisionType.TrafficCone;
            if (obj.CompareTag(curbTag)) return CollisionType.Curb;
            if (obj.CompareTag(barrierTag)) return CollisionType.Barrier;
            return CollisionType.Other;
        }
    }
}
