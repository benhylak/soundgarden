using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bose.Wearable
{
	/// <summary>
	/// Provides a minimal data provider that allows connection to a virtual device, and logs messages when provider
	/// methods are called. Never generates data frames.
	/// </summary>
	[Serializable]
	public sealed class WearableDebugProvider : WearableProviderBase
	{
		private enum RotationType
		{
			Euler,
			AxisAngle
		}

		public string Name
		{
			get { return _name; }
			set {_name = value; }
		}

		public string FirmwareVersion
		{
			get { return _firmwareVersion; }
			set { _firmwareVersion = value; }
		}

		public int RSSI
		{
			get { return _rssi; }
			set { _rssi = value; }
		}

		public string UID
		{
			get { return _uid; }
			set { _uid = value; }
		}

		public ProductId ProductId
		{
			get { return _productId; }
			set { _productId = value; }
		}

		public byte VariantId
		{
			get { return _variantId; }
			set { _variantId = value; }
		}

		public bool Verbose
		{
			get { return _verbose; }
			set { _verbose = value; }
		}

		#region Provider Unique

		public void SimulateDisconnect()
		{
			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderSimulateDisconnect);
			}

			DisconnectFromDevice();
		}

		/// <summary>
		/// Simulate a triggered gesture. If multiple gestures are triggered, they will be queued across sensor frames.
		/// </summary>
		/// <param name="gesture"></param>
		public void SimulateGesture(GestureId gesture)
		{
			if (gesture != GestureId.None)
			{
				_pendingGestures.Enqueue(gesture);
			}
		}

		#endregion

		#region WearableProvider Implementation

		internal override void SearchForDevices(Action<Device[]> onDevicesUpdated)
		{
			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderSearchingForDevices);
			}

			UpdateVirtualDeviceInfo();

			if (onDevicesUpdated != null)
			{
				onDevicesUpdated.Invoke(new []{_virtualDevice});
			}
		}

		internal override void StopSearchingForDevices()
		{
			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderStoppedSearching);
			}
		}

		internal override void ConnectToDevice(Device device, Action onSuccess, Action onFailure)
		{
			DisconnectFromDevice();

			UpdateVirtualDeviceInfo();

			if (device != _virtualDevice)
			{
				Debug.LogWarning(WearableConstants.DebugProviderInvalidConnectionWarning);
				return;
			}

			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderConnectingToDevice);
			}

			OnDeviceConnecting(_virtualDevice);

			_virtualDevice.isConnected = true;
			_connectedDevice = _virtualDevice;
			_nextSensorUpdateTime = Time.unscaledTime;

			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderConnectedToDevice);
			}

			if (onSuccess != null)
			{
				onSuccess.Invoke();
			}

			OnDeviceConnected(_virtualDevice);
		}

		internal override void DisconnectFromDevice()
		{
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

			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderDisconnectedToDevice);
			}

			OnDeviceDisconnected(_connectedDevice.Value);

			_virtualDevice.isConnected = false;
			_connectedDevice = null;
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

			if (_verbose)
			{
				Debug.LogFormat(
					WearableConstants.DebugProviderSetUpdateInterval,
					Enum.GetName(typeof(SensorUpdateInterval), updateInterval));
			}

			_sensorUpdateInterval = updateInterval;
		}

		internal override RotationSensorSource GetRotationSource()
		{
			return _rotationSource;
		}

		internal override void SetRotationSource(RotationSensorSource source)
		{
			// N.B. This has no affect on the simulated data.

			if (_connectedDevice == null)
			{
				Debug.LogWarning(WearableConstants.SetRotationSourceWithoutDeviceWarning);
				return;
			}

			if (_verbose)
			{
				Debug.LogFormat(
					WearableConstants.DebugProviderSetRotationSource,
					Enum.GetName(typeof(RotationSensorSource), source));
			}

			Debug.Log(WearableConstants.DebugProviderRotationSourceUnsupportedInfo);
			_rotationSource = source;
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

			if (_verbose)
			{
				Debug.LogFormat(WearableConstants.DebugProviderStartSensor, Enum.GetName(typeof(SensorId), sensorId));
			}

			_sensorStatus[sensorId] = true;
			_nextSensorUpdateTime = Time.unscaledTime;
		}

		internal override void StopSensor(SensorId sensorId)
		{
			if (!_sensorStatus[sensorId])
			{
				return;
			}

			if (_verbose)
			{
				Debug.LogFormat(WearableConstants.DebugProviderStopSensor, Enum.GetName(typeof(SensorId), sensorId));
			}

			_sensorStatus[sensorId] = false;
		}

		internal override bool GetSensorActive(SensorId sensorId)
		{
			return (_connectedDevice != null) && _sensorStatus[sensorId];
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

			if (_verbose)
			{
				Debug.LogFormat(WearableConstants.DebugProviderEnableGesture, Enum.GetName(typeof(GestureId), gestureId));
			}

			_gestureStatus[gestureId] = true;
		}

		internal override void DisableGesture(GestureId gestureId)
		{
			if (!_gestureStatus[gestureId])
			{
				return;
			}

			if (_verbose)
			{
				Debug.LogFormat(WearableConstants.DebugProviderDisableGesture, Enum.GetName(typeof(GestureId), gestureId));
			}

			_gestureStatus[gestureId] = false;
		}

		internal override bool GetGestureEnabled(GestureId gestureId)
		{
			return (_connectedDevice != null) && _gestureStatus[gestureId];
		}

		internal override void OnInitializeProvider()
		{
			base.OnInitializeProvider();

			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderInit);
			}
		}

		internal override void OnDestroyProvider()
		{
			base.OnDestroyProvider();

			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderDestroy);
			}
		}

		internal override void OnEnableProvider()
		{
			base.OnEnableProvider();

			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderEnable);
			}

			_nextSensorUpdateTime = Time.unscaledTime;
			_pendingGestures.Clear();
		}

		internal override void OnDisableProvider()
		{
			base.OnDisableProvider();

			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderDisable);
			}
		}

		internal override void OnUpdate()
		{
			UpdateVirtualDeviceInfo();

			if (!_enabled)
			{
				return;
			}

			// Clear the current frames; _lastSensorFrame will retain its previous value.
			if (_currentSensorFrames.Count > 0)
			{
				_currentSensorFrames.Clear();
			}

			if (_connectedDevice == null)
			{
				return;
			}

			while (Time.unscaledTime >= _nextSensorUpdateTime)
			{
				// If it's time to emit frames, do so until we have caught up.
				float deltaTime = WearableTools.SensorUpdateIntervalToSeconds(_sensorUpdateInterval);
				_nextSensorUpdateTime += deltaTime;

				// Check if sensors need to be updated
				bool anySensorsEnabled = false;
				for (int i = 0; i < WearableConstants.SensorIds.Length; i++)
				{
					if (_sensorStatus[WearableConstants.SensorIds[i]])
					{
						anySensorsEnabled = true;
						break;
					}
				}

				// Emit a gesture if needed
				bool gestureEmitted = UpdateGestureData();

				if (anySensorsEnabled || gestureEmitted)
				{
					// Update the timestamp and delta-time
					_lastSensorFrame.deltaTime = deltaTime;
					_lastSensorFrame.timestamp = _nextSensorUpdateTime;
				}

				if (anySensorsEnabled)
				{
					if (_simulateMovement)
					{
						// Calculate rotation, which is used by all sensors.
						if (_rotationType == RotationType.Euler)
						{
							_rotation = Quaternion.Euler(_eulerSpinRate * _lastSensorFrame.timestamp);
						}
						else if (_rotationType == RotationType.AxisAngle)
						{
							_rotation = Quaternion.AngleAxis(
								_axisAngleSpinRate.w * _lastSensorFrame.timestamp,
								new Vector3(_axisAngleSpinRate.x, _axisAngleSpinRate.y, _axisAngleSpinRate.z).normalized);
						}
					}
					else
					{
						_rotation = Quaternion.identity;
					}

					// Update all active sensors, even if motion is not simulated
					if (_sensorStatus[SensorId.Accelerometer])
					{
						UpdateAccelerometerData();
					}

					if (_sensorStatus[SensorId.Gyroscope])
					{
						UpdateGyroscopeData();
					}

					if (_sensorStatus[SensorId.Rotation])
					{
						UpdateRotationSensorData();
					}
				}

				// Emit the frame if needed
				if (anySensorsEnabled || gestureEmitted)
				{
					_currentSensorFrames.Add(_lastSensorFrame);
					OnSensorsOrGestureUpdated(_lastSensorFrame);
				}
			}
		}

		#endregion

		#region Private

		[SerializeField]
		private string _name;

		[SerializeField]
		private string _firmwareVersion;

		[SerializeField]
		private int _rssi;

		[SerializeField]
		private ProductId _productId;

		[SerializeField]
		private byte _variantId;

		[SerializeField]
		private string _uid;

		[SerializeField]
		private bool _verbose;

		[SerializeField]
		private bool _simulateMovement;

		[SerializeField]
		private Vector3 _eulerSpinRate;

		[SerializeField]
		private Vector4 _axisAngleSpinRate;

		[SerializeField]
		private RotationType _rotationType;

		private Quaternion _rotation;
		private readonly Queue<GestureId> _pendingGestures;

		private readonly Dictionary<SensorId, bool> _sensorStatus;
		private SensorUpdateInterval _sensorUpdateInterval;
		private RotationSensorSource _rotationSource;
		private float _nextSensorUpdateTime;

		private readonly Dictionary<GestureId, bool> _gestureStatus;

		private Device _virtualDevice;

		internal WearableDebugProvider()
		{
			_virtualDevice = new Device
			{
				name = _name,
				firmwareVersion = _firmwareVersion,
				rssi = _rssi,
				uid = _uid,
				productId = _productId,
				variantId = _variantId
			};

			_name = WearableConstants.DebugProviderDefaultDeviceName;
			_firmwareVersion = WearableConstants.DefaultFirmwareVersion;
			_rssi = WearableConstants.DebugProviderDefaultRSSI;
			_uid = WearableConstants.DebugProviderDefaultUID;
			_productId = WearableConstants.DebugProviderDefaultProductId;
			_variantId = WearableConstants.DebugProviderDefaultVariantId;

			_verbose = true;

			_eulerSpinRate = Vector3.zero;
			_axisAngleSpinRate = Vector3.up;

			_sensorStatus = new Dictionary<SensorId, bool>();
			_sensorUpdateInterval = WearableConstants.DefaultUpdateInterval;

			_rotationSource = WearableConstants.DefaultRotationSource;


			_sensorStatus.Add(SensorId.Accelerometer, false);
			_sensorStatus.Add(SensorId.Gyroscope, false);
			_sensorStatus.Add(SensorId.Rotation, false);

			// All gestures start disabled.
			_gestureStatus = new Dictionary<GestureId, bool>();
			for (var i = 0; i < WearableConstants.GestureIds.Length; i++)
			{
				if (WearableConstants.GestureIds[i] == GestureId.None)
				{
					continue;
				}

				_gestureStatus.Add(WearableConstants.GestureIds[i], false);
			}

			_pendingGestures = new Queue<GestureId>();

			_nextSensorUpdateTime = 0.0f;
			_rotation = Quaternion.identity;
		}

		private void UpdateVirtualDeviceInfo()
		{
			_virtualDevice.name = _name;
			_virtualDevice.firmwareVersion = _firmwareVersion;
			_virtualDevice.rssi = _rssi;
			_virtualDevice.uid = _uid;
			_virtualDevice.productId = _productId;
			_virtualDevice.variantId = _variantId;
		}

		/// <summary>
		/// Simulate some acceleration data.
		/// </summary>
		private void UpdateAccelerometerData()
		{
			Quaternion invRot = new Quaternion(-_rotation.x, -_rotation.y, -_rotation.z, _rotation.w);
			_lastSensorFrame.acceleration.value = invRot * new Vector3(0.0f, 9.80665f, 0.0f);
			_lastSensorFrame.acceleration.accuracy = SensorAccuracy.High;
		}

		/// <summary>
		/// Simulate some gyro data.
		/// </summary>
		private void UpdateGyroscopeData()
		{

			Quaternion invRot = new Quaternion(-_rotation.x, -_rotation.y, -_rotation.z, _rotation.w);
			_lastSensorFrame.angularVelocity.value = invRot * (_eulerSpinRate * Mathf.Deg2Rad);
			_lastSensorFrame.angularVelocity.accuracy = SensorAccuracy.High;
		}

		/// <summary>
		/// Simulate some rotation data.
		/// </summary>
		private void UpdateRotationSensorData()
		{
			// This is already calculated for us since the other sensors need it too.
			_lastSensorFrame.rotation.value = _rotation;
			_lastSensorFrame.rotation.measurementUncertainty = 0.0f;
		}

		/// <summary>
		/// Simulate some gesture data.
		/// </summary>
		/// <returns>True if a gesture was generated, else false</returns>
		private bool UpdateGestureData()
		{
			if (_pendingGestures.Count > 0)
			{
				GestureId gesture = _pendingGestures.Dequeue();
				if (_gestureStatus[gesture])
				{
					// If the gesture is enabled, go ahead and trigger it.
					if (_verbose)
					{
						Debug.LogFormat(WearableConstants.DebugProviderTriggerGesture, Enum.GetName(typeof(GestureId), gesture));
					}

					_lastSensorFrame.gestureId = gesture;
					return true;
				}
				else
				{
					// Otherwise, warn, and drop the gesture from the queue.
					Debug.LogWarning(WearableConstants.DebugProviderTriggerDisabledGestureWarning);
				}
			}

			_lastSensorFrame.gestureId = GestureId.None;
			return false;
		}

		#endregion
	}
}
