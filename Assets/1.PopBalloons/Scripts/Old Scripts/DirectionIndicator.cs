using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;



/// <summary>
/// this script is copied from HTK
/// </summary>
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
        [SerializeField]
        // Cache the Material to prevent material leak.
        private Material indicatorMaterial;

        // Check if the cursor direction indicator is visible.
        private bool isDirectionIndicatorVisible;


        public IMixedRealityCursor cursor;

        public void Awake()
        {
            // Try to find the MRTK primary cursor first
            try
            {
                var inputSystem = Microsoft.MixedReality.Toolkit.CoreServices.InputSystem;
                if (inputSystem != null)
                {
                    var primaryCursor = inputSystem.FocusProvider?.PrimaryPointer?.Controller as MonoBehaviour;
                    if (primaryCursor != null)
                    {
                        Cursor = primaryCursor.gameObject;
                    }
                }
            }
            catch
            {
                // Ignore any issues trying to access the input system here; fall back to Find below
            }

            if (Cursor == null)
            {
                Cursor = GameObject.Find("Pointer");
            }

            // Do not warn if Cursor is null here; lazy-init in Update() will attempt to find it when available.

            if (DirectionIndicatorObject == null)
            {
                // Try to load a default indicator prefab from Resources/DefaultDirectionIndicator
                var loaded = Resources.Load<GameObject>("DefaultDirectionIndicator");
                if (loaded != null)
                {
                    DirectionIndicatorObject = loaded;
                }
                else
                {
                    // As a last resort, create a simple primitive to act as a visual indicator (small cylinder)
                    var fallback = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    fallback.name = "DirectionIndicatorFallback";
                    // Make it thin and short so it resembles an indicator shaft
                    fallback.transform.localScale = new Vector3(0.05f, 0.15f, 0.05f);
                    // Ensure fallback does not interfere with physics
                    foreach (var c in fallback.GetComponents<Collider>()) Destroy(c);
                    // Optionally tint it so it's visible
                    var rend = fallback.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        rend.material = new Material(Shader.Find("Standard"));
                        rend.material.color = Color.yellow;
                    }

                    // Use this fallback as the template to instantiate (we keep it inactive)
                    fallback.SetActive(false);
                    DirectionIndicatorObject = fallback;
                }
            }

            // Instantiate the direction indicator (guarded)
            var usedTemplateName = DirectionIndicatorObject.name;
            DirectionIndicatorObject = InstantiateDirectionIndicator(DirectionIndicatorObject);

            if (DirectionIndicatorObject == null)
            {
                UnityEngine.Debug.LogWarning("Direction Indicator failed to instantiate. Disabling indicator behavior.");
            }
            else
            {
                UnityEngine.Debug.Log($"DirectionIndicator instantiated using template: {usedTemplateName}");
            }
        }




        public void OnDestroy()
        {
            DestroyImmediate(indicatorMaterial);
            Destroy(DirectionIndicatorObject);
        }

        private GameObject InstantiateDirectionIndicator(GameObject directionIndicator)
        {
            if (directionIndicator == null)
            {
                return null;
            }

            GameObject indicator = Instantiate(directionIndicator);

            // Set local variables for the indicator.
            directionIndicatorDefaultRotation = indicator.transform.rotation;
            directionIndicatorRenderer = indicator.GetComponent<Renderer>();

            // Start with the indicator disabled.
            directionIndicatorRenderer.enabled = false;

            // Remove any colliders and rigidbodies so the indicators do not interfere with Unity's physics system.
            foreach (Collider indicatorCollider in indicator.GetComponents<Collider>())
            {
                Destroy(indicatorCollider);
            }

            foreach (Rigidbody rigidBody in indicator.GetComponents<Rigidbody>())
            {
                Destroy(rigidBody);
            }

            indicatorMaterial = directionIndicatorRenderer.material;

            return indicator;
        }

        private float valeurZ = 0;
        //private Vector3 offsetZ = new Vector3(0,0,0);
        private bool done = true;

        public void Update()
        {
            // Lazy-init the Cursor in case MRTK initializes it after this object's Awake
            if (Cursor == null)
            {
                try
                {
                    var primary = Microsoft.MixedReality.Toolkit.CoreServices.InputSystem?.FocusProvider?.PrimaryPointer;
                    var mb = primary as MonoBehaviour;
                    if (mb != null)
                    {
                        Cursor = mb.gameObject;
                    }
                }
                catch { }

                if (Cursor == null)
                {
                    Cursor = GameObject.Find("Pointer");
                }
            }

            if (done)
            {
                valeurZ += Time.deltaTime;
                if (valeurZ >= 1)
                    done = false;
            }
            if (!done)
            {
                valeurZ -= Time.deltaTime;
                if (valeurZ <= 0.02f)
                    done = true;
            }

            if (DirectionIndicatorObject == null)
            {
                return;
            }
            Camera mainCamera = Camera.main;
            // Direction from the Main Camera to this script's parent gameObject.
            Vector3 camToObjectDirection = gameObject.transform.position - mainCamera.transform.position;
            camToObjectDirection.Normalize();

            // The cursor indicator should only be visible if the target is not visible.
            isDirectionIndicatorVisible = debugForceShow ? true : !IsTargetVisible(mainCamera);
            if (directionIndicatorRenderer != null)
            {
                if (directionIndicatorRenderer.enabled != isDirectionIndicatorVisible)
                {
                    Debug.Log($"DirectionIndicator visibility for '{gameObject.name}' changed to {isDirectionIndicatorVisible}");
                }
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
                //offsetZ = new Vector3(0, 0, valeurZ);
                //Debug.Log(valeurZ);

                DirectionIndicatorObject.transform.position = position;// + offsetZ;
                DirectionIndicatorObject.transform.rotation = rotation;
                DirectionIndicatorObject.transform.position += DirectionIndicatorObject.transform.forward.normalized * MetersFromCursor * Mathf.SmoothStep(0, 1, valeurZ);
            }
        }



        private bool IsTargetVisible(Camera mainCamera)
        {
            // This will return true if the target's mesh is within the Main Camera's view frustums.
            Vector3 targetViewportPosition = mainCamera.WorldToViewportPoint(gameObject.transform.position);
            return (targetViewportPosition.x > VisibilitySafeFactor && targetViewportPosition.x < 1 - VisibilitySafeFactor &&
                    targetViewportPosition.y > VisibilitySafeFactor && targetViewportPosition.y < 1 - VisibilitySafeFactor &&
                    targetViewportPosition.z > 0);
        }

        private void GetDirectionIndicatorPositionAndRotation(Vector3 camToObjectDirection, Transform cameraTransform, out Vector3 position, out Quaternion rotation)
        {
            // Find position:
            // Save the cursor transform position in a variable.
            Vector3 origin = Camera.main.transform.position + Camera.main.transform.forward * 1.5f;//Cursor.transform.position;

            // Project the camera to target direction onto the screen plane.
            Vector3 cursorIndicatorDirection = Vector3.ProjectOnPlane(camToObjectDirection, -1 * cameraTransform.forward);
            cursorIndicatorDirection.Normalize();

            // If the direction is 0, set the direction to the right.
            // This will only happen if the camera is facing directly away from the target.
            if (cursorIndicatorDirection == Vector3.zero)
            {
                cursorIndicatorDirection = cameraTransform.right;
            }

            // The final position is translated from the center of the screen along this direction vector.
            position = origin + cursorIndicatorDirection * MetersFromCursor;

            // Find the rotation from the facing direction to the target object.
            rotation = Quaternion.LookRotation(cameraTransform.forward, cursorIndicatorDirection) * directionIndicatorDefaultRotation;
        }
    }

}

