using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace TNDPlatform.UI.Components
{
    /// <summary>
    /// Variante de bouton
    /// </summary>
    public enum ButtonVariant
    {
        Primary,    // Couleur du profil, texte blanc
        Secondary,  // Fond blanc, bordure colorée
        Outline,    // Transparent, bordure colorée
        Ghost,      // Transparent, texte coloré
        Danger      // Rouge, pour actions destructives
    }

    /// <summary>
    /// Taille de bouton
    /// </summary>
    public enum ButtonSize
    {
        Small,      // 80x40
        Medium,     // 120x50
        Large       // 160x60
    }

    /// <summary>
    /// Composant StyledButton réutilisable
    /// Bouton avec icône, texte et styles adaptés au profil
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class StyledButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Button Settings")]
        [SerializeField]
        private ButtonVariant variant = ButtonVariant.Primary;

        [SerializeField]
        private ButtonSize buttonSize = ButtonSize.Medium;

        [SerializeField]
        [Tooltip("Utiliser la couleur du profil actif")]
        private bool useProfileColor = true;

        [SerializeField]
        [Tooltip("Couleur personnalisée (si useProfileColor = false)")]
        private Color customColor = new Color(0.29f, 0.56f, 0.89f);

        [Header("UI References")]
        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private Image borderImage;

        [SerializeField]
        private TextMeshProUGUI labelText;

        [SerializeField]
        private TextMeshProUGUI iconText;

        [SerializeField]
        private Image iconImage;

        [SerializeField]
        [Tooltip("Icône à droite au lieu de gauche")]
        private bool iconOnRight = false;

        private Button button;
        private RectTransform rectTransform;
        private bool isHovered = false;
        private Color currentColor;

        #region Unity Lifecycle
        private void Awake()
        {
            button = GetComponent<Button>();
            rectTransform = GetComponent<RectTransform>();
            
            ApplySettings();
        }

        private void OnEnable()
        {
            // S'abonner aux changements de profil
            if (useProfileColor)
            {
                Managers.ProfileManager.OnProfileChanged += OnProfileChanged;
            }
        }

        private void OnDisable()
        {
            // Se désabonner
            Managers.ProfileManager.OnProfileChanged -= OnProfileChanged;
        }

        private void OnValidate()
        {
            if (Application.isPlaying) return;
            
            rectTransform = GetComponent<RectTransform>();
            ApplySettings();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Définir le texte du bouton
        /// </summary>
        public void SetText(string text)
        {
            if (labelText != null)
            {
                labelText.text = text;
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
        /// Masquer l'icône
        /// </summary>
        public void HideIcon()
        {
            if (iconText != null)
                iconText.gameObject.SetActive(false);
            
            if (iconImage != null)
                iconImage.gameObject.SetActive(false);
        }

        /// <summary>
        /// Changer la variante du bouton
        /// </summary>
        public void SetVariant(ButtonVariant newVariant)
        {
            variant = newVariant;
            ApplyVariantStyle();
        }

        /// <summary>
        /// Changer la taille du bouton
        /// </summary>
        public void SetSize(ButtonSize newSize)
        {
            buttonSize = newSize;
            ApplySize();
        }

        /// <summary>
        /// Activer/désactiver le bouton
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
                ApplyDisabledState(!interactable);
            }
        }

        /// <summary>
        /// Définir une couleur personnalisée
        /// </summary>
        public void SetCustomColor(Color color)
        {
            useProfileColor = false;
            customColor = color;
            ApplyVariantStyle();
        }

        /// <summary>
        /// Utiliser la couleur du profil
        /// </summary>
        public void UseProfileColor(bool use)
        {
            useProfileColor = use;
            ApplyVariantStyle();
        }

        /// <summary>
        /// Ajouter un listener au clic
        /// </summary>
        public void AddClickListener(UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.AddListener(action);
            }
        }

        /// <summary>
        /// Retirer tous les listeners
        /// </summary>
        public void ClearClickListeners()
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
        #endregion

        #region Event Handlers
        private void OnProfileChanged(Managers.UserProfile newProfile)
        {
            if (useProfileColor)
            {
                ApplyVariantStyle();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button != null && button.interactable)
            {
                isHovered = true;
                ApplyHoverState();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            ApplyVariantStyle();
        }
        #endregion

        #region Settings Application
        /// <summary>
        /// Appliquer tous les paramètres
        /// </summary>
        private void ApplySettings()
        {
            ApplySize();
            ApplyVariantStyle();
        }

        /// <summary>
        /// Appliquer la taille
        /// </summary>
        private void ApplySize()
        {
            if (rectTransform == null) return;

            Vector2 sizeVector = GetSizeVector();
            rectTransform.sizeDelta = sizeVector;

            // Ajuster la taille du texte
            if (labelText != null)
            {
                float fontSize = GetFontSize();
                labelText.fontSize = fontSize;
            }

            if (iconText != null)
            {
                float iconSize = GetIconSize();
                iconText.fontSize = iconSize;
            }
        }

        /// <summary>
        /// Obtenir la taille en pixels
        /// </summary>
        private Vector2 GetSizeVector()
        {
            switch (buttonSize)
            {
                case ButtonSize.Small:
                    return new Vector2(80f, 40f);
                case ButtonSize.Medium:
                    return new Vector2(120f, 50f);
                case ButtonSize.Large:
                    return new Vector2(160f, 60f);
                default:
                    return new Vector2(120f, 50f);
            }
        }

        /// <summary>
        /// Obtenir la taille de police
        /// </summary>
        private float GetFontSize()
        {
            switch (buttonSize)
            {
                case ButtonSize.Small:
                    return 14f;
                case ButtonSize.Medium:
                    return 16f;
                case ButtonSize.Large:
                    return 18f;
                default:
                    return 16f;
            }
        }

        /// <summary>
        /// Obtenir la taille de l'icône
        /// </summary>
        private float GetIconSize()
        {
            switch (buttonSize)
            {
                case ButtonSize.Small:
                    return 16f;
                case ButtonSize.Medium:
                    return 20f;
                case ButtonSize.Large:
                    return 24f;
                default:
                    return 20f;
            }
        }

        /// <summary>
        /// Appliquer le style de la variante
        /// </summary>
        private void ApplyVariantStyle()
        {
            currentColor = GetCurrentColor();

            switch (variant)
            {
                case ButtonVariant.Primary:
                    ApplyPrimaryStyle();
                    break;
                
                case ButtonVariant.Secondary:
                    ApplySecondaryStyle();
                    break;
                
                case ButtonVariant.Outline:
                    ApplyOutlineStyle();
                    break;
                
                case ButtonVariant.Ghost:
                    ApplyGhostStyle();
                    break;
                
                case ButtonVariant.Danger:
                    ApplyDangerStyle();
                    break;
            }
        }

        /// <summary>
        /// Obtenir la couleur actuelle (profil ou personnalisée)
        /// </summary>
        private Color GetCurrentColor()
        {
            if (useProfileColor && Managers.ProfileManager.Instance != null)
            {
                return Managers.ProfileManager.GetProfileColor(
                    Managers.ProfileManager.Instance.CurrentProfile
                );
            }
            
            return customColor;
        }

        /// <summary>
        /// Style Primary: fond coloré, texte blanc
        /// </summary>
        private void ApplyPrimaryStyle()
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = currentColor;
            }

            if (borderImage != null)
            {
                borderImage.gameObject.SetActive(false);
            }

            SetTextColor(Color.white);
        }

        /// <summary>
        /// Style Secondary: fond blanc, bordure colorée
        /// </summary>
        private void ApplySecondaryStyle()
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = Color.white;
            }

            if (borderImage != null)
            {
                borderImage.gameObject.SetActive(true);
                borderImage.color = currentColor;
            }

            SetTextColor(currentColor);
        }

        /// <summary>
        /// Style Outline: transparent, bordure colorée
        /// </summary>
        private void ApplyOutlineStyle()
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(1f, 1f, 1f, 0f); // Transparent
            }

            if (borderImage != null)
            {
                borderImage.gameObject.SetActive(true);
                borderImage.color = currentColor;
            }

            SetTextColor(currentColor);
        }

        /// <summary>
        /// Style Ghost: transparent, texte coloré
        /// </summary>
        private void ApplyGhostStyle()
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(1f, 1f, 1f, 0f); // Transparent
            }

            if (borderImage != null)
            {
                borderImage.gameObject.SetActive(false);
            }

            SetTextColor(currentColor);
        }

        /// <summary>
        /// Style Danger: rouge, texte blanc
        /// </summary>
        private void ApplyDangerStyle()
        {
            Color dangerColor = new Color(0.86f, 0.2f, 0.2f); // Rouge #DC3545

            if (backgroundImage != null)
            {
                backgroundImage.color = dangerColor;
            }

            if (borderImage != null)
            {
                borderImage.gameObject.SetActive(false);
            }

            SetTextColor(Color.white);
        }

        /// <summary>
        /// Définir la couleur du texte et de l'icône
        /// </summary>
        private void SetTextColor(Color color)
        {
            if (labelText != null)
                labelText.color = color;
            
            if (iconText != null)
                iconText.color = color;
            
            if (iconImage != null)
                iconImage.color = color;
        }

        /// <summary>
        /// Appliquer l'état hover
        /// </summary>
        private void ApplyHoverState()
        {
            if (backgroundImage != null)
            {
                // Assombrir légèrement
                Color hoverColor = backgroundImage.color * 0.9f;
                hoverColor.a = backgroundImage.color.a;
                backgroundImage.color = hoverColor;
            }
        }

        /// <summary>
        /// Appliquer l'état désactivé
        /// </summary>
        private void ApplyDisabledState(bool disabled)
        {
            float alpha = disabled ? 0.5f : 1f;

            if (backgroundImage != null)
            {
                Color color = backgroundImage.color;
                color.a = alpha;
                backgroundImage.color = color;
            }

            if (labelText != null)
            {
                Color color = labelText.color;
                color.a = alpha;
                labelText.color = color;
            }

            if (iconText != null)
            {
                Color color = iconText.color;
                color.a = alpha;
                iconText.color = color;
            }

            if (iconImage != null)
            {
                Color color = iconImage.color;
                color.a = alpha;
                iconImage.color = color;
            }
        }
        #endregion
    }
}
