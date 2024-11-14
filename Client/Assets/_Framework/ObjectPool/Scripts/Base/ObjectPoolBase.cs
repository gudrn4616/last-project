using Ironcow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class ObjectPoolBase
#if USE_AUTO_CACHING
: MonoAutoCaching
#else
: MonoBehaviour
#endif
{
    public abstract void Init(params object[] param);

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void Release()
    {
        PoolManager.instance.Release(this);
    }
}
