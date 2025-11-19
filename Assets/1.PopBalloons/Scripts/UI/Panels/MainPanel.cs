using Microsoft.MixedReality.Toolkit.UI;
using PopBalloons.Boundaries;
using PopBalloons.Configuration;
using PopBalloons.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PopBalloons.UI
{
    public enum MainPanelState { PROFILE, MODE_PICK, COGNITIVE, MOBILITY, FREEPLAY }

    /// <summary>
    /// Main panel of the application. Handle the higher state of the app interface
    /// </summary>
    public class MainPanel : SubPanel<MainPanelState>
    {
        #region Variables

        private static MainPanel instance;

        [SerializeField]
        private AnimationSettings settings;

        private bool animationToggler = false;
        private Billboard billboard;

        /// <summary>
        /// Singleton instance accessor
        /// </summary>
        public static MainPanel Instance { get => instance; }
        #endregion Variables


        #region Unity Functions
        /// <summary>
        /// Singleton's implementation.
        /// </summary>
        private void Awake()
        {
            if(instance != null)
            {
                Debug.LogError("Should'nt have two instances of MainPanel.");
                DestroyImmediate(this.gameObject);
            }
            else
            {
                instance = this;
            }
        }


        /// <summary>
        /// Initialization
        /// </summary>
        private void Start()
        {
            billboard = this.GetComponent<Billboard>();
            Invoke("Initialize", 0.1f);
            GameManager.OnGameStateChanged += HandleGameStateChanged;
        }

        private void Initialize()
        {
            this.SetState(MainPanelState.PROFILE);
        }

        private void OnDestroy()
        {
            GameManager.OnGameStateChanged -= HandleGameStateChanged;
        }
        #endregion Unity Functions    

        #region Functions
        /// <summary>
        /// The objective is to move the panel around the user when needed and to put it aside otherwise
        /// </summary>
        /// <param name="newState"></param>
        private void HandleGameStateChanged(GameManager.GameState newState)
        {
            switch (newState)
            {
                case GameManager.GameState.INIT:
                case GameManager.GameState.SETUP:
                    break;
                case GameManager.GameState.HOME:
                    // Always show MainPanel when returning home
                    this.gameObject.SetActive(true);
                    // Return to mode selection screen
                    this.SetState(MainPanelState.MODE_PICK);
                    //Place in front of user (~2 meters)
                    Vector3 target = Camera.main.transform.position + Camera.main.transform.forward*2.0f;
                    target.y = this.transform.position.y;
                    StartCoroutine(MovePanel(target, 2f * target - Camera.main.transform.position));
                    billboard.enabled = true;
                    break;
                case GameManager.GameState.PLAY:
                    // Keep panel in place and maintain current state during gameplay
                    // Don't move or change panel state - let it stay where it is
                    billboard.enabled = true; // Keep billboard enabled so it faces the player
                    break;
            }
        }

        /// <summary>
        /// We prevent trying to move the panel if the user is holding it.
        /// </summary>
        /// <param name="data"></param>
        public void OnBeginDrag(Microsoft.MixedReality.Toolkit.UI.ManipulationEventData data)
        {
            Debug.Log("Manipulation started, coroutine stopped");
            this.StopAllCoroutines();
        }

        public override void Init()
        {
            this.SetState(MainPanelState.PROFILE);
        }
        #endregion

        #region Coroutine
        private IEnumerator MovePanel(Vector3 target,Vector3 facingObject)
        {
            animationToggler = !animationToggler;
            bool b = animationToggler;
            float t = 0;
            this.transform.LookAt(facingObject);
            Vector3 initialPosition = this.transform.position;
            while (t < settings.Duration)
            {
                if (b != animationToggler)
                    yield break;
                this.transform.LookAt(facingObject);
                this.transform.position = Vector3.Lerp(initialPosition, target, settings.Curve.Evaluate(t / settings.Duration));
                yield return null;
                t += Time.deltaTime;
            }
            this.transform.position = target;
            this.transform.LookAt(facingObject);
        }

        #endregion
    }

}