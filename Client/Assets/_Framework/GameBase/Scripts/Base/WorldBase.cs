using Ironcow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class WorldBase<T> : ObjectPoolBase where T : BaseDataSO
{
    [HideInInspector] public T data;

    public override void Init(params object[] param)
    {
        if(param.Length > 0)
            Init(DataManager.instance.GetData<BaseDataSO>((string)param[0]));
    }

    public abstract void Init(BaseDataSO data);

}
