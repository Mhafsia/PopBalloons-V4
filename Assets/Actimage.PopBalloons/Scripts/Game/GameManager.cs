using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.EventSystems;
using PopBalloons.Configuration;
using PopBalloons.Data;
using TMPro;
using UnityEngine.UI;

namespace PopBalloons.Utilities
{
    /// <summary>
    /// Handle the game status, scene initialisation, and the level management
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Variables
        public TMP_Text EndOfLevelMessage;
        public TMP_Text EndOfLevelMessageMotor;
        public static GameManager Instance;
        public TMP_Text NextMotor;
        public TMP_Text NextCognitive;
        
        public enum GameState {INIT,SETUP,HOME,PLAY}
        public enum GameType {NONE,MOBILITY,COGNITIVE,FREEPLAY}


        private GameState currentState;
        private int currentLevelIndex = 0; //0 means tutorial
        private GameType currentGameType;
 
        [SerializeField]
        private List<LevelInfo> AvailableLevels;

        /// <summary>
        /// Instance of JulieManager
        /// </summary>
        private JulieManager julie;

        //Accessors
        public GameType CurrentGameType { get => currentGameType; }
        public int CurrentLevelIndex { get => currentLevelIndex;  }
        public GameState CurrentState { get => currentState;}
        public int MaxLevelCount
        {
            get
            {
                switch (CurrentGameType)
                {
                    case GameType.MOBILITY:
                        return 4;
                    case GameType.COGNITIVE:
                        return 7;
                    case GameType.FREEPLAY:
                        return 1;
                    case GameType.NONE:
                        return 0;
                    default:
                        return 0;
                };
            }
        }
        #endregion

        #region Events
        public delegate void GameStateChange(GameState newState);
        public static event GameStateChange OnGameStateChanged;

        #endregion

        #region Unity Functions

        /// <summary>
        /// Singleton execution
        /// </summary>
        private void Awake()
        {
            if(Instance != null)
            {
                DestroyImmediate(this);
            }
            else
            {
                DontDestroyOnLoad(this);
                Instance = this;
            }
        }

        private void Start()
        {
            ScoreBoard.OnNextLevel += NextLevel;
        }

        private void OnDestroy()
        {
            ScoreBoard.OnNextLevel -= NextLevel;
        }

        private void Update()
        {
            EndOfLevelMessage.text = $"Bravo ! Tu as r�ussi le niveau  {currentLevelIndex.ToString()} !";
            EndOfLevelMessageMotor.text =  EndOfLevelMessage.text;
            //Switch that will handle the 
            switch (currentState)
            {
                case GameState.INIT:
                    //LoadScene, then wait for setup  
                    UnloadAllScenes();
                    //SceneManager.LoadScene("PlaySpace", LoadSceneMode.Additive);
                    this.currentState = GameState.SETUP;
                    julie = this.GetComponent<JulieManager>();
                    break;
                case GameState.SETUP:
                    //Wait for area to be set
                    break;
                default:
                    //Do nothing
                    break;
            }

        }

        #endregion

        #region Functions 

