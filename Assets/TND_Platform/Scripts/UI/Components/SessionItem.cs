using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TNDPlatform.Data;

namespace TNDPlatform.UI.Components
{
    /// <summary>
    /// Item de séance dans la liste du programme
    /// Affiche: heure, nom, statut, score
    /// </summary>
    public class SessionItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private TextMeshProUGUI timeText;

        [SerializeField]
        private TextMeshProUGUI gameNameText;

        [SerializeField]
        private TextMeshProUGUI statusIconText;

        [SerializeField]
        private TextMeshProUGUI scoreText;

        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private Image statusIndicator;

        [SerializeField]
        private Button itemButton;

        [Header("Colors")]
        [SerializeField]
        private Color completedColor = new Color(0.49f, 0.83f, 0.13f); // Vert #7ED321

        [SerializeField]
        private Color pendingColor = new Color(0.96f, 0.65f, 0.14f); // Orange #F5A623

        [SerializeField]
        private Color inProgressColor = new Color(0.29f, 0.56f, 0.89f); // Bleu #4A90E2

        [SerializeField]
        private Color skippedColor = new Color(0.7f, 0.7f, 0.7f); // Gris

        private SessionData sessionData;
        private System.Action<SessionData> onItemClicked;

        #region Public Methods
        /// <summary>
        /// Configurer l'item avec les données de session
        /// </summary>
        public void Setup(SessionData session, System.Action<SessionData> clickCallback = null)
        {
            sessionData = session;
            onItemClicked = clickCallback;

            UpdateDisplay();

            // Configurer le bouton
            if (itemButton != null)
            {
                itemButton.onClick.RemoveAllListeners();
                
                if (clickCallback != null)
                {
                    itemButton.onClick.AddListener(() => clickCallback.Invoke(session));
                }
            }
        }

        /// <summary>
        /// Mettre à jour l'affichage
        /// </summary>
        public void Refresh()
        {
            if (sessionData != null)
            {
                UpdateDisplay();
            }
        }
        #endregion

        #region Display Update
        /// <summary>
        /// Mettre à jour tous les éléments visuels
        /// </summary>
        private void UpdateDisplay()
        {
            if (sessionData == null) return;

            UpdateTimeText();
            UpdateGameName();
            UpdateStatus();
            UpdateScore();
            UpdateBackgroundColor();
        }

        /// <summary>
        /// Afficher l'heure de la séance
        /// </summary>
        private void UpdateTimeText()
        {
            if (timeText == null) return;

            if (sessionData.Status == SessionStatus.Completed && sessionData.CompletedTime.HasValue)
            {
                // Afficher l'heure de complétion
                timeText.text = sessionData.CompletedTime.Value.ToString("HH:mm");
            }
            else
            {
                // Afficher l'heure prévue
                timeText.text = sessionData.ScheduledTime.ToString("HH:mm");
            }
        }

        /// <summary>
        /// Afficher le nom du jeu/exercice
        /// </summary>
        private void UpdateGameName()
        {
            if (gameNameText == null) return;

            gameNameText.text = sessionData.GameName;
        }

        /// <summary>
        /// Afficher le statut avec icône et couleur
        /// </summary>
        private void UpdateStatus()
        {
            string statusIcon = sessionData.GetStatusIcon();
            Color statusColor = GetStatusColor();

            // Icône de statut
            if (statusIconText != null)
            {
                statusIconText.text = statusIcon;
                statusIconText.color = statusColor;
            }

            // Indicateur coloré à gauche
            if (statusIndicator != null)
            {
                statusIndicator.color = statusColor;
            }
        }

        /// <summary>
        /// Afficher le score (si complété)
        /// </summary>
        private void UpdateScore()
        {
            if (scoreText == null) return;

            if (sessionData.Status == SessionStatus.Completed && sessionData.Score.HasValue)
            {
                scoreText.text = $"{sessionData.Score.Value} pts";
                scoreText.gameObject.SetActive(true);
            }
            else if (sessionData.Status == SessionStatus.InProgress)
            {
                scoreText.text = "En cours...";
                scoreText.gameObject.SetActive(true);
            }
            else
            {
                scoreText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Couleur de fond selon le statut
        /// </summary>
        private void UpdateBackgroundColor()
        {
            if (backgroundImage == null) return;

            Color bgColor = Color.white;

            switch (sessionData.Status)
            {
                case SessionStatus.Completed:
                    // Fond légèrement vert
                    bgColor = new Color(
                        completedColor.r,
                        completedColor.g,
                        completedColor.b,
                        0.1f
                    );
                    break;

                case SessionStatus.InProgress:
                    // Fond légèrement bleu
                    bgColor = new Color(
                        inProgressColor.r,
                        inProgressColor.g,
                        inProgressColor.b,
                        0.1f
                    );
                    break;

                case SessionStatus.Pending:
                    // Fond blanc
                    bgColor = Color.white;
                    break;

                case SessionStatus.Skipped:
                    // Fond gris clair
                    bgColor = new Color(0.95f, 0.95f, 0.95f);
                    break;
            }

            backgroundImage.color = bgColor;
        }

        /// <summary>
        /// Obtenir la couleur selon le statut
        /// </summary>
        private Color GetStatusColor()
        {
            switch (sessionData.Status)
            {
                case SessionStatus.Completed:
                    return completedColor;
                
                case SessionStatus.InProgress:
                    return inProgressColor;
                
                case SessionStatus.Pending:
                    return pendingColor;
                
                case SessionStatus.Skipped:
                    return skippedColor;
                
                default:
                    return Color.gray;
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Obtenir les données de session
        /// </summary>
        public SessionData GetSessionData()
        {
            return sessionData;
        }

        /// <summary>
        /// Définir l'interactivité
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            if (itemButton != null)
            {
                itemButton.interactable = interactable;
            }
        }

        /// <summary>
        /// Marquer la séance comme complétée (avec animation)
        /// </summary>
        public void MarkAsCompleted(int score, float duration)
        {
            if (sessionData == null) return;

            sessionData.Status = SessionStatus.Completed;
            sessionData.Score = score;
            sessionData.Duration = duration;
            sessionData.CompletedTime = System.DateTime.Now;

            StartCoroutine(AnimateCompletion());
        }

        /// <summary>
        /// Animation de complétion
        /// </summary>
        private System.Collections.IEnumerator AnimateCompletion()
        {
            // Scale pulse
            Vector3 originalScale = transform.localScale;
            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.1f;
                transform.localScale = originalScale * scale;
                yield return null;
            }

            transform.localScale = originalScale;

            // Mettre à jour l'affichage
            UpdateDisplay();
        }
        #endregion
    }
}
