using UnityEngine;

namespace VRCarSimulator.VR
{
    /// <summary>
    /// Otomatik XR Rig pozisyonlama – aracın kokpitine yerleştirir.
    /// XR Origin'in Tracking Origin Mode'u "Floor" olarak ayarlanmalıdır.
    /// </summary>
    public class VRRigAnchor : MonoBehaviour
    {
        [Header("Anchor")]
        [SerializeField] private Transform cockpitSeatPosition;

        [Header("Settings")]
        [SerializeField] private bool followVehicle = true;
        [SerializeField] private float heightOffset = 0f;

        private void LateUpdate()
        {
            if (!followVehicle || cockpitSeatPosition == null) return;

            // XR Origin'i araç koltuğuna kilitle
            Vector3 targetPos = cockpitSeatPosition.position + Vector3.up * heightOffset;
            transform.position = targetPos;
            transform.rotation = cockpitSeatPosition.rotation;
        }

        /// <summary>
        /// Oyuncu oturduğunda çağır – aracın koltuğuna snap eder.
        /// </summary>
        public void SnapToSeat()
        {
            if (cockpitSeatPosition == null) return;
            transform.position = cockpitSeatPosition.position + Vector3.up * heightOffset;
            transform.rotation = cockpitSeatPosition.rotation;
        }

        public void SetCockpitAnchor(Transform anchor)
        {
            cockpitSeatPosition = anchor;
        }
    }
}
