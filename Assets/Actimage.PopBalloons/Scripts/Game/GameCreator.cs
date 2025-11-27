using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine.EventSystems;
using System;
using System.Diagnostics;
using System.Linq;
using Random = UnityEngine.Random;
using PopBalloons.Boundaries;
using PopBalloons.Data;
using Debug = UnityEngine.Debug;

namespace PopBalloons.Utilities
{
    /// <summary>
    /// Begining the game and managing it in different levels
    /// </summary>
    public class GameCreator : MonoBehaviour
    {
        /// <summary>
        /// Enum handeling different tutorial state
        /// </summary>
        public enum TutorialState { INITIALISATION, POSITIONNING, WAITING_FOR_DEMONSTRATION, DEMONSTRATION, DIY, FINISH }
        public enum SampleType
        {
            Uniform,
            Gaussian,
            HeadHeight
        }

        [Header("Balloon Height Randomization Min Max (Motor/Classic Only). Height Modifier is always used when Gaussian is selected. " +
                "Height Modifier is not used when Uniform and 'Use Above Below' is on.")] 
        [Header("Single value used for range")]
        
        [Tooltip("When Sample type is set to Gaussian the height of the balloon will be determined using a normal distribution" +
                 " with a mean of the participants head height and a standard deviation of the 'Height Modifier' variable below")]
        public SampleType sampleDistributionType;
        
        [Tooltip("Min and Max height that the balloon is instantiated at when 'Use Above Below is toggled off' ")]
        [Range(0, 3)]
        public float heightModifier = 0.4f; // this can be modified to increase or decrease the min/max height of balloon.
        
        
        [Header("Used for setting different min and max values. Only used when uniform distribution is selected)")]
        [Tooltip("Toggle On to use separate values for min-max toggle off to us height modifier value for both min max")]
        public bool useAboveBelow;
        
        [Tooltip("Determine the max height above the participants head that the balloon will be instantiated at. " +
                 "ONLY USED IF 'Use Above Below' is toggled on. Motor/Classic mode only")]
        [Range(0, 3)]
        public float balloonAbove = 0.4f;
        
        [Tooltip("Determine the min height below the participants head that the balloon will be instantiated at. " +
                 "ONLY USED IF 'Use Above Below' is toggled on. Motor/Classic mode only")]
        [Range(0, 3)]
        public float balloonBelow = 0.4f;
        
        #region variables
        [Header("Classic Game settings: ")]
        
        
        [SerializeField]
        [Range(-1f, 1f)]
        [Tooltip("Offset of the height range (Vector3.UP * this factor)")]
        private float headOffset = -0.25f;
        [SerializeField]
        [Range(0f, 1.5f)]
        private float minDistance = 1f;
        [SerializeField]
        [Range(1,5)]
        private int maxElementsOnScene = 1;
        [Header("Cognitive Game settings: ")]
        [SerializeField]
        [Range(2, 10)]
        [Tooltip("Determine the number of option displayed to the user")]
        private int maxOptions = 5;
        [SerializeField]
        [Range(2, 10)]
        [Tooltip("Determine the number of option displayed to the user")]
        private int minOptions = 2;

        private Camera cam;


        [Header("Object settings: ")]
        [SerializeField]
        private GameObject countdown;

        [SerializeField]
        private List<BalloonBehaviour> balloonsPrefabs;
        [SerializeField]
        private List<BonusBehaviour> bonusBalloonsPrefabs;

    [Header("FreePlay settings:")]
    [SerializeField]
    [Tooltip("Interval in seconds between spawns in FreePlay mode.")]
    private float freePlaySpawnInterval = 1.5f;
    [SerializeField]
    [Tooltip("Maximum number of balloons simultaneously in scene during FreePlay. 0 = unlimited")]
    private int freePlayMaxSimultaneous = 10;
       
        public enum BalloonColor {BLUE,RED,GREEN,YELLOW,NONE}

        private TutorialState currentTutorialState = TutorialState.FINISH;
        public static GameCreator instance;
        private float balloonDestroyed = 0;

        private bool GameIsRunning;
        private int balloonsInScene; //the number of balloons in scene
        private List<BalloonBehaviour> balloons;
        private List<BalloonColor> availableColors;
        private List<BalloonColor> usedColors;
        public BalloonColor intendedColor;
        public Options balloonOptions = new Options();
        private List<Options> temporaryOptionsList = new List<Options>();
        
