using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Microsoft.MixedReality.Toolkit.Input;
using System;
using PopBalloons.Data;
using PopBalloons.Utilities;
using PopBalloons.HandTracking;

namespace PopBalloons
{
    /// <summary>
    /// we have all the balloon's behaviour in this class 
    /// should never use singleton for this script
    /// </summary>
    /// 

    public class BalloonBehaviour : MonoBehaviour  /*IMixedRealitySourceStateHandler*/
    {
        #region variables
        [SerializeField]
        private GameObject particleBurst;

        [SerializeField]
        private GameObject particlePoof;

        [SerializeField]
        private TMPro.TextMeshPro scoreDisplay;

        [SerializeField]
        private bool isFloating = false;

        [SerializeField]
        private float amplitude = 0.02f;

        [SerializeField]
        private float frequency = 0.33f;

        [SerializeField]
        private GameCreator.BalloonColor color = GameCreator.BalloonColor.BLUE;

        private bool wasDeflated = false;
        private Animator anim;
        private Rigidbody rigidBody;
        private GameObject particleBurstClone;
        private Vector3 tempPos = new Vector3();
        private Vector3 posOffset = new Vector3();
        private bool isOnFloor = false;
        private DateTime initializationTime;
        private float balloonDuration = -1f;
        public static DateTime timeOfCollision;
        public static bool highScore;
        private bool prefered = false;
        private bool normalballoon = false;
        private bool balloonDestroyedByUser = false;
        private bool popOnce = false;
        private bool isTheWrongOne = false;
        private bool detectedCollision = false;
        private HandDetected HD;
        
        public string poppedBalloon;
        public string intendedBalloon;
        public string PoppedColor {get => poppedBalloon; } 
        public string IntendedBalloon {get => intendedBalloon; }

        #endregion
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region events and delegates
        public delegate void DestroyAction(string timeStamp, float duration, bool isBonus, string intendedBalloon, string poppedBalloon);
        public static event DestroyAction OnDestroyBalloon;


        public delegate void BalloonMissed(string timeStamp, float duration, bool timeout, string intendedBalloon, string poppedBalloon);
        public static event BalloonMissed OnBalloonMissed;
        
        public delegate void BalloonSpawned(string timeStamp, Vector3 position);
        public static event BalloonSpawned OnBalloonSpawned;

        public delegate void CognitiveBalloonDestroy(BalloonBehaviour balloon);
        public static event CognitiveBalloonDestroy OnCognitiveBalloonDestroyed;
        #endregion
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region unity functions

        private void Start()
        {
            
            initializationTime = DateTime.UtcNow;
            
            anim = this.GetComponentInChildren<Animator>();
            rigidBody = this.GetComponent<Rigidbody>();
            if(OnBalloonSpawned != null)
            {
                OnBalloonSpawned.Invoke(initializationTime.ToString("yyyy-MM-ddTHH:mm:ss.fff"), this.transform.position);
            }
            AdaptBehaviour();
            posOffset = transform.position;
            StartCoroutine(AutoDestroyBalloon());
        }



        private void Update()
        {
            //floating
            if (isFloating)
            {
                tempPos = posOffset;
                tempPos.x += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;
                transform.position = tempPos;
            }

            //score
            double timeSinceInstantiation = (DateTime.UtcNow - initializationTime).TotalSeconds;
            highScore = (timeSinceInstantiation <= 7.0f);

            timeOfCollision = DateTime.UtcNow ; 
        }
        
        #endregion
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region functions
        /// <summary>
        /// Will return the current instance color
        /// </summary>
        /// <returns>Balloon color (enumeration)</returns>
        public GameCreator.BalloonColor GetColor()
        {
            return this.color;
            
        }

       
        public void OnCollisionEnter(Collision collision)
        {
            poppedBalloon = GetColor().ToString();
            intendedBalloon = GameCreator.instance.intendedColor.ToString();
            

            if (!detectedCollision  && collision.gameObject.tag == "VirtualHand")
            {
                this.OnCollisionStay(collision);
                detectedCollision = true;
            }
            else
            {
                // Debug.Log("Another collision...");
            }
            
        }

