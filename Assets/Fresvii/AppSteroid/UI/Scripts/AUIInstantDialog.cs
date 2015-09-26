using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    public class AUIInstantDialog : MonoBehaviour
    {
        public float fadeTime;

        public float showTime;

        public static AUIInstantDialog instance;

        public Graphic graphic;

        public Text text;

        public RectTransform rectTransform;

        public Vector2 margin;

        void Awake()
        {
            instance = this;
        }

        public static void Show(string text)
        {
            instance.text.text = text;

            instance.gameObject.SetActive(true);
        }

        void OnEnable()
        {
            StartCoroutine(Animate());
        }

        IEnumerator Animate()
        {            
            graphic.CrossFadeAlpha(0f, 0f, true);

            text.CrossFadeAlpha(0f, 0f, true);

            yield return 1;

            this.rectTransform.sizeDelta = new Vector2(text.preferredWidth, text.preferredHeight) + margin;

            graphic.CrossFadeAlpha(1f, fadeTime, true);

            text.CrossFadeAlpha(1f, fadeTime, true);

            yield return new WaitForSeconds(fadeTime + showTime);

            graphic.CrossFadeAlpha(0f, fadeTime, true);

            text.CrossFadeAlpha(0f, fadeTime, true);

            yield return new WaitForSeconds(fadeTime);

            this.gameObject.SetActive(false);
        }
    }
}