        /// <summary>
        /// Determine how many balloon there is to be destroy in this lvl
        /// </summary>
        private int maxBalloon;

        public static GameCreator Instance { get => instance; }
        public int BalloonsInScene { get => balloonsInScene; }
        public int MaxBalloon { get => maxBalloon; }
        private bool MoreThanEnough { get => this.BalloonsInScene >= maxElementsOnScene; }
        private bool CorrectBalloonsRemains { get => this.balloons.FindAll(balloon => balloon.GetColor() == IntendedColor).Count > 0; }
        public float BalloonDestroyed { get => balloonDestroyed; set => balloonDestroyed = value; }
        public BalloonColor IntendedColor { get => intendedColor; }


        List<Waves> options = new List<Waves>();

        #endregion

        /// <summary>
        /// Utility coroutine to wait until MRTK InputSystem and FocusProvider are available.
        /// Call this before instantiating prefabs that rely on MRTK cursors/pointers.
        /// </summary>
        private IEnumerator WaitForMRTKReady()
        {
            // Wait until InputSystem and FocusProvider exist and until a primary pointer or a Pointer GameObject exists.
            float timeout = 2.0f; // seconds
            float waited = 0f;
            while (waited < timeout)
            {
                var inputSystem = Microsoft.MixedReality.Toolkit.CoreServices.InputSystem;
                var focusProvider = inputSystem?.FocusProvider;
                var primary = focusProvider?.PrimaryPointer;

                if (inputSystem != null && focusProvider != null && (primary != null || GameObject.Find("Pointer") != null))
                {
                    // MRTK is ready (or a Pointer GameObject exists)
                    yield return null; // one extra frame to let instantiation finish
                    yield break;
                }

                waited += Time.deltaTime;
                yield return null;
            }

            // timeout reached: continue anyway (we've waited enough)
            yield return null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Events
        public delegate void GameStateChange(GameManager.GameType type);
        public static event GameStateChange OnGameStarted;
        public static event GameStateChange OnGameInterrupted;
        public static event GameStateChange OnGameEnded;
        #endregion
        
        #region unity functions

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                // If another instance already exists, destroy this GameObject to avoid leftover components
                Debug.LogWarning("GameCreator: Duplicate instance detected, destroying...");
                Destroy(this.gameObject);
                return;
            }
            
            instance = this;
        }
        private void Start()
        {
            balloons = new List<BalloonBehaviour>();
            BalloonDestroyed = 0;
            balloonsInScene = 0;
            BalloonBehaviour.OnCognitiveBalloonDestroyed += HandleCognitiveBalloonPopped;
            
            // Listen for game state changes to stop FreePlay when returning home
            GameManager.OnGameStateChanged += HandleGameStateChanged;
        }

        /// <summary>
        /// Set FreePlay settings (called from WebSocket)
        /// </summary>
        public void SetFreePlaySettings(float spawnInterval, int maxSimultaneous)
        {
            freePlaySpawnInterval = spawnInterval;
            freePlayMaxSimultaneous = maxSimultaneous;
        }

        private void OnDestroy()
        {
            BalloonBehaviour.OnCognitiveBalloonDestroyed -= HandleCognitiveBalloonPopped;
            GameManager.OnGameStateChanged -= HandleGameStateChanged;
        }
        
