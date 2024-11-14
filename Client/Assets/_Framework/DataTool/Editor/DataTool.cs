using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEditorInternal;
using System.Collections;
using UnityEngine.Networking;
using Unity.VisualScripting;

namespace Ironcow
{
    public class DataTool : EditorWindow
    {
        public static DataTool instance;
#if USE_SO_DATA
        [MenuItem("Ironcow/Tool/Data Tool #&x")]
#endif
        public static void Open()
        {
            var window = GetWindow<DataTool>();
            window.minSize = new Vector2(512f, 728f);
            instance = window;
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
                var window = GetWindow<DataTool>();
                window.minSize = new Vector2(512f, 728f);
                instance = window;
            }

            datas = Directory.GetFiles(DataToolSO.DataScriptableObjectFullPath).ToList();
            datas.RemoveAll(obj => obj.Contains(".meta"));
            datas.RemoveAll(obj => obj.Contains(".json"));

            isOpened.Clear();
            foreach (var sheet in sheets)
            {
                isOpened.Add(sheet.key, false);
            }
        }

        public void InitThumbnail()
        {
            for (int i = 0; i < datas.Count; i++)
            {
#if USE_SO_DATA
                var asset = AssetDatabase.LoadAssetAtPath<BaseDataSO>(datas[i].Replace(Application.dataPath, "Assets"));
                if (asset == null) continue;
                var field = asset.GetType().GetField("thumbnail");
                if (field != null)
                {
                    var rcode = asset.rcode;
                    var path = DataToolSO.ThumbnailPath + "/" + rcode + ".png";
                    var texture = (Sprite)field.GetValue(asset);
                    if (asset.GetType().GetField("createItem") != null)
                        rcode = (string)asset.GetType().GetField("createItem").GetValue(asset);
                    if (asset.GetType().GetField("target") != null)
                        rcode = (string)asset.GetType().GetField("target").GetValue(asset);
                    if (texture == null)
                    {
                        field.SetValue(asset, AssetDatabase.LoadAssetAtPath(path, field.FieldType) as Sprite);
                        asset.SetDirty();
                    }
                }
#endif
            }
            AssetDatabase.SaveAssets();
        }

        List<string> datas = new List<string>();
        string selectRCode;
        public BaseDataSO currentAsset;
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

