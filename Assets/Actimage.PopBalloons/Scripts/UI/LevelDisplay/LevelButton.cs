using PopBalloons.Configuration;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Input;

namespace PopBalloons.UI
{
    public class LevelButton : MonoBehaviour,
        IMixedRealityPointerHandler,
        IMixedRealityTouchHandler
    {
        #region Variables

        private Image background;
        private TextMeshProUGUI label;
        private LevelList parentList;
        private LevelInfo infos;
        private BoxCollider buttonCollider;
        private NearInteractionTouchable touchable;

        [SerializeField]
        private bool enableTouchInteraction = true;

        public LevelInfo Infos { get => infos;}

        #endregion

        #region Unity Functions
        private void Awake()
        {
            this.background = this.GetComponentInChildren<Image>();
            this.label = this.GetComponentInChildren<TextMeshProUGUI>();
            SetupTouchInteraction();
        }

        private void Update()
        {
            // Don't disable collider - MRTK handles collision
        }

        private bool IsButtonVisible()
        {
            if (!gameObject.activeInHierarchy) return false;

            CanvasGroup localCG = GetComponent<CanvasGroup>();
            if (localCG != null && (localCG.alpha < 0.01f || !localCG.interactable))
            {
                return false;
            }

            return true;
        }

        private void SetupTouchInteraction()
        {
            if (!enableTouchInteraction) return;

            buttonCollider = GetComponent<BoxCollider>();
            if (buttonCollider == null)
            {
                buttonCollider = gameObject.AddComponent<BoxCollider>();
                RectTransform rectTransform = GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    buttonCollider.size = new Vector3(rectTransform.rect.width, rectTransform.rect.height, 0.1f);
                }
                else
                {
                    buttonCollider.size = new Vector3(0.1f, 0.1f, 0.1f);
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
        public void Hydrate(LevelInfo infos, LevelList parentList)
        {
            this.parentList = parentList;
            this.infos = infos;
            label.text = this.Infos.GameIndex.ToString();
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
            background.sprite = (b) ? this.parentList.ActiveBackground : this.parentList.InactiveBackground;
        }
        #endregion
    }
}