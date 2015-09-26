﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

namespace Fresvii.AppSteroid.UI
{
    public class AUIScrollRect : ScrollRect
    {
        public Image scrollVerticalHandle;

        public Image scrollHorizontalHandle;

        public bool IsScroll { get; protected set; }

        public bool IsDrag { get; protected set; }

        AUIScrollPass auiScrollPass;

        public event System.Action EndDrag;

        protected override void Awake()
        {
            base.Awake();

            if (verticalScrollbar != null)
                scrollVerticalHandle = verticalScrollbar.handleRect.gameObject.GetComponent<Image>();

            if (horizontalScrollbar != null)
                scrollHorizontalHandle = horizontalScrollbar.handleRect.gameObject.GetComponent<Image>();
            
            contentsRectTransform = content.GetComponent<RectTransform>();

            auiScrollPass = GetComponent<AUIScrollPass>();
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (scrollVerticalHandle != null)
                scrollVerticalHandle.CrossFadeAlpha(0.0f, 0.0f, true);

            if (scrollHorizontalHandle != null)
                scrollHorizontalHandle.CrossFadeAlpha(0.0f, 0.0f, true);

            IsScroll = IsDrag = false;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            velocity = Vector2.zero;
        }

        void Update()
        {
            if (Mathf.Abs(velocity.y) < 3f && IsScroll && !IsDrag && !pass)
            {
                IsScroll = false;

                if (vertical && scrollVerticalHandle != null)
                    scrollVerticalHandle.CrossFadeAlpha(0.0f, 0.25f, true);

                if (horizontal && scrollHorizontalHandle != null)
                    scrollHorizontalHandle.CrossFadeAlpha(0.0f, 0.25f, true);
            }
            else
            {
                IsScroll = true;
            }
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            pass = false;

            if (auiScrollPass == null)
            {
                if (vertical && scrollVerticalHandle != null)
                    scrollVerticalHandle.CrossFadeAlpha(1f, 0.25f, true);

                if (horizontal && scrollHorizontalHandle != null)
                    scrollHorizontalHandle.CrossFadeAlpha(1f, 0.25f, true);

                IsDrag = true;

                IsScroll = true;

                base.OnBeginDrag(eventData);
            }
            else
            {
                pass = (Mathf.Abs(eventData.pressPosition.x - eventData.position.x) < Mathf.Abs(eventData.pressPosition.y - eventData.position.y));

                if (!pass)
                {
                    if (vertical && scrollVerticalHandle != null)
                        scrollVerticalHandle.CrossFadeAlpha(1f, 0.25f, true);

                    if (horizontal && scrollHorizontalHandle != null)
                        scrollHorizontalHandle.CrossFadeAlpha(1f, 0.25f, true);

                    IsDrag = true;

                    IsScroll = true;

                    base.OnBeginDrag(eventData);
                }
                else
                {
                    if (auiScrollPass == null)
                    {
                        base.OnBeginDrag(eventData);
                    }
                    else
                    {
                        auiScrollPass.passScrollRect.OnBeginDrag(eventData);
                    }
                }
            }
        }

        bool pass;

        public override void OnDrag(PointerEventData eventData)
        {
            if(auiScrollPass != null)
            {
                if (pass)
                {
                    auiScrollPass.passScrollRect.OnDrag(eventData);
                }
                else
                {
                    base.OnDrag(eventData);
                }
            }
            else
            {
                base.OnDrag(eventData);
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            IsDrag = false;

            if (pass)
            {
                if (auiScrollPass != null)
                {
                    auiScrollPass.passScrollRect.OnEndDrag(eventData);
                }
                else
                {
                    base.OnEndDrag(eventData);

                    if (EndDrag != null)
                    {
                        EndDrag();
                    }
                }
            }
            else
            {
                base.OnEndDrag(eventData);

                if (EndDrag != null)
                {
                    EndDrag();
                }
            }
        }
       
        public iTween.EaseType toTopTweenEasetype = iTween.EaseType.easeOutExpo;

        private RectTransform contentsRectTransform;

        public void GoToTop(float duration)
        {
            if (duration == 0f)
            {
                contentsRectTransform.anchoredPosition = Vector2.zero;   
            }
            else
            {
                iTween.ValueTo(this.gameObject, iTween.Hash("from", contentsRectTransform.anchoredPosition, "to", Vector2.zero, "time", duration, "easytype", toTopTweenEasetype, "onupdate", "OnUpdateContentsPosition", "oncomplete", "OnCompleteContentsPosition"));
            }
        }

        public void GoToBottom(float duration)
        {
            StartCoroutine(GoToBottomCoroutine(duration));
        }

        IEnumerator GoToBottomCoroutine(float duration)
        {
            yield return new WaitForEndOfFrame();

            if (contentsRectTransform.sizeDelta.y > AUIManager.Instance.sizedCanvas.rect.height)
            {
                if (duration > 0f)
                {
                    iTween.ValueTo(this.gameObject, iTween.Hash("from", contentsRectTransform.anchoredPosition, "to", new Vector2(contentsRectTransform.anchoredPosition.x, contentsRectTransform.sizeDelta.y - AUIManager.Instance.sizedCanvas.rect.height), "time", duration, "easytype", toTopTweenEasetype, "onupdate", "OnUpdateContentsPosition", "oncomplete", "OnCompleteContentsPosition"));
                }
                else
                {
                    contentsRectTransform.anchoredPosition = new Vector2(contentsRectTransform.anchoredPosition.x, contentsRectTransform.sizeDelta.y - AUIManager.Instance.sizedCanvas.rect.height);   
                }
            }
        }

        public void Pinned()
        {
            StartCoroutine(PinnedCoroutine());
        }

        IEnumerator PinnedCoroutine()
        {
            float postHeight = contentsRectTransform.sizeDelta.y;

            yield return new WaitForEndOfFrame();

            contentsRectTransform.anchoredPosition = new Vector2(contentsRectTransform.anchoredPosition.x, contentsRectTransform.anchoredPosition.y + contentsRectTransform.sizeDelta.y - postHeight);
        }

        public void SetToBottom()
        {
            StartCoroutine(SetToBottomCoroutine());
        }

        IEnumerator SetToBottomCoroutine()
        {
            yield return new WaitForEndOfFrame();

            contentsRectTransform.anchoredPosition = new Vector2(contentsRectTransform.anchoredPosition.x, contentsRectTransform.sizeDelta.y - AUIManager.Instance.sizedCanvas.rect.height);

            velocity = Vector2.zero;
        }

        void OnUpdateContentsPosition(Vector2 pos)
        {
            contentsRectTransform.anchoredPosition = pos;   
        }

        void OnCompleteContentsPosition()
        {
            velocity = Vector2.zero;
        }

        public void StopScroll()
        {
            velocity = Vector2.zero;
        }
    }
}