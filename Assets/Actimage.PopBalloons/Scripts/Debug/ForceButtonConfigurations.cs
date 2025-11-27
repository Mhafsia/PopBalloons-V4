using UnityEngine;
using PopBalloons.Utilities;

namespace PopBalloons.DebugTools
{
    /// <summary>
    /// Force correct values for specific buttons by name
    /// Add this to the scene to auto-fix button configurations
    /// </summary>
    public class ForceButtonConfigurations : MonoBehaviour
    {
        [Header("Auto-fix on Start")]
        [SerializeField] private bool autoFixOnStart = true;
        
        private void Start()
        {
            if (autoFixOnStart)
            {
                ForceCorrectValues();
            }
        }
        
        [ContextMenu("Force Correct Button Values")]
        public void ForceCorrectValues()
        {
            Debug.Log("========================================");
            Debug.Log("FORCING CORRECT BUTTON CONFIGURATIONS");
            Debug.Log("========================================");
            
            // Find all ModeButtons and fix by name
            var modeButtons = FindObjectsOfType<PopBalloons.UI.ModeButton>();
            
            foreach (var btn in modeButtons)
            {
                string name = btn.gameObject.name.ToLower();
                GameManager.GameType correctType = GameManager.GameType.NONE;
                
                if (name.Contains("mobility") || name.Contains("motor"))
                {
                    correctType = GameManager.GameType.MOBILITY;
                }
                else if (name.Contains("cognitive") || name.Contains("cognitif"))
                {
                    correctType = GameManager.GameType.COGNITIVE;
                }
                else if (name.Contains("freeplay") || name.Contains("free"))
                {
                    correctType = GameManager.GameType.FREEPLAY;
                }
                
                if (correctType != GameManager.GameType.NONE)
                {
                    // Use reflection to set the private field
                    var gameTypeField = typeof(PopBalloons.UI.ModeButton).GetField("gameType", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (gameTypeField != null)
                    {
                        var currentType = (GameManager.GameType)gameTypeField.GetValue(btn);
                        
                        if (currentType != correctType)
                        {
                            gameTypeField.SetValue(btn, correctType);
                            Debug.Log($"  ✓ Fixed {btn.gameObject.name}: {currentType} → {correctType}");
                        }
                        else
                        {
                            Debug.Log($"  ✓ {btn.gameObject.name}: already correct ({correctType})");
                        }
                    }
                }
            }
            
            Debug.Log("========================================");
        }
    }
}
