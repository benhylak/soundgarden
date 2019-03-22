#if UNITY_IOS

using System.IO;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
using UnityEngine;

namespace Bose.Wearable.Editor
{
    /// <summary>
    /// XcodeBuildProcessor links all of the necessary binaries and frameworks, sets search paths, and otherwise helps to
    /// automate setting up the Unity-generated Xcode project to be able to build to device without additional customization.
    /// </summary>
    public class XcodePostBuildProcessor 
        #if UNITY_2018_1_OR_NEWER
        : IPostprocessBuildWithReport
        #else
        : IPostprocessBuild
        #endif
    {
		public int callbackOrder
        {
            get { return WearableConstants.XcodePostBuildProcessorOrder; }
        }

        private PBXProject _project;
        private string _appGuid;
        
        #if UNITY_2018_1_OR_NEWER
        public void OnPostprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            Process(report.summary.outputPath);
        }
        #else
        public void OnPostprocessBuild(UnityEditor.BuildTarget target, string path)
        {
            Process(path);
        }
        #endif

        private void Process(string path)
        {
            // Read the project contents from file
            var pbxProjectPath = Path.Combine(path, WearableConstants.XcodeProjectName);
            _project = new PBXProject();
            _project.ReadFromFile(pbxProjectPath);

            _appGuid = _project.TargetGuidByName(PBXProject.GetUnityTargetName());

            // Link Frameworks
            AddFrameworksToEmbeddedBinaries();

            // Add Empty Swift File
            EnableEmbeddedSwift();
            
            // Ensure Info.plist contains message for Bluetooth usage.
            AddBluetoothMessageToInfoPlist(path);

            // Finalize the changes by writing them back to file.
            _project.WriteToFile(pbxProjectPath);
        }

        /// <summary>
        /// For each framework, get the filename and add that framework to embedded binaries section.
        /// </summary>
        private void AddFrameworksToEmbeddedBinaries()
        {
            var directory = Path.Combine(Application.dataPath, WearableConstants.NativeArtifactsPath);
            var frameworks = Directory.GetDirectories(directory, WearableConstants.FrameworkFileFilter)
                .Select(Path.GetFileName)
                .ToArray();

            for (var i = 0; i < frameworks.Length; i++)
            {
                AddFrameworkToEmbeddedBinaries(frameworks[i]);
            }
        }

        /// <summary>
        /// Add framework to the embedded binaries section.
        /// </summary>
        /// <param name="frameworkName"></param>
        private void AddFrameworkToEmbeddedBinaries(string frameworkName)
        {
            // Get the GUID of the framework that Unity will automatically add to the xcode project
            var projectFrameworkPath = Path.Combine(WearableConstants.XcodeProjectFrameworksPath, frameworkName);
            var frameworkGuid = _project.FindFileGuidByProjectPath(projectFrameworkPath);

            // Add framework as embedded binary
            _project.AddFileToEmbedFrameworks(_appGuid, frameworkGuid);
        }

        /// <summary>
        /// Enables the compilation of Swift in embedded code by setting several build properties.
        /// </summary>
        private void EnableEmbeddedSwift()
        {
            // Add several build properties that help
            _project.SetBuildProperty(
                _appGuid,
                WearableConstants.XcodeBuildPropertyModulesKey,
                WearableConstants.XcodeBuildPropertyEnableValue);

            _project.AddBuildProperty(
                _appGuid,
                WearableConstants.XcodeBuildPropertySearchPathsKey,
                WearableConstants.XcodeBuildPropertySearchPathsValue);

            _project.SetBuildProperty(
                _appGuid,
                WearableConstants.XcodeBuildPropertyEmbedSwiftKey,
                WearableConstants.XcodeBuildPropertyEnableValue);

            _project.SetBuildProperty(
                _appGuid,
                WearableConstants.XcodeBuildPropertySwiftVersionKey,
                WearableConstants.XcodeBuildPropertySwiftVersionValue);

            _project.SetBuildProperty(
                _appGuid,
                WearableConstants.XcodeBuildPropertySwiftOptimizationKey,
                WearableConstants.XcodeBuildPropertySwiftOptimizationValue);
        }
		/// <summary>
		/// Ensures that the project's Info.plist contains a message that describes the Bluetooth usage for app submission.
		/// </summary>
        private void AddBluetoothMessageToInfoPlist(string projectPath)
		{
			string pListPath = Path.GetFullPath(Path.Combine(projectPath, WearableConstants.XcodeInfoPlistRelativePath));
			
			PlistDocument infoPlist = new PlistDocument();
			infoPlist.ReadFromFile(pListPath);

			PlistElementDict infoDict = infoPlist.root;
			// Set a valid description for the use case of the bluetooth devices if none is set. Otherwise we assume
			// the user has set one and we don't want to overwrite it. Without this message, Apple may reject your
			// app submission.
			if (!infoDict.values.ContainsKey(WearableConstants.XcodeInfoPlistBluetoothKey))
			{
				infoDict.SetString(
					WearableConstants.XcodeInfoPlistBluetoothKey, 
					WearableConstants.XcodeInfoPlistBluetoothMessage);
				
				Debug.LogWarningFormat(WearableConstants.XcodeInfoPlistAlterationWarningWithMessage, 
										WearableConstants.XcodeInfoPlistBluetoothKey,
										WearableConstants.XcodeInfoPlistBluetoothMessage);
				
				infoPlist.WriteToFile(pListPath);
			}
		}
    }
}
#endif
