using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;

namespace PopBalloons.UI
{

    /// <summary>
    /// Handle navigation inside MainPanel with automatic touch/pointer interaction support
    /// </summary>
    public abstract class SubPanelButton<T> : MonoBehaviour, 
        IMixedRealityPointerHandler, 
        IMixedRealityTouchHandler
    {
        [SerializeField]
        protected T destination;

        [Header("Touch Interaction (Auto-configured)")]
        [SerializeField]
        [Tooltip("Enable touch interaction (poke with finger)")]
        private bool enableTouchInteraction = true;

        private BoxCollider buttonCollider;
        private NearInteractionTouchable touchable;

        /// <summary>
        /// Would be interesting to make an abstract link from T to link panel and button automatically
        /// </summary>
        public abstract void OnClick();

        protected virtual void Awake()
        {
            SetupTouchInteraction();
        }

        private void SetupTouchInteraction()
        {
            if (!enableTouchInteraction) return;

            // Auto-create BoxCollider if missing
            buttonCollider = GetComponent<BoxCollider>();
            if (buttonCollider == null)
            {
                buttonCollider = gameObject.AddComponent<BoxCollider>();
                
                // Auto-size based on RectTransform or Renderer
                RectTransform rectTransform = GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    buttonCollider.size = new Vector3(
                        rectTransform.rect.width,
                        rectTransform.rect.height,
                        0.01f
                    );
                }
                else
                {
                    Renderer renderer = GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        buttonCollider.size = renderer.bounds.size;
                    }
                    else
                    {
                        buttonCollider.size = new Vector3(0.1f, 0.1f, 0.01f);
                    }
                }
            }

            // Must be trigger for MRTK
            buttonCollider.isTrigger = true;

            // Auto-add NearInteractionTouchable if missing
            touchable = GetComponent<NearInteractionTouchable>();
            if (touchable == null)
            {
                touchable = gameObject.AddComponent<NearInteractionTouchable>();
            }

            // Link touchable to collider
            touchable.SetLocalForward(Vector3.forward);
            touchable.SetBounds(buttonCollider.size);
        }

        // ===== MRTK Touch Handlers (Near Interaction) =====
        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            OnClick();
            eventData.Use();
        }

        public void OnTouchCompleted(HandTrackingInputEventData eventData) { }
        public void OnTouchUpdated(HandTrackingInputEventData eventData) { }

        // ===== MRTK Pointer Handlers (Far Interaction - Ray/Pinch) =====
        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            OnClick();
            eventData.Use();
        }

        public void OnPointerUp(MixedRealityPointerEventData eventData) { }
        public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
        public void OnPointerClicked(MixedRealityPointerEventData eventData) { }
    }

}