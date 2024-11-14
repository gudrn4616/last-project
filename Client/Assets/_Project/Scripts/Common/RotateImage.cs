using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;

public class RotateImage : 
#if USE_AUTO_CACHING
    MonoAutoCaching
#else
    MonoBehaviour
#endif
{
    private void Update()
    {
        transform.Rotate(0, 0, 2);
    }
}