        /// <summary>
        /// Handle game state changes - stop spawning when returning to HOME
        /// </summary>
        private void HandleGameStateChanged(GameManager.GameState newState)
        {
            
            if (newState == GameManager.GameState.HOME && GameIsRunning)
            {
                QuitLevel();
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Functions
        
        /// <summary>
        /// Will handle the destruction of a balloon inside a cognitive level
        /// </summary>
        /// <param name="color"></param>
        private void HandleCognitiveBalloonPopped(BalloonBehaviour balloon)
        {
            if (balloons.Contains(balloon))
            {
                // Liste des animations joyeuses et tristes
                List<JulieManager.JulieAnimation> julieHappy = new List<JulieManager.JulieAnimation> { JulieManager.JulieAnimation.Dance, JulieManager.JulieAnimation.Clap };
                List<JulieManager.JulieAnimation> julieNotHappy = new List<JulieManager.JulieAnimation> { JulieManager.JulieAnimation.Sad, JulieManager.JulieAnimation.Angry, JulieManager.JulieAnimation.Defeat };
                // Utiliser Random.Range pour générer un index aléatoire dans la plage de la liste
                int index = Random.Range(0, julieHappy.Count);
                JulieManager.JulieAnimation randomHappyAnimation = julieHappy[index];
                index = Random.Range(0, julieNotHappy.Count);
                JulieManager.JulieAnimation randomNotHappyAnimation = julieNotHappy[index];
                // Appeler la méthode CognitiveBalloonPopped de ScoreManager
                ScoreManager.instance.CognitiveBalloonPopped(balloon.GetColor() == IntendedColor);
                // Jouer l'animation en fonction de la couleur du ballon
                JulieManager.Instance.Play((balloon.GetColor() == IntendedColor) ? randomHappyAnimation : randomNotHappyAnimation);
                // Le ballon gère son animation lui-même
                balloon.PopBalloon();
                GameCreator.Instance.BalloonDestroyed++;
                this.DeflateAllBalloons();
            }
            else
            {
                Debug.Log("A cognitive balloon has been destroyed by user, but such balloon wasn't known by GameCreator. This should not happened");
            }

        }

        private void DeflateAllBalloons()
        {
            for (int i = balloons.Count - 1; i >= 0; i--)
            {
                balloons[i].DeflateBalloon();
                this.RemoveBalloon(balloons[i], false);
            }
        }

        /// <summary>
        /// Increse balloon counting
        /// </summary>
        public void AddBalloon(BalloonBehaviour balloon)
        {
            balloons.Add(balloon);
            balloonsInScene++;
        }

        /// <summary>
        /// decrese balloon counting 
        /// </summary>
        public void RemoveBalloon(BalloonBehaviour balloon,bool destroy = true)
        {
            if (balloons.Contains(balloon))
                balloonsInScene--;
            balloons.Remove(balloon);
            if (destroy)
                Destroy(balloon.gameObject);
        }

        /// <summary>
        /// Spawn a bonus balloon inside the area
        /// </summary>
        private void CreateRandomBonusBalloon()
        {
            Vector3 pos = PlaySpace.GetRandomPointInArea();
            //Random height
            pos.y = GetBalloonHeight();
            Quaternion rot = Quaternion.identity;
            int random = Random.Range(0, bonusBalloonsPrefabs.Count);
            BonusBehaviour balloon = Instantiate(bonusBalloonsPrefabs[random], pos, rot); 
        }

        /// <summary>
        /// Will add a random balloon in the area
        /// </summary>
        private void CreateRandomBalloon()
        {
            Vector3 pos;
            Quaternion rot;
            int random = 0;

            if (BalloonDestroyed == 0)
            {
                rot = Quaternion.Euler(-90, -90, 0);
                pos = Camera.main.transform.position + Vector3.Scale(new Vector3(1f,0f,1f),Camera.main.transform.forward * 1.0f);
            }
            else
            {
                pos = PlaySpace.GetRandomPointInArea();
                pos.y = GetBalloonHeight();
                rot = Quaternion.Euler(-90, -90, 0);
                random = Random.Range(0, balloonsPrefabs.Count);
            }

            BalloonBehaviour balloon = Instantiate(balloonsPrefabs[random], pos, rot);
            this.AddBalloon(balloon);
        }

    /// <summary>
        /// Cognitive Game - balloon heights are always the same
        /// Motor Game - balloon heights are randomized.
        /// </summary>
        /// <returns></returns>
        private float GetBalloonHeight()
        {
            if(!cam) cam = Camera.main;
            var headPosition = cam ? cam.transform.position.y : 1.4f;

            if (currentType == GameManager.GameType.COGNITIVE)
            {
                //Debug.Log($"****BALLOON HEIGHT = {headPosition}**** - Cognitive Mode - Head height - NaN -" +
                          //$"Head Height=({headPosition})");
                return headPosition;
            }
            var valueSelect = 0f;
            
            switch(sampleDistributionType)
            {
                case SampleType.Uniform:
                    if (useAboveBelow)
                    {
                        valueSelect = headPosition + Random.Range(-balloonBelow, balloonAbove);
                        //Debug.Log($"****BALLOON HEIGHT = {valueSelect}**** - Motor/Classic Mode - Uniform - Use Above Below -" +
                                  //$"Head Height=({headPosition}) +/- {balloonBelow}/{balloonAbove} ");
                        return valueSelect;
                    }
                    // When not using Above/Below, fall back to using heightModifier for a symmetric range
                    valueSelect = headPosition + Random.Range(-heightModifier, heightModifier);
                    //Debug.Log($"****BALLOON HEIGHT = {valueSelect}**** - Motor/Classic Mode - Uniform - Use Height Modifier -" +
                              //$"Head Height=({headPosition}) +/- {heightModifier}/{heightModifier} ");
                    return valueSelect;
                case SampleType.Gaussian:
                    valueSelect = SampleGaussian(headPosition, heightModifier);
                    //Debug.Log($"****BALLOON HEIGHT = {valueSelect}**** - Motor/Classic Mode - Gaussian - Use Height Modifier -" +
                              //$"Head Height=({headPosition}) SD={heightModifier} ");
                    return valueSelect;
                case SampleType.HeadHeight:
                    //Debug.Log($"****BALLOON HEIGHT = {headPosition}**** - Motor/Classic Mode - Head height - NaN -" +
                              //$"Head Height=({headPosition})");
                    return headPosition;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public static float SampleGaussian(float mean, float stddev)
        {
            var x1 = Random.Range(0.0f, 1.0f);
            var x2 = Random.Range(0.0f, 1.0f);

            var y1 = (float)(Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2));

            var sample = y1 * stddev + mean;

            //Debug.Log("Gaussian Sample = " + sample + " (Mean = " + mean + " SD = " + stddev + ")");

            return sample;
        }
        
        /// <summary>
        /// Will create a specific balloon between user and Julie
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="position"></param>
        /// <param name="totalBalloon"></param>
        private void CreateBalloon(BalloonBehaviour prefab,int position,int totalBalloon)
        {
            Vector3 pos;
            Quaternion rot;
            float spaceBetweenBalloon = 30;
            Vector3 center = PlaySpace.Instance.GetCenter().transform.position;
            Vector3 julie = JulieManager.Instance.GetPosition();

            Vector3 direction = (julie - center);
            //Quaternion.LookRotation(julie - center, Vector3.up).
            float angle = (totalBalloon % 2 == 0)
                ? (position % 2 == 0)
                    ? 0.5f * spaceBetweenBalloon + position / 2 * spaceBetweenBalloon
                    : -0.5f * spaceBetweenBalloon - position / 2 * spaceBetweenBalloon
                : (position % 2 == 0)
                    ? position / 2 * spaceBetweenBalloon
                    : -(position + 1) / 2 * spaceBetweenBalloon;

            //Debug.Log("Angle : "+angle + " Balloon ID: " + position + " TotalBalloon :" + totalBalloon+"Calc : "+ position / 2);
            Vector3 finalDir = Quaternion.Euler(0, angle, 0) * direction;
            pos = center + finalDir.normalized * minDistance;
            //Random height
            pos.y = GetBalloonHeight();
            rot = Quaternion.Euler(-90, -90, 0);

            BalloonBehaviour balloon = Instantiate(prefab, pos, rot);
            
            Options balloonOptions = new Options();
                balloonOptions.id = balloon.GetInstanceID();
                balloonOptions.color = balloon.GetColor().ToString();
                balloonOptions.balloonPosition = pos;
                temporaryOptionsList.Add(balloonOptions);
                
            this.AddBalloon(balloon);
            
            if (position == totalBalloon - 1)
            {
                BalloonDataRegistrer.RegisterWavesData(intendedColor.ToString(), temporaryOptionsList);
                temporaryOptionsList.Clear();
            }
            
        }


        private void Init()
        {
            BalloonDestroyed = 0;
            balloonsInScene = 0;
            ScoreManager.instance.SetScore(0);
        }

        private GameManager.GameType currentType ;
        public void Play(GameManager.GameType type)
        {
            if (GameIsRunning)
            {
                //Quit current session ?
                Debug.Log("A game is already running, should quit.");
                
            }

            currentType = type;

            Init();

            switch (type)
            {
                case GameManager.GameType.MOBILITY:
                    this.PlayClassic();
                    break;
                case GameManager.GameType.COGNITIVE:
                    this.PlayCognitive();
                    break;
                case GameManager.GameType.FREEPLAY:
                    this.FreePlay();
                    break;
            }

            //We notify panels that a game has started
            OnGameStarted?.Invoke(type);
        }

        /// <summary>
        /// Will launch a classic game of popballoon.
        /// </summary>
        public void PlayClassic()
        {
            StartCoroutine(ClassicSpawning());
        }

        /// <summary>
        /// Will launch game of the new version of popballoon.
        /// </summary>
        public void PlayCognitive()
        {
            StartCoroutine(CognitiveSpawning());
        }

        /// <summary>
        /// Will launch a with countdown and infinite balloons.
        /// </summary>
        public void FreePlay()
        {
            // Start the FreePlay coroutine which spawns balloons continuously until QuitLevel is called
            StartCoroutine(ContinuousFreePlay());
        }

        private IEnumerator ContinuousFreePlay()
        {
            // Ensure MRTK ready for any DirectionIndicator initialization
            yield return StartCoroutine(WaitForMRTKReady());

            // Reset counters
            BalloonDestroyed = 0;
            balloonsInScene = 0;

            countdown?.SetActive(false);
            GameIsRunning = true;

            while (GameIsRunning && 
                   GameManager.Instance.CurrentState == GameManager.GameState.PLAY &&
                   GameManager.Instance.CurrentGameType == GameManager.GameType.FREEPLAY)  // Triple check
            {
                try
                {
                    // Additional safety check - stop if not in FreePlay anymore
                    if (GameManager.Instance.CurrentGameType != GameManager.GameType.FREEPLAY)
                    {
                        break;
                    }

                    if (freePlayMaxSimultaneous <= 0 || balloonsInScene < freePlayMaxSimultaneous)
                    {
                        CreateRandomBalloon();
                    }

                    // Occasionally spawn a bonus balloon
                    if (bonusBalloonsPrefabs != null && bonusBalloonsPrefabs.Count > 0 && Random.value < 0.05f)
                    {
                        CreateRandomBonusBalloon();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"GameCreator: Error in FreePlay spawn loop: {e.Message}");
                    Debug.LogError($"Stack trace: {e.StackTrace}");
                }

                yield return new WaitForSeconds(freePlaySpawnInterval);
            }
            
        }

        /// <summary>
        /// Will abort current level status
        /// </summary>
        public void QuitLevel()
        {
            Debug.Log("[GameCreator] QuitLevel called. Stopping all coroutines and resetting state.");
            
            // Stop the game loop FIRST to prevent any new spawns
            GameIsRunning = false;
            
            // Stop all spawning coroutines
            StopAllCoroutines();
            
            // Clean up all remaining balloons
            DeflateAllBalloons();
            
            // Reset balloon counters
            balloonsInScene = 0;
            BalloonDestroyed = 0;
            
            // Hide countdown if visible
            if (countdown != null) countdown.SetActive(false);
            
            // Reset tutorial state just in case
            currentTutorialState = TutorialState.FINISH;
            
            // Notify listeners
            OnGameInterrupted?.Invoke(GameManager.Instance.CurrentGameType);
        }

        /// <summary>
        /// Return a prefab of the correct balloon for this level
        /// </summary>
        /// <returns>A balloon behaviour prefab</returns>
        private BalloonBehaviour GetCorrectBalloonPrefab()
        {
            return balloonsPrefabs.Find(balloon => balloon.GetColor() == IntendedColor);
        }
        /// <summary>
        /// Return a prefab one of the wrong balloon for this level
        /// </summary>
        /// <returns>A balloon behaviour prefab</returns>
        private BalloonBehaviour GetWrongBalloonPrefab()
        {
            
            var candidates = balloonsPrefabs.FindAll(balloon => availableColors.Contains(balloon.GetColor()) && !usedColors.Contains(balloon.GetColor()));
            if (candidates == null || candidates.Count == 0)
            {
                // Fallback: return any prefab that isn't the intended color, or the first prefab if none found
                var fallback = balloonsPrefabs.Find(balloon => balloon.GetColor() != IntendedColor);
                if (fallback != null) return fallback;
                return balloonsPrefabs.Count > 0 ? balloonsPrefabs[0] : null;
            }

            int rand = Random.Range(0, candidates.Count);
            BalloonBehaviour b = candidates[rand];
            usedColors.Add(b.GetColor());
            return b;
        }


        private List<BalloonColor> GetAvailableColors(int maxOption = 2)
        {
            List<BalloonColor> colors = new List<BalloonColor>();
            switch (GameManager.Instance.CurrentLevelIndex)
            {
                case 0:
                    colors.Add(BalloonColor.BLUE);
                    colors.Add(BalloonColor.RED);
                    break;
                case 1:
                    colors.Add(BalloonColor.BLUE);
                    colors.Add(BalloonColor.RED);
                    break;
                default:
                    Debug.Log("DEFAULT LEVEL");
                    for (int i = 0; i<4; i++)
                    {
                        colors.Add((BalloonColor)i);
                    }

                    while(colors.Count > maxOption)
                    {
                        colors.RemoveAt(Random.Range(0, colors.Count)); //Remove N random color from the list
                    }
                    break;
                    
            }
            return colors;
        }

        /// <summary>
        /// Cognitve mode's settings
        /// </summary>
        /// <returns></returns>
        private int GetMaxOptions()
        {
            switch (GameManager.Instance.CurrentLevelIndex)
            {
                case 0:
                case 1:
                case 2: 
                case 5: return 2;
                case 3: 
                case 6: return 3;
                case 4:
                case 7: return 4;
                default: return 2;
            }
        }

        public float GetAdvancement()
        {
            if(maxBalloon != 0)
                return ((float) this.balloonDestroyed) / (float)this.maxBalloon;
            return 0f;
        }

        #endregion


        #region Coroutines
        IEnumerator HandleMotricityTutorialState()
        {
            switch (currentTutorialState)
            {
                case TutorialState.INITIALISATION:
                    JulieManager.Instance.Init(true);
                    this.currentTutorialState = TutorialState.POSITIONNING;
                    break;
                case TutorialState.POSITIONNING:
                    if (JulieManager.Instance.CurrentState == JulieManager.JulieState.READY)
                    {
                        JulieManager.Instance.Play(JulieManager.JulieAnimation.Setup_Motricity);
                        this.currentTutorialState = TutorialState.WAITING_FOR_DEMONSTRATION;
                        yield return new WaitForSeconds(1f);
                    }
                    break;
                case TutorialState.WAITING_FOR_DEMONSTRATION:
                    if (JulieManager.Instance.IsFocused())
                    {
                        this.currentTutorialState = TutorialState.DEMONSTRATION;
                    }
                    break;
                case TutorialState.DEMONSTRATION:
                    JulieManager.Instance.Play(JulieManager.JulieAnimation.Demonstrate_Motricity);
                    yield return new WaitForSeconds(3f);
                    balloonDestroyed = 0;
                    this.currentTutorialState = TutorialState.DIY;
                    break;
                case TutorialState.DIY:
                    //TODO: spawn a ballon
                    if(balloonDestroyed == 0 && balloonsInScene == 0)
                    {
                        yield return new WaitForSeconds(1f);
                        CreateRandomBalloon();
                    }

                    if(balloonDestroyed != 0)
                    {
                        this.currentTutorialState = TutorialState.FINISH;
                    }

                    break;
                case TutorialState.FINISH:
                    //JulieManager.Instance.Play(JulieManager.JulieAnimation.Disappear);
                    break;
            }

            yield return null;
        }

        IEnumerator HandleCognitiveTutorialState()
        {
            switch (currentTutorialState)
            {
                case TutorialState.INITIALISATION:
                    JulieManager.Instance.Init(true);
                    this.currentTutorialState = TutorialState.POSITIONNING;
                    break;
                case TutorialState.POSITIONNING:
                    if (JulieManager.Instance.CurrentState == JulieManager.JulieState.READY)
                    {
                        JulieManager.Instance.Play(JulieManager.JulieAnimation.Setup_Cognitive);
                        this.currentTutorialState = TutorialState.WAITING_FOR_DEMONSTRATION;
                        yield return new WaitForSeconds(1f);
                    }
                    break;
                case TutorialState.WAITING_FOR_DEMONSTRATION:
                    if (JulieManager.Instance.IsFocused())
                    {
                        this.currentTutorialState = TutorialState.DEMONSTRATION;
                    }
                    break;
                case TutorialState.DEMONSTRATION:
                    JulieManager.Instance.Play(JulieManager.JulieAnimation.Demonstrate_Cognitive);
                    yield return new WaitForSeconds(10.5f);
                    balloonDestroyed = 0;
                    ScoreManager.initScore();
                    this.currentTutorialState = TutorialState.DIY;
                    break;
                case TutorialState.DIY:
                    //the tutorial balloon is always blue.
                    maxBalloon = 2;

                    if (BalloonDestroyed > 4 || (BalloonDestroyed >= 3 && ScoreManager.instance.CorrectBalloon < 2))
                    {
                        this.currentTutorialState = TutorialState.POSITIONNING;
                        break;
                    }

                    if (balloonsInScene == 0 && ScoreManager.instance.CorrectBalloon == 0)
                    {
                        yield return new WaitForSeconds(3f);
                        usedColors.Clear();
                        //We spawn balloons
                        this.CreateBalloon(GetCorrectBalloonPrefab(), 0, 2);
                        this.CreateBalloon(GetWrongBalloonPrefab(), 1, 2);
                        
                        break;
                    }

                    if (balloonsInScene == 0 && ScoreManager.instance.CorrectBalloon == 1)
                    {
                        yield return new WaitForSeconds(3f);
                        usedColors.Clear();
                        //We spawn balloons
                        this.CreateBalloon(GetWrongBalloonPrefab(), 0, 2);
                        this.CreateBalloon(GetCorrectBalloonPrefab(), 1, 2);
                        //yield return new WaitForSeconds(3f);
                        break;
                    }


                    if (balloonsInScene == 0 && ScoreManager.instance.CorrectBalloon == 2)
                    {
                        this.currentTutorialState = TutorialState.FINISH;
                        break;
                    }

                    break;
                case TutorialState.FINISH:
                    
                    break;
            }

            yield return null;
        }

        /// <summary>
        /// Classic game level, five balloon to destroy inside the area
        /// </summary>
        /// <returns></returns>
        IEnumerator ClassicSpawning()
        {
            // Ensure MRTK (InputSystem/FocusProvider) and cursors are initialized before spawning
            yield return StartCoroutine(WaitForMRTKReady());
            TimerManager.LevelEnd();
            TimerManager.InitTimer();
            yield return new WaitForSeconds(1.0f);

            // Set game running flag to enable spawn loop
            GameIsRunning = true;

            intendedColor = BalloonColor.BLUE;
            RefreshJulieShirt();

            if (GameManager.Instance.CurrentLevelIndex == 0)  // level is Tutorial (simplified version)
            {
                this.currentTutorialState = TutorialState.INITIALISATION;
                while (currentTutorialState != TutorialState.FINISH)
                {
                    yield return HandleMotricityTutorialState();
                }
                JulieManager.Instance.Play(JulieManager.JulieAnimation.Disappear);
            }
            else
            {
                countdown.SetActive(true);
                maxBalloon = 5;

                // Level intro delay
                yield return new WaitForSeconds(3.2f);
                TimerManager.LevelStart();

                while (GameIsRunning && BalloonDestroyed < maxBalloon)
                {
                    if (!MoreThanEnough)
                    {
                        CreateRandomBalloon();
                        
                        yield return new WaitForSeconds(2);
                    }
                    else
                    {
                        yield return new WaitForSeconds(2);
                    }
                }

                
                CreateRandomBonusBalloon();

                //TODO: Subscribe TimerManager to this dedicated event.
                yield return new WaitForSeconds(3f);
                TimerManager.LevelEnd();
                //We wait a few second before ending level completly
                //ScoreBoard.Instance.LevelEnd();
                //GameManager.LevelEnd();
            }
            yield return null;
            Debug.Log("InvokingOnGameEnded");
            OnGameEnded?.Invoke(GameManager.GameType.MOBILITY);
            Debug.Log("OnGameEndedInvoked");
        }

        /// <summary>
        /// Classic game level, five balloon to destroy inside the area
        /// </summary>
        /// <returns></returns>
        IEnumerator CognitiveSpawning()
        {
            // Ensure MRTK (InputSystem/FocusProvider) and cursors are initialized before spawning
            yield return StartCoroutine(WaitForMRTKReady());
            TimerManager.LevelEnd();
            TimerManager.InitTimer();
            yield return new WaitForSeconds(1.0f);

            // Set game running flag to enable spawn loop
            GameIsRunning = true;

            int nbOption = GetMaxOptions();
            availableColors = GetAvailableColors(nbOption);
            usedColors = new List<BalloonColor>();
           

            if (GameManager.Instance.CurrentLevelIndex == 0)//level is Tutorial (simplified version)
            {
                intendedColor = BalloonColor.BLUE;
                availableColors.Remove(intendedColor);

                this.currentTutorialState = TutorialState.INITIALISATION;
                while (this.currentTutorialState != TutorialState.FINISH)
                {
                    yield return HandleCognitiveTutorialState();
                }
                RefreshJulieShirt();
            }
            else
            {
                intendedColor = availableColors[Random.Range(0, availableColors.Count)];
                availableColors.Remove(intendedColor);
                RefreshJulieShirt();

                while (JulieManager.Instance.CurrentState != JulieManager.JulieState.READY)
                {
                    yield return null;
                }



                countdown.SetActive(true);
                // Level intro delay
                yield return new WaitForSeconds(3.2f);
                
                TimerManager.LevelStart();
                //We pick one color at random
               
                //Permet de pondérer les échecs en fonction du nombre de ballons présents dans la scene. = MAX_BALLOON + (NB_COLOR - 2)*2
                maxBalloon = 10 + (nbOption - 2) * 2;
                while (GameIsRunning && BalloonDestroyed < maxBalloon)
                {
                    //Reversal
                    if (BalloonDestroyed == maxBalloon / 2)
                    {
                        BalloonColor tmp = intendedColor;
                        intendedColor = availableColors[Random.Range(0, availableColors.Count)];
                        availableColors.Add(tmp);
                        availableColors.Remove(intendedColor);
                        RefreshJulieShirt();
                    }

                    int correctBalloonPosition = Random.Range(0, nbOption);
                    usedColors.Clear();
                    
                    for (int i = 0; i < nbOption; i++)
                    {
                        BalloonBehaviour prefabToInstantiate;
                        //We ensure we have at least one correct balloon in the list
                        if (correctBalloonPosition == i)
                        {
                            prefabToInstantiate = this.GetCorrectBalloonPrefab();
                        }
                        else
                        {
                            prefabToInstantiate = this.GetWrongBalloonPrefab();
                        }

                        this.CreateBalloon(prefabToInstantiate, i, nbOption);
                        //options.Add(prefabToInstantiate.GetColor());
                        //Debug.Log("Contents of options list: " + prefabToInstantiate.ToString().GetType());
                    }


                    while (BalloonsInScene > 0)
                    {
                        yield return null;
                        //TODO: Make Julie move once in a while ? Maybe inside Julie's code.
                    }

                    //Prevent intempestive spawning. // Check if child is in place
                    yield return new WaitForSeconds(3f);
                }
                TimerManager.LevelEnd();
            }

            //Old 
            //if (!CorrectBalloonsRemains)
            //{
            //    balloonDestroyed++;
            //    if (BalloonDestroyed < 5)
            //    {
            //        int correctBalloonPosition = Random.Range(0, nbColor);
            //        for (int i = 0; i < nbColor; i++)
            //        {
            //            BalloonBehaviour prefabToInstantiate;
            //            //We ensure we have at least one correct balloon in the list
            //            if (correctBalloonPosition == i)
            //            {
            //                prefabToInstantiate = this.GetCorrectBalloonPrefab();
            //            }
            //            else
            //            {
            //                prefabToInstantiate = this.GetWrongBalloonPrefab();
            //            }

            //            this.CreateBalloon(prefabToInstantiate, i, nbColor);
            //        }

            //        yield return new WaitForSeconds(2);
            //    }
            //}
            //else
            //{
            //    yield return null;
            //}



            yield return null;
            OnGameEnded?.Invoke(GameManager.GameType.COGNITIVE);

            //GameManager.LevelEnd();
            //ScoreBoard.Instance.LevelEnd();
        }

        private void RefreshJulieShirt()
        {
            if (GameManager.Instance.CurrentLevelIndex < 5 && GameManager.Instance.CurrentGameType == GameManager.GameType.COGNITIVE)
            {
                JulieManager.Instance.UpdateShirtColor(intendedColor);
            }
            else
            {
                JulieManager.Instance.UpdateShirtColor(BalloonColor.NONE);
            }
        }
        #endregion

    }
}


