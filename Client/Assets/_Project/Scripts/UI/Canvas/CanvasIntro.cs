using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CanvasIntro : CanvasBase<CanvasIntro>
{
    [SerializeField] private Image logo;
    [SerializeField] private Sprite logoSprite;

    protected override void Awake()
    {
        
    }

    private IEnumerator Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        logo.sprite = logoSprite;
        logo.SetNativeSize();
#endif
        yield return SceneManager.LoadSceneAsync("DontDestroy", LoadSceneMode.Additive);
        DataManager.instance.Init();
        yield return new WaitUntil(() => DataManager.instance.isInit);
        ResourceManager.instance.Init();
        yield return new WaitUntil(() => ResourceManager.instance.isInit);
        yield return new WaitForSeconds(2);
        SceneManager.LoadSceneAsync("Main");
    }
}