﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Fresvii.AppSteroid.UI
{
    public class AUIPreviewVideoList : MonoBehaviour
    {
        enum Mode { Advertisement, Apps };

        Mode mode;

        public AUIFrame frame;

        AUIMoreApps auiMoreApps;

        public GameObject prfbPreviewVideoCell;

        public AUIScrollViewContents contents;

        private List<AUIPreviewDetailCell> cells = new List<AUIPreviewDetailCell>();

        public Fresvii.AppSteroid.Models.ListMeta PreviewListMeta;

        private bool isPullRefleshProc;

        public AUIScrollViewPullReflesh pullReflesh;

        public AUIScrollRect scrollView;

        public IList<Fresvii.AppSteroid.Models.Video> PreviewVideos { get; set; }

        public Fresvii.AppSteroid.Models.App App { get; set; }

        public uint loadCount = 25;

        string query = "{\"order\": {\"created_at\": \"desc\"}}";

        void OnEnable()
        {
            AUIManager.OnEscapeTapped += Back;

            pullReflesh.OnPullUpReflesh += OnPullUpReflesh;

            StartCoroutine(Init());
        }

        void OnDisable()
        {
            AUIManager.OnEscapeTapped -= Back;

            pullReflesh.OnPullUpReflesh -= OnPullUpReflesh;

            AUIManager.Instance.HideLoadingSpinner();
        }

        void OnPullUpReflesh()
        {
            if (PreviewListMeta != null && PreviewListMeta.NextPage.HasValue && App != null)
            {
                isPullRefleshProc = true;

                FASApps.GetVideoList(App.Id, (uint)PreviewListMeta.NextPage, null, query, OnGetVideoList);
            }
            else
            {
                pullReflesh.PullRefleshCompleted();
            }
        }

        IEnumerator Init()
        {
            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            while (frame.Animating)
            {
                yield return 1;
            }

            yield return 1;

            AUIManager.Instance.ShowLoadingSpinner();

            if (App == null)
            {
                mode = Mode.Advertisement;

                FASUtility.SendPageView("pv.app_gallery.show.previews", "", System.DateTime.UtcNow, (e) =>
                {
                    if (e != null)
                        Debug.LogError(e.ToString());
                });

                FASAdvertisement.GetVideoList(loadCount, OnGetVideoList);
            }
            else
            {
                mode = Mode.Apps;

                FASUtility.SendPageView("pv.apps.previews", App.Id, System.DateTime.UtcNow, (e) =>
                {
                    if (e != null)
                        Debug.LogError(e.ToString());
                });

                FASApps.GetVideoList(App.Id, 1, loadCount, query, OnGetVideoList);
            }
        }

        void OnGetVideoList(IList<Fresvii.AppSteroid.Models.Video> videos, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
        {
            AUIManager.Instance.HideLoadingSpinner();

            if (this == null)
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

            if (this.PreviewListMeta == null)
            {
                this.PreviewListMeta = meta;
            }
            else if (this.PreviewListMeta.CurrentPage != 0)
            {
                this.PreviewListMeta = meta;
            }

            foreach (Fresvii.AppSteroid.Models.Video video in videos)
            {
                if (mode == Mode.Apps)
                {
                    video.App = App;
                }

                UpdateVideo(video);
            }

            if (isPullRefleshProc)
            {
                pullReflesh.PullRefleshCompleted();

                isPullRefleshProc = false;
            }
        }

        private bool UpdateVideo(Fresvii.AppSteroid.Models.Video video)
        {
            var cell = cells.Find(x => x.Video.Id == video.Id);

            if (cell != null)
            {
                cell.SetPreview(video, this);

                return false;
            }

            GameObject go = Instantiate(prfbPreviewVideoCell) as GameObject;

            var item = go.GetComponent<RectTransform>();

            contents.AddItem(item);

            cell = item.GetComponent<AUIPreviewDetailCell>();

            cell.DetaiButtonEnabled = (mode == Mode.Advertisement);

            cell.SetPreview(video, this);

            cells.Add(cell);

            cell.gameObject.SetActive(false);

            return true;
        }

        public void Back()
        {
            if (frame.Animating) return;

            frame.backFrame.gameObject.SetActive(true);

            RectTransform rectTransform = GetComponent<RectTransform>();

            frame.backFrame.Animate(new Vector2(-rectTransform.rect.width * 0.5f, 0f), Vector2.zero, () => { });

            frame.Animate(Vector2.zero, new Vector2(rectTransform.rect.width, 0f), () =>
            {
                Destroy(this.gameObject);
            });
        }

        public GameObject prfbAppDetail;

        public void GoToAppDetail(Fresvii.AppSteroid.Models.App app)
        {
            if (frame.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            var auiAppDetail = ((GameObject)Instantiate(prfbAppDetail)).GetComponent<AUIAppDetail>();

            auiAppDetail.SetApp(app);

            auiAppDetail.transform.SetParent(transform.parent, false);

            auiAppDetail.transform.SetAsLastSibling();

            auiAppDetail.frame.backFrame = this.frame;

            auiAppDetail.frame.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () => { });

            this.frame.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });
        }

    }
}
