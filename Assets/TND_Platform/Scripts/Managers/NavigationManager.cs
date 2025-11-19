using System;
using UnityEngine;

namespace TNDPlatform.Managers
{
    /// <summary>
    /// Gestionnaire de navigation entre les différents écrans de la plateforme
    /// </summary>
    public class NavigationManager : MonoBehaviour
    {
        #region Singleton
        private static NavigationManager instance;
        public static NavigationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<NavigationManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("NavigationManager");
                        instance = go.AddComponent<NavigationManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Event déclenché lors d'un changement de page
        /// </summary>
        public static event Action<string> OnPageChanged;

        /// <summary>
        /// Event déclenché lors d'un retour en arrière
        /// </summary>
        public static event Action OnNavigatedBack;
        #endregion

        #region Variables
        [Header("Navigation Settings")]
        [SerializeField]
        [Tooltip("Page de démarrage par défaut")]
        private string defaultPage = "ProfileSelector";

        [SerializeField]
        [Tooltip("Activer les animations de transition")]
        private bool enableTransitions = true;

        [SerializeField]
        [Range(0.1f, 1f)]
        [Tooltip("Durée des animations de transition (secondes)")]
        private float transitionDuration = 0.3f;

        private System.Collections.Generic.Stack<string> navigationHistory = new System.Collections.Generic.Stack<string>();
        private string currentPage = "";
        #endregion

        #region Properties
        /// <summary>
        /// Page actuellement affichée
        /// </summary>
        public string CurrentPage => currentPage;

        /// <summary>
        /// Peut-on naviguer en arrière ?
        /// </summary>
        public bool CanGoBack => navigationHistory.Count > 0;

        /// <summary>
        /// Durée des transitions
        /// </summary>
        public float TransitionDuration => transitionDuration;

        /// <summary>
        /// Les transitions sont-elles activées ?
        /// </summary>
        public bool TransitionsEnabled => enableTransitions;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log("🧭 NavigationManager initialized");
        }

        private void Start()
        {
            // Naviguer vers la page par défaut
            if (!string.IsNullOrEmpty(defaultPage))
            {
                NavigateTo(defaultPage, addToHistory: false);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Naviguer vers une page
        /// </summary>
        /// <param name="pageName">Nom de la page de destination</param>
        /// <param name="addToHistory">Ajouter la page actuelle à l'historique ?</param>
        public void NavigateTo(string pageName, bool addToHistory = true)
        {
            if (string.IsNullOrEmpty(pageName))
            {
                Debug.LogWarning("⚠️ Tentative de navigation vers une page vide");
                return;
            }

            // Ajouter la page actuelle à l'historique si demandé
            if (addToHistory && !string.IsNullOrEmpty(currentPage))
            {
                navigationHistory.Push(currentPage);
                Debug.Log($"📚 Historique: {currentPage} ajouté ({navigationHistory.Count} pages)");
            }

            string previousPage = currentPage;
            currentPage = pageName;

            Debug.Log($"🧭 Navigation: {previousPage} → {currentPage}");

            // Déclencher l'event
            OnPageChanged?.Invoke(currentPage);
        }

        /// <summary>
        /// Retourner à la page précédente
        /// </summary>
        public void GoBack()
        {
            if (!CanGoBack)
            {
                Debug.LogWarning("⚠️ Pas de page précédente dans l'historique");
                return;
            }

            string previousPage = navigationHistory.Pop();
            currentPage = previousPage;

            Debug.Log($"⬅️ Retour vers: {currentPage} ({navigationHistory.Count} pages restantes)");

            OnNavigatedBack?.Invoke();
            OnPageChanged?.Invoke(currentPage);
        }

        /// <summary>
        /// Effacer l'historique de navigation
        /// </summary>
        public void ClearHistory()
        {
            int count = navigationHistory.Count;
            navigationHistory.Clear();
            Debug.Log($"🗑️ Historique effacé ({count} pages supprimées)");
        }

        /// <summary>
        /// Naviguer vers la page d'accueil du profil actuel
        /// </summary>
        public void GoToProfileHome()
        {
            UserProfile profile = ProfileManager.Instance.CurrentProfile;

            switch (profile)
            {
                case UserProfile.Family:
                    NavigateTo("FamilyDashboard", addToHistory: false);
                    break;
                case UserProfile.Clinician:
                    NavigateTo("ClinicianDashboard", addToHistory: false);
                    break;
                case UserProfile.Teacher:
                    NavigateTo("TeacherDashboard", addToHistory: false);
                    break;
                default:
                    NavigateTo("ProfileSelector", addToHistory: false);
                    break;
            }

            // Effacer l'historique lors du retour au dashboard
            ClearHistory();
        }

        /// <summary>
        /// Obtenir le fil d'Ariane (breadcrumb)
        /// </summary>
        public string GetBreadcrumb(string separator = " > ")
        {
            if (navigationHistory.Count == 0)
            {
                return currentPage;
            }

            var pages = new System.Collections.Generic.List<string>(navigationHistory);
            pages.Reverse();
            pages.Add(currentPage);

            return string.Join(separator, pages);
        }
        #endregion

        #region Navigation Helpers
        /// <summary>
        /// Pages communes de navigation rapide
        /// </summary>
        public static class Pages
        {
            // Commun
            public const string ProfileSelector = "ProfileSelector";
            public const string Settings = "Settings";

            // Famille
            public const string FamilyDashboard = "FamilyDashboard";
            public const string FamilyGames = "FamilyGames";
            public const string FamilyProgress = "FamilyProgress";
            public const string FamilyRewards = "FamilyRewards";

            // Clinicien
            public const string ClinicianDashboard = "ClinicianDashboard";
            public const string PatientView = "PatientView";
            public const string Prescription = "Prescription";
            public const string Analytics = "Analytics";
            public const string Reports = "Reports";

            // Enseignant
            public const string TeacherDashboard = "TeacherDashboard";
            public const string StudentProfile = "StudentProfile";
            public const string Recommendations = "Recommendations";
            public const string Resources = "Resources";
        }
        #endregion
    }
}
