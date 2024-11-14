using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;
using UnityEditor.Build.Reporting;
using System.Reflection;
using UnityEngine.U2D;
using UnityEngine.UI;
using UnityEditor.U2D;

namespace Ironcow
{
    [ExecuteInEditMode]
    public class EditorHotkeyManager : MonoBehaviour
    {
        [MenuItem("Ironcow/Tool/Screen Shot #&d")]
        static void ScreenShot()
        {
            var path = Application.dataPath + "/screenshot/";
            if(!Directory.Exists(path)) Directory.CreateDirectory(path);
            DirectoryInfo info = new DirectoryInfo(path);
            var idx = info.GetFiles().Length / 2;
            ScreenCapture.CaptureScreenshot(path + "shot" + idx + ".png");
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        [MenuItem("Ironcow/PlayerPrefs/Auto Login Clear")]
        static void SetAutoLoginClear()
        {
            PlayerPrefs.DeleteKey("USER_ID_AUTO");
            PlayerPrefs.DeleteKey("editorNickname");
            PlayerPrefs.DeleteKey("JWT");
        }

        [MenuItem("Ironcow/PlayerPrefs/Reset UserInfo")]
        static void ResetUserInfo()
        {
            PlayerPrefs.SetString("userInfo", JsonUtility.ToJson(new UserInfo()));

        }

        [MenuItem("Ironcow/PlayerPrefs/Tutorial Clear")]
        static void SetTutorialClear()
        {
            PlayerPrefs.DeleteKey("TUTORIAL_CLEAR_DATA");
        }

        [MenuItem("Ironcow/PlayerPrefs/Patch List Clear")]
        static void SetPatchListClear()
        {
            AssetDatabase.DeleteAsset("Assets/StreamingAssets~/meta/PatchList.json");
        }
        private static MethodInfo createScriptMethod = typeof(ProjectWindowUtil).GetMethod("CreateScriptAsset", BindingFlags.Static | BindingFlags.NonPublic);

        [MenuItem("Assets/Create/UIBase Script", false, 32)]
        static void CreateUIBaseScript()
        {
            string templetePath = Application.dataPath + EditorDataSO.TempletePath.Replace("Assets", "") + "/";
            var template = templetePath + "UIBaseTemplete.cs.txt";
            var dest = "UI.cs";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(template, dest);
        }

        [MenuItem("Assets/Create/UIListBase Script", false, 32)]
        static void CreateUIListBaseScript()
        {
            string templetePath = Application.dataPath + EditorDataSO.TempletePath.Replace("Assets", "") + "/";
            var template = templetePath + "UIListBaseTemplete.cs.txt";
            var dest = "UI.cs";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(template, dest);
        }

        [MenuItem("Assets/Create/UIPopupScript", false, 33)]
        static void CreateUIPopupScript()
        {
            string templetePath = Application.dataPath + EditorDataSO.TempletePath.Replace("Assets", "") + "/";
            var template = templetePath + "UIPopupTemplete.cs.txt";
            var dest = "Popup.cs";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(template, dest);
        }

        [MenuItem("Assets/Create/UICanvasScript", false, 33)]
        static void CreateUICanvasScript()
        {
            string templetePath = Application.dataPath + EditorDataSO.TempletePath.Replace("Assets", "") + "/";
            var template = templetePath + "UICanvasTemplete.cs.txt";
            var dest = "UI.cs";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(template, dest);
        }

        [MenuItem("Assets/Create/UIListItemScript", false, 33)]
        static void CreateUIListItemScript()
        {
            string templetePath = Application.dataPath + EditorDataSO.TempletePath.Replace("Assets", "") + "/";
            var template = templetePath + "UIListItemTemplete.cs.txt";
            var dest = "Item.cs";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(template, dest);
        }


        [MenuItem("Assets/Create/NetworkScript", false, 34)]
        static void CreateRequestScript()
        {
            string templetePath = Application.dataPath + EditorDataSO.TempletePath.Replace("Assets", "") + "/";
            var template = templetePath + "NetworkTemplete.cs.txt";
            var dest = "NetworkScript.cs";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(template, dest);
        }


        [MenuItem("Assets/Create/WorldBaseScript", false, 35)]
        static void CreateWorldBaseScript()
        {
            string templetePath = Application.dataPath + EditorDataSO.TempletePath.Replace("Assets", "") + "/";
            var template = templetePath + "WorldBaseTemplete.cs.txt";
            var dest = "WorldBaseScript.cs";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(template, dest);
        }

        [MenuItem("Assets/Create/ManagerScript", false, 37)]
        static void CreateManagerScript()
        {
            string templetePath = Application.dataPath + EditorDataSO.TempletePath.Replace("Assets", "") + "/";
            var template = templetePath + "ManagerTemplete.cs.txt";
            var dest = "Manager.cs";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(template, dest);
        }


        [MenuItem("Assets/Create/MonoBehaviourScript", false, 38)]
        static void CreateMonoBehaviourScript()
        {
            string templetePath = Application.dataPath + EditorDataSO.TempletePath.Replace("Assets", "") + "/";
            var template = templetePath + "MonoBehaviourTemplete.cs.txt";
            var dest = "NewScript.cs";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(template, dest);
        }

        [MenuItem("Assets/Create/UnitScript", false, 34)]
        static void CreateUnitScript()
        {
            string templetePath = Application.dataPath + EditorDataSO.TempletePath.Replace("Assets", "") + "/";
            var template = templetePath + "UnitTemplete.cs.txt";
            var dest = "UnitScript.cs";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(template, dest);
        }

        [MenuItem("Assets/Create/BaseData", false, 34)]
        static void CreateBaseDataScript()
        {
            string templetePath = Application.dataPath + EditorDataSO.TempletePath.Replace("Assets", "") + "/";
            var template = templetePath + "BaseDataTemplete.cs.txt";
            var dest = "DataSO.cs";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(template, dest);
        }
    }
}