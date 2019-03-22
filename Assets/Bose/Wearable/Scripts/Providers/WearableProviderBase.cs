using System;
using System.Collections.Generic;

namespace Bose.Wearable
{
	[Serializable]
	public abstract class WearableProviderBase
	{
		/// <summary>
		/// Invoked when an attempt is made to connect to a device
		/// </summary>
		internal event Action<Device> DeviceConnecting;

		/// <summary>
		/// Invoked when a device has been successfully connected.
		/// </summary>
		internal event Action<Device> DeviceConnected;

		/// <summary>
		/// Invoked when a device has disconnected.
		/// </summary>
		internal event Action<Device> DeviceDisconnected;

		/// <summary>
		/// Invoked when there are sensor or gesture updates from the Wearable device.
		/// </summary>
		internal event Action<SensorFrame> SensorsOrGestureUpdated;

		/// <summary>
		/// Whether or not the provider has been initialized
		/// </summary>
		internal bool Initialized
		{
			get { return _initialized; }
		}

		protected bool _initialized;

		/// <summary>
		/// Whether or not the provider is enabled
		/// </summary>
		internal bool Enabled
		{
			get { return _enabled; }
		}

		protected bool _enabled;

		/// <summary>
		/// The last reported value for the sensor.
		/// </summary>
		internal SensorFrame LastSensorFrame
		{
			get { return _lastSensorFrame; }
		}

		protected SensorFrame _lastSensorFrame;

		/// <summary>
		/// An list of SensorFrames returned from the plugin bridge in order from oldest to most recent.
		/// </summary>
		internal List<SensorFrame> CurrentSensorFrames
		{
			get { return _currentSensorFrames; }
		}

		protected List<SensorFrame> _currentSensorFrames;

		/// <summary>
		/// The Wearable device that is currently connected in Unity.
		/// </summary>
		internal Device? ConnectedDevice
		{
			get { return _connectedDevice; }
		}

		protected Device? _connectedDevice;

		/// <summary>
		/// Searches for all Wearable devices that can be connected to.
		/// </summary>
		/// <param name="onDevicesUpdated"></param>
		internal abstract void SearchForDevices(Action<Device[]> onDevicesUpdated);

		/// <summary>
		/// Stops searching for Wearable devices that can be connected to.
		/// </summary>
		internal abstract void StopSearchingForDevices();

		/// <summary>
		/// Connects to a specified device and invokes either <paramref name="onSuccess"/> or <paramref name="onFailure"/>
		/// depending on the result.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="onSuccess"></param>
		/// <param name="onFailure"></param>
		internal abstract void ConnectToDevice(Device device, Action onSuccess, Action onFailure);

		/// <summary>
		/// Stops all attempts to connect to or monitor a device and disconnects from a device if connected.
		/// </summary>
		internal abstract void DisconnectFromDevice();

		/// <summary>
		/// Set the update interval of all sensors on the provider.
		/// </summary>
		/// <param name="updateInterval"></param>
		internal abstract void SetSensorUpdateInterval(SensorUpdateInterval updateInterval);

		/// <summary>
		/// Get the update interval of all sensors on the provider.
		/// </summary>
		internal abstract SensorUpdateInterval GetSensorUpdateInterval();
		
		/// <summary>
		/// Set the data source for the rotation sensor.
		/// </summary>
		/// <param name="source"></param>
		internal abstract void SetRotationSource(RotationSensorSource source);

		/// <summary>
		/// Get the data source for the rotation sensor.
		/// </summary>
		/// <returns></returns>
		internal abstract RotationSensorSource GetRotationSource();

		/// <summary>
		/// Start a sensor with a given <see cref="SensorId"/>. Providers should override this method.
		/// </summary>
		/// <param name="sensorId"></param>
		internal abstract void StartSensor(SensorId sensorId);

		/// <summary>
		/// Stop a sensor with a given <see cref="SensorId"/>. Providers should override this method.
		/// </summary>
		/// <param name="sensorId"></param>
		internal abstract void StopSensor(SensorId sensorId);

