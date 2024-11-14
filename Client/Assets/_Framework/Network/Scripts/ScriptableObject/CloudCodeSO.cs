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
    public class CloudCodeSO : ScriptableObject
    {
#if UNITY_EDITOR
        public static string ScriptFolderFullPath { get; private set; }      // "......\이 스크립트가 위치한 폴더 경로"
        public static string ScriptFolderInProjectPath { get; private set; } // "Assets\...\이 스크립트가 위치한 폴더 경로"
        public static string AssetFolderPath { get; private set; }
        static private CloudCodeSO instance = null;
        static public CloudCodeSO SharedInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load("CloudCodeSO") as CloudCodeSO;
                    if (instance == null)
                    {
                        InitFolderPath();
                        CloudCodeSO instance = CreateInstance<CloudCodeSO>();
                        string directory = System.IO.Path.Combine(ScriptFolderInProjectPath, "Resources");
                        if (!System.IO.Directory.Exists(directory))
                            AssetDatabase.CreateFolder(ScriptFolderInProjectPath, "Resources");

                        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{ScriptFolderInProjectPath}/Resources/CloudCodeSO.asset");
                        AssetDatabase.CreateAsset(instance, assetPath);
                    }
                }
                return instance;
            }
        }

#if UNITY_EDITOR
        [MenuItem("Ironcow/Data/Cloud Code Settings")]
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
        [Header("Java Script Templete Path")]
        public Object jsTempletePath;
        public static string JSTempletePath { get => AssetDatabase.GetAssetPath(SharedInstance.jsTempletePath); }
        public static string JSTempleteFullPath { get => Application.dataPath.Replace("Assets", "") + AssetDatabase.GetAssetPath(SharedInstance.jsTempletePath); }

        [Header("Java Scripts Path")]
        public Object jsPath;
        public static string JSPath { get => AssetDatabase.GetAssetPath(SharedInstance.jsPath); }
        public static string JSFullPath { get => Application.dataPath.Replace("Assets", "") + AssetDatabase.GetAssetPath(SharedInstance.jsPath); }

        [Header("C# Network Code Path")]
        public Object networkPath;
        public static string NetworkPath { get => AssetDatabase.GetAssetPath(SharedInstance.networkPath); }

#endif
    }
}