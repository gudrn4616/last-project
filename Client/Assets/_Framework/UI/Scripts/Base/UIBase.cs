using Ironcow;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Ironcow
{
    [System.Serializable]
    public class UIOptions
    {
        public bool isActiveOnLoad = true;
        public bool isDestroyOnHide = true;
    }

#if USE_LOCALE
[System.Serializable]
public class LocaleText
{
    public string key;
    public TMP_Text text;

    public LocaleText(string key, TMP_Text text)
    {
        this.key = key;
        this.text = text;
    }
}
#endif
#if USE_AUTO_CACHING
public class UIBase : MonoAutoCaching
#else
public class UIBase : MonoBehaviour
#endif
#if USE_LOCALE
, ILocale
#endif
    {
        public eUIPosition uiPosition;
        public UIOptions uiOptions;
        public UnityAction<object[]> opened;
        public UnityAction<object[]> closed;

        protected virtual void Awake()
        {
            opened = Opened;
            closed = Closed;
#if USE_LOCALE
            foreach(var text in texts)
            {
                text.text.text = LocaleDataSO.SharedInstance.LocaleDic[text.key];
            }
        }
    
        public List<LocaleText> texts;
        public void SetLocaleTexts()
        {
            texts.Clear();
            var tmpTexts = GetComponentsInChildren<TMPro.TMP_Text>(true).ToList();
            tmpTexts.ForEach(text =>
            {
                var localeData = LocaleDataSO.SharedInstance.localeData.Find(obj => obj.Korean == text.text);
                if(localeData != null)
                {
                    texts.Add(new LocaleText(localeData.Key, text));
                }
            });
#endif
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public virtual void HideDirect() { }

        public virtual void Opened(object[] param) { }

        public virtual void Closed(object[] param) { }
    }
}