		/// <summary>
		/// Returns whether or not a sensor with a given <see cref="SensorId"/> is active.
		/// Providers should override this method.
		/// </summary>
		/// <param name="sensorId"></param>
		/// <returns></returns>
		internal abstract bool GetSensorActive(SensorId sensorId);

		/// <summary>
		/// Start a Gesture with a given <see cref="GestureId"/>. Will never be called with None.
		/// </summary>
		/// <param name="gestureId"></param>
		internal abstract void EnableGesture(GestureId gestureId);

		/// <summary>
		/// Stop a Gesture with a given <see cref="GestureId"/>. Will never be called with None.
		/// </summary>
		/// <param name="gestureId"></param>
		internal abstract void DisableGesture(GestureId gestureId);

		/// <summary>
		/// Returns whether or not a gesture with a given <see cref="GestureId"/> is enabled. Will never be called with None.
		/// </summary>
		/// <param name="gestureId"></param>
		/// <returns></returns>
		internal abstract bool GetGestureEnabled(GestureId gestureId);

		/// <summary>
		/// Called by <see cref="WearableControl"/> when the provider is first initialized.
		/// Providers must call <code>base.OnInitializeProvider()</code> when overriding to update internal state.
		/// </summary>
		internal virtual void OnInitializeProvider()
		{
			_initialized = true;
			_enabled = false;
		}

		/// <summary>
		/// Called by <see cref="WearableControl"/> when the provider is destroyed at application quit.
		/// Providers must call <code>base.OnDestroyProvider()</code> when overriding to update internal state.
		/// </summary>
		internal virtual void OnDestroyProvider()
		{
			_initialized = false;
			_enabled = false;
		}

		/// <summary>
		/// Called by <see cref="WearableControl"/> when the provider is being enabled.
		/// Automatically invokes <see cref="OnDeviceConnected"/> if a device is still connected.
		/// Providers must call <code>base.OnEnableProvider()</code> when overriding to update internal state.
		/// </summary>
		internal virtual void OnEnableProvider()
		{
			_enabled = true;

			if (_connectedDevice != null)
			{
				OnDeviceConnected(_connectedDevice.Value);
			}
		}

		/// <summary>
		/// Called by <see cref="WearableControl"/> when the provider is being disabled.
		/// Automatically invokes <see cref="OnDeviceDisconnected"/> if a device is still connected.
		/// Providers must call <code>base.OnDisableProvider()</code> when overriding to update internal state.
		/// </summary>
		internal virtual void OnDisableProvider()
		{
			_enabled = false;

			if (_connectedDevice != null)
			{
				OnDeviceDisconnected(_connectedDevice.Value);
			}
		}

		/// <summary>
		/// Called by <see cref="WearableControl"/> during the appropriate Unity update method if the provider is active.
		/// </summary>
		internal abstract void OnUpdate();

		protected WearableProviderBase()
		{
			_currentSensorFrames = new List<SensorFrame>();
			_lastSensorFrame = WearableConstants.EmptyFrame;
		}

		/// <summary>
		/// Invokes the <see cref="DeviceConnecting"/> event.
		/// </summary>
		protected void OnDeviceConnecting(Device device)
		{
			if (DeviceConnecting != null)
			{
				DeviceConnecting.Invoke(device);
			}
		}

		/// <summary>
		/// Invokes the <see cref="DeviceConnected"/> event.
		/// </summary>
		/// <param name="device"></param>
		protected void OnDeviceConnected(Device device)
		{
			if (DeviceConnected != null)
			{
				DeviceConnected.Invoke(device);
			}
		}

		/// <summary>
		/// Invokes the <see cref="DeviceDisconnected"/> event.
		/// </summary>
		/// <param name="device"></param>
		protected void OnDeviceDisconnected(Device device)
		{
			if (DeviceDisconnected != null)
			{
				DeviceDisconnected.Invoke(device);
			}
		}

		/// <summary>
		/// Invokes the <see cref="SensorsOrGestureUpdated"/> event.
		/// </summary>
		/// <param name="frame"></param>
		protected void OnSensorsOrGestureUpdated(SensorFrame frame)
		{
			if (SensorsOrGestureUpdated != null)
			{
				SensorsOrGestureUpdated.Invoke(frame);
			}
		}
	}
}
