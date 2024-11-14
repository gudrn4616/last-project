using Ironcow;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Ironcow
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class EditorDataSO : ScriptableObject
    {
#if UNITY_EDITOR
        public static string ScriptFolderFullPath { get; private set; }      // "......\이 스크립트가 위치한 폴더 경로"
        public static string ScriptFolderInProjectPath { get; private set; } // "Assets\...\이 스크립트가 위치한 폴더 경로"
        public static string AssetFolderPath { get; private set; }
        static private EditorDataSO instance = null;
        static public EditorDataSO SharedInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load("EditorData") as EditorDataSO;
                    if (instance == null)
                    {
                        InitFolderPath();
                        EditorDataSO instance = CreateInstance<EditorDataSO>();
                        string directory = System.IO.Path.Combine(ScriptFolderInProjectPath, "Resources");
                        if (!System.IO.Directory.Exists(directory))
                            AssetDatabase.CreateFolder(ScriptFolderInProjectPath, "Resources");

                        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{ScriptFolderInProjectPath}/Resources/EditorData.asset");
                        AssetDatabase.CreateAsset(instance, assetPath);
                    }
                }
                return instance;
            }
        }

#if UNITY_EDITOR
        [MenuItem("Ironcow/Data/Editor Data Settings")]
        private static void Edit()
        {
            Selection.activeObject = SharedInstance;
        }
#endif

        private static void InitFolderPath([System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            ScriptFolderFullPath = System.IO.Path.GetDirectoryName(sourceFilePath);
            int rootIndex = ScriptFolderFullPath.IndexOf(@"Assets\");
            if (rootIndex > -1)
            {
                ScriptFolderInProjectPath = ScriptFolderFullPath.Substring(rootIndex, ScriptFolderFullPath.Length - rootIndex);
            }
        }
        [Header("Prefab Path")]
        public List<Object> prefabFolders;

        [Header("Scene Path")]
        public Object scenePath;
        public static string ScenePath { get => AssetDatabase.GetAssetPath(SharedInstance.scenePath); }

        public Object editorScenePath;
        public static string EditorScenePath { get => AssetDatabase.GetAssetPath(SharedInstance.editorScenePath); }

        public Object introScene;
        public static string IntroScenePath { get => AssetDatabase.GetAssetPath(SharedInstance.introScene); }

        public Object dontDestroyScene;
        public static string DontDestroyScenePath { get => AssetDatabase.GetAssetPath(SharedInstance.dontDestroyScene); }

        [Header("Script Templete Path")]
        public Object templetePath;
        public static string TempletePath { get => AssetDatabase.GetAssetPath(SharedInstance.templetePath); }


#endif
    }
}