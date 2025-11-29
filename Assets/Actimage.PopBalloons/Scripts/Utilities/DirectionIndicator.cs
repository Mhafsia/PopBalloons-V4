using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;

namespace PopBalloons
{
    public class DirectionIndicator : MonoBehaviour
    {
        [Tooltip("The Cursor object the direction indicator will be positioned around.")]
        public GameObject Cursor;

        [Tooltip("Model to display the direction to the object this script is attached to.")]
        public GameObject DirectionIndicatorObject;

        [Tooltip("Force showing the direction indicator for debugging (overrides visibility checks).")]
        public bool debugForceShow = false;

        [Tooltip("Allowable percentage inside the holographic frame to continue to show a directional indicator.")]
        [Range(-0.3f, 0.3f)]
        public float VisibilitySafeFactor = 0.1f;

        [Tooltip("Multiplier to decrease the distance from the cursor center an object is rendered to keep it in view.")]
        [Range(0.0f, 1.0f)]
        public float MetersFromCursor = 0.1f;

        // The default rotation of the cursor direction indicator.
        private Quaternion directionIndicatorDefaultRotation = Quaternion.identity;

        // Cache the MeshRenderer for the on-cursor indicator since it will be enabled and disabled frequently.
        private Renderer directionIndicatorRenderer;
        
        // Cache the Material to prevent material leak.
        private Material indicatorMaterial;

        // Check if the cursor direction indicator is visible.
        private bool isDirectionIndicatorVisible;
        
        private bool isInitialized = false;

        public void Awake()
        {
            // Defer initialization to Start to ensure other objects are ready
        }

        public void Start()
        {
            InitializeIndicator();
        }

        private void InitializeIndicator()
        {
            if (isInitialized) return;

            // 1. Find Cursor
            if (Cursor == null)
            {
                // Try MRTK first
                var inputSystem = Microsoft.MixedReality.Toolkit.CoreServices.InputSystem;
                if (inputSystem != null)
                {
                    var primaryCursor = inputSystem.FocusProvider?.PrimaryPointer?.Controller as MonoBehaviour;
                    if (primaryCursor != null)
                    {
                        Cursor = primaryCursor.gameObject;
                    }
                }

                // Fallback search
                if (Cursor == null)
                {
                    Cursor = GameObject.Find("Pointer");
                }
            }

            // 2. Setup Indicator Object
            if (DirectionIndicatorObject == null)
            {
                // Try load resource
                var loaded = Resources.Load<GameObject>("DefaultDirectionIndicator");
                if (loaded != null)
                {
                    DirectionIndicatorObject = loaded;
                }
                else
                {
                    // Create simple fallback
                    var fallback = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    fallback.name = "DirectionIndicatorFallback";
                    fallback.transform.localScale = new Vector3(0.05f, 0.15f, 0.05f);
                    
                    // Cleanup physics
                    foreach (var c in fallback.GetComponents<Collider>()) Destroy(c);
                    
                    // Color
                    var rend = fallback.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        rend.material = new Material(Shader.Find("Standard"));
                        rend.material.color = Color.yellow;
                    }

                    fallback.SetActive(false);
                    DirectionIndicatorObject = fallback;
                }
            }

            // 3. Instantiate the indicator
            // SAFETY CHECK: Ensure we are not instantiating ourselves!
            if (DirectionIndicatorObject == this.gameObject)
            {
                Debug.LogError("DirectionIndicator: Infinite recursion detected! DirectionIndicatorObject cannot be the object itself.");
                this.enabled = false;
                return;
            }

            GameObject instantiatedIndicator = Instantiate(DirectionIndicatorObject);
            instantiatedIndicator.name = "Indicator_Instance_" + gameObject.name;
            
            // Setup references
            directionIndicatorDefaultRotation = instantiatedIndicator.transform.rotation;
            directionIndicatorRenderer = instantiatedIndicator.GetComponent<Renderer>();
            
