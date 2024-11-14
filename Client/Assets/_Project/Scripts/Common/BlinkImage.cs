using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;

public enum eBlinkType
{
    none,
    blink,
    pingpong,
    smooth,
}

public class BlinkImage : 
#if USE_AUTO_CACHING
    MonoAutoCaching
#else
    MonoBehaviour
#endif
{
    [SerializeField] private eBlinkType blinkType;
    [SerializeField] private Image image;
    [SerializeField] private bool isLoop;
    [SerializeField] private int loopCount;
    [SerializeField] private float interval;

    private void OnEnable()
    {
        switch (blinkType)
        {
            case eBlinkType.blink:
                StartCoroutine(OnBlink()); break;
            case eBlinkType.pingpong:
                StartCoroutine(OnPingPong()); break;
            case eBlinkType.smooth:
                StartCoroutine(OnSmooth()); break;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public IEnumerator OnBlink()
    {
        float time = 0;
        while (isLoop || loopCount > 0)
        {
            yield return null;
            time += Time.deltaTime;
            if (time >= interval)
            {
                time = 0;
                image.enabled = !image.enabled;
                if(image.enabled && !isLoop)
                    loopCount--;
            }
        }
    }


    public IEnumerator OnPingPong()
    {
        float time = interval;
        float sign = -1;
        while (isLoop || loopCount > 0)
        {
            yield return null;
            time += Time.deltaTime * sign;

            var alpha = time / interval;
            image.color = new Color(1, 1, 1, alpha);
            if (alpha <= 0 || alpha >= 1) sign *= -1;
            if (image.color.a >= 1 && !isLoop)
            {
                loopCount--;
            }
        }
    }

    public IEnumerator OnSmooth()
    {
        float time = 0;
        float sign = 1;
        while (isLoop || loopCount > 0)
        {
            yield return null;
            time += Time.deltaTime * sign;

            var alpha = time / interval;
            image.color = new Color(1, 1, 1, alpha);
            if (image.color.a >= 1 && !isLoop)
            {
                loopCount--;
            }
        }
    }
}