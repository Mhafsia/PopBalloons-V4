using UnityEngine;
using PopBalloons.Utilities;

namespace PopBalloons.UI
{
    /// <summary>
    /// Singleton that stores globally selected game mode and level
    /// Updated by ModeButton and LevelSelectionButton, read by LoadLevelButton
    /// </summary>
    public class GameModeSelector : MonoBehaviour
    {
        private static GameModeSelector _instance;
        public static GameModeSelector Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameModeSelector>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameModeSelector");
                        _instance = go.AddComponent<GameModeSelector>();
                    }
                }
                return _instance;
            }
        }

        [Header("Current Selection")]
        [SerializeField] private GameManager.GameType _currentGameType = GameManager.GameType.COGNITIVE;
        [SerializeField] private int _currentLevelNumber = 0;

        public GameManager.GameType CurrentGameType
        {
            get => _currentGameType;
            set
            {
                _currentGameType = value;
                Debug.Log($"[GameModeSelector] Mode mis à jour: {value}");
            }
        }

        public int CurrentLevelNumber
        {
            get => _currentLevelNumber;
            set
            {
                _currentLevelNumber = value;
                Debug.Log($"[GameModeSelector] Niveau mis à jour: {value}");
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Reset to default values
        /// </summary>
        public void Reset()
        {
            CurrentGameType = GameManager.GameType.COGNITIVE;
            CurrentLevelNumber = 0;
            Debug.Log("[GameModeSelector] Reset to default values");
        }
    }
}
