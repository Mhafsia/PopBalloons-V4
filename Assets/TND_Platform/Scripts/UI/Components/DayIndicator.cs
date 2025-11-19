using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace TNDPlatform.UI.Components
{
    /// <summary>
    /// Indicateur de jour dans le calendrier hebdomadaire
    /// Affiche: lettre du jour (L/M/M/J/V/S/D) + statut
    /// </summary>
    public class DayIndicator : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private TextMeshProUGUI dayLetterText;

        [SerializeField]
        private Image statusDotImage;

        [SerializeField]
        private Image backgroundCircle;

        [Header("Colors")]
        [SerializeField]
        private Color completedColor = new Color(0.49f, 0.83f, 0.13f); // Vert #7ED321

        [SerializeField]
        private Color todayColor = new Color(0.29f, 0.56f, 0.89f); // Bleu #4A90E2

        [SerializeField]
        private Color futureColor = new Color(0.8f, 0.8f, 0.8f); // Gris clair

        [SerializeField]
        private Color missedColor = new Color(0.86f, 0.2f, 0.2f); // Rouge #DC3545

        private DayOfWeek dayOfWeek;
        private bool isCompleted;
        private bool isToday;
        private bool isPast;

        #region Public Methods
        /// <summary>
        /// Configurer le jour
        /// </summary>
        public void Setup(DayOfWeek day, bool completed, bool today, bool past)
        {
            dayOfWeek = day;
            isCompleted = completed;
            isToday = today;
            isPast = past;

            UpdateDisplay();
        }

        /// <summary>
        /// Configurer avec une date
        /// </summary>
        public void Setup(DateTime date, bool completed)
        {
            dayOfWeek = date.DayOfWeek;
            isCompleted = completed;
            isToday = date.Date == DateTime.Today;
            isPast = date.Date < DateTime.Today;

            UpdateDisplay();
        }

        /// <summary>
        /// Marquer comme complété
        /// </summary>
        public void SetCompleted(bool completed)
        {
            isCompleted = completed;
            UpdateDisplay();
        }

        /// <summary>
        /// Rafraîchir l'affichage
        /// </summary>
        public void Refresh()
        {
            UpdateDisplay();
        }
        #endregion

        #region Display Update
        /// <summary>
        /// Mettre à jour l'affichage
        /// </summary>
        private void UpdateDisplay()
        {
            UpdateDayLetter();
            UpdateStatusDot();
            UpdateBackground();
        }

        /// <summary>
        /// Afficher la lettre du jour
        /// </summary>
        private void UpdateDayLetter()
        {
            if (dayLetterText == null) return;

            // Obtenir la lettre du jour en français
            string letter = GetDayLetter(dayOfWeek);
            dayLetterText.text = letter;

            // Couleur du texte selon l'état
            if (isToday)
            {
                dayLetterText.color = Color.white;
                dayLetterText.fontStyle = FontStyles.Bold;
            }
            else if (isCompleted)
            {
                dayLetterText.color = new Color(0.17f, 0.24f, 0.31f); // Texte sombre
                dayLetterText.fontStyle = FontStyles.Bold;
            }
            else
            {
                dayLetterText.color = new Color(0.5f, 0.5f, 0.5f); // Gris
                dayLetterText.fontStyle = FontStyles.Normal;
            }
        }

        /// <summary>
        /// Mettre à jour le point de statut
        /// </summary>
        private void UpdateStatusDot()
        {
            if (statusDotImage == null) return;

            // Afficher le point seulement si complété
            statusDotImage.gameObject.SetActive(isCompleted);

            if (isCompleted)
            {
                statusDotImage.color = completedColor;
            }
        }

        /// <summary>
        /// Mettre à jour le cercle de fond
        /// </summary>
        private void UpdateBackground()
        {
            if (backgroundCircle == null) return;

            Color bgColor;

            if (isToday)
            {
                // Aujourd'hui: fond coloré
                bgColor = todayColor;
            }
            else if (isCompleted)
            {
                // Complété: fond vert clair
                bgColor = new Color(
                    completedColor.r,
                    completedColor.g,
                    completedColor.b,
                    0.2f
                );
            }
            else if (isPast && !isCompleted)
            {
                // Passé mais pas fait: fond rouge clair
                bgColor = new Color(
                    missedColor.r,
                    missedColor.g,
                    missedColor.b,
                    0.1f
                );
            }
            else
            {
                // Futur: fond gris très clair
                bgColor = new Color(0.95f, 0.95f, 0.95f);
            }

            backgroundCircle.color = bgColor;
        }

        /// <summary>
        /// Obtenir la lettre du jour en français
        /// </summary>
        private string GetDayLetter(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday:
                    return "L";
                case DayOfWeek.Tuesday:
                    return "M";
                case DayOfWeek.Wednesday:
                    return "M";
                case DayOfWeek.Thursday:
                    return "J";
                case DayOfWeek.Friday:
                    return "V";
                case DayOfWeek.Saturday:
                    return "S";
                case DayOfWeek.Sunday:
                    return "D";
                default:
                    return "?";
            }
        }
        #endregion

        #region Animation
        /// <summary>
        /// Animation de complétion
        /// </summary>
        public void AnimateCompletion()
        {
            StartCoroutine(CompletionAnimation());
        }

        private System.Collections.IEnumerator CompletionAnimation()
        {
            // Marquer comme complété
            isCompleted = true;

            // Scale pulse
            Vector3 originalScale = transform.localScale;
            float duration = 0.4f;
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

            // Mettre à jour l'affichage
            UpdateDisplay();
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Obtenir le jour de la semaine
        /// </summary>
        public DayOfWeek GetDay()
        {
            return dayOfWeek;
        }

        /// <summary>
        /// Vérifier si complété
        /// </summary>
        public bool IsCompleted()
        {
            return isCompleted;
        }

        /// <summary>
        /// Vérifier si c'est aujourd'hui
        /// </summary>
        public bool IsToday()
        {
            return isToday;
        }

        /// <summary>
        /// Réinitialiser
        /// </summary>
        public void Reset()
        {
            isCompleted = false;
            UpdateDisplay();
        }
        #endregion

        #region Static Helpers
        /// <summary>
        /// Obtenir la lettre d'un jour (méthode statique)
        /// </summary>
        public static string GetDayLetterStatic(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday: return "L";
                case DayOfWeek.Tuesday: return "M";
                case DayOfWeek.Wednesday: return "M";
                case DayOfWeek.Thursday: return "J";
                case DayOfWeek.Friday: return "V";
                case DayOfWeek.Saturday: return "S";
                case DayOfWeek.Sunday: return "D";
                default: return "?";
            }
        }

        /// <summary>
        /// Obtenir le nom complet du jour en français
        /// </summary>
        public static string GetDayNameFr(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday: return "Lundi";
                case DayOfWeek.Tuesday: return "Mardi";
                case DayOfWeek.Wednesday: return "Mercredi";
                case DayOfWeek.Thursday: return "Jeudi";
                case DayOfWeek.Friday: return "Vendredi";
                case DayOfWeek.Saturday: return "Samedi";
                case DayOfWeek.Sunday: return "Dimanche";
                default: return "Inconnu";
            }
        }
        #endregion
    }
}
