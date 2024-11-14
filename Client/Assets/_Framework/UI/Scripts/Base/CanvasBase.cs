using Ironcow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if USE_AUTO_CACHING
public class CanvasBase<T> : MonoSingleton<T> where T : CanvasBase<T>
#else
public class CanvasBase : MonoBehaviour
#endif
{
    [SerializeField] protected List<Transform> parents;
    [SerializeField] private bool isCreateSafeArea;

#if USE_AUTO_CACHING
    protected async override void Awake()
    {
        base.Awake();
#else
    void Awake()
    {
#endif
        if (parents.Count > 0) UIManager.SetParents(parents);
        for (int i = 0; i < parents.Count; i++)
        {
            var rectTransform = parents[i] as RectTransform;
            var safeArea = Screen.safeArea;
            var minAnchor = safeArea.position;
            var maxAnchor = minAnchor + safeArea.size;

            minAnchor.x /= Screen.width;
            minAnchor.y /= Screen.height;
            maxAnchor.x /= Screen.width;
            maxAnchor.y /= Screen.height;

            rectTransform.anchorMin = minAnchor;
            rectTransform.anchorMax = maxAnchor;
        }
        if(isCreateSafeArea)
        {
            var ui = Instantiate(await ResourceManager.instance.LoadAsset<GameObject>("SafeArea", eAddressableType.Prefabs), transform);
        }
    }
}
