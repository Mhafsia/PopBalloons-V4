using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;

namespace PopBalloons.Utilities
{
    /// <summary>
    /// Class that manage level loading from menu with touch/pointer support
    /// </summary>
    public class LoadLevelButton : MonoBehaviour, 
        IMixedRealityPointerHandler, 
        IMixedRealityTouchHandler
    {
        [SerializeField]
        private GameManager.GameType type = GameManager.GameType.COGNITIVE;

        [SerializeField]
        private int levelNumber = 1;

        [Header("Touch Interaction (Auto-configured)")]
        [SerializeField]
        [Tooltip("Enable touch interaction (poke with finger)")]
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
            Load();
            eventData.Use();
        }

        public void OnTouchCompleted(HandTrackingInputEventData eventData) { }
        public void OnTouchUpdated(HandTrackingInputEventData eventData) { }

        // MRTK Pointer Handlers
        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            Load();
            eventData.Use();
        }

        public void OnPointerUp(MixedRealityPointerEventData eventData) { }
        public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
        public void OnPointerClicked(MixedRealityPointerEventData eventData) { }

        public void Load()
        {
            if (type == GameManager.GameType.NONE || levelNumber < 0)
            {
                // Stop any running game (especially FreePlay continuous spawning)
                if (GameCreator.Instance != null)
                {
                    GameCreator.Instance.QuitLevel();
                }
                
                GameManager.Instance.Home();
            }
            else
            {
                GameManager.Instance.NewGame(type, levelNumber);
            }
                
        }
    }

}