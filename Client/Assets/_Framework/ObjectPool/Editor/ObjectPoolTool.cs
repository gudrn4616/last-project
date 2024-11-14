using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Networking;
using Ironcow;
using GUI = UnityEngine.GUI;
using NUnit.Framework;

namespace Ironcow
{
    public class ObjectPoolTool : EditorWindow
    {

        public static ObjectPoolTool instance;
#if USE_OBJECT_POOL
        [MenuItem("Ironcow/Tool/ObjectPool Tool #&o")]
#endif
        public static void Open()
        {
            var window = GetWindow<ObjectPoolTool>();
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
                var window = GetWindow<ObjectPoolTool>();
                window.minSize = new Vector2(512f, 728f);
                instance = window;
            }
        }

        private void OnDestroy()
        {
            ObjectPoolDataSO.SharedInstance.SaveSO();
        }

        int selectIndex = 0;
        public ObjectPoolData poolData;
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
                    GUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space(50);
                    if (GUILayout.Button("+", GUILayout.Width(20)))
                    {
                        ObjectPoolDataSO.SharedInstance.objectPoolDatas.Add(new ObjectPoolData());
                        ObjectPoolDataSO.SharedInstance.SaveSO();
                    }
                    EditorGUILayout.EndHorizontal();
                    int index = 0;
                    string lastID = "";
                    foreach (var data in ObjectPoolDataSO.SharedInstance.objectPoolDatas)
                    {
                        var name = data.rCode;
                        if (data != null)
                        {
                            EditorGUILayout.BeginHorizontal();
                            var style = new GUIStyle(UnityEngine.GUI.skin.button);
                            style.alignment = TextAnchor.MiddleRight;
                            var selection = GUILayout.Toggle(selectIndex == index, string.IsNullOrEmpty(name) ? " " : name, style);
                            if (selection) selectIndex = index;
                            EditorGUILayout.EndHorizontal();
                        }
                        index++;
                    }
                }
                GUILayout.EndVertical();
                EditorGUILayout.EndScrollView();
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(10);
            if (ObjectPoolDataSO.SharedInstance.objectPoolDatas != null && ObjectPoolDataSO.SharedInstance.objectPoolDatas.Count > 0)
            {
                poolData = ObjectPoolDataSO.SharedInstance.objectPoolDatas[selectIndex];
                if (poolData != null)
                {
                    GUILayout.BeginVertical();
                    {
                        var fields = poolData.GetType().GetFields().ToList();
                        fields.ForEach(obj =>
                        {
                            if (obj.FieldType == typeof(string))
                            {
                                obj.SetValue(poolData, SetText(obj.Name, (string)obj.GetValue(poolData)));
                            }
                            else if (obj.FieldType == typeof(int))
                            {
                                obj.SetValue(poolData, SetInt(obj.Name, (int)obj.GetValue(poolData)));
                            }
#if !USE_ASYNC && !USE_COROUTINE
                            else if (obj.FieldType == typeof(ObjectPoolBase))
                            {
                                SetObject(obj);
                            }
#endif
                        });
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndHorizontal();
            ObjectPoolDataSO.SharedInstance.SaveSO();
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

        public void SetObject(FieldInfo fieldInfo)
        {
            var obj = (ObjectPoolBase)fieldInfo.GetValue(poolData);
            var rcode = poolData.rCode;
            if (poolData.GetType().GetField("createItem") != null)
                rcode = (string)poolData.GetType().GetField("createItem").GetValue(poolData);
            if (poolData.GetType().GetField("target") != null)
                rcode = (string)poolData.GetType().GetField("target").GetValue(poolData);
            var path = DataToolSO.ThumbnailPath + "/" + rcode + ".png";
            if (obj == null)
            {
                obj = AssetDatabase.LoadAssetAtPath(path, fieldInfo.FieldType) as ObjectPoolBase;
            }
            GUILayout.BeginHorizontal();
            fieldInfo.SetValue(poolData, EditorGUILayout.ObjectField(fieldInfo.Name.FirstCharacterToUpper(), obj, fieldInfo.FieldType, false));
            GUILayout.EndHorizontal();
        }
        private void OnGUI()
        {
            this.windowScrollPosition = EditorGUILayout.BeginScrollView(this.windowScrollPosition, "box");
            {
                this.Draw();
            }
            EditorGUILayout.EndScrollView();
        }
    }
}