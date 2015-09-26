using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Fresvii.AppSteroid.UI
{
    public class AUIEventCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.GameEvent GameEvent { get; protected set; }

        public AUIRawImageTextureSetter clipImage;

        public Text title;

        public Text duration;

        public Text description;

        private RectTransform clipImageRectTransform;

        public float margin;

        public RectTransform bottomButtonRectTransform;

        AUIScrollViewContents contents;

        AUIEvents auiEvents;

        public Button buttonCell;

        void OnEnable()
        {
            AUIManager.OnScreenSizeChanged += OnScreenSizeChanged;

			if(contents != null)
				contents.ReLayout();
        }

        void OnDisable()
        {
            AUIManager.OnScreenSizeChanged -= OnScreenSizeChanged;
        }

        public void SetGameEvent(Fresvii.AppSteroid.Models.GameEvent gameEvent, AUIEvents auiEvents, AUIScrollViewContents contents, Action callback)
        {
            this.contents = contents;

            this.GameEvent = gameEvent;

            this.auiEvents = auiEvents;

            if (buttonCell != null)
            {
                buttonCell.gameObject.SetActive(auiEvents != null);
            }

            this.gameObject.SetActive(false);

            if (string.IsNullOrEmpty(GameEvent.ImageLargeUrl))
            {
                FASLeaderboard.GetEvent(GameEvent.Id, (ge, error) =>
                {
                    if (error == null)
                    {
                        SetGameEvent(ge, auiEvents, this.contents, callback);

                        SetLayout();
                    }
                });
            }
            else
            {
                this.gameObject.SetActive(true);

                clipImage.Set(GameEvent.ImageLargeUrl);

                title.text = GameEvent.Name;

                duration.text = GameEvent.StartAt.ToLocalTime().ToString(FASText.Get("LocalDateTimeFormat")) + " - " + ((GameEvent.EndAt != System.DateTime.MaxValue) ? GameEvent.EndAt.ToLocalTime().ToString(FASText.Get("LocalDateTimeFormat")) : "");

                description.text = GameEvent.Description;

                callback();

                SetLayout();
            }
        }

        string ToDateString(string str)
        {
            if (str[0] == '0')
            {
                return str.Substring(1);
            }
            else
            {
                return str;
            }
        }

        IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            SetLayout();
        }

        public void SetClipImage(Texture2D texture)
        {
            clipImage.SetTexture(texture);
        }

        void OnScreenSizeChanged()
        {
            SetLayout();
        }

        public float Height { get; protected set; }

        void SetLayout()
        {
            if(contents != null)
                contents.ReLayout();
        }

        public void OnCellClicked()
        {
            if (auiEvents != null)
            {
                if (!string.IsNullOrEmpty(GameEvent.WebSiteUrl))
                {
                    FASUtility.SendPageView("pv.community.events.show", GameEvent.Id, System.DateTime.UtcNow, (e) =>
                    {
                        if (e != null)
                            Debug.LogError(e.ToString());

                        Application.OpenURL(GameEvent.WebSiteUrl);
                    });
                }
                else
                {
                    auiEvents.GoToGameEvent(GameEvent);
                }
            }
        }

        public void ShareTwitter()
        {
			Debug.Log ("ShareTwitter");

            string text = (string.IsNullOrEmpty(FAS.AppPreference.EventShareText)) ? "" : FAS.AppPreference.EventShareText;

            var urls = new List<string>();

            if (this.GameEvent.App != null && !string.IsNullOrEmpty(this.GameEvent.App.StoreUrl))
            {
                urls.Add(this.GameEvent.App.StoreUrl);
            }
            else if (!string.IsNullOrEmpty(FAS.AppPreference.StoreUrl))
            {
                urls.Add(FAS.AppPreference.StoreUrl);
            }

            if (!string.IsNullOrEmpty(this.GameEvent.WebSiteUrl))
            {
                if (urls.Count == 0)
                {
                    urls.Add(this.GameEvent.WebSiteUrl);
                }
                else
                {
                    text += " " + this.GameEvent.WebSiteUrl;
                }
            }


            Fresvii.AppSteroid.Util.SocialNetworkingService.ShareTwitterWithUI(text, urls.ToArray(), (result) =>
            {
                if (result == Fresvii.AppSteroid.Util.SocialNetworkingService.Result.Error)
                {
                    Debug.LogError("FASSNS.ShareTwitter : error");

                    Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("Yes"), FASText.Get("No"), FASText.Get("Close"));

                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("TwitterError"), (del) => { });
                }
            });

        }

        public void ShareFacebook()
        {
			Debug.Log ("ShareFacebook");

            string text = (string.IsNullOrEmpty(FAS.AppPreference.EventShareText)) ? "" : FAS.AppPreference.EventShareText;

            var urls = new List<string>();

            if (this.GameEvent.App != null && !string.IsNullOrEmpty(this.GameEvent.App.StoreUrl))
            {
                urls.Add(this.GameEvent.App.StoreUrl);
            }
            else if (!string.IsNullOrEmpty(FAS.AppPreference.StoreUrl))
            {
                urls.Add(FAS.AppPreference.StoreUrl);
            }

            if (!string.IsNullOrEmpty(this.GameEvent.WebSiteUrl))
            {
                if (urls.Count == 0)
                {
                    urls.Add(this.GameEvent.WebSiteUrl);
                }
                else
                {
                    text += " " + this.GameEvent.WebSiteUrl;
                }
            }


            Fresvii.AppSteroid.Util.SocialNetworkingService.ShareFacebook(text, urls.ToArray(), (result) =>
            {
                if (result == Fresvii.AppSteroid.Util.SocialNetworkingService.Result.Error)
                {
                    Debug.LogError("FASSNS.ShareFacebook : error");

                    Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("Yes"), FASText.Get("No"), FASText.Get("Close"));

                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("FacebookError"), (del) => { });
                }
            });
        }
    }
}