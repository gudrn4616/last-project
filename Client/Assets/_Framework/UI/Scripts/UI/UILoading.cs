using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;

#if USE_ASYNC || USE_COROUTINE
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

public class UILoading : MonoSingleton<UILoading>
{
    [SerializeField] private Image bg;
    [SerializeField] private Slider slider;
    [SerializeField] private TMPro.TMP_Text desc;

    protected override void Awake()
    {
        base.Awake();
        gameObject.SetActive(false);
    }

    public static void Show(Sprite bg = null)
    {
        instance.SetBG(bg);
        instance.gameObject.SetActive(true);
    }

    public void SetBG(Sprite bg = null)
    {
        if (bg != null)
            this.bg.sprite = bg;
    }

    public static void Hide()
    {
        instance.gameObject.SetActive(false);
    }

    public void SetProgress(float progress, string desc = "")
    {
        this.desc.text = desc;
        slider.value = progress;
    }

    public void SetProgress(AsyncOperation op, string desc = "")
    {
        this.desc.text = desc;
        StartCoroutine(Progress(op));
    }

    public IEnumerator Progress(AsyncOperation op)
    {
        while (op.isDone)
        {
            slider.value = op.progress;
            yield return new WaitForEndOfFrame();
        }
        slider.value = 1;
    }

#if USE_ASYNC
    public void SetProgress(AsyncOperationHandle op, string desc = "")
    {
        this.desc.text = desc;
        StartCoroutine(Progress(op));
    }

    public IEnumerator Progress(AsyncOperationHandle op)
    {
        while (op.IsDone)
        {
            slider.value = op.GetDownloadStatus().Percent;
            yield return new WaitForEndOfFrame();
        }
        slider.value = 1;
    }
#endif
}