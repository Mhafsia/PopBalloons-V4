using UnityEngine;
using PopBalloons.Utilities;

namespace PopBalloons.DebugTools
{
    /// <summary>
    /// Debug utility to inspect all LoadLevelButton configurations in the scene
    /// Add this to any GameObject and check the logs on Start
    /// </summary>
    public class InspectLoadLevelButtons : MonoBehaviour
    {
        [Header("Auto-inspect on Start")]
        [SerializeField] private bool inspectOnStart = true;

        private void Start()
        {
            if (inspectOnStart)
            {
                InspectAll();
            }
        }

        [ContextMenu("Inspect All LoadLevelButtons")]
        public void InspectAll()
        {
            LoadLevelButton[] buttons = FindObjectsOfType<LoadLevelButton>();
            
            UnityEngine.Debug.Log($"========== INSPECTION: Found {buttons.Length} LoadLevelButton(s) ==========");
            
            for (int i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                var gameTypeField = typeof(LoadLevelButton).GetField("gameType", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                var levelNumberField = typeof(LoadLevelButton).GetField("levelNumber", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);

                if (gameTypeField != null && levelNumberField != null)
                {
                    var gameType = (GameManager.GameType)gameTypeField.GetValue(button);
                    var levelNumber = (int)levelNumberField.GetValue(button);
                    
                    string path = GetGameObjectPath(button.gameObject);
                    UnityEngine.Debug.Log($"[{i}] {button.gameObject.name} | Type: {gameType} | Level: {levelNumber} | Path: {path}");
                }
                else
                {
                    UnityEngine.Debug.LogError($"Could not access private fields for {button.gameObject.name}");
                }
            }
            
            UnityEngine.Debug.Log($"========== END INSPECTION ==========");
        }

        private string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform parent = obj.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }
    }
}
