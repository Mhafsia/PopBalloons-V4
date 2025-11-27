using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PopBalloons.UI
{

    /// <summary>
    /// Main section of the panel
    /// </summary>
    public class MainPanelElement : UIStateDispatcher<MainPanelState>
    {
        protected override void Subscribe()
        {
            MainPanel.Instance.Subscribe(this.HandleChange);
        }

        public override void HandleChange(MainPanelState status)
        {
            bool match = mask.Contains(status);
            // Debug.Log($"[MainPanelElement] {gameObject.name} received state {status}. Match: {match}");
            base.HandleChange(status);
        }

        protected override void UnSubscribe()
        {
            MainPanel.Instance.UnSubscribe(this.HandleChange);
        }
    }
}