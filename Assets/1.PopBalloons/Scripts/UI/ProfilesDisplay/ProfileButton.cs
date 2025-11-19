using System;
using System.Collections;
using System.Collections.Generic;
using PopBalloons.Configuration;
using PopBalloons.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Input;

namespace PopBalloons.UI
{

    public class ProfileButton : MonoBehaviour,
        IMixedRealityPointerHandler,
        IMixedRealityTouchHandler
    {

        #region Variables

        [SerializeField]
        private AnimationSettings settings;

        [SerializeField]
        private ProfileAvatar avatar;

        [SerializeField]
        private CanvasGroup editButton;

        [SerializeField]
        private bool enableTouchInteraction = true;

        private Button background;
        private TextMeshProUGUI label;
        private ProfileList parentList;
        private PlayerData infos;
        private BoxCollider buttonCollider;
        private NearInteractionTouchable touchable;

        public PlayerData Infos { get => infos; }


        private RectTransform rectTransform;
        private Vector2 defaultSize;
        private bool isSelected = false;
        #endregion

        #region Unity Functions
        private void Awake()
        {
            this.background = this.GetComponentInChildren<Button>();
            this.label = this.GetComponentInChildren<TextMeshProUGUI>();
            this.rectTransform = this.GetComponent<RectTransform>();
            this.defaultSize = rectTransform.sizeDelta;
            editButton.alpha = 0;
            editButton.interactable = false;
            editButton.blocksRaycasts = false;
            SetupTouchInteraction();
        }

        private void SetupTouchInteraction()
        {
            if (!enableTouchInteraction) return;

            buttonCollider = GetComponent<BoxCollider>();
            if (buttonCollider == null)
            {
                buttonCollider = gameObject.AddComponent<BoxCollider>();
                if (rectTransform != null)
                {
                    buttonCollider.size = new Vector3(rectTransform.rect.width, rectTransform.rect.height, 0.01f);
                }
                else
                {
                    buttonCollider.size = new Vector3(0.1f, 0.1f, 0.01f);
                }
            }
            buttonCollider.isTrigger = true;

            touchable = GetComponent<NearInteractionTouchable>();
            if (touchable == null)
            {
                touchable = gameObject.AddComponent<NearInteractionTouchable>();
            }
            touchable.SetLocalForward(Vector3.forward);
            touchable.SetBounds(buttonCollider.size);
        }
        #endregion

        #region MRTK Handlers
        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            OnClick();
            eventData.Use();
        }

        public void OnTouchCompleted(HandTrackingInputEventData eventData) { }
        public void OnTouchUpdated(HandTrackingInputEventData eventData) { }

        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            OnClick();
            eventData.Use();
        }

        public void OnPointerUp(MixedRealityPointerEventData eventData) { }
        public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
        public void OnPointerClicked(MixedRealityPointerEventData eventData) { }
        #endregion

        #region Functions
        /// <summary>
        /// Will store the matching level info on this button
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="parentList"></param>
        public void Hydrate(PlayerData infos, ProfileList parentList)
        {
            this.parentList = parentList;
            this.infos = infos;
            this.avatar.Hydrate(infos.avatar);
            label.text = this.Infos.username;
            
            //Todo : set avatar item
        }

        public void Edit()
        {
            if(this.parentList != null)
                this.parentList.EditProfile();
        }

        /// <summary>
        /// Will select the current level
        /// </summary>
        public void OnClick()
        {
            this.parentList.Select(this.infos);
        }

        public void SetSelected(bool b)
        {
            label.color = (b) ? new Color(0.19607f, 0.19607f, 0.19607f) : Color.white;


            editButton.alpha = (b)?1:0;
            editButton.interactable = b;
            editButton.blocksRaycasts = b;
            background.image.sprite = (b) ? this.parentList.ActiveBackground : this.parentList.InactiveBackground;
            background.spriteState = (b) ? this.parentList.ActiveSpriteState : this.parentList.InactiveSpriteState;

            if (isSelected != b)
            {
                StartCoroutine(Animate(b));
            }
        }

        #endregion

        #region Coroutines
        private IEnumerator Animate(bool b)
        {
            isSelected = b;
            float t = 0;

            Vector2 initialSize = this.rectTransform.sizeDelta;
            Vector2 targetSize = (b) ? defaultSize * 2f : defaultSize;
            while(t < settings.Duration)
            {
                this.rectTransform.sizeDelta = Vector2.Lerp(initialSize, targetSize, settings.Curve.Evaluate(t / settings.Duration));
                t += Time.deltaTime;
                yield return null;
            }
            this.rectTransform.sizeDelta = targetSize;
        }

        #endregion
    }
}