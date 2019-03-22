using System;

namespace Bose.Wearable
{
	/// <summary>
	/// Represents the state of a WearableDevice including its sensors and gestures.
	/// </summary>
	[Serializable]
	internal class WearableDeviceConfig
	{
		/// <summary>
		/// Represents the state of a WearableDevice's gesture
		/// </summary>
		[Serializable]
		internal class WearableGestureConfig
		{
			public bool isEnabled;
		}

		/// <summary>
		/// Represents the state of a WearableDevice's sensor
		/// </summary>
		[Serializable]
		internal class WearableSensorConfig
		{
			public bool isEnabled;
		}

		public WearableSensorConfig accelerometer;

		public WearableSensorConfig gyroscope;

		public WearableSensorConfig rotation;

		public WearableGestureConfig doubleTap;

		public WearableGestureConfig headNod;

		public WearableGestureConfig headShake;

		public SensorUpdateInterval updateInterval;

		public RotationSensorSource rotationSource;

		public WearableDeviceConfig()
		{
			accelerometer = new WearableSensorConfig();
			gyroscope = new WearableSensorConfig();
			rotation = new WearableSensorConfig();

			doubleTap = new WearableGestureConfig();
			headNod = new WearableGestureConfig();
			headShake = new WearableGestureConfig();

			updateInterval = WearableConstants.DefaultUpdateInterval;
			rotationSource = WearableConstants.DefaultRotationSource;
		}

		/// <summary>
		/// Returns an appropriate <see cref="WearableSensorConfig"/> for the passed <see cref="SensorId"/>
		/// <paramref name="sensorId"/>
		/// </summary>
		/// <param name="sensorId"></param>
		/// <returns></returns>
		public WearableSensorConfig GetSensorConfig(SensorId sensorId)
		{
			WearableSensorConfig config;
			switch (sensorId)
			{
				case SensorId.Accelerometer:
					config = accelerometer;
					break;
				case SensorId.Gyroscope:
					config = gyroscope;
					break;
				case SensorId.Rotation:
					config = rotation;
					break;
				default:
					throw new ArgumentOutOfRangeException("sensorId", sensorId, null);
			}

			return config;
		}

		/// <summary>
		/// Returns an appropriate <see cref="WearableGestureConfig"/> for the passed <see cref="GestureId"/>
		/// <paramref name="gestureId"/>
		/// </summary>
		/// <param name="gestureId"></param>
		/// <returns></returns>
		public WearableGestureConfig GetGestureConfig(GestureId gestureId)
		{
			WearableGestureConfig config;
			switch (gestureId)
			{
				case GestureId.DoubleTap:
					config = doubleTap;
					break;
				case GestureId.HeadNod:
					config = headNod;
					break;
				case GestureId.HeadShake:
					config = headShake;
					break;
				case GestureId.None:
				default:
					throw new ArgumentOutOfRangeException("gestureId", gestureId, null);
			}

			return config;
		}

		/// <summary>
		/// Returns true if any sensors are enabled.
		/// </summary>
		/// <returns></returns>
		internal bool HasAnySensorsEnabled()
		{
			return GetNumberOfEnabledSensors() > 0;
		}

		/// <summary>
		/// Returns true if three or more sensors are enabled.
		/// </summary>
		/// <returns></returns>
		internal bool HasThreeOrMoreSensorsEnabled()
		{
			return GetNumberOfEnabledSensors() >= 3;
		}

		/// <summary>
		/// Returns the number of sensor configs that are enabled.
		/// </summary>
		/// <returns></returns>
		private int GetNumberOfEnabledSensors()
		{
			var numberOfSensorsActive = 0;
			for (var i = 0; i < WearableConstants.SensorIds.Length; i++)
			{
				var sensor = GetSensorConfig(WearableConstants.SensorIds[i]);
				if (!sensor.isEnabled)
				{
					continue;
				}

				numberOfSensorsActive++;
			}

			return numberOfSensorsActive;
		}
	}
}
