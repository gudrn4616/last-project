using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;

namespace Ironcow
{
    public class PoolManager : MonoSingleton<PoolManager>
    {
        Dictionary<string, Queue<ObjectPoolBase>> pools = new Dictionary<string, Queue<ObjectPoolBase>>();
        public bool isInit = false;

        public async void Init()
        {
            foreach (var data in ObjectPoolDataSO.SharedInstance.objectPoolDatas)
            {
#if USE_ASYNC
                data.prefab = await ResourceManager.instance.LoadAsset<ObjectPoolBase>(data.rCode, eAddressableType.prefab);
#elif USE_COROUTINE
                ResourceManager.instance.LoadAsset<ObjectPoolBase>(data.rCode, eAddressableType.prefab, (obj) =>
                {
                    data.prefab = obj;
                };
#endif
                data.parent = new GameObject(data.rCode + "parent").transform;
                data.parent.parent = transform;
                Queue<ObjectPoolBase> queue = new Queue<ObjectPoolBase>();
                pools.Add(data.rCode, queue);
                for (int i = 0; i < data.count; i++)
                {
                    var obj = Instantiate(data.prefab, data.parent);
                    obj.name = obj.name.Replace("(Clone)", "");
                    obj.SetActive(false);
                    queue.Enqueue(obj);
                }
            }
            isInit = true;
        }

        public T Spawn<T>(string rcode, params object[] param) where T : ObjectPoolBase
        {
            if (pools[rcode].Count == 0)
            {
                var data = ObjectPoolDataSO.SharedInstance.objectPoolDatas.Find(obj => obj.rCode == rcode);
                for (int i = 0; i < data.count; i++)
                {
                    var obj = Instantiate(data.prefab, data.parent);
                    obj.name.Replace("(Clone)", "");
                    pools[rcode].Enqueue(obj);
                }
            }
            var retObj = (T)pools[rcode].Dequeue();
            retObj.SetActive(true);
            retObj.Init(param);
            return retObj;
        }

        public T Spawn<T>(string rcode, Transform parent, params object[] param) where T : ObjectPoolBase
        {
            var obj = Spawn<T>(rcode, param);
            obj.transform.parent = parent;
            return obj;
        }

        public T Spawn<T>(string rcode, Vector3 position, Transform parent, params object[] param) where T : ObjectPoolBase
        {
            var obj = Spawn<T>(rcode, parent, param);
            obj.transform.position = position;
            return obj;
        }

        public T Spawn<T>(string rcode, Vector3 position, Quaternion rotation, Transform parent, params object[] param) where T : ObjectPoolBase
        {
            var obj = Spawn<T>(rcode, position, parent, param);
            obj.transform.rotation = rotation;
            return obj;
        }

        public void Release(ObjectPoolBase item)
        {
            item.SetActive(false);
            var data = ObjectPoolDataSO.SharedInstance.objectPoolDatas.Find(obj => obj.rCode == item.name);
            item.transform.parent = data.parent;
            pools[item.name].Enqueue(item);
        }
    }
}