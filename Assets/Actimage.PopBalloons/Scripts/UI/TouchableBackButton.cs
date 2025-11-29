using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using PopBalloons.Utilities;

namespace PopBalloons.UI
{
    [RequireComponent(typeof(Collider))]
    public class TouchableBackButton : MonoBehaviour, IMixedRealityPointerHandler, IMixedRealityTouchHandler
    {
        [Header("Configuration")]
        [SerializeField]
        [Tooltip("État du panel vers lequel revenir (par défaut MODE_PICK)")]
        private MainPanelState targetState = MainPanelState.MODE_PICK;

        [SerializeField]
        [Tooltip("Afficher des logs de debug")]
        private bool verbose = false; // Changed to false to reduce log spam

        private bool isPressed = false;
        private NearInteractionTouchable touchable;
        private BoxCollider boxCollider;

        public void SetTargetState(MainPanelState newState)
        {
            this.targetState = newState;
        }

        void Awake()
        {
            Debug.Log("[TouchableBackButton] ===== AWAKE sur '" + gameObject.name + "' =====");
            
            boxCollider = GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                Debug.Log("[TouchableBackButton] Pas de BoxCollider trouvé, création automatique...");
                Renderer r = GetComponentInChildren<Renderer>();
                boxCollider = gameObject.AddComponent<BoxCollider>();
                
                if (r != null)
                {
                    boxCollider.center = transform.InverseTransformPoint(r.bounds.center);
                    Vector3 size = r.bounds.size;
                    boxCollider.size = new Vector3(
                        Mathf.Max(0.001f, size.x),
                        Mathf.Max(0.001f, size.y),
                        Mathf.Max(10f,  size.z)); // Increased from 0.01f to 10f
                    Debug.Log("[TouchableBackButton] BoxCollider dimensionné depuis Renderer: " + boxCollider.size);
                }
                else
                {
                    RectTransform rt = GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        Vector2 s = rt.sizeDelta;
                        boxCollider.center = Vector3.zero;
                        boxCollider.size = new Vector3(
                            Mathf.Max(0.001f, s.x),
                            Mathf.Max(0.001f, s.y),
                            0.1f); // Reduced from 10f
                    }
                    else
                    {
                        boxCollider.size = new Vector3(0.1f, 0.1f, 0.1f); // Reduced from 10f
                    }
                }
            }
            else
            {
                Debug.Log("[TouchableBackButton] BoxCollider existant trouvé: size=" + boxCollider.size + ", center=" + boxCollider.center);
                // Force proper depth even if BoxCollider exists
                if (boxCollider.size.z < 1f)
                {
                    Vector3 newSize = boxCollider.size;
                    newSize.z = 10f;
                    boxCollider.size = newSize;
                    Debug.Log("[TouchableBackButton] BoxCollider depth corrected to: " + boxCollider.size);
                }
            }
            boxCollider.isTrigger = true;

            touchable = GetComponent<NearInteractionTouchable>();
            if (touchable == null)
            {
                touchable = gameObject.AddComponent<NearInteractionTouchable>();
                Debug.Log("[TouchableBackButton] NearInteractionTouchable ajouté sur '" + gameObject.name + "'");
            }
            else
            {
                Debug.Log("[TouchableBackButton] NearInteractionTouchable existant trouvé");
            }
            
