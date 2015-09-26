using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fresvii.AppSteroid.UI
{
    public class AUIMyPageSettingMenu : MonoBehaviour
    {
        public AUIFrame frame;

        public GameObject prfbMyProfileEdit;

        public GameObject prfbHiddenGroups;

        void OnEnable()
        {
            AUIManager.OnEscapeTapped += Back;
        }

        void OnDisable()
        {
            AUIManager.OnEscapeTapped -= Back;
        }

        public void GoToMyProfileEdit()
        {
            if (frame.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            GameObject myProfileEdit = Instantiate(prfbMyProfileEdit) as GameObject;

            myProfileEdit.GetComponent<RectTransform>().SetParent(transform.parent, false);

            myProfileEdit.transform.SetAsLastSibling();

            AUIMyProfileEdit auiMyProfileEdit = myProfileEdit.GetComponent<AUIMyProfileEdit>();

            auiMyProfileEdit.frame.backFrame = this.frame;

            frame.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });

            auiMyProfileEdit.frame.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () =>
            {

            });
        }

        public void GoToHiddenGroups()
        {
            if (frame.Animating) return;

            RectTransform rectTransform = GetComponent<RectTransform>();

            GameObject hiddenGroups = Instantiate(prfbHiddenGroups) as GameObject;

            hiddenGroups.GetComponent<RectTransform>().SetParent(transform.parent, false);

            hiddenGroups.transform.SetAsLastSibling();

            AUIHiddenMessageList auiHiddenGroups = hiddenGroups.GetComponent<AUIHiddenMessageList>();

            auiHiddenGroups.frame.backFrame = this.frame;

            frame.Animate(Vector2.zero, new Vector2(-rectTransform.rect.width * 0.5f, 0f), () =>
            {
                this.gameObject.SetActive(false);
            });

            auiHiddenGroups.frame.Animate(new Vector2(rectTransform.rect.width, 0f), Vector2.zero, () =>
            {

            });
        }

        public void Back()
        {
            frame.backFrame.gameObject.SetActive(true);

            RectTransform rectTransform = GetComponent<RectTransform>();

            frame.backFrame.Animate(new Vector2(-rectTransform.rect.width * 0.5f, 0f), Vector2.zero, () => { });

            frame.Animate(Vector2.zero, new Vector2(rectTransform.rect.width, 0f), () =>
            {
                Destroy(this.gameObject);
            });
        }
    }
}