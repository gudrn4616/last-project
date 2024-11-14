using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Ironcow
{
    [CustomEditor( typeof( BuildSettings ) )]
    public class BuildSettingsEditor : UnityEditor.Editor
    {
        private BuildSettings buildSettings = null;
        private int selected = 0;
        private List<string> serviceTabTexts;

        private void OnEnable()
        {
            this.buildSettings = base.target as BuildSettings;
            this.selected = (int)this.buildSettings.TargetService;

            this.serviceTabTexts = new List<string>();
            this.serviceTabTexts.Add( "Live" );
            this.serviceTabTexts.Add( "Staging" );
            this.serviceTabTexts.Add( "Developer" );
            this.serviceTabTexts.Add("AppBundle");
        }

        public override void OnInspectorGUI()
        {
            if( this.buildSettings == null )
                return;

            EditorGUI.BeginChangeCheck();

            GUILayout.Space( 10f );
            this.buildSettings.Name = EditorGUILayout.TextField( "빌드 환경 이름", this.buildSettings.Name );
            this.buildSettings.TargetPlatform = (BuildSettings.Platform)EditorGUILayout.EnumPopup( "Target Platform", this.buildSettings.TargetPlatform );
            this.buildSettings.TargetService = (BuildSettings.Service)EditorGUILayout.EnumPopup( "Target Service", this.buildSettings.TargetService );

            GUILayout.Space( 10f );
            if( GUI.DrawToolbar( ref this.selected, this.serviceTabTexts.ToArray() ) )
            {
                this.buildSettings.TargetService = (BuildSettings.Service)this.selected;
                EditorUtility.SetDirty( this.buildSettings );
            }

            EditorGUI.indentLevel++;
            {
                switch( this.buildSettings.TargetService )
                {
                    case BuildSettings.Service.LIVE:
                        {
                            this.buildSettings.LiveVersion = EditorGUILayout.TextField( "Version", this.buildSettings.LiveVersion );
#if UNITY_ANDROID
                            this.buildSettings.LiveBundleVersionCode = EditorGUILayout.IntField( "Bundle Version", this.buildSettings.LiveBundleVersionCode);
                            this.buildSettings.AppBundle = EditorGUILayout.Toggle( "App Bundle", false );
#elif UNITY_IOS
                            this.buildSettings.LiveBuildNumber = EditorGUILayout.TextField( "Build", this.buildSettings.LiveVersion );
#endif
                        }
                        break;

                    case BuildSettings.Service.APP_BUNDLE:
                        {
                            this.buildSettings.LiveVersion = EditorGUILayout.TextField("Version", this.buildSettings.LiveVersion);
#if UNITY_ANDROID
                            this.buildSettings.LiveBundleVersionCode = EditorGUILayout.IntField("Bundle Version", this.buildSettings.LiveBundleVersionCode);
                            this.buildSettings.AppBundle = EditorGUILayout.Toggle("App Bundle", true);
#elif UNITY_IOS
                            this.buildSettings.DeveloperBuildNumber = EditorGUILayout.TextField( "Build", this.buildSettings.DeveloperVersion );
#endif
                        }
                        break;
                    case BuildSettings.Service.DEVELOPER:
                        {
                            this.buildSettings.DeveloperVersion = EditorGUILayout.TextField("Version", this.buildSettings.DeveloperVersion);
#if UNITY_ANDROID
                            this.buildSettings.DeveloperBundleVersionCode = EditorGUILayout.IntField("Bundle Version", this.buildSettings.DeveloperBundleVersionCode);
                            this.buildSettings.AppBundle = EditorGUILayout.Toggle( "App Bundle", false );
#elif UNITY_IOS
                            this.buildSettings.DeveloperBuildNumber = EditorGUILayout.TextField( "Build", this.buildSettings.DeveloperVersion );
#endif
                        }
                        break;
                    case BuildSettings.Service.STAGING:
                        {
                            this.buildSettings.StagingVersion = EditorGUILayout.TextField("Version", this.buildSettings.StagingVersion);
#if UNITY_ANDROID
                            this.buildSettings.StagingBundleVersionCode = EditorGUILayout.IntField("Bundle Version", this.buildSettings.StagingBundleVersionCode);
                            this.buildSettings.AppBundle = EditorGUILayout.Toggle( "App Bundle", false );
#elif UNITY_IOS
                            this.buildSettings.StagingBuildNumber = EditorGUILayout.TextField( "Build", this.buildSettings.StagingVersion );
#endif
                        }
                        break;
                }
            }
            EditorGUI.indentLevel--;
            GUILayout.Space( 10f );

            this.buildSettings.CompanyName = EditorGUILayout.TextField( "Company Name", this.buildSettings.CompanyName );
            this.buildSettings.ProductName = EditorGUILayout.TextField( "Product Name", this.buildSettings.ProductName );
            this.buildSettings.PackageName = EditorGUILayout.TextField( "Package Name", this.buildSettings.PackageName );
            this.buildSettings.Icon = EditorGUILayout.ObjectField( "Application Icon", this.buildSettings.Icon, typeof( Texture2D ), false ) as Texture2D;

            if( this.buildSettings.TargetPlatform == BuildSettings.Platform.Android && EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android )
            {
#if UNITY_ANDROID
				if( GUILayout.Button( "Browse Keystore" ) )
				{
					string defaultValue = Application.dataPath.Substring( 0, Application.dataPath.LastIndexOf( "Assets" ) );
					string latestKeyStoreName = EditorPrefs.GetString( "Latest KeyStore Name", defaultValue );
					
					string path = EditorUtility.OpenFilePanel( "Open existing keystore...", latestKeyStoreName, "keystore" );
					if( !string.IsNullOrEmpty( path ) )
					{
						this.buildSettings.KeyStoreName = path;

						EditorPrefs.SetString( "Latest KeyStore Name", path );
					}
				}

				GUILayout.Space( 10f );

				EditorGUILayout.LabelField( "Keystore", EditorStyles.boldLabel );
				EditorGUI.indentLevel++;
				{
					this.buildSettings.KeyStoreName = EditorGUILayout.TextField( "Key Store Name", this.buildSettings.KeyStoreName );
					this.buildSettings.KeyStorePass = EditorGUILayout.TextField( "Key Store Pass", this.buildSettings.KeyStorePass );
					this.buildSettings.KeyAliasName = EditorGUILayout.TextField( "Key Alias Name", this.buildSettings.KeyAliasName );
					this.buildSettings.KeyAliasPass = EditorGUILayout.TextField( "Key Alias Pass", this.buildSettings.KeyAliasPass );
				}
				EditorGUI.indentLevel--;
#endif
			}

			GUILayout.Space( 10f );

			if( this.buildSettings.TargetPlatform == BuildSettings.Platform.Android && EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android )
			{
				if( GUILayout.Button( "Apply Keystore" ) )
				{
#if UNITY_ANDROID
					PlayerSettings.Android.keystoreName = this.buildSettings.KeyStoreName;
					PlayerSettings.Android.keystorePass = this.buildSettings.KeyStorePass;
					PlayerSettings.Android.keyaliasName = this.buildSettings.KeyAliasName;
					PlayerSettings.Android.keyaliasPass = this.buildSettings.KeyAliasPass;
#endif
				}
			}

			GUILayout.Space( 10f );

            if( GUILayout.Button( "Player Settings..." ) )
            {
#if UNITY_2018_3_OR_NEWER
                SettingsService.OpenProjectSettings( "Project/Player" );
#else
                    EditorApplication.ExecuteMenuItem( "Edit/Project Settings/Player" );
#endif
            }

            if( EditorGUI.EndChangeCheck() )
				EditorUtility.SetDirty( this.buildSettings );
		}
	}
}