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
        private bool verbose = true;

        private bool isPressed = false;
        private NearInteractionTouchable touchable;
        private BoxCollider boxCollider;

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
                        Mathf.Max(0.01f,  size.z));
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
                            0.01f);
                        Debug.Log("[TouchableBackButton] BoxCollider dimensionné depuis RectTransform: " + boxCollider.size);
                    }
                    else
                    {
                        boxCollider.size = new Vector3(0.1f, 0.1f, 0.01f);
                        Debug.Log("[TouchableBackButton] BoxCollider taille par défaut: " + boxCollider.size);
                    }
                }
            }
            else
            {
                Debug.Log("[TouchableBackButton] BoxCollider existant trouvé: size=" + boxCollider.size + ", center=" + boxCollider.center);
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
            if (boxCollider != null)
            {
                boxCollider.isTrigger = true;
                Debug.Log("[TouchableBackButton] Initialisé - BoxCollider trigger configuré");
                Debug.Log("[TouchableBackButton] Position: " + transform.position + ", Rotation: " + transform.rotation.eulerAngles);
                Debug.Log("[TouchableBackButton] Layer: " + LayerMask.LayerToName(gameObject.layer));
            }
            else
            {
                Debug.LogError("[TouchableBackButton] Pas de BoxCollider trouvé!");
            }
            
            MainPanelButton mpb = GetComponent<MainPanelButton>();
            if (mpb != null)
            {
                Debug.Log("[TouchableBackButton] MainPanelButton détecté - délégation activée");
            }
        }

        void GoBack()
        {
            Debug.Log("[TouchableBackButton] GoBack() appelé sur '" + gameObject.name + "'");
            
            if (isPressed)
            {
                Debug.Log("[TouchableBackButton] Debounce - bouton déjà pressé");
                return;
            }
            isPressed = true;

            Debug.Log("[TouchableBackButton] *** BOUTON TOUCHÉ sur '" + gameObject.name + "' ***");

            MainPanelButton proxy = GetComponent<MainPanelButton>();
            if (proxy != null)
            {
                Debug.Log("[TouchableBackButton] Délégation à MainPanelButton.OnClick()");
                proxy.OnClick();
                Invoke("Release", 0.3f);
                return;
            }

            if (MainPanel.Instance != null)
            {
                if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.PLAY)
                {
                    Debug.Log("[TouchableBackButton] Game en cours, appel GameManager.Home()");
                    GameManager.Instance.Home();
                }
                else
                {
                    Debug.Log("[TouchableBackButton] Navigation directe: " + MainPanel.Instance.GetState() + " vers " + targetState);
                    MainPanel.Instance.SetState(targetState);
                }
            }
            else
            {
                Debug.LogError("[TouchableBackButton] MainPanel.Instance est null!");
            }

            Invoke("Release", 0.3f);
        }

        void Release()
        {
            Debug.Log("[TouchableBackButton] Release sur '" + gameObject.name + "'");
            isPressed = false;
        }

        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            Debug.Log("[TouchableBackButton] OnPointerDown appelé sur '" + gameObject.name + "'");
            GoBack();
        }

        public void OnPointerUp(MixedRealityPointerEventData eventData)
        {
            Debug.Log("[TouchableBackButton] OnPointerUp sur '" + gameObject.name + "'");
        }

        public void OnPointerClicked(MixedRealityPointerEventData eventData) 
        {
            Debug.Log("[TouchableBackButton] OnPointerClicked sur '" + gameObject.name + "'");
        }
        
        public void OnPointerDragged(MixedRealityPointerEventData eventData) 
        {
            if (verbose) Debug.Log("[TouchableBackButton] OnPointerDragged sur '" + gameObject.name + "'");
        }

        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            Debug.Log("[TouchableBackButton] OnTouchStarted appelé sur '" + gameObject.name + "'");
            GoBack();
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
