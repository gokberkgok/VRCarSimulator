using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRCarSimulator.VR
{
    /// <summary>
    /// VR hand visual controller – shows hand models for left/right controllers.
    /// Animates grip and trigger based on controller input.
    /// Attach to each hand's controller model parent.
    /// </summary>
    public class VRHandController : MonoBehaviour
    {
        [Header("Hand Side")]
        [SerializeField] private HandSide handSide = HandSide.Left;

        [Header("Animator")]
        [SerializeField] private Animator handAnimator;

        [Header("Input References")]
        [SerializeField] private UnityEngine.InputSystem.InputActionReference triggerAction;
        [SerializeField] private UnityEngine.InputSystem.InputActionReference gripAction;

        // Animator parameter hashes
        private static readonly int TriggerParam = Animator.StringToHash("Trigger");
        private static readonly int GripParam = Animator.StringToHash("Grip");

        private float triggerValue;
        private float gripValue;

        public enum HandSide { Left, Right }

        private void OnEnable()
        {
            if (triggerAction != null && triggerAction.action != null)
                triggerAction.action.Enable();
            if (gripAction != null && gripAction.action != null)
                gripAction.action.Enable();
        }

        private void Update()
        {
            ReadInputs();
            AnimateHand();
        }

        private void ReadInputs()
        {
            if (triggerAction != null && triggerAction.action != null)
                triggerValue = triggerAction.action.ReadValue<float>();

            if (gripAction != null && gripAction.action != null)
                gripValue = gripAction.action.ReadValue<float>();
        }

        private void AnimateHand()
        {
            if (handAnimator == null) return;

            handAnimator.SetFloat(TriggerParam, triggerValue);
            handAnimator.SetFloat(GripParam, gripValue);
        }
    }
}
