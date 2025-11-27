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
                GameManager.Instance.Home();
                return;
            }
            
            // Synchronize GameModeSelector with the destination panel state
            if (GameModeSelector.Instance != null)
            {
                switch (destination)
                {
                    case MainPanelState.MOBILITY:
                        GameModeSelector.Instance.CurrentGameType = GameManager.GameType.MOBILITY;
                        GameModeSelector.Instance.CurrentLevelNumber = 1;
                        break;
                    case MainPanelState.COGNITIVE:
                        GameModeSelector.Instance.CurrentGameType = GameManager.GameType.COGNITIVE;
                        GameModeSelector.Instance.CurrentLevelNumber = 1;
                        break;
                    case MainPanelState.FREEPLAY:
                        GameModeSelector.Instance.CurrentGameType = GameManager.GameType.FREEPLAY;
                        GameModeSelector.Instance.CurrentLevelNumber = 0;
                        MainPanel.Instance.SetState(MainPanelState.FREEPLAY);
                        GameManager.Instance.NewGame(GameManager.GameType.FREEPLAY, 0);
                        return;
                    case MainPanelState.MODE_PICK:
                        break;
                }
            }
            
            // Otherwise just change the panel state normally
            MainPanel.Instance.SetState(destination);
        }
    }
}