using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TNDPlatform.Data
{
    /// <summary>
    /// Gestionnaire des données de la plateforme
    /// Gère les sessions, progression, aptitudes et récompenses
    /// </summary>
    public class DataManager : MonoBehaviour
    {
        #region Singleton
        private static DataManager instance;
        public static DataManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<DataManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("DataManager");
                        instance = go.AddComponent<DataManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        #endregion

        #region Events
        public static event Action<SessionData> OnSessionCompleted;
        public static event Action<RewardData> OnRewardUnlocked;
        public static event Action OnDataUpdated;
        #endregion

        #region Variables
        private List<SessionData> todaySessions = new List<SessionData>();
        private List<SessionData> allSessions = new List<SessionData>();
        private List<AptitudeData> aptitudes = new List<AptitudeData>();
        private List<RewardData> rewards = new List<RewardData>();
        private WeeklyProgress currentWeekProgress;
        #endregion

        #region Properties
        public List<SessionData> TodaySessions => todaySessions;
        public List<SessionData> AllSessions => allSessions;
        public List<AptitudeData> Aptitudes => aptitudes;
        public List<RewardData> Rewards => rewards;
        public WeeklyProgress CurrentWeekProgress => currentWeekProgress;

        public int TotalSessionsThisWeek => currentWeekProgress?.totalSessions ?? 0;
        public int CompletedSessionsThisWeek => currentWeekProgress?.completedSessions ?? 0;
        public float WeeklyCompletionRate => currentWeekProgress?.CompletionRate ?? 0f;
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

            Debug.Log("📊 DataManager initialized");
        }

        private void Start()
        {
            // Générer des données fictives pour la démo
            GenerateMockData();
        }
        #endregion

        #region Mock Data Generation
        /// <summary>
        /// Générer des données fictives pour Marie
        /// </summary>
        public void GenerateMockData()
        {
            Debug.Log("🎲 Génération des données fictives...");

            // Progression hebdomadaire
            GenerateMockWeeklyProgress();

            // Aptitudes
            GenerateMockAptitudes();

            // Séances d'aujourd'hui
            GenerateMockTodaySessions();

            // Historique des séances
            GenerateMockSessionHistory();

            // Récompenses
            GenerateMockRewards();

            Debug.Log($"✅ Données fictives créées: {todaySessions.Count} séances aujourd'hui, {aptitudes.Count} aptitudes, {rewards.Count} récompenses");
        }

        private void GenerateMockWeeklyProgress()
        {
            // Début de la semaine (lundi dernier)
            DateTime today = DateTime.Now;
            int daysFromMonday = ((int)today.DayOfWeek - 1 + 7) % 7;
            DateTime monday = today.AddDays(-daysFromMonday).Date;

            currentWeekProgress = new WeeklyProgress(monday);
            currentWeekProgress.totalSessions = 15;
            currentWeekProgress.completedSessions = 3;

            // Marquer lundi, mardi, mercredi comme complétés
            currentWeekProgress.MarkDayCompleted(DayOfWeek.Monday);
            currentWeekProgress.MarkDayCompleted(DayOfWeek.Tuesday);
            currentWeekProgress.MarkDayCompleted(DayOfWeek.Wednesday);
        }

        private void GenerateMockAptitudes()
        {
            aptitudes.Clear();

            aptitudes.Add(new AptitudeData("Motricité fine", "🎯", 80f) { previousValue = 75f });
            aptitudes.Add(new AptitudeData("Attention", "🧠", 60f) { previousValue = 48f });
            aptitudes.Add(new AptitudeData("Coordination", "🎨", 70f) { previousValue = 70f });
            aptitudes.Add(new AptitudeData("Temps de réaction", "⚡", 50f) { previousValue = 42f });
            aptitudes.Add(new AptitudeData("Résolution", "🧩", 75f) { previousValue = 72f });
        }

        private void GenerateMockTodaySessions()
        {
            todaySessions.Clear();

            DateTime now = DateTime.Now;
            DateTime morning = new DateTime(now.Year, now.Month, now.Day, 9, 0, 0);
            DateTime afternoon = new DateTime(now.Year, now.Month, now.Day, 14, 0, 0);
            DateTime evening = new DateTime(now.Year, now.Month, now.Day, 17, 0, 0);

            // Séance du matin (terminée)
            var morningSession = new SessionData("PopBalloons", "PopBalloons", "Motricité", morning);
            morningSession.Complete(450, 8.5f);
            todaySessions.Add(morningSession);

            // Séance de l'après-midi (à faire)
            var afternoonSession = new SessionData("PopBalloons", "PopBalloons", "Motricité", afternoon);
            afternoonSession.status = SessionStatus.Pending;
            todaySessions.Add(afternoonSession);

            // Séance du soir (à faire)
            var eveningSession = new SessionData("AttentionGame", "Jeu Cognitif", "Attention", evening);
            eveningSession.status = SessionStatus.Pending;
            todaySessions.Add(eveningSession);
        }

        private void GenerateMockSessionHistory()
        {
            allSessions.Clear();
            allSessions.AddRange(todaySessions);

            // Ajouter des séances des jours précédents
            DateTime today = DateTime.Now.Date;

            for (int i = 1; i <= 7; i++)
            {
                DateTime day = today.AddDays(-i);
                
                if (i <= 3) // Lundi, Mardi, Mercredi complétés
                {
                    var session1 = new SessionData("PopBalloons", "PopBalloons", "Motricité", day.AddHours(9));
                    session1.Complete(400 + i * 10, 7.0f + i * 0.5f);
                    allSessions.Add(session1);

                    var session2 = new SessionData("AttentionGame", "Jeu Cognitif", "Attention", day.AddHours(14));
                    session2.Complete(350 + i * 15, 6.0f + i * 0.3f);
                    allSessions.Add(session2);
                }
            }
        }

        private void GenerateMockRewards()
        {
            rewards.Clear();

            var reward1 = new RewardData("Première Séance", "Terminé ta première séance", RewardData.RewardType.Bronze);
            reward1.Unlock();
            rewards.Add(reward1);

            var reward2 = new RewardData("Une Semaine", "7 jours consécutifs", RewardData.RewardType.Silver);
            rewards.Add(reward2);

            var reward3 = new RewardData("Champion", "Score de 500 points", RewardData.RewardType.Gold);
            rewards.Add(reward3);

            var reward4 = new RewardData("Progrès Remarquable", "+20% dans toutes les aptitudes", RewardData.RewardType.Special);
            rewards.Add(reward4);

            Debug.Log($"🏆 {rewards.Count} récompenses créées ({rewards.Count(r => r.isUnlocked)} débloquées)");
        }
        #endregion

        #region Session Management
        /// <summary>
        /// Obtenir la prochaine séance à faire
        /// </summary>
        public SessionData GetNextSession()
        {
            return todaySessions.FirstOrDefault(s => s.status == SessionStatus.Pending);
        }

        /// <summary>
        /// Compléter une séance
        /// </summary>
        public void CompleteSession(string sessionId, int score, float duration)
        {
            var session = todaySessions.FirstOrDefault(s => s.sessionId == sessionId);
            if (session != null)
            {
                session.Complete(score, duration);
                
                // Mettre à jour la progression hebdomadaire
                currentWeekProgress.completedSessions++;
                currentWeekProgress.MarkDayCompleted(DateTime.Now.DayOfWeek);

                Debug.Log($"✅ Séance terminée: {session.gameDisplayName} - Score: {score}");
                
                OnSessionCompleted?.Invoke(session);
                OnDataUpdated?.Invoke();

                // Vérifier les récompenses
                CheckForRewards();
            }
        }

        /// <summary>
        /// Démarrer une séance
        /// </summary>
        public void StartSession(string sessionId)
        {
            var session = todaySessions.FirstOrDefault(s => s.sessionId == sessionId);
            if (session != null)
            {
                session.status = SessionStatus.InProgress;
                Debug.Log($"▶️ Séance démarrée: {session.gameDisplayName}");
                OnDataUpdated?.Invoke();
            }
        }
        #endregion

        #region Aptitudes Management
        /// <summary>
        /// Mettre à jour une aptitude
        /// </summary>
        public void UpdateAptitude(string aptitudeName, float newValue)
        {
            var aptitude = aptitudes.FirstOrDefault(a => a.name == aptitudeName);
            if (aptitude != null)
            {
                aptitude.UpdateValue(newValue);
                Debug.Log($"📊 Aptitude mise à jour: {aptitudeName} = {newValue:F1}% ({aptitude.GetEvolutionText()})");
                OnDataUpdated?.Invoke();
            }
        }

        /// <summary>
        /// Obtenir une aptitude par nom
        /// </summary>
        public AptitudeData GetAptitude(string name)
        {
            return aptitudes.FirstOrDefault(a => a.name == name);
        }
        #endregion

        #region Rewards Management
        /// <summary>
        /// Vérifier si de nouvelles récompenses doivent être débloquées
        /// </summary>
        private void CheckForRewards()
        {
            foreach (var reward in rewards.Where(r => !r.isUnlocked))
            {
                bool shouldUnlock = false;

                switch (reward.name)
                {
                    case "Une Semaine":
                        shouldUnlock = CompletedSessionsThisWeek >= 7;
                        break;
                    case "Champion":
                        shouldUnlock = allSessions.Any(s => s.score >= 500);
                        break;
                    case "Progrès Remarquable":
                        shouldUnlock = aptitudes.All(a => a.Evolution >= 20);
                        break;
                }

                if (shouldUnlock)
                {
                    UnlockReward(reward.rewardId);
                }
            }
        }

        /// <summary>
        /// Débloquer une récompense
        /// </summary>
        public void UnlockReward(string rewardId)
        {
            var reward = rewards.FirstOrDefault(r => r.rewardId == rewardId);
            if (reward != null && !reward.isUnlocked)
            {
                reward.Unlock();
                Debug.Log($"🎉 Récompense débloquée: {reward.name}");
                OnRewardUnlocked?.Invoke(reward);
                OnDataUpdated?.Invoke();
            }
        }

        /// <summary>
        /// Obtenir le nombre de récompenses débloquées
        /// </summary>
        public int GetUnlockedRewardsCount()
        {
            return rewards.Count(r => r.isUnlocked);
        }
        #endregion

        #region Statistics
        /// <summary>
        /// Obtenir les statistiques globales
        /// </summary>
        public Dictionary<string, object> GetGlobalStats()
        {
            return new Dictionary<string, object>
            {
                { "TotalSessions", allSessions.Count(s => s.status == SessionStatus.Completed) },
                { "TotalPoints", allSessions.Where(s => s.status == SessionStatus.Completed).Sum(s => s.score) },
                { "AverageScore", allSessions.Any() ? allSessions.Where(s => s.status == SessionStatus.Completed).Average(s => s.score) : 0 },
                { "WeeklyCompletion", WeeklyCompletionRate },
                { "UnlockedRewards", GetUnlockedRewardsCount() },
                { "CurrentStreak", CalculateStreak() }
            };
        }

        /// <summary>
        /// Calculer la série de jours consécutifs
        /// </summary>
        private int CalculateStreak()
        {
            int streak = 0;
            DateTime checkDate = DateTime.Now.Date;

            while (true)
            {
                bool hasSession = allSessions.Any(s => 
                    s.status == SessionStatus.Completed && 
                    s.completedTime.HasValue && 
                    s.completedTime.Value.Date == checkDate);

                if (!hasSession)
                    break;

                streak++;
                checkDate = checkDate.AddDays(-1);

                // Limiter à 30 jours pour éviter une boucle infinie
                if (streak > 30)
                    break;
            }

            return streak;
        }
        #endregion
    }
}
