using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif

namespace Bose.Wearable.Editor
{
	public static class BuildTools
	{
		// Shared
		private const string AppVersion = "1.0";

		// Wearable Demo
		private const string WearableDemoProductName = "Wearable Demo";
		private const string WearableDemoAppIdentifier = "com.bose.demo.wearableunity";
		private const string WearableDemoIconGuid = "e06b243adbc49564b8ba586a2a0ed2d0";

		private const string RootSceneGuid = "b100476eb79061246a7d53542a204e54";
		private const string MainMenuSceneGuid = "814d265ed5a714b2f8a496b0e00010e1";
		private const string BasicDemoSceneGuid = "e822a72393d35429f941bfee942e76f4";
		private const string AdvancedDemoSceneGuid = "422b6c809820b4a78b2c60a058c8a7b4";
		private const string GestureDemoSceneGuid = "6cb706a67df9fd948a79d1d93f05bef2";

		private static readonly string[] WearableDemoSceneGuids =
		{
			RootSceneGuid,
			MainMenuSceneGuid,
			BasicDemoSceneGuid,
			AdvancedDemoSceneGuid,
			GestureDemoSceneGuid
		};

		// Proxy Server
		private const string ProxyServerProductName = "Proxy Server";
		private const string ProxyServerAppIdentifier = "com.bose.demo.wearableproxyserver";
		private const string ProxyAppIconGuid = "f8f31fc86e17d1b489c1e409610b715a";

		private const string ProxyServerSceneGuid = "be6d1a5a9c2994033954c8265229c4e8";

		// Build
		private const string CannotBuildErrorMessage = "[Bose Wearable] Cannot build the {0} for {1} as component " +
		                                               "support for that platform is not installed. Please " +
		                                               "install this component to continue, stopping build...";
		private const string CannotBuildMissingSceneErrorMessage = "[Bose Wearable] Could not find a scene for " +
		                                                           "the {0}, stopping build";
		private const string CannotFindAppIcon = "[Bose Wearable] Could not find the application icon for the Bose Wearable " +
		                                         "example content.";
		private const string BuildScenesCouldNotBeFound = "[Bose Wearable] Scenes could not be found for {0}, " +
		                                                  "stopping build..";
		private const string BuildSucceededMessage = "[Bose Wearable] {0} Build Succeeded!";
		private const string BuildFailedMessage = "[Bose Wearable] {0} Build Failed! {1}";

		/// <summary>
		/// An editor-pref key for where the user last selected the build location.
		/// </summary>
		private const string BuildLocationPreferenceKey = "bose_wearable_pref_key";

		// Folder Picker
		private const string FolderPickerTitle = "Build Location for {0}";

		// Unity Cloud Build
		private const string BuildScenesSetMessage = "[Bose Wearable] Build Scenes Set for {0}.";

		internal static void BuildProxyServer()
		{
			// Check for player support
			if (!CanBuildTarget(EditorUserBuildSettings.activeBuildTarget))
			{
				Debug.LogErrorFormat(CannotBuildErrorMessage, ProxyServerProductName, EditorUserBuildSettings.activeBuildTarget);
				return;
			}

			var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

			// Get folder path from the user for the build
			var folderPath = GetBuildLocation(ProxyServerProductName);
			if (string.IsNullOrEmpty(folderPath))
			{
				return;
			}

			// Cache values for the current Player Settings
			var originalProductName = PlayerSettings.productName;
			var bundleVersion = PlayerSettings.bundleVersion;
			var appId = PlayerSettings.GetApplicationIdentifier(buildTargetGroup);
			var iconGroup = PlayerSettings.GetIconsForTargetGroup(buildTargetGroup);

			// Override Player Settings for this build.
			EditorBuildSettingsScene[] buildScenes;
			if (SetBuildSettingsForWearableProxy(out buildScenes))
			{
				var sceneAssetPaths = buildScenes.Where(x => x.enabled).Select(x => x.path).ToArray();

				// Attempt to build the app
				var buildPlayerOptions = new BuildPlayerOptions
				{
					scenes = sceneAssetPaths,
					locationPathName = folderPath,
					target = EditorUserBuildSettings.activeBuildTarget
				};

				var buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);

				#if UNITY_2018_1_OR_NEWER
				if (buildReport.summary.result == BuildResult.Succeeded)
				#else
				if (string.IsNullOrEmpty(buildReport))
				#endif
				{
					Debug.LogFormat(BuildSucceededMessage, ProxyServerProductName);
				}
				else
				{
					Debug.LogFormat(BuildFailedMessage, ProxyServerProductName, buildReport);
				}
			}

			// Reset all PlayerSetting changes back to their original values.
			PlayerSettings.productName = originalProductName;
			PlayerSettings.bundleVersion = bundleVersion;
			PlayerSettings.SetApplicationIdentifier(buildTargetGroup, appId);
			PlayerSettings.SetIconsForTargetGroup(buildTargetGroup, iconGroup);
			AssetDatabase.SaveAssets();
		}

