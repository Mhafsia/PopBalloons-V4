using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;

namespace PopBalloons.Utilities
{
    /// <summary>
    /// Class that manage level loading from menu with touch/pointer support
    /// Reads global values from GameModeSelector instead of local properties
    /// </summary>
    public class LoadLevelButton : MonoBehaviour, 
        IMixedRealityPointerHandler, 
        IMixedRealityTouchHandler
    {
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

        private void Start()
        {

        }

        private void Update()
        {
            if (buttonCollider != null)
            {
                bool visible = IsButtonVisible();
                if (buttonCollider.enabled != visible)
                {
                    buttonCollider.enabled = visible;
                }
            }
        }

        private bool IsButtonVisible()
        {
            if (!gameObject.activeInHierarchy) return false;

            // Check local CanvasGroup
            CanvasGroup localCG = GetComponent<CanvasGroup>();
            if (localCG != null)
            {
                if (localCG.alpha < 0.01f || !localCG.interactable)
                    return false;
            }

            // Check all parent CanvasGroups
            Transform current = transform.parent;
            while (current != null)
            {
                CanvasGroup parentCG = current.GetComponent<CanvasGroup>();
                if (parentCG != null)
                {
                    if (parentCG.alpha < 0.01f || !parentCG.interactable)
                        return false;
                }
                current = current.parent;
            }

            return true;
        }

        private void SetupTouchInteraction()
        {
            if (!enableTouchInteraction)
            {
                return;
            }

            // Setup BoxCollider
            buttonCollider = GetComponent<BoxCollider>();
            if (buttonCollider == null)
            {
                buttonCollider = gameObject.AddComponent<BoxCollider>();
            }
            
            // Always reconfigure size to ensure proper depth
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Important: depth must be > 0 for touch to work (0.1f is enough)
                float depth = 0.1f; // Reduced from 10f to avoid hitting through panels
                Vector3 newSize = new Vector3(rectTransform.rect.width, rectTransform.rect.height, depth);
                buttonCollider.size = newSize;
                buttonCollider.center = new Vector3(0, 0, depth / 2f); // Center the collider
            }
            else
            {
                Renderer renderer = GetComponent<Renderer>();
                if (renderer != null)
                {
                    Vector3 size = renderer.bounds.size;
                    if (size.z < 0.01f) size.z = 10f; // Ensure minimum depth
                    buttonCollider.size = size;
                }
                else
                {
                    buttonCollider.size = new Vector3(0.1f, 0.1f, 10f);
                }
            }
            
            buttonCollider.isTrigger = true;

            // Setup NearInteractionTouchable
            touchable = GetComponent<NearInteractionTouchable>();
            if (touchable == null)
            {
                touchable = gameObject.AddComponent<NearInteractionTouchable>();
            }
            
            // Important: Set correct orientation for UI elements
            touchable.SetLocalForward(Vector3.forward);
            touchable.SetBounds(buttonCollider.size);
            
        }

        // MRTK Touch Handlers
        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            // Don't launch if button is not visible
            if (!IsButtonVisible())
            {
                return;
            }
            
            Load();
            eventData.Use();
        }

        public void OnTouchCompleted(HandTrackingInputEventData eventData) { }
        public void OnTouchUpdated(HandTrackingInputEventData eventData) { }

        // MRTK Pointer Handlers
        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            // Don't launch if button is not visible
            if (!IsButtonVisible())
            {
                return;
            }
            
            Load();
            eventData.Use();
        }

        public void OnPointerUp(MixedRealityPointerEventData eventData) { }
        public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
        public void OnPointerClicked(MixedRealityPointerEventData eventData) { }

        public void Load()
        {
            // Read from global GameModeSelector
            var gameType = PopBalloons.UI.GameModeSelector.Instance.CurrentGameType;
            var levelNumber = PopBalloons.UI.GameModeSelector.Instance.CurrentLevelNumber;

            // Update MainPanel state if needed
            if (PopBalloons.UI.MainPanel.Instance != null)
            {
                var currentState = PopBalloons.UI.MainPanel.Instance.GetState();

                if (currentState == PopBalloons.UI.MainPanelState.MODE_PICK)
                {
                    PopBalloons.UI.MainPanelState targetState = PopBalloons.UI.MainPanelState.MODE_PICK;
                    
                    switch (gameType)
                    {
                        case GameManager.GameType.MOBILITY:
                            targetState = PopBalloons.UI.MainPanelState.MOBILITY;
                            break;
                        case GameManager.GameType.COGNITIVE:
                            targetState = PopBalloons.UI.MainPanelState.COGNITIVE;
                            break;
                        case GameManager.GameType.FREEPLAY:
                            targetState = PopBalloons.UI.MainPanelState.FREEPLAY;
                            break;
                    }

                    PopBalloons.UI.MainPanel.Instance.SetState(targetState);
                }
            }

            // Launch game
            if (gameType == GameManager.GameType.NONE || levelNumber < 0)
            {
                if (GameCreator.Instance != null)
                {
                    GameCreator.Instance.QuitLevel();
                }
                GameManager.Instance.Home();
            }
            else
            {
                GameManager.Instance.NewGame(gameType, levelNumber);
            }
        }
    }
}