            if (boxCollider != null)
            {
                touchable.SetTouchableCollider(boxCollider);
                Debug.Log("[TouchableBackButton] Touchable lié au BoxCollider");
            }
        }

        void Start()
        {
            Debug.Log("[TouchableBackButton] ===== START sur '" + gameObject.name + "' =====");
            
            // Check for conflicting components
            var loadLevelBtn = GetComponent<PopBalloons.Utilities.LoadLevelButton>();
            if (loadLevelBtn != null)
            {
                Debug.LogWarning($"[TouchableBackButton] WARNING: {gameObject.name} has BOTH TouchableBackButton AND LoadLevelButton! This will cause conflicts!");
            }
            
            
            // Désactiver le collider si le GameObject ou l'un de ses parents n'est pas visible
            UpdateColliderState();
            
            MainPanelButton mpb = GetComponent<MainPanelButton>();
        }

        void OnEnable()
        {
            // Reset state when button becomes active again
            isPressed = false;
            UpdateColliderState();
        }

        void OnDisable()
        {
            // Cancel any pending releases to prevent errors
            CancelInvoke("Release");
            isPressed = false;
        }

        void Update()
        {
            // For back buttons, we DO want to disable the collider when invisible
            // because they can be stacked behind other panels
            UpdateColliderState();
        }

        void UpdateColliderState()
        {
            if (boxCollider == null) return;

            // Check visibility thoroughly for back buttons
            bool shouldBeEnabled = IsButtonInteractable();
            
            if (boxCollider.enabled != shouldBeEnabled)
            {
                boxCollider.enabled = shouldBeEnabled;
            }

            // DYNAMIC SIZE FIX:
            // If the button is resized by a Layout Group (like in FixContent), the collider might be wrong.
            // We check and update it if necessary.
            if (shouldBeEnabled)
            {
                RectTransform rt = GetComponent<RectTransform>();
                if (rt != null)
                {
                    Vector2 rectSize = rt.rect.size;
                    Vector3 colSize = boxCollider.size;
                    
                    // If width or height differs by more than 1 unit, update collider
                    if (Mathf.Abs(rectSize.x - colSize.x) > 1f || Mathf.Abs(rectSize.y - colSize.y) > 1f)
                    {
                        // Keep the Z depth (usually 10f or 0.1f)
                        float depth = Mathf.Max(0.1f, colSize.z); 
                        boxCollider.size = new Vector3(rectSize.x, rectSize.y, depth);
                        // Also re-center it just in case
                        boxCollider.center = new Vector3(0, 0, depth / 2f);
                        
                        // Update Touchable bounds too
                        if (touchable != null)
                        {
                            touchable.SetBounds(boxCollider.size);
                        }
                    }
                }
            }
        }

        bool IsButtonInteractable()
        {
            return true;
        }

        public void GoBack()
        {
            // Ne pas lancer de jeu, juste changer le panel
            if (isPressed) return;
            isPressed = true;

            // PRIORITY: If a game is running, we MUST stop it first via GameManager.Home()
            // This overrides any local navigation logic (like MainPanelButton)
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.PLAY)
            {
                // Pass the desired target state to Home() so it doesn't default to MODE_PICK
                GameManager.Instance.Home(targetState);
                Invoke("Release", 0.3f);
                return;
            }

            MainPanelButton proxy = GetComponent<MainPanelButton>();
            if (proxy != null)
            {proxy.OnClick();
                Invoke("Release", 0.3f);
                return;
            }

            if (MainPanel.Instance != null)
            {
                MainPanel.Instance.SetState(targetState);
            }
            else
            {
                Debug.LogError("[TouchableBackButton] MainPanel.Instance is null!");
            }

            Invoke("Release", 0.3f);
        }

        void Release()
        {
            isPressed = false;
        }

        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            // Only process if button is actually visible
            if (!IsButtonInteractable())
            {
                eventData.Use();
                return;
            }
            
            GoBack();
            eventData.Use(); // Consume event to prevent other handlers
        }

        public void OnPointerUp(MixedRealityPointerEventData eventData)
        {
        }

        public void OnPointerClicked(MixedRealityPointerEventData eventData) 
        {
        }
        
        public void OnPointerDragged(MixedRealityPointerEventData eventData) 
        {
        }

        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            // Only process if button is actually visible
            if (!IsButtonInteractable())
            {
                eventData.Use();
                return;
            }
            
            GoBack();
            eventData.Use(); // Consume event to prevent other handlers
        }

        public void OnTouchCompleted(HandTrackingInputEventData eventData)
        {
            Debug.Log("[TouchableBackButton] OnTouchCompleted sur '" + gameObject.name + "'");
        }

        public void OnTouchUpdated(HandTrackingInputEventData eventData) 
        {
            if (verbose) Debug.Log("[TouchableBackButton] OnTouchUpdated sur '" + gameObject.name + "'");
        }
    }
}
