using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ironcow
{
    public class UIListItem : MonoAutoCaching
#if USE_LOCALE
        , ILocale
    {
        public List<LocaleText> texts;
#else
    { 
#endif

        /// <summary>
        /// 현재 UI상 순서에 해당하는 값
        /// </summary>

        public int index { get => transform.GetSiblingIndex(); }

        public RectTransform rectTransform { get => transform as RectTransform; }

#if USE_LOCALE
        void Awake()
        {
            foreach (var text in texts)
            {
                text.text.text = LocaleDataSO.SharedInstance.LocaleDic[text.key];
            }
        }
        public void SetLocaleTexts()
        {
            texts.Clear();
            var tmpTexts = GetComponentsInChildren<TMPro.TMP_Text>(true).ToList();
            tmpTexts.ForEach(text =>
            {
                var localeData = LocaleDataSO.SharedInstance.localeData.Find(obj => obj.Korean == text.text);
                if (localeData != null)
                {
                    texts.Add(new LocaleText(localeData.Key, text));
                }
            });
        }
#endif

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

    }
}