		/// <summary>
		/// Ensure all settings for the Wearable Proxy build are in place.
		/// (Parameterless version for headless build systems.)
		/// </summary>
		public static void SetBuildSettingsForWearableProxy()
		{
			EditorBuildSettingsScene[] buildScenes;
			SetBuildSettingsForWearableProxy(out buildScenes);

			#if UNITY_CLOUD_BUILD
			// Only in Unity Cloud Build do we want to override the native scene list
			EditorBuildSettings.scenes = buildScenes;
			Debug.LogFormat(BuildScenesSetMessage, ProxyServerProductName);
			#endif
		}

		/// <summary>
		/// Ensure all settings for the Wearable Proxy build are in place.
		/// </summary>
		private static bool SetBuildSettingsForWearableProxy(out EditorBuildSettingsScene[] buildScenes)
		{
			buildScenes = new[]
			{
				new EditorBuildSettingsScene(AssetDatabase.GUIDToAssetPath(ProxyServerSceneGuid), true)
			};

			var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

			PlayerSettings.productName = ProxyServerProductName;
			PlayerSettings.bundleVersion = AppVersion;
			PlayerSettings.SetApplicationIdentifier(buildTargetGroup, ProxyServerAppIdentifier);
			TrySetAppIcons(ProxyAppIconGuid, buildTargetGroup);
			AssetDatabase.SaveAssets();

			return buildScenes.Length > 0 &&
			       buildScenes.All(x => !string.IsNullOrEmpty(x.path) && x.enabled);
		}

		internal static void BuildWearableDemo()
		{
			// Check for player support
			if (!CanBuildTarget(EditorUserBuildSettings.activeBuildTarget))
			{
				Debug.LogErrorFormat(CannotBuildErrorMessage, WearableDemoProductName, EditorUserBuildSettings.activeBuildTarget);
				return;
			}

			// Get folder path from the user for the build
			var folderPath = GetBuildLocation(WearableDemoProductName);
			if (string.IsNullOrEmpty(folderPath))
			{
				return;
			}

			var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

			// Cache values for the current Player Settings
			var originalProductName = PlayerSettings.productName;
			var bundleVersion = PlayerSettings.bundleVersion;
			var appId = PlayerSettings.GetApplicationIdentifier(buildTargetGroup);
			var iconGroup = PlayerSettings.GetIconsForTargetGroup(buildTargetGroup);

			// Override Player Settings for this build.
			EditorBuildSettingsScene[] buildScenes;
			if (SetBuildSettingsForWearableDemo(out buildScenes))
			{
				var sceneAssetPaths = buildScenes.Where(x => x.enabled).Select(x => x.path).ToArray();

				// Attempt to build the app
				var buildPlayerOptions = new BuildPlayerOptions
				{
					scenes = sceneAssetPaths,
					locationPathName = folderPath,
					target = EditorUserBuildSettings.activeBuildTarget
				};

				var buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
				#if UNITY_2018_1_OR_NEWER
				if (buildReport.summary.result == BuildResult.Succeeded)
				#else
				if (string.IsNullOrEmpty(buildReport))
				#endif
				{
					Debug.LogFormat(BuildSucceededMessage, WearableDemoProductName);
				}
				else
				{
					Debug.LogFormat(BuildFailedMessage, WearableDemoProductName, buildReport);
				}
			}
			else
			{
				Debug.LogErrorFormat(BuildScenesCouldNotBeFound, WearableDemoProductName);
			}

			// Reset all PlayerSetting changes back to their original values.
			PlayerSettings.productName = originalProductName;
			PlayerSettings.bundleVersion = bundleVersion;
			PlayerSettings.SetApplicationIdentifier(buildTargetGroup, appId);
			PlayerSettings.SetIconsForTargetGroup(buildTargetGroup, iconGroup);
			AssetDatabase.SaveAssets();
		}

		/// <summary>
		/// Ensure all settings for the Wearable Proxy build are in place.
		/// (Parameterless version for headless build systems.)
		/// </summary>
		public static void SetBuildSettingsForWearableDemo()
		{
			EditorBuildSettingsScene[] buildScenes;
			SetBuildSettingsForWearableDemo(out buildScenes);

			#if UNITY_CLOUD_BUILD
			// Only in Unity Cloud Build do we want to override the native scene list
			EditorBuildSettings.scenes = buildScenes;
			Debug.LogFormat(BuildScenesSetMessage, WearableDemoProductName);
			#endif
		}

