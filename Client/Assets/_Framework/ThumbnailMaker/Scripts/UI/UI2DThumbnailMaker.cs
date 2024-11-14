using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Ironcow
{
    public class UI2DThumbnailMaker : UIListBase<ItemPrefab>
    {
        [SerializeField] private List<Sprite> masks = new List<Sprite>();
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private Image viewport;
        [SerializeField] private Image model;
        [SerializeField] private Image cameraRect;
        [SerializeField] private RawImage thumbnail;
        [SerializeField] private TMP_InputField cardName;

        [SerializeField] private Transform objectParent;
        [SerializeField] private Camera camera;

        private IEnumerator Start()
        {
            yield return null;
            SetList();
        }

        GameObject lastAsset;
        string selectedRCode;

        public async void OnSelectItem(string path)
        {
#if UNITY_EDITOR
            selectedRCode = path.FileName();
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) return;

            model.sprite = sprite;
            thumbnail.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(ThumanailPathSO.ThumbnailPath + "/" + path.FileName().Split('_')[0] + ".png");
#endif
        }

        public void OnZoomChanged(float value)
        {
            model.transform.localScale = new Vector3(3 - value, 3 - value, 3 - value);
        }

        public void OnVerticalChanged(float value)
        {
            var pos = model.rectTransform.anchoredPosition;
            pos.y = -value;
            model.rectTransform.anchoredPosition = pos;
        }

        public void OnHorizontalChanged(float value)
        {
            var pos = model.rectTransform.anchoredPosition;
            pos.x = -value;
            model.rectTransform.anchoredPosition = pos;
        }

        public void OnMaskChange(int value)
        {
            viewport.sprite = masks[value];
        }

        private IEnumerator ScreenShot()
        {
            yield return new WaitForEndOfFrame();
            //await Task.Delay(10); // it must be a coroutine 

            Vector3[] corners = new Vector3[4];
            viewport.rectTransform.GetWorldCorners(corners);

            int width = ((int)corners[3].x - (int)corners[0].x);
            int height = (int)corners[1].y - (int)corners[0].y;
            var startX = corners[0].x;
            var startY = corners[0].y;

            var tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            tex.ReadPixels(new Rect(startX, startY, width + startX, height), 0, 0);
            tex.Apply();
            for (int x = 0; x < tex.width; ++x)
            {
                for (int y = 0; y < tex.height; ++y)
                {
                    Color color = tex.GetPixel(x, y);
                    if (color == cameraRect.color)
                    {
                        tex.SetPixel(x, y, new Color(0, 0, 0, 0));
                    }
                    else
                    { 
                        tex.SetPixel(x, y,color);
                    }
                }
            }

            // Encode texture into PNG
            var bytes = tex.EncodeToPNG();
            //Destroy(tex);

            string imgsrc = System.Convert.ToBase64String(bytes);
            Texture2D scrnShot = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            scrnShot.LoadImage(System.Convert.FromBase64String(imgsrc));

#if UNITY_EDITOR
            var name = cardName != null && cardName.text.Length > 0 ? cardName.text : selectedRCode.Split('_')[0];
            File.WriteAllBytes(ThumanailPathSO.ThumbnailPath + "/" + name + ".png", bytes);

            AssetDatabase.Refresh();

            thumbnail.texture = scrnShot;// AssetDatabase.LoadAssetAtPath(ThumanailPathSO.ThumbnailPath + "/" + selectedRCode + ".png", typeof(Texture)) as Texture;
#endif
        }

        public void OnClickSave()
        {
            StartCoroutine(ScreenShot());
        }

        public override void SetList()
        {
#if UNITY_EDITOR
            foreach (var folder in ThumanailPathSO.SharedInstance.prefabFolders)
            {
                var path = AssetDatabase.GetAssetPath(folder) + "/";
                var fullPath = UnityEngine.Application.dataPath.Replace("/Assets", "/") + path;
                Debug.Log(fullPath);
                var files = Directory.GetFiles(fullPath).ToList();
                files.RemoveAll(obj => obj.Contains(".meta"));
                for (int i = 0; i < files.Count; i++)
                {
                    var item = Instantiate(itemPrefab, listParent);
                    FileInfo file = new FileInfo(files[i]);
                    item.SetData(file.Name, path + file.Name, OnSelectItem);
                }
            }
#endif
        }
    }
}