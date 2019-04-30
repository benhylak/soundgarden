using System;
using UnityEngine;

namespace Bose.Wearable
{
	public static class WearableConstants
	{
		public static readonly SensorFrame EmptyFrame;
		public static readonly Device[] EmptyDeviceList;
		public static readonly GestureId[] GestureIds;
		public static readonly SensorId[] SensorIds;

		public const string EmptyUID = "00000000-0000-0000-0000-000000000000";

		public const string DefaultFirmwareVersion = "0.0.0";

		public const SensorUpdateInterval DefaultUpdateInterval = SensorUpdateInterval.EightyMs;

		public const RotationSensorSource DefaultRotationSource = RotationSensorSource.SixDof;

		/// <summary>
		/// Conversion from sensor timestamps (millis) to Unity time (seconds).
		/// </summary>
		public const float Sensor2UnityTime = 0.001f;

		/// <summary>
		/// The default RSSI threshold that devices will be filtered by.
		/// </summary>
		public const int DefaultRSSIThreshold = -65;

		/// <summary>
		/// The minimum rssi threshold value.
		/// </summary>
		public const int MinimumRSSIValue = -70;

		/// <summary>
		/// The maximum rssi threshold value.
		/// </summary>
		public const int MaximumRSSIValue = -30;

		/// <summary>
		/// The number of seconds to wait when the sensor frequency update capability has been locked before
		/// unlocking it.
		/// </summary>
		public const float NumberOfSecondsToLockSensorFrequencyUpdate = 0.33f;

		public const float DeviceConnectUpdateIntervalInSeconds = 1.0f;
		public const float DeviceUSBConnectUpdateIntervalInSeconds = 2.0f;
		public const float DeviceSearchUpdateIntervalInSeconds = 1.0f;

		// Console Warnings
		public const string UnsupportedPlatformError = "[Bose Wearable] The active provider does not support this platform.";
		public const string DeviceConnectionPermissionError =
			"[Bose Wearable] User has denied the \"Coarse Location\" permission required to search for devices " +
			"over Bluetooth. No devices will be discovered.";
		public const string DeviceConnectionFailed = "[Bose Wearable] The connection to the device has failed. " +
		                                             "Ending attempt to connect to device.";
		public const string DeviceConnectionFailedWithMessage = "[Bose Wearable] The connection to the device has failed with " +
		                                                        "error message '{0}'. Ending attempt to connect to device.";
		public const string DeviceConnectionOpened = "[Bose Wearable] The connection to the device has been opened.";
		public const string DeviceConnectionMonitorWarning = "[Bose Wearable] The connection to the device has ended.";
		public const string DeviceConnectionMonitorWarningWithMessage = "[Bose Wearable] The connection to the device has ended " +
		                                                                "with message '{0}'.";

		public const string StartSensorWithoutDeviceWarning = "[Bose Wearable] A device must be connected before starting a sensor. " +
		                                                      "The sensor will not be started when device connects.";
		public const string SetUpdateRateWithoutDeviceWarning = "[Bose Wearable] A device must be connected before setting the " +
																"update interval. The rate will not be set when a device connects.";
		public const string SetRotationSourceWithoutDeviceWarning = "[Bose Wearable] A device must be connected before setting the " +
																	"rotation source.";
		public const string EnableGestureWithoutDeviceWarning = "[Bose Wearable] A device must be connected before starting a gesture. " +
															  "The gesture will not be started when device connects.";
		public const string DisableGestureWithoutDeviceWarning = "[Bose Wearable] A device must be connected before disabling a gesture.";
		public const string EnableGestureNotSupportedWarning = "[Bose Wearable] Attempted to start gesture {0}, which is not supported.";
		public const string DisableGestureNotSupportedWarning = "[Bose Wearable] Attempted to stop gesture {0}, which is not supported.";

		public const string SensorUpdateIntervalDecreasedWarning = "[BoseWearable] A SensorUpdateInterval of TwentyMs cannot be used when " +
		                                                           "three or more sensors are enabled; this value has been changed to FourtyMs " +
		                                                           "to prevent a fatal crash.";

		public const string GestureIdNoneInvalidError = "[Bose Wearable] GestureId.None will not return a valid WearableGesture.";

		public const string WearableGestureNotYetSupported = "[Bose Wearable] GestureId.{0} is not yet supported yet and will not return a " +
		                                                     "valid WearableGesture";

		public const string OnlyOneSensorFrequencyUpdatePerFrameWarning =
			"[Bose Wearable] Device state can only be updated once per frame and is locked for several seconds afterward; " +
		    "additional updates after the first will not occur until this has been unlocked.";

		public const string DuplicateWearableModelProductTypeWarning =
			"[Bose Wearable] There is a duplicate WearableModel ProductType " +
			"definition for {0} on this WearableModelLoader";

		public const string DuplicateWearableModelVariantTypeWarning =
			"[Bose Wearable] There is a duplicate WearableModel VariantType " +
			"definition for {0} on this WearableModelLoader";

