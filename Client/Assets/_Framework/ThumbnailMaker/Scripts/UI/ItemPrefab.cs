using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Ironcow
{
    public class ItemPrefab : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text nameText;
        private UnityAction<string> callback;
        string path;
        public void SetData(string rcode, string path, UnityAction<string> callback)
        {
            nameText.text = rcode;
            this.path = path;
            this.callback = callback;
            gameObject.SetActive(true);
        }

        public void OnClickItem()
        {
            callback.Invoke(path);
        }
    }
}