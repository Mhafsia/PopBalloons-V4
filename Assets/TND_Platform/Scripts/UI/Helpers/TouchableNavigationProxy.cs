using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Input;
using PopBalloons.UI; // For TouchableButton
using TNDPlatform.Attributes;

namespace TNDPlatform.UI.Helpers
{
    /// <summary>
    /// Proxy pour relier un <see cref="TouchableButton"/> aux actions de navigation existantes.
    /// Objectif: éviter de dupliquer la logique des boutons (retour, navigation vers une page, onClick Unity UI).
    ///
    /// Scénarios supportés:
    /// 1. Bouton retour: cochez "Trigger Navigation Back" pour appeler NavigationManager.Instance.GoBack().
    /// 2. Bouton navigation directe: renseignez "Navigate To Page" pour aller vers une page spécifique.
    /// 3. Reuse d'un Button Unity: référencez un Button existant (targetButton) pour réutiliser ses listeners.
    /// 4. Boutons de niveaux: mettez uniquement le Button cible existant; le press tactile invoquera onClick.
    ///
    /// Ordre d'exécution lors d'un press:
    /// - GoBack (si activé)
    /// - NavigateTo (si défini)
    /// - targetButton.onClick (si présent)
    ///
    /// Ajoutez ce composant sur le même GameObject que le collider + TouchableButton.
    /// </summary>
    [DisallowMultipleComponent]
    public class TouchableNavigationProxy : MonoBehaviour, IMixedRealityPointerHandler, IMixedRealityTouchHandler
    {
        [Header("Configuration")]
        [SerializeField]
        [Tooltip("Déclencher NavigationManager.GoBack() lors du press.")] 
        private bool triggerNavigationBack = false;

        [SerializeField]
        [PageSelector(includeEmpty: true)]
        [Tooltip("Nom de la page vers laquelle naviguer (optionnel). Ignoré si vide.")] 
        private string navigateToPage = "";

        [SerializeField]
        [Tooltip("Button Unity existant dont on réutilise les listeners onClick (optionnel).")]
        private Button targetButton;

        [SerializeField]
        [Tooltip("Chercher automatiquement un Button dans ce GameObject ou ses enfants s'il n'est pas renseigné.")] 
        private bool autoFindButton = true;

        [SerializeField]
        [Tooltip("Afficher des logs de debug.")] 
        private bool verbose = false;

        private TouchableButton touchableButton; // Optionnel: si le projet utilise déjà le script TouchableButton
        private bool isPressed;

        private void Awake()
        {
            if (autoFindButton && targetButton == null)
            {
                targetButton = GetComponent<Button>();
                if (targetButton == null)
                {
                    targetButton = GetComponentInChildren<Button>();
                }
            }

            touchableButton = GetComponent<TouchableButton>();
            // Pas besoin de s'abonner à OnPressed: on va intercepter pointer/touch directement pour être indépendant.
        }

        private void Execute()
        {
            if (isPressed) return; // Debounce pour éviter double appel pointeur + touch même frame
            isPressed = true;

            if (triggerNavigationBack)
            {
                Managers.NavigationManager.Instance.GoBack();
            }

            if (!string.IsNullOrEmpty(navigateToPage))
            {
                Managers.NavigationManager.Instance.NavigateTo(navigateToPage);
            }

            if (targetButton != null)
            {
                targetButton.onClick.Invoke();
            }
        }

        private void Release()
        {
            isPressed = false;
        }

        #region MRTK Pointer (far interactions: ray/pinch)
        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            Execute();
        }

        public void OnPointerUp(MixedRealityPointerEventData eventData)
        {
            Release();
        }

        public void OnPointerClicked(MixedRealityPointerEventData eventData) { }
        public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
        #endregion

        #region MRTK Touch (near interactions: poke)
        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            Execute();
        }

        public void OnTouchCompleted(HandTrackingInputEventData eventData)
        {
            Release();
        }

        public void OnTouchUpdated(HandTrackingInputEventData eventData) { }
        #endregion
    }
}
