using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PopBalloons.Utilities;

namespace PopBalloons.UI
{

    /// <summary>
    /// Handle navigation inside MainPanel
    /// </summary>
    public class MainPanelButton : SubPanelButton<MainPanelState>
    {
        public override void OnClick()
        {
            // If we're in a game (any type), quit it properly first
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.PLAY)
            {
                Debug.Log("MainPanelButton: Quitting active game before navigation");
                GameManager.Instance.Home();
                return; // Home() will handle the state change
            }
            
            // Otherwise just change the panel state normally
            MainPanel.Instance.SetState(destination);
        }
    }

}