﻿#pragma warning disable 0414

using Fresvii.AppSteroid;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Fresvii.AppSteroid.Gui
{
    [CustomEditor(typeof(FASSettings))]
    public class FresviiSettingsEditor : Editor
    {
        GUIContent appIconLabel = new GUIContent("App Icon");

        GUIContent appIdLabel = new GUIContent("App Id (required)");

        GUIContent secretKeyLabel = new GUIContent("Secret key (required)");

        GUIContent environmentLabel = new GUIContent("Server Environment");

        GUIContent enableLabel = new GUIContent("Enable");

        GUIContent pushLabel = new GUIContent("Push notification");

        GUIContent iOSLabel = new GUIContent("iOS Settings");

		GUIContent apnsEnvironmentLabel = new GUIContent("APNS Certificate");

		GUIContent androidLabel = new GUIContent("Android Settings");

        GUIContent androidGCMPjNumLabel = new GUIContent("GCM Project Number");

        GUIContent androidGCMApiKeyLabel = new GUIContent("GCM Api key");

        GUIContent androidBackUpApiKeyLabel = new GUIContent("Backup Api key");
        
        GUIContent requestPushRequestOnStartLabel = new GUIContent("Register push request on start");

        GUIContent androidVibrateLabel = new GUIContent("Vibrate on push notification");

        GUIContent androidGetAccountsLabel = new GUIContent("GET_ACCOUNTS permission");

        GUIContent reloginOnResumeLabel = new GUIContent("Auto relogin silently on resume");

        GUIContent videoFeatureEnableLabel = new GUIContent("Video feature");

        GUIContent orientationLabel = new GUIContent("Orientation");

        GUIContent appSteroidGuiOrientationLabel = new GUIContent("AppSetroid GUI Orientation");

        GUIContent allowedOrientationEnableLabel = new GUIContent("Allowed Orientation for Auto Rotation");

        private static FASSettings instance;
        
        private static FresviiSettingsEditor window = null;

        private bool showPushSettings = true;

#if UNITY_5
        private bool showIOSSettings = (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS);
#else
        private bool showIOSSettings = (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iPhone);
#endif

        private bool showAndroidSettings = (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android);

        private bool showGUISettings;

        [MenuItem("Fresvii/FAS Settings")]
        public static void Edit()
        {
            Selection.activeObject = FASSettings.Settings;

            Debug.Log("Fresvii/FAS Settings");
        }

        public override void OnInspectorGUI()
        {
            instance = (FASSettings)target;

            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("AppSteroid Settings", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Version " + FAS.Version);

            EditorGUILayout.Space();

            #region app settings

            instance.appId = EditorGUILayout.TextField(appIdLabel, instance.appId);

            instance.secretKey = EditorGUILayout.TextField(secretKeyLabel, instance.secretKey);

            instance.environment = (FAS.ProvisioningEnvironment)EditorGUILayout.EnumPopup("Server Environment", instance.environment);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("AppSteroid GUI settings", EditorStyles.boldLabel);

            instance.guiType = (FAS.GuiType)EditorGUILayout.EnumPopup("Gui type", instance.guiType);

            instance.appIcon = (Texture2D)EditorGUILayout.ObjectField(appIconLabel, instance.appIcon, typeof(Texture2D), false);

            instance.lightFont = (Font)EditorGUILayout.ObjectField("Light font", instance.lightFont, typeof(Font), false);

            instance.regularFont = (Font)EditorGUILayout.ObjectField("Regular font", instance.regularFont, typeof(Font), false);

            instance.semiboldFont = (Font)EditorGUILayout.ObjectField("Semibold font", instance.semiboldFont, typeof(Font), false);

            instance.boldFont = (Font)EditorGUILayout.ObjectField("Bold font", instance.boldFont, typeof(Font), false);

            instance.extraboldFont = (Font)EditorGUILayout.ObjectField("Extrabold font", instance.extraboldFont, typeof(Font), false);

            EditorGUILayout.LabelField("AppSteroid GUI orientation", EditorStyles.boldLabel);

            instance.orientation = (ScreenOrientation)EditorGUILayout.EnumPopup("Orientation", instance.orientation);

            if (instance.orientation == ScreenOrientation.AutoRotation)
            {
                EditorGUILayout.LabelField("  Allowed orientation", EditorStyles.boldLabel);
                instance.portrait = EditorGUILayout.Toggle("   Portrait", instance.portrait);
                instance.portraitUpsideDown = EditorGUILayout.Toggle("   Portrait Upside Down", instance.portraitUpsideDown);
                instance.landscapeLeft = EditorGUILayout.Toggle("   Landscape Left", instance.landscapeLeft);
                instance.landscapeRight = EditorGUILayout.Toggle("   Landscape Right", instance.landscapeRight);
            }

            #endregion

            EditorGUILayout.Space();

            #region Util

            instance.officialChat = EditorGUILayout.Toggle("CSR", instance.officialChat);

            instance.logLevel = (FAS.LogLevels) EditorGUILayout.EnumPopup("Log level", instance.logLevel);

            instance.reloginOnResume = EditorGUILayout.Toggle(reloginOnResumeLabel, instance.reloginOnResume);

            #endregion

            EditorGUILayout.Space();

            #region push settings
            showPushSettings = EditorGUILayout.Foldout(showPushSettings, pushLabel);

            if (showPushSettings)
            {
                instance.pushNotification = EditorGUILayout.Toggle(enableLabel, instance.pushNotification);

                instance.registerPushRequestOnStart = EditorGUILayout.Toggle(requestPushRequestOnStartLabel, instance.registerPushRequestOnStart);
            }

            #endregion

            EditorGUILayout.Space();

			showIOSSettings = EditorGUILayout.Foldout(showIOSSettings, iOSLabel);

			if (showIOSSettings)
			{
                instance.videoEnable = EditorGUILayout.Toggle(videoFeatureEnableLabel, instance.videoEnable);
			}

			EditorGUILayout.Space();

			#region Android Settings
            showAndroidSettings = EditorGUILayout.Foldout(showAndroidSettings, androidLabel);

            if (showAndroidSettings)
            {
                instance.gcmProjectNumber = EditorGUILayout.TextField(androidGCMPjNumLabel, instance.gcmProjectNumber);

                instance.gcmApiKey = EditorGUILayout.TextField(androidGCMApiKeyLabel, instance.gcmApiKey);

                instance.androidNotificationSmallIconBackground = EditorGUILayout.ColorField("Notification small icon background", instance.androidNotificationSmallIconBackground);

                instance.backUpApiKey = EditorGUILayout.TextField(androidBackUpApiKeyLabel, instance.backUpApiKey);

                instance.pushVibration = EditorGUILayout.Toggle(androidVibrateLabel, instance.pushVibration);

                instance.underAndroidApiLevel15 = EditorGUILayout.Toggle(androidGetAccountsLabel, instance.underAndroidApiLevel15);
                EditorGUILayout.LabelField(" (necessary only if if the device is running a version lower than Android 4.0.4)");

                if (GUILayout.Button("Generate Android.manifest"))
                {
                    FresviiAndroidManifestGenerator.GenerateManifest();
                }
            }
            #endregion

            EditorGUILayout.Space();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(instance);
            }
        }

    }
}