using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using System.Net.Sockets;

namespace Ironcow
{
    public class CloudCodeEditor : EditorWindow
    {
        public static CloudCodeEditor instance;
#if USE_CLOUD_CODE
        [MenuItem("Ironcow/Tool/CloudCode Tool #&m")]
#endif
        public static void Open()
        {
            var window = GetWindow<CloudCodeEditor>();
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
                var window = GetWindow<CloudCodeEditor>();
                window.minSize = new Vector2(512f, 728f);
                instance = window;
            }
        }

        private void Draw()
        {
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            EditorGUI.indentLevel++;
            {
                EditorGUI.indentLevel++;
                this.sceneViewScrollPosition = EditorGUILayout.BeginScrollView(this.sceneViewScrollPosition, "box", GUILayout.Width(200));
                {

                }
                EditorGUILayout.EndScrollView();
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(10);
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
                AssetDatabase.Refresh();
            }
            if (GUILayout.Button(new GUIContent("Download Google Sheet")))
            {

            }
            GUILayout.EndHorizontal();
        }

    }
}