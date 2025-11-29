using PopBalloons;
using PopBalloons.Boundaries;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PopBalloons.Utilities
{

    /// <summary>
    /// Class that will manage the avatars position and animation.
    /// </summary>
    public class JulieManager : MonoBehaviour
    {
        #region variables
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static JulieManager instance;


        [Header("Settings :")]
        [SerializeField]
        private GameObject juliePrefab;

        [SerializeField]
        [Tooltip("Textures correspondants au pull de Julie (Bleu,Rouge,Vert,Jaune)")]
        private List<Texture2D> texs;
        [Header("Walking settings :")]
        [SerializeField]
        private bool autoFollow = false;
        [SerializeField]
        [Range(0, 10)]
        private float speed;
        [SerializeField]
        private Vector3 destination;
        [SerializeField]
        private Transform target;

        [Header("Focus :")]
        [SerializeField]
        [Range(0, 5)]
        private float focusTimeRequired;
        [SerializeField]
        [Range(0.5f, 1)]
        private float focusTolerance;

        private float focusTime;
        private Animator julie;
        private Camera player;
        private JulieState currentState = JulieState.NONE;
        private Vector3 initialSpawnPosition; // Store where Julie first appeared
        
        // Track goodbye sequence to handle re-entrance
        private Coroutine goodbyeCoroutine = null;
        private bool shouldRespawnAfterGoodbye = false;


        public enum JulieState {NONE,INIT,WALKING,READY}
        public enum JulieAnimation {Dance,Clap,Sad,Angry,Defeat,Jump,Pop,Point_Left,Point_Right,Point_Middle,Setup_Motricity,Demonstrate_Motricity,Disappear,Setup_Cognitive,Demonstrate_Cognitive,Salute};

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static JulieManager Instance { get => instance;}
        public JulieState CurrentState { get => currentState; }

        #endregion

        #region Unity functions
        /// <summary>
        /// Singleton implementation
        /// </summary>
        private void Awake()
        {
            if(instance != null)
            {
                DestroyImmediate(this);
            }
            else
            {
                GameManager.OnGameStateChanged += HandleGameStateChanged;
                player = Camera.main;
                instance = this;
            }
        }
        
        private void Start()
        {
            // Subscribe to MainPanel state changes to detect when leaving Cognitive mode
            if (PopBalloons.UI.MainPanel.Instance != null)
            {
                PopBalloons.UI.MainPanel.Instance.Subscribe(HandleMainPanelStateChanged);
            }
        }
        
        private void OnDestroy()
        {
            GameManager.OnGameStateChanged -= HandleGameStateChanged;
            if (PopBalloons.UI.MainPanel.Instance != null)
            {
                PopBalloons.UI.MainPanel.Instance.UnSubscribe(HandleMainPanelStateChanged);
            }
        }
        
        private PopBalloons.UI.MainPanelState lastPanelState = PopBalloons.UI.MainPanelState.PROFILE;
        
        /// <summary>
        /// Handles MainPanel state changes to dismiss Julie when leaving Cognitive mode
        /// </summary>
        private void HandleMainPanelStateChanged(PopBalloons.UI.MainPanelState newPanelState)
        {
            // If we're ENTERING COGNITIVE mode - spawn Julie at her position
            if (newPanelState == PopBalloons.UI.MainPanelState.COGNITIVE && 
                lastPanelState != PopBalloons.UI.MainPanelState.COGNITIVE)
            {
                // Check if goodbye sequence is running
                if (goodbyeCoroutine != null)
                {
                    // User re-entered Cognitive mode during goodbye - let goodbye finish then respawn
                    shouldRespawnAfterGoodbye = true;
                }
                else
                {
                    // Normal spawn
                    SpawnJulieAtStart();
                }
            }
            // If we're leaving COGNITIVE mode to go to another mode
            else if (lastPanelState == PopBalloons.UI.MainPanelState.COGNITIVE && 
                newPanelState != PopBalloons.UI.MainPanelState.COGNITIVE)
            {
                DismissJulie();
            }
            
            lastPanelState = newPanelState;
        }

        private void HandleGameStateChanged(GameManager.GameState newState)
        {
            if (newState == GameManager.GameState.PLAY && GameManager.Instance.CurrentGameType == GameManager.GameType.COGNITIVE)
            {
                // Game is starting - Julie walks towards player (she's already spawned from panel change)
                WalkJulieToPlayer();
            }
            else if (newState == GameManager.GameState.HOME)
            {
                // Game ended or user went back - Julie should move to the side to avoid blocking the screen
                if (julie != default && PlaySpace.Instance != null)
                {
                    // Stop any ongoing walking animation
                    StopAllCoroutines();
                    
                    // IMPORTANT: Stop the walking animation properly
                    if (julie.GetBool("isWalking"))
                    {
                        julie.SetBool("isWalking", false);
                    }
                    
                    // If Julie is currently walking or ready (i.e., visible and active)
                    if (currentState == JulieState.WALKING || currentState == JulieState.READY)
                    {
                        // Move Julie to the side (right) so she doesn't block the UI panel
                        if (Camera.main != null)
                        {
                            Vector3 playerPos = Camera.main.transform.position;
                            Vector3 playerForward = Camera.main.transform.forward;
                            Vector3 playerRight = Camera.main.transform.right; // Right direction
                            
                            // Position: 1.5m forward + 1m to the right, at Julie's Y level
                            Vector3 sidePosition = playerPos + playerForward * 1.5f + playerRight * 1.0f;
                            sidePosition.y = julie.transform.parent.position.y;
                            
                            destination = sidePosition;
                            julie.transform.parent.LookAt(new Vector3(destination.x, julie.transform.parent.position.y, destination.z), Vector3.up);
                            
                            // Walk to side position smoothly
                            WalkToward(destination);
                            currentState = JulieState.WALKING;
                        }
                        else
                        {
                            // Fallback to corner if camera not available
                            AreaSegment segment = PlaySpace.Instance.GetFacingSegment();
                            GameObject center = PlaySpace.Instance.GetCenter();
                            Vector3 cornerPosition = segment.V2.transform.position + (segment.V2.transform.position - center.transform.position).normalized * 0.5f;
                            cornerPosition.y = julie.transform.parent.position.y;
                            
                            destination = cornerPosition;
                            julie.transform.parent.LookAt(new Vector3(destination.x, julie.transform.parent.position.y, destination.z), Vector3.up);
                            WalkToward(destination);
                            currentState = JulieState.WALKING;
                        }
                    }
                    
                    // Reset state for next game
                    currentState = JulieState.INIT;
                }
            }
        }
        
        /// <summary>
        /// Spawns Julie at her initial position when Cognitive mode is selected
        /// She appears but doesn't walk towards player yet
        /// </summary>
        private void SpawnJulieAtStart()
        {
            // If Julie doesn't exist, create her
            if (julie == default)
            {
                currentState = JulieState.INIT;
                julie = Instantiate(juliePrefab, this.transform).GetComponentInChildren<Animator>();
                
                if (PlaySpace.Instance)
                {
                    // Play appearing animation
                    julie.Play("Julie@Appearing");
                    
                    // Position Julie at a segment edge
                    AreaSegment segment = PlaySpace.Instance.GetFacingSegment();
                    GameObject center = PlaySpace.Instance.GetCenter();
                    julie.transform.parent.position = segment.V1.transform.position + (segment.V1.transform.position - center.transform.position).normalized * 0.5f;
                    
                    // Save this position so Julie can return here when dismissed
                    initialSpawnPosition = julie.transform.parent.position;
                    
                    currentState = JulieState.READY;
                }
            }
        }
        
        /// <summary>
        /// Makes Julie walk towards the player (called when game starts)
        /// </summary>
        private void WalkJulieToPlayer()
        {
            if (julie == default || PlaySpace.Instance == null)
            {
                return;
            }
            
            // Calculate destination 1.5m in front of the player
            if (Camera.main != null)
            {
                Vector3 playerPos = Camera.main.transform.position;
                Vector3 playerForward = Camera.main.transform.forward;
                
                // Position 1.5 meters in front of player (1m + 50cm extra), at Julie's Y level
                destination = playerPos + playerForward * 1.5f;
                destination.y = julie.transform.parent.position.y;
                
                julie.transform.parent.LookAt(new Vector3(destination.x, julie.transform.parent.position.y, destination.z), Vector3.up);
                Invoke("WalkTowardDestination", 0.5f);
                currentState = JulieState.WALKING;
            }
        }
        
        /// <summary>
        /// Called when completely leaving Cognitive mode (e.g., going to another game mode)
        /// This makes Julie walk back to spawn, wave goodbye, then disappear
        /// </summary>
        public void DismissJulie()
        {
            if (julie != default && goodbyeCoroutine == null)
            {
                StopAllCoroutines();
                
                // Stop walking animation
                if (julie.GetBool("isWalking"))
                {
                    julie.SetBool("isWalking", false);
                }
                
                // Reset respawn flag and start goodbye sequence
                shouldRespawnAfterGoodbye = false;
                goodbyeCoroutine = StartCoroutine(GoodbyeSequence());
            }
        }
        
        /// <summary>
        /// Coroutine that orchestrates Julie's goodbye: walk to spawn → salute with disappear effect
        /// </summary>
        private System.Collections.IEnumerator GoodbyeSequence()
        {
            // Step 1: Walk back to spawn position
            destination = initialSpawnPosition;
            julie.transform.parent.LookAt(new Vector3(destination.x, julie.transform.parent.position.y, destination.z), Vector3.up);
            
            // Start walking manually instead of using WalkToward to avoid coroutine conflicts
            julie.SetBool("isWalking", true);
            currentState = JulieState.WALKING;
            
            // Wait until Julie reaches the spawn position (or timeout after 10 seconds)
            float timeout = 10f;
            float elapsed = 0f;
            while (Vector3.Distance(julie.transform.parent.position, destination) > 0.2f && elapsed < timeout)
            {
                // Move Julie towards destination
                Vector3 initialDirection = (destination - julie.transform.parent.position).normalized;
                Vector3 offset = initialDirection * Time.deltaTime * speed;
                
                if (offset.sqrMagnitude > (destination - julie.transform.parent.position).sqrMagnitude)
                {
                    julie.transform.parent.position = destination;
                    break;
                }
                
                julie.transform.parent.position += offset;
                julie.transform.parent.rotation = Quaternion.Slerp(
                    julie.transform.parent.rotation, 
                    Quaternion.LookRotation(new Vector3(destination.x, julie.transform.parent.position.y, destination.z) - julie.transform.parent.position, Vector3.up), 
                    0.1f
                );
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Stop walking
            julie.SetBool("isWalking", false);
            currentState = JulieState.READY;
            
            // Step 2: Play Salute animation
            julie.SetTrigger("Salute");
            
            // Wait for Salute to play (not too long to avoid double salute)
            yield return new WaitForSeconds(2f);
            
            // Step 3: Play Disappear
            this.Play(JulieAnimation.Disappear);
            
            // Wait for Disappear to finish
            yield return new WaitForSeconds(4f);
            
            currentState = JulieState.NONE;
            DestroyJulie();
            
            // Clear goodbye coroutine reference
            goodbyeCoroutine = null;
            
            // Check if we should respawn (user re-entered Cognitive during goodbye)
            if (shouldRespawnAfterGoodbye)
            {
                shouldRespawnAfterGoodbye = false;
                SpawnJulieAtStart();
            }
        }
        
        private void DestroyJulie()
        {
            if (julie != default)
            {
                Destroy(julie.transform.parent.gameObject);
                julie = null;
                currentState = JulieState.NONE;
            }
        }

        private void Update()
        {

            if(julie == default)
            {
                return;
            }

            //Gaze at player at all time for now
            if (!julie.GetBool("isWalking"))
            {
                Vector3 center = PlaySpace.Instance.GetCenter().transform.position;
                julie.transform.parent.rotation = Quaternion.Slerp(julie.transform.parent.rotation, Quaternion.LookRotation(new Vector3(center.x, julie.transform.parent.position.y, center.z) - julie.transform.parent.position, Vector3.up), 0.1f);
            }

            if (autoFollow && target != null)
            {
                destination = new Vector3(target.position.x, julie.transform.parent.position.y, target.position.z);
                FollowTarget();
            }
            else if (autoFollow && target == null)
            {
                // Auto-assign target if needed
                if (Camera.main != null)
                {
                    target = Camera.main.transform;
                }
            }
        }

        #endregion

        #region Julie Functions
        public Vector3 GetPosition()
        {
            return this.julie.transform.parent.position;
        }

        /// <summary>
        /// Should add eye tracking to this feature
        /// </summary>
        /// <returns></returns>
        public bool IsFocused()
        {
            // Auto-assign target to Camera.main if not set
            if (target == null)
            {
                if (Camera.main != null)
                {
                    target = Camera.main.transform;
                }
                else
                {
                    Debug.LogWarning("JulieManager: target is null and Camera.main is not available. Cannot check focus.");
                    return false;
                }
            }

            return Vector3.Dot(target.forward.normalized, (julie.transform.parent.position - target.position).normalized) > focusTolerance;
        }

        /// <summary>
        /// Will stop Julie Walking animation and coroutine when she is focus by the user.
        /// </summary>
        public void StopWhenFocused()
        {
            // Auto-assign target to Camera.main if not set
            if (target == null)
            {
                if (Camera.main != null)
                {
                    target = Camera.main.transform;
                }
                else
                {
                    Debug.LogWarning("JulieManager: target is null and Camera.main is not available. Cannot check focus.");
                    return;
                }
            }

            if (julie.GetBool("isWalking") && Vector3.Dot(target.forward.normalized, (julie.transform.parent.position - target.position).normalized) > focusTolerance)
            {
                StopAllCoroutines();
                julie.SetBool("isWalking", false);               
            }
        }


        /// <summary>
        /// Place julie around the player area
        /// </summary>
        public void Init(bool forceReinit = false)
        {
            if(forceReinit)
            {
                currentState = JulieState.INIT;
            }
            
            if (julie == default)
            {
                currentState = JulieState.INIT;
                julie = Instantiate(juliePrefab, this.transform).GetComponentInChildren<Animator>();
            }

            
            if (PlaySpace.Instance && currentState != JulieState.READY)
            {
                julie.Play("Julie@Appearing");
                GameObject target = PlaySpace.Instance.GetCenter();
                AreaSegment segment = PlaySpace.Instance.GetFacingSegment();
                julie.transform.parent.position = segment.V1.transform.position + (segment.V1.transform.position - target.transform.position).normalized * 0.5f;        
                destination = segment.V2.transform.position - (segment.V2.transform.position - target.transform.position).normalized * 0.25f;

                julie.transform.parent.LookAt(new Vector3(target.transform.position.x, julie.transform.parent.position.y, target.transform.position.z), Vector3.up);
                Invoke("WalkTowardDestination", 3f);// WalkToward(destination);
                currentState = JulieState.WALKING;
                
            }
            //target.position = julie.transform.parent.position;
        }

        /// <summary>
        /// Will try to play the current animation
        /// </summary>
        /// <param name="animation"></param>
        public void Play(JulieAnimation animation)
        {
            if (julie.GetBool("isWalking"))
            {
                //julie.SetBool("isWalking", false);
            }
            julie.SetTrigger(animation.ToString());
        }

        /// <summary>
        /// Iterate one time toward a specific target
        /// </summary>
        public void FollowTarget()
        {

            if (julie.transform.parent.position != destination)
            {
                this.julie.SetBool("isWalking", true);
                Vector3 initialDirection = (destination - julie.transform.parent.position).normalized;
                Vector3 offset = initialDirection * Time.deltaTime * speed;
                julie.transform.parent.LookAt(new Vector3(destination.x, julie.transform.parent.position.y, destination.z), Vector3.up);
                if (offset.sqrMagnitude > (destination - julie.transform.parent.position).sqrMagnitude)
                {
                    julie.transform.parent.position = destination;
                    return;
                }
                julie.transform.parent.position += offset;
            }
            else
            {
                this.julie.SetBool("isWalking", false);
            }
        }

        private void WalkTowardDestination()
        {
            this.WalkToward(destination);
        }

        public void UpdateShirtColor(GameCreator.BalloonColor color)
        {
            if(julie != default)
            {
                //julie.GetComponentInChildren<SkinnedMeshRenderer>().materials[0].SetTexture("_MainTex",GetTextureFromColor(color));
                julie.GetComponentInChildren<SkinnedMeshRenderer>().materials[0]
                    .SetColor("_BaseColor", Color.magenta);
            }
        }

        private Texture2D GetTextureFromColor(GameCreator.BalloonColor color)
        {
            switch (color)
            {
                case GameCreator.BalloonColor.BLUE:
                    return texs[0];
                case GameCreator.BalloonColor.RED:
                    return texs[1];
                case GameCreator.BalloonColor.GREEN:
                    return texs[2];
                case GameCreator.BalloonColor.YELLOW:
                    return texs[3];
                default:
                    return texs[4];
            }
        }


        /// <summary>
        /// Walk toward a specific point in space with a coroutine
        /// </summary>
        /// <param name="target">Intended destination</param>
        private void WalkToward(Vector3 target)
        {
            Vector3[] steps = new Vector3[1];
            steps[0] = target;

            //Debug.Log("Walk toward :" + target);
            if(!julie.GetBool("isWalking"))
                StartCoroutine(Walk(steps));
        }

        /// <summary>
        /// Walk toward a specific point in space with a coroutine
        /// </summary>
        /// <param name="target">Intended destination</param>
        private Vector3[] GetPathToBestAngle()
        {
            Vector3 center = PlaySpace.Instance.GetCenter().transform.position;
            List<Vector3> steps = new List<Vector3>();
            PlaySpace.Instance.Segments.ForEach((segment) => { steps.Add(segment.V1.transform.position); });
            Vector3 playerPos = Camera.main.transform.position;
            bool clockwise = (Vector2.SignedAngle(Camera.main.transform.forward, (julie.transform.parent.position - playerPos).normalized) < 0);
            bool correctOrder = (Vector2.SignedAngle(steps[0] - center,steps[1] -center) < 0);

            float minDistance = Mathf.Infinity;
            Vector3 closestStep = default;
            foreach (Vector3 step in steps)
            {
                float current = Vector3.Distance(step, julie.transform.parent.position);
                if (current < minDistance)
                {
                    closestStep = step;
                    minDistance = current;
                }
            }
            int index = steps.IndexOf(closestStep);
            bool sameDirection = (Vector2.SignedAngle((closestStep - playerPos).normalized, (julie.transform.parent.position - playerPos).normalized) < 0);

            if (clockwise)
            {
                int offset = (sameDirection != clockwise) ? 0 : 1;
                List<Vector3> items = steps.GetRange(0, index+ offset);
                steps.RemoveRange(0, index+ offset);
                items.Reverse();
                steps.InsertRange(0,items);
            }
            else
            {
                int offset = (sameDirection == !clockwise) ? 0 : 1;
                Vector3[] items = steps.GetRange(0, index+ offset).ToArray();
                steps.RemoveRange(0, index+ offset);
                steps.AddRange(items);
                //steps.Reverse();
            }
            Debug.Log("Clockwise : " + clockwise + " SameDirection : " + sameDirection + " Correct Order :" + correctOrder);

            return steps.ToArray();
        }


        #endregion

        #region Coroutines

        /// <summary>
        /// Coroutine for walking along a predetermined path.
        /// </summary>
        /// <param name="steps"></param>
        /// <returns></returns>
        private IEnumerator Walk(Vector3[] steps)
        {

            Vector3 initialPosition = julie.transform.parent.position;
            this.julie.SetBool("isWalking",true);
            foreach(Vector3 step in steps)
            {
                Vector3 initialDirection =  (step - julie.transform.parent.position).normalized;
                while (julie.transform.parent.position != step)
                {
                    if (julie.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Julie@Walking")//!julie.GetBool("isWalking"))
                    {
                        if (!this.julie.GetBool("isWalking"))
                            this.julie.SetBool("isWalking", true);
                        //Debug.Log("Wait for end animation :"+ julie.GetCurrentAnimatorClipInfo(0)[0].clip.name);
                        yield return null;
                        continue;
                        //yield break;
                    }

                    //Look better with this line, but can introduce an error with the threshold getting julie to turn around one of the point for a long period of time.
                    //Vector3 initialDirection = julie.transform.parent.forward.normalized;
                    Vector3 offset = initialDirection * Time.deltaTime * speed;
                    //julie.transform.parent.LookAt(new Vector3(step.x, julie.transform.parent.position.y, step.z), Vector3.up);
                    julie.transform.parent.rotation = Quaternion.Slerp(julie.transform.parent.rotation, Quaternion.LookRotation(new Vector3(step.x, julie.transform.parent.position.y, step.z) - julie.transform.parent.position, Vector3.up), 0.1f);
                    if (offset.sqrMagnitude > (step - julie.transform.parent.position).sqrMagnitude)
                    {
                        julie.transform.parent.position = step;
                        break;
                    }
                    julie.transform.parent.position += offset;
                    yield return null;
                }
                yield return null;
            }

            this.julie.SetBool("isWalking", false);
            currentState = JulieState.READY;
            yield return null;
        }

        #endregion


    }

}
