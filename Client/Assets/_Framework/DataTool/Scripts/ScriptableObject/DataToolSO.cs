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
    public class DataToolSO : ScriptableObject
    {
#if UNITY_EDITOR
        public static string ScriptFolderFullPath { get; private set; }      // "......\이 스크립트가 위치한 폴더 경로"
        public static string ScriptFolderInProjectPath { get; private set; } // "Assets\...\이 스크립트가 위치한 폴더 경로"
        public static string AssetFolderPath { get; private set; }
        static private DataToolSO instance = null;
        static public DataToolSO SharedInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load("DataToolSO") as DataToolSO;
                    if (instance == null)
                    {
                        InitFolderPath();
                        DataToolSO instance = CreateInstance<DataToolSO>();
                        string directory = System.IO.Path.Combine(ScriptFolderInProjectPath, "Resources");
                        if (!System.IO.Directory.Exists(directory))
                            AssetDatabase.CreateFolder(ScriptFolderInProjectPath, "Resources");

                        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{ScriptFolderInProjectPath}/Resources/DataToolSO.asset");
                        AssetDatabase.CreateAsset(instance, assetPath);
                    }
                }
                return instance;
            }
        }

#if UNITY_EDITOR
        [MenuItem("Ironcow/Data/Data Tool Settings")]
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
        [Header("Scriptable Object Data Path")]
        public Object dataScriptableObjectPath;
        public static string DataScriptableObjectPath { get => AssetDatabase.GetAssetPath(SharedInstance.dataScriptableObjectPath); }
        public static string DataScriptableObjectFullPath { get => Application.dataPath.Replace("Assets", "") + AssetDatabase.GetAssetPath(SharedInstance.dataScriptableObjectPath); }

        [Header("Thumbnail Path")]
        public Object thumbnailPath;
        public static string ThumbnailPath { get => AssetDatabase.GetAssetPath(SharedInstance.thumbnailPath); }

        [Header("Google Sheet Data")]
        public string GSheetUrl;
        public List<SheetInfoSO> sheets;

#endif
    }

    [System.Serializable]
    public class SheetInfoSO
    {
        public string className;
        public string sheetId;
        public string key;
        public List<Dictionary<string, string>> datas;
        public bool isUpdate;
    }

}