using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Ironcow
{
    public class UIThumbnailMaker : MonoBehaviour
    {
        [SerializeField] private Slider sliderRotation;
        [SerializeField] private Slider sliderZoom;
        [SerializeField] private ItemPrefab itemPrefab;
        [SerializeField] private Transform listParent;
        [SerializeField] private List<UnityEngine.Object> targetFolder;
        [SerializeField] private UnityEngine.Object thumbnailFolder;
        [SerializeField] private RawImage thumbnailImage;

        [SerializeField] private Transform objectParent;
        [SerializeField] private Camera targetCamera;

        private IEnumerator Start()
        {
            yield return null;
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

        GameObject lastAsset;
        string selectedRCode;

        public async void OnSelectItem(string path)
        {
#if UNITY_EDITOR
            selectedRCode = path.FileName();
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) return;
            if (lastAsset != null) Destroy(lastAsset);

            lastAsset = Instantiate(asset, objectParent);
            lastAsset.transform.localPosition = Vector3.zero;
            lastAsset.transform.localEulerAngles = Vector3.zero;
            var scripts = lastAsset.GetComponentsInChildren<Behaviour>();
            foreach (var script in scripts)
            {
                script.enabled = false;
            }
            if (lastAsset.TryGetComponent(out Animator animator))
            {
                animator.enabled = false;
            }
            if (lastAsset.TryGetComponent(out Rigidbody rigidbody))
            {
                rigidbody.useGravity = false;
            }
            thumbnailImage.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(ThumanailPathSO.ThumbnailPath + "/" + path.FileName() + ".png");
#endif
        }

        public void OnRotationChanged(float value)
        {
            var euler = objectParent.localEulerAngles;
            euler.y = value;
            objectParent.localEulerAngles = euler;
        }

        public void OnRotationXChanged(float value)
        {
            var euler = objectParent.localEulerAngles;
            euler.x = value;
            objectParent.localEulerAngles = euler;
        }

        public void OnRotationZChanged(float value)
        {
            var euler = objectParent.localEulerAngles;
            euler.z = value;
            objectParent.localEulerAngles = euler;
        }

        public void OnZoomChanged(float value)
        {
            targetCamera.fieldOfView = value;
        }

        public void OnVerticalChanged(float value)
        {
            var pos = targetCamera.transform.localPosition;
            pos.y = value;
            targetCamera.transform.localPosition = pos;
        }

        public void OnHorizontalChanged(float value)
        {
            var pos = targetCamera.transform.localPosition;
            pos.x = value;
            targetCamera.transform.localPosition = pos;
        }

        private async Task<Texture2D> ScreenShot(RenderTexture externalTexture)
        {
            Texture2D myTexture2D = new Texture2D(externalTexture.width, externalTexture.height);
            if (myTexture2D == null)
            {
                myTexture2D = new Texture2D(externalTexture.width, externalTexture.height);
            }

            //Make RenderTexture type variable
            RenderTexture tmp = RenderTexture.GetTemporary(
                externalTexture.width,
                externalTexture.height,
                0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB);

            Graphics.Blit(externalTexture, tmp);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tmp;

            myTexture2D.ReadPixels(new UnityEngine.Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);


            byte[] screenshot = myTexture2D.EncodeToPNG();
#if UNITY_EDITOR
            File.WriteAllBytes(ThumanailPathSO.ThumbnailPath + "/" + selectedRCode + ".png", screenshot);

            AssetDatabase.Refresh();

            thumbnailImage.texture = AssetDatabase.LoadAssetAtPath(ThumanailPathSO.ThumbnailPath + "/" + selectedRCode + ".png", typeof(Texture)) as Texture;
#endif
            return myTexture2D;
        }

        public void OnClickSave()
        {
            ScreenShot(targetCamera.targetTexture);
        }
    }
}