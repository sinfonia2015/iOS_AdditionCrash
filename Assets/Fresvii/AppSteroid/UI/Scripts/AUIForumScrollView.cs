using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIForumScrollView : MonoBehaviour
    {
        public AUIForum auiForum;

        public enum Mode { All, Images, Videos };

        public Mode mode;

        public AUIScrollViewContents contents;

        [HideInInspector]
        public List<AUIForumThreadCell> threadCells = new List<AUIForumThreadCell>();

        [HideInInspector]
        public List<AUICommentGridCell> gridCells = new List<AUICommentGridCell>();

        public AUIScrollViewPullReflesh pullReflesh;

        private Fresvii.AppSteroid.Models.ListMeta listMeta;

        private bool isPullUpRefleshProc, isPullDownRefleshProc;

        public AUIScrollRect scrollView;

        public RectTransform prfbThreadCell;

        public RectTransform prfbCommentGridCell;

        private bool loaded;

        uint? maxPage = 1;

        private readonly uint ImagePerPage = 24;

        public GameObject noData;

        public AUIVerticalLayoutPullReflesh pullReflesh2;

        void OnEnable()
        {
            isPullUpRefleshProc = isPullDownRefleshProc = false;

            pullReflesh.Reset();

            pullReflesh.OnPullUpReflesh += OnPullUpReflesh;

            if (pullReflesh2 != null)
                pullReflesh2.OnPullDownReflesh += OnPullDownReflesh;
            else
                pullReflesh.OnPullDownReflesh += OnPullDownReflesh;

            contents.ReLayout();

            noData.SetActive(listMeta != null && listMeta.TotalCount == 0);

            StartCoroutine(Init());
        }

        void OnDisable()
        {
            pullReflesh.OnPullUpReflesh -= OnPullUpReflesh;

            if (pullReflesh2 != null)
                pullReflesh2.OnPullDownReflesh -= OnPullDownReflesh;
            else
                pullReflesh.OnPullDownReflesh -= OnPullDownReflesh;

            AUIManager.Instance.HideLoadingSpinner();
        }

        void OnPullDownReflesh()
        {
            isPullDownRefleshProc = true;

            if (mode == Mode.All)
            {
                FASForum.GetForumThreads(OnGetForumThreads);
            }
            else if (mode == Mode.Images)
            {
                string query = "{\"where\":[{\"column\": \"image\", \"operator\": \"!=\", \"value\": null}],\"operation\": \"any\",\"order\": {\"created_at\": \"desc\"}}";

                Fresvii.AppSteroid.FASForum.GetCommentList(1, ImagePerPage, query, OnGetImageAndVideoComments);
            }
            else if (mode == Mode.Videos)
            {
                string query = "{\"where\":[{\"column\": \"video_id\", \"operator\": \"!=\", \"value\": null}],\"operation\": \"any\",\"order\": {\"created_at\": \"desc\"}}";

                Fresvii.AppSteroid.FASForum.GetCommentList(1, ImagePerPage, query, OnGetImageAndVideoComments);
            }
        }

        void OnPullUpReflesh()
        {
            if (listMeta != null && listMeta.TotalPages >= maxPage && !isPullUpRefleshProc)
            {                
                isPullUpRefleshProc = true;

                maxPage++;

                if (mode == Mode.All)
                {
                    FASForum.GetForumThreads((uint)maxPage, OnGetForumThreads);
                }
                else if (mode == Mode.Images)
                {
                    string query = "{\"where\":[{\"column\": \"image\", \"operator\": \"!=\", \"value\": null}],\"operation\": \"any\",\"order\": {\"created_at\": \"desc\"}}";

                    Fresvii.AppSteroid.FASForum.GetCommentList((uint)maxPage, ImagePerPage, query, OnGetImageAndVideoComments);
                }
                else if (mode == Mode.Videos)
                {
                    string query = "{\"where\":[{\"column\": \"video_id\", \"operator\": \"!=\", \"value\": null}],\"operation\": \"any\",\"order\": {\"created_at\": \"desc\"}}";

                    Fresvii.AppSteroid.FASForum.GetCommentList((uint)maxPage, ImagePerPage, query, OnGetImageAndVideoComments);
                }
            }
            else
            {
                pullReflesh.PullRefleshCompleted();
            }
        }

        // Use this for initialization
        IEnumerator Init()
        {
            if (!this.gameObject.activeSelf)
            {
                yield break;
            }

            if (listMeta == null)
                AUIManager.Instance.ShowLoadingSpinner();

            yield return 1;

            while (!FASUser.IsLoggedIn() || auiForum.frame.Animating)
            {
                yield return 1;
            }

            yield return 1;

            if (!loaded && mode == Mode.All)
            {
                FASForum.GetForumThreadsFromCache(OnGetForumThreads);
            }

            if (mode == Mode.All)
            {
                if (!string.IsNullOrEmpty(auiForum.showThreadId))
                {
                    var cell = threadCells.Find(x => x.Thread.Id == auiForum.showThreadId);

                    if (cell != null)
                    {
                        auiForum.GoToThread(cell, auiForum.showComment, auiForum.showThreadWithAnimation, true);

                        if (auiForum.showThreadRedayCallback != null)
                        {
                            auiForum.showThreadRedayCallback(null);
                        }

                        auiForum.showThreadId = null;

                        auiForum.showThreadWithAnimation = false;

                        auiForum.showComment = null;

                        auiForum.showThreadRedayCallback = null;
                    }
                    else
                    {
                        AddThreadCell(auiForum.showThreadId, (_cell, _error) => 
                        {
                            if (auiForum.showThreadRedayCallback != null)
                            {
                                auiForum.showThreadRedayCallback(_error);
                            }

                            if (_error == null)
                            {
                                auiForum.GoToThread(_cell, auiForum.showComment, auiForum.showThreadWithAnimation, true);
                            }

                            auiForum.showThreadId = null;

                            auiForum.showThreadWithAnimation = false;

                            auiForum.showComment = null;

                            auiForum.showThreadRedayCallback = null;
                        });
                    }
                }
                else
                {
                    FASForum.GetForumThreads(OnGetForumThreads);

                    if (auiForum.showThreadRedayCallback != null)
                    {
                        auiForum.showThreadRedayCallback(null);
                    }

                    auiForum.showThreadId = null;

                    auiForum.showThreadWithAnimation = false;

                    auiForum.showComment = null;

                    auiForum.showThreadRedayCallback = null;
                }
            }
            else if (mode == Mode.Images)
            {
                string query = "{\"where\":[{\"column\": \"image\", \"operator\": \"!=\", \"value\": null}],\"operation\": \"any\",\"order\": {\"created_at\": \"desc\"}}";

                Fresvii.AppSteroid.FASForum.GetCommentList(1, ImagePerPage, query, OnGetImageAndVideoComments);
            }
            else if (mode == Mode.Videos)
            {
                string query = "{\"where\":[{\"column\": \"video_id\", \"operator\" : \"!=\", \"value\": null}],\"order\": {\"created_at\": \"desc\"}}";

                Fresvii.AppSteroid.FASForum.GetCommentList(1, ImagePerPage, query, OnGetImageAndVideoComments);
            }

			contents.ReLayout();

        }

        IEnumerator SetContentPadding()
        {
            yield return new WaitForEndOfFrame();

            contents.padding.bottom += (AUITabBar.Instance.gameObject.activeInHierarchy) ? AUITabBar.Instance.GetHeight() : 0;
        }

        public void AddThreadCell(string threadId, Action<AUIForumThreadCell, Fresvii.AppSteroid.Models.Error> callback)
        {
            FASForum.GetThread(threadId, (thread, error) =>
            {
                if (error == null)
                {
                    var item = GameObject.Instantiate(prfbThreadCell) as RectTransform;

                    contents.AddItem(item);

                    AUIForumThreadCell cell = item.GetComponent<AUIForumThreadCell>();

                    cell.auiForum = auiForum;

                    cell.SetThraed(thread);

                    threadCells.Add(cell);

                    callback(cell, null);
                }
                else
                {
                    callback(null, error);
                }
            });
        }

        private void OnGetImageAndVideoComments(IList<Fresvii.AppSteroid.Models.Comment> comments, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
        {
            AUIManager.Instance.HideLoadingSpinner();

            if (isPullUpRefleshProc)
            {
                isPullDownRefleshProc = isPullUpRefleshProc = false;

                pullReflesh.PullRefleshCompleted();
            }

            if (isPullDownRefleshProc)
            {
                isPullDownRefleshProc = isPullUpRefleshProc = false;

                pullReflesh.PullRefleshCompleted();

                if (pullReflesh2 != null)
                {
                    pullReflesh2.PullRefleshCompleted();
                }
            }

            if (this == null || this.enabled == false)
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

            noData.SetActive(listMeta != null && listMeta.TotalCount == 0);

            if (this.gameObject.activeInHierarchy)
            {
                StartCoroutine(LoadImages(comments));
            }
        }

        public Transform gridContents;

        IEnumerator LoadImages(IList<Fresvii.AppSteroid.Models.Comment> comments)
        {
            int i = 0;

            foreach (var comment in comments)
            {
                if (comment.VideoState == Models.Comment.VideoStatus.Removed)
                {
                    continue;
                }

                var cell = gridCells.Find(x => x.Comment.Id == comment.Id);

                if (cell != null)
                {
                    cell.SetComment(comment, this.auiForum);

                    continue;
                }

                var item = GameObject.Instantiate(prfbCommentGridCell) as RectTransform;

                //contents.AddItem(item);

                cell = item.GetComponent<AUICommentGridCell>();

                cell.SetComment(comment, this.auiForum);

                gridCells.Add(cell);

                cell.transform.SetParent(gridContents, false);

                if(++i%2 == 0)  
                    yield return 1;
            }

            gridCells.Sort((a, b) => System.DateTime.Compare(b.Comment.CreatedAt, a.Comment.CreatedAt));

            foreach (var obj in gridCells)
            {
                obj.transform.SetSiblingIndex(contents.transform.childCount - 1);
            }

            contents.ReLayout();

            yield return 1;
        }

        private void OnGetForumThreads(IList<Fresvii.AppSteroid.Models.Thread> threads, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
        {
            AUIManager.Instance.HideLoadingSpinner();

            if (this == null)
            {
                return;
            }

            if (this.enabled == false)
            {
                return;
            }

            if (isPullDownRefleshProc || isPullUpRefleshProc)
            {
                pullReflesh.PullRefleshCompleted();

                isPullUpRefleshProc = isPullDownRefleshProc = false;
            }

            if (error != null)
            {
                if (FASSettings.Instance.logLevel <= FAS.LogLevels.Error)
                {
                    Debug.LogError(error.ToString());
                }

                return;
            }

            loaded = true;

            if (this.listMeta == null || this.listMeta.CurrentPage != 1)
            {
                this.listMeta = meta;
            }

            noData.SetActive(listMeta != null && listMeta.TotalCount == 0);

            bool added = false;

            foreach (Fresvii.AppSteroid.Models.Thread thread in threads)
            {
                added |= UpdateThread(thread);
            }

            Sort();
        }

        public void Sort()
        {
            // Sort
            threadCells.Sort((a, b) => System.DateTime.Compare(b.Thread.LastUpdateAt, a.Thread.LastUpdateAt));

            foreach (var obj in threadCells)
            {
                obj.transform.SetSiblingIndex(contents.transform.childCount - 1);
            }

            contents.ReLayout();
        }

        public void LoadLatestThreads()
        {
            FASForum.GetForumThreads(OnGetForumThreads);
        }

        private bool UpdateThread(Fresvii.AppSteroid.Models.Thread thread)
        {
            AUIForumThreadCell cell = threadCells.Find(x => x.Thread.Id == thread.Id);

            if (cell != null)
            {
                cell.SetThraed(thread);

                return false;
            }

            var item = GameObject.Instantiate(prfbThreadCell) as RectTransform;

            contents.AddItem(item);

            cell = item.GetComponent<AUIForumThreadCell>();

            cell.auiForum = auiForum;

            cell.SetThraed(thread);

            threadCells.Add(cell);

            cell.gameObject.SetActive(true);

            return true;
        }

        public void RemoveCell(AUIForumThreadCell cell)
        {
            var removeCell = threadCells.Find(x => x.Thread.Id == cell.Thread.Id);

            if (removeCell != null)
            {
                threadCells.Remove(removeCell);

                contents.RemoveItem(removeCell.GetComponent<RectTransform>());

                Destroy(removeCell.gameObject);

                Sort();
            }
        }
    }
}
