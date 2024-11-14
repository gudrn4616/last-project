using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
#if USE_ASYNC || USE_COROUTINE
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
#endif

namespace Ironcow
{
    public class ResourceManagerBase<T> : MonoSingleton<T> where T : ResourceManagerBase<T>
    {
        public bool isAutoLoading = false;
        [NonSerialized] public bool isInit = false;

        public async void Init()
        {
#if USE_ASYNC
            await LoadAddressable();
#else
            PreloadUIs();
#endif
            isInit = true;
        }

        #region Use Resources
#if !USE_COROUTINE && !USE_ASYNC
        public Dictionary<string, object> assetPools = new Dictionary<string, object>();

        public async void PreloadUIs()
        {
            await LoadAssets<UIBase>("", eAddressableType.UI);
        }

        public async Task<T> LoadAsset<T>(string key, eAddressableType type) where T : UnityEngine.Object
        {
            var poolKey = type.ToString() + "/" + key;
            if (assetPools.ContainsKey(poolKey)) return (T)assetPools[poolKey];
            var asset = await Resources.LoadAsync<T>(poolKey);
            if (asset != null && !assetPools.ContainsKey(poolKey)) assetPools.Add(poolKey, asset);
            return (T)asset;
        }

        public async Task<List<T>> LoadAssets<T>(string key, eAddressableType type) where T : UnityEngine.Object
        {
            var path = type.ToString() + "/" + key;
            List<T> retList = new List<T>();
            var keys = new List<string>(assetPools.Keys);
            foreach (var val in keys)
            {
                if (val.Contains(path))
                {
                    retList.Add((T)assetPools[val]);
                }
            }
            if (retList.Count == 0)
            {
                retList = Resources.LoadAll<T>(path).ToList();
                var filename = key.FileName().ToUpper();
                foreach (var item in retList)
                {
                    if (!assetPools.ContainsKey(item.name))
                        assetPools.Add(path + "/" + item.name, item);
                }
            }
            return retList;
        }

#if USE_SO_DATA
        public async Task<List<T>> LoadDataAssets<T>() where T : UnityEngine.Object
        {
            return await LoadAssets<T>("", eAddressableType.Datas);
        }
#else
        public async Task<List<T>> LoadDataAssets<T>() where T : BaseDataSO
        {
            var datas = LoadAssets<TextAsset>("Datas");
            for(int i = 0; i < datas.Count; i++)
            {

            }
            return null;
        }
#endif

        public async Task<T> LoadThumbnail<T>(string key) where T : UnityEngine.Object
        {
            return await LoadAsset<T>(key, eAddressableType.Thumbnail);
        }

        public async Task<T> LoadUI<T>(string key) where T : UnityEngine.Object
        {
            return await LoadAsset<T>(key, eAddressableType.UI);
        }
#endif
        #endregion

        #region Addressable Callback
#if USE_COROUTINE
    private Dictionary<eAddressableType, Dictionary<string, AddressableMap>> addressableMap = new Dictionary<eAddressableType, Dictionary<string, AddressableMap>>();

    void Start()
    {
        LoadAddressable();
    }

    private void InitAddressableMap()
    {
        Addressables.LoadAssetsAsync<TextAsset>("AddressableMap", (text) =>
        {
            var map = JsonUtility.FromJson<AddressableMapData>(text.text);
            var key = eAddressableType.prefab;
            Dictionary<string, AddressableMap> mapDic = new Dictionary<string, AddressableMap>();
            foreach (var data in map.list)
            {
                key = data.addressableType;
                if(!mapDic.ContainsKey(data.key))
                    mapDic.Add(data.key, data);
            }
            if (!addressableMap.ContainsKey(key)) addressableMap.Add(key, mapDic);

        });
    }

    public void LoadAddressable()
    {
        StartCoroutine(CLoadAddressable());
    }

    public IEnumerator CLoadAddressable()
    {
        yield return Addressables.InitializeAsync();
        var handle = Addressables.DownloadDependenciesAsync("InitDownload");
        yield return SetProgress(handle);
        switch (handle.Status)
        {
            case AsyncOperationStatus.None:
                break;
            case AsyncOperationStatus.Succeeded:
                ICLogger.Log("다운로드 성공!");
                break;
            case AsyncOperationStatus.Failed:
                ICLogger.Log("다운로드 실패 : " + handle.OperationException.Message);
                ICLogger.LogError(handle.OperationException.ToString());
                break;
            default:
                break;
        }
        Addressables.Release(handle);
        InitAddressableMap();
    }

    public IEnumerator SetProgress(AsyncOperationHandle handle)
    {
        while (!handle.IsDone)
        {
            //UILoading.instance.SetProgress(handle.GetDownloadStatus().Percent, "Resource Download...");
            yield return new WaitForEndOfFrame();
        }
        //UILoading.instance.SetProgress(1);

    }

    public List<string> GetPaths(string key, eAddressableType addressableType, eAssetType assetType)
    {
        var keys = new List<string>(addressableMap[addressableType].Keys);
        keys.RemoveAll(obj => !obj.Contains(key));
        List<string> retList = new List<string>();
        keys.ForEach(obj =>
        {
            if (addressableMap[addressableType][obj].assetType == assetType)
                retList.Add(addressableMap[addressableType][obj].path);
        });
        return retList;
    }

    public string GetPath(string key, eAddressableType addressableType)
    {
        var map = addressableMap[addressableType][key.ToLower()];
        return map.path;
    }

    public void LoadAssets<T>(string key, eAddressableType addressableType, eAssetType assetType, Action<List<T>> callback)
    {
        StartCoroutine(CLoadAssets(key, addressableType, assetType, callback));
    }

