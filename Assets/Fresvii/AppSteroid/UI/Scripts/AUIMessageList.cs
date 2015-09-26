using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIMessageList : MonoBehaviour
    {
        public AUIFrame frameTween;

        [HideInInspector]
        public AUIFrame parentFrameTween;

        public GameObject prfbMessageCell;

        public AUIScrollViewContents contents;

        private List<AUIMessageListCell> messageCells = new List<AUIMessageListCell>();

        private Fresvii.AppSteroid.Models.ListMeta listMeta;

        private bool isPullRefleshProc;

        public AUIScrollViewPullReflesh pullReflesh;

        public AUIScrollRect scrollView;

        public GameObject prfbMessages;

        public GameObject prfbMessageCreate;

        public GameObject prfbDirectMessages;

        public Text title;

        public GameObject gameBackButton;

        public GameObject frameBackButton;

        public Text backButtonText;

        public AUIScrollRect scrollRect;

        string query = "{\"where\":{\"column\": \"hidden\", \"operator\": \"=\", \"value\": false}}";

        void OnEnable()
        {
            //AUIManager.OnEscapeTapped += Back;

            AUISlideButton.open = false;

            pullReflesh.OnPullDownReflesh += OnPullDownReflesh;

            pullReflesh.OnPullUpReflesh += OnPullUpReflesh;

            FASEvent.OnGroupMessageCreated += OnGroupMessageCreated;

            StartCoroutine(Init());
        }

        void OnApplicationPause(bool paused)
        {
            if (!paused)
            {
                StartCoroutine(Init());
            }
        }

        void OnDisable()
        {
            pullReflesh.OnPullDownReflesh += OnPullDownReflesh;

            pullReflesh.OnPullUpReflesh += OnPullUpReflesh;

            FASEvent.OnGroupMessageCreated -= OnGroupMessageCreated;

            //AUIManager.OnEscapeTapped -= Back;
        }

        void OnPullDownReflesh()
        {
            isPullRefleshProc = true;

            FASGroup.GetGroupMessageGroupList(1, query, OnGetGroupList);
        }

        void OnPullUpReflesh()
        {
            if (listMeta != null && listMeta.NextPage.HasValue)
            {
                isPullRefleshProc = true;

                FASGroup.GetGroupMessageGroupList((uint)listMeta.NextPage, query, OnGetGroupList);
            }
            else
            {
                pullReflesh.PullRefleshCompleted();
            }
        }

        public void SetBackButton(string text)
        {
            gameBackButton.SetActive(false);

            frameBackButton.SetActive(true);

            backButtonText.text = text;
        }
        
        IEnumerator Init()
        {
            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            while (GetComponent<AUIFrame>().Animating)
            {
                yield return 1;
            }

            yield return 1;

            FASGroup.GetGroupMessageGroupListFromCache(OnGetGroupList);

            FASGroup.GetGroupMessageGroupList(1, query, OnGetGroupList);
        }

        void OnGetGroupList(IList<Fresvii.AppSteroid.Models.Group> groups, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
        {
            if (this == null || this.enabled == false || !this.gameObject.activeInHierarchy)
            {
                return;
            }

            if (error != null)
            {
                if (FASSettings.Instance.logLevel <= FAS.LogLevels.Error)
                {
                    Debug.LogError(error.ToString());
                }

                return;
            }

            this.listMeta = meta;

            bool added = false;

            foreach (Fresvii.AppSteroid.Models.Group group in groups)
            {
                if (group.Hidden) 
                {
                    continue; 
                }

	            added |= UpdateGroup(group);
            }

            if (isPullRefleshProc)
            {
                pullReflesh.PullRefleshCompleted();

                isPullRefleshProc = false;
            }

            Sort();
        }

        private void Sort()
        {
            // Sort
            messageCells.Sort((a, b)=> System.DateTime.Compare(b.Group.LatestMessage.CreatedAt, a.Group.LatestMessage.CreatedAt));

            foreach (var obj in messageCells)
            {
                obj.transform.SetSiblingIndex(contents.transform.childCount - 1);
            }

            contents.ReLayout();
        }

        int SortCondition(AUIMessageListCell a, AUIMessageListCell b)
        {
            int ret = System.DateTime.Compare(a.Group.UpdatedAt, b.Group.UpdatedAt);

            if (ret != 0)
            {
                return ret;
            }

            ret = System.DateTime.Compare(a.Group.CreatedAt, b.Group.CreatedAt);

            if (ret != 0)
            {
                return ret;
            }

            ret = string.Compare(a.Group.Id, b.Group.Id);

            return ret;
        }

        private bool UpdateGroup(Fresvii.AppSteroid.Models.Group group)
        {
            AUIMessageListCell cell = messageCells.Find(x => x.Group.Id == group.Id);

            if (cell != null)
            {
                cell.SetGroup(group, this);

                return false;
            }

            GameObject go = Instantiate(prfbMessageCell) as GameObject;

            var item = go.GetComponent<RectTransform>();

            contents.AddItem(item);

            cell = item.GetComponent<AUIMessageListCell>();

            cell.SetGroup(group, this);

            messageCells.Add(cell);

            cell.gameObject.SetActive(true);

            cell.scrollPass.passScrollRect = scrollRect;

            return true;
        }

        public void GoToMessage(AppSteroid.Models.Group group, bool animation)
        {
            if (frameTween.Animating || AUISlideButton.open) return;

            scrollView.StopScroll();

            RectTransform rectTransform = GetComponent<RectTransform>();

            AUIMessages messagesPage = ((GameObject)Instantiate(prfbMessages)).GetComponent<AUIMessages>();

            messagesPage.Group = group;

            messagesPage.transform.SetParent(transform.parent, false);

            messagesPage.transform.SetAsLastSibling();

            messagesPage.parentFrameTween = this.frameTween;

            if (animation)
            {
                messagesPage.frame.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });

                this.frameTween.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
                {
                    this.gameObject.SetActive(false);
                });
            }
            else
            {
                this.gameObject.SetActive(false);

                this.frameTween.SetPosition(Vector2.zero);
            }

            AUIMessageListCell cell = messageCells.Find(x => x.Group.Id == group.Id);

            if (cell != null)
            {
                cell.Read();
            }
        }

        public void GoToCreateNewMessage()
        {
            if (frameTween.Animating) return;

            scrollView.StopScroll();

            RectTransform rectTransform = GetComponent<RectTransform>();

            AUIMessageCreate messagesPage = ((GameObject)Instantiate(prfbMessageCreate)).GetComponent<AUIMessageCreate>();            

            messagesPage.transform.SetParent(transform.parent, false);

            messagesPage.transform.SetAsLastSibling();

            messagesPage.parentFrameTween = this.frameTween;

            messagesPage.frameTween.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });

            this.frameTween.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }

        public void GoToDirectMessage(IList<Fresvii.AppSteroid.Models.DirectMessage> directMessages, Fresvii.AppSteroid.Models.ListMeta meta)
        {
            GoToDirectMessage(directMessages, meta, true);
        } 

        public void GoToDirectMessage(IList<Fresvii.AppSteroid.Models.DirectMessage> directMessages, Fresvii.AppSteroid.Models.ListMeta meta, bool animation)
        {
            if (frameTween.Animating || AUISlideButton.open) return;

            scrollView.StopScroll();

            RectTransform rectTransform = GetComponent<RectTransform>();

            AUIDirectMessages directMessagesPage = ((GameObject)Instantiate(prfbDirectMessages)).GetComponent<AUIDirectMessages>();

            directMessagesPage.listMeta = meta;

            directMessagesPage.transform.SetParent(transform.parent, false);

            directMessagesPage.transform.SetAsLastSibling();

            directMessagesPage.parentFrameTween = this.frameTween;

            if (animation)
            {
                directMessagesPage.frameTween.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });

                this.frameTween.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
                {
                    this.gameObject.SetActive(false);
                });
            }
            else
            {
                directMessagesPage.frameTween.SetPosition(Vector2.zero);

                this.gameObject.SetActive(false);
            }            
        }

        public void RemoveGroup(Fresvii.AppSteroid.Models.Group group)
        {
            AUIMessageListCell cell = messageCells.Find(x => x.Group.Id == group.Id);

            if (cell != null)
            {
                contents.RemoveItem(cell.GetComponent<RectTransform>());

                messageCells.Remove(cell);

                Destroy(cell.gameObject);
            }
        }

        public void Back()
        {
            if (frameTween.Animating) return;

            parentFrameTween.gameObject.SetActive(true);

            RectTransform rectTransform = GetComponent<RectTransform>();

            parentFrameTween.Animate(new Vector2(-rectTransform.rect.width * 0.5f, 0f), Vector2.zero, () => { });

            GetComponent<AUIFrame>().Animate(Vector2.zero, new Vector2(rectTransform.rect.width, 0f), () =>
            {
                Destroy(this.gameObject);
            });
        }

        void OnGroupMessageCreated(Fresvii.AppSteroid.Models.GroupMessage groupMessage)
        {
            FASGroup.GetGroupMessageGroupList(OnGetGroupList);
        }

        public void RemoveCell(AUIMessageListCell cell)
        {
            messageCells.Remove(cell);

            contents.RemoveItem(cell.GetComponent<RectTransform>());

            Destroy(cell.gameObject);

            StartCoroutine(DelayLayout());
        }

        IEnumerator DelayLayout()
        {
            yield return 1;

            contents.ReLayout();
        }
    }
}
