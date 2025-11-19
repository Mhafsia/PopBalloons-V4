using UnityEngine;
using TNDPlatform.Managers;

namespace TNDPlatform.UI
{
    /// <summary>
    /// Classe de base pour toutes les pages de l'application
    /// Gère l'affichage/masquage et les transitions
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class BasePage : MonoBehaviour
    {
        [Header("Page Settings")]
        [SerializeField]
        [Tooltip("Nom unique de la page")]
        protected string pageName;

        [SerializeField]
        [Tooltip("Cette page nécessite-t-elle un profil sélectionné ?")]
        protected bool requiresProfile = true;

        [SerializeField]
        [Tooltip("Profils autorisés (laisser vide pour tous)")]
        protected UserProfile[] allowedProfiles;

        [Header("Animation Settings")]
        [SerializeField]
        [Tooltip("Activer l'animation d'entrée")]
        protected bool animateIn = true;

        [SerializeField]
        [Tooltip("Activer l'animation de sortie")]
        protected bool animateOut = true;

        [SerializeField]
        [Range(0.1f, 1f)]
        [Tooltip("Durée des animations")]
        protected float animationDuration = 0.3f;

        protected CanvasGroup canvasGroup;
        protected bool isVisible = false;

        #region Properties
        public string PageName => pageName;
        public bool IsVisible => isVisible;
        #endregion

        #region Unity Lifecycle
        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // Masquer par défaut
            SetVisibility(false, instant: true);
        }

        protected virtual void OnEnable()
        {
            // S'abonner aux événements de navigation
            NavigationManager.OnPageChanged += OnNavigationChanged;
        }

        protected virtual void OnDisable()
        {
            // Se désabonner
            NavigationManager.OnPageChanged -= OnNavigationChanged;
        }

        protected virtual void Start()
        {
            // Vérifier si c'est la page active au démarrage
            if (NavigationManager.Instance.CurrentPage == pageName)
            {
                Show();
            }
        }
        #endregion

        #region Navigation
        /// <summary>
        /// Appelé quand la navigation change
        /// </summary>
        protected virtual void OnNavigationChanged(string newPage)
        {
            if (newPage == pageName)
            {
                // Cette page devient visible
                if (CanShow())
                {
                    Show();
                }
                else
                {
                    Debug.LogWarning($"⚠️ Impossible d'afficher {pageName} - vérifiez les permissions");
                }
            }
            else if (isVisible)
            {
                // Cette page devient invisible
                Hide();
            }
        }

        /// <summary>
        /// Vérifier si la page peut être affichée
        /// </summary>
        protected virtual bool CanShow()
        {
            // Vérifier si un profil est requis
            if (requiresProfile && ProfileManager.Instance.CurrentProfile == UserProfile.None)
            {
                Debug.LogWarning($"⚠️ {pageName} nécessite un profil sélectionné");
                return false;
            }

            // Vérifier les profils autorisés
            if (allowedProfiles != null && allowedProfiles.Length > 0)
            {
                UserProfile currentProfile = ProfileManager.Instance.CurrentProfile;
                bool isAllowed = System.Array.Exists(allowedProfiles, p => p == currentProfile);

                if (!isAllowed)
                {
                    Debug.LogWarning($"⚠️ {pageName} non autorisé pour le profil {currentProfile}");
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region Visibility
        /// <summary>
        /// Afficher la page
        /// </summary>
        public virtual void Show()
        {
            if (isVisible) return;

            isVisible = true;

            if (animateIn && NavigationManager.Instance.TransitionsEnabled)
            {
                StartCoroutine(AnimateShow());
            }
            else
            {
                SetVisibility(true, instant: true);
            }

            OnShow();
            Debug.Log($"👁️ Page affichée: {pageName}");
        }

        /// <summary>
        /// Masquer la page
        /// </summary>
        public virtual void Hide()
        {
            if (!isVisible) return;

            isVisible = false;

            if (animateOut && NavigationManager.Instance.TransitionsEnabled)
            {
                StartCoroutine(AnimateHide());
            }
            else
            {
                SetVisibility(false, instant: true);
            }

            OnHide();
            Debug.Log($"🙈 Page masquée: {pageName}");
        }

        /// <summary>
        /// Définir la visibilité directement
        /// </summary>
        protected void SetVisibility(bool visible, bool instant = false)
        {
            if (canvasGroup == null) return;

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;

            gameObject.SetActive(visible);
        }
        #endregion

        #region Animations
        /// <summary>
        /// Animation d'apparition
        /// </summary>
        protected virtual System.Collections.IEnumerator AnimateShow()
        {
            gameObject.SetActive(true);

            float elapsed = 0f;
            canvasGroup.alpha = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / animationDuration);

                // Courbe smooth
                t = Mathf.SmoothStep(0f, 1f, t);

                canvasGroup.alpha = t;

                yield return null;
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// Animation de disparition
        /// </summary>
        protected virtual System.Collections.IEnumerator AnimateHide()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            float elapsed = 0f;
            canvasGroup.alpha = 1f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / animationDuration);

                // Courbe smooth
                t = Mathf.SmoothStep(0f, 1f, t);

                canvasGroup.alpha = 1f - t;

                yield return null;
            }

            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }
        #endregion

        #region Lifecycle Hooks
        /// <summary>
        /// Appelé quand la page devient visible
        /// </summary>
        protected virtual void OnShow()
        {
            // Override dans les classes dérivées
        }

        /// <summary>
        /// Appelé quand la page devient invisible
        /// </summary>
        protected virtual void OnHide()
        {
            // Override dans les classes dérivées
        }
        #endregion
    }
}
