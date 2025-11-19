using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Microsoft.MixedReality.Toolkit.Input;

namespace PopBalloons.UI
{
    /// <summary>
    /// Generic touchable wrapper for Unity UI Buttons.
    /// Add this to any Unity Button to make it touchable/pokeable.
    /// Automatically triggers the button's onClick event when touched.
    /// </summary>
    public class TouchableUIButton : MonoBehaviour,
        IMixedRealityPointerHandler,
        IMixedRealityTouchHandler
    {
        [Header("Touch Interaction (Auto-configured)")]
        [SerializeField]
        [Tooltip("Enable touch interaction (poke with finger)")]
        private bool enableTouchInteraction = true;

        [SerializeField]
        [Tooltip("Optional: reference to Button component. Auto-detected if null.")]
        private Button button;

        private BoxCollider buttonCollider;
        private NearInteractionTouchable touchable;
        private bool isProcessingTouch = false;

        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }
            SetupTouchInteraction();
        }

        private void SetupTouchInteraction()
        {
            if (!enableTouchInteraction) return;

            buttonCollider = GetComponent<BoxCollider>();
            if (buttonCollider == null)
            {
                buttonCollider = gameObject.AddComponent<BoxCollider>();
                RectTransform rectTransform = GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    buttonCollider.size = new Vector3(rectTransform.rect.width, rectTransform.rect.height, 0.01f);
                }
                else
                {
                    buttonCollider.size = new Vector3(0.1f, 0.1f, 0.01f);
                }
            }
            buttonCollider.isTrigger = true;

            touchable = GetComponent<NearInteractionTouchable>();
            if (touchable == null)
            {
                touchable = gameObject.AddComponent<NearInteractionTouchable>();
            }
            touchable.SetLocalForward(Vector3.forward);
            touchable.SetBounds(buttonCollider.size);
        }

        // MRTK Touch Handlers
        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            if (isProcessingTouch) return;
            isProcessingTouch = true;
            TriggerButton();
            eventData.Use();
            StartCoroutine(ResetTouchFlag());
        }

        public void OnTouchCompleted(HandTrackingInputEventData eventData) { }
        public void OnTouchUpdated(HandTrackingInputEventData eventData) { }

        // MRTK Pointer Handlers
        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            if (isProcessingTouch) return;
            isProcessingTouch = true;
            TriggerButton();
            eventData.Use();
            StartCoroutine(ResetTouchFlag());
        }

        public void OnPointerUp(MixedRealityPointerEventData eventData) { }
        public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
        public void OnPointerClicked(MixedRealityPointerEventData eventData) { }

        private System.Collections.IEnumerator ResetTouchFlag()
        {
            yield return new WaitForSeconds(0.2f);
            isProcessingTouch = false;
        }

        /// <summary>
        /// Triggers the Unity Button's onClick event
        /// </summary>
        private void TriggerButton()
        {
            if (button != null && button.interactable)
            {
                button.onClick.Invoke();
            }
        }
    }
}
