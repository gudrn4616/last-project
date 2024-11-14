using Ironcow;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public enum eUIPosition
{
    UI,
    Popup,
    Navigator,
    Top,
}

public class UIManager : MonoSingleton<UIManager>
{
    [SerializeField] private List<Transform> parents;
    [SerializeField] private Transform worldParent;

    private List<UIBase> uiList = new List<UIBase>();

    public static void SetWorldCanvas(Transform worldCanvas)
    {
        instance.worldParent = worldCanvas;
    }

    public static void SetParents(List<Transform> parents)
    {
        instance.parents = parents;
        instance.uiList.Clear();
    }

    public static async Task<T> Show<T>(params object[] param) where T : UIBase
    {
        var ui = instance.uiList.Find(obj => obj.name == typeof(T).ToString());
        if (ui == null)
        {
#if USE_COROUTINE
            T prefab = null;
            ResourceManager.instance.LoadAsset<T>(typeof(T).ToString(), eAddressableType.UI, obj =>
            {
                prefab = obj;
            });
            await UniTask.WaitUntil(() => prefab != null);
#elif USE_ASYNC
            var prefab = await ResourceManager.instance.LoadAsset<T>(typeof(T).ToString(), eAddressableType.UI);
#else
            var prefab = await ResourceManager.instance.LoadAsset<T>(typeof(T).ToString(), eAddressableType.UI);
#endif
            ui = Instantiate(prefab, instance.parents[(int)prefab.uiPosition]);
            ui.name = ui.name.Replace("(Clone)", "");
            if (ui.uiPosition == eUIPosition.UI)
            {
                instance.uiList.ForEach(obj =>
                {
                    if (obj.uiPosition == eUIPosition.UI) obj.gameObject.SetActive(false);
                });
            }
            instance.uiList.Add(ui);
        }
        ui.SetActive(ui.uiOptions.isActiveOnLoad);
        ui.opened?.Invoke(param);
        ui.uiOptions.isActiveOnLoad = true;
        return (T)ui;
    }

    public static void Hide<T>(params object[] param) where T : UIBase
    {
        var ui = instance.uiList.Find(obj => obj.name == typeof(T).ToString());
        if (ui != null)
        {
            instance.uiList.Remove(ui);
            if (ui.uiPosition == eUIPosition.UI)
            {
                var prevUI = instance.uiList.FindLast(obj => obj.uiPosition == eUIPosition.UI);
                prevUI.SetActive(true);
            }
            ui.closed?.Invoke(param);
            if (ui.uiOptions.isDestroyOnHide)
            {
                Destroy(ui.gameObject);
            }
            else
            {
                ui.SetActive(false);
            }
        }
    }


    public static T Get<T>() where T : UIBase
    {
        return (T)instance.uiList.Find(obj => obj.name == typeof(T).ToString());
    }

    public static bool IsOpened<T>() where T : UIBase
    {
        var ui = instance.uiList.Find(obj => obj.name == typeof(T).ToString());
        return ui != null && ui.gameObject.activeInHierarchy;
    }

    public static void ShowIndicator()
    {

    }

    public static void HideIndicator()
    {

    }
    public static async void ShowAlert(string desc, string title = "", string okBtn = "OK", string cancelBtn = "Cancel", UnityAction okCallback = null, UnityAction cancelCallback = null)
    {
#if USE_ASYNC || USE_COROUTINE
        await 
#endif
        Show<PopupAlert>(desc, title, okBtn, cancelBtn, okCallback, cancelCallback);
    }


    public static async void ShowAlert<T>(string desc, string title = "", string okBtn = "OK", string cancelBtn = "Cancel", UnityAction okCallback = null, UnityAction cancelCallback = null, T image = default)
    {
#if USE_ASYNC || USE_COROUTINE
        await 
#endif
        Show<PopupAlert>(desc, title, okBtn, cancelBtn, okCallback, cancelCallback, image);
    }

    public static async void ShowInputAlert(string desc, string title = "", UnityAction<string> okCallback = null, UnityAction cancelCallback = null, string okBtn = "", string cancelBtn = "")
    {

#if USE_ASYNC || USE_COROUTINE
        await 
#endif
        Show<PopupAlert>(desc, title, okBtn, cancelBtn, okCallback, cancelCallback);
    }



    public static void HideAlert()
    {
        Hide<PopupAlert>();
    }


    int count = 0;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            count++;
        }
        if (count > 5)
        {
            count = 0;
            SocketManager.instance.Disconnect(false);
        }
    }
}
