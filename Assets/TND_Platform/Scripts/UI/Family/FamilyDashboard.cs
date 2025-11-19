using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using TNDPlatform.Managers;
using TNDPlatform.Data;

namespace TNDPlatform.UI.Family
{
    /// <summary>
    /// Dashboard principal de l'interface Famille
    /// Affiche le programme du jour, la progression et les jeux disponibles
    /// </summary>
    public class FamilyDashboard : BasePage
    {
        [Header("Top Bar")]
        [SerializeField]
        private TextMeshProUGUI welcomeText;

        [SerializeField]
        private Button settingsButton;

        [SerializeField]
        private Button helpButton;

        [Header("Today's Program")]
        [SerializeField]
        private Transform sessionsContainer;

        [SerializeField]
        private GameObject sessionItemPrefab;

        [SerializeField]
        private Button launchNextSessionButton;

        [SerializeField]
        private TextMeshProUGUI nextSessionButtonText;

        [Header("Weekly Progress")]
        [SerializeField]
        private TextMeshProUGUI weeklyStatsText;

        [SerializeField]
        private Transform weeklyCalendarContainer;

        [SerializeField]
        private GameObject dayIndicatorPrefab;

        [SerializeField]
        private Image progressFillBar;

        [Header("Quick Actions")]
        [SerializeField]
        private Button gamesButton;

        [SerializeField]
        private Button progressButton;

        [SerializeField]
        private Button rewardsButton;

        [Header("Visual Feedback")]
        [SerializeField]
        private Image backgroundGradient;

        #region Unity Lifecycle
        protected override void Awake()
        {
            base.Awake();
            pageName = NavigationManager.Pages.FamilyDashboard;
            requiresProfile = true;
            allowedProfiles = new UserProfile[] { UserProfile.Family };
        }

        protected override void Start()
        {
            base.Start();
            SetupListeners();
        }

        protected void OnDestroy()
        {
            CleanupListeners();
        }
        #endregion

        #region Setup
        private void SetupListeners()
        {
            // Boutons d'action
            if (launchNextSessionButton != null)
                launchNextSessionButton.onClick.AddListener(OnLaunchNextSession);

            if (gamesButton != null)
                gamesButton.onClick.AddListener(() => NavigationManager.Instance.NavigateTo(NavigationManager.Pages.FamilyGames));

            if (progressButton != null)
                progressButton.onClick.AddListener(() => NavigationManager.Instance.NavigateTo(NavigationManager.Pages.FamilyProgress));

            if (rewardsButton != null)
                rewardsButton.onClick.AddListener(() => NavigationManager.Instance.NavigateTo(NavigationManager.Pages.FamilyRewards));

            if (settingsButton != null)
                settingsButton.onClick.AddListener(() => NavigationManager.Instance.NavigateTo(NavigationManager.Pages.Settings));

            if (helpButton != null)
                helpButton.onClick.AddListener(OnHelpClicked);

            // Events du DataManager
            DataManager.OnSessionCompleted += OnSessionCompleted;
            DataManager.OnDataUpdated += RefreshUI;
        }

        private void CleanupListeners()
        {
            if (launchNextSessionButton != null)
                launchNextSessionButton.onClick.RemoveListener(OnLaunchNextSession);

            if (gamesButton != null)
                gamesButton.onClick.RemoveAllListeners();

            if (progressButton != null)
                progressButton.onClick.RemoveAllListeners();

            if (rewardsButton != null)
                rewardsButton.onClick.RemoveAllListeners();

            if (settingsButton != null)
                settingsButton.onClick.RemoveAllListeners();

            if (helpButton != null)
                helpButton.onClick.RemoveListener(OnHelpClicked);

            DataManager.OnSessionCompleted -= OnSessionCompleted;
            DataManager.OnDataUpdated -= RefreshUI;
        }
        #endregion

        #region Lifecycle Overrides
        protected override void OnShow()
        {
            base.OnShow();
            RefreshUI();
            
            Debug.Log("👨‍👩‍👧 Dashboard Famille affiché");
        }

        protected override void OnHide()
        {
            base.OnHide();
            Debug.Log("👨‍👩‍👧 Dashboard Famille masqué");
        }
        #endregion

