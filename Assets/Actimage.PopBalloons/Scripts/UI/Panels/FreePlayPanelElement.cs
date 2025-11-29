using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PopBalloons.UI
{
    /// <summary>
    /// UI element that responds to FreePlayPanel state changes.
    /// Attach to UI GameObjects that should show/hide based on FreePlay state (SETUP, INGAME, ENDGAME).
    /// </summary>
    public class FreePlayPanelElement : UIStateElement<FreePlaySubState>
    {
        protected override void Subscribe()
        {
            if (FreePlayPanel.Instance != null)
            {
                FreePlayPanel.Instance.OnStateChanged += this.HandleChange;
            }
        }

        protected override void UnSubscribe()
        {
            if (FreePlayPanel.Instance != null)
            {
                FreePlayPanel.Instance.OnStateChanged -= this.HandleChange;
            }
        }
    }
}
