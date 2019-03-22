using System;
using UnityEngine;

namespace Bose.Wearable
{
	/// <summary>
	/// <see cref="WearableRequirement"/> allows for configuration of a WearableDevice from the inspector. All
	/// <see cref="WearableRequirement"/> instances will be summed into a final configuration that represents the
	/// minimum required state for a WearableDevice. This helps to support lower-battery usage by the device and
	/// prevents clobbering of desired state from multiple systems that have different WearableDevice Sensor and
	/// Gesture configurations.
	/// </summary>
	[AddComponentMenu("Bose/Wearable/WearableRequirement")]
	public sealed class WearableRequirement : MonoBehaviour
	{
		internal event Action Updated;
		
		[SerializeField]
		private WearableDeviceConfig _wearableDeviceConfig;
		
		private void Start()
		{
			if (WearableControl.Instance != null)
			{
				WearableControl.Instance.RegisterRequirement(this);
			}
		}

		private void OnEnable()
		{
			if (WearableControl.Instance != null)
			{
				WearableControl.Instance.RegisterRequirement(this);
			}
		}

		private void OnDisable()
		{
			if (WearableControl.Instance != null)
			{
				WearableControl.Instance.UnregisterRequirement(this);
			}
		}

		/// <summary>
		/// The <see cref="WearableDeviceConfig"/> for this <see cref="WearableRequirement"/>.
		/// </summary>
		internal WearableDeviceConfig DeviceConfig
		{
			get
			{
				if (_wearableDeviceConfig == null)
				{
					_wearableDeviceConfig = new WearableDeviceConfig();
				}

				return _wearableDeviceConfig;
			}
		}

		/// <summary>
		/// Sets the sensor configuration for <see cref="SensorId"/> <paramref name="sensorId"/> to be enabled.
		/// </summary>
		/// <param name="sensorId"></param>
		public void EnableSensor(SensorId sensorId)
		{
			var sensor = DeviceConfig.GetSensorConfig(sensorId);
			if (sensor.isEnabled)
			{
				return;
			}

			sensor.isEnabled = true;

			SetDirty();
		}

		/// <summary>
		/// Sets the sensor configuration for <see cref="SensorId"/> <paramref name="sensorId"/> to be disabled.
		/// </summary>
		/// <param name="sensorId"></param>
		public void DisableSensor(SensorId sensorId)
		{
			var sensor = DeviceConfig.GetSensorConfig(sensorId);
			if (!sensor.isEnabled)
			{
				return;
			}

			sensor.isEnabled = false;

			SetDirty();
		}

		/// <summary>
		/// Sets the gesture configuration for <see cref="GestureId"/> <paramref name="gestureId"/> to be enabled.
		/// </summary>
		/// <param name="gestureId"></param>
		public void EnableGesture(GestureId gestureId)
		{
			var gesture = DeviceConfig.GetGestureConfig(gestureId);
			if (gesture.isEnabled)
			{
				return;
			}

			gesture.isEnabled = true;

			SetDirty();
		}

		/// <summary>
		/// Sets the gesture configuration for <see cref="GestureId"/> <paramref name="gestureId"/> to be disabled.
		/// </summary>
		/// <param name="gestureId"></param>
		public void DisableGesture(GestureId gestureId)
		{
			var gesture = DeviceConfig.GetGestureConfig(gestureId);
			if (!gesture.isEnabled)
			{
				return;
			}

			gesture.isEnabled = false;

			SetDirty();
		}

		/// <summary>
		/// Sets the <see cref="SensorUpdateInterval"/> <paramref name="updateInterval"/> value.
		/// </summary>
		/// <param name="updateInterval"></param>
		public void SetSensorUpdateInterval(SensorUpdateInterval updateInterval)
		{
			if (DeviceConfig.updateInterval == updateInterval)
			{
				return;
			}

			DeviceConfig.updateInterval = updateInterval;

			SetDirty();
		}

		/// <summary>
		/// Sets the <see cref="RotationSensorSource"/> <paramref name="rotSource"/> value.
		/// </summary>
		/// <param name="rotSource"></param>
		public void SetRotationSensorSource(RotationSensorSource rotSource)
		{
			if (DeviceConfig.rotationSource == rotSource)
			{
				return;
			}

			DeviceConfig.rotationSource = rotSource;

			SetDirty();
		}

		internal void SetDirty()
		{
			if (Updated != null)
			{
				Updated.Invoke();
			}
		}
	}
}