        #region UI Update
        /// <summary>
        /// Rafraîchir toute l'interface
        /// </summary>
        private void RefreshUI()
        {
            UpdateWelcomeMessage();
            UpdateTodaysSessions();
            UpdateWeeklyProgress();
            UpdateNextSessionButton();
        }

        /// <summary>
        /// Mettre à jour le message de bienvenue
        /// </summary>
        private void UpdateWelcomeMessage()
        {
            if (welcomeText == null) return;

            string patientName = ProfileManager.Instance.PatientName;
            int hour = DateTime.Now.Hour;
            string greeting = hour < 12 ? "Bonjour" : hour < 18 ? "Bon après-midi" : "Bonsoir";

            welcomeText.text = $"{greeting}, {patientName} ! 👋";
        }

        /// <summary>
        /// Mettre à jour la liste des séances du jour
        /// </summary>
        private void UpdateTodaysSessions()
        {
            if (sessionsContainer == null || sessionItemPrefab == null) return;

            // Nettoyer les éléments existants
            foreach (Transform child in sessionsContainer)
            {
                Destroy(child.gameObject);
            }

            // Créer les nouveaux éléments
            var sessions = DataManager.Instance.TodaySessions;
            
            foreach (var session in sessions)
            {
                GameObject item = Instantiate(sessionItemPrefab, sessionsContainer);
                SetupSessionItem(item, session);
            }

            Debug.Log($"📅 {sessions.Count} séances affichées pour aujourd'hui");
        }

        /// <summary>
        /// Configurer un élément de séance
        /// </summary>
        private void SetupSessionItem(GameObject item, SessionData session)
        {
            // Trouver les composants
            var statusIcon = item.transform.Find("StatusIcon")?.GetComponent<TextMeshProUGUI>();
            var titleText = item.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            var timeText = item.transform.Find("Time")?.GetComponent<TextMeshProUGUI>();
            var typeTag = item.transform.Find("TypeTag")?.GetComponent<Image>();
            var typeText = item.transform.Find("TypeTag/Text")?.GetComponent<TextMeshProUGUI>();

            // Configurer le contenu
            if (statusIcon != null)
            {
                statusIcon.text = session.GetStatusIcon();
                statusIcon.color = session.GetStatusColor();
            }

            if (titleText != null)
            {
                titleText.text = session.gameDisplayName;
                titleText.color = session.status == SessionStatus.Completed ? 
                    new Color(0.5f, 0.5f, 0.5f) : Color.black;
            }

            if (timeText != null)
            {
                if (session.status == SessionStatus.Completed)
                {
                    timeText.text = $"Terminé - {session.score} pts";
                }
                else
                {
                    timeText.text = session.scheduledTime.ToString("HH:mm");
                }
            }

            if (typeTag != null && typeText != null)
            {
                typeText.text = session.exerciseType;
                // Couleur selon le type
                typeTag.color = GetExerciseTypeColor(session.exerciseType);
            }
        }

        /// <summary>
        /// Mettre à jour la progression hebdomadaire
        /// </summary>
        private void UpdateWeeklyProgress()
        {
            if (DataManager.Instance.CurrentWeekProgress == null) return;

            var progress = DataManager.Instance.CurrentWeekProgress;

            // Texte de statistiques
            if (weeklyStatsText != null)
            {
                weeklyStatsText.text = $"{progress.completedSessions} séances effectuées - Continue comme ça ! 🎉";
            }

            // Barre de progression
            if (progressFillBar != null)
            {
                progressFillBar.fillAmount = progress.CompletionRate;
            }

            // Calendrier hebdomadaire
            UpdateWeeklyCalendar(progress);
        }

