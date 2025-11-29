using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using PopBalloons.Utilities;

namespace PopBalloons.UI
{
    /// <summary>
    /// Button to select game mode (Mobility, Cognitive, FreePlay)
    /// Updates GameModeSelector.CurrentGameType and switches to level selection panel
    /// </summary>
    public class ModeButton : MonoBehaviour, 
        IMixedRealityPointerHandler, 
        IMixedRealityTouchHandler
    {
        [Header("Mode Configuration")]
        [SerializeField]
        [Tooltip("Game type for this mode button")]
        private GameManager.GameType gameType = GameManager.GameType.COGNITIVE;

        [Header("Touch Interaction")]
        [SerializeField] private bool enableTouchInteraction = true;

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
            if (!enableTouchInteraction) return;

            // Setup BoxCollider
            // Setup BoxCollider
            buttonCollider = GetComponent<BoxCollider>();
            
            // Only configure size if we had to add the collider ourselves
            // This preserves manually configured colliders in the Inspector
            if (buttonCollider == null)
            {
                buttonCollider = gameObject.AddComponent<BoxCollider>();
                
                RectTransform rectTransform = GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    buttonCollider.size = new Vector3(rectTransform.rect.width, rectTransform.rect.height, 0.1f);
                }
                else
                {
                    buttonCollider.size = new Vector3(0.1f, 0.1f, 0.1f);
                }
            }
            
            buttonCollider.isTrigger = true;

            // Setup NearInteractionTouchable
            touchable = GetComponent<NearInteractionTouchable>();
            if (touchable == null)
            {
                touchable = gameObject.AddComponent<NearInteractionTouchable>();
            }
            
            touchable.SetLocalForward(Vector3.forward);
            touchable.SetBounds(buttonCollider.size);
        }

        // MRTK Touch Handlers
        // MRTK Touch Handlers
        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            if (!IsButtonVisible())
            {
                return;
            }SelectMode();
            eventData.Use();
        }

        public void OnTouchCompleted(HandTrackingInputEventData eventData) { }
        public void OnTouchUpdated(HandTrackingInputEventData eventData) { }

        // MRTK Pointer Handlers
        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            if (!IsButtonVisible())
            {
                return;
            }SelectMode();
            eventData.Use();
        }

        public void OnPointerUp(MixedRealityPointerEventData eventData) { }
        public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
        public void OnPointerClicked(MixedRealityPointerEventData eventData) { }

        public void SelectMode()
        {
            // Update global game type
            GameModeSelector.Instance.CurrentGameType = gameType;
            
            // Handle FreePlay special case
            if (gameType == GameManager.GameType.FREEPLAY)
            {
                GameModeSelector.Instance.CurrentLevelNumber = 0;
                
                // Update MainPanel state to show intro panel
                if (MainPanel.Instance != null)
                {
                    MainPanel.Instance.SetState(MainPanelState.FREEPLAY);
                }
                return;
            }
            else
            {
                GameModeSelector.Instance.CurrentLevelNumber = 1;
            }

            // For Mobility/Cognitive - switch to level selection panel
            if (MainPanel.Instance != null)
            {
                MainPanelState targetState = MainPanelState.MODE_PICK;
                
                switch (gameType)
                {
                    case GameManager.GameType.MOBILITY:
                        targetState = MainPanelState.MOBILITY;
                        break;
                    case GameManager.GameType.COGNITIVE:
                        targetState = MainPanelState.COGNITIVE;
                        break;
                }

                MainPanel.Instance.SetState(targetState);
            }
        }
    }
}
