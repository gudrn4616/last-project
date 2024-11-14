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
    public class FrameworkController : ScriptableObject
    {
#if UNITY_EDITOR
        public static string ScriptFolderFullPath { get; private set; }      // "......\이 스크립트가 위치한 폴더 경로"
        public static string ScriptFolderInProjectPath { get; private set; } // "Assets\...\이 스크립트가 위치한 폴더 경로"
        public static string AssetFolderPath { get; private set; }
        static private FrameworkController instance = null;
        static public FrameworkController SharedInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load("FrameworkController") as FrameworkController;
                    if (instance == null)
                    {
                        InitFolderPath();
                        FrameworkController instance = CreateInstance<FrameworkController>();
                        string directory = System.IO.Path.Combine(ScriptFolderInProjectPath, "Resources");
                        if (!System.IO.Directory.Exists(directory))
                            AssetDatabase.CreateFolder(ScriptFolderInProjectPath, "Resources");

                        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{ScriptFolderInProjectPath}/Resources/FrameworkController.asset");
                        AssetDatabase.CreateAsset(instance, assetPath);
                    }
                }
                return instance;
            }
        }

#if UNITY_EDITOR
        [MenuItem("Ironcow/Tool/FrameworkController")]
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
#endif

        public bool isLocale;
        public bool isAutoCaching;
        public bool isScriptableObjectData;
        public bool isAddressableCoroutine;
        public bool isAddressableAsync;
        public bool isObjectPool;
        public bool isCloudCode;
    }
}