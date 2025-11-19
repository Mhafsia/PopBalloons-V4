using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace TNDPlatform.UI.Components
{
    /// <summary>
    /// Style de barre de progression
    /// </summary>
    public enum ProgressBarStyle
    {
        Default,        // Bleu standard
        ProfileColor,   // Couleur du profil actif
        Success,        // Vert
        Warning,        // Orange
        Danger,         // Rouge
        Gradient        // Dégradé vert→jaune→orange→rouge
    }

    /// <summary>
    /// Composant ProgressBar réutilisable
    /// Barre de progression horizontale avec label et pourcentage
    /// </summary>
    public class ProgressBar : MonoBehaviour
    {
        [Header("Progress Settings")]
        [SerializeField]
        [Range(0f, 1f)]
        private float value = 0.5f;

        [SerializeField]
        private ProgressBarStyle style = ProgressBarStyle.Default;

        [SerializeField]
        [Tooltip("Animer les changements de valeur")]
        private bool animateChanges = true;

        [SerializeField]
        [Tooltip("Durée de l'animation (secondes)")]
        private float animationDuration = 0.5f;

        [Header("UI References")]
        [SerializeField]
        private Image fillImage;

        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private TextMeshProUGUI labelText;

        [SerializeField]
        private TextMeshProUGUI percentageText;

        [SerializeField]
        [Tooltip("Afficher le pourcentage")]
        private bool showPercentage = true;

        private Coroutine animationCoroutine;
        private float currentDisplayValue;

        #region Unity Lifecycle
        private void Awake()
        {
            currentDisplayValue = value;
            UpdateVisuals();
        }

        private void OnValidate()
        {
            if (Application.isPlaying) return;
            
            currentDisplayValue = value;
            UpdateVisuals();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Définir la valeur de progression (0-1)
        /// </summary>
        public void SetValue(float newValue, bool animate = true)
        {
            newValue = Mathf.Clamp01(newValue);
            
            if (animateChanges && animate && Application.isPlaying)
            {
                AnimateToValue(newValue);
            }
            else
            {
                value = newValue;
                currentDisplayValue = newValue;
                UpdateVisuals();
            }
        }

        /// <summary>
        /// Définir la valeur en pourcentage (0-100)
        /// </summary>
        public void SetPercentage(float percentage, bool animate = true)
        {
            SetValue(percentage / 100f, animate);
        }

        /// <summary>
        /// Définir le label
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
        /// Changer le style de la barre
        /// </summary>
        public void SetStyle(ProgressBarStyle newStyle)
        {
            style = newStyle;
            UpdateVisuals();
        }

        /// <summary>
        /// Afficher/masquer le pourcentage
        /// </summary>
        public void ShowPercentage(bool show)
        {
            showPercentage = show;
            
            if (percentageText != null)
                percentageText.gameObject.SetActive(show);
        }

        /// <summary>
        /// Obtenir la valeur actuelle
        /// </summary>
        public float GetValue()
        {
            return value;
        }

        /// <summary>
        /// Obtenir le pourcentage actuel
        /// </summary>
        public float GetPercentage()
        {
            return value * 100f;
        }

        /// <summary>
        /// Réinitialiser à zéro
        /// </summary>
        public void Reset(bool animate = true)
        {
            SetValue(0f, animate);
        }

        /// <summary>
        /// Remplir complètement
        /// </summary>
        public void Fill(bool animate = true)
        {
            SetValue(1f, animate);
        }
        #endregion

        #region Animation
        /// <summary>
        /// Animer la progression vers une nouvelle valeur
        /// </summary>
        private void AnimateToValue(float targetValue)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }

            animationCoroutine = StartCoroutine(AnimateProgressCoroutine(targetValue));
        }

        private IEnumerator AnimateProgressCoroutine(float targetValue)
        {
            float startValue = currentDisplayValue;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                
                // SmoothStep pour une animation fluide
                t = t * t * (3f - 2f * t);
                
                currentDisplayValue = Mathf.Lerp(startValue, targetValue, t);
                value = currentDisplayValue;
                
                UpdateVisuals();
                
                yield return null;
            }

            currentDisplayValue = targetValue;
            value = targetValue;
            UpdateVisuals();
            
            animationCoroutine = null;
        }
        #endregion

        #region Visual Updates
        /// <summary>
        /// Mettre à jour l'affichage visuel
        /// </summary>
        private void UpdateVisuals()
        {
            // Mise à jour du fill
            if (fillImage != null)
            {
                fillImage.fillAmount = currentDisplayValue;
                fillImage.color = GetFillColor();
            }

            // Mise à jour du texte de pourcentage
            if (percentageText != null && showPercentage)
            {
                int percentage = Mathf.RoundToInt(currentDisplayValue * 100f);
                percentageText.text = $"{percentage}%";
                percentageText.gameObject.SetActive(true);
            }
            else if (percentageText != null)
            {
                percentageText.gameObject.SetActive(false);
            }

            // Mise à jour du fond
            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(0.8f, 0.8f, 0.8f, 0.3f);
            }
        }

        /// <summary>
        /// Obtenir la couleur de remplissage selon le style
        /// </summary>
        private Color GetFillColor()
        {
            switch (style)
            {
                case ProgressBarStyle.Default:
                    return new Color(0.29f, 0.56f, 0.89f); // Bleu #4A90E2
                
                case ProgressBarStyle.ProfileColor:
                    if (Managers.ProfileManager.Instance != null)
                    {
                        return Managers.ProfileManager.GetProfileColor(
                            Managers.ProfileManager.Instance.CurrentProfile
                        );
                    }
                    return new Color(0.29f, 0.56f, 0.89f); // Fallback bleu
                
                case ProgressBarStyle.Success:
                    return new Color(0.49f, 0.83f, 0.13f); // Vert #7ED321
                
                case ProgressBarStyle.Warning:
                    return new Color(0.96f, 0.65f, 0.14f); // Orange #F5A623
                
                case ProgressBarStyle.Danger:
                    return new Color(0.86f, 0.2f, 0.2f); // Rouge #DC3545
                
                case ProgressBarStyle.Gradient:
                    return GetGradientColor(currentDisplayValue);
                
                default:
                    return new Color(0.29f, 0.56f, 0.89f);
            }
        }

        /// <summary>
        /// Obtenir une couleur en dégradé selon la valeur
        /// 0-25%: Rouge, 25-50%: Orange, 50-75%: Jaune, 75-100%: Vert
        /// </summary>
        private Color GetGradientColor(float progress)
        {
            if (progress < 0.25f)
            {
                // Rouge → Orange
                float t = progress / 0.25f;
                return Color.Lerp(
                    new Color(0.86f, 0.2f, 0.2f),  // Rouge
                    new Color(0.96f, 0.65f, 0.14f), // Orange
                    t
                );
            }
            else if (progress < 0.5f)
            {
                // Orange → Jaune
                float t = (progress - 0.25f) / 0.25f;
                return Color.Lerp(
                    new Color(0.96f, 0.65f, 0.14f), // Orange
                    new Color(1f, 0.76f, 0.03f),    // Jaune
                    t
                );
            }
            else if (progress < 0.75f)
            {
                // Jaune → Vert clair
                float t = (progress - 0.5f) / 0.25f;
                return Color.Lerp(
                    new Color(1f, 0.76f, 0.03f),    // Jaune
                    new Color(0.49f, 0.83f, 0.13f), // Vert clair
                    t
                );
            }
            else
            {
                // Vert clair → Vert foncé
                float t = (progress - 0.75f) / 0.25f;
                return Color.Lerp(
                    new Color(0.49f, 0.83f, 0.13f), // Vert clair
                    new Color(0.16f, 0.65f, 0.27f), // Vert foncé
                    t
                );
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Incrémenter la valeur
        /// </summary>
        public void Increment(float amount, bool animate = true)
        {
            SetValue(value + amount, animate);
        }

        /// <summary>
        /// Décrémenter la valeur
        /// </summary>
        public void Decrement(float amount, bool animate = true)
        {
            SetValue(value - amount, animate);
        }

        /// <summary>
        /// Définir la couleur personnalisée
        /// </summary>
        public void SetCustomColor(Color color)
        {
            if (fillImage != null)
            {
                fillImage.color = color;
            }
        }
        #endregion
    }
}
