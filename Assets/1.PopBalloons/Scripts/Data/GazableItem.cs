using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.Serialization;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using PopBalloons.Utilities;

namespace PopBalloons.Data
{

public class GazableItem : InputSystemGlobalHandlerListener, IMixedRealityPointerHandler
    {
        [SerializeField]
        private GazableElement itemType;

        //private float gazeTimestamp;
        private string gazeTimestamp;
        
        [Tooltip("Duration in seconds that the user needs to keep looking at the target to select it via dwell activation.")]
        [Range(0, 10)]
        [SerializeField]
        private float dwellTimeInSec = 0.8f;

        /// <summary>
        /// Returns true if the user looks at the target or more specifically when the eye gaze ray intersects 
        /// with the target's bounding box.
        /// </summary>
        public bool IsLookedAt { get; private set; }

        /// <summary>
        /// Returns true if the user has been looking at the target for a certain amount of time specified by dwellTimeInSec.
        /// </summary>
        public bool IsDwelledOn { get; private set; } = false;

        private DateTime lookAtStartTime;

        /// <summary>
        /// Duration in milliseconds to indicate that if more time than this passes without new eye tracking data, then timeout. 
        /// </summary>
        private float EyeTrackingTimeoutInMilliseconds = 200;

        /// <summary>
        /// The time stamp received from the eye tracker to indicate when the eye tracking signal was last updated.
        /// </summary>
        private static DateTime lastEyeSignalUpdateTimeFromET = DateTime.MinValue;

        /// <summary>
        /// The time stamp from the eye tracker has its own time frame, which makes it difficult to compare to local times. 
        /// </summary>
        private static DateTime lastEyeSignalUpdateTimeLocal = DateTime.MinValue;

        public static GameObject LookedAtTarget { get; private set; }
        public static GazableItem LookedAtEyeTarget { get; private set; }
        public static Vector3 LookedAtPoint { get; private set; }

        //private void Awake()
        //{
        //    GazeManager.Instance.RegisterGazeData();
        //}

        protected override void Start()
        {
            base.Start();
            IsLookedAt = false;
            LookedAtTarget = null;
            LookedAtEyeTarget = null;
            gazeTimestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fff");
        }

        private void Update()
        {
            var eyeGazeProvider = CoreServices.InputSystem?.EyeGazeProvider;
            // Try to manually poll the eye tracking data
            if (eyeGazeProvider != null
                && eyeGazeProvider.UseEyeTracking
                && eyeGazeProvider.IsEyeGazeValid)
            {
                UpdateHitTarget();
                // Ensure GazeManager and LookedAtTarget are available before using them
                if (GazeManager.Instance != null && LookedAtTarget != null)
                {
                    StartCoroutine(GazeManager.Instance.RegisterGazeData(0.001f, LookedAtTarget.name));
                }

                bool isLookedAtNow = (LookedAtTarget == this.gameObject);

                if (IsLookedAt && (!isLookedAtNow))
                {
                    // Stopped looking at the target
                    OnEyeFocusStop();
                }
                else if ((!IsLookedAt) && (isLookedAtNow))
                {
                    // Started looking at the target
                    OnEyeFocusStart();
                }
                else if (IsLookedAt && (isLookedAtNow))
                {
                    // Keep looking at the target
                    OnEyeFocusStay();
                }
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            // Safely handle stopping focus in case objects have been destroyed
            try
            {
                OnEyeFocusStop();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"OnEyeFocusStop threw an exception during OnDisable: {ex.Message}");
            }
        }

        /// <inheritdoc />
        protected override void RegisterHandlers()
        {
            CoreServices.InputSystem?.RegisterHandler<IMixedRealityPointerHandler>(this);
        }

        /// <inheritdoc />
        protected override void UnregisterHandlers()
        {
            CoreServices.InputSystem?.UnregisterHandler<IMixedRealityPointerHandler>(this);
        }

        private void UpdateHitTarget()
        {
            var eyeGazeProvider = CoreServices.InputSystem?.EyeGazeProvider;
            if (eyeGazeProvider != null)
            {
                if (lastEyeSignalUpdateTimeFromET != eyeGazeProvider.Timestamp)
                {
                    lastEyeSignalUpdateTimeFromET = eyeGazeProvider.Timestamp;
                    lastEyeSignalUpdateTimeLocal = DateTime.UtcNow;

                    // ToDo: Handle raycasting layers
                    RaycastHit hitInfo = default(RaycastHit);
                    Ray lookRay = new Ray(eyeGazeProvider.GazeOrigin, eyeGazeProvider.GazeDirection.normalized);
                    bool isHit = UnityEngine.Physics.Raycast(lookRay, out hitInfo);

                    if (isHit)
                    {
                        LookedAtTarget = hitInfo.collider.gameObject;
                        LookedAtEyeTarget = LookedAtTarget.GetComponent<GazableItem>();
                        LookedAtPoint = hitInfo.point;
                    }
                    else
                    {
                        LookedAtTarget = null;
                        LookedAtEyeTarget = null;
                    }
                }
                else if ((DateTime.UtcNow - lastEyeSignalUpdateTimeLocal).TotalMilliseconds > EyeTrackingTimeoutInMilliseconds)
                {
                    LookedAtTarget = null;
                    LookedAtEyeTarget = null;
                }
            }
        }

        protected void OnEyeFocusStart()
        {
            lookAtStartTime = DateTime.UtcNow;
            IsLookedAt = true;
            //gazeTimestamp = TimerManager.GetTimeStamp();
            gazeTimestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fff");
        }

        protected void OnEyeFocusStay()
        {
            if ((!IsDwelledOn) && (DateTime.UtcNow - lookAtStartTime).TotalSeconds > dwellTimeInSec)
            {
                OnEyeFocusDwell();
            }
        }

        protected void OnEyeFocusDwell()
        {
            IsDwelledOn = true;
        }

        protected void OnEyeFocusStop()
        {
            string targetType = "";
            IsDwelledOn = false;
            IsLookedAt = false;
            float duration = (float)(DateTime.UtcNow - lookAtStartTime).TotalSeconds; // Time.time - gazeTimestamp;
            string targetName = LookedAtTarget != null ? LookedAtTarget.name : "Unknown";
            string datetime = lookAtStartTime.ToString("yyyy-MM-ddTHH:mm:ss.fff");
            if (LookedAtEyeTarget != null && LookedAtEyeTarget.itemType != null)
            {
                targetType = LookedAtEyeTarget.itemType.ToString();
            }
            else
            {
                targetType = "TargetNotDetected";
            }

            if (GazeManager.Instance != null)
            {
                GazeManager.Instance.RegisterGazeItemData(targetType, gazeTimestamp, duration, datetime, targetName);
            }
            // GazeManager not available - this is normal in editor or when gaze tracking is disabled
        }

        void IMixedRealityPointerHandler.OnPointerUp(MixedRealityPointerEventData eventData) { }

        void IMixedRealityPointerHandler.OnPointerDown(MixedRealityPointerEventData eventData) { }

        void IMixedRealityPointerHandler.OnPointerDragged(MixedRealityPointerEventData eventData) { }

        void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData){ }

    }

}