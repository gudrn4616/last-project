using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ironcow
{
    public class BuildSettings : ScriptableObject
    {
#if UNITY_EDITOR
        static public BuildSettings Create()
        {
            BuildSettings instance = CreateInstance<BuildSettings>();

#if UNITY_EDITOR
            string directory = System.IO.Path.Combine(Application.dataPath, "Settings/Resources");
            if (!System.IO.Directory.Exists(directory))
                AssetDatabase.CreateFolder("Assets/Settings", "Resources");

            string assetPath = AssetDatabase.GenerateUniqueAssetPath(string.Format("Assets/Settings/Resources/New{0}.asset", typeof(BuildSettings).ToString()));
            AssetDatabase.CreateAsset(instance, assetPath);
#endif

            return instance;
        }
#endif

        public enum Platform
        {
            Android,
            iOS,
        }

        public enum Service
        {
            LIVE = 0,
            STAGING,
            DEVELOPER,
            APP_BUNDLE
        }

        [HideInInspector]
        public Platform TargetPlatform = Platform.Android;

        [HideInInspector]
        public Service TargetService = Service.DEVELOPER;

        [HideInInspector]
        public string LiveVersion = "0.4";

        [HideInInspector]
        public string StagingVersion = "0.6";

        [HideInInspector]
        public string DeveloperVersion = "0.4";

        [HideInInspector]
        public string Name = "Android";

        [HideInInspector]
        public string CompanyName = "Ironcow";

        [HideInInspector]
        public string ProductName = "projectM";

        [HideInInspector]
        public string PackageName = "com.ironcow.projectm";

        [HideInInspector]
        public Texture2D Icon;

#if UNITY_ANDROID
        [HideInInspector]
        public int LiveBundleVersionCode = 4;

        [HideInInspector]
        public int StagingBundleVersionCode = 6;

        [HideInInspector]
        public int DeveloperBundleVersionCode = 4;

        [HideInInspector]
        public string KeyStoreName = string.Empty;

        [HideInInspector]
        public string KeyStorePass = string.Empty;

        [HideInInspector]
        public string KeyAliasName = string.Empty;

        [HideInInspector]
        public string KeyAliasPass = string.Empty;

        [HideInInspector]
        public bool AppBundle = false;

        [HideInInspector]
        public bool ArchitectureARMv7 = true;

        [HideInInspector]
        public bool ArchitectureARM64 = true;

        [HideInInspector]
        public bool ArchitectureX86 = false;
#endif

#if UNITY_IOS
    [HideInInspector]
    public string LiveBuildNumber = "1.0.00";

    [HideInInspector]
    public string StagingBundleNumber = "1.0.00";

    [HideInInspector]
    public string DeveloperBuildNumber = "1.0.00";
#endif

        public string Version
        {
            get
            {
                string version = string.Empty;

                switch (this.TargetService)
                {
                    case Service.LIVE: version = this.LiveVersion; break;
                    case Service.APP_BUNDLE: version = this.LiveVersion; break;
                    case Service.DEVELOPER: version = this.DeveloperVersion; break;
                }

                return version;
            }
            set
            {
                switch (this.TargetService)
                {
                    case Service.LIVE: this.LiveVersion = value; break;
                    case Service.APP_BUNDLE: this.LiveVersion = value; break;
                    case Service.DEVELOPER: this.DeveloperVersion = value; break;
                }
            }
        }

#if UNITY_ANDROID
        public int BundleVersion
        {
            get
            {
                int bundleVersion = 0;
                switch (this.TargetService)
                {
                    case Service.LIVE: bundleVersion = this.LiveBundleVersionCode; break;
                    case Service.APP_BUNDLE: bundleVersion = this.LiveBundleVersionCode; break;
                    case Service.DEVELOPER: bundleVersion = this.DeveloperBundleVersionCode; break;
                }

                return bundleVersion;
            }
            set
            {
                switch (this.TargetService)
                {
                    case Service.LIVE: this.LiveBundleVersionCode = value; break;
                    case Service.APP_BUNDLE: this.LiveBundleVersionCode = value; break;
                    case Service.DEVELOPER: this.DeveloperBundleVersionCode = value; break;
                }
            }
        }
#endif
    }
}