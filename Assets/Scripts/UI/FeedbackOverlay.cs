using UnityEngine;
using UnityEngine.UI;

namespace VRCarSimulator.UI
{
    /// <summary>
    /// Visual + haptic feedback system for errors and successes.
    /// Shows colored glow overlay and triggers VR haptics.
    /// </summary>
    public class FeedbackOverlay : MonoBehaviour
    {
        [Header("Overlay")]
        [SerializeField] private Image overlayImage;
        [SerializeField] private CanvasGroup overlayGroup;

        [Header("Error Feedback")]
        [SerializeField] private Color errorColor = new Color(1f, 0f, 0f, 0.3f);
        [SerializeField] private float errorFlashDuration = 0.5f;

        [Header("Success Feedback")]
        [SerializeField] private Color successColor = new Color(0f, 1f, 0f, 0.2f);
        [SerializeField] private float successFlashDuration = 0.4f;

        [Header("Warning Feedback")]
        [SerializeField] private Color warningColor = new Color(1f, 0.8f, 0f, 0.25f);
        [SerializeField] private float warningFlashDuration = 0.6f;

        private Coroutine activeFlash;

        private void OnEnable()
        {
            Core.EventBus.OnCollision += OnCollision;
            Core.EventBus.OnTrafficViolation += OnViolation;
        }

        private void OnDisable()
        {
            Core.EventBus.OnCollision -= OnCollision;
            Core.EventBus.OnTrafficViolation -= OnViolation;
        }

        private void OnCollision(Core.CollisionType type, float force)
        {
            ShowError();
        }

        private void OnViolation(string violation)
        {
            ShowWarning();
        }

        public void ShowError()
        {
            Flash(errorColor, errorFlashDuration);
            Core.AudioManager.Instance?.PlayError();
        }

        public void ShowSuccess()
        {
            Flash(successColor, successFlashDuration);
            Core.AudioManager.Instance?.PlaySuccess();
        }

        public void ShowWarning()
        {
            Flash(warningColor, warningFlashDuration);
            Core.AudioManager.Instance?.PlayWarning();
        }

        private void Flash(Color color, float duration)
        {
            if (activeFlash != null) StopCoroutine(activeFlash);
            activeFlash = StartCoroutine(FlashRoutine(color, duration));
        }

        private System.Collections.IEnumerator FlashRoutine(Color color, float duration)
        {
            if (overlayImage != null) overlayImage.color = color;
            if (overlayGroup != null) overlayGroup.alpha = 1f;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                if (overlayGroup != null)
                    overlayGroup.alpha = 1f - t;
                yield return null;
            }

            if (overlayGroup != null) overlayGroup.alpha = 0f;
            activeFlash = null;
        }
    }
}
