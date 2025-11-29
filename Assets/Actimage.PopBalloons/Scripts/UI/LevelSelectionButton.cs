using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;

namespace PopBalloons.UI
{
    /// <summary>
    /// Button to select a specific level number (0=tutorial, 1-15=levels)
    /// Updates GameModeSelector.CurrentLevelNumber
    /// </summary>
    public class LevelSelectionButton : MonoBehaviour, 
        IMixedRealityPointerHandler, 
        IMixedRealityTouchHandler
    {
        [Header("Level Configuration")]
        [SerializeField]
        [Tooltip("Level number (0 = tutorial, 1-15 = normal levels)")]
        private int levelNumber = 0;

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
            // Don't disable collider - let MRTK handle collision detection
        }

        private bool IsButtonVisible()
        {
            if (!gameObject.activeInHierarchy) return false;

            CanvasGroup localCG = GetComponent<CanvasGroup>();
            if (localCG != null && (localCG.alpha < 0.01f || !localCG.interactable))
            {
                return false;
            }

            return true;
        }

        private void SetupTouchInteraction()
        {
            if (!enableTouchInteraction) return;

            // Setup BoxCollider
            buttonCollider = GetComponent<BoxCollider>();
            if (buttonCollider == null)
            {
                buttonCollider = gameObject.AddComponent<BoxCollider>();
            }
            
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                buttonCollider.size = new Vector3(rectTransform.rect.width, rectTransform.rect.height, 0.1f);
            }
            else
            {
                buttonCollider.size = new Vector3(0.1f, 0.1f, 0.1f);
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
        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            if (!IsButtonVisible())
            {
                Debug.LogWarning($"[LevelSelectionButton] {gameObject.name} - Ignoring touch, not visible");
                return;
            }
            
            SelectLevel();
            eventData.Use();
        }

        public void OnTouchCompleted(HandTrackingInputEventData eventData) { }
        public void OnTouchUpdated(HandTrackingInputEventData eventData) { }

        // MRTK Pointer Handlers
        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            if (!IsButtonVisible())
            {
                Debug.LogWarning($"[LevelSelectionButton] {gameObject.name} - Ignoring click, not visible");
                return;
            }
            
            SelectLevel();
            eventData.Use();
        }

        public void OnPointerUp(MixedRealityPointerEventData eventData) { }
        public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
        public void OnPointerClicked(MixedRealityPointerEventData eventData) { }

        public void SelectLevel()
        {
            
            // Update global level number
            GameModeSelector.Instance.CurrentLevelNumber = levelNumber;
        }
    }
}
