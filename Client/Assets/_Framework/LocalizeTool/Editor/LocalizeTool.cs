using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Ironcow
{
    public class LocalizeTool : EditorWindow
    {

        public static LocalizeTool instance;
#if USE_LOCALE
        [MenuItem("Ironcow/Tool/Localize Tool #&l")]
#endif
        public static void Open()
        {
            var window = GetWindow<LocalizeTool>();
            window.minSize = new Vector2(512f, 728f);
            instance = window;
        }

#if USE_LOCALE
        [MenuItem("Ironcow/Tool/Set Locale Prefabs")]
#endif
        public static void SetLocalePrefabs()
        {
            var folders = LocalePathSO.SharedInstance.prefabFolders;
            foreach (var folder in folders)
            {
                var path = AssetDatabase.GetAssetPath(folder);
                var lists = AssetDatabase.FindAssets("", new string[] { path });
                foreach (var itemGuid in lists)
                {
                    var itemPath = AssetDatabase.GUIDToAssetPath(itemGuid);
                    var item = AssetDatabase.LoadAssetAtPath<GameObject>(itemPath);
                    if (item == null) continue;
                    if (item.TryGetComponent(out ILocale locale))
                    {
                        locale.SetLocaleTexts();
                    }
                    EditorUtility.SetDirty(item);
                    PrefabUtility.SavePrefabAsset(item);
                }
            }
        }

        private Vector2 sceneViewScrollPosition = Vector2.zero;
        private Vector2 windowScrollPosition = Vector2.zero;

        private void OnFocus()
        {
            if (instance == null)
                this.OnEnable();
        }

        private void OnEnable()
        {
            if (instance == null)
            {
                var window = GetWindow<LocalizeTool>();
                window.minSize = new Vector2(512f, 728f);
                instance = window;
            }
        }

        int selectIndex = 0;
        public LocaleData localeData;
        Dictionary<string, bool> isOpened = new Dictionary<string, bool>();
        private void Draw()
        {
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            EditorGUI.indentLevel++;
            {
                EditorGUI.indentLevel++;
                this.sceneViewScrollPosition = EditorGUILayout.BeginScrollView(this.sceneViewScrollPosition, "box", GUILayout.Width(200));
                {
                    int index = 0;
                    string lastID = "";
                    foreach (var data in LocaleDataSO.SharedInstance.localeData)
                    {
                        var name = data.Key;
                        if (data != null)
                        {
                            EditorGUILayout.BeginHorizontal();
                            var style = new GUIStyle(UnityEngine.GUI.skin.button);
                            style.alignment = TextAnchor.MiddleRight;
                            var selection = GUILayout.Toggle(selectIndex == index, new GUIContent(data.Korean.Substring(0, Mathf.Min(data.Korean.Length, 7)) + "(" + name.Substring(0, Mathf.Min(name.Length, 10)) + ")"), style);
                            if (selection) selectIndex = index;
                            EditorGUILayout.EndHorizontal();
                        }
                        index++;
                    }
                }
                EditorGUILayout.EndScrollView();
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(10);
            if (LocaleDataSO.SharedInstance.localeData != null && LocaleDataSO.SharedInstance.localeData.Count > 0)
            {
                localeData = LocaleDataSO.SharedInstance.localeData[selectIndex];
                if (localeData != null)
                {
                    GUILayout.BeginVertical();
                    {
                        var fields = localeData.GetType().GetFields().ToList();
                        fields.ForEach(obj =>
                        {
                            if (obj.FieldType == typeof(string))
                            {
                                obj.SetValue(localeData, SetText(obj.Name, (string)obj.GetValue(localeData)));
                            }
                        });
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Set Prefab Localize")))
            {
                SetLocalePrefabs();
            }
            if (GUILayout.Button(new GUIContent("Download Localize Data from Google Sheet")))
            {
                ClearLog();
                Init();
                LocaleDataSO.SharedInstance.SetDirty();
#if USE_ASYNC || USE_COROUTINE
                AddressableUtils.Mapping();
#endif
            }
            GUILayout.EndHorizontal();
        }
        public void ClearLog()
        {
            var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }

        public string SetText(string label, string value)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label.FirstCharacterToUpper(), GUILayout.Width(150));
            var retStr = EditorGUILayout.TextField(value);
            GUILayout.EndHorizontal();
            return retStr;
        }

        private void OnGUI()
        {
            this.windowScrollPosition = EditorGUILayout.BeginScrollView(this.windowScrollPosition, "box");
            {
                this.Draw();
            }
            EditorGUILayout.EndScrollView();
        }

        [Serializable]
        public class GameDataSO<T> where T : LocaleData
        {
            public List<T> data = new List<T>();

            public void SetData()
            {
                data = ImportData<T>();
            }
        }

        public GameDataSO<LocaleData> localeDatas = new GameDataSO<LocaleData>();
        public List<Dictionary<string, string>> dics;

        public async void Init()
        {
            var url = $"{LocalePathSO.SharedInstance.GSheetUrl}export?format=tsv&gid={LocalePathSO.SharedInstance.localeSheetId}";
            var req = UnityWebRequest.Get(url);
            var op = req.SendWebRequest();
            await op;
            var res = req.downloadHandler.text;
            Debug.Log(res);
            dics = TsvToDic(res);
            ImportDatas();
        }

        List<Dictionary<string, string>> TsvToDic(string data)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            var rows = data.Split('\n');
            var keys = rows[0].Trim().Split('\t');
            for (int i = 1; i < rows.Length; i++)
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                var columns = rows[i].Trim().Split('\t');
                for (int j = 0; j < columns.Length; j++)
                {
                    dic.Add(keys[j], columns[j]);
                }
                list.Add(dic);
            }
            return list;
        }

        protected void ImportDatas()
        {
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                field.FieldType.GetMethod("SetData")?.Invoke(field.GetValue(this), null);
            }
            LocaleDataSO.SharedInstance.localeData = localeDatas.data;
            LocaleDataSO.SharedInstance.SetDirty();
            OnEnable();
        }

        public static List<T> ImportData<T>() where T : LocaleData
        {
            return instance.GetDatas<T>(instance.dics);
        }

        public List<T> GetDatas<T>(List<Dictionary<string, string>> datas) where T : LocaleData
        {
            List<T> list = new List<T>();
            foreach (var data in datas)
            {
                list.Add(DicToClass<T>(data));
            }
            return list;
        }

        public T DicToClass<T>(Dictionary<string, string> data) where T : LocaleData
        {
            var dt = Activator.CreateInstance<T>();
            return DicToClass<T>(dt, data);
        }

        public T DicToClass<T>(T dt, Dictionary<string, string> data) where T : LocaleData
        {
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var keys = new List<string>(data.Keys);
            foreach (var field in fields)
            {
                try
                {
                    var idx = keys.FindIndex(obj => obj == field.Name);
                    if (idx >= 0)
                    {
                        if (field.FieldType == typeof(int))
                        {
                            field.SetValue(dt, int.Parse(data[keys[idx]]));
                        }
                        else if (field.FieldType == typeof(float))
                        {
                            field.SetValue(dt, float.Parse(data[keys[idx]]));
                        }
                        else if (field.FieldType == typeof(bool))
                        {
                            field.SetValue(dt, bool.Parse(data[keys[idx]]));
                        }
                        else if (field.FieldType == typeof(double))
                        {
                            field.SetValue(dt, double.Parse(data[keys[idx]]));
                        }
                        else if (field.FieldType == typeof(string))
                        {
                            field.SetValue(dt, data[keys[idx]]);
                        }
                        else if (field.FieldType == typeof(List<int>))
                        {
                            var datas = data[keys[idx]].Split('|');
                            List<int> list = new List<int>();
                            foreach (var str in datas)
                            {
                                list.Add(int.Parse(str));
                            }
                            field.SetValue(dt, list);
                        }
                        else if (field.FieldType == typeof(int[]))
                        {
                            var datas = data[keys[idx]].Split('|');
                            List<int> list = new List<int>();
                            foreach (var str in datas)
                            {
                                list.Add(int.Parse(str));
                            }
                            field.SetValue(dt, list.ToArray());
                        }
                        else if (field.FieldType == typeof(List<string>))
                        {
                            field.SetValue(dt, data[keys[idx]].Split('|').ToList());
                        }
                        else if (field.FieldType == typeof(string[]))
                        {
                            field.SetValue(dt, data[keys[idx]].Split('|'));
                        }
                        else if (field.FieldType == typeof(Vector3))
                        {
                            field.SetValue(dt, data[keys[idx]].ToVector3());
                        }
                        else if (field.FieldType.BaseType == typeof(Enum))
                        {
                            field.SetValue(dt, Enum.Parse(field.FieldType, data[keys[idx]]));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log("Convert Failed");
                }
            }
            return (T)dt;
        }
    }
}