using UnityEngine;

namespace PopBalloons.DebugTools
{
    /// <summary>
    /// Debug component to display current GameModeSelector values in the inspector
    /// Add this to any GameObject to monitor current selection
    /// </summary>
    public class DisplayGameModeSelector : MonoBehaviour
    {
        [Header("Current Global Selection (Read-Only)")]
        [SerializeField] private string currentGameType = "Not initialized";
        [SerializeField] private int currentLevelNumber = -1;

        [Header("Auto-Update")]
        [SerializeField] private bool updateEveryFrame = true;
        [SerializeField] private float updateInterval = 0.5f;
        
        private float lastUpdateTime;

        private void Update()
        {
            if (!updateEveryFrame)
            {
                if (Time.time - lastUpdateTime < updateInterval) return;
                lastUpdateTime = Time.time;
            }

            if (PopBalloons.UI.GameModeSelector.Instance != null)
            {
                currentGameType = PopBalloons.UI.GameModeSelector.Instance.CurrentGameType.ToString();
                currentLevelNumber = PopBalloons.UI.GameModeSelector.Instance.CurrentLevelNumber;
            }
            else
            {
                currentGameType = "GameModeSelector not found";
                currentLevelNumber = -1;
            }
        }

        [ContextMenu("Print Current Values")]
        public void PrintCurrentValues()
        {
            if (PopBalloons.UI.GameModeSelector.Instance != null)
            {
                UnityEngine.Debug.Log($"========== GameModeSelector Values ==========");
                UnityEngine.Debug.Log($"Current Game Type: {PopBalloons.UI.GameModeSelector.Instance.CurrentGameType}");
                UnityEngine.Debug.Log($"Current Level Number: {PopBalloons.UI.GameModeSelector.Instance.CurrentLevelNumber}");
                UnityEngine.Debug.Log($"===========================================");
            }
            else
            {
                UnityEngine.Debug.LogError("GameModeSelector.Instance is null!");
            }
        }

        [ContextMenu("Reset Values")]
        public void ResetValues()
        {
            if (PopBalloons.UI.GameModeSelector.Instance != null)
            {
                PopBalloons.UI.GameModeSelector.Instance.Reset();
                UnityEngine.Debug.Log("GameModeSelector values reset to defaults");
            }
        }
    }
}