    IEnumerator CLoadAssets<T>(string key, eAddressableType addressableType, eAssetType assetType, Action<List<T>> callback)
    {
        var paths = GetPaths(key, addressableType, assetType);
        List<T> retList = new List<T>();
        foreach (var path in paths)
        {
            yield return CLoadAsset<T>(path, obj =>
            {
                retList.Add(obj);
            });
        }
        yield return new WaitUntil(() => paths.Count == retList.Count);
        callback.Invoke(retList);
    }

    public void LoadAsset<T>(string key, eAddressableType addressableType, Action<T> callback)
    {
        var path = GetPath(key, addressableType);
        LoadAsset<T>(path, callback);
    }

    public void LoadAsset<T>(string path, Action<T> callback)
    {
        StartCoroutine(CLoadAsset(path, callback));
    }

    public IEnumerator CLoadAsset<T>(string path, Action<T> callback)
    {
        if (path.Contains(".prefab") && typeof(T) != typeof(GameObject) || path.Contains("UI/"))
        {
            var handler = Addressables.LoadAssetAsync<GameObject>(path);
            handler.Completed += (op) =>
            {
                callback.Invoke(op.Result.GetComponent<T>());
            };
            yield return handler;
        }
        else
        {
            var handler = Addressables.LoadAssetAsync<T>(path);
            handler.Completed += (op) =>
            {
                callback.Invoke(op.Result);
            };
            yield return handler;
        }
    }
#endif
        #endregion

        #region Addressable Async
#if USE_ASYNC

        void Start()
        {
            if (isAutoLoading)
                Init();
        }

        private Dictionary<eAddressableType, Dictionary<string, AddressableMap>> addressableMap = new Dictionary<eAddressableType, Dictionary<string, AddressableMap>>();

        private async void InitAddressableMap()
        {
            await Addressables.LoadAssetsAsync<TextAsset>("AddressableMap", (text) =>
            {
                var map = JsonUtility.FromJson<AddressableMapData>(text.text);
                var key = eAddressableType.prefab;
                Dictionary<string, AddressableMap> mapDic = new Dictionary<string, AddressableMap>();
                foreach (var data in map.list)
                {
                    key = data.addressableType;
                    if (!mapDic.ContainsKey(data.key))
                        mapDic.Add(data.key, data);
                }
                if (!addressableMap.ContainsKey(key)) addressableMap.Add(key, mapDic);

            }).Task;
            isInit = true;
        }

        public async Task LoadAddressable()
        {
            var init = await Addressables.InitializeAsync().Task;
            //var handle = Addressables.DownloadDependenciesAsync("InitDownload");
            //UILoading.instance.SetProgress(handle, "리소스 로딩 중...");
            ////StartCoroutine(SetProgress(handle));
            //await handle.Task;
            //switch (handle.Status)
            //{
            //    case AsyncOperationStatus.None:
            //        break;
            //    case AsyncOperationStatus.Succeeded:
            //        ICLogger.Log("다운로드 성공!");
            //        break;
            //    case AsyncOperationStatus.Failed:
            //        ICLogger.Log("다운로드 실패 : " + handle.OperationException.Message);
            //        ICLogger.LogError(handle.OperationException.ToString());
            //        break;
            //    default:
            //        break;
            //}
            //Addressables.Release(handle);
            InitAddressableMap();
        }

        public IEnumerator SetProgress(AsyncOperationHandle handle)
        {
            while (!handle.IsDone)
            {
                UILoading.instance.SetProgress(handle.GetDownloadStatus().Percent, "Resource Download...");
                yield return new WaitForEndOfFrame();
            }
            UILoading.instance.SetProgress(1);

        }

        public List<string> GetPaths(string key, eAddressableType addressableType)
        {
            var keys = new List<string>(addressableMap[addressableType].Keys);
            keys.RemoveAll(obj => !obj.Contains(key));
            List<string> retList = new List<string>();
            keys.ForEach(obj =>
            {
                retList.Add(addressableMap[addressableType][obj].path);
            });
            return retList;
        }
        public string GetPath(string key, eAddressableType addressableType)
        {
            var map = addressableMap[addressableType][key.Split('/').Last().ToLower()];
            return map.path;
        }

        public async Task<List<T>> LoadAssets<T>(string key, eAddressableType addressableType)
        {
            try
            {
                var paths = GetPaths(key, addressableType);
                List<T> retList = new List<T>();
                foreach (var path in paths)
                {
                    retList.Add(await LoadAssetAsync<T>(path));
                }
                return retList;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            return default;
        }

        public async Task<List<T>> LoadDataAssets<T>()
        {
            try
            {
                int idx = 0;
                var ao = Addressables.LoadAssetsAsync<T>("Data", (datas) => // ��Ʈ���� Addressable Label�� ������ ���̺�
                {

                });
                //UILoading.instance.SetProgress(ao, "������ �ε���...");
                var retList = new List<T>(await ao.Task);
                return retList;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            return default;
        }


        public async Task<T> LoadAsset<T>(string key, eAddressableType addressableType)
        {
            try
            {
                var path = GetPath(key, addressableType);
                return await LoadAssetAsync<T>(path);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
            return default;
        }
        private async Task<T> LoadAssetAsync<T>(string path)
        {
            try
            {
                if (typeof(T).IsSubclassOf(typeof(MonoBehaviour)))
                {
                    var obj = await Addressables.LoadAssetAsync<GameObject>(path).Task;
                    return obj.GetComponent<T>();
                }
                else
                    return await Addressables.LoadAssetAsync<T>(path).Task;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            return default;
        }

        public async Task LoadScene(string key, LoadSceneMode mode = LoadSceneMode.Additive)
        {
            try
            {
                var path = GetPath(key, eAddressableType.scene);
                await Addressables.LoadSceneAsync(path, mode).ToUniTask();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
#endif
        #endregion
    }
}