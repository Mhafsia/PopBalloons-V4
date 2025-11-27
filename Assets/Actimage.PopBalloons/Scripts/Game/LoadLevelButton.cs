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
            // Debug log to confirm setup
            Debug.Log($"[LoadLevelButton] {gameObject.name} initialized (reads from GameModeSelector)");
            
            // UI Configuration Check
            var rect = GetComponent<RectTransform>();
            var col = GetComponent<BoxCollider>();
            
            string status = $"[UI Check] {gameObject.name} initialized.\n";
            if (rect != null && col != null)
            {
                status += $"   - Rect Size: {rect.rect.width}x{rect.rect.height}\n";
                status += $"   - Collider Size: {col.size.x}x{col.size.y}x{col.size.z}\n";
                
                if (Mathf.Abs(rect.rect.width - col.size.x) > 1f || Mathf.Abs(rect.rect.height - col.size.y) > 1f)
                {
                    status += "   - WARNING: Collider size mismatch! Ghost clicks possible.\n";
                }
                else
                {
                    status += "   - Size OK.\n";
                }
            }
            else
            {
                status += "   - MISSING RectTransform or BoxCollider!\n";
            }
            Debug.Log(status);

            if (buttonCollider != null)
            {
                Debug.Log($"[LoadLevelButton] BoxCollider size: {buttonCollider.size}, isTrigger: {buttonCollider.isTrigger}");
            }
            if (touchable != null)
            {
                Debug.Log($"[LoadLevelButton] NearInteractionTouchable configured");
            }
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
                Debug.Log($"[LoadLevelButton] {gameObject.name} - Touch interaction disabled");
                return;
            }

            // Setup BoxCollider
            buttonCollider = GetComponent<BoxCollider>();
            if (buttonCollider == null)
            {
                buttonCollider = gameObject.AddComponent<BoxCollider>();
                Debug.Log($"[LoadLevelButton] {gameObject.name} - Added BoxCollider");
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
                Debug.Log($"[LoadLevelButton] BoxCollider size from RectTransform: {newSize}");
            }
            else
            {
                Renderer renderer = GetComponent<Renderer>();
                if (renderer != null)
                {
                    Vector3 size = renderer.bounds.size;
                    if (size.z < 0.01f) size.z = 10f; // Ensure minimum depth
                    buttonCollider.size = size;
                    Debug.Log($"[LoadLevelButton] BoxCollider size from Renderer: {buttonCollider.size}");
                }
                else
                {
                    buttonCollider.size = new Vector3(0.1f, 0.1f, 10f);
                    Debug.LogWarning($"[LoadLevelButton] {gameObject.name} - Using default BoxCollider size");
                }
            }
            
            buttonCollider.isTrigger = true;

            // Setup NearInteractionTouchable
            touchable = GetComponent<NearInteractionTouchable>();
            if (touchable == null)
            {
                touchable = gameObject.AddComponent<NearInteractionTouchable>();
                Debug.Log($"[LoadLevelButton] {gameObject.name} - Added NearInteractionTouchable");
            }
            
            // Important: Set correct orientation for UI elements
            touchable.SetLocalForward(Vector3.forward);
            touchable.SetBounds(buttonCollider.size);
            
            Debug.Log($"[LoadLevelButton] {gameObject.name} - Touch interaction setup complete");
        }

        // MRTK Touch Handlers
        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            // Don't launch if button is not visible
            if (!IsButtonVisible())
            {
                return;
            }
            
            Debug.Log($"[Navigation] Level Button Touched: {gameObject.name}");
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
            
            Debug.Log($"[Navigation] Level Button Clicked: {gameObject.name}");
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

            Debug.Log($"[Navigation] Requesting Level Load: Type={gameType}, Level={levelNumber}");

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