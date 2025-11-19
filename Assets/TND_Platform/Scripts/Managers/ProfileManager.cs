using System;
using UnityEngine;

namespace TNDPlatform.Managers
{
    /// <summary>
    /// Types de profils utilisateurs de la plateforme TND
    /// </summary>
    public enum UserProfile
    {
        None,
        Family,      // 👨‍👩‍👧 Interface Famille
        Clinician,   // 🏥 Interface Clinicien
        Teacher      // 🎓 Interface Enseignant
    }

    /// <summary>
    /// Gestionnaire central des profils utilisateurs
    /// Singleton pattern pour accès global
    /// </summary>
    public class ProfileManager : MonoBehaviour
    {
        #region Singleton
        private static ProfileManager instance;
        public static ProfileManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ProfileManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("ProfileManager");
                        instance = go.AddComponent<ProfileManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Event déclenché quand le profil change
        /// </summary>
        public static event Action<UserProfile> OnProfileChanged;
        #endregion

        #region Variables
        [Header("Configuration")]
        [SerializeField]
        [Tooltip("Profil sélectionné par défaut au démarrage")]
        private UserProfile defaultProfile = UserProfile.None;

        [Header("Mock Data Settings")]
        [SerializeField]
        [Tooltip("Utiliser des données fictives pour la démo")]
        private bool useMockData = true;

        [SerializeField]
        [Tooltip("Nom de l'enfant/patient pour les données fictives")]
        private string mockPatientName = "Marie";

        [SerializeField]
        [Tooltip("Âge du patient fictif")]
        private int mockPatientAge = 8;

        private UserProfile currentProfile = UserProfile.None;
        #endregion

        #region Properties
        /// <summary>
        /// Profil actuellement actif
        /// </summary>
        public UserProfile CurrentProfile => currentProfile;

        /// <summary>
        /// Est-ce qu'on utilise des données fictives ?
        /// </summary>
        public bool UseMockData => useMockData;

        /// <summary>
        /// Nom du patient (mock ou réel)
        /// </summary>
        public string PatientName => useMockData ? mockPatientName : "Patient";

        /// <summary>
        /// Âge du patient
        /// </summary>
        public int PatientAge => useMockData ? mockPatientAge : 0;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Singleton pattern
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log("🎯 ProfileManager initialized");
        }

        private void Start()
        {
            // Charger le profil par défaut si défini
            if (defaultProfile != UserProfile.None)
            {
                SelectProfile(defaultProfile);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Sélectionner un profil utilisateur
        /// </summary>
        public void SelectProfile(UserProfile profile)
        {
            if (profile == UserProfile.None)
            {
                Debug.LogWarning("⚠️ Tentative de sélection du profil 'None' ignorée");
                return;
            }

            UserProfile previousProfile = currentProfile;
            currentProfile = profile;

            Debug.Log($"✅ Profil changé: {previousProfile} → {currentProfile}");

            // Déclencher l'event
            OnProfileChanged?.Invoke(currentProfile);

            // Log selon le profil
            switch (currentProfile)
            {
                case UserProfile.Family:
                    Debug.Log($"👨‍👩‍👧 Interface Famille activée pour {PatientName}");
                    break;
                case UserProfile.Clinician:
                    Debug.Log($"🏥 Interface Clinicien activée - Patient: {PatientName}, {PatientAge} ans");
                    break;
                case UserProfile.Teacher:
                    Debug.Log($"🎓 Interface Enseignant activée - Élève: {PatientName}");
                    break;
            }
        }

        /// <summary>
        /// Retourner au sélecteur de profil
        /// </summary>
        public void ResetProfile()
        {
            UserProfile previousProfile = currentProfile;
            currentProfile = UserProfile.None;

            Debug.Log($"🔄 Retour au sélecteur de profil (depuis {previousProfile})");

            OnProfileChanged?.Invoke(UserProfile.None);
        }

        /// <summary>
        /// Obtenir le nom d'affichage du profil
        /// </summary>
        public static string GetProfileDisplayName(UserProfile profile)
        {
            switch (profile)
            {
                case UserProfile.Family:
                    return "👨‍👩‍👧 Famille";
                case UserProfile.Clinician:
                    return "🏥 Clinicien";
                case UserProfile.Teacher:
                    return "🎓 Enseignant";
                default:
                    return "Sélectionnez un profil";
            }
        }

        /// <summary>
        /// Obtenir la description du profil
        /// </summary>
        public static string GetProfileDescription(UserProfile profile)
        {
            switch (profile)
            {
                case UserProfile.Family:
                    return "Suivi du programme et progression de l'enfant";
                case UserProfile.Clinician:
                    return "Prescription, analyse et suivi thérapeutique";
                case UserProfile.Teacher:
                    return "Profil élève et adaptations pédagogiques";
                default:
                    return "";
            }
        }

        /// <summary>
        /// Obtenir la couleur principale du profil
        /// </summary>
        public static Color GetProfileColor(UserProfile profile)
        {
            switch (profile)
            {
                case UserProfile.Family:
                    return ColorUtility.TryParseHtmlString("#4A90E2", out Color familyColor) ? familyColor : Color.blue;
                case UserProfile.Clinician:
                    return ColorUtility.TryParseHtmlString("#2C5F8D", out Color clinicianColor) ? clinicianColor : Color.cyan;
                case UserProfile.Teacher:
                    return ColorUtility.TryParseHtmlString("#28A745", out Color teacherColor) ? teacherColor : Color.green;
                default:
                    return Color.gray;
            }
        }
        #endregion

        #region Mock Data Methods
        /// <summary>
        /// Définir les données fictives
        /// </summary>
        public void SetMockData(string patientName, int age)
        {
            mockPatientName = patientName;
            mockPatientAge = age;
            Debug.Log($"📝 Données fictives mises à jour: {patientName}, {age} ans");
        }

        /// <summary>
        /// Toggle mock data mode
        /// </summary>
        public void ToggleMockData(bool enabled)
        {
            useMockData = enabled;
            Debug.Log($"📊 Mode données fictives: {(enabled ? "ACTIVÉ" : "DÉSACTIVÉ")}");
        }
        #endregion
    }
}
