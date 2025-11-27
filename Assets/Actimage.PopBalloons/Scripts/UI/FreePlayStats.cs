using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PopBalloons.Utilities;

namespace PopBalloons.UI
{
    /// <summary>
    /// Tracks and displays FreePlay statistics: time played, balloons popped, current score.
    /// Attach to a UI Text/TMP object to auto-update the display.
    /// </summary>
    public class FreePlayStats : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField]
        private TMP_Text statsText;

        [SerializeField]
        private bool showTime = true;

        [SerializeField]
        private bool showBalloonsPopped = true;

        [SerializeField]
        private bool showScore = false; // Pas de score en mode FREEPLAY

        private float sessionStartTime;
        private int balloonsPopped;
        private bool isPlaying;

        private void Start()
        {
            if (statsText == null)
            {
                statsText = GetComponent<TMP_Text>();
            }

            GameCreator.OnGameStarted += HandleGameStarted;
            GameCreator.OnGameEnded += HandleGameEnded;
            GameCreator.OnGameInterrupted += HandleGameEnded;
        }

        private void OnDestroy()
        {
            GameCreator.OnGameStarted -= HandleGameStarted;
            GameCreator.OnGameEnded -= HandleGameEnded;
            GameCreator.OnGameInterrupted -= HandleGameEnded;
        }

        private void HandleGameStarted(GameManager.GameType type)
        {
            if (type == GameManager.GameType.FREEPLAY)
            {
                sessionStartTime = Time.time;
                balloonsPopped = 0;
                isPlaying = true;
            }
        }

        private void HandleGameEnded(GameManager.GameType type)
        {
            if (type == GameManager.GameType.FREEPLAY)
            {
                isPlaying = false;
            }
        }

        private float lastUpdateTime = 0f;
        private const float UPDATE_INTERVAL = 0.5f; // Update stats twice per second instead of 60 times

        private void Update()
        {
            if (isPlaying && GameManager.Instance != null && GameManager.Instance.CurrentGameType == GameManager.GameType.FREEPLAY)
            {
                // Only update display every 0.5 seconds to improve performance
                if (Time.time - lastUpdateTime >= UPDATE_INTERVAL)
                {
                    UpdateDisplay();
                    lastUpdateTime = Time.time;
                }
            }
        }

        private void UpdateDisplay()
        {
            if (statsText == null) return;

            string display = "";

            if (showTime)
            {
                float elapsedTime = Time.time - sessionStartTime;
                int minutes = Mathf.FloorToInt(elapsedTime / 60f);
                int seconds = Mathf.FloorToInt(elapsedTime % 60f);
                display += $"Temps: {minutes:00}:{seconds:00}\n";
            }

            if (showBalloonsPopped && GameCreator.Instance != null)
            {
                display += $"Ballons: {GameCreator.Instance.BalloonDestroyed:F0}\n";
            }

            if (showScore && ScoreManager.instance != null)
            {
                display += $"Score: {ScoreManager.instance.score}";
            }

            statsText.text = display.TrimEnd();
        }
    }
}
