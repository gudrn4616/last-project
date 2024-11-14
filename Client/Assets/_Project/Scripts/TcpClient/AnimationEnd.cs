using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using UnityEngine.Events;

public class AnimationEnd : 
#if USE_AUTO_CACHING
    MonoAutoCaching
#else
    MonoBehaviour
#endif
{
    public void OnAnimationEnd()
    {
        SocketManager.instance.isAnimationPlaying = false;
        GameManager.instance.virtualCamera.Target.TrackingTarget = GameManager.instance.userCharacter.transform;
        Destroy(gameObject);
    }
}