		public const string RotationSourceSelectionUnsupportedWarning =
			"[Bose Wearable] Rotation source selection is not implemented by the active provider.";
		public const string NoneIsInvalidGesture = "[Bose Wearable] GestureId.None is an invalid value to " +
		                                            "set on [{0}].";

		// Debug Provider Defaults
		public const string DebugProviderDefaultDeviceName = "Debug Device";
		public const ProductId DebugProviderDefaultProductId = ProductId.Undefined;
		public const byte DebugProviderDefaultVariantId = 0;
		public const int DebugProviderDefaultRSSI = 0;
		public const string DebugProviderDefaultUID = EmptyUID;

		// Debug Provider Messages
		public const string DebugProviderInit = "[Bose Wearable] Debug provider initialized.";
		public const string DebugProviderDestroy = "[Bose Wearable] Debug provider destroyed.";
		public const string DebugProviderEnable = "[Bose Wearable] Debug provider enabled.";
		public const string DebugProviderDisable = "[Bose Wearable] Debug provider disabled.";
		public const string DebugProviderSearchingForDevices = "[Bose Wearable] Debug provider searching for devices...";
		public const string DebugProviderStoppedSearching = "[Bose Wearable] Debug provider stopped searching for devices.";
		public const string DebugProviderConnectingToDevice = "[Bose Wearable] Debug provider connecting to virtual device...";
		public const string DebugProviderConnectedToDevice = "[Bose Wearable] Debug provider connected to virtual device.";
		public const string DebugProviderDisconnectedToDevice = "[Bose Wearable] Debug provider disconnected from virtual device.";
		public const string DebugProviderStartSensor = "[Bose Wearable] Debug provider starting sensor {0}";
		public const string DebugProviderStopSensor = "[Bose Wearable] Debug provider stopping sensor {0}";
		public const string DebugProviderSetUpdateInterval = "[Bose Wearable] Debug provider setting update interval to {0}";
		public const string DebugProviderSetRotationSource = "[Bose Wearable] Debug provider setting rotation source to {0}";
		public const string DebugProviderInvalidConnectionWarning = "[Bose Wearable] Debug provider may only connect to" +
																	"its own virtual device as returned by SearchForDevices().";
		public const string DebugProviderSimulateDisconnect = "[Bose Wearable] Debug provider simulating a disconnected device.";
		public const string DebugProviderEnableGesture = "[Bose Wearable] Debug provider starting gesture {0}";
		public const string DebugProviderDisableGesture = "[Bose Wearable] Debug provider stopping gesture {0}";
		public const string DebugProviderRotationSourceUnsupportedInfo = "[Bose Wearable] Setting the rotation source " +
																		 "has no effect when using the debug provider, " +
																		 "and will be ignored.";
		public const string DebugProviderTriggerDisabledGestureWarning =
			"[Bose Wearable] A gesture was triggered that is not currently enabled. This gesture will be ignored.";
		public const string DebugProviderTriggerGesture = "[Bose Wearable] Simulating gesture {0}.";

		// Mobile Provider Messages
		public const string MobileProviderDeviceName = "Mobile Device Simulator";
		public const ProductId MobileProviderProductId = ProductId.Undefined;
		public const byte MobileProviderVariantId = 0;
		public const string MobileProviderRotationSourceUnsupportedInfo = "[Bose Wearable] Setting the rotation source " +
																		  "has no effect when using the mobile provider, " +
																		  "and will be ignored.";

		// Proxy Provider Messages
		public const string ProxyProviderInvalidVersionError = "[Bose Wearable] Unsupported proxy protocol version." +
															   "Got version: 0x{0:X2}, supported version: 0x{1:X2}";
		public const string ProxyProviderInvalidPacketError = "[Bose Wearable] Invalid packet received by proxy.";
		public const string ProxyProviderNoDataWarning = "[Bose Wearable] Proxy provider has not yet received data and " +
														 "will return default values. Check again later.";
		public const string ProxyProviderBufferFullWarning = "[Bose Wearable] Proxy provider receive buffer is full, " +
															"and no packets can be consumed. Ensure the buffer is large " +
															"enough to consume the largest supported packet size.";
		public const string ProxyProviderConnectionFailedWarning = "[Bose Wearable] Proxy provider failed to connect to " +
																"server at {0}:{1}.";
		public const string ProxyProviderNotConnectedWarning = "[Bose Wearable] Proxy provider must be connected before " +
															"sending commands or requesting data.";

		// Editor Tooltips
		public const string SimulateHardwareDeviceTooltip = "If enabled when built to an iOS player the normal plugin " +
		                                                    "functionality will be swapped with a debug version to allow " +
		                                                    "for testing searching, connecting to, and receiving sensor " +
		                                                    "updates from a virtual device.";

		// Build Tools Messages
		public const string BuildToolsUnsupportedPlatformWarning = "[Bose Wearable] Trying to build for unsupported platform {0}.";

