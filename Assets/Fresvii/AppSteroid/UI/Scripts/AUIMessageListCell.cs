using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIMessageListCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.Group Group { get; set; }

        public AUIGroupIcon groupIcon;

        public Text groupName;

        public Text message;

        public Text updatedAt;

        AUIMessageList parantPage;

        public Color unreadUpdateAt, readUpdateAt;

        public AUITextSetter groupNameTextSetter;

        public AUICellDeleteAnimator cellDeleteAnimator;

        public AUISlideButton slideButton;

        public AUIScrollPass scrollPass;

        void OnEnable()
        {
            slideButton.OnOpenStateChanged += OnOpenStateChanged;

            StartCoroutine(UpdateUpdatedAt());

            Invoke("CloseSlideButton", 0.1f);

            deleteTapped = false;
        }

        void OnDisable()
        {
            slideButton.OnOpenStateChanged -= OnOpenStateChanged;
        }

        public void SetGroup(Fresvii.AppSteroid.Models.Group group, AUIMessageList parantPage)
        {
            this.Group = group;

            this.parantPage = parantPage;

            Fresvii.AppSteroid.Models.Member member = null;

            //  Icon set
            if (Group.MessageMembers != null)
            {
                for (int i = 0; i < Group.MessageMembers.Count; i++)
                {
                    if (Group.MessageMembers[i].Id != FAS.CurrentUser.Id)
                    {
                        member = Group.MessageMembers[i];

                        break;
                    }                    
                }

                List<string> userIcons = new List<string>();

                for (int i = 0; i < Group.MessageMembers.Count; i++)
                {
                    userIcons.Add(Group.MessageMembers[i].ProfileImageUrl);

                    if (i == 2)
                    {
                        break;
                    }
                }

                groupIcon.Set(userIcons.ToArray());
            }

            // Group member names set
            string memberNames = "";

            if (Group.MembersCount <= 1)
            {
                memberNames = FresviiGUIText.Get("NobodyElse");
            }
            else
            {
                if (Group.Pair && member != null)
                {
                    memberNames = member.Name;
                }
                else
                {
                    for (int i = 0; i < Group.MessageMembers.Count; i++)
                    {
                        memberNames += Group.MessageMembers[i].Name + ((i == Group.MessageMembers.Count - 1) ? "" : ", ");

                        if (i == 5) break;
                    }
                }

                /*if (!Group.Pair)
                {
                    memberNames += "(" + Group.MembersCount + ")";
                }*/
            }

            groupName.text = memberNames;

            if (group.LatestMessage != null)
            {
                if (group.LatestMessage.Type == Models.GroupMessage.ContentType.Text)
                {
                      message.text = group.LatestMessage.Text;
                }
                else if (group.LatestMessage.Type == Models.GroupMessage.ContentType.Image)
                {
                    if (Group.LatestMessage.User.Id == FAS.CurrentUser.Id)
                    {
                        message.text = FASText.Get("UserSentAPhoto").Replace("%username", FASText.Get("You"));
                    }
                    else
                    {
                        message.text = FASText.Get("UserSentAPhoto").Replace("%username", Group.LatestMessage.User.Name);
                    }
                }
                else if (group.LatestMessage.Type == Models.GroupMessage.ContentType.Video)
                {
                    if (Group.LatestMessage.User.Id == FAS.CurrentUser.Id)
                    {
                        message.text = FASText.Get("UserSentAVideo").Replace("%username", FASText.Get("You"));
                    }
                    else
                    {
                        message.text = FASText.Get("UserSentAVideo").Replace("%username", Group.LatestMessage.User.Name);
                    }
                }
                else if (group.LatestMessage.Type == Models.GroupMessage.ContentType.Sticker)
                {
                    if (Group.LatestMessage.User.Id == FAS.CurrentUser.Id)
                    {
                        message.text = FASText.Get("UserSentASticker").Replace("%username", FASText.Get("You"));
                    }
                    else
                    {
                        message.text = FASText.Get("UserSentASticker").Replace("%username", Group.LatestMessage.User.Name);
                    }
                }
            }

            bool isUnred = IsUnread();

            groupName.fontStyle = message.fontStyle = (IsUnread()) ?  FontStyle.Bold : FontStyle.Normal;

            updatedAt.color = (IsUnread()) ? unreadUpdateAt : readUpdateAt;

            if (isUnred && !group.Hidden)
            {
                AUITabBar.Instance.AddUnreadGroup(Group.Id);
            }

            if (!Group.Pair)
            {
                groupNameTextSetter.truncatedReplacement = "...(" + Group.MembersCount.ToString() + ")";
            }

            updatedAt.text = AUIUtility.CurrentTimeSpan(Group.LatestMessage.CreatedAt);

            updatedAt.rectTransform.sizeDelta = new Vector2(20f + updatedAt.preferredWidth, updatedAt.rectTransform.sizeDelta.y);

            groupName.rectTransform.sizeDelta = new Vector2(this.GetComponent<RectTransform>().rect.width - 190f - updatedAt.rectTransform.sizeDelta.x, groupName.rectTransform.sizeDelta.y);

            postScreenWidth = Screen.width;
        }

        bool IsUnread()
        {
            bool unread;

            if (Group.LatestMessage == null)
            {
                unread = false;
            }
            else
            {
                unread = (Group.LastReadMessageId != Group.LatestMessage.Id) || string.IsNullOrEmpty(Group.LastReadMessageId);
            }

            if (Group.LatestMessage == null)
            {
                unread = false;
            }
            else if (Group.LatestMessage.User.Id == FAS.CurrentUser.Id)
            {
                unread = false;
            }

            return unread;
        }

        public void Read()
        {
            AUITabBar.Instance.GroupMessageRead(Group.Id);

            message.fontStyle = FontStyle.Normal;

            Group.LastReadMessageId = Group.LatestMessage.Id;
        }

        float postScreenWidth;

        void LateUpdate()
        {
            if (postScreenWidth != Screen.width)
            {
                updatedAt.text = AUIUtility.CurrentTimeSpan(Group.LatestMessage.CreatedAt);

                updatedAt.rectTransform.sizeDelta = new Vector2(20f + updatedAt.preferredWidth, updatedAt.rectTransform.sizeDelta.y);

                groupName.rectTransform.sizeDelta = new Vector2(this.GetComponent<RectTransform>().rect.width - 190f - updatedAt.rectTransform.sizeDelta.x, groupName.rectTransform.sizeDelta.y);

                groupName.GetComponent<AUITextSetter>().TruncateImediately();

                postScreenWidth = Screen.width;
            }

            if (openState == AUISlideButton.OpenState.RightOpen)
            {
                if (Input.GetMouseButtonDown(0) && !deleteTapped)
                {
                    Invoke("CloseSlideButton", 0.1f);
                }
            }
        }

        void CloseSlideButton()
        {
            slideButton.Close();
        }

        IEnumerator UpdateUpdatedAt()
        {
            while (true)
            {
                if (Group != null && Group.LatestMessage != null)
                {
                    yield return new WaitForSeconds(60f);

                    updatedAt.text = AUIUtility.CurrentTimeSpan(Group.LatestMessage.CreatedAt);

                    updatedAt.rectTransform.sizeDelta = new Vector2(20f + updatedAt.preferredWidth, updatedAt.rectTransform.sizeDelta.y);

                    groupName.rectTransform.sizeDelta = new Vector2(this.GetComponent<RectTransform>().rect.width - 190f - updatedAt.rectTransform.sizeDelta.x, groupName.rectTransform.sizeDelta.y);
                }
                else
                {
                    yield return 1;
                }
            }
        }

        public void GoToMessage()
        {
            if (Group != null)
            {
                parantPage.GoToMessage(Group, true);
            }

            Read();
        }

        bool deleteTapped;

        public void OnClickDeleteButton()
        {
            deleteTapped = true;

            DeleteGroupMessage();
        }

        void DeleteGroupMessage()
        {
            
            if (Group.Pair)
            {
                Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("NotArrowDeletePair"), delegate(bool del) { });

                Debug.LogError(FASText.Get("NotArrowDeletePair"));

                Invoke("CloseSlideButton", 0.1f);

                deleteTapped = false;

                return;
            }
           
            Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSelectDialog(FASText.Get("ConfirmDeleteGroup"), delegate(bool del)
            {
                if (!del)
                {
                    Invoke("CloseSlideButton", 0.1f);

                    deleteTapped = false;
                }
                else
                {
                    if (Group.MembersCount <= 1)
                    {
                        FASGroup.DeleteGroup(Group.Id, delegate(Fresvii.AppSteroid.Models.Error error)
                        {
                            if (error != null)
                            {
                                Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), delegate(bool _del) { });

                                if (FASConfig.Instance.logLevel <= FAS.LogLevels.Error)
                                {
                                    Debug.LogError("GroupService.DeleteGroup : " + error.ToString());
                                }

                                Invoke("CloseSlideButton", 0.1f);

                                deleteTapped = false;
                            }
                            else
                            {
                                if (IsUnread())
                                {
                                    AUITabBar.Instance.GroupMessageRead(this.Group.Id);
                                }

                                cellDeleteAnimator.Animate(this.parantPage.contents, (size) =>
                                {
                                    this.parantPage.RemoveCell(this);
                                });
                            }
                        });
                    }
                    else
                    {
                        FASGroup.DeleteMember(Group.Id, FAS.CurrentUser.Id, delegate(Fresvii.AppSteroid.Models.Error error)
                        {
                            if (error != null)
                            {
                                Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                                Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), delegate(bool _del) { });

                                if (FASConfig.Instance.logLevel <= FAS.LogLevels.Error)
                                {
                                    Debug.LogError("GroupService.DeleteMember : " + error.ToString());
                                }

                                Invoke("CloseSlideButton", 0.1f);

                                deleteTapped = false;
                            }
                            else
                            {
                                if (IsUnread())
                                {
                                    AUITabBar.Instance.GroupMessageRead(this.Group.Id);
                                }

                                cellDeleteAnimator.Animate(this.parantPage.contents, (size) =>
                                {
                                    this.parantPage.RemoveCell(this);
                                });
                            }
                        });
                    }
                }
            });            
        }

        public void OnClickHideButton()
        {
            deleteTapped = true;

            FASGroup.HideGroup(Group.Id, (group, error) =>
            {
                if (error != null)
                {
                    Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                    Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), delegate(bool del) { });

                    if (FASConfig.Instance.logLevel <= FAS.LogLevels.Error)
                    {
                        Debug.LogError("GroupService.HideGroup : " + error.ToString());
                    }
                }
                else
                {
                    if (IsUnread())
                    {
                        AUITabBar.Instance.GroupMessageRead(this.Group.Id);
                    }

                    cellDeleteAnimator.Animate(this.parantPage.contents, (size) =>
                    {
                        this.parantPage.RemoveCell(this);
                    });
                }
            });
        }

        AUISlideButton.OpenState openState;

        void OnOpenStateChanged(AUISlideButton.OpenState openState)
        {
            this.openState = openState;
        }
    }
}