                    foreach (var sheet in sheets)
                    {
                        var key = sheet.key;
                        bool opened = isOpened[key];
                        var style = new GUIStyle(UnityEngine.GUI.skin.button);
                        style.alignment = TextAnchor.MiddleLeft;
                        opened = GUILayout.Toggle(opened, new GUIContent((opened ? "▼" : "▶") + sheet.className), style, GUILayout.Width(180));
                        isOpened[key] = opened;
                        lastID = key;

                        if (isOpened[key])
                        {
                            var list = datas.FindAll(obj => obj.Contains(key));
                            foreach (var data in list)
                            {
                                var name = data.Split('\\').Last().Split('.')[0];
                                var path = DataToolSO.DataScriptableObjectPath + "/" + name + ".asset";
                                //var dt = AssetDatabase.LoadAssetAtPath<BaseData>(path);
                                //if (dt != null)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.Space(5);
                                    var toggleStyle = new GUIStyle(UnityEngine.GUI.skin.button);
                                    toggleStyle.alignment = TextAnchor.MiddleRight;
                                    var selection = GUILayout.Toggle(selectRCode == name, new GUIContent(
#if USE_LOCALE
                                        $"{LocaleDataSO.GetString("name_" + name)}({name})"
#else
                                        $"{name}"
#endif
                                        ), toggleStyle, GUILayout.Width(170));
                                    if (selection) selectRCode = name;
                                    EditorGUILayout.Space(10);
                                    EditorGUILayout.EndHorizontal();
                                }
                                index++;
                            }
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(10);
            if (datas != null && datas.Count > 0)
            {
#if USE_SO_DATA
            currentAsset = AssetDatabase.LoadAssetAtPath<BaseDataSO>(DataToolSO.DataScriptableObjectPath + "/" + selectRCode + ".asset");
            if (currentAsset != null)
            {
                GUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField(LocaleDataSO.GetString("name_" + currentAsset.rcode));
                    var fields = currentAsset.GetType().GetFields().ToList();
                    var baseFields = typeof(BaseDataSO).GetFields().ToList();
                    fields.InsertRange(0, fields.GetRange(fields.Count - baseFields.Count, baseFields.Count));
                    fields.RemoveRange(fields.Count - baseFields.Count, baseFields.Count);
                    fields.ForEach(fieldInfo =>
                    {
                        if (fieldInfo.FieldType == typeof(string))
                        {
                            fieldInfo.SetValue(currentAsset, SetText(fieldInfo.Name, (string)fieldInfo.GetValue(currentAsset)));
                        }
                        else if (fieldInfo.FieldType == typeof(int))
                        {
                            fieldInfo.SetValue(currentAsset, SetInt(fieldInfo.Name, (int)fieldInfo.GetValue(currentAsset)));
                        }
                        else if (fieldInfo.FieldType == typeof(float))
                        {
                            fieldInfo.SetValue(currentAsset, SetFloat(fieldInfo.Name, (float)fieldInfo.GetValue(currentAsset)));
                        }
                        else if (fieldInfo.FieldType == typeof(bool))
                        {
                            fieldInfo.SetValue(currentAsset, SetBool(fieldInfo.Name, (bool)fieldInfo.GetValue(currentAsset)));
                        }
                        else if (fieldInfo.FieldType == typeof(Vector3))
                        {
                            fieldInfo.SetValue(currentAsset, SetVector3(fieldInfo.Name, (Vector3)fieldInfo.GetValue(currentAsset)));
                        }
                        else if (fieldInfo.FieldType.BaseType == typeof(Enum))
                        {
                            SetEnum(fieldInfo);
                            //obj.SetValue(currentAsset, );
                        }
                        else if (fieldInfo.FieldType == typeof(Texture))
                        {
                            SetTexture(fieldInfo);
                        }
                        else if (fieldInfo.FieldType == typeof(Sprite))
                        {
                            SetSprite(fieldInfo);
                        }
                        else
                        {
                            SetList(fieldInfo.Name, (IList)fieldInfo.GetValue(currentAsset), fieldInfo.FieldType);
                        }
                    });
                }
                GUILayout.EndVertical();
            }
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

        public int SetInt(string label, int value)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label.FirstCharacterToUpper(), GUILayout.Width(150));
            var retStr = EditorGUILayout.IntField(value);
            GUILayout.EndHorizontal();
            return retStr;
        }

        public float SetFloat(string label, float value)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label.FirstCharacterToUpper(), GUILayout.Width(150));
            var retStr = EditorGUILayout.FloatField(value);
            GUILayout.EndHorizontal();
            return retStr;
        }

        public bool SetBool(string label, bool value)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label.FirstCharacterToUpper(), GUILayout.Width(150));
            var retStr = EditorGUILayout.Toggle(value);
            GUILayout.EndHorizontal();
            return retStr;
        }

        public Vector3 SetVector3(string label, Vector3 value)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label.FirstCharacterToUpper(), GUILayout.Width(150));
            var retStr = EditorGUILayout.Vector3Field("", value);
            GUILayout.EndHorizontal();
            return retStr;
        }

        public void SetList(string label, IList value, Type type)
        {
            ReorderableList list = new ReorderableList(value, type, true, true, true, true);
            list.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, label.FirstCharacterToUpper());
            };
            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                rect.height -= 4;
                rect.y += 2;
                if (value[index].GetType() == typeof(string))
                    value[index] = EditorGUI.TextField(rect, (string)value[index]);
                if (value[index].GetType() == typeof(int))
                    value[index] = EditorGUI.IntField(rect, (int)value[index]);
                if (value[index].GetType() == typeof(float))
                    value[index] = EditorGUI.FloatField(rect, (float)value[index]);
            };
            list.DoLayoutList();
        }

        public void SetEnum(FieldInfo fieldInfo)
        {
            GUILayout.BeginHorizontal();
            var names = Enum.GetValues(fieldInfo.FieldType);
            GenericMenu menu = new GenericMenu();
            int idx = 0;
            string selectedEnumDropdown = fieldInfo.GetValue(currentAsset).ToString();
            foreach (var name in names)
            {
                var isEquip = name.ToString() == selectedEnumDropdown;
                menu.AddItem(new GUIContent(name.ToString()), isEquip, (nm) =>
                {
                    fieldInfo.SetValue(currentAsset, nm);
                }, name);
                idx++;
            }
            GUIContent content = new GUIContent(selectedEnumDropdown);
            EditorGUILayout.LabelField(fieldInfo.Name.FirstCharacterToUpper(), GUILayout.Width(150));
            if (EditorGUILayout.DropdownButton(content, FocusType.Keyboard))
            {
                menu.ShowAsContext();
            }
            GUILayout.EndHorizontal();
        }

        public void SetTexture(FieldInfo fieldInfo)
        {
            var texture = (Texture)fieldInfo.GetValue(currentAsset);
            var rcode = currentAsset.rcode;
            if (currentAsset.GetType().GetField("createItem") != null)
                rcode = (string)currentAsset.GetType().GetField("createItem").GetValue(currentAsset);
            if (currentAsset.GetType().GetField("target") != null)
                rcode = (string)currentAsset.GetType().GetField("target").GetValue(currentAsset);
            var path = DataToolSO.ThumbnailPath + "/" + rcode + ".png";
            if (texture == null)
            {
                texture = AssetDatabase.LoadAssetAtPath(path, fieldInfo.FieldType) as Texture;
            }
            GUILayout.BeginHorizontal();
            fieldInfo.SetValue(currentAsset, EditorGUILayout.ObjectField(fieldInfo.Name.FirstCharacterToUpper(), texture, fieldInfo.FieldType, false));
            GUILayout.EndHorizontal();
        }

        public void SetSprite(FieldInfo fieldInfo)
        {
            var sprite = (Sprite)fieldInfo.GetValue(currentAsset);
            var rcode = currentAsset.rcode;
            if (currentAsset.GetType().GetField("createItem") != null)
                rcode = (string)currentAsset.GetType().GetField("createItem").GetValue(currentAsset);
            if (currentAsset.GetType().GetField("target") != null)
                rcode = (string)currentAsset.GetType().GetField("target").GetValue(currentAsset);
            var path = DataToolSO.ThumbnailPath + "/" + rcode + ".png";
            if (sprite == null)
            {
                sprite = AssetDatabase.LoadAssetAtPath(path, fieldInfo.FieldType) as Sprite;
            }
            GUILayout.BeginHorizontal();
            fieldInfo.SetValue(currentAsset, EditorGUILayout.ObjectField(fieldInfo.Name.FirstCharacterToUpper(), sprite, fieldInfo.FieldType, false));
            GUILayout.EndHorizontal();
        }

        private void OnGUI()
        {
            this.windowScrollPosition = EditorGUILayout.BeginScrollView(this.windowScrollPosition, "box");
            {
                this.Draw();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Init Thumbnail")))
            {
                InitThumbnail();
                AssetDatabase.Refresh();
            }
            if (GUILayout.Button(new GUIContent("Download Google Sheet")))
            {
                ClearLog();
                Init();
            }
            GUILayout.EndHorizontal();
        }

        public async void Init()
        {
            foreach (var sheet in sheets)
            {
                var url = $"{DataToolSO.SharedInstance.GSheetUrl}export?format=tsv&gid={sheet.sheetId}";
                var req = UnityWebRequest.Get(url);
                var op = req.SendWebRequest();
                Debug.Log($"{sheet.className} 데이터 로딩중");
                await op;
                var res = req.downloadHandler.text;
                Debug.Log(res);
                sheet.datas = TsvToDic(res);
            }
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
            foreach (var sheet in sheets)
            {
                ImportData(sheet);
            }
            OnEnable();
        }

        public static void ImportData(SheetInfoSO sheet)
        {
            if (sheet.isUpdate)
            {
                Assembly assembly = typeof(BaseDataSO).Assembly;
                var type = assembly.GetType(sheet.className);
                instance.GetDatas(type, sheet.datas);
            }
        }

        public void GetDatas(Type type, List<Dictionary<string, string>> datas)
        {
#if USE_SO_DATA
            foreach (var data in datas)
            {
                /*
                if (type == typeof(SkillLevelUpData))
                {
                    var path = DataToolSO.DataScriptableObjectPath + "/" + data["targetRcode"].Replace("SKI", "SLV") + ".asset";
                    var dt = (ScriptableObject)AssetDatabase.LoadAssetAtPath(path, type);
                    if (dt == null)
                    {
                        dt = DicToClass(path, type, data);
                    }
                    else
                    {

                        dt = DicToClassSkillLevelUpData(dt, data);
                    }

                    EditorUtility.SetDirty(dt);
                    AssetDatabase.SaveAssets();
                }
                else*/
                {
                    var path = DataToolSO.DataScriptableObjectPath + "/" + data["rcode"] + ".asset";
                    var dt = (ScriptableObject)AssetDatabase.LoadAssetAtPath(path, type);
                    if (dt == null)
                    {
                        dt = DicToClass(type, data);
                    }
                    else
                    {

                        dt = DicToClass(type, dt, data);
                    }

                    EditorUtility.SetDirty(dt);
                    AssetDatabase.SaveAssets();
                }
            }
#endif
        }

        private List<SheetInfoSO> sheets { get => DataToolSO.SharedInstance.sheets; }

#if USE_SO_DATA
        public ScriptableObject DicToClass(Type type, Dictionary<string, string> data)
        {
            var dt = CreateInstance(type);
            AssetDatabase.CreateAsset(dt, DataToolSO.DataScriptableObjectPath + "/" + data["rcode"] + ".asset");
            return DicToClass(type, dt, data);
        }
        public ScriptableObject DicToClass(string path, Type type, Dictionary<string, string> data)
        {
            var dt = CreateInstance(type);
            AssetDatabase.CreateAsset(dt, path);
            return DicToClassSkillLevelUpData(dt, data);
        }
#endif

        public ScriptableObject DicToClass(Type type, ScriptableObject dt, Dictionary<string, string> data)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
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
            return dt;
        }

        public ScriptableObject DicToClassSkillLevelUpData(ScriptableObject dt, Dictionary<string, string> data)
        {
            /*var keys = new List<string>(data.Keys);
            var skillLevelUpData = (SkillLevelUpData)dt;
            skillLevelUpData.rcode = dt.name;
            skillLevelUpData.targetRcode = data["targetRcode"];
            if (skillLevelUpData.skillLevels == null) skillLevelUpData.skillLevels = new List<SkillLevelUp>();
            skillLevelUpData.skillLevels.Add(new SkillLevelUp() { rcode = data["rcode"], lv = int.Parse(data["lv"]), unlock = int.Parse(data["unlock"]), value = int.Parse(data["value"]), price = data["price"] });*/
            return dt;
        }
    }
}