using UnityEngine;
using System.Collections.Generic;

namespace VRCarSimulator.AI
{
    /// <summary>
    /// Computes and displays the ideal driving route using waypoints.
    /// Shows a ghost overlay to guide the player during parking and maneuvers.
    /// </summary>
    public class IdealRouteCalculator : MonoBehaviour
    {
        [Header("Waypoints")]
        [SerializeField] private Transform[] routeWaypoints;

        [Header("Ghost Overlay")]
        [SerializeField] private LineRenderer routeLineRenderer;
        [SerializeField] private Color routeColor = new Color(0f, 1f, 0.5f, 0.4f);
        [SerializeField] private float lineWidth = 0.15f;
        [SerializeField] private bool showGhostRoute = true;

        [Header("Evaluation")]
        [SerializeField] private float maxDeviationMeters = 3f; // max acceptable deviation

        private List<Vector3> idealPath = new List<Vector3>();
        private float totalDeviation;
        private int deviationSamples;

        public float AverageDeviation => deviationSamples > 0 ? totalDeviation / deviationSamples : 0f;
        public float DeviationScore => 1f - Mathf.Clamp01(AverageDeviation / maxDeviationMeters);

        private void Start()
        {
            BuildIdealPath();
            if (showGhostRoute) RenderRoute();
        }

        /// <summary>
        /// Sample the player's current position to calculate deviation from ideal route.
        /// Call this periodically (e.g., every 0.5s).
        /// </summary>
        public void SampleDeviation(Vector3 playerPosition)
        {
            float minDist = float.MaxValue;
            foreach (var point in idealPath)
            {
                float dist = Vector3.Distance(new Vector3(playerPosition.x, 0f, playerPosition.z),
                    new Vector3(point.x, 0f, point.z));
                if (dist < minDist) minDist = dist;
            }

            totalDeviation += minDist;
            deviationSamples++;
        }

        /// <summary>
        /// Returns a score 0-100 for how closely the player followed the ideal route.
        /// </summary>
        public float GetRouteAccuracy()
        {
            return DeviationScore * 100f;
        }

        private void BuildIdealPath()
        {
            idealPath.Clear();
            if (routeWaypoints == null) return;

            foreach (var wp in routeWaypoints)
            {
                if (wp != null) idealPath.Add(wp.position);
            }
        }

        private void RenderRoute()
        {
            if (routeLineRenderer == null || idealPath.Count == 0) return;

            routeLineRenderer.positionCount = idealPath.Count;
            routeLineRenderer.SetPositions(idealPath.ToArray());
            routeLineRenderer.startWidth = lineWidth;
            routeLineRenderer.endWidth = lineWidth;
            routeLineRenderer.startColor = routeColor;
            routeLineRenderer.endColor = routeColor;
        }

        public void SetShowGhost(bool show)
        {
            showGhostRoute = show;
            if (routeLineRenderer != null)
                routeLineRenderer.enabled = show;
        }

        public void ResetDeviation()
        {
            totalDeviation = 0f;
            deviationSamples = 0;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (routeWaypoints == null) return;
            Gizmos.color = Color.green;
            for (int i = 0; i < routeWaypoints.Length - 1; i++)
            {
                if (routeWaypoints[i] != null && routeWaypoints[i + 1] != null)
                    Gizmos.DrawLine(routeWaypoints[i].position, routeWaypoints[i + 1].position);
            }
        }
#endif
    }
}
