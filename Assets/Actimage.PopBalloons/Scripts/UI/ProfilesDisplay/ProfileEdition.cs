using System;
using PopBalloons.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PopBalloons.UI
{

    public class ProfileEdition : MonoBehaviour
    {
        [SerializeField]
        private ProfileAvatar avatar;

        [SerializeField]
        private TMPro.TMP_InputField name;

        private PlayerData data;
        public Button deleteButton;
        //[SerializeField] private QuestionDialogUi questionDialog;
        
        public void Start()
        {
            deleteButton.onClick.AddListener(OnButtonClick);
            //questionDialog = FindObjectOfType<QuestionDialogUi>();
            //Debug.Log("icittt " + questionDialog);
        }

        public void EditProfile(PlayerData data)
        {
            this.data = data;
            avatar.Hydrate(data.avatar);
            name.text = data.username;
        }

        public void NewProfile()
        {
            PlayerData data = new PlayerData();
            data.id = System.Guid.NewGuid().ToString();
            data.avatar = new AvatarData();
            EditProfile(data);
            //avatar.Init();
            //data.avatar = avatar.Data;
            //name.text = "";

        }

        public void OnClick()
        {
            data.username = name.text;

            ProfilesManager.Instance.UpdatePlayerData(data);
            //Set it as current profile
            ProfilesManager.Instance.SetCurrentProfile(data.id);
            
            // REMOVED: Automatic transition to MODE_PICK
            // User should manually click on a mode button to proceed
            // MainPanel.Instance.SetState(MainPanelState.MODE_PICK);

        }
        
        public void OnButtonClick()
        {
            QuestionDialogUi.Instance.ShowQuestion("hello",  () =>
            {
                QuestionDialogUi.Instance.deleteProfile(data);
            }, () =>
            {
                //Do nothing 
            });
        }
    }

}