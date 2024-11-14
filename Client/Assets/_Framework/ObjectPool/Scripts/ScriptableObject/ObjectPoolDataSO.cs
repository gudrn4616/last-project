using Ironcow;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace Ironcow
{
    [System.Serializable]
    public class ObjectPoolData
    {
        public string rCode;
        public int count = 50;
        public int velocity = 25;
        [HideInInspector] public ObjectPoolBase prefab;
        [HideInInspector] public Transform parent;
    }

    public class ObjectPoolDataSO : ScriptableObject
    {
        static private ObjectPoolDataSO instance = null;
        static public ObjectPoolDataSO SharedInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load("ObjectPool Datas") as ObjectPoolDataSO;
                    if (instance == null)
                    {
                        instance = CreateInstance<ObjectPoolDataSO>();

#if UNITY_EDITOR
                        string directory = ObjectPoolPathSO.ObjectPoolPath;
                        if (!System.IO.Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(ObjectPoolPathSO.ObjectPoolPath);
                            AssetDatabase.Refresh();
                        }

                        string fullPath = System.IO.Path.Combine(ObjectPoolPathSO.ObjectPoolPath, "ObjectPool Datas.asset");
                        AssetDatabase.CreateAsset(instance, fullPath);
#endif
                    }
                }

                return instance;
            }
        }

#if UNITY_EDITOR
        [MenuItem("Ironcow/Data/ObjectPool Datas")]
        private static void Edit()
        {
            Selection.activeObject = SharedInstance;
        }
#endif

        public void SaveSO()
        {
            SetDirty();
        }

        [SerializeField] public List<ObjectPoolData> objectPoolDatas = new List<ObjectPoolData>();
    }
}