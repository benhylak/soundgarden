#if UNITY_ANDROID

using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Bose.Wearable.Editor
{
	public class AndroidPreBuildProcessor
		#if UNITY_2018_1_OR_NEWER
		: IPreprocessBuildWithReport
		#else
        : IPreprocessBuild
        #endif
	{
		public int callbackOrder
		{
			get { return WearableConstants.AndroidPreBuildProcessorOrder; }
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
			// Make sure the target Android version is at or above the minimum.
			int minSdkVersion = (int)PlayerSettings.Android.minSdkVersion;
			if (minSdkVersion < WearableConstants.MinimumSupportedAndroidVersion)
			{
				PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)WearableConstants.MinimumSupportedAndroidVersion;

				var msg = string.Format(
					WearableConstants.AndroidVersionAlterationWarningWithMessage,
					WearableConstants.MinimumSupportedAndroidVersion,
					minSdkVersion
				);
				Debug.LogWarning(msg);
			}
		}
	}
}

#endif