		/// <summary>
		/// Ensure all settings for the Wearable Proxy build are in place.
		/// </summary>
		private static bool SetBuildSettingsForWearableDemo(out EditorBuildSettingsScene[] buildScenes)
		{
			buildScenes = new EditorBuildSettingsScene[WearableDemoSceneGuids.Length];
			for (var i = 0; i < WearableDemoSceneGuids.Length; i++)
			{
				buildScenes[i] = new EditorBuildSettingsScene
				{
					path = AssetDatabase.GUIDToAssetPath(WearableDemoSceneGuids[i]),
					enabled = true
				};

				if (string.IsNullOrEmpty(buildScenes[i].path))
				{
					Debug.LogErrorFormat(CannotBuildMissingSceneErrorMessage, WearableDemoProductName);
				}
			}

			var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

			PlayerSettings.productName = WearableDemoProductName;
			PlayerSettings.bundleVersion = AppVersion;
			PlayerSettings.SetApplicationIdentifier(buildTargetGroup, WearableDemoAppIdentifier);
			TrySetAppIcons(WearableDemoIconGuid, buildTargetGroup);
			AssetDatabase.SaveAssets();

			return buildScenes.Length > 0 &&
			       buildScenes.All(x => !string.IsNullOrEmpty(x.path) && x.enabled);
		}

		/// <summary>
		/// Returns true or false depending on whether the local Unity Editor can build
		/// the desired BuildTarget based on whether or not the player support has been
		/// installed.
		/// </summary>
		/// <param name="buildTarget"></param>
		/// <returns></returns>
		private static bool CanBuildTarget(BuildTarget buildTarget)
		{
			var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);

			#if UNITY_2018_1_OR_NEWER

			return BuildPipeline.IsBuildTargetSupported(buildTargetGroup, buildTarget);

			#else

			try
			{
				// IsBuildTargetSupported is an internal method of BuildPipeline on 2017.4 so we must
				// use reflection in order to access it.
				const string BuildTargetSupportedMethodName = "IsBuildTargetSupported";
				var internalMethod = typeof(BuildPipeline).GetMethod(
					BuildTargetSupportedMethodName,
					BindingFlags.NonPublic | BindingFlags.Static);

				var result = internalMethod.Invoke(null, new object[] { buildTargetGroup, buildTarget });

				return (bool)result;
			}
			// Default to true if we cannot programmatically determine player support in the editor.
			catch (Exception e)
			{
				Debug.LogError(e);
				return true;
			}

			#endif
		}

		/// <summary>
		/// Get a build location from the user via a dialog box. If the path is valid, it will be saved in the
		/// user's preferences for use next time as a suggestion.
		/// </summary>
		/// <returns></returns>
		private static string GetBuildLocation(string productName)
		{
			// Get folder path from the user for the build
			var startFolder = string.Empty;
			if (EditorPrefs.HasKey(BuildLocationPreferenceKey))
			{
				startFolder = EditorPrefs.GetString(BuildLocationPreferenceKey);
			}

			var panelTitle = string.Format(FolderPickerTitle, productName);
			BuildTarget activeTarget = EditorUserBuildSettings.activeBuildTarget;
			string folderPath;
			switch (activeTarget)
			{
				case BuildTarget.Android:
				{
					folderPath = EditorUtility.SaveFilePanel(panelTitle, startFolder, productName, "apk");
					break;
				}

				case BuildTarget.iOS:
				{
					folderPath = EditorUtility.SaveFolderPanel(panelTitle, startFolder, productName);
					break;
				}

				default:
				{
					folderPath = string.Empty;
					Debug.LogWarningFormat(WearableConstants.BuildToolsUnsupportedPlatformWarning, activeTarget);
					break;
				}
			}

			if (!string.IsNullOrEmpty(folderPath))
			{
				var directory = new DirectoryInfo(folderPath);
				if (directory.Parent != null)
				{
					var parentDirectory = directory.Parent;
					EditorPrefs.SetString(BuildLocationPreferenceKey, parentDirectory.FullName);
				}
			}

			return folderPath;
		}

		/// <summary>
		/// Attempt to use an <see cref="Texture2D"/> identified by <see cref="string"/> <paramref name="iconGuid"/> to
		/// override the App Icon settings for <see cref="BuildTargetGroup"/> <paramref name="buildTargetGroup"/>.
		/// </summary>
		/// <param name="iconGuid"></param>
		/// <param name="buildTargetGroup"></param>
		private static void TrySetAppIcons(string iconGuid, BuildTargetGroup buildTargetGroup)
		{
			var iconPath = AssetDatabase.GUIDToAssetPath(iconGuid);
			if (string.IsNullOrEmpty(iconPath))
			{
				Debug.LogWarning(CannotFindAppIcon);
			}
			else
			{
				var iconSizes = PlayerSettings.GetIconSizesForTargetGroup(buildTargetGroup);
				var iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
				var newIconGroup = new Texture2D[iconSizes.Length];
				for (var i = 0; i < newIconGroup.Length; i++)
				{
					newIconGroup[i] = iconTexture;
				}

				PlayerSettings.SetIconsForTargetGroup(buildTargetGroup, newIconGroup);
			}
		}
	}
}
