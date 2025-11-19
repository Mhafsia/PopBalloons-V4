using System;
using UnityEngine;

namespace TNDPlatform.Data
{
    /// <summary>
    /// Statut d'une séance d'exercice
    /// </summary>
    public enum SessionStatus
    {
        Pending,      // ○ À faire
        InProgress,   // ⏳ En cours
        Completed,    // ✓ Terminée
        Skipped       // ⊗ Passée
    }

    /// <summary>
    /// Données d'une séance d'exercice
    /// </summary>
    [System.Serializable]
    public class SessionData
    {
        public string sessionId;
        public string gameName;
        public string gameDisplayName;
        public string exerciseType;  // "Motricité", "Attention", "Coordination", etc.
        public DateTime scheduledTime;
        public DateTime? completedTime;
        public SessionStatus status;
        public int score;
        public float duration;  // En minutes
        public string iconName; // Pour l'icône du jeu

        // Properties pour compatibilité avec SessionItem.cs
        public SessionStatus Status
        {
            get => status;
            set => status = value;
        }

        public DateTime? CompletedTime
        {
            get => completedTime;
            set => completedTime = value;
        }

        public DateTime ScheduledTime
        {
            get => scheduledTime;
            set => scheduledTime = value;
        }

        public string GameName
        {
            get => gameDisplayName;
            set => gameDisplayName = value;
        }

        public int? Score
        {
            get => score;
            set => score = value ?? 0;
        }

        public float Duration
        {
            get => duration;
            set => duration = value;
        }

        public SessionData(string gameName, string displayName, string exerciseType, DateTime scheduledTime)
        {
            this.sessionId = Guid.NewGuid().ToString();
            this.gameName = gameName;
            this.gameDisplayName = displayName;
            this.exerciseType = exerciseType;
            this.scheduledTime = scheduledTime;
            this.status = SessionStatus.Pending;
            this.score = 0;
            this.duration = 0f;
        }

        /// <summary>
        /// Marquer la séance comme terminée
        /// </summary>
        public void Complete(int finalScore, float durationMinutes)
        {
            status = SessionStatus.Completed;
            completedTime = DateTime.Now;
            score = finalScore;
            duration = durationMinutes;
        }

        /// <summary>
        /// Obtenir l'icône de statut
        /// </summary>
        public string GetStatusIcon()
        {
            switch (status)
            {
                case SessionStatus.Pending:
                    return "○";
                case SessionStatus.InProgress:
                    return "⏳";
                case SessionStatus.Completed:
                    return "✓";
                case SessionStatus.Skipped:
                    return "⊗";
                default:
                    return "?";
            }
        }

        /// <summary>
        /// Obtenir la couleur de statut
        /// </summary>
        public Color GetStatusColor()
        {
            switch (status)
            {
                case SessionStatus.Pending:
                    return new Color(0.7f, 0.7f, 0.7f); // Gris
                case SessionStatus.InProgress:
                    return new Color(1f, 0.65f, 0f); // Orange
                case SessionStatus.Completed:
                    return new Color(0.16f, 0.65f, 0.27f); // Vert #28A745
                case SessionStatus.Skipped:
                    return new Color(0.8f, 0.2f, 0.2f); // Rouge
                default:
                    return Color.gray;
            }
        }
    }

    /// <summary>
    /// Progression hebdomadaire
    /// </summary>
    [System.Serializable]
    public class WeeklyProgress
    {
        public DateTime weekStart;
        public int totalSessions;
        public int completedSessions;
        public bool[] dailyCompletion = new bool[7]; // Lundi = 0, Dimanche = 6

        public float CompletionRate => totalSessions > 0 ? (float)completedSessions / totalSessions : 0f;

        public WeeklyProgress(DateTime weekStart)
        {
            this.weekStart = weekStart;
            this.totalSessions = 0;
            this.completedSessions = 0;
        }

        /// <summary>
        /// Marquer un jour comme complété
        /// </summary>
        public void MarkDayCompleted(DayOfWeek day)
        {
            int dayIndex = (int)day;
            if (dayIndex >= 0 && dayIndex < 7)
            {
                dailyCompletion[dayIndex] = true;
            }
        }

        /// <summary>
        /// Obtenir le symbole pour un jour
        /// </summary>
        public string GetDaySymbol(DayOfWeek day)
        {
            int dayIndex = (int)day;
            if (dayIndex >= 0 && dayIndex < 7)
            {
                return dailyCompletion[dayIndex] ? "✓" : "•";
            }
            return "";
        }
    }

    /// <summary>
    /// Aptitude mesurée par les jeux
    /// </summary>
    [System.Serializable]
    public class AptitudeData
    {
        public string name;           // "Motricité fine", "Attention", etc.
        public string icon;           // Emoji ou nom d'icône
        public float currentValue;    // 0-100
        public float previousValue;   // Pour calculer l'évolution
        public float targetValue;     // Objectif

        public float Progress => currentValue / 100f;
        public float Evolution => currentValue - previousValue;
        public bool IsImproving => Evolution > 0;

        public AptitudeData(string name, string icon, float currentValue)
        {
            this.name = name;
            this.icon = icon;
            this.currentValue = currentValue;
            this.previousValue = currentValue;
            this.targetValue = 100f;
        }

        /// <summary>
        /// Mettre à jour la valeur
        /// </summary>
        public void UpdateValue(float newValue)
        {
            previousValue = currentValue;
            currentValue = Mathf.Clamp(newValue, 0f, 100f);
        }

        /// <summary>
        /// Obtenir le texte d'évolution
        /// </summary>
        public string GetEvolutionText()
        {
            if (Mathf.Abs(Evolution) < 0.1f)
                return "→ stable";
            
            return Evolution > 0 ? $"↗️ +{Evolution:F0}%" : $"↘️ {Evolution:F0}%";
        }
    }

    /// <summary>
    /// Badge ou récompense
    /// </summary>
    [System.Serializable]
    public class RewardData
    {
        public string rewardId;
        public string name;
        public string description;
        public string iconName;
        public DateTime unlockedDate;
        public bool isUnlocked;
        public RewardType type;

        public enum RewardType
        {
            Bronze,
            Silver,
            Gold,
            Special
        }

        public RewardData(string name, string description, RewardType type)
        {
            this.rewardId = Guid.NewGuid().ToString();
            this.name = name;
            this.description = description;
            this.type = type;
            this.isUnlocked = false;
        }

        public void Unlock()
        {
            isUnlocked = true;
            unlockedDate = DateTime.Now;
        }

        public Color GetTypeColor()
        {
            switch (type)
            {
                case RewardType.Bronze:
                    return new Color(0.8f, 0.5f, 0.2f); // Bronze
                case RewardType.Silver:
                    return new Color(0.75f, 0.75f, 0.75f); // Argent
                case RewardType.Gold:
                    return new Color(1f, 0.84f, 0f); // Or
                case RewardType.Special:
                    return new Color(0.58f, 0.26f, 0.76f); // Violet
                default:
                    return Color.gray;
            }
        }
    }
}
