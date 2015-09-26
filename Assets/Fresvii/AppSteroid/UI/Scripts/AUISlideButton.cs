using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    public class AUISlideButton : MonoBehaviour
    {
        public static bool open;

        public enum OpenState { Closed, LeftOpen, RightOpen, Closing };

        private OpenState openState;

        public RectTransform content;

        public int slideWidth;

        public bool openLeft, openRight;

        public AUIScrollRect scrollRect;

        public event Action<OpenState> OnOpenStateChanged;

        public AUIScrollRect slideButtonScrollRect;

        public RectTransform[] rightSlideButtons;

        public float buttonWidth = 160f;

        public float rightOpenThreshold = 160f;

        void OnEnable()
        {
            scrollRect.enabled = true;

            openState = OpenState.Closed;

            scrollRect.EndDrag += OnEndDrag;
        }

        void OnDisable()
        {
            scrollRect.EndDrag -= OnEndDrag;
        }

        void Update()
        {
            if (openRight)
            {
                if (content.anchoredPosition.x <= -slideWidth && openState == OpenState.Closed)
                {
                    openState = OpenState.RightOpen;

                    open = true;

                    if (OnOpenStateChanged != null)
                    {
                        OnOpenStateChanged(openState);
                    }
                }
            }
        }

        void LateUpdate()
        {
            if (!openLeft)
            {
                if (content.anchoredPosition.x > 0)
                {
                    content.anchoredPosition = new Vector2(0f, content.anchoredPosition.y);
                }
            }

            if (content.anchoredPosition.x > -slideWidth && openState == OpenState.RightOpen)
            {
                content.anchoredPosition = new Vector2(-slideWidth, content.anchoredPosition.y);

                scrollRect.enabled = false;
            }

            if(openRight)
            {
                for (int i = 0; i < rightSlideButtons.Length; i++)
                {
                    rightSlideButtons[i].anchoredPosition = new Vector2(buttonWidth + content.anchoredPosition.x * (i + 1) / rightSlideButtons.Length, rightSlideButtons[i].anchoredPosition.y);
                }
            }
        }

        public iTween.EaseType openEaseType = iTween.EaseType.easeOutExpo;

        public float openDuration = 0.3f;

        public void OnEndDrag()
        {
            if (content.anchoredPosition.x < -rightOpenThreshold && content.anchoredPosition.x > -slideWidth && openState == OpenState.Closed)
            {
                scrollRect.enabled = false;

                iTween.ValueTo(this.gameObject, iTween.Hash("from", content.anchoredPosition, "to", new Vector2(-slideWidth, content.anchoredPosition.y), "easetype", openEaseType, "time", openDuration, "onupdate", "OnUpdateOpen", "oncomplete", "OnCompleteOpen"));
            }
        }

        void OnUpdateOpen(Vector2 value)
        {
            content.anchoredPosition = value;
        }

        void OnCompleteOpen()
        {
            content.anchoredPosition = new Vector2(-slideWidth, content.anchoredPosition.y);
        }

        public void Close()
        {
            if (openState == OpenState.Closed || openState == OpenState.Closing)
            {
                return;
            }

            openState = OpenState.Closing;

            scrollRect.enabled = true;

            Invoke("DeleyStateChanged", 0.5f);
        }

        void DeleyStateChanged()
        {
            open = false;

            openState = OpenState.Closed;

            if (OnOpenStateChanged != null)
            {
                OnOpenStateChanged(openState);
            }
        }
    }
}