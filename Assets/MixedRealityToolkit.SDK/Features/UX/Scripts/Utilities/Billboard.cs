// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.UI
{
    /// <summary>
    /// The Billboard class implements the behaviors needed to keep a GameObject oriented towards the user.
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        /// <summary>
        /// The axis about which the object will rotate.
        /// </summary>
        public PivotAxis PivotAxis
        {
            get { return pivotAxis; }
            set { pivotAxis = value; }
        }

        [Tooltip("Specifies the axis about which the object will rotate.")]
        [SerializeField]
        private PivotAxis pivotAxis = PivotAxis.XY;

        /// <summary>
        /// The target we will orient to. If no target is specified, the main camera will be used.
        /// </summary>
        public Transform TargetTransform
        {
            get { return targetTransform; }
            set { targetTransform = value; }
        }

        [Tooltip("Specifies the target we will orient to. If no target is specified, the main camera will be used.")]
        [SerializeField]
        private Transform targetTransform;

        [Tooltip("Minimum distance (in meters) before billboard stops rotating. Default 0.5m (50cm).")]
        [SerializeField]
        private float minDistanceToRotate = 0.5f;

        private void OnEnable()
        {
            if (targetTransform == null)
            {
                targetTransform = CameraCache.Main.transform;
            }
        }

        /// <summary>
        /// Keeps the object facing the camera.
        /// </summary>
        private void Update()
        {
            if (targetTransform == null)
            {
                return;
            }

            // Get a Vector that points from the target to the main camera.
            Vector3 directionToTarget = targetTransform.position - transform.position;

            // If we are too close to the panel, stop rotating to avoid disorienting movement
            float distanceToTarget = directionToTarget.magnitude;
            if (distanceToTarget < minDistanceToRotate)
            {
                return;
            }

            bool useCameraAsUpVector = true;

            // Adjust for the pivot axis.
            switch (pivotAxis)
            {
                case PivotAxis.X:
                    directionToTarget.x = 0.0f;
                    useCameraAsUpVector = false;
                    break;

                case PivotAxis.Y:
                    directionToTarget.y = 0.0f;
                    useCameraAsUpVector = false;
                    break;

                case PivotAxis.Z:
                    directionToTarget.x = 0.0f;
                    directionToTarget.y = 0.0f;
                    break;

                case PivotAxis.XY:
                    useCameraAsUpVector = false;
                    break;

                case PivotAxis.XZ:
                    directionToTarget.x = 0.0f;
                    break;

                case PivotAxis.YZ:
                    directionToTarget.y = 0.0f;
                    break;

                case PivotAxis.Free:
                default:
                    // No changes needed.
                    break;
            }

            // If we are right next to the camera the rotation is undefined. 
            if (directionToTarget.sqrMagnitude < 0.001f)
            {
                return;
            }

            // Calculate and apply the rotation required to reorient the object
            if (useCameraAsUpVector)
            {
                transform.rotation = Quaternion.LookRotation(-directionToTarget, CameraCache.Main.transform.up);
            }
            else
            {
                transform.rotation = Quaternion.LookRotation(-directionToTarget);
            }
        }
    }
}