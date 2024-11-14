using Ironcow;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace Ironcow
{
    [System.Serializable]
    public class LocaleData
    {
        public string Key;
        public string Korean;
        public string English;
    }

    public class LocaleDataSO : ScriptableObject
    {
        static private LocaleDataSO instance = null;
        static public LocaleDataSO SharedInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load("Localize Datas") as LocaleDataSO;
                    if (instance == null)
                    {
                        instance = CreateInstance<LocaleDataSO>();

#if UNITY_EDITOR
                        string directory = LocalePathSO.LocaleDataPath;
                        if (!System.IO.Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(LocalePathSO.LocaleDataPath);
                            AssetDatabase.Refresh();
                        }

                        string fullPath = System.IO.Path.Combine(LocalePathSO.LocaleDataPath, "Localize Datas.asset");
                        AssetDatabase.CreateAsset(instance, fullPath);
#endif
                    }
                }

                return instance;
            }
        }

#if UNITY_EDITOR
        [MenuItem("Ironcow/Data/Localize Datas")]
        private static void Edit()
        {
            Selection.activeObject = SharedInstance;
        }
#endif

        public static string languageValue { get => Application.systemLanguage.ToString(); }

        [SerializeField] public List<LocaleData> localeData = new List<LocaleData>();

        private Dictionary<string, string> localeDic = new Dictionary<string, string>();
        public Dictionary<string, string> LocaleDic
        {
            get
            {
                if (localeDic == null) localeDic = new Dictionary<string, string>();
                if (localeDic.Count != localeData.Count)
                {
                    InitLocaleDic();
                }

                return localeDic;
            }
        }

        public void InitLocaleDic()
        {
            localeData.ForEach(obj =>
            {
                var fields = obj.GetType().GetFields().ToList();
                if (!localeDic.ContainsKey(obj.Key))
                    localeDic.Add(obj.Key, (string)fields.Find(obj => obj.Name == Application.systemLanguage.ToString()).GetValue(obj));
                else
                    localeDic[obj.Key] = (string)fields.Find(obj => obj.Name == Application.systemLanguage.ToString()).GetValue(obj);
                //localeDic.Add(obj.Key, (string)fields.Find(obj => obj.Name == "English").GetValue(obj));
            });
        }

        public static string GetString(string key, params object[] param)
        {
            if (SharedInstance.localeDic.Count == 0) SharedInstance.InitLocaleDic();
            if (SharedInstance.localeDic.ContainsKey(key))
            {
                if (param.Length > 0)
                    return string.Format(SharedInstance.localeDic[key], param);
                else
                    return SharedInstance.localeDic[key];
            }
            else
                return key;
        }
    }

    public class Locale
    {
        public static string GetString(string key, params object[] param)
        {
            return LocaleDataSO.GetString(key.Replace("/n", "\n"), param);
        }
    }
}