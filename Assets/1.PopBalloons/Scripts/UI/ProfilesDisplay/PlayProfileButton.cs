using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;

namespace PopBalloons.UI
{
    /// <summary>
    /// Button to play/confirm selected profile with touch support.
    /// Attach to the "Play" or "Validate" button in profile selection.
    /// </summary>
    public class PlayProfileButton : MonoBehaviour,
        IMixedRealityPointerHandler,
        IMixedRealityTouchHandler
    {
        [SerializeField]
        private ProfileList profileList;

        [SerializeField]
        private bool enableTouchInteraction = true;

        private BoxCollider buttonCollider;
        private NearInteractionTouchable touchable;
        private bool isProcessingTouch = false;

        private void Awake()
        {
            if (profileList == null)
            {
                profileList = FindObjectOfType<ProfileList>();
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
            PlayProfile();
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
            PlayProfile();
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
        /// Called by Unity Button OnClick OR touch/pointer events
        /// </summary>
        public void PlayProfile()
        {
            if (profileList != null)
            {
                profileList.OnClick();
            }
        }
    }
}