        /// <summary>
        /// Create a new game and set the Game Settings
        /// </summary>
        /// <param name="type"></param>
        /// <param name="levelIndex"></param>
        public void NewGame(GameType type, int levelIndex)
        {// Synchronize GameModeSelector
            if (PopBalloons.UI.GameModeSelector.Instance != null)
            {
                PopBalloons.UI.GameModeSelector.Instance.CurrentGameType = type;
                PopBalloons.UI.GameModeSelector.Instance.CurrentLevelNumber = levelIndex;
            }
            
            // Validation: ensure GameCreator exists
            if (GameCreator.Instance == null)
            {
                this.currentState = GameState.HOME;  // Reset state to prevent further issues
                return;
            }
            
            if(currentState == GameState.HOME)
            {
                UnloadAllScenes(); 
            }

            if(currentState == GameState.PLAY)
            {
                // Save current level score before quitting
                Save();
                TimerManager.LevelEnd();
                GameCreator.Instance.QuitLevel();
                //TODO: Manage level quitting?
            }

            this.currentLevelIndex = levelIndex;
            this.currentGameType = type;
            this.currentState = GameState.PLAY;
            ScoreManager.initScore();
            
            // Change MainPanel state based on game type
            switch (type)
            {
                case GameType.MOBILITY:
                    PopBalloons.UI.MainPanel.Instance.SetState(PopBalloons.UI.MainPanelState.MOBILITY);
                    break;
                case GameType.COGNITIVE:
                    PopBalloons.UI.MainPanel.Instance.SetState(PopBalloons.UI.MainPanelState.COGNITIVE);
                    break;
                case GameType.FREEPLAY:
                    PopBalloons.UI.MainPanel.Instance.SetState(PopBalloons.UI.MainPanelState.FREEPLAY);
                    break;
            }
            
            OnGameStateChanged?.Invoke(this.currentState);
            GameCreator.Instance.Play(type);
            var nextButton = (currentLevelIndex == MaxLevelCount)
                ? NextMotor.text = NextCognitive.text = "Fin"
                : NextMotor.text = NextCognitive.text = "Niveau suivant";

        }

        /// <summary>
        /// Will play the next level in the same mode as current.
        /// </summary>
        
        public void NextLevel()
        {
            // Save current level score before moving to next
            if (currentState == GameState.PLAY)
            {
                Save();
            }
            
            if(currentLevelIndex < MaxLevelCount)
            {
                this.NewGame(currentGameType, currentLevelIndex + 1);

            }
            else
            {
                GameManager.Instance.Home();
            }
        }


        /// <summary>
        /// Will display home menu scene or specified state
        /// </summary>
        public void Home(PopBalloons.UI.MainPanelState? nextState = null)
        {
            
            bool wasPlaying = (currentState == GameState.PLAY);
            
            if(currentState == GameState.PLAY)
            {
                // Save current level score before going home
                Save();
                
                // Set game type to NONE first to stop any spawning loops
                this.currentGameType = GameType.NONE;
                
                // FORCE STOP TIMER
                TimerManager.LevelEnd();
                
                GameCreator.Instance.QuitLevel();
            }

            this.currentState = GameState.HOME;
            this.currentGameType = GameType.NONE;  // Ensure it's NONE even if not in PLAY state
            this.currentLevelIndex = 0;
            
            // Update UI state ONLY if explicitly requested
            if (nextState.HasValue && PopBalloons.UI.MainPanel.Instance != null)
            {PopBalloons.UI.MainPanel.Instance.SetState(nextState.Value);
            }
            else
            {}
            
            OnGameStateChanged?.Invoke(this.currentState);
            //We remove unwanted scene because off additing loading
            UnloadAllScenes();
            //SceneManager.LoadScene("Menu", LoadSceneMode.Additive);

        }

        /// <summary>
        /// Will save current session data
        /// </summary>
        public void Save()
        {
            Debug.Log(Application.persistentDataPath);
            if (ProfilesManager.Instance != null)
                ProfilesManager.Instance.Save(CurrentGameType.ToString() + "_" + (currentLevelIndex + 1).ToString(), ScoreManager.instance.score);
            //if (DataManager.instance != null)
            //    DataManager.instance.SaveDatas(CurrentGameType.ToString()+"_"+(currentLevelIndex+1).ToString(), ScoreManager.instance.score);
        }

        
        
        /// <summary>
        /// Unloads all the scenes in the game except for the "Setup" which is the principal scene of the game and the the scene which we have it's name here.
        /// </summary>
        /// <param name="SceneName"></param>
        public void UnloadAllScenes()
        {
            int c = SceneManager.sceneCount;for (int i = 0; i < c; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);if (scene.name != "Setup")
                {SceneManager.UnloadSceneAsync(scene);
                }

            }
        }

        public List<LevelInfo> GetAvailableLevels(GameType type)
        {
            if(AvailableLevels != default)
            {
                return AvailableLevels.FindAll((level) => level.Type == type);
            }
            return new List<LevelInfo>();
        }


        public void SetupCompleted()
        {
         //   julie.Init();
            Home();
        }
        #endregion
    }
}

