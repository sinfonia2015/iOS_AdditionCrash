using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIHiddenMessageListCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.Group Group { get; set; }

        public AUIGroupIcon groupIcon;

        public Text groupName;

        public Text message;

        AUIHiddenMessageList parantPage;

        public AUITextSetter groupNameTextSetter;

        public AUICellDeleteAnimator cellDeleteAnimator;

        public void SetGroup(Fresvii.AppSteroid.Models.Group group, AUIHiddenMessageList parantPage)
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

            groupName.fontStyle = message.fontStyle = (IsUnread()) ? FontStyle.Bold : FontStyle.Normal;

            if (isUnred && !group.Hidden)
            {
                AUITabBar.Instance.AddUnreadGroup(Group.Id);
            }

            if (!Group.Pair)
            {
                groupNameTextSetter.truncatedReplacement = "...(" + Group.MembersCount.ToString() + ")";
            }

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
                groupName.GetComponent<AUITextSetter>().TruncateImediately();

                postScreenWidth = Screen.width;
            }
        }

        public void GoToMessage()
        {
            if (Group != null)
            {
                parantPage.GoToMessage(Group, true);
            }
        }

        public void OnClickShowButton()
        {
            Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

            Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSelectDialog(FASText.Get("ConfirmShowGroup"), delegate(bool del)
            {
                if (!del) return;

                FASGroup.ShowGroup(Group.Id, (group, error) =>
                {
                    if (error != null)
                    {
                        Fresvii.AppSteroid.Util.DialogManager.Instance.SetLabel(FASText.Get("OK"), FASText.Get("Cancel"), FASText.Get("Close"));

                        Fresvii.AppSteroid.Util.DialogManager.Instance.ShowSubmitDialog(FASText.Get("UnknownError"), delegate(bool _del) { });

                        if (FASConfig.Instance.logLevel <= FAS.LogLevels.Error)
                        {
                            Debug.LogError("GroupService.ShowGroup : " + error.ToString());
                        }
                    }
                    else
                    {
                        if (IsUnread())
                        {
                            AUITabBar.Instance.AddUnreadGroup(this.Group.Id);
                        }

                        cellDeleteAnimator.Animate(this.parantPage.contents, (size) =>
                        {
                            this.parantPage.RemoveCell(this);
                        });
                    }
                });
            });


        }
    }
}