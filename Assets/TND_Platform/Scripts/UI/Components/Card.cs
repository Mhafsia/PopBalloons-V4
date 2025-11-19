using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TNDPlatform.UI.Components
{
    /// <summary>
    /// Style de carte UI
    /// </summary>
    public enum CardStyle
    {
        Default,      // Blanc, ombre subtile
        Primary,      // Couleur primaire du profil
        Success,      // Vert
        Warning,      // Orange
        Danger,       // Rouge
        Info          // Bleu
    }

    /// <summary>
    /// Composant Card réutilisable
    /// Carte avec titre, icône, contenu et actions
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class Card : MonoBehaviour
    {
        [Header("Card Settings")]
        [SerializeField]
        private CardStyle style = CardStyle.Default;

        [SerializeField]
        [Tooltip("Afficher l'ombre portée")]
        private bool showShadow = true;

        [SerializeField]
        [Tooltip("Rayon des coins arrondis")]
        [Range(0, 30)]
        private float cornerRadius = 12f;

        [Header("Header Section")]
        [SerializeField]
        private GameObject headerSection;

        [SerializeField]
        private TextMeshProUGUI titleText;

        [SerializeField]
        private TextMeshProUGUI iconText;

        [SerializeField]
        private Image iconImage;

        [Header("Content Section")]
        [SerializeField]
        private GameObject contentSection;

        [SerializeField]
        private TextMeshProUGUI bodyText;

        [Header("Footer Section")]
        [SerializeField]
        private GameObject footerSection;

        [SerializeField]
        private Button actionButton;

        [SerializeField]
        private TextMeshProUGUI actionButtonText;

        private Image backgroundImage;
        private Shadow shadowComponent;

        #region Unity Lifecycle
        private void Awake()
        {
            backgroundImage = GetComponent<Image>();
            shadowComponent = GetComponent<Shadow>();

            if (shadowComponent == null && showShadow)
            {
                shadowComponent = gameObject.AddComponent<Shadow>();
            }

            ApplyStyle();
        }

        private void OnValidate()
        {
            // Appliquer le style en mode édition
            if (Application.isPlaying) return;
            
            backgroundImage = GetComponent<Image>();
            ApplyStyle();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Définir le titre de la carte
        /// </summary>
        public void SetTitle(string title)
        {
            if (titleText != null)
            {
                titleText.text = title;
                
                if (headerSection != null)
                    headerSection.SetActive(!string.IsNullOrEmpty(title));
            }
        }

        /// <summary>
        /// Définir l'icône (emoji ou texte)
        /// </summary>
        public void SetIcon(string icon)
        {
            if (iconText != null)
            {
                iconText.text = icon;
                iconText.gameObject.SetActive(!string.IsNullOrEmpty(icon));
            }

            if (iconImage != null)
            {
                iconImage.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Définir l'icône (sprite)
        /// </summary>
        public void SetIcon(Sprite iconSprite)
        {
            if (iconImage != null)
            {
                iconImage.sprite = iconSprite;
                iconImage.gameObject.SetActive(iconSprite != null);
            }

            if (iconText != null)
            {
                iconText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Définir le contenu texte
        /// </summary>
        public void SetBody(string body)
        {
            if (bodyText != null)
            {
                bodyText.text = body;
                
                if (contentSection != null)
                    contentSection.SetActive(!string.IsNullOrEmpty(body));
            }
        }

        /// <summary>
        /// Définir le bouton d'action
        /// </summary>
        public void SetAction(string buttonText, System.Action onClick)
        {
            if (actionButtonText != null)
            {
                actionButtonText.text = buttonText;
            }

            if (actionButton != null)
            {
                actionButton.onClick.RemoveAllListeners();
                actionButton.onClick.AddListener(() => onClick?.Invoke());
                
                if (footerSection != null)
                    footerSection.SetActive(!string.IsNullOrEmpty(buttonText));
            }
        }

        /// <summary>
        /// Masquer le bouton d'action
        /// </summary>
        public void HideAction()
        {
            if (footerSection != null)
                footerSection.SetActive(false);
        }

        /// <summary>
        /// Changer le style de la carte
        /// </summary>
        public void SetStyle(CardStyle newStyle)
        {
            style = newStyle;
            ApplyStyle();
        }

        /// <summary>
        /// Activer/désactiver l'ombre
        /// </summary>
        public void SetShadow(bool enabled)
        {
            showShadow = enabled;
            
            if (shadowComponent != null)
            {
                shadowComponent.enabled = enabled;
            }
            else if (enabled)
            {
                shadowComponent = gameObject.AddComponent<Shadow>();
                ConfigureShadow();
            }
        }
        #endregion

        #region Style Application
        /// <summary>
        /// Appliquer le style visuel
        /// </summary>
        private void ApplyStyle()
        {
            if (backgroundImage == null) return;

            // Couleur de fond selon le style
            Color backgroundColor = GetStyleColor();
            backgroundImage.color = backgroundColor;

            // Configuration de l'ombre
            if (showShadow)
            {
                ConfigureShadow();
            }
            else if (shadowComponent != null)
            {
                shadowComponent.enabled = false;
            }

            // Ajuster la couleur du texte selon le contraste
            UpdateTextColors(backgroundColor);
        }

        /// <summary>
        /// Obtenir la couleur selon le style
        /// </summary>
        private Color GetStyleColor()
        {
            switch (style)
            {
                case CardStyle.Default:
                    return Color.white;
                
                case CardStyle.Primary:
                    // Utiliser la couleur du profil actif
                    if (Managers.ProfileManager.Instance != null)
                    {
                        return Managers.ProfileManager.GetProfileColor(
                            Managers.ProfileManager.Instance.CurrentProfile
                        );
                    }
                    return new Color(0.29f, 0.56f, 0.89f); // Bleu par défaut
                
                case CardStyle.Success:
                    return new Color(0.16f, 0.65f, 0.27f); // #28A745
                
                case CardStyle.Warning:
                    return new Color(1f, 0.76f, 0.03f); // #FFC107
                
                case CardStyle.Danger:
                    return new Color(0.86f, 0.2f, 0.2f); // #DC3545
                
                case CardStyle.Info:
                    return new Color(0.09f, 0.63f, 0.72f); // #17A2B8
                
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// Configurer l'ombre portée
        /// </summary>
        private void ConfigureShadow()
        {
            if (shadowComponent == null) return;

            shadowComponent.effectColor = new Color(0f, 0f, 0f, 0.1f);
            shadowComponent.effectDistance = new Vector2(0f, 4f);
            shadowComponent.enabled = true;
        }

        /// <summary>
        /// Ajuster la couleur du texte pour le contraste
        /// </summary>
        private void UpdateTextColors(Color backgroundColor)
        {
            // Calculer la luminosité du fond
            float luminance = (backgroundColor.r * 0.299f + backgroundColor.g * 0.587f + backgroundColor.b * 0.114f);
            
            // Texte sombre sur fond clair, texte clair sur fond sombre
            Color textColor = luminance > 0.5f ? 
                new Color(0.17f, 0.24f, 0.31f) :  // Texte sombre #2C3E50
                Color.white;                       // Texte clair

            if (titleText != null)
                titleText.color = textColor;

            if (bodyText != null)
                bodyText.color = textColor;

            if (iconText != null)
                iconText.color = textColor;
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Afficher/masquer le header
        /// </summary>
        public void ShowHeader(bool show)
        {
            if (headerSection != null)
                headerSection.SetActive(show);
        }

        /// <summary>
        /// Afficher/masquer le contenu
        /// </summary>
        public void ShowContent(bool show)
        {
            if (contentSection != null)
                contentSection.SetActive(show);
        }

        /// <summary>
        /// Afficher/masquer le footer
        /// </summary>
        public void ShowFooter(bool show)
        {
            if (footerSection != null)
                footerSection.SetActive(show);
        }

        /// <summary>
        /// Obtenir le bouton d'action (pour configuration avancée)
        /// </summary>
        public Button GetActionButton()
        {
            return actionButton;
        }
        #endregion
    }
}
