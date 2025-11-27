using UnityEngine;
using UnityEngine.Events;
using Microsoft.MixedReality.Toolkit.Input;

namespace PopBalloons.UI
{
    /// <summary>
    /// Button that responds to both near interaction (poke/touch) and far interaction (pinch/click).
    /// Add this component to any GameObject with a Collider to make it touchable.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TouchableButton : MonoBehaviour, IMixedRealityPointerHandler, IMixedRealityTouchHandler
    {
        [Header("Events")]
        [Tooltip("Event triggered when the button is pressed (touch or click)")]
        public UnityEvent OnPressed;

        [Header("Visual Feedback")]
        [SerializeField]
        [Tooltip("Optional object to scale when pressed")]
        private Transform visualFeedback;

        [SerializeField]
        [Range(0.8f, 1.0f)]
        [Tooltip("Scale multiplier when button is pressed")]
        private float pressedScale = 0.95f;

        private Vector3 originalScale;
        private bool isPressed = false;

        private void Start()
        {
            if (visualFeedback != null)
            {
                originalScale = visualFeedback.localScale;
            }

            // Ensure collider is set as trigger for MRTK interaction
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        #region IMixedRealityPointerHandler - Handles far interaction (pinch, click)

        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            PressButton();
        }

        public void OnPointerUp(MixedRealityPointerEventData eventData)
        {
            ReleaseButton();
        }

        public void OnPointerClicked(MixedRealityPointerEventData eventData)
        {
            // Button press is triggered on pointer down, this is just for compatibility
        }

        public void OnPointerDragged(MixedRealityPointerEventData eventData) { }

        #endregion

        #region IMixedRealityTouchHandler - Handles near interaction (poke, touch)

        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            PressButton();
        }

        public void OnTouchCompleted(HandTrackingInputEventData eventData)
        {
            ReleaseButton();
        }

        public void OnTouchUpdated(HandTrackingInputEventData eventData) { }

        #endregion

        private void PressButton()
        {
            if (isPressed) return;

            isPressed = true;

            // Visual feedback
            if (visualFeedback != null)
            {
                visualFeedback.localScale = originalScale * pressedScale;
            }

            // Trigger event
            OnPressed?.Invoke();

            Debug.Log($"Button '{gameObject.name}' pressed!");
        }

        private void ReleaseButton()
        {
            if (!isPressed) return;

            isPressed = false;

            // Reset visual feedback
            if (visualFeedback != null)
            {
                visualFeedback.localScale = originalScale;
            }
        }
    }
}
