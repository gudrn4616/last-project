using UnityEngine;
using UnityEditor;
using System;
using System.IO;
//using SFB;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ironcow;
using GUI = UnityEngine.GUI;
using Unity.VisualScripting;

namespace Ironcow
{
    public class BuildTool : EditorWindow
    {
        public static BuildTool instance;
        [MenuItem("Ironcow/Tool/Build #&1")]
        public static void Open()
        {
            var window = GetWindow<BuildTool>();
            window.minSize = new Vector2(1024f, 728f);
            instance = window;
        }

        private ApplicationSettings applicationSettings = null;
        private Vector2 sceneViewScrollPosition = Vector2.zero;
        private Vector2 windowScrollPosition = Vector2.zero;
        private bool actionUpload = false;

        private void OnFocus()
        {
            if (this.applicationSettings == null)
                this.OnEnable();

        }

        private void OnEnable()
        {
            this.applicationSettings = ApplicationSettings.SharedInstance;
        }

        private async void Draw()
        {
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("현재 빌드 타겟", EditorUserBuildSettings.activeBuildTarget.ToString(), EditorStyles.boldLabel);
            if (GUILayout.Button("Windows", GUILayout.Width(80f)))
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Unknown, BuildTarget.StandaloneWindows);
            }
            if (GUILayout.Button("Android", GUILayout.Width(80f)))
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }
            if (GUILayout.Button("IOS", GUILayout.Width(80f)))
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
            }

            GUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.LabelField("ApplicationSettings", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.ObjectField(this.applicationSettings, typeof(ApplicationSettings), false);
                        if (GUILayout.Button("Edit", GUILayout.Width(80f)))
                        {
                            Selection.activeObject = this.applicationSettings;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    if (this.applicationSettings.CurrentBuildSettings != null)
                    {
                        this.applicationSettings.LogEnabled = EditorGUILayout.Toggle("Log Enabled", this.applicationSettings.LogEnabled);
                        this.applicationSettings.Development = EditorGUILayout.Toggle("Development", this.applicationSettings.Development);
                        this.applicationSettings.TestMode = EditorGUILayout.Toggle("TestMode", this.applicationSettings.TestMode);
                        this.applicationSettings.FullBuild = EditorGUILayout.Toggle("FullBuild", this.applicationSettings.FullBuild);

                        EditorGUILayout.BeginHorizontal();
                        {
                            this.applicationSettings.CurrentBuildSettings.TargetService =
                                (BuildSettings.Service)EditorGUILayout.EnumPopup("Service", this.applicationSettings.CurrentBuildSettings.TargetService);
                            EditorGUILayout.LabelField("Version", this.applicationSettings.CurrentBuildSettings.Version);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUI.indentLevel--;

                GUILayout.Space(10f);

                EditorGUILayout.LabelField("Facebook", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                {
                    //EditorGUILayout.LabelField("SDK Version", Facebook.Unity.FacebookSdkVersion.Build);
                    EditorGUILayout.LabelField("App Name", this.applicationSettings.FacebookAppName);
                    EditorGUILayout.LabelField("App Id", this.applicationSettings.FacebookAppId);

                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Edit"))
                        {
                            //Facebook.Unity.Editor.FacebookSettingsEditor.Edit();
                        }

                        if (GUILayout.Button("Apply"))
                        {
                            this.ApplyFacebook();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;

                GUILayout.Space(10f);

                EditorGUILayout.LabelField("BuildSetting List", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                {
                    for (int i = 0; i < this.applicationSettings.BuildSettingsList.Count; ++i)
                    {
                        BuildSettings value = this.applicationSettings.BuildSettingsList[i];

                        EditorGUILayout.BeginHorizontal();
                        {
                            if (value == null)
                            {
                                EditorGUILayout.ObjectField("Null", value, typeof(BuildSettings), false);

                                UnityEngine.GUI.backgroundColor = Color.red;
                                if (GUILayout.Button("Remove"))
                                {
                                    this.applicationSettings.BuildSettingsList.RemoveAt(i);
                                    EditorGUILayout.EndHorizontal();
                                    break;
                                }

                                UnityEngine.GUI.backgroundColor = Color.white;
                            }
                            else
                            {
                                if (this.applicationSettings.CurrentBuildSettings == value)
                                    UnityEngine.GUI.backgroundColor = Color.yellow;

                                this.applicationSettings.BuildSettingsList[i] =
                                    EditorGUILayout.ObjectField(value.Name, value, typeof(BuildSettings), false) as BuildSettings;

                                UnityEngine.GUI.backgroundColor = Color.white;

                                if (GUILayout.Button("Edit", GUILayout.Width(150f)))
                                {
                                    Selection.activeObject = value;
                                }

                                bool disabled = this.applicationSettings.CurrentBuildSettings == value;
                                if (!disabled)
                                {
                                    switch (EditorUserBuildSettings.activeBuildTarget)
                                    {
                                        case BuildTarget.Android:
                                            {
                                                if (value.TargetPlatform != BuildSettings.Platform.Android)
                                                    disabled = true;
                                            }
                                            break;

                                        case BuildTarget.iOS:
                                            {
                                                if (value.TargetPlatform != BuildSettings.Platform.iOS)
                                                    disabled = true;
                                            }
                                            break;
                                    }
                                }

                                EditorGUI.BeginDisabledGroup(disabled);
                                {
                                    UnityEngine.GUI.backgroundColor = Color.green;
                                    if (GUILayout.Button("Select", GUILayout.Width(150f)))
                                    {
                                        this.applicationSettings.CurrentBuildSettings = value;

                                        PlayerSettings.bundleVersion = value.Version;
                                    }

                                    UnityEngine.GUI.backgroundColor = Color.white;

                                    UnityEngine.GUI.backgroundColor = Color.red;
                                    if (GUILayout.Button("Delete"))
                                    {
                                        if (EditorUtility.DisplayDialog("알림", "선택된 빌드환경 파일을 지우시겠습니까?", "Ok", "Cancel"))
                                        {
                                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(value));
                                            this.applicationSettings.BuildSettingsList.RemoveAt(i);
                                            EditorGUILayout.EndHorizontal();
                                            break;
                                        }
                                    }

                                    UnityEngine.GUI.backgroundColor = Color.white;
                                }
                                EditorGUI.EndDisabledGroup();
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Add BuildSetting"))
                        {
                            string path = EditorUtility.OpenFilePanel("Add BuildSetting...", "Assets/Settings/Resources", "asset");
                            if (!string.IsNullOrEmpty(path))
                            {
                                int index = path.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
                                if (index != -1)
                                    path = path.Substring(index);

                                BuildSettings asset = AssetDatabase.LoadAssetAtPath(path, typeof(BuildSettings)) as BuildSettings;
                                if (asset != null)
                                {
                                    bool skipAdd = false;

                                    for (int i = 0; i < this.applicationSettings.BuildSettingsList.Count; ++i)
                                    {
                                        if (this.applicationSettings.BuildSettingsList[i] == null)
                                        {
                                            this.applicationSettings.BuildSettingsList.Add(asset);
                                            skipAdd = true;
                                            break;
                                        }
                                    }

                                    if (!skipAdd)
                                        this.applicationSettings.BuildSettingsList.Add(asset);
                                }
                            }
                        }

                        if (GUILayout.Button("Create New BuildSettings..."))
                        {
                            BuildSettings instance = BuildSettings.Create();

                            if (instance != null)
                            {
                                this.applicationSettings.BuildSettingsList.Add(instance);

                                Selection.activeObject = instance;
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;

                if (GUILayout.Button("Apply PlayerSettings..."))
                {
                    this.Apply();
                }

                GUILayout.Space(10f);

#if UNITY_IOS
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField( "Target SDK" );

                    if( GUILayout.Button( "Device SDK" ) )
                    {
                        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
                    }

                    if( GUILayout.Button( "Simulator SDK" ) )
                    {
                        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.SimulatorSDK;
                    }
                }
                EditorGUILayout.EndHorizontal();
#endif

                if (this.applicationSettings.CurrentBuildSettings != null)
                {
                    EditorGUILayout.LabelField("현재 빌드 환경", this.applicationSettings.CurrentBuildSettings.Name, EditorStyles.boldLabel);
                    GUILayout.Space(10f);
                }

                EditorGUI.indentLevel++;
                {
                    if (this.applicationSettings.CurrentBuildSettings != null)
                    {
                        EditorGUILayout.LabelField("Company Name", this.applicationSettings.CurrentBuildSettings.CompanyName);
                        EditorGUILayout.LabelField("Product Name", this.applicationSettings.CurrentBuildSettings.ProductName);
                        GUILayout.BeginHorizontal();
                        this.applicationSettings.CurrentBuildSettings.Version = EditorGUILayout.TextField("Version*", this.applicationSettings.CurrentBuildSettings.Version);
                        var ver = int.Parse(this.applicationSettings.CurrentBuildSettings.Version.Replace(".", ""));
                        if (GUILayout.Button("+", GUILayout.Width(20)))
                        {
                            ver++;
                            this.applicationSettings.CurrentBuildSettings.Version = string.Format("{0}.{1:00}.{2:000}", ver / 100000, (ver / 1000) % 100, ver % 1000);
                        }
                        if (GUILayout.Button("-", GUILayout.Width(20)))
                        {
                            ver--;
                            this.applicationSettings.CurrentBuildSettings.Version = string.Format("{0}.{1:00}.{2:000}", ver / 100000, (ver / 1000) % 100, ver % 1000);
                        }
                        if (this.applicationSettings.CurrentBuildSettings.TargetService == BuildSettings.Service.DEVELOPER)
                        {
                            ver = int.Parse(this.applicationSettings.CurrentBuildSettings.DeveloperVersion.Replace(".", "")) - 1;
                            this.applicationSettings.CurrentBuildSettings.StagingVersion = string.Format("{0}.{1:00}.{2:000}", ver / 100000, (ver / 1000) % 100, ver % 1000);
                            ver = int.Parse(this.applicationSettings.CurrentBuildSettings.StagingVersion.Replace(".", "")) - 1;
                            this.applicationSettings.CurrentBuildSettings.LiveVersion = string.Format("{0}.{1:00}.{2:000}", ver / 100000, (ver / 1000) % 100, ver % 1000);
                        }
                        else if (this.applicationSettings.CurrentBuildSettings.TargetService == BuildSettings.Service.STAGING)
                        {
                            ver = int.Parse(this.applicationSettings.CurrentBuildSettings.StagingVersion.Replace(".", "")) - 1;
                            this.applicationSettings.CurrentBuildSettings.LiveVersion = string.Format("{0}.{1:00}.{2:000}", ver / 100000, (ver / 1000) % 100, ver % 1000);
                            ver = int.Parse(this.applicationSettings.CurrentBuildSettings.StagingVersion.Replace(".", "")) + 1;
                            this.applicationSettings.CurrentBuildSettings.DeveloperVersion = string.Format("{0}.{1:00}.{2:000}", ver / 100000, (ver / 1000) % 100, ver % 1000);
                        }
                        else
                        {
                            ver = int.Parse(this.applicationSettings.CurrentBuildSettings.LiveVersion.Replace(".", "")) + 1;
                            this.applicationSettings.CurrentBuildSettings.StagingVersion = string.Format("{0}.{1:00}.{2:000}", ver / 100000, (ver / 1000) % 100, ver % 1000);
                            ver = int.Parse(this.applicationSettings.CurrentBuildSettings.StagingVersion.Replace(".", "")) + 1;
                            this.applicationSettings.CurrentBuildSettings.DeveloperVersion = string.Format("{0}.{1:00}.{2:000}", ver / 100000, (ver / 1000) % 100, ver % 1000);
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.Space(5f);
                        EditorGUILayout.ObjectField("Application Icon", this.applicationSettings.CurrentBuildSettings.Icon, typeof(Texture2D), false);

#if UNITY_ANDROID
                    GUILayout.Space(10f);

                    EditorGUILayout.LabelField("Identification", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    {
                        EditorGUILayout.TextField("Package Name", this.applicationSettings.CurrentBuildSettings.PackageName);
                        //applicationSettings.CurrentBuildSettings.BundleVersion = int.Parse(this.applicationSettings.CurrentBuildSettings.Version.Replace(".", ""));
                        EditorGUILayout.IntField("Bundle Version Code", this.applicationSettings.CurrentBuildSettings.BundleVersion);
                        PlayerSettings.Android.minSdkVersion =
                            (AndroidSdkVersions)EditorGUILayout.EnumPopup("Minimum API Level", PlayerSettings.Android.minSdkVersion);
                        PlayerSettings.Android.targetSdkVersion =
                            (AndroidSdkVersions)EditorGUILayout.EnumPopup("Target API Level", PlayerSettings.Android.targetSdkVersion);

                        EditorGUILayout.LabelField("Target Architectures", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        {
                            this.applicationSettings.CurrentBuildSettings.ArchitectureARMv7 =
                                EditorGUILayout.Toggle("ARMv7", false);
                            this.applicationSettings.CurrentBuildSettings.ArchitectureARM64 =
                                EditorGUILayout.Toggle("ARM64", true);
                            this.applicationSettings.CurrentBuildSettings.ArchitectureX86 =
                                EditorGUILayout.Toggle("x86", false);
                        }
                        EditorGUI.indentLevel--;


                        ScriptingImplementation oldScriptingImplementation = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);
                        oldScriptingImplementation = ScriptingImplementation.IL2CPP;
                        ScriptingImplementation newScriptingImplementation =
                            (ScriptingImplementation)EditorGUILayout.EnumPopup("Scripting Backend", oldScriptingImplementation);
                        if (oldScriptingImplementation != newScriptingImplementation)
                        {
                            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, newScriptingImplementation);
                        }
                    }
                    EditorGUI.indentLevel--;

                    this.applicationSettings.CurrentBuildSettings.AppBundle =
                        EditorGUILayout.Toggle("App Bundle", this.applicationSettings.CurrentBuildSettings.TargetService == BuildSettings.Service.APP_BUNDLE);

                    GUILayout.Space(10f);
                    
                    GUILayout.Space(10f);


                    EditorGUILayout.LabelField("Keystore", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            this.applicationSettings.CurrentBuildSettings.KeyStoreName =
                                EditorGUILayout.TextField("Key Store Name", this.applicationSettings.CurrentBuildSettings.KeyStoreName);

                            if (GUILayout.Button("Browse Keystore", GUILayout.Width(150f)))
                            {
                                string directory = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets", StringComparison.Ordinal));
                                string path = EditorUtility.OpenFilePanel("Open existing keystore...", System.IO.Path.Combine(directory, "Keystore"), "keystore");
                                if (!string.IsNullOrEmpty(path))
                                {
                                    this.applicationSettings.CurrentBuildSettings.KeyStoreName = path;
                                }
                            }

                            if (GUILayout.Button("Reveal In Finder", GUILayout.Width(150f)))
                            {
                                EditorUtility.RevealInFinder(this.applicationSettings.CurrentBuildSettings.KeyStoreName);
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        this.applicationSettings.CurrentBuildSettings.KeyStorePass =
                            EditorGUILayout.TextField("Key Store Pass", this.applicationSettings.CurrentBuildSettings.KeyStorePass);
                        this.applicationSettings.CurrentBuildSettings.KeyAliasName =
                            EditorGUILayout.TextField("Key Alias Name", this.applicationSettings.CurrentBuildSettings.KeyAliasName);
                        this.applicationSettings.CurrentBuildSettings.KeyAliasPass =
                            EditorGUILayout.TextField("Key Alias Pass", this.applicationSettings.CurrentBuildSettings.KeyAliasPass);
                    }
                    EditorGUI.indentLevel--;
#endif

#if UNITY_IOS
                        GUILayout.Space( 10f );

                        EditorGUILayout.LabelField( "Identification", EditorStyles.boldLabel );
                        EditorGUI.indentLevel++;
                        {
                            EditorGUILayout.LabelField( "Package Name", this.applicationSettings.CurrentBuildSettings.PackageName );
                            switch( this.applicationSettings.CurrentBuildSettings.TargetService )
                            {
                                case BuildSettings.Service.LIVE:
                                {
                                    EditorGUILayout.LabelField( "Build", this.applicationSettings.CurrentBuildSettings.LiveBuildNumber );
                                }
                                break;

                                //case BuildSettings.Service.STAGING:
                                //{
                                //    EditorGUILayout.LabelField( "Build", this.applicationSettings.CurrentBuildSettings.StagingBundleNumber );
                                //}
                                //break;

                                case BuildSettings.Service.DEVELOPER:
                                {
                                    EditorGUILayout.LabelField( "Build", this.applicationSettings.CurrentBuildSettings.DeveloperBuildNumber );
                                }
                                break;
                            }
                        }
                        EditorGUI.indentLevel--;

                        EditorGUILayout.LabelField("Ban Folder List");
                        for (int i = 0; i < this.applicationSettings.iosBanFolders.Count; ++i)
                        {
                            var value = this.applicationSettings.iosBanFolders[i];

                            EditorGUILayout.BeginHorizontal();
                            {
                                if (value != null)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField(value);
                                    if (GUILayout.Button("X", GUILayout.Width(20)))
                                    {
                                        this.applicationSettings.iosBanFolders.Remove(value);
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button("Add Ban Folders"))
                            {
                                string path = EditorUtility.OpenFolderPanel("Add Folder...", "Assets", "");
                                if (!string.IsNullOrEmpty(path))
                                {
                                    applicationSettings.iosBanFolders.Add("Assets" + path.Replace(Application.dataPath, ""));
                                }
                            }
                        }
                        EditorGUILayout.EndHorizontal();
#endif
                    }
                }
                EditorGUI.indentLevel--;

                if (EditorGUI.EndChangeCheck())
                {
                    if (EditorApplication.isPlaying)
                        Debug.unityLogger.logEnabled = this.applicationSettings.LogEnabled;

                    EditorUtility.SetDirty(this.applicationSettings);
                }
            }

            GUILayout.Space(20f);

            EditorGUILayout.LabelField("Scenes In Build", EditorStyles.boldLabel);
            if (EditorBuildSettings.scenes.Length > 0)
            {
                EditorGUI.indentLevel++;
                {
                    EditorGUILayout.Space();
                    EditorGUI.indentLevel++;
                    this.sceneViewScrollPosition = EditorGUILayout.BeginScrollView(this.sceneViewScrollPosition, "box", GUILayout.Height(150f));
                    {
                        int index = 0;
                        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.Toggle(scene.enabled);
                            EditorGUILayout.LabelField(index.ToString());
                            EditorGUILayout.LabelField(scene.path);
                            EditorGUILayout.EndHorizontal();
                            ++index;
                        }
                    }
                    EditorGUILayout.EndScrollView();
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }

            GUILayout.Space(10f);

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Build Settings..."))
                {
                    EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
                }

                if (GUILayout.Button("Player Settings..."))
                {
#if UNITY_2018_3_OR_NEWER
                    SettingsService.OpenProjectSettings("Project/Player");
#else
                    EditorApplication.ExecuteMenuItem( "Edit/Project Settings/Player" );
#endif
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5f);

            EditorGUI.BeginDisabledGroup(this.applicationSettings.CurrentBuildSettings == null || EditorApplication.isPlaying);
            {
                if (this.applicationSettings.CurrentBuildSettings != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
#if UNITY_ANDROID
                    UnityEngine.GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("Build"))
                    {
                        this.Build();
                    }
#elif UNITY_IOS
                        UnityEngine.GUI.backgroundColor = Color.green;
                        if( GUILayout.Button( "Build / Replace" ) )
                        {
                            this.Build();
                        }

                        UnityEngine.GUI.backgroundColor = Color.green;
                        if( GUILayout.Button( "Build / Append" ) )
                        {
                            this.Build( BuildOptions.AcceptExternalModificationsToPlayer );
                        }
#endif

                        UnityEngine.GUI.backgroundColor = Color.yellow;
                        if (GUILayout.Button("Build And Run"))
                        {
                            this.Build(BuildOptions.AutoRunPlayer);
                        }

#if UNITY_ANDROID
                    UnityEngine.GUI.backgroundColor = Color.cyan;
                    if (GUILayout.Button("Build And Upload"))
                    {
                        this.actionUpload = true;
                        this.Build();
                    }
                    UnityEngine.GUI.backgroundColor = Color.cyan;
                    if (GUILayout.Button("Upload"))
                    {
                        this.actionUpload = true;
                        string defaultValue = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets"));
                        string directory = EditorPrefs.GetString("Latest Build Directory", defaultValue);
                        /*var extensions = new[] {
                            new ExtensionFilter("APK", "apk", "aab" ),
                        };
                        var files = StandaloneFileBrowser.OpenFilePanel("APK 업로드", directory, extensions, false);*/
                        /*if (files.Length > 0)
                        {
                            //Upload(Path.GetFileName(files[0]));
                            await UploadToFTP(files[0]);
                        }*/
                        this.actionUpload = false;
                    }
#endif

                        UnityEngine.GUI.backgroundColor = Color.white;
#if UNITY_EDITOR && !UNITY_ANDROID && !UNITY_IOS
                        UnityEngine.GUI.backgroundColor = Color.yellow;
#endif
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private void OnGUI()
        {
            if (this.applicationSettings == null)
                return;

            this.windowScrollPosition = EditorGUILayout.BeginScrollView(this.windowScrollPosition, "box");
            {
                this.Draw();
            }
            EditorGUILayout.EndScrollView();
        }

        private void Build(BuildOptions buildOptions = BuildOptions.None)
        {
            if (this.applicationSettings.CurrentBuildSettings.TargetService == BuildSettings.Service.DEVELOPER)
            {
                if (EditorUtility.DisplayDialog("알림", "개발자 버전으로 설정되어있습니.\n\n빌드를 진행하시겠습니까?\n\n확인을 누르면 빌드를 진행합니다.", "확인", "취소"))
                {
                    this.Apply();
                    this.BuildPlayer(buildOptions);
                }
            }
            else if (EditorUtility.DisplayDialog("알림", "현재 선택된 빌드 환경으로 빌드를 진행하시겠습니까?\n\n빌드 설정을 다시 한번 확인하세요!!!(App Bundle)\n\n확인을 누르면 빌드를 진행합니다.", "확인", "취소"))
            {
                this.Apply();
                this.BuildPlayer(buildOptions);
            }
        }

        private void ApplyFacebook()
        {
            /*Facebook.Unity.Settings.FacebookSettings.AppLabels.Clear();
            Facebook.Unity.Settings.FacebookSettings.AppLabels.Add(this.applicationSettings.FacebookAppName);

            Facebook.Unity.Settings.FacebookSettings.AppIds.Clear();
            Facebook.Unity.Settings.FacebookSettings.AppIds.Add(this.applicationSettings.FacebookAppId);

            Facebook.Unity.Editor.ManifestMod.GenerateManifest();*/
        }

        private void BuildPlayer(BuildOptions buildOptions)
        {
#if UNITY_ANDROID
        PlayerSettings.SplashScreen.showUnityLogo = false;
        PlayerSettings.Android.splitApplicationBinary = this.applicationSettings.CurrentBuildSettings.AppBundle;
        string extension = this.applicationSettings.CurrentBuildSettings.AppBundle ? "aab" : "apk";
        string defaultValue = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets")) + "/Build";
        string directory = EditorPrefs.GetString("Latest Build Directory", defaultValue);
        string fileName = string.Format("[{0:d4}_{1:d2}_{2:d2}_{3:d2}_{4:d2}]{5}{6}_Ver.{7}", System.DateTime.Now.Year, System.DateTime.Now.Month,
            System.DateTime.Now.Day, System.DateTime.Now.Hour, System.DateTime.Now.Minute, PlayerSettings.productName, applicationSettings.FullBuild ? " Full" : "", PlayerSettings.bundleVersion.Replace(".", ""));
        if (!actionUpload && buildOptions != BuildOptions.AutoRunPlayer)
        {
            fileName = string.Format("{0}", PlayerSettings.productName);
        }
        fileName += applicationSettings.CurrentBuildSettings.TargetService == BuildSettings.Service.DEVELOPER ? "_Test" : applicationSettings.CurrentBuildSettings.TargetService == BuildSettings.Service.STAGING ? "_Staging" : "";
        string fullPath = EditorUtility.SaveFilePanel("Build", directory, fileName, extension);
        var target = string.Format("{0}{1}.{2}", PlayerSettings.productName, applicationSettings.CurrentBuildSettings.TargetService == BuildSettings.Service.DEVELOPER ? "_Test" :
            applicationSettings.CurrentBuildSettings.TargetService == BuildSettings.Service.STAGING ? "_Staging" : "",
            applicationSettings.CurrentBuildSettings.TargetService == BuildSettings.Service.APP_BUNDLE ? "aab" : "apk");
        if (!string.IsNullOrEmpty(fullPath))
        {
            EditorApplication.delayCall += async () =>
            {
#if UNITY_2018_3_OR_NEWER
                //await SendMessageAsync(target + " 빌드 시작합니다.");
                UnityEditor.Build.Reporting.BuildReport buildReport =
                            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, fullPath, BuildTarget.Android, buildOptions);
                Ironcow.ICLogger.LogWarning(buildReport.summary.result);
                if (buildReport.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
                {
                    //await SendMessageAsync(PlayerSettings.productName + " " + target + " 빌드 완료!");
                    if (this.actionUpload)
                    {
                        EditorPrefs.SetString("Latest Build Directory", System.IO.Path.GetDirectoryName(fullPath));
                        await UploadToFTP(fullPath);
                        /*if (EditorUtility.DisplayDialog("빌드 성공", "빌드를 완료하였습니다.\n업로드를 진행하시겠습니까?.", "확인", "취소"))
                        {
                            UploadToFTP(fullPath);
                        }*/
                    }
                    else
                    {
                        if (EditorUtility.DisplayDialog("빌드 성공", PlayerSettings.productName + " " + "빌드를 완료하였습니다.\n확인을 누르면 빌드된 폴더로 이동합니다.", "확인", "취소"))
                            EditorUtility.RevealInFinder(fullPath);
                    }
                }
#else
                string message = BuildPipeline.BuildPlayer( EditorBuildSettings.scenes, fullPath, BuildTarget.Android, buildOptions );

                // Windows에서 빌드시 아래와 같은 XCode 관련 오류 메세지이므로 해당 메세지는 스킵한다.
                // OSX환경에서 안드로이드 빌드시 이상없음.
                // Facebook.Unity.Editor.XCodePostProcess:OnPostProcessBuild(BuildTarget, String)
                // Your Android setup is not correct. See Settings in Facebook menu.
                if( message.Equals( "Your Android setup is not correct. See Settings in Facebook menu.", StringComparison.OrdinalIgnoreCase ) )
                {
                    message = string.Empty;
                }

                if( !string.IsNullOrEmpty( message ) )
                {
                    EditorApplication.Beep();
                    EditorUtility.DisplayDialog( "빌드 실패", message, Locale.GetString("AlertOk0") );
                }
                else
                {
                    EditorPrefs.SetString( "Latest Build Directory", System.IO.Path.GetDirectoryName( fullPath ) );
                    if( EditorUtility.DisplayDialog( "빌드 성공", "빌드를 완료하였습니다.\n확인을 누르면 빌드된 폴더로 이동합니다.", "확인", "취소" ) )
                        EditorUtility.RevealInFinder( fullPath );
                }
#endif
                };
        }
#elif UNITY_IOS
        PlayerSettings.iOS.statusBarStyle = (iOSStatusBarStyle)1;
            this.ExportXcode( buildOptions );
#endif
        }

        static void Upload(string path)
        {
            EditorApplication.isPlaying = true;
            var uploader = Instantiate(Resources.Load("Uploader")) as GameObject;
            uploader.transform.GetChild(0).name = path;
        }

        private void Apply()
        {
            EditorUtility.SetDirty(this.applicationSettings);

            this.ApplyFacebook();

            PlayerSettings.bundleVersion = this.applicationSettings.CurrentBuildSettings.Version;
            PlayerSettings.companyName = this.applicationSettings.CurrentBuildSettings.CompanyName;
            PlayerSettings.productName = this.applicationSettings.CurrentBuildSettings.ProductName;

#if UNITY_ANDROID
        PlayerSettings.Android.keystoreName = this.applicationSettings.CurrentBuildSettings.KeyStoreName;
        PlayerSettings.Android.keystorePass = this.applicationSettings.CurrentBuildSettings.KeyStorePass;
        PlayerSettings.Android.keyaliasName = this.applicationSettings.CurrentBuildSettings.KeyAliasName;
        PlayerSettings.Android.keyaliasPass = this.applicationSettings.CurrentBuildSettings.KeyAliasPass;
        PlayerSettings.Android.bundleVersionCode = this.applicationSettings.CurrentBuildSettings.BundleVersion;

        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);

        if (this.applicationSettings.CurrentBuildSettings.AppBundle)
        {
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
        }
        else
        {
            AndroidArchitecture androidArchitecture = AndroidArchitecture.None;
            if (this.applicationSettings.CurrentBuildSettings.ArchitectureARMv7)
            {
                androidArchitecture |= AndroidArchitecture.ARMv7;
            }

            if (this.applicationSettings.CurrentBuildSettings.ArchitectureARM64)
            {
                androidArchitecture |= AndroidArchitecture.ARM64;
            }

            if (this.applicationSettings.CurrentBuildSettings.ArchitectureX86)
            {
                androidArchitecture |= AndroidArchitecture.X86;
            }

            PlayerSettings.Android.targetArchitectures = androidArchitecture;
        }

        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, this.applicationSettings.CurrentBuildSettings.PackageName);
        EditorUserBuildSettings.buildAppBundle = this.applicationSettings.CurrentBuildSettings.AppBundle;
#endif

#if UNITY_IOS
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
			PlayerSettings.SetApplicationIdentifier( BuildTargetGroup.iOS, this.applicationSettings.CurrentBuildSettings.PackageName );

            switch( this.applicationSettings.CurrentBuildSettings.TargetService )
            {
                case BuildSettings.Service.LIVE:
                {
                    PlayerSettings.iOS.buildNumber = this.applicationSettings.CurrentBuildSettings.LiveBuildNumber;
                }
                break;

                //case BuildSettings.Service.STAGING:
                //{
                //    PlayerSettings.iOS.buildNumber = this.applicationSettings.CurrentBuildSettings.StagingBundleNumber;
                //}
                //break;

                case BuildSettings.Service.DEVELOPER:
                {
                    PlayerSettings.iOS.buildNumber = this.applicationSettings.CurrentBuildSettings.DeveloperBuildNumber;
                }
                break;
            }
#endif

            BuildTargetGroup buildTargetGroup = BuildTargetGroup.Unknown;
#if UNITY_ANDROID
        buildTargetGroup = BuildTargetGroup.Android;
#elif UNITY_IOS
			buildTargetGroup = BuildTargetGroup.iOS;
#endif

            if (buildTargetGroup != BuildTargetGroup.Unknown && this.applicationSettings.CurrentBuildSettings.Icon != null)
            {
                int[] iconSizes = PlayerSettings.GetIconSizesForTargetGroup(buildTargetGroup, IconKind.Application);

                Texture2D[] newIcons = new Texture2D[iconSizes.Length];
                for (int i = 0; i < iconSizes.Length; ++i)
                    newIcons[i] = this.applicationSettings.CurrentBuildSettings.Icon;

                PlayerSettings.SetIconsForTargetGroup(buildTargetGroup, newIcons, IconKind.Application);
            }
        }

#if UNITY_IOS
        private void ExportXcode( BuildOptions buildOptions = BuildOptions.None )
        {
            var path = Application.dataPath;
            string targetFolder = "apple/desktop/build/IOSBuild";// Application.dataPath.Substring( 0, Application.dataPath.LastIndexOf( "Assets", StringComparison.OrdinalIgnoreCase ) );

            string fullPath = EditorUtility.SaveFolderPanel( "Export", targetFolder, "" );

            if( !string.IsNullOrEmpty( fullPath ) )
            {
                this.Apply();

                EditorApplication.delayCall += () =>
                {
                    UnityEditor.Build.Reporting.BuildReport buildReport =
 BuildPipeline.BuildPlayer( EditorBuildSettings.scenes, fullPath, BuildTarget.iOS, buildOptions );
                    ICLogger.LogWarning( buildReport.summary.result );
                    if( buildReport.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded )
                    {
                        if( EditorUtility.DisplayDialog( "빌드 성공", "빌드를 완료하였습니다.\n확인을 누르면 빌드된 폴더로 이동합니다.", "확인", "취소" ) )
                        {
                            ExecuteProcessTerminal();
                            EditorUtility.RevealInFinder( fullPath );
                        }
                    }
                };
            }
        }
#endif

        public string getDayOfWeek(DayOfWeek dow)
        {
            return dow == DayOfWeek.Monday ? "월" : dow == DayOfWeek.Tuesday ? "화" : dow == DayOfWeek.Wednesday ? "수" : dow == DayOfWeek.Thursday ? "목" :
                dow == DayOfWeek.Friday ? "금" : dow == DayOfWeek.Saturday ? "토" : "일";
        }

        private string ExecuteProcessTerminal()
        {
            try
            {
                Ironcow.ICLogger.Log("============== Start Command ===============");
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = "IOSBuildTool/pods.command",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                };
                System.Diagnostics.Process myProcess = new System.Diagnostics.Process
                {
                    StartInfo = startInfo
                };
                myProcess.Start();
                string output = myProcess.StandardOutput.ReadToEnd();
                Ironcow.ICLogger.Log(output);
                myProcess.WaitForExit();
                Ironcow.ICLogger.Log("============== End ===============");

                return output;
            }
            catch (Exception e)
            {
                Ironcow.ICLogger.Log(e);
                return null;
            }
        }

        public async Task UploadToFTP(string path)
        {
            Debug.Log("Upload Start");
            var fileName = string.Format("bbangya{0}.{1}", applicationSettings.CurrentBuildSettings.TargetService == BuildSettings.Service.DEVELOPER ? "_test" :
                applicationSettings.CurrentBuildSettings.TargetService == BuildSettings.Service.STAGING ? "_staging" : "",
                applicationSettings.CurrentBuildSettings.TargetService == BuildSettings.Service.APP_BUNDLE ? "aab" : "apk");

            var target = File.ReadAllBytes(path);

            var ftpUrl = new Uri("ftp://wocjf84.synology.me/web/bbangya/resource/");
            var request = WebRequest.Create(ftpUrl + fileName) as FtpWebRequest;

            request.Credentials = new NetworkCredential("ftp_uploader", "ftpUploader12");
            request.KeepAlive = false;
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.UseBinary = true;
            request.ContentLength = target.Length;

            using (var ftpStream = request.GetRequestStream())
            {
                await ftpStream.WriteAsync(target, 0, target.Length);
                ftpStream.Close();
            }
            try
            {
                // 이제 디렉토리를 만든다고 FTP로 요청을 합니다.
                FtpWebResponse res = (FtpWebResponse)await request.GetResponseAsync();
                res.Dispose();
            }
            catch (WebException ex)
            {
                // 예외처리.
                FtpWebResponse response = (FtpWebResponse)ex.Response;

                switch (response.StatusCode)
                {
                    case FtpStatusCode.ActionNotTakenFileUnavailable:
                        {
                            Ironcow.ICLogger.Log("CreateFolders ] Probably the folder already exist : ");
                        }
                        break;
                }
                response.Dispose();
            }
            //await SendMessageAsync(fileName + " 업로드 완료 다운로드 링크 : http://wocjf84.synology.me/ds_apk/");
            this.actionUpload = false;
            Debug.Log("Upload End");
        }

        const string WebhookUrl = "https://hooks.slack.com/services/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";
        public async System.Threading.Tasks.Task<bool> SendMessageAsync(string message, string slackUrl = WebhookUrl)
        {
            if (!actionUpload) return false;
            string paramJson = "{\"text\":\"" + message + "\"}";

            // Payload
            var content = new System.Net.Http.FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "payload", paramJson }
            });

            // POST!!
            using System.Net.Http.HttpClient _httpClient = new System.Net.Http.HttpClient();

            System.Net.Http.HttpResponseMessage res = await _httpClient.PostAsync(slackUrl, content);

            var result = res.StatusCode == System.Net.HttpStatusCode.OK;
            if (!result) Debug.Log(res.RequestMessage);
            return (res.StatusCode == System.Net.HttpStatusCode.OK);
        }

    }
}