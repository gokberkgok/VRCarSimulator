using UnityEngine;
using UnityEngine.AI;

namespace VRCarSimulator.AI
{
    /// <summary>
    /// Simple NPC traffic vehicle that follows NavMesh or waypoints.
    /// Difficulty-aware: speed, spawn frequency, and behavior change with AdaptiveDifficultySystem.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class TrafficAI : MonoBehaviour
    {
        [Header("Waypoints")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private bool loop = true;

        [Header("Movement")]
        [SerializeField] private float baseSpeed = 30f;  // km/h
        [SerializeField] private float acceleration = 5f;
        [SerializeField] private float brakingDistance = 15f;
        [SerializeField] private float stoppingDistance = 2f;
        [SerializeField] private float rotationSpeed = 3f;

        [Header("Awareness")]
        [SerializeField] private float detectionRange = 20f;
        [SerializeField] private LayerMask obstacleLayer;

        private int currentWaypointIndex;
        private Rigidbody rb;
        private float currentSpeed;
        private float difficultyMultiplier = 1f;

        public float SpeedKmh => currentSpeed * 3.6f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        private void Update()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            Transform target = waypoints[currentWaypointIndex];
            if (target == null) return;

            float distance = Vector3.Distance(transform.position, target.position);

            // Braking logic
            float targetSpeed = baseSpeed * difficultyMultiplier / 3.6f;
            if (distance < brakingDistance)
                targetSpeed *= Mathf.InverseLerp(stoppingDistance, brakingDistance, distance);

            // Obstacle detection
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward,
                out RaycastHit hit, detectionRange, obstacleLayer))
            {
                float obstacleDistance = hit.distance;
                if (obstacleDistance < brakingDistance)
                    targetSpeed *= Mathf.InverseLerp(2f, brakingDistance, obstacleDistance);
            }

            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

            // Move
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * currentSpeed * Time.deltaTime;

            // Rotate
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Advance waypoint
            if (distance < stoppingDistance)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Length)
                {
                    if (loop) currentWaypointIndex = 0;
                    else enabled = false;
                }
            }
        }

        public void SetDifficultyMultiplier(float multiplier)
        {
            difficultyMultiplier = multiplier;
        }

        public void SetWaypoints(Transform[] newWaypoints)
        {
            waypoints = newWaypoints;
            currentWaypointIndex = 0;
        }
    }
}
