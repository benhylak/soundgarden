using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bose.Wearable
{
	/// <summary>
	/// A provider that simulates a Wearable Device using the IMU built into a mobile device. In addition to on-device
	/// builds, this also works in the editor via the Unity Remote app for easier prototyping. All coördinates are
	/// relative to the same frame and have the same units as the Wearable Device.
	/// </summary>
	[Serializable]
	public sealed class WearableMobileProvider : WearableProviderBase
	{
		#region Provider Unique

		public void SimulateDisconnect()
		{
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
			if (onDevicesUpdated != null)
			{
				onDevicesUpdated.Invoke(new []{_virtualDevice});
			}
		}

		internal override void StopSearchingForDevices()
		{

		}

		internal override void ConnectToDevice(Device device, Action onSuccess, Action onFailure)
		{
			DisconnectFromDevice();

			if (device != _virtualDevice)
			{
				Debug.LogWarning(WearableConstants.DebugProviderInvalidConnectionWarning);
				return;
			}


			OnDeviceConnecting(_virtualDevice);

			_virtualDevice.isConnected = true;
			_connectedDevice = _virtualDevice;
			_nextSensorUpdateTime = Time.unscaledTime;

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

			_virtualDevice.isConnected = false;
			OnDeviceDisconnected(_connectedDevice.Value);

			_connectedDevice = null;
		}

		internal override void SetSensorUpdateInterval(SensorUpdateInterval updateInterval)
		{
			if (_connectedDevice == null)
			{
				Debug.LogWarning(WearableConstants.SetUpdateRateWithoutDeviceWarning);
				return;
			}

			_sensorUpdateInterval = updateInterval;
		}

		internal override SensorUpdateInterval GetSensorUpdateInterval()
		{
			return _sensorUpdateInterval;
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

			Debug.Log(WearableConstants.MobileProviderRotationSourceUnsupportedInfo);
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

			_sensorStatus[sensorId] = true;
			_nextSensorUpdateTime = Time.unscaledTime;
		}

		internal override void StopSensor(SensorId sensorId)
		{
			if (!_sensorStatus[sensorId])
			{
				return;
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

			_gestureStatus[gestureId] = true;
		}

		internal override void DisableGesture(GestureId gestureId)
		{
			if (!_gestureStatus[gestureId])
			{
				return;
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

			// Must be done here not in the constructor to avoid a serialization error.
			_gyro = Input.gyro;
		}

		internal override void OnEnableProvider()
		{
			base.OnEnableProvider();

			_wasGyroEnabled = _gyro.enabled;
			_gyro.enabled = true;
			_nextSensorUpdateTime = Time.unscaledTime;
			_pendingGestures.Clear();
		}

		internal override void OnDisableProvider()
		{
			base.OnDisableProvider();

			_gyro.enabled = _wasGyroEnabled;
		}

		internal override void OnUpdate()
		{
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


				bool anySensorsEnabled = false;

				// Update all active sensors
				if (_sensorStatus[SensorId.Accelerometer])
				{
					UpdateAccelerometerData();
					anySensorsEnabled = true;
				}

				if (_sensorStatus[SensorId.Gyroscope])
				{
					UpdateGyroscopeData();
					anySensorsEnabled = true;
				}

				if (_sensorStatus[SensorId.Rotation])
				{
					UpdateRotationSensorData();
					anySensorsEnabled = true;
				}

				// Emit a gesture if needed
				bool gestureEmitted = UpdateGestureData();

				if (anySensorsEnabled || gestureEmitted)
				{
					// Update the timestamp and delta-time and emit
					_lastSensorFrame.deltaTime = deltaTime;
					_lastSensorFrame.timestamp = _nextSensorUpdateTime;

					_currentSensorFrames.Add(_lastSensorFrame);
					OnSensorsOrGestureUpdated(_lastSensorFrame);
				}
			}
		}

		#endregion

		#region Private

		private Gyroscope _gyro;
		private bool _wasGyroEnabled;

		private readonly Dictionary<SensorId, bool> _sensorStatus;
		private SensorUpdateInterval _sensorUpdateInterval;
		private RotationSensorSource _rotationSource;
		private float _nextSensorUpdateTime;

		// Gestures
		private readonly Dictionary<GestureId, bool> _gestureStatus;
		private readonly Queue<GestureId> _pendingGestures;

		private Device _virtualDevice;

		internal WearableMobileProvider()
		{
			_virtualDevice = new Device
			{
				isConnected = false,
				name = WearableConstants.MobileProviderDeviceName,
				firmwareVersion = WearableConstants.DefaultFirmwareVersion,
				productId = WearableConstants.MobileProviderProductId,
				variantId = WearableConstants.MobileProviderVariantId,
				rssi = 0,
				uid = WearableConstants.EmptyUID
			};

			_sensorStatus = new Dictionary<SensorId, bool>();
			_sensorUpdateInterval = WearableConstants.DefaultUpdateInterval;
			_nextSensorUpdateTime = 0.0f;

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
		}

		/// <summary>
		/// Copy over the mobile device's acceleration to the cached sensor frame, switching from right- to left-handed coördinates
		/// </summary>
		private void UpdateAccelerometerData()
		{
			Vector3 raw = Input.acceleration;
			_lastSensorFrame.acceleration.value.Set(-raw.x, -raw.y, raw.z);
			_lastSensorFrame.acceleration.accuracy = SensorAccuracy.High;
		}

		/// <summary>
		/// Copy over the mobile device's angular velocity to the cached sensor frame, switching from right- to left-handed coördinates
		/// </summary>
		private void UpdateGyroscopeData()
		{
			Vector3 raw = _gyro.rotationRate;
			_lastSensorFrame.angularVelocity.value.Set(-raw.x, -raw.y, raw.z);
			_lastSensorFrame.angularVelocity.accuracy = SensorAccuracy.High;
		}

		/// <summary>
		/// Copy over the mobile device's orientation data to the cached sensor frame, changing frames of reference as needed.
		/// </summary>
		private void UpdateRotationSensorData()
		{
			// This is based on an iPhone 6, but should be cross-compatible with other devices.
			Quaternion raw = _gyro.attitude;
			const float InverseRootTwo = 0.7071067812f; // 1 / sqrt(2)
			_lastSensorFrame.rotation.value = new Quaternion(
				InverseRootTwo * (raw.w - raw.x),
				InverseRootTwo * -(raw.y + raw.z),
				InverseRootTwo * (raw.z - raw.y),
				InverseRootTwo * (raw.w + raw.x)
			);
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
					_lastSensorFrame.gestureId = gesture;
					return true;
				}
			}

			_lastSensorFrame.gestureId = GestureId.None;
			return false;
		}

		#endregion
	}
}
