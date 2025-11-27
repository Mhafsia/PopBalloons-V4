using PopBalloons.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Input;

namespace PopBalloons.UI
{

    public abstract class AvatarOptionButton<T> : MonoBehaviour,
        IMixedRealityPointerHandler,
        IMixedRealityTouchHandler
    {
        [SerializeField]
        private Image icon;

        [SerializeField]
        private bool enableTouchInteraction = true;

        protected bool isSelected = false;

        Button b;
        protected AvatarOptions<T> option;
        protected ProfileAvatar target;
        private BoxCollider buttonCollider;
        private NearInteractionTouchable touchable;
        private bool isProcessingTouch = false; // Empêcher double déclenchement

        public void Awake()
        {
            b = this.GetComponent<Button>();
            SetupTouchInteraction();
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

        // MRTK Touch Handlers
        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            if (isProcessingTouch) return;
            isProcessingTouch = true;
            OnClick();
            eventData.Use();
            StartCoroutine(ResetTouchFlag());
        }

        public void OnTouchCompleted(HandTrackingInputEventData eventData) { }
        public void OnTouchUpdated(HandTrackingInputEventData eventData) { }

        // MRTK Pointer Handlers
        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            if (isProcessingTouch) return;
            isProcessingTouch = true;
            OnClick();
            eventData.Use();
            StartCoroutine(ResetTouchFlag());
        }

        public void OnPointerUp(MixedRealityPointerEventData eventData) { }
        public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
        public void OnPointerClicked(MixedRealityPointerEventData eventData) { }

        private IEnumerator ResetTouchFlag()
        {
            yield return new WaitForSeconds(0.2f);
            isProcessingTouch = false;
        }

        public void Hydrate(AvatarOptions<T> option, ProfileAvatar target)
        {
            this.option = option;
            this.target = target;
            if(target != null && target.Data != null)
            {
                this.SetSelected();
            }
            this.Redraw();
        }


        public void Redraw()
        {
            SpriteState state = new SpriteState();
            state.pressedSprite = (isSelected) ?option.Icon: option.SelectedIcon;
            state.disabledSprite = (!isSelected) ? option.Icon : option.SelectedIcon;
            state.highlightedSprite = (isSelected) ? option.SelectedIcon : option.HoverIcon;

            b.spriteState = state;
            (this.b.targetGraphic as Image).sprite =(isSelected)?option.SelectedIcon :option.Icon;
            if(isSelected)
            {
                //Debug.Log("I am selected ");
            }
        }


        public abstract void OnClick();

        public abstract void SetSelected();

    }

}