		// Runtime Dialogs
		public const string DeviceConnectSuccessMessage = "Device connected successfully!";
		public const string DeviceConnectFailureMessage = "Device failed to connect!";
		public const string DeviceConnectionSearchMessage = "Searching for devices...";
		public const string DeviceConnectSearchFailureMessage =
			"Searching for devices failed. Location permissions are required.";
		public const string DeviceConnectionUnderwayMessage = "Device connecting...";
		public const string DeviceDisconnectionMessage = "Device disconnected, please reconnect";
		public const string WaitForCalibrationMessage = "Please look forward and remain still...";

		#region Platform Constants: iOS
		// Post Build Processor
		public const int XcodePreBuildProcessorOrder = 0;
		public const int XcodePostBuildProcessorOrder = 0;

		// Xcode Workspace
		public const string XcodeProjectName = "Unity-iPhone.xcodeproj/project.pbxproj";

		// Framework path was broken in 2018.3.0->2018.3.3
		// From 2018.3.4 Release notes:
		// *  iOS: Fixed iOS Frameworks location is ignored when building Xcode project. (1108970)
		#if UNITY_2018_3_0 || UNITY_2018_3_1 || UNITY_2018_3_2 || UNITY_2018_3_3

		public const string XcodeProjectFrameworksPath = "Frameworks";

		#else

		public const string XcodeProjectFrameworksPath = "Frameworks/Plugins/iOS/BoseWearable";

		#endif

		// Messages
		public const string ArchitectureAlterationWarningWithMessage = "[Bose Wearable] iOS Architecture forced to 'ARM64'. " +
		                                                               "Was set to '{0}'.";
		public const string iOSVersionAlterationWarningWithMessage = "[Bose Wearable] iOS Minimum Version forced to '{0}'. Was set to '{1}'.";

		public const string iOSBluetoothAlterationWarning = "[Bose Wearable] Background Mode forced to allow connections to BLE devices.";

		// Info.plist properties
		public const string XcodeInfoPlistRelativePath = "./Info.plist";
		public const string XcodeInfoPlistBluetoothKey = "NSBluetoothPeripheralUsageDescription";
		public const string XcodeInfoPlistBluetoothMessage = "This app uses Bluetooth to communicate with Bose AR devices.";
		public const string XcodeInfoPlistAlterationWarningWithMessage = "[Bose Wearable] Added missing property to Info.plist: {0}: {1}";

		// Xcode Build Properties and Values
		public const string XcodeBuildPropertyEnableValue = "TRUE";
		public const string XcodeBuildPropertyDisableValue = "FALSE";
		public const string XcodeBuildPropertyBitCodeKey = "ENABLE_BITCODE";
		public const string XcodeBuildPropertyModulesKey = "CLANG_ENABLE_MODULES";
		public const string XcodeBuildPropertySearchPathsKey = "LD_RUNPATH_SEARCH_PATHS";
		public const string XcodeBuildPropertySearchPathsValue = "@executable_path/Frameworks";
		public const string XcodeBuildPropertyEmbedSwiftKey = "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES";
		public const string XcodeBuildPropertySwiftVersionKey = "SWIFT_VERSION";
		public const string XcodeBuildPropertySwiftVersionValue = "5.0";
		public const string XcodeBuildPropertySwiftOptimizationKey = "SWIFT_OPTIMIZATION_LEVEL";
		public const string XcodeBuildPropertySwiftOptimizationValue = "-Onone";

		// Framework Path and Names
		public const string FrameworkFileFilter = "*.framework";

		// Supported iOS Versions
		public const float MinimumSupportediOSVersion = 11.4f;

		/// <summary>
		/// This corresponds to the location of the native plugins in the Unity project.
		/// </summary>
		public const string NativeArtifactsPath = "Plugins/iOS/BoseWearable/";

		#endregion

		#region Platform Constants: Android


		// Build Order
		public const int AndroidPreBuildProcessorOrder = 0;

		// Messages
		public const string AndroidVersionAlterationWarningWithMessage = "[Bose Wearable] Android Minimum SDK Version forced to '{0}'. Was set to '{1}'.";

		// Supported Android SDK Versions
		public const int MinimumSupportedAndroidVersion = 21;

		#endregion


		// Scenes
		public const string MainMenuScene = "MainMenu";
		public const string BasicDemoScene = "BasicDemo";
		public const string AdvancedDemoScene = "AdvancedDemo";
		public const string GestureDemoScene = "GestureDemo";

		// Inspector
		public const float SingleLineHeight = 20f;
		public static readonly GUILayoutOption[] EmptyLayoutOptions;

		static WearableConstants()
		{
			// Ensure that empty frame has a valid rotation quaternion
			EmptyFrame = new SensorFrame
			{
				rotation = new SensorQuaternion {value = Quaternion.identity}
			};

			EmptyDeviceList = new Device[0];

			EmptyLayoutOptions = new GUILayoutOption[0];

			GestureIds = (GestureId[])Enum.GetValues(typeof(GestureId));
			SensorIds = (SensorId[])Enum.GetValues(typeof(SensorId));
		}
	}
}
