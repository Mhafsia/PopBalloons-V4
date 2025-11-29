using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PopBalloons.Utilities;

namespace PopBalloons.UI
{
    public enum FreePlaySubState { SETUP, INGAME, ENDGAME };

    /// <summary>
    /// Manages UI states for FreePlay mode (infinite balloon spawning without levels or tutorials).
    /// </summary>
    public class FreePlayPanel : SubPanel<FreePlaySubState>
    {
        #region Variables
        private static FreePlayPanel instance;

        /// <summary>
        /// Singleton instance accessor
        /// </summary>
        public static FreePlayPanel Instance { get => instance; }
        #endregion Variables

        #region Unity Functions
        /// <summary>
        /// Singleton's implementation.
        /// </summary>
        private void Awake()
        {
            if (instance != null)
            {
                DestroyImmediate(this.gameObject);
                return;
            }
            
            instance = this;
        }

        /// <summary>
        /// Initialization
        /// </summary>
        private void Start()
        {
            this.SetState(FreePlaySubState.SETUP);

            // Check FreePlayPanel GameObject state
            CanvasGroup cg = GetComponent<CanvasGroup>();

            GameManager.OnGameStateChanged += HandleStateChanged;
            GameCreator.OnGameStarted += (type) => { if (type == GameManager.GameType.FREEPLAY) this.HandleNewGame(); };
            GameCreator.OnGameEnded += (type) => { if (type == GameManager.GameType.FREEPLAY) this.HandleGameEnded(); };
            GameCreator.OnGameInterrupted += (type) => { if (type == GameManager.GameType.FREEPLAY) this.HandleGameInterrupted(); };
        }

        private void OnDestroy()
        {
            GameManager.OnGameStateChanged -= HandleStateChanged;
        }
        #endregion Unity Functions

        #region Functions

        /// <summary>
        /// Exit FreePlay and return to menu
        /// </summary>
        public void ExitLevel()
        {
            GameCreator.Instance.QuitLevel();
            GameManager.Instance.Home();
        }

        private void HandleStateChanged(GameManager.GameState newState)
        {
            switch (newState)
            {
                case GameManager.GameState.INIT:
                case GameManager.GameState.SETUP:
                case GameManager.GameState.HOME:
                    // Return to setup state and hide InGame panel
                    this.SetState(FreePlaySubState.SETUP);
                    HideInGamePanel();
                    break;
                case GameManager.GameState.PLAY:
                    if (GameManager.Instance.CurrentGameType == GameManager.GameType.FREEPLAY)
                    {
                        // Start FreePlay
                        this.HandleNewGame();
                    }
                    break;
            }
        }

        private void HandleNewGame()
        {
            if (GameManager.Instance.CurrentGameType == GameManager.GameType.NONE || GameManager.Instance.CurrentState != GameManager.GameState.PLAY)
            {
                this.SetState(FreePlaySubState.SETUP);
            }
            else
            {
                // FreePlay has no tutorial, go directly to in-game
                this.SetState(FreePlaySubState.INGAME);
            }
        }
        
        private void HideInGamePanel()
        {
            Transform inGameTransform = transform.Find("Game");
            if (inGameTransform != null)
            {
                CanvasGroup inGameCanvas = inGameTransform.GetComponent<CanvasGroup>();
                if (inGameCanvas != null)
                {
                    inGameCanvas.alpha = 0f;
                    inGameCanvas.interactable = false;
                    inGameCanvas.blocksRaycasts = false;
                }
            }
        }

        private void HandleGameInterrupted()
        {
            this.SetState(FreePlaySubState.SETUP);
        }

        private void HandleGameEnded()
        {
            // FreePlay ends when user quits manually (no automatic end)
            this.SetState(FreePlaySubState.ENDGAME);
        }

        public override void Init()
        {
            this.SetState(FreePlaySubState.SETUP);
        }

        #endregion
    }
}
