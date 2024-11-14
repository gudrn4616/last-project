using UnityEngine;
using UnityEditor;
using System;
using System.IO;
//using SFB;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ironcow;
using GUI = UnityEngine.GUI;

using UnityEditor.SearchService;
using UnityEditor.SceneManagement;

namespace Ironcow
{
    public class SceneSelect : EditorWindow
    {
        public static SceneSelect instance;
        [MenuItem("Ironcow/Tool/Scene Select &1")]
        public static void Open()
        {
            selectScene = ScenePath;
            var window = GetWindow<SceneSelect>();
            window.minSize = new Vector2(500f, 200f);
            window.maxSize = new Vector2(500f, 200f);
            instance = window;
        }

        [MenuItem("Ironcow/Tool/Editor Scene Select &2")]
        public static void OpenEditor()
        {
            selectScene = EditorScenePath;
            var window = GetWindow<SceneSelect>();
            window.minSize = new Vector2(500f, 200f);
            window.maxSize = new Vector2(500f, 200f);
            instance = window;
        }

        [MenuItem("Ironcow/Scenes/Setting/DontDestroy Scene &3")]
        static void OpenDontDestroyScene()
        {
            EditorSceneManager.OpenScene(DontDestroyScenePath);
        }


        [MenuItem("Ironcow/Scenes/Setting/Intro Scene &4")]
        static void OpenIntroScene()
        {
            EditorSceneManager.OpenScene(IntroScenePath);
        }

        private Vector2 sceneViewScrollPosition = Vector2.zero;
        private Vector2 windowScrollPosition = Vector2.zero;

        static string selectScene;
        static string ScenePath { get => Application.dataPath + EditorDataSO.ScenePath.Replace("Assets", "") + "/"; }
        static string EditorScenePath { get => Application.dataPath + EditorDataSO.EditorScenePath.Replace("Assets", "") + "/"; }
        static string DontDestroyScenePath { get => Application.dataPath + EditorDataSO.DontDestroyScenePath.Replace("Assets", ""); }
        static string IntroScenePath { get => Application.dataPath + EditorDataSO.IntroScenePath.Replace("Assets", ""); }
        List<SceneAsset> scenes = new List<SceneAsset>();

        private void OnFocus()
        {
            if (scenes.Count == 0)
            {
                OnEnable();
            }
        }

        private void OnEnable()
        {
            scenes.Clear();
            if (instance == null) instance = GetWindow<SceneSelect>();
            if (selectScene == null) selectScene = ScenePath;
            if (scenes.Count == 0)
            {
                var paths = Directory.GetFiles(selectScene);
                foreach (var path in paths)
                {
                    if (path.Contains("meta")) continue;
                    scenes.Add(AssetDatabase.LoadAssetAtPath<SceneAsset>(path.Replace(Application.dataPath, "Assets")));
                }
                scenes.RemoveAll(obj => obj == null);
            }
        }

        private async void Draw()
        {
            if (EditorBuildSettings.scenes.Length > 0)
            {
                EditorGUI.indentLevel++;
                {
                    EditorGUI.indentLevel++;
                    this.sceneViewScrollPosition = EditorGUILayout.BeginScrollView(this.sceneViewScrollPosition, "box");
                    {
                        foreach (var scene in scenes)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(scene.name, GUILayout.Width(370));
                            if (GUILayout.Button("Move", GUILayout.Width(80f)))
                            {
                                EditorSceneManager.OpenScene(selectScene + scene.name + ".unity");
                                instance.Close();
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    EditorGUILayout.EndScrollView();
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndDisabledGroup();
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