using UnityEngine;
using PopBalloons.UI;

namespace PopBalloons.Utilities
{
    /// <summary>
    /// This script forces the correct configuration for Back Buttons at runtime
    /// to avoid inspector configuration errors.
    /// </summary>
    public class FixButtonConfig : MonoBehaviour
    {
        void Start()
        {
            Invoke("ApplyFixes", 1.0f); // Wait for initialization
        }

        void ApplyFixes()
        {
            Debug.Log("[FixButtonConfig] Applying runtime fixes to Back Buttons...");

            // Only fix in-game back buttons to return to their respective mode menus
            // We do NOT fix the Mode_Selection back button as it should remain as PROFILE by default
            
            // 1. Fix Motricity Game Back Button -> Should go to MOBILITY
            var motricityPanel = FindObjectOfType<MotricityPanel>();
            if (motricityPanel != null)
            {
                var buttons = motricityPanel.GetComponentsInChildren<TouchableBackButton>(true);
                foreach (var btn in buttons)
                {
                    if (btn.gameObject.name == "Back_Button" || btn.gameObject.name == "back_button")
                    {
                        // Check if it's the one in Game panel
                        if (btn.transform.parent.name == "FixContent" || btn.transform.parent.name == "InGame" || btn.transform.parent.parent.name == "Game")
                        {
                            btn.SetTargetState(MainPanelState.MOBILITY);
                            Debug.Log($"[FixButtonConfig] Fixed Motricity Game Back Button on {btn.gameObject.name}");
                        }
                    }
                }
            }

            // 2. Fix Cognitive Game Back Button -> Should go to COGNITIVE
            var cognitivePanel = FindObjectOfType<CognitivePanel>();
            if (cognitivePanel != null)
            {
                var buttons = cognitivePanel.GetComponentsInChildren<TouchableBackButton>(true);
                foreach (var btn in buttons)
                {
                    if (btn.gameObject.name == "Back_Button" || btn.gameObject.name == "back_button")
                    {
                        if (btn.transform.parent.name == "FixContent" || btn.transform.parent.name == "InGame" || btn.transform.parent.parent.name == "Game")
                        {
                            btn.SetTargetState(MainPanelState.COGNITIVE);
                            Debug.Log($"[FixButtonConfig] Fixed Cognitive Game Back Button on {btn.gameObject.name}");
                        }
                    }
                }
            }
        }

        void FixButton(string partialPath, MainPanelState target)
        {
            // Simple search by name if path is too hard to match exactly
            // Or search recursively
            var allButtons = Resources.FindObjectsOfTypeAll<TouchableBackButton>();
            foreach (var btn in allButtons)
            {
                if (GetPath(btn.transform).Contains(partialPath))
                {
                    btn.SetTargetState(target);
                    Debug.Log($"[FixButtonConfig] Fixed {partialPath} to target {target}");
                }
            }
        }

        string GetPath(Transform t)
        {
            if (t.parent == null) return t.name;
            return GetPath(t.parent) + "/" + t.name;
        }
    }
}
