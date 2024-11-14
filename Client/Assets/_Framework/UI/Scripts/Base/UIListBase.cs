using Ironcow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIListBase<T> : UIBase, IUIList<T> where T : MonoBehaviour
{
    [SerializeField] protected Transform listParent;
    [SerializeField] protected T itemPrefab;
    protected List<T> items = new List<T>();

    public T AddItem()
    {
        var item = Instantiate(itemPrefab, listParent);
        items.Add(item);
        return item;
    }

    public void ClearList()
    {
        items.ForEach(obj =>
        {
            Destroy(obj.gameObject);
        });
        items.Clear();
    }

    public abstract void SetList();
}
