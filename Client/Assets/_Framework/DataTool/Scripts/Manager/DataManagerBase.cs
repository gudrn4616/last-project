using System.Collections.Generic;
using UnityEngine;

namespace Ironcow
{
    public class DataManagerBase<T> : GSpreadReader<T> where T : DataManagerBase<T>
    {
        public UserInfo userInfo;

        private Dictionary<string, BaseDataSO> dataDics = new Dictionary<string, BaseDataSO>();

        public
#if USE_ASYNC
            async 
#endif
            override void Init()
        {
#if USE_ASYNC
            AddDataDics(await ResourceManager.instance.LoadDataAssets<BaseData>());
#else
#if USE_SO_DATA
            AddDataDics(Resources.LoadAll<BaseDataSO>("Datas").ToList());
#endif
#endif
            isInit = true;
        }

        public T GetData<T>(string rcode) where T : BaseDataSO
        {
            return (T)dataDics[rcode];
        }

        public T GetCloneData<T>(string rcode) where T : BaseDataSO
        {
            return (T)dataDics[rcode].clone;
        }

        public override void AddDataDics<T>(List<T> datas)
        {
            dataDics.AddRange(datas);
        }

        public List<T> GetDatas<T>() where T : BaseDataSO
        {
            List<T> datas = new List<T>();
            foreach(var key in dataDics.Keys)
            {
                if (dataDics[key].GetType().Equals(typeof(T)))
                {
                    datas.Add((T)dataDics[key]);
                }
            }
            return datas;
        }
    }
}