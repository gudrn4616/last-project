using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ironcow
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "BaseData", menuName = "ScriptableObjects/Base Data")]
    public class BaseDataSO
#if USE_SO_DATA
    : ScriptableObject
#endif
    {
#if USE_SO_DATA
        [SerializeField] public Sprite thumbnail;
#endif
        public string rcode;
        public string displayName;
        public string localeName { get => LocaleDataSO.GetString("name_" + rcode); }
        public string description;
        public string localeDesc { get => LocaleDataSO.GetString("desc_" + rcode); }

        public string GetLocaleDesc(params object[] param)
        {
            return LocaleDataSO.GetString("desc_" + rcode, param);
        }

        public object clone
        {
            get
            {
                var obj = MemberwiseClone();
                return obj;
            }
        }

        protected virtual void OnClone(object obj) { }
    }
}