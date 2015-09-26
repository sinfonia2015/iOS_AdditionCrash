﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIMessageFriendCell : MonoBehaviour
    {
        public Fresvii.AppSteroid.Models.Friend Friend { get; set; }

        private Fresvii.AppSteroid.Models.User user;

        public AUIFriendList FriendList { get; set; }

        public AUIRawImageTextureSetter userIcon;

        public Text userName;

        private bool isMe;

        public GameObject checkMarkOn, checkMarkOff;
        
        private AUIMessageCreate parentPage;

        public event Action<bool, Fresvii.AppSteroid.Models.Friend> OnSelectStateChanged;

        public event Action<Fresvii.AppSteroid.Models.User> OnGoToUserPage;

        private bool selected;

        public Button buttonCell;

        public void SetFriend(Fresvii.AppSteroid.Models.Friend friend)
        {
            this.Friend = friend;

            GetUser();
        }

        public void Selected(bool on)
        {
            selected = on;

            checkMarkOn.SetActive(selected);

            checkMarkOff.SetActive(!selected);

            if (OnSelectStateChanged != null)
            {
                OnSelectStateChanged(selected, Friend);
            }
        }

        public void SetButtonInteractable(bool on)
        {
            buttonCell.interactable = on;
        }

        void GetUser()
        {
            FASUser.GetUser(Friend.Id, (_user, _error) =>
            {
                if (this == null) return;

                if (_error == null)
                {
                    this.user = _user;
                    
                    userIcon.Set(user.ProfileImageUrl);

                    userName.text = user.Name;
                }
                else
                {
                    Invoke("GetUser", 3f);
                }
            });
        }

        public void GoToUserPage()
        {
            if (user == null) return;

            if (OnGoToUserPage != null)
            {
                OnGoToUserPage(this.user);
            }
        }

        public void OnClickCell()
        {
            selected = !selected;

            if (OnSelectStateChanged != null)
            {
                OnSelectStateChanged(selected, Friend);
            }

            checkMarkOn.SetActive(selected);

            checkMarkOff.SetActive(!selected);

        }
    }
}