        /// <summary>
        /// this fonction detrmines the behavoiur of balloons and it depends on different types of collisions. 
        /// </summary>
        /// <param name="collision"></param>
        public void OnCollisionStay(Collision collision)
        {
            // Early return if balloon was already popped
            if (popOnce)
            {
                Physics.IgnoreCollision(collision.collider, gameObject.GetComponentInChildren<Collider>());
                return;
            }

            if (GameManager.Instance.CurrentGameType == GameManager.GameType.MOBILITY)
            {
                if (collision.gameObject.tag == "VirtualHand")
                {
                    popOnce = true;
                    balloonDestroyedByUser = true;
                }
                if (collision.gameObject.name == "Floor")
                {
                    balloonDestroyedByUser = false;
                    DisposeBalloon();
                }
                balloonDuration = (float) (DateTime.UtcNow - initializationTime).TotalSeconds;
                ScoreManager.onScoreChange += DisplayScore;
                if (OnDestroyBalloon != null) OnDestroyBalloon(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fff"), balloonDuration, false, intendedBalloon, poppedBalloon);
                ScoreManager.onScoreChange -= DisplayScore;
                DisposeBalloon();
            }
            else if (GameManager.Instance.CurrentGameType == GameManager.GameType.FREEPLAY)
            {
                if (collision.gameObject.tag == "VirtualHand")
                {
                    popOnce = true;
                    balloonDestroyedByUser = true;
                    balloonDuration = (float) (DateTime.UtcNow - initializationTime).TotalSeconds;
                    ScoreManager.onScoreChange += DisplayScore;
                    if (OnDestroyBalloon != null) OnDestroyBalloon(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fff"), balloonDuration, false, intendedBalloon, poppedBalloon);
                    ScoreManager.onScoreChange -= DisplayScore;
                    DisposeBalloon();
                }
                if (collision.gameObject.name == "Floor")
                {
                    balloonDestroyedByUser = false;
                    DisposeBalloon();
                }
            }
            else if (GameManager.Instance.CurrentGameType == GameManager.GameType.COGNITIVE)
            {
                if (collision.gameObject.tag == "VirtualHand" && !this.wasDeflated)
                {
                    popOnce = true;
                    balloonDestroyedByUser = true;
                    balloonDuration = (float) (DateTime.UtcNow - initializationTime).TotalSeconds;
                    //Debug.Log("GameCreator.instance.intendedColor " + GameCreator.instance.targetColor);
                    //Debug.Log("GameCreator.instance.poppedColor " + GameCreator.instance.poppedColor);
                    ScoreManager.onScoreChange -= DisplayScore;
                    if (OnDestroyBalloon != null) OnDestroyBalloon(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fff"), balloonDuration, false, intendedBalloon, poppedBalloon);
                    OnCognitiveBalloonDestroyed?.Invoke(this);
                }
            }

            if (collision.gameObject.layer == LayerMask.NameToLayer("Spatial Awareness") && collision.gameObject.tag == "VirtualHand")
            {
                popOnce = true;
                balloonDestroyedByUser = false;
                float _duration = (float) (DateTime.UtcNow - initializationTime).TotalSeconds;
                if (OnBalloonMissed != null) OnBalloonMissed(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fff"), _duration, false, intendedBalloon, poppedBalloon);
                PopBalloon();
            }
            else
            {
                Physics.IgnoreCollision(collision.collider, gameObject.GetComponentInChildren<Collider>());
            }
        }

        private void DisplayScore(int score, int scoreGain)
        {
            TMPro.TextMeshPro scoreDisplayClone = Instantiate(scoreDisplay, this.gameObject.transform.position, Quaternion.identity);
            scoreDisplayClone.text = "+" + scoreGain.ToString();
            Destroy(scoreDisplayClone.gameObject, 3.0f);
        }

        /// <summary>
        /// this function is using when we delete a balloon. if the user blows it, the balloon destroys and will be counted. 
        /// </summary>
        /// <param name="Bref"></param>
        public void DisposeBalloon()
        {
            if (this.balloonDestroyedByUser)
            {
                GameCreator.Instance.BalloonDestroyed++;
                //JulieManager.Instance.Play(JulieManager.JulieAnimation.Clap);
            }
            PopBalloon();
        }

        /// <summary>
        /// this function manages the sound and confetti of the balloon and destroys it
        /// </summary>
        public void PopBalloon()
        {
            if (GameManager.Instance.CurrentGameType == GameManager.GameType.COGNITIVE && this.color != GameCreator.Instance.IntendedColor)
            {
                //TODO: Error sound
                //particleBurstClone.GetComponent<SoundManager>().PlayErrorSound();
                particleBurstClone = Instantiate(particlePoof, this.gameObject.transform.position, Quaternion.identity);
                particleBurstClone.GetComponent<SoundManager>().PlayPop();
                Destroy(particleBurstClone, 0.5f);
            }
            else
            {
                particleBurstClone = Instantiate(particleBurst, this.gameObject.transform.position, Quaternion.identity);
                particleBurstClone.GetComponent<SoundManager>().PlayPopAndConfetti();
                Destroy(particleBurstClone, 0.5f);
            }
            GameCreator.Instance.RemoveBalloon(this);
        }

        /// <summary>
        /// Play deflate animation, then destroy balloon
        /// </summary>
        public void DeflateBalloon()
        {
            //Prevent destruction from other balloon
            wasDeflated = true;
            this.anim.SetTrigger("Deflate");
            Destroy(this.gameObject, 2.0f);
            //TODO: Start deflate animation
        }
        //TODO: Vanish  balloon


        /// <summary>
       /// After 15 seconds the balloon will be disappeared 
       /// </summary>
      /// <returns></returns>
        IEnumerator AutoDestroyBalloon()
        {
            yield return new WaitForSeconds(15.0f);
            
            // Safety checks: ensure balloon is still valid and hasn't been popped yet
            if (this != null && !popOnce && GameCreator.Instance != null)
            {
                isTheWrongOne = false;
                PopBalloon();
                float _duration = (float)(DateTime.UtcNow - initializationTime).TotalSeconds;
                if (OnBalloonMissed != null) OnBalloonMissed(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fff"), _duration, true, intendedBalloon, poppedBalloon);
            }
        }


        /// <summary>
        /// adapts behaviour of the balloons depend on the levels they are in it. 
        /// </summary>
        public void AdaptBehaviour()
        {
            if (rigidBody == null)
            {
                UnityEngine.Debug.LogError("RigidBody shouldn't be null");
                return;
            }

            switch (GameManager.Instance.CurrentGameType)
            {
                case GameManager.GameType.MOBILITY:
                    switch (GameManager.Instance.CurrentLevelIndex)
                    {
                        case 1:
                            this.isFloating = false;
                            this.frequency = 0f;
                            rigidBody.useGravity = false;
                            break;

                        case 2:
                            this.isFloating = true;
                            this.frequency = 0.33f;
                            this.amplitude = 0.15f;
                            rigidBody.useGravity = false;
                            break;

                        case 3:
                            this.isFloating = false;
                            rigidBody.useGravity = true;
                            rigidBody.isKinematic = false;
                            rigidBody.drag = 35.0f;
                            break;

                        case 4:
                            this.isFloating = false;
                            rigidBody.useGravity = true;
                            rigidBody.isKinematic = false;
                            rigidBody.drag = 28.0f;
                            break;
                        default:
                            this.isFloating = false;
                            this.frequency = 0f;
                            rigidBody.useGravity = false;
                            break;
                    }
                    break;
                case GameManager.GameType.FREEPLAY:
                    this.isFloating = false;
                    rigidBody.useGravity = true;
                    rigidBody.isKinematic = false;
                    // Random drag between 28.0f (faster) and 40.0f (slower) for variety
                    // Each balloon will have a different falling speed
                    rigidBody.drag = UnityEngine.Random.Range(28.0f, 40.0f);
                    break;
                default:
                    this.isFloating = false;
                    rigidBody.useGravity = false;
                    this.frequency = 0f;
                    break;
            }
            
        }

        #endregion
    }
}

