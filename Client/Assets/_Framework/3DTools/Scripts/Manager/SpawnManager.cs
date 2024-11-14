using Google.Protobuf.WellKnownTypes;
using System.Threading.Tasks;
using UnityEngine;

namespace Ironcow
{
    public class SpawnManager : MonoSingleton<SpawnManager>
    {
        [SerializeField] private Transform objectPool;
        private Transform ObjectPool
        {
            get
            {
                if (objectPool == null) objectPool = new GameObject("ObjectPool").transform;
                return objectPool;
            }
        }

        public bool OnRay(ref Vector3 position)
        {
            position.y = 200;
            Ray ray = new Ray(position, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Ground")) return false;
                position = hit.point;
                return true;
            }
            return false;
        }

        public async Task<bool> SpawnObject(BaseDataSO data, Vector3 position, bool isRandom = false)
        {
            if (OnRay(ref position) || !isRandom)
            {
#if USE_SO_DATA
#if USE_OBJECT_POOL
                var worldObject = PoolManager.instance.Spawn<WorldBase<BaseDataSO>>(data.rcode, position, ObjectPool);
#elif USE_COROUTINE
                WorldBase<BaseDataSO> worldObject = null;
                ResourceManager.instance.LoadAsset<WorldBase<BaseDataSO>>(data.rcode, eAddressableType.Prefabs, obj =>
                {
                    worldObject = Instantiate(obj , position, Quaternion.identity, ObjectPool);
                });

                float timeCheck = 0;
                while (worldObject != null)
                {
                    timeCheck += Time.deltaTime;
                    if (timeCheck > 5)
                    {
                        break;
                    }
                    await System.Threading.Tasks.Task.Delay(100);
                }
#elif USE_ASYNC
                var worldObject = Instantiate(await ResourceManager.instance.LoadAsset<WorldBase<BaseDataSO>>(data.rcode, eAddressableType.Prefabs), position, Quaternion.identity, ObjectPool);
#else
                var worldObject = Instantiate(await ResourceManager.instance.LoadAsset<WorldBase<BaseDataSO>>(data.rcode, eAddressableType.Prefabs), position, Quaternion.identity, ObjectPool);
#endif
                worldObject.name = worldObject.name.Replace("(Clone)", "");
                worldObject.Init(data);
#endif
                return true;
            }
            return false;
        }
    }
}