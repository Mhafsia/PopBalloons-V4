using UnityEngine;
using Microsoft.MixedReality.Toolkit;

namespace PopBalloons.Utilities
{
    /// <summary>
    /// Automatically disable MRTK diagnostics profiler in production builds
    /// </summary>
    public class DisableProfilerInBuild : MonoBehaviour
    {
        [SerializeField] private bool disableInEditor = false;
        [SerializeField] private bool disableInBuild = true;

        private void Start()
        {
            bool shouldDisable = false;

#if UNITY_EDITOR
            shouldDisable = disableInEditor;
#else
            shouldDisable = disableInBuild;
#endif

            if (shouldDisable && CoreServices.DiagnosticsSystem != null)
            {
                CoreServices.DiagnosticsSystem.ShowProfiler = false;
                CoreServices.DiagnosticsSystem.ShowDiagnostics = false;
                Debug.Log("Visual Profiler disabled for better performance");
            }
        }

        /// <summary>
        /// Toggle profiler visibility (can be called from UI button)
        /// </summary>
        public void ToggleProfiler()
        {
            if (CoreServices.DiagnosticsSystem != null)
            {
                bool newState = !CoreServices.DiagnosticsSystem.ShowProfiler;
                CoreServices.DiagnosticsSystem.ShowProfiler = newState;
                Debug.Log($"Visual Profiler: {(newState ? "ON" : "OFF")}");
            }
        }

        /// <summary>
        /// Hotkey to toggle profiler (useful for debugging on HoloLens)
        /// </summary>
        private void Update()
        {
            // Press 'P' key to toggle profiler
            if (Input.GetKeyDown(KeyCode.P))
            {
                ToggleProfiler();
            }
        }
    }
}
