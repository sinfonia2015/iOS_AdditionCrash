using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    public class AUIVerticalLayoutPullReflesh : MonoBehaviour 
    {
        private enum PullRefleshType { None, PullDown, PullUp }

        private PullRefleshType pullRefleshing = PullRefleshType.None;

        public RectTransform content;

        public int pullRefleshThreshold;

        public GameObject loadingSpinner;

        public event Action OnPullDownReflesh;

        //public event Action OnPullUpReflesh;

        private bool canPullRefresh = true;

        public ScrollRect scrollRect;

        // Update is called once per frame
        void OnEnable()
        {
            pullRefleshing = PullRefleshType.None;

            canPullRefresh = true;

            scrollRect.enabled = true;

            if (loadingSpinner != null)
            {
                loadingSpinner.SetActive(false);
            }
        }

        void Update()
        {
            if (content.anchoredPosition.y < -pullRefleshThreshold && pullRefleshing == PullRefleshType.None && canPullRefresh)
            {
                pullRefleshing = PullRefleshType.PullDown;

                canPullRefresh = false;

                if (OnPullDownReflesh != null)
                {
                    if (loadingSpinner != null)
                    {
                        loadingSpinner.SetActive(true);
                    }

                    OnPullDownReflesh();
                }
            }
        }

        void LateUpdate()
        {
            if (content.anchoredPosition.y > -pullRefleshThreshold && pullRefleshing == PullRefleshType.PullDown)
            {
                content.anchoredPosition = new Vector2(content.anchoredPosition.x, -pullRefleshThreshold);

                scrollRect.enabled = false;

                scrollRect.velocity = Vector2.zero;
            }
        }

        public void PullRefleshCompleted()
        {
            if(this.gameObject.activeInHierarchy)
            {
                StartCoroutine(WaitTouchRelease());
            }
            else
            {
                pullRefleshing = PullRefleshType.None;

                scrollRect.enabled = true;

                if (loadingSpinner != null)
                {
                    loadingSpinner.SetActive(false);
                }

                canPullRefresh = true;
            }
        }

        public float pullRefleshWaitInterval = 2.0f;

        IEnumerator WaitTouchRelease()
        {
            pullRefleshing = PullRefleshType.None;

            scrollRect.enabled = true;

            if (loadingSpinner != null)
            {
                loadingSpinner.SetActive(false);
            }

            while (Input.touchCount > 0 || Input.GetMouseButton(0))
                yield return 1;

            yield return new WaitForSeconds(pullRefleshWaitInterval);

            canPullRefresh = true;
        }
    }
}
