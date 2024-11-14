using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Ironcow
{
    public class S3Uploader : MonoBehaviour
    {
        public const string BUCKET_INSIDE = "ironcow-projectm";
        [SerializeField]
        public string identityPoolId = "ap-northeast-2:5655a0d3-db44-4360-a3a0-266d38226b90";

        string full = "";
        bool isAAB = false;
        IEnumerator Start()
        {
            yield return null;
            var platform = transform.GetChild(0).name;
            var dir = Application.dataPath.Replace("Assets", "") + "AssetBundles/" + platform + "/AssetBundle/";
            Dictionary<string, Hash128> hashes = new Dictionary<string, Hash128>();
            if (platform.Equals("StandaloneWindows") || platform.Equals("Android") || platform.Equals("IOS"))
            {
                Upload(platform, dir, hashes);
            }
            else
            {
                full = platform.Contains("Full") ? " Full" : "";
                isAAB = platform.ToLower().Contains(".aab");
                UploadApk(platform);
            }
        }

        public void requestUpload(int idx, List<string> list, string bucket, string uploadPath)
        {
            var platform = transform.GetChild(0).name;
            //var oldVersion = oldList.AssetBundle.Count <= idx ? 0 : platform == "Android" ? oldList.AssetBundle[idx].AndroidVersion : platform == "IOS" ? oldList.AssetBundle[idx].IOSVersion : oldList.AssetBundle[idx].WindowsVersion;
            //var nowVersion = nowList.AssetBundle.Count <= idx ? 0 : platform == "Android" ? nowList.AssetBundle[idx].AndroidVersion : platform == "IOS" ? nowList.AssetBundle[idx].IOSVersion : nowList.AssetBundle[idx].WindowsVersion;
            //if (oldVersion >= nowVersion && idx > 0)

            if (idx * 2 >= list.Count - 1)
            {
                if (idx * 2 < list.Count - 1)
                {
                    requestUpload(idx + 1, list, bucket, uploadPath);
                }
                else
                {
                    var patchListDir = Application.dataPath.Replace("Assets", "") + "AssetBundles/PatchList.json";
                    requestUpload(BUCKET_INSIDE, patchListDir, "Meta/PatchList.json", () =>
                    {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.playModeStateChanged += PlayModeChanged;
                        UnityEditor.EditorApplication.isPlaying = false;
#endif
                });
                }
            }
            else
            {
                var file = list[idx * 2];
                var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);

                FileInfo fileInfo = new FileInfo(file);
                var upload = uploadPath + fileInfo.Name;
            }
        }

        public void requestUpload(string bucket, string path, string uploadPath, UnityAction callback = null)
        {
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        }

#if UNITY_EDITOR
        private void PlayModeChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.EnteredEditMode)
            {
                var target = GameObject.Find("Uploader(Clone)");
                DestroyImmediate(target);
                /*
                var obj = GameObject.Find("Uploader(Clone)");
                while(obj != null)
                {
                    Destroy(obj);
                    obj = GameObject.Find("Uploader(Clone)");
                }*/
            }
        }
#endif

        int uploadCount = 0;
        int count = 0;
        public void Upload(string platform, string dir, Dictionary<string, Hash128> list)
        {
            /*NetworkManager.instance.requestS3Download("Meta", "PatchList.json", (response) =>
            {
                var patchListDir = Application.dataPath.Replace("Assets", "") + "AssetBundles/PatchList.json";
                oldList = JsonUtility.FromJson<PatchList>(response);
                nowList = JsonUtility.FromJson<PatchList>(File.ReadAllText(patchListDir));
                var files = Directory.GetFiles(dir).ToList();
                requestUpload(0, files, BUCKET_INSIDE, platform + "/AssetBundle/");
                foreach (var file in files)
                {
                    count++;
                    requestUpload(0, files, BUCKET_INSIDE, platform + "/AssetBundle/");
                }

                count++;
                //var patchListDir = Application.dataPath.Replace("Assets", "") + "AssetBundles/PatchList.json";
                //requestUpload(BUCKET_INSIDE, patchListDir, "Meta/PatchList.json");
                //UnityEditor.EditorApplication.isPlaying = false;
                //Destroy(gameObject);
            });*/
        }

        public void UploadApk(string name)
        {
            var path = Application.dataPath.Replace("Assets", "Build/") + name;
            requestUploadApk(path, BUCKET_INSIDE, string.Format("Apk/inside{0}.{1}", full, isAAB ? "aab" : "apk"));
        }

        public void requestUploadApk(string path, string bucket, string uploadPath)
        {
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            FileInfo fileInfo = new FileInfo(path);
        }

    }
}