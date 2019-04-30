/* WearableUSBProvider.cs
*
* Provider class to connect to a device over USB.  We do not expect this class to be used
* on phones or tablets, since the point is to keep someone from walking off with the device.
* The native function implementations are in BoseWearableUSB.dll.
*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Bose.Wearable
{
	[Serializable]
	public class WearableUSBProvider : WearableProviderBase
	{
		#region Provider-Specific
		/// <summary>
		/// Represents a session with an Wearable Device.
		/// </summary>
		private enum SessionStatus
		{
			Closed,
			Opening,
			Open
		}


		#pragma warning disable 0414
		[SerializeField]
		// This flag controls printing of log messages which aren't warnings or errors, both here
		// and in the DLL.
		private bool _debugLogging;
		private char[] _statusMessageSeparators;

		// log any status accumulated since the previous update
		private SessionStatus _sessionStatus;
		private StringBuilder _statusMessage;
		#pragma warning restore 0414


		public void SetDebugLoggingInPlugin()
		{
			#if UNITY_EDITOR
			WearableUSBSetDebugLogging(_debugLogging);
			#endif // UNITY_EDITOR
		}

		#endregion // Provider-Specific

		#region Provider API


		internal override void SearchForDevices(Action<Device[]> onDevicesUpdated)
		{
			#if UNITY_EDITOR
			StopSearchingForDevices();

			if (onDevicesUpdated == null)
			{
				return;
			}

			WearableUSBRefreshDeviceList();
			_deviceSearchCallback = onDevicesUpdated;
			_performDeviceSearch = true;
			_nextDeviceSearchTime = Time.unscaledTime + WearableConstants.DeviceUSBConnectUpdateIntervalInSeconds;
			#else
			Debug.LogError(WearableConstants.UnsupportedPlatformError);
			onDevicesUpdated(WearableConstants.EmptyDeviceList);
			#endif // UNITY_EDITOR
		}

		internal override void StopSearchingForDevices()
		{
			if (_performDeviceSearch)
			{
				_performDeviceSearch = false;
				_deviceSearchCallback = null;
				_nextDeviceSearchTime = float.PositiveInfinity;
			}
		}

		internal override void ConnectToDevice(Device device, Action onSuccess, Action onFailure)
		{
			DisconnectFromDevice();

			_performDeviceConnection = true;
			_deviceConnectSuccessCallback = onSuccess;
			_deviceConnectFailureCallback = onFailure;
			_deviceToConnect = device;
			_nextDeviceConnectTime = Time.unscaledTime + WearableConstants.DeviceUSBConnectUpdateIntervalInSeconds;

			#if UNITY_EDITOR
			WearableUSBSetDebugLogging(_debugLogging);
			WearableUSBOpenSession(_deviceToConnect.uid);
			#endif // UNITY_EDITOR

			OnDeviceConnecting(_deviceToConnect);
		}

		internal override void DisconnectFromDevice()
		{
			StopDeviceConnection();
			StopDeviceMonitor();

			for (var i = 0; i < WearableConstants.GestureIds.Length; i++)
			{
				if (WearableConstants.GestureIds[i] == GestureId.None)
				{
					continue;
				}

				_gestureStatus[WearableConstants.GestureIds[i]] = false;
			}

			if (_connectedDevice == null)
			{
				return;
			}

			OnDeviceDisconnected(_connectedDevice.Value);

			_connectedDevice = null;

			#if UNITY_EDITOR
			WearableUSBCloseSession();
			#endif // UNITY_EDITOR
		}

		internal override SensorUpdateInterval GetSensorUpdateInterval()
		{
			return _sensorUpdateInterval;
		}

		internal override void SetSensorUpdateInterval(SensorUpdateInterval updateInterval)
		{
			if (_connectedDevice == null)
			{
				Debug.LogWarning(WearableConstants.SetUpdateRateWithoutDeviceWarning);
				return;
			}

			_sensorUpdateInterval = updateInterval;

			// n.b. currently, the only way to set the global update interval is along with a call to EnableSensor.
			// Until a method is added for this, a suitable workaround is to call EnableSensor on a sensor that is
			// already enabled. If no sensors are enabled, the cached value will of _sensorUpdateInterval will be
			// used the next time a sensor is enabled.
			#if UNITY_EDITOR
			var enumerator = _sensorStatus.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<SensorId, bool> element = enumerator.Current;
					if (element.Value)
					{
						int milliseconds = (int)WearableTools.SensorUpdateIntervalToMilliseconds(_sensorUpdateInterval);
						WearableUSBEnableSensor((int)element.Key, milliseconds);
					}
				}
			}
			finally
			{
				enumerator.Dispose();
			}
			#endif // UNITY_EDITOR
		}

		internal override RotationSensorSource GetRotationSource()
		{
			return _rotationSource;
		}

		internal override void SetRotationSource(RotationSensorSource source)
		{
			if (_connectedDevice == null)
			{
				Debug.LogWarning(WearableConstants.SetRotationSourceWithoutDeviceWarning);
				return;
			}

			_rotationSource = source;

			#if UNITY_EDITOR
			WearableUSBSetRotationSource((int)source);
			#endif
		}

		internal override void StartSensor(SensorId sensorId)
		{
			if (_connectedDevice == null)
			{
				_sensorStatus[sensorId] = false;
				Debug.LogWarning(WearableConstants.StartSensorWithoutDeviceWarning);
				return;
			}

			if (_sensorStatus[sensorId])
			{
				return;
			}

			#if UNITY_EDITOR
			int milliseconds = (int)WearableTools.SensorUpdateIntervalToMilliseconds(_sensorUpdateInterval);
			WearableUSBEnableSensor((int)sensorId, milliseconds);
			#endif // UNITY_EDITOR

			_sensorStatus[sensorId] = true;
		}

		internal override void StopSensor(SensorId sensorId)
		{
			if (!_sensorStatus[sensorId])
			{
				return;
			}

			#if UNITY_EDITOR
			WearableUSBDisableSensor((int)sensorId);
			#endif // UNITY_EDITOR

			_sensorStatus[sensorId] = false;
		}

		internal override bool GetSensorActive(SensorId sensorId)
		{
			return _sensorStatus[sensorId];
		}

		internal override void EnableGesture(GestureId gestureId)
		{
			if (_connectedDevice == null)
			{
				_gestureStatus[gestureId] = false;
				Debug.LogWarning(WearableConstants.EnableGestureWithoutDeviceWarning);
				return;
			}

			if (_gestureStatus[gestureId])
			{
				return;
			}

			#if UNITY_EDITOR
			WearableUSBEnableGesture((int) gestureId);
			#endif // UNITY_EDITOR

			_gestureStatus[gestureId] = true;
		}

		internal override void DisableGesture(GestureId gestureId)
		{
			if (_connectedDevice == null)
			{
				Debug.LogWarning(WearableConstants.DisableGestureWithoutDeviceWarning);
				return;
			}

			if (_gestureStatus[gestureId] == false)
			{
				return;
			}

			#if UNITY_EDITOR
			WearableUSBDisableGesture((int)gestureId);
			#endif // UNITY_EDITOR

			_gestureStatus[gestureId] = false;
		}

		internal override bool GetGestureEnabled(GestureId gestureId)
		{
			return _gestureStatus[gestureId];
		}

		internal override void OnInitializeProvider()
		{
			if (_initialized)
			{
				return;
			}

			#if UNITY_EDITOR
			WearableUSBInitialize();
			WearableUSBSetDebugLogging(_debugLogging);
			#endif // UNITY_EDITOR

			base.OnInitializeProvider();

			StopDeviceConnection();
			StopDeviceMonitor();
			StopSearchingForDevices();
		}

		internal override void OnDestroyProvider()
		{
			base.OnDestroyProvider();

			DisconnectFromDevice();

			StopDeviceConnection();
			StopDeviceMonitor();
			StopSearchingForDevices();
		}

		/// <summary>
		/// When enabled, resume monitoring the device session if necessary.
		/// </summary>
		internal override void OnEnableProvider()
		{
			if (_enabled)
			{
				return;
			}

			base.OnEnableProvider();

			if (_connectedDevice != null)
			{
				StartDeviceMonitor();
			}
		}

		/// <summary>
		/// When disabled, stop actively searching for, connecting to, and monitoring devices.
		/// </summary>
		internal override void OnDisableProvider()
		{
			if (!_enabled)
			{
				return;
			}

			base.OnDisableProvider();


			StopSearchingForDevices();
			StopDeviceMonitor();
			StopDeviceConnection();
		}

		internal override void OnUpdate()
		{
			UpdateDeviceConnection();

			// Request the latest updates for this frame
			if (_connectedDevice != null)
			{
				GetLatestSensorUpdates();
			}

			#if UNITY_EDITOR
			// Check if it's time to query discovered devices
			if (_performDeviceSearch)
			{
				if (Time.unscaledTime >= _nextDeviceSearchTime)
				{
					_nextDeviceSearchTime += WearableConstants.DeviceUSBConnectUpdateIntervalInSeconds;
					Device[] devices = GetDiscoveredDevices();
					if (_deviceSearchCallback != null)
					{
						_deviceSearchCallback.Invoke(devices);
					}
				}
			}

			// Check if it's time to query the connection routine
			if (_performDeviceConnection && Time.unscaledTime >= _nextDeviceConnectTime)
			{
				_nextDeviceConnectTime += WearableConstants.DeviceUSBConnectUpdateIntervalInSeconds;
				PerformDeviceConnection();
			}

			// Check if it's time to query the device monitor
			if (_pollDeviceMonitor && Time.unscaledTime >= _nextDeviceMonitorTime)
			{
				// NB: The monitor uses the same time interval
				_nextDeviceMonitorTime += WearableConstants.DeviceUSBConnectUpdateIntervalInSeconds;
				MonitorDeviceSession();
			}
			#endif // UNITY_EDITOR
		}

		#endregion // Provider API

		#region Private

		#pragma warning disable CS0414

		// Sensor status
		private readonly Dictionary<SensorId, bool> _sensorStatus;
		private SensorUpdateInterval _sensorUpdateInterval;
		private RotationSensorSource _rotationSource;

		// Gesture status
		private readonly Dictionary<GestureId, bool> _gestureStatus;

		// Device search
		private bool _performDeviceSearch;
		private Action<Device[]> _deviceSearchCallback;
		private float _nextDeviceSearchTime;
		private StringBuilder _uidBuilder;
		private StringBuilder _nameBuilder;
		private StringBuilder _firmwareVersionBuilder;

		// Device connection
		private bool _performDeviceConnection;
		private Device _deviceToConnect;
		private Action _deviceConnectSuccessCallback;
		private Action _deviceConnectFailureCallback;
		private float _nextDeviceConnectTime;

		// Device monitoring
		private bool _pollDeviceMonitor;
		private float _nextDeviceMonitorTime;

		#pragma warning restore CS0414

		internal WearableUSBProvider()
		{
			_statusMessageSeparators = new[] { '\n' };
			_sessionStatus = SessionStatus.Closed;
			_statusMessage = new StringBuilder(8192);
			_uidBuilder = new StringBuilder(256);
			_nameBuilder = new StringBuilder(256);
			_firmwareVersionBuilder = new StringBuilder(256);

			_sensorStatus = new Dictionary<SensorId, bool>();
			_sensorUpdateInterval = WearableConstants.DefaultUpdateInterval;
			_rotationSource = WearableConstants.DefaultRotationSource;

			_sensorStatus.Add(SensorId.Accelerometer, false);
			_sensorStatus.Add(SensorId.Gyroscope, false);
			_sensorStatus.Add(SensorId.Rotation, false);

			_gestureStatus = new Dictionary<GestureId, bool>();
			for (var i = 0; i < WearableConstants.GestureIds.Length; i++)
			{
				if (WearableConstants.GestureIds[i] == GestureId.None)
				{
					continue;
				}

				_gestureStatus.Add(WearableConstants.GestureIds[i], false);
			}
		}


		/// <summary>
		/// Used internally by WearableControl to get the latest buffer of SensorFrame updates from
		/// the Wearable Device; the newest frame in that batch is set as the CurrentSensorFrame.
		/// </summary>
		private void GetLatestSensorUpdates()
		{
			_currentSensorFrames.Clear();

			#if UNITY_EDITOR
			unsafe
			{
				bool anyNewSensorFrames = false;
				USBSensorFrame frame;
				while (WearableUSBGetNextSensorFrame(&frame))
				{
					anyNewSensorFrames = true;

					_currentSensorFrames.Add(new SensorFrame
					{
						timestamp = WearableConstants.Sensor2UnityTime * frame.timestamp,
						deltaTime = WearableConstants.Sensor2UnityTime * frame.deltaTime,
						acceleration = frame.acceleration,
						angularVelocity = frame.angularVelocity,
						rotation = frame.rotation,
						gestureId = frame.gesture
					});
				}

				if (anyNewSensorFrames)
				{
					_lastSensorFrame = _currentSensorFrames[_currentSensorFrames.Count - 1];
					OnSensorsOrGestureUpdated(_lastSensorFrame);
				}
			}
			#endif // UNITY_EDITOR
		}


		private void UpdateDeviceConnection()
		{
			#if UNITY_EDITOR
			WearableUSBUpdate();

			// log any status accumulated since the previous update
			_statusMessage.Length = 0;
			_sessionStatus = (SessionStatus) WearableUSBGetSessionStatus(_statusMessage, _statusMessage.Capacity);

			if (_statusMessage.Length > 0)
			{
				string[] lines = _statusMessage.ToString().Split(_statusMessageSeparators);
				int numLines = lines.Length;
				for (int i = 0; i < numLines; ++i)
				{
					if (lines[i].Length > 1)
					{
						Debug.Log(lines[i]);
					}
				}
			}
			#endif // UNITY_EDITOR
		}


		/// <summary>
		/// Used internally to get the latest list of discovered devices from
		/// the native SDK.
		/// </summary>
		private Device[] GetDiscoveredDevices()
		{
			Device[] devices = WearableConstants.EmptyDeviceList;

			#if UNITY_EDITOR
			WearableUSBRefreshDeviceList();

			unsafe
			{
				int count = WearableUSBGetNumDiscoveredDevices();
				if (count > 0)
				{
					devices = new Device[count];
					for (int i = 0; i < count; i++)
					{
						_uidBuilder.Length = 0;
						WearableUSBGetDiscoveredDeviceUID(i, _uidBuilder, _uidBuilder.Capacity);
						_nameBuilder.Length = 0;
						WearableUSBGetDiscoveredDeviceName(i, _nameBuilder, _nameBuilder.Capacity);
						_firmwareVersionBuilder.Length = 0;
						WearableUSBGetDiscoveredDeviceFirmwareVersion(i, _firmwareVersionBuilder, _firmwareVersionBuilder.Capacity);

						devices[i] = new Device
						{
							uid = _uidBuilder.ToString(),
							name = _nameBuilder.ToString(),
							firmwareVersion = _firmwareVersionBuilder.ToString(),
							isConnected = (WearableUSBGetDiscoveredDeviceIsConnected(i) == 0)? false : true,
							productId = ProductId.Undefined,
							variantId = (byte) VariantType.Unknown
						};
					}
				}
			}
			#endif // UNITY_EDITOR

			return devices;
		}

		/// <summary>
		/// Attempts to create a session to a specified device and then checks for the session status perpetually until
		/// a SessionStatus of either Open or Closed is returned, equating to either successful or failed.
		/// </summary>
		private void PerformDeviceConnection()
		{
			#if UNITY_EDITOR
			switch (_sessionStatus)
			{
				// Receiving a session status of Closed while attempting to open a session indicates an error occured.
				case SessionStatus.Closed:
					if (string.IsNullOrEmpty(_statusMessage.ToString()))
					{
						Debug.LogWarning(WearableConstants.DeviceConnectionFailed);
					}
					else
					{
						Debug.LogWarningFormat(WearableConstants.DeviceConnectionFailedWithMessage, _statusMessage);
					}

					if (_deviceConnectFailureCallback != null)
					{
						_deviceConnectFailureCallback.Invoke();
					}

					StopDeviceConnection();

					break;

				case SessionStatus.Opening:
					// Device is still connecting.
					break;

				case SessionStatus.Open:
					if (_debugLogging)
					{
						Debug.Log(WearableConstants.DeviceConnectionOpened);
					}

					// ProductId and VariantId are only accessible after a connection has been opened. Update the values for the _connectDevice.
					_deviceToConnect.productId = (ProductId)WearableUSBGetDeviceProductID();
					_deviceToConnect.variantId = (byte) WearableUSBGetDeviceVariantID();

					// Make sure productId value is defined.
					if (!Enum.IsDefined(typeof(ProductId), _deviceToConnect.productId))
					{
						_deviceToConnect.productId = ProductId.Undefined;
					}

					if (!Enum.IsDefined(typeof(VariantType), (VariantType) _deviceToConnect.variantId))
					{
						_deviceToConnect.variantId = (byte) VariantType.Unknown;
					}

					_connectedDevice = _deviceToConnect;


					if (_deviceConnectSuccessCallback != null)
					{
						_deviceConnectSuccessCallback.Invoke();
					}

					OnDeviceConnected(_deviceToConnect);

					StartDeviceMonitor();

					StopDeviceConnection();
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
			#endif // UNITY_EDITOR
		}

		/// <summary>
		/// Enables the device monitor
		/// </summary>
		private void StartDeviceMonitor()
		{
			_pollDeviceMonitor = true;

			// NB The device monitor runs on the same time interval as the connection routine
			_nextDeviceMonitorTime = Time.unscaledTime + WearableConstants.DeviceUSBConnectUpdateIntervalInSeconds;
		}

		/// <summary>
		/// Halts the device monitor
		/// </summary>
		private void StopDeviceMonitor()
		{
			_pollDeviceMonitor = false;
			_nextDeviceMonitorTime = float.PositiveInfinity;
		}

		/// <summary>
		/// Monitors the current device SessionStatus until a non-Open session status is returned. Once this has occured,
		/// the device has become disconnected and should render all state as such.
		/// </summary>
		private void MonitorDeviceSession()
		{
			#if UNITY_EDITOR
			if (_sessionStatus != SessionStatus.Open)
			{
				if (string.IsNullOrEmpty(_statusMessage.ToString()))
				{
					Debug.Log(WearableConstants.DeviceConnectionMonitorWarning);
				}
				else
				{
					Debug.LogFormat(WearableConstants.DeviceConnectionMonitorWarningWithMessage, _statusMessage);
				}

				if (_connectedDevice != null)
				{
					OnDeviceDisconnected(_connectedDevice.Value);
				}

				_sensorStatus[SensorId.Accelerometer] = false;
				_sensorStatus[SensorId.Gyroscope] = false;
				_sensorStatus[SensorId.Rotation] = false;

				for (var i = 0; i < WearableConstants.GestureIds.Length; i++)
				{
					if (WearableConstants.GestureIds[i] == GestureId.None)
					{
						continue;
					}

					_gestureStatus[WearableConstants.GestureIds[i]] = false;
				}

				StopDeviceMonitor();

				_connectedDevice = null;
			}
			#endif // UNITY_EDITOR
		}

		/// <summary>
		/// Halts the device connection routine
		/// </summary>
		private void StopDeviceConnection()
		{
			_performDeviceConnection = false;
			_deviceConnectFailureCallback = null;
			_deviceConnectSuccessCallback = null;
			_nextDeviceConnectTime = float.PositiveInfinity;
		}

		#endregion // Private

		#region DLL Imports

		#if UNITY_EDITOR

		/// <summary>
		/// This struct matches the plugin bridge definition and is only used as a temporary convert from the native
		/// code struct to the public struct.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		private struct USBSensorFrame
		{
			public int timestamp;
			public int deltaTime;
			public SensorVector3 acceleration;
			public SensorVector3 angularVelocity;
			public SensorQuaternion rotation;
			public GestureId gesture;
		}

		/// <summary>
		/// Initializes the USB DLL.  This only needs to be called once per session.
		/// </summary>
		[DllImport("BoseWearableUSBBridge")]
		private static extern void WearableUSBInitialize();

		/// <summary>
		/// Starts the search for available Wearable devices on USB.
		/// </summary>
		[DllImport("BoseWearableUSBBridge")]
		private static extern void WearableUSBRefreshDeviceList();

		/// <summary>
		/// Returns number of available USB Wearable devices.
		/// </summary>
		[DllImport("BoseWearableUSBBridge")]
		private static extern int WearableUSBGetNumDiscoveredDevices();

		/// <summary>
		/// Returns UID of available Wearable device at index.  Returns false if index is invalid.
		/// </summary>
		[DllImport("BoseWearableUSBBridge", CharSet = CharSet.Unicode)]
		private static extern int WearableUSBGetDiscoveredDeviceUID(int index, StringBuilder uidBuilder, int builderLength);

		/// <summary>
		/// Returns name of available Wearable device at index.  Returns false if index is invalid.
		/// </summary>
		[DllImport("BoseWearableUSBBridge", CharSet = CharSet.Unicode)]
		private static extern int WearableUSBGetDiscoveredDeviceName(int index, StringBuilder nameBuilder, int builderLength);
		
		/// <summary>
		/// Returns name of available Wearable device at index.  Returns false if index is invalid.
		/// </summary>
		[DllImport("BoseWearableUSBBridge", CharSet = CharSet.Unicode)]
		private static extern int WearableUSBGetDiscoveredDeviceFirmwareVersion(int index, StringBuilder firmware, int builderLength);

		/// <summary>
		/// Returns name of available Wearable device at index.  Returns false if index is invalid.
		/// </summary>
		[DllImport("BoseWearableUSBBridge")]
		private static extern unsafe int WearableUSBGetDiscoveredDeviceIsConnected(int index);

		/// <summary>
		/// Returns the ProductId of a device. This will default to 0 if there is not an open session yet.
		/// The ProductId of a device is only available once a session has been opened.
		/// </summary>
		[DllImport("BoseWearableUSBBridge")]
		private static extern int WearableUSBGetDeviceProductID();

		/// <summary>
		/// Returns the VariantId of a device. This will default to 0 if there is not an open session yet.
		/// The VariantId of a device is only available once a session has been opened.
		/// </summary>
		[DllImport("BoseWearableUSBBridge")]
		private static extern int WearableUSBGetDeviceVariantID();

		/// <summary>
		/// Attempts to open a session with a specific Wearable Device by way of <paramref name="deviceUid"/>.
		/// </summary>
		[DllImport("BoseWearableUSBBridge", CharSet = CharSet.Unicode)]
		private static extern void WearableUSBOpenSession(string deviceUid);

		/// <summary>
		/// Assesses the SessionStatus of the currently opened session for a specific Wearable device. If there has
		/// been an error, <paramref name="errorMsg"/> will be populated with the text contents of the error message.
		/// The return value is really a SessionStatus.
		/// </summary>
		[DllImport("BoseWearableUSBBridge", CharSet = CharSet.Unicode)]
		private static extern int WearableUSBGetSessionStatus(StringBuilder errorMsg, int bufferLength);

		/// <summary>
		/// Closes the session.  You can open another.
		/// </summary>
		[DllImport("BoseWearableUSBBridge")]
		private static extern void WearableUSBCloseSession();

		/// <summary>
		/// Have the DLL fetch the latest state from the device.  Returns 0 if not connected.
		/// </summary>
		[DllImport("BoseWearableUSBBridge")]
		private static extern unsafe int WearableUSBUpdate();

		/// <summary>
		/// Returns unread USBSensorFrame at index.  Returns false if no next frame is available.
		/// </summary>
		[DllImport("BoseWearableUSBBridge")]
		private static extern unsafe bool WearableUSBGetNextSensorFrame(USBSensorFrame* sensorFrameindex);

		/// <summary>
		/// Enable a specific sensor by passing the desired <paramref name="sensorId"/> which is the int value
		/// of the SensorId enum. Pass the desired <paramref name="intervalId"/> which is the enum value of
		/// SensorUpdateInterval.  All sensors by default will have the same update interval. If you enable a
		/// new sensor with a different update interval than the current configuration, all old sensors will be
		/// set to have the new update interval.
		/// </summary>
		[DllImport("BoseWearableUSBBridge")]
		private static extern void WearableUSBEnableSensor(int sensorId, int intervalMilliseconds);

		/// <summary>
		/// Disable a specific sensor by passing the desired <paramref name="sensorId"/> which is the int value
		/// of the SensorId enum. Any listeners on the sensor that you disable will stop receiving callbacks once disabled.
		/// </summary>
		[DllImport("BoseWearableUSBBridge")]
		private static extern void WearableUSBDisableSensor(int sensorId);

		/// <summary>
		/// Switch our rotation data between RotationSensorSource.SixDof and RotationSensorSource.NineDof.
		/// </summary>
		[DllImport("BoseWearableUSBBridge")]
		private static extern void WearableUSBSetRotationSource(int rotationSource);

		/// <summary>
		/// Enable a specific gesture by passing the desired <paramref name="gestureId"/> which is the int value
		/// of the SensorGesture enum.
		/// </summary>
		[DllImport("BoseWearableUSBBridge")]
		private static extern void WearableUSBEnableGesture(int gestureId);

		/// <summary>
		/// Disable a specific gesture by passing the desired <paramref name="gestureId"/> which is the int value
		/// of the SensorGesture enum.
		/// </summary>
		[DllImport("BoseWearableUSBBridge")]
		private static extern void WearableUSBDisableGesture(int gestureId);

		/// <summary>
		/// Turn on or off logging of TAP commands sent, responses received, and warnings.  Errors are logged even
		/// if this is off.
		/// </summary>
		[DllImport("BoseWearableUSBBridge")]
		private static extern void WearableUSBSetDebugLogging(bool loggingOn);

		#endif // UNITY_EDITOR

		#endregion // DLL Imports
	}
}