        /// <summary>
        /// Mettre à jour le calendrier hebdomadaire
        /// </summary>
        private void UpdateWeeklyCalendar(WeeklyProgress progress)
        {
            if (weeklyCalendarContainer == null || dayIndicatorPrefab == null) return;

            // Nettoyer
            foreach (Transform child in weeklyCalendarContainer)
            {
                Destroy(child.gameObject);
            }

            // Jours de la semaine
            string[] dayNames = { "L", "M", "M", "J", "V", "S", "D" };

            for (int i = 0; i < 7; i++)
            {
                GameObject dayItem = Instantiate(dayIndicatorPrefab, weeklyCalendarContainer);
                
                var dayText = dayItem.transform.Find("DayLabel")?.GetComponent<TextMeshProUGUI>();
                var statusIcon = dayItem.transform.Find("StatusIcon")?.GetComponent<TextMeshProUGUI>();
                var background = dayItem.GetComponent<Image>();

                if (dayText != null)
                    dayText.text = dayNames[i];

                if (statusIcon != null)
                {
                    bool isCompleted = progress.dailyCompletion[i];
                    statusIcon.text = isCompleted ? "✓" : "•";
                    statusIcon.color = isCompleted ? new Color(0.16f, 0.65f, 0.27f) : Color.gray;
                }

                if (background != null)
                {
                    // Mettre en surbrillance le jour actuel
                    int currentDay = (int)System.DateTime.Now.DayOfWeek;
                    if (i == currentDay)
                    {
                        background.color = new Color(0.9f, 0.95f, 1f); // Bleu très clair
                    }
                }
            }
        }

        /// <summary>
        /// Mettre à jour le bouton de lancement de séance
        /// </summary>
        private void UpdateNextSessionButton()
        {
            if (launchNextSessionButton == null) return;

            var nextSession = DataManager.Instance.GetNextSession();

            if (nextSession != null)
            {
                launchNextSessionButton.interactable = true;
                
                if (nextSessionButtonText != null)
                {
                    nextSessionButtonText.text = $"Lancer {nextSession.gameDisplayName}";
                }
            }
            else
            {
                launchNextSessionButton.interactable = false;
                
                if (nextSessionButtonText != null)
                {
                    nextSessionButtonText.text = "Aucune séance en attente";
                }
            }
        }
        #endregion

        #region Event Handlers
        private void OnLaunchNextSession()
        {
            var nextSession = DataManager.Instance.GetNextSession();
            
            if (nextSession != null)
            {
                Debug.Log($"🚀 Lancement de la séance: {nextSession.gameDisplayName}");
                
                // Marquer comme en cours
                DataManager.Instance.StartSession(nextSession.sessionId);

                // TODO: Lancer le jeu correspondant
                // Pour l'instant, on va juste naviguer vers une page de jeu
                
                switch (nextSession.gameName)
                {
                    case "PopBalloons":
                        // Lancer PopBalloons
                        LaunchPopBalloons();
                        break;
                    default:
                        Debug.LogWarning($"⚠️ Jeu non implémenté: {nextSession.gameName}");
                        break;
                }
            }
        }

        private void LaunchPopBalloons()
        {
            Debug.Log("🎮 Lancement de PopBalloons...");
            
            // Utiliser le système existant de GameManager
            if (PopBalloons.Utilities.GameManager.Instance != null)
            {
                // Lancer le mode FreePlay par exemple
                PopBalloons.Utilities.GameManager.Instance.NewGame(
                    PopBalloons.Utilities.GameManager.GameType.FREEPLAY, 
                    0
                );
            }
        }

        private void OnSessionCompleted(SessionData session)
        {
            Debug.Log($"🎉 Séance terminée: {session.gameDisplayName} - Score: {session.score}");
            
            // Animation de célébration ou notification
            // TODO: Afficher une popup de félicitations
        }

        private void OnHelpClicked()
        {
            Debug.Log("❓ Aide demandée");
            // TODO: Afficher un panneau d'aide
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Obtenir la couleur selon le type d'exercice
        /// </summary>
        private Color GetExerciseTypeColor(string exerciseType)
        {
            switch (exerciseType.ToLower())
            {
                case "motricité":
                    return new Color(0.29f, 0.56f, 0.89f); // Bleu #4A90E2
                case "attention":
                    return new Color(0.49f, 0.83f, 0.13f); // Vert #7ED321
                case "coordination":
                    return new Color(0.96f, 0.65f, 0.14f); // Orange #F5A623
                case "mémoire":
                    return new Color(0.61f, 0.35f, 0.71f); // Violet #9C59B6
                default:
                    return Color.gray;
            }
        }
        #endregion
    }
}
