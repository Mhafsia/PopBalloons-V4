using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PopBalloons.Utilities;
using Microsoft.MixedReality.Toolkit.Input;

namespace PopBalloons.UI
{
    /// <summary>
    /// Button component to quit FreePlay mode and return to the main menu with touch support.
    /// </summary>
    public class QuitFreePlayButton : MonoBehaviour,
        IMixedRealityPointerHandler,
        IMixedRealityTouchHandler
    {
        [SerializeField]
        private bool enableTouchInteraction = true;

        private BoxCollider buttonCollider;
        private NearInteractionTouchable touchable;

        private void Awake()
        {
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

        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            QuitFreePlay();
            eventData.Use();
        }

        public void OnTouchCompleted(HandTrackingInputEventData eventData) { }
        public void OnTouchUpdated(HandTrackingInputEventData eventData) { }

        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            QuitFreePlay();
            eventData.Use();
        }

        public void OnPointerUp(MixedRealityPointerEventData eventData) { }
        public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
        public void OnPointerClicked(MixedRealityPointerEventData eventData) { }

        public void QuitFreePlay()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentGameType == GameManager.GameType.FREEPLAY)
            {
                Debug.Log("Quitting FreePlay mode...");

                // IMPORTANT: Stop spawning FIRST before changing state
                if (GameCreator.Instance != null)
                {
                    GameCreator.Instance.QuitLevel();
                }

                // THEN return to home (this changes GameState which might restart coroutines)
                GameManager.Instance.Home();
            }
            else
            {
                Debug.LogWarning("QuitFreePlay called but not in FreePlay mode.");
            }
        }
    }
}
