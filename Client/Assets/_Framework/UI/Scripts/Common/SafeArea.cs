using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;

public class SafeArea : 
#if USE_AUTO_CACHING
    MonoAutoCaching
#else
    MonoBehaviour
#endif
{
    private void Awake()
    {
        var rectTransform = transform as RectTransform;
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
}