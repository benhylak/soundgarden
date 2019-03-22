#if UNITY_IOS

using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Bose.Wearable.Editor
{
	public class XcodePreBuildProcessor
		#if UNITY_2018_1_OR_NEWER
		: IPreprocessBuildWithReport
		#else
        : IPreprocessBuild
        #endif
	{
		/// <summary>
		/// The architecture of the build.
		/// </summary>
		private enum Architecture
		{
			ARMv7,
			ARM64,
			Universal
		}

		public int callbackOrder
		{
			get { return WearableConstants.XcodePreBuildProcessorOrder; }
		}
		
		#if UNITY_2018_1_OR_NEWER
		public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
		{
			Process();
		}
		#else
		public void OnPreprocessBuild(BuildTarget target, string path)
		{
			Process();
		}
        #endif

		private void Process()
		{
			// Check the architecture and if not Arm64 log an error to the console.
			var arch = (Architecture)PlayerSettings.GetArchitecture(BuildTargetGroup.iOS);
			if (arch != Architecture.ARM64)
			{
				// Set the Project to ARM64 for iOS.
				PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, (int)Architecture.ARM64);

				Debug.LogWarningFormat(WearableConstants.ArchitectureAlterationWarningWithMessage, arch);
			}

			// Make sure the target iOS version is at or above the minimum.
			float targetOSVersion;
			if (float.TryParse(PlayerSettings.iOS.targetOSVersionString, out targetOSVersion))
			{
				if (targetOSVersion < WearableConstants.MinimumSupportediOSVersion)
				{
					var minimumVersion = WearableConstants.MinimumSupportediOSVersion.ToString("0.0");
					PlayerSettings.iOS.targetOSVersionString = minimumVersion;

					var msg = string.Format(
						WearableConstants.iOSVersionAlterationWarningWithMessage,
						minimumVersion,
						targetOSVersion.ToString("0.0")
					);
					Debug.LogWarning(msg);
				}
			}

			// Make sure that the app is set to use BLE accessories.
			var backgroundBehavior = PlayerSettings.iOS.appInBackgroundBehavior;
			var backgroundModes = PlayerSettings.iOS.backgroundModes;
			if (backgroundBehavior != iOSAppInBackgroundBehavior.Custom ||
				(backgroundModes & iOSBackgroundMode.BluetoothCentral) == 0)
			{
				PlayerSettings.iOS.appInBackgroundBehavior = iOSAppInBackgroundBehavior.Custom;
				PlayerSettings.iOS.backgroundModes |= iOSBackgroundMode.BluetoothCentral;

				Debug.LogWarning(WearableConstants.iOSBluetoothAlterationWarning);
			}
		}
	}
}

#endif
