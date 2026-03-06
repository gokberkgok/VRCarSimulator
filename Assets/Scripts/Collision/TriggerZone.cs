using UnityEngine;

namespace VRCarSimulator.Collision
{
    /// <summary>
    /// Trigger zone for traffic lights, speed zones, checkpoints, and park areas.
    /// Fires events when the player vehicle enters/exits.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TriggerZone : MonoBehaviour
    {
        [Header("Zone Settings")]
        [SerializeField] private ZoneType zoneType = ZoneType.Checkpoint;
        [SerializeField] private string zoneId;

        [Header("Speed Zone")]
        [SerializeField] private float speedLimit = 50f;

        [Header("Detection")]
        [SerializeField] private string playerTag = "Player";

        public event System.Action<TriggerZone> OnPlayerEnter;
        public event System.Action<TriggerZone> OnPlayerExit;

        public ZoneType Type => zoneType;
        public string ZoneId => zoneId;
        public float SpeedLimit => speedLimit;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;

            OnPlayerEnter?.Invoke(this);

            switch (zoneType)
            {
                case ZoneType.Checkpoint:
                    Core.EventBus.TriggerCheckpointReached(zoneId);
                    break;
                case ZoneType.SpeedLimit:
                    // Module can read SpeedLimit property
                    break;
                case ZoneType.ParkZone:
                    Core.EventBus.TriggerCheckpointReached($"park_{zoneId}");
                    break;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            OnPlayerExit?.Invoke(this);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Color c = zoneType switch
            {
                ZoneType.Checkpoint => Color.blue,
                ZoneType.SpeedLimit => Color.yellow,
                ZoneType.StopLine => Color.red,
                ZoneType.ParkZone => Color.green,
                _ => Color.white
            };
            c.a = 0.25f;
            Gizmos.color = c;

            var col = GetComponent<BoxCollider>();
            if (col != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(col.center, col.size);
                Gizmos.DrawWireCube(col.center, col.size);
            }
        }
#endif
    }

    public enum ZoneType
    {
        Checkpoint,
        SpeedLimit,
        StopLine,
        ParkZone,
        TrafficLight,
        HazardZone
    }
}
