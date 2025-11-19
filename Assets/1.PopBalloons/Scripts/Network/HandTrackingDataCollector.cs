using System;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;

namespace PopBalloons.Network
{
    /// <summary>
    /// Collects hand tracking data from MRTK for both left and right hands.
    /// Captures joint positions, rotations, and timestamps for analysis.
    /// </summary>
    public class HandTrackingDataCollector : MonoBehaviour
    {
        [Header("Tracking Settings")]
        [SerializeField] private bool trackLeftHand = true;
        [SerializeField] private bool trackRightHand = true;
        [SerializeField] private float captureInterval = 0.1f; // Capture every 100ms (10 Hz)

        private float lastCaptureTime = 0f;

        /// <summary>
        /// Get current hand tracking data for both hands
        /// </summary>
        public HandTrackingFrame GetCurrentFrame()
        {
            var frame = new HandTrackingFrame
            {
                timestamp = Time.time,
                timestampMs = (long)(Time.time * 1000),
                leftHand = trackLeftHand ? GetHandData(Handedness.Left) : null,
                rightHand = trackRightHand ? GetHandData(Handedness.Right) : null
            };

            // Debug: Log if hands are detected
            if (frame.leftHand != null || frame.rightHand != null)
            {
                string msg = "🖐️ Hands detected: ";
                if (frame.leftHand != null) msg += "LEFT ";
                if (frame.rightHand != null) msg += "RIGHT ";
                Debug.Log(msg);
            }

            return frame;
        }

        /// <summary>
        /// Get hand tracking data for a specific hand
        /// </summary>
        private HandData GetHandData(Handedness handedness)
        {
            // Check if hand is tracked
            if (!HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, handedness, out MixedRealityPose palmPose))
            {
                Debug.Log($"❌ {handedness} hand NOT tracked - Palm joint not found");
                return null; // Hand not tracked
            }

            Debug.Log($"✅ {handedness} hand IS tracked - capturing {System.Enum.GetValues(typeof(TrackedHandJoint)).Length} joints");

            var handData = new HandData
            {
                handedness = handedness.ToString(),
                isTracked = true,
                joints = new List<JointData>()
            };

            // Capture all hand joints
            foreach (TrackedHandJoint joint in Enum.GetValues(typeof(TrackedHandJoint)))
            {
                if (HandJointUtils.TryGetJointPose(joint, handedness, out MixedRealityPose jointPose))
                {
                    handData.joints.Add(new JointData
                    {
                        jointName = joint.ToString(),
                        position = new Vector3Data
                        {
                            x = jointPose.Position.x,
                            y = jointPose.Position.y,
                            z = jointPose.Position.z
                        },
                        rotation = new QuaternionData
                        {
                            x = jointPose.Rotation.x,
                            y = jointPose.Rotation.y,
                            z = jointPose.Rotation.z,
                            w = jointPose.Rotation.w
                        }
                    });
                }
            }

            return handData;
        }

        /// <summary>
        /// Should capture this frame based on interval
        /// </summary>
        public bool ShouldCapture()
        {
            if (Time.time - lastCaptureTime >= captureInterval)
            {
                lastCaptureTime = Time.time;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get all MRTK tracked hand joints as a readable list
        /// </summary>
        public static List<string> GetAllJointNames()
        {
            var joints = new List<string>();
            foreach (TrackedHandJoint joint in Enum.GetValues(typeof(TrackedHandJoint)))
            {
                joints.Add(joint.ToString());
            }
            return joints;
        }
    }

    #region Data Structures

    [Serializable]
    public class HandTrackingFrame
    {
        public float timestamp;          // Unity Time.time (seconds since app start)
        public long timestampMs;         // Milliseconds timestamp
        public HandData leftHand;
        public HandData rightHand;
    }

    [Serializable]
    public class HandData
    {
        public string handedness;        // "Left" or "Right"
        public bool isTracked;
        public List<JointData> joints;
    }

    [Serializable]
    public class JointData
    {
        public string jointName;         // e.g., "Palm", "ThumbTip", "IndexTip", etc.
        public Vector3Data position;     // 3D position in world space (meters)
        public QuaternionData rotation;  // Rotation as quaternion
    }

    [Serializable]
    public class Vector3Data
    {
        public float x;
        public float y;
        public float z;
    }

    [Serializable]
    public class QuaternionData
    {
        public float x;
        public float y;
        public float z;
        public float w;
    }

    #endregion
}
