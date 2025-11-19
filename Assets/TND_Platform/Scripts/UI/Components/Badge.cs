using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TNDPlatform.UI.Components
{
    /// <summary>
    /// Forme du badge
    /// </summary>
    public enum BadgeShape
    {
        Circle,         // Circulaire
        RoundedSquare,  // Carré arrondi
        Hexagon,        // Hexagonal (via sprite)
        Star            // Étoile (via sprite)
    }

    /// <summary>
    /// Taille du badge
    /// </summary>
    public enum BadgeSize
    {
        Small,    // 40x40
        Medium,   // 60x60
        Large,    // 80x80
        ExtraLarge // 120x120
    }

    /// <summary>
    /// Composant Badge réutilisable
    /// Indicateur visuel pour récompenses, statuts, niveaux
    /// </summary>
    public class Badge : MonoBehaviour
    {
        [Header("Badge Settings")]
        [SerializeField]
        private BadgeShape shape = BadgeShape.Circle;

        [SerializeField]
        private BadgeSize size = BadgeSize.Medium;

        [SerializeField]
        private Color badgeColor = new Color(0.29f, 0.56f, 0.89f); // Bleu par défaut

        [SerializeField]
        [Tooltip("Verrouillé/débloqué")]
        private bool isLocked = false;

        [SerializeField]
        [Tooltip("Afficher un cadre doré pour les badges spéciaux")]
        private bool isSpecial = false;

        [Header("UI References")]
        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private Image iconImage;

        [SerializeField]
        private TextMeshProUGUI iconText;

        [SerializeField]
        private TextMeshProUGUI labelText;

        [SerializeField]
        private Image lockOverlay;

        [SerializeField]
        private Image borderImage;

        [SerializeField]
        private GameObject glowEffect;

        private RectTransform rectTransform;

        #region Unity Lifecycle
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplySettings();
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
        /// Définir le label sous le badge
        /// </summary>
        public void SetLabel(string label)
        {
            if (labelText != null)
            {
                labelText.text = label;
                labelText.gameObject.SetActive(!string.IsNullOrEmpty(label));
            }
        }

        /// <summary>
        /// Définir la couleur du badge
        /// </summary>
        public void SetColor(Color color)
        {
            badgeColor = color;
            
            if (backgroundImage != null)
            {
                backgroundImage.color = isLocked ? 
                    new Color(0.5f, 0.5f, 0.5f, 0.5f) : // Gris si verrouillé
                    color;
            }
        }

        /// <summary>
        /// Verrouiller/déverrouiller le badge
        /// </summary>
        public void SetLocked(bool locked)
        {
            isLocked = locked;
            ApplyLockedState();
        }

        /// <summary>
        /// Marquer comme spécial (cadre doré)
        /// </summary>
        public void SetSpecial(bool special)
        {
            isSpecial = special;
            
            if (borderImage != null)
            {
                borderImage.gameObject.SetActive(special);
                borderImage.color = new Color(1f, 0.84f, 0f); // Or #FFD700
            }

            if (glowEffect != null)
            {
                glowEffect.SetActive(special && !isLocked);
            }
        }

        /// <summary>
        /// Changer la forme du badge
        /// </summary>
        public void SetShape(BadgeShape newShape)
        {
            shape = newShape;
            ApplyShape();
        }

        /// <summary>
        /// Changer la taille du badge
        /// </summary>
        public void SetSize(BadgeSize newSize)
        {
            size = newSize;
            ApplySize();
        }

        /// <summary>
        /// Déverrouiller avec animation
        /// </summary>
        public void Unlock()
        {
            isLocked = false;
            StartCoroutine(UnlockAnimation());
        }
        #endregion

        #region Settings Application
        /// <summary>
        /// Appliquer tous les paramètres
        /// </summary>
        private void ApplySettings()
        {
            ApplySize();
            ApplyShape();
            ApplyLockedState();
            SetSpecial(isSpecial);
        }

        /// <summary>
        /// Appliquer la taille
        /// </summary>
        private void ApplySize()
        {
            if (rectTransform == null) return;

            Vector2 sizeVector = GetSizeVector();
            rectTransform.sizeDelta = sizeVector;

            // Ajuster la taille du texte selon la taille du badge
            if (iconText != null)
            {
                float fontSize = GetIconFontSize();
                iconText.fontSize = fontSize;
            }

            if (labelText != null)
            {
                float fontSize = GetLabelFontSize();
                labelText.fontSize = fontSize;
            }
        }

        /// <summary>
        /// Obtenir la taille en pixels
        /// </summary>
        private Vector2 GetSizeVector()
        {
            switch (size)
            {
                case BadgeSize.Small:
                    return new Vector2(40f, 40f);
                case BadgeSize.Medium:
                    return new Vector2(60f, 60f);
                case BadgeSize.Large:
                    return new Vector2(80f, 80f);
                case BadgeSize.ExtraLarge:
                    return new Vector2(120f, 120f);
                default:
                    return new Vector2(60f, 60f);
            }
        }

        /// <summary>
        /// Obtenir la taille de police pour l'icône
        /// </summary>
        private float GetIconFontSize()
        {
            switch (size)
            {
                case BadgeSize.Small:
                    return 20f;
                case BadgeSize.Medium:
                    return 28f;
                case BadgeSize.Large:
                    return 36f;
                case BadgeSize.ExtraLarge:
                    return 48f;
                default:
                    return 28f;
            }
        }

        /// <summary>
        /// Obtenir la taille de police pour le label
        /// </summary>
        private float GetLabelFontSize()
        {
            switch (size)
            {
                case BadgeSize.Small:
                    return 10f;
                case BadgeSize.Medium:
                    return 12f;
                case BadgeSize.Large:
                    return 14f;
                case BadgeSize.ExtraLarge:
                    return 16f;
                default:
                    return 12f;
            }
        }

        /// <summary>
        /// Appliquer la forme
        /// </summary>
        private void ApplyShape()
        {
            if (backgroundImage == null) return;

            // Note: Pour les formes complexes (hexagone, étoile),
            // utiliser des sprites personnalisés
            switch (shape)
            {
                case BadgeShape.Circle:
                    backgroundImage.type = Image.Type.Simple;
                    // Sprite circulaire nécessaire
                    break;
                
                case BadgeShape.RoundedSquare:
                    backgroundImage.type = Image.Type.Sliced;
                    // Sprite avec coins arrondis nécessaire
                    break;
                
                case BadgeShape.Hexagon:
                case BadgeShape.Star:
                    backgroundImage.type = Image.Type.Simple;
                    // Sprites hexagone/étoile nécessaires
                    break;
            }
        }

        /// <summary>
        /// Appliquer l'état verrouillé
        /// </summary>
        private void ApplyLockedState()
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = isLocked ? 
                    new Color(0.5f, 0.5f, 0.5f, 0.5f) : // Gris désaturé
                    badgeColor;
            }

            if (lockOverlay != null)
            {
                lockOverlay.gameObject.SetActive(isLocked);
            }

            if (iconImage != null)
            {
                Color iconColor = isLocked ? 
                    new Color(0.7f, 0.7f, 0.7f, 0.7f) :
                    Color.white;
                iconImage.color = iconColor;
            }

            if (iconText != null)
            {
                Color iconColor = isLocked ? 
                    new Color(0.7f, 0.7f, 0.7f, 0.7f) :
                    Color.white;
                iconText.color = iconColor;
            }

            if (glowEffect != null)
            {
                glowEffect.SetActive(isSpecial && !isLocked);
            }
        }
        #endregion

        #region Animations
        /// <summary>
        /// Animation de déverrouillage
        /// </summary>
        private System.Collections.IEnumerator UnlockAnimation()
        {
            // Désactiver l'overlay immédiatement
            if (lockOverlay != null)
            {
                lockOverlay.gameObject.SetActive(false);
            }

            // Animation de scale
            Vector3 originalScale = transform.localScale;
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Bounce effect
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.3f;
                transform.localScale = originalScale * scale;

                yield return null;
            }

            transform.localScale = originalScale;

            // Restaurer les couleurs
            ApplyLockedState();

            // Activer l'effet glow si spécial
            if (isSpecial && glowEffect != null)
            {
                glowEffect.SetActive(true);
            }
        }
        #endregion

        #region Preset Configurations
        /// <summary>
        /// Configuration pour récompense Bronze
        /// </summary>
        public void SetBronzeReward(string icon, string label)
        {
            SetColor(new Color(0.8f, 0.5f, 0.2f)); // Bronze #CD7F32
            SetIcon(icon);
            SetLabel(label);
            SetSpecial(false);
        }

        /// <summary>
        /// Configuration pour récompense Argent
        /// </summary>
        public void SetSilverReward(string icon, string label)
        {
            SetColor(new Color(0.75f, 0.75f, 0.75f)); // Argent #C0C0C0
            SetIcon(icon);
            SetLabel(label);
            SetSpecial(false);
        }

        /// <summary>
        /// Configuration pour récompense Or
        /// </summary>
        public void SetGoldReward(string icon, string label)
        {
            SetColor(new Color(1f, 0.84f, 0f)); // Or #FFD700
            SetIcon(icon);
            SetLabel(label);
            SetSpecial(true);
        }

        /// <summary>
        /// Configuration pour récompense Spéciale
        /// </summary>
        public void SetSpecialReward(string icon, string label)
        {
            SetColor(new Color(0.58f, 0.26f, 0.84f)); // Violet #9542D6
            SetIcon(icon);
            SetLabel(label);
            SetSpecial(true);
        }
        #endregion
    }
}
