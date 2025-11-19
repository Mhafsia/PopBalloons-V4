using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TNDPlatform.Managers;

namespace TNDPlatform.UI
{
    /// <summary>
    /// Écran de sélection de profil utilisateur
    /// Affiche un dropdown pour choisir entre Famille, Clinicien, Enseignant
    /// </summary>
    public class ProfileSelector : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        [Tooltip("Dropdown pour sélectionner le profil")]
        private TMP_Dropdown profileDropdown;

        [SerializeField]
        [Tooltip("Bouton de validation")]
        private Button validateButton;

        [SerializeField]
        [Tooltip("Texte de description du profil sélectionné")]
        private TextMeshProUGUI descriptionText;

        [SerializeField]
        [Tooltip("Panel de fond pour la couleur du profil")]
        private Image backgroundPanel;

        [Header("Visual Feedback")]
        [SerializeField]
        [Tooltip("Durée de l'animation de sélection")]
        private float animationDuration = 0.3f;

        [SerializeField]
        [Tooltip("Son de sélection (optionnel)")]
        private AudioClip selectionSound;

        private AudioSource audioSource;
        private UserProfile selectedProfile = UserProfile.None;

        #region Unity Lifecycle
        private void Awake()
        {
            // Vérifications
            if (profileDropdown == null)
            {
                Debug.LogError("❌ ProfileDropdown n'est pas assigné !");
            }

            if (validateButton == null)
            {
                Debug.LogError("❌ ValidateButton n'est pas assigné !");
            }

            // Audio source
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && selectionSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        private void Start()
        {
            InitializeDropdown();
            SetupListeners();
            UpdateUI();
        }

        private void OnDestroy()
        {
            // Cleanup listeners
            if (profileDropdown != null)
            {
                profileDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
            }

            if (validateButton != null)
            {
                validateButton.onClick.RemoveListener(OnValidateClicked);
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialiser le dropdown avec les profils disponibles
        /// </summary>
        private void InitializeDropdown()
        {
            if (profileDropdown == null) return;

            profileDropdown.ClearOptions();

            var options = new System.Collections.Generic.List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData("Sélectionnez votre profil..."),
                new TMP_Dropdown.OptionData(ProfileManager.GetProfileDisplayName(UserProfile.Family)),
                new TMP_Dropdown.OptionData(ProfileManager.GetProfileDisplayName(UserProfile.Clinician)),
                new TMP_Dropdown.OptionData(ProfileManager.GetProfileDisplayName(UserProfile.Teacher))
            };

            profileDropdown.AddOptions(options);
            profileDropdown.value = 0;
            profileDropdown.RefreshShownValue();

            Debug.Log("✅ ProfileDropdown initialisé avec 3 profils");
        }

        /// <summary>
        /// Configurer les listeners d'events
        /// </summary>
        private void SetupListeners()
        {
            if (profileDropdown != null)
            {
                profileDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            }

            if (validateButton != null)
            {
                validateButton.onClick.AddListener(OnValidateClicked);
                validateButton.interactable = false; // Désactivé par défaut
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Appelé quand la valeur du dropdown change
        /// </summary>
        private void OnDropdownValueChanged(int index)
        {
            // Index 0 = "Sélectionnez..."
            // Index 1 = Family
            // Index 2 = Clinician
            // Index 3 = Teacher

            switch (index)
            {
                case 1:
                    selectedProfile = UserProfile.Family;
                    break;
                case 2:
                    selectedProfile = UserProfile.Clinician;
                    break;
                case 3:
                    selectedProfile = UserProfile.Teacher;
                    break;
                default:
                    selectedProfile = UserProfile.None;
                    break;
            }

            Debug.Log($"📋 Profil sélectionné dans dropdown: {selectedProfile}");

            UpdateUI();
            PlaySelectionSound();
        }

        /// <summary>
        /// Appelé quand on clique sur le bouton Valider
        /// </summary>
        private void OnValidateClicked()
        {
            if (selectedProfile == UserProfile.None)
            {
                Debug.LogWarning("⚠️ Aucun profil sélectionné");
                return;
            }

            Debug.Log($"✅ Validation du profil: {selectedProfile}");

            // Activer le profil dans le ProfileManager
            ProfileManager.Instance.SelectProfile(selectedProfile);

            // Naviguer vers le dashboard approprié
            NavigationManager.Instance.GoToProfileHome();
        }
        #endregion

        #region UI Update
        /// <summary>
        /// Mettre à jour l'interface en fonction du profil sélectionné
        /// </summary>
        private void UpdateUI()
        {
            bool hasSelection = selectedProfile != UserProfile.None;

            // Activer/désactiver le bouton de validation
            if (validateButton != null)
            {
                validateButton.interactable = hasSelection;
            }

            // Mettre à jour le texte de description
            if (descriptionText != null)
            {
                if (hasSelection)
                {
                    descriptionText.text = ProfileManager.GetProfileDescription(selectedProfile);
                    descriptionText.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                }
                else
                {
                    descriptionText.text = "Choisissez votre profil pour commencer";
                    descriptionText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                }
            }

            // Changer la couleur de fond
            if (backgroundPanel != null)
            {
                Color targetColor = hasSelection
                    ? ProfileManager.GetProfileColor(selectedProfile)
                    : new Color(0.95f, 0.95f, 0.95f, 1f);

                // Animation simple de la couleur
                StartCoroutine(AnimateBackgroundColor(targetColor));
            }
        }

        /// <summary>
        /// Animer le changement de couleur de fond
        /// </summary>
        private System.Collections.IEnumerator AnimateBackgroundColor(Color targetColor)
        {
            if (backgroundPanel == null) yield break;

            Color startColor = backgroundPanel.color;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;

                // Courbe d'animation smooth
                t = Mathf.SmoothStep(0f, 1f, t);

                backgroundPanel.color = Color.Lerp(startColor, targetColor, t);

                yield return null;
            }

            backgroundPanel.color = targetColor;
        }
        #endregion

        #region Audio
        /// <summary>
        /// Jouer le son de sélection
        /// </summary>
        private void PlaySelectionSound()
        {
            if (audioSource != null && selectionSound != null)
            {
                audioSource.PlayOneShot(selectionSound);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Réinitialiser la sélection
        /// </summary>
        public void ResetSelection()
        {
            if (profileDropdown != null)
            {
                profileDropdown.value = 0;
                profileDropdown.RefreshShownValue();
            }

            selectedProfile = UserProfile.None;
            UpdateUI();

            Debug.Log("🔄 Sélection de profil réinitialisée");
        }
        #endregion
    }
}
