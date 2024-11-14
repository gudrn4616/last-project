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
    public class ObjectPoolPathSO : ScriptableObject
    {
#if UNITY_EDITOR
        public static string ScriptFolderFullPath { get; private set; }      // "......\이 스크립트가 위치한 폴더 경로"
        public static string ScriptFolderInProjectPath { get; private set; } // "Assets\...\이 스크립트가 위치한 폴더 경로"
        public static string AssetFolderPath { get; private set; }
        static private ObjectPoolPathSO instance = null;
        static public ObjectPoolPathSO SharedInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load("ObjectPoolPath") as ObjectPoolPathSO;
                    if (instance == null)
                    {
                        InitFolderPath();
                        ObjectPoolPathSO instance = CreateInstance<ObjectPoolPathSO>();
                        string directory = System.IO.Path.Combine(ScriptFolderInProjectPath, "Resources");
                        if (!System.IO.Directory.Exists(directory))
                            AssetDatabase.CreateFolder(ScriptFolderInProjectPath, "Resources");

                        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{ScriptFolderInProjectPath}/Resources/ObjectPoolPath.asset");
                        AssetDatabase.CreateAsset(instance, assetPath);
                    }
                }
                return instance;
            }
        }

#if UNITY_EDITOR
        [MenuItem("Ironcow/Data/ObjectPool Path Settings")]
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

        public Object objectPoolPath;
        public static string ObjectPoolPath { get => AssetDatabase.GetAssetPath(SharedInstance.objectPoolPath); }
#endif
    }
}