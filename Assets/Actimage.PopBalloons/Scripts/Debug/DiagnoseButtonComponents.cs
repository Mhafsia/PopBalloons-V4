using UnityEngine;
using PopBalloons.Utilities;

namespace PopBalloons.DebugTools
{
    /// <summary>
    /// Add this to any GameObject to monitor ALL button components and their event handlers
    /// </summary>
    public class DiagnoseButtonComponents : MonoBehaviour
    {
        [ContextMenu("Diagnose This GameObject")]
        public void DiagnoseThisObject()
        {
            Debug.Log("========================================");
            Debug.Log($"DIAGNOSTIC: {gameObject.name}");
            Debug.Log("========================================");
            
            // List ALL MonoBehaviour components
            var components = GetComponents<MonoBehaviour>();
            Debug.Log($"Found {components.Length} MonoBehaviour components:");
            
            foreach (var comp in components)
            {
                if (comp == null) continue;
                Debug.Log($"  - {comp.GetType().Name}");
            }
            
            // Check specific button types
            var loadLevelBtn = GetComponent<LoadLevelButton>();
            if (loadLevelBtn != null)
            {
                Debug.LogWarning($"  ⚠️ Has LoadLevelButton!");
            }
            
            var modeBtn = GetComponent<PopBalloons.UI.ModeButton>();
            if (modeBtn != null)
            {
                // Use reflection to get gameType value
                var gameTypeField = typeof(PopBalloons.UI.ModeButton).GetField("gameType", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (gameTypeField != null)
                {
                    var gameType = (GameManager.GameType)gameTypeField.GetValue(modeBtn);
                    Debug.Log($"  ✓ ModeButton with gameType = {gameType}");
                }
            }
            
            var backBtn = GetComponent<PopBalloons.UI.TouchableBackButton>();
            if (backBtn != null)
            {
                var targetStateField = typeof(PopBalloons.UI.TouchableBackButton).GetField("targetState",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (targetStateField != null)
                {
                    var targetState = targetStateField.GetValue(backBtn);
                    Debug.Log($"  ✓ TouchableBackButton with targetState = {targetState}");
                }
            }
            
            var mainPanelBtn = GetComponent<PopBalloons.UI.MainPanelButton>();
            if (mainPanelBtn != null)
            {
                Debug.Log($"  ✓ Has MainPanelButton");
            }
            
            // Check collider
            var collider = GetComponent<BoxCollider>();
            if (collider != null)
            {
                Debug.Log($"  BoxCollider: size={collider.size}, enabled={collider.enabled}, isTrigger={collider.isTrigger}");
            }
            
            // Check active state and CanvasGroup
            Debug.Log($"  GameObject.activeInHierarchy = {gameObject.activeInHierarchy}");
            var cg = GetComponent<CanvasGroup>();
            if (cg != null)
            {
                Debug.Log($"  CanvasGroup: alpha={cg.alpha}, interactable={cg.interactable}");
            }
            
            Debug.Log("========================================");
        }
        
        [ContextMenu("Diagnose All Buttons In Scene")]
        public void DiagnoseAllButtons()
        {
            Debug.Log("================================================");
            Debug.Log("DIAGNOSING ALL BUTTONS IN SCENE");
            Debug.Log("================================================");
            
            // Find all ModeButtons
            var modeButtons = FindObjectsOfType<PopBalloons.UI.ModeButton>();
            Debug.Log($"\n--- ModeButtons ({modeButtons.Length}) ---");
            foreach (var btn in modeButtons)
            {
                var gameTypeField = typeof(PopBalloons.UI.ModeButton).GetField("gameType", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (gameTypeField != null)
                {
                    var gameType = (GameManager.GameType)gameTypeField.GetValue(btn);
                    Debug.Log($"  {btn.gameObject.name} → gameType = {gameType}");
                }
            }
            
            // Find all TouchableBackButtons
            var backButtons = FindObjectsOfType<PopBalloons.UI.TouchableBackButton>();
            Debug.Log($"\n--- TouchableBackButtons ({backButtons.Length}) ---");
            foreach (var btn in backButtons)
            {
                var targetStateField = typeof(PopBalloons.UI.TouchableBackButton).GetField("targetState",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (targetStateField != null)
                {
                    var targetState = targetStateField.GetValue(btn);
                    Debug.Log($"  {btn.gameObject.name} → targetState = {targetState}");
                }
                
                // Check for conflicting components
                var loadLevelBtn = btn.GetComponent<LoadLevelButton>();
                if (loadLevelBtn != null)
                {
                    Debug.LogError($"  ⚠️ {btn.gameObject.name} has BOTH TouchableBackButton AND LoadLevelButton!");
                }
                
                var mainPanelBtn = btn.GetComponent<PopBalloons.UI.MainPanelButton>();
                if (mainPanelBtn != null)
                {
                    Debug.Log($"  ✓ {btn.gameObject.name} also has MainPanelButton (OK, will delegate)");
                }
            }
            
            // Find all LoadLevelButtons
            var loadButtons = FindObjectsOfType<LoadLevelButton>();
            Debug.Log($"\n--- LoadLevelButtons ({loadButtons.Length}) ---");
            foreach (var btn in loadButtons)
            {
                Debug.Log($"  {btn.gameObject.name}");
            }
            
            Debug.Log("\n================================================");
        }
    }
}