            // Disable initially
            if (directionIndicatorRenderer != null)
            {
                directionIndicatorRenderer.enabled = false;
                indicatorMaterial = directionIndicatorRenderer.material;
            }

            // Remove physics from indicator
            foreach (Collider c in instantiatedIndicator.GetComponents<Collider>()) Destroy(c);
            foreach (Rigidbody r in instantiatedIndicator.GetComponents<Rigidbody>()) Destroy(r);

            // Update reference to the INSTANCE
            DirectionIndicatorObject = instantiatedIndicator;
            
            isInitialized = true;
        }

        public void OnDestroy()
        {
            if (indicatorMaterial != null) DestroyImmediate(indicatorMaterial);
            if (DirectionIndicatorObject != null) Destroy(DirectionIndicatorObject);
        }

        private float valeurZ = 0;
        private bool done = true;

        public void Update()
        {
            if (!isInitialized) return;
            
            // Retry finding cursor if missing (but not every frame if possible - simple check here)
            if (Cursor == null)
            {
                // Simple retry logic
                if (Time.frameCount % 60 == 0) // Check once per second approx
                {
                     Cursor = GameObject.Find("Pointer");
                }
                
                if (Cursor == null) return;
            }
            
            if (DirectionIndicatorObject == null) return;

            // Animation logic
            if (done)
            {
                valeurZ += Time.deltaTime;
                if (valeurZ >= 1) done = false;
            }
            else
            {
                valeurZ -= Time.deltaTime;
                if (valeurZ <= 0.02f) done = true;
            }

            Camera mainCamera = Camera.main;
            if (mainCamera == null) return;

            // Direction calculation
            Vector3 camToObjectDirection = gameObject.transform.position - mainCamera.transform.position;
            camToObjectDirection.Normalize();

            // Visibility check
            isDirectionIndicatorVisible = debugForceShow ? true : !IsTargetVisible(mainCamera);

            if (directionIndicatorRenderer != null)
            {
                directionIndicatorRenderer.enabled = isDirectionIndicatorVisible;
            }

            if (isDirectionIndicatorVisible)
            {
                Vector3 position;
                Quaternion rotation;
                GetDirectionIndicatorPositionAndRotation(
                    camToObjectDirection,
                    mainCamera.transform,
                    out position,
                    out rotation);

                DirectionIndicatorObject.transform.position = position;
                DirectionIndicatorObject.transform.rotation = rotation;
                
                // Animation offset
                DirectionIndicatorObject.transform.position += DirectionIndicatorObject.transform.forward.normalized * MetersFromCursor * Mathf.SmoothStep(0, 1, valeurZ);
            }
        }

        private bool IsTargetVisible(Camera mainCamera)
        {
            Vector3 targetViewportPosition = mainCamera.WorldToViewportPoint(gameObject.transform.position);
            return (targetViewportPosition.x > VisibilitySafeFactor && targetViewportPosition.x < 1 - VisibilitySafeFactor &&
                    targetViewportPosition.y > VisibilitySafeFactor && targetViewportPosition.y < 1 - VisibilitySafeFactor &&
                    targetViewportPosition.z > 0);
        }

        private void GetDirectionIndicatorPositionAndRotation(Vector3 camToObjectDirection, Transform cameraTransform, out Vector3 position, out Quaternion rotation)
        {
            Vector3 origin = cameraTransform.position + cameraTransform.forward * 1.5f;
            Vector3 cursorIndicatorDirection = Vector3.ProjectOnPlane(camToObjectDirection, -1 * cameraTransform.forward);
            cursorIndicatorDirection.Normalize();

            if (cursorIndicatorDirection == Vector3.zero)
            {
                cursorIndicatorDirection = cameraTransform.right;
            }

            position = origin + cursorIndicatorDirection * MetersFromCursor;
            rotation = Quaternion.LookRotation(cameraTransform.forward, cursorIndicatorDirection) * directionIndicatorDefaultRotation;
        }
    }
}

