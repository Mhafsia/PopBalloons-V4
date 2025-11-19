using PopBalloons.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PopBalloons.UI
{
    public enum ProfileSubState { PICK, EDIT, SELECTED};

    public class ProfilePanel : SubPanel<ProfileSubState>
    {
        #region Variables
        private static ProfilePanel instance;


        /// <summary>
        /// Singleton instance accessor
        /// </summary>
        public static ProfilePanel Instance { get => instance; }

        [SerializeField]
        /// <summary>
        /// Manager all profile edition
        /// </summary>
        private ProfileEdition editor;

        #endregion Variables


        #region Unity Functions
        /// <summary>
        /// Singleton's implementation.
        /// </summary>
        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("Should'nt have two instances of ProfilePanel.");
                DestroyImmediate(this.gameObject);
            }
            else
            {
                instance = this;
                this.PopulateWithChildren();
            }
        }

        private void Start()
        {
            this.SetState(ProfileSubState.PICK);
        }
        #endregion Unity Functions


    #region Functions
        public void ClearAllSelection()
        {
            //TODO: ProfileManager unset current profile
            this.SetState(ProfileSubState.PICK);
        }


        public void PlayAsGuest()
        {
            UnityEngine.Debug.Log("ProfilePanel: PlayAsGuest called");
            ProfilesManager.Instance.PlayAsGuest();
            
            if (MainPanel.Instance != null)
            {
                UnityEngine.Debug.Log("ProfilePanel: Setting MainPanel to MODE_PICK");
                MainPanel.Instance.SetState(MainPanelState.MODE_PICK);
            }
            else
            {
                UnityEngine.Debug.LogError("ProfilePanel: MainPanel.Instance is null!");
            }
        }

        public void SelectPlayer(string id)
        {
            UnityEngine.Debug.Log($"ProfilePanel: SelectPlayer called with id={id}");
            ProfilesManager.Instance.SetCurrentProfile(id);
            
            if (MainPanel.Instance != null)
            {
                UnityEngine.Debug.Log("ProfilePanel: Setting MainPanel to MODE_PICK");
                MainPanel.Instance.SetState(MainPanelState.MODE_PICK);
            }
            else
            {
                UnityEngine.Debug.LogError("ProfilePanel: MainPanel.Instance is null!");
            }
        }


        public void EditPlayer(PlayerData profile)
        {
            editor.EditProfile(profile);
            this.SetState(ProfileSubState.EDIT);
        }

        public void NewProfile()
        {
            editor.NewProfile();
            this.SetState(ProfileSubState.EDIT);
        }


        public override void Init()
        {
            this.SetState(ProfileSubState.PICK);
        }
        #endregion

    }
}
