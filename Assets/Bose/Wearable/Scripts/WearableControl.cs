using System;
using System.Collections.Generic;
using Bose.Wearable.Extensions;
using Bose.Wearable.Proxy;
using UnityEngine;

namespace Bose.Wearable
{
	[AddComponentMenu("Bose/Wearable/WearableControl")]
	public sealed class WearableControl : Singleton<WearableControl>
	{
		/// <summary>
		/// Represents a sensor available to the WearablePlugin.
		/// </summary>
		public sealed class WearableSensor
		{
			/// <summary>
			/// Returns true or false depending on whether or not the sensor is enabled and
			/// retrieving updates.
			/// </summary>
			public bool IsActive
			{
				get { return _wearableControl.GetSensorActive(_id); }
			}

			private readonly WearableControl _wearableControl;
			private readonly SensorId _id;

			internal WearableSensor(WearableControl wearableControl, SensorId id)
			{
				_wearableControl = wearableControl;
				_id = id;
			}

			public void Start()
			{
				_wearableControl.StartSensor(_id);
			}

			public void Stop()
			{
				_wearableControl.StopSensor(_id);
			}
		}

		/// <summary>
		/// Represents a Gesture available to the WearablePlugin.
		/// </summary>
		public sealed class WearableGesture
		{
			/// <summary>
			/// Returns true or false depending on whether or not the Gesture is enabled and
			/// retrieving updates.
			/// </summary>
			public bool IsActive
			{
				get { return _wearableControl.GetGestureEnabled(_gestureId); }
			}

			private readonly WearableControl _wearableControl;
			private readonly GestureId _gestureId;

			internal WearableGesture(WearableControl wearableControl, GestureId gestureId)
			{
				_wearableControl = wearableControl;
				_gestureId = gestureId;
			}

			public void Enable()
			{
				_wearableControl.EnableGesture(_gestureId);
			}

			public void Disable()
			{
				_wearableControl.DisableGesture(_gestureId);
			}
		}

		#region Public API

		/// <summary>
		/// Invoked when an attempt is made to connect to a device
		/// </summary>
		public event Action<Device> DeviceConnecting;

		/// <summary>
		/// Invoked when a device has been successfully connected.
		/// </summary>
		public event Action<Device> DeviceConnected;

		/// <summary>
		/// Invoked when a device has disconnected.
		/// </summary>
		public event Action<Device> DeviceDisconnected;

		/// <summary>
		/// Invoked when there are sensor updates from the Wearable device.
		/// </summary>
		public event Action<SensorFrame> SensorsUpdated;

		/// <summary>
		/// Invoked when a sensor frame includes a gesture.
		/// </summary>
		public event Action<GestureId> GestureDetected;

		/// <summary>
		/// Invoked when a double-tap gesture has completed
		/// </summary>
		public event Action DoubleTapDetected;

		/// <summary>
		/// Invoked when a head shake gesture has completed
		/// </summary>
		public event Action HeadShakeDetected;

		/// <summary>
		/// Invoked when a head nod gesture has completed
		/// </summary>
		public event Action HeadNodDetected;

		/// <summary>
		/// The last reported value for the sensor.
		/// </summary>
		public SensorFrame LastSensorFrame
		{
			get { return _activeProvider.LastSensorFrame; }
		}

		/// <summary>
		/// An list of SensorFrames returned from the plugin bridge in order from oldest to most recent.
		/// </summary>
		public List<SensorFrame> CurrentSensorFrames
		{
			get { return _activeProvider.CurrentSensorFrames; }
		}

		/// <summary>
		/// The Accelerometer sensor on the Wearable device.
		/// </summary>
		public WearableSensor AccelerometerSensor
		{
			get { return _accelerometerSensor; }
		}

		private WearableSensor _accelerometerSensor;

		/// <summary>
		/// The Gyroscope sensor on the Wearable device.
		/// </summary>
		public WearableSensor GyroscopeSensor
		{
			get { return _gyroscopeSensor; }
		}

		private WearableSensor _gyroscopeSensor;

		/// <summary>
		/// The rotation sensor on the Wearable device.
		/// </summary>
		public WearableSensor RotationSensor
		{
			get { return _rotationSensor; }
		}

		private WearableSensor _rotationSensor;

		/// <summary>
		/// Get object for double-tap gesture.
		/// </summary>
		public WearableGesture DoubleTapGesture
		{
			get { return _wearableGestures[GestureId.DoubleTap]; }
		}

		/// <summary>
		/// Get object for head nod gesture
		/// </summary>
		public WearableGesture HeadNodGesture
		{
			get { return _wearableGestures[GestureId.HeadNod]; }
		}

		/// <summary>
		/// Get object for head shake gesture
		/// </summary>
		public WearableGesture HeadShakeGesture
		{
			get { return _wearableGestures[GestureId.HeadShake]; }
		}

		/// <summary>
		/// Returns a <see cref="WearableGesture"/> based on the passed <see cref="GestureId"/>.
		/// <paramref name="gestureId"/>
		/// </summary>
		/// <param name="gestureId"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public WearableGesture GetWearableGestureById(GestureId gestureId)
		{
			if (gestureId == GestureId.None)
			{
				throw new Exception(WearableConstants.GestureIdNoneInvalidError);
			}

			WearableGesture wearableGesture;
			if (_wearableGestures.TryGetValue(gestureId, out wearableGesture))
			{
				return wearableGesture;
			}

			throw new Exception(string.Format(WearableConstants.WearableGestureNotYetSupported, gestureId));
		}

		private Dictionary<GestureId, WearableGesture> _wearableGestures;

		/// <summary>
		/// The Wearable device that is currently connected in Unity.
		/// </summary>
		public Device? ConnectedDevice
		{
			get
			{
				// Safeguard against uninitialized active provider
				return _activeProvider == null ? null : _activeProvider.ConnectedDevice;
			}
		}

		/// <summary>
		/// Searches for all Wearable devices that can be connected to.
		/// </summary>
		/// <param name="onDevicesUpdated"></param>
		public void SearchForDevices(Action<Device[]> onDevicesUpdated)
		{
			_activeProvider.SearchForDevices(onDevicesUpdated);
		}

		/// <summary>
		/// Stops searching for Wearable devices that can be connected to.
		/// </summary>
		public void StopSearchingForDevices()
		{
			_activeProvider.StopSearchingForDevices();
		}

		/// <summary>
		/// Connects to a specified device and invokes either <paramref name="onSuccess"/> or <paramref name="onFailure"/>
		/// depending on the result.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="onSuccess"></param>
		/// <param name="onFailure"></param>
		public void ConnectToDevice(Device device, Action onSuccess = null, Action onFailure = null)
		{
			_activeProvider.ConnectToDevice(device, onSuccess, onFailure);
		}

		/// <summary>
		/// Stops all attempts to connect to or monitor a device and disconnects from a device if connected.
		/// </summary>
		public void DisconnectFromDevice()
		{
			_activeProvider.DisconnectFromDevice();
		}

		/// <summary>
		/// The update interval of all sensors on the active provider
		/// </summary>
		public SensorUpdateInterval UpdateInterval
		{
			get { return _activeProvider.GetSensorUpdateInterval(); }
		}

		/// <summary>
		/// Sets the update interval for the WearableDevice.
		/// </summary>
		public void SetSensorUpdateInterval(SensorUpdateInterval interval)
		{
			// If we've made a change that would result in the device's state changing,
			// mark the device config as dirty.
			if (SetSensorUpdateIntervalInternal(interval))
			{
				LockDeviceStateUpdate();
			}
		}

		/// <summary>
		/// The data source used by the rotation sensor
		/// </summary>
		public RotationSensorSource RotationSource
		{
			get { return _activeProvider.GetRotationSource(); }
		}

		/// <summary>
		/// Sets the data source used by the rotation sensor
		/// </summary>
		/// <param name="source"></param>
		public void SetRotationSource(RotationSensorSource source)
		{
			if (SetRotationSourceInternal(source))
			{
				LockDeviceStateUpdate();
			}
		}

		/// <summary>
		/// Set the update mode, determining when SensorFrame updates are polled and made available.
		/// </summary>
		/// <param name="unityUpdateMode"></param>
		public void SetUnityUpdateMode(UnityUpdateMode unityUpdateMode)
		{
			_updateMode = unityUpdateMode;
		}

		/// <summary>
		/// The Unity Update method sensor updates should be retrieved and dispatched on.
		/// </summary>
		public UnityUpdateMode UpdateMode
		{
			get { return _updateMode; }
		}

		[SerializeField]
		private UnityUpdateMode _updateMode;

		/// <summary>
		/// An instance of the currently-active provider for configuration
		/// </summary>
		public WearableProviderBase ActiveProvider
		{
			get { return _activeProvider; }
		}

		private WearableProviderBase _activeProvider;


		/// <summary>
		/// Set the active provider to a specific provider instance
		/// </summary>
		/// <param name="provider"></param>
		public void SetActiveProvider(WearableProviderBase provider)
		{
			// Uninitialized providers should never have OnEnable/Disable called
			if (_activeProvider != null)
			{
				if (_activeProvider.Initialized)
				{
					_activeProvider.OnDisableProvider();
				}

				// Unsubscribe after disabling in case OnDisableProvider invokes an event
				// Using an invocation method here rather than the event proper ensures that any events added or removed
				// after setting the provider will be accounted for.
				_activeProvider.DeviceConnecting -= OnDeviceConnecting;
				_activeProvider.DeviceConnected -= OnDeviceConnected;
				_activeProvider.DeviceDisconnected -= OnDeviceDisconnected;
				_activeProvider.SensorsOrGestureUpdated -= OnSensorsOrGestureUpdated;
			}

			_activeProvider = provider;

			// Initialize if this is the first time the provider is active
			if (!_activeProvider.Initialized)
			{
				_activeProvider.OnInitializeProvider();
			}

			// Subscribe to the provider's events
			_activeProvider.DeviceConnecting += OnDeviceConnecting;
			_activeProvider.DeviceConnected += OnDeviceConnected;
			_activeProvider.DeviceDisconnected += OnDeviceDisconnected;
			_activeProvider.SensorsOrGestureUpdated += OnSensorsOrGestureUpdated;

			// Enable the new provider after subscribing in case enabling the provider invokes an event
			_activeProvider.OnEnableProvider();
		}

		/// <summary>
		/// Set the active provider by provider type
		/// </summary>
		public void SetActiveProvider<T>()
			where T : WearableProviderBase
		{
			SetActiveProvider(GetOrCreateProvider<T>());
		}

		/// <summary>
		///  Returns a provider of the specified provider type for manipulation
		/// </summary>
		public T GetOrCreateProvider<T>()
			where T : WearableProviderBase
		{
			if (_debugProvider is T)
			{
				return (T)GetOrCreateProvider(ProviderId.DebugProvider);
			}
			else if (_deviceProvider is T)
			{
				return (T)GetOrCreateProvider(ProviderId.WearableDevice);
			}
			else if (_mobileProvider is T)
			{
				return (T)GetOrCreateProvider(ProviderId.MobileProvider);
			}
			else if (_usbProvider is T)
			{
				return (T)GetOrCreateProvider(ProviderId.USBProvider);
			}
			else if (_proxyProvider is T)
			{
				return (T)GetOrCreateProvider(ProviderId.WearableProxy);
			}
			else
			{
				throw new NotImplementedException();
			}
		}
		#endregion

		#region Private

		[SerializeField]
		private WearableDebugProvider _debugProvider;

		[SerializeField]
		private WearableDeviceProvider _deviceProvider;

		[SerializeField]
		private WearableMobileProvider _mobileProvider;

		[SerializeField]
		private WearableUSBProvider _usbProvider;

		[SerializeField]
		private WearableProxyProvider _proxyProvider;

		#pragma warning disable 0414
		[SerializeField]
		private ProviderId _editorDefaultProvider = ProviderId.MobileProvider;

		[SerializeField]
		private ProviderId _runtimeDefaultProvider = ProviderId.WearableDevice;
		#pragma warning restore 0414

		/// <summary>
		/// The wearable device config used for public methods on WearableControl that are intended to
		/// update device state.
		/// </summary>
		private WearableDeviceConfig _wearableDeviceConfig;

		/// <summary>
		/// The <see cref="WearableDeviceConfig"/> used as the resolved version after all registered requirements have been
		/// processed into a single config where enabled sensors/gestures are preferred over disabled ones and
		/// faster sensor update intervals are preferred over slower ones.
		/// </summary>
		[SerializeField]
		private WearableDeviceConfig _finalWearableDeviceConfig;

		/// <summary>
		/// The <see cref="WearableDeviceConfig"/> used to override the requirements resolved device config.
		/// </summary>
		internal WearableDeviceConfig OverrideDeviceConfig
		{
			get { return _overrideDeviceConfig; }
		}

		[SerializeField]
		private WearableDeviceConfig _overrideDeviceConfig;

		/// <summary>
		/// Returns true if an override <see cref="WearableDeviceConfig"/> is present, otherwise false.
		/// </summary>
		/// <returns></returns>
		public bool IsOverridingDeviceConfig
		{
			get { return _isOverridingDeviceConfig; }
		}

		private bool _isOverridingDeviceConfig;

		/// <summary>
		/// Have we made a device state update on this frame or a previous frame
		/// such that we consider it locked?
		/// </summary>
		private bool _isDeviceStateUpdateLocked;

		/// <summary>
		/// Have we applied a device state update this frame?
		/// </summary>
		private bool _hasDeviceUpdateBeenApplied;

		/// <summary>
		/// Is a device state update pending during the lockout?
		/// </summary>
		private bool _isDeviceStateUpdatePendingDuringLock;

		/// <summary>
		/// The time since the app started when we last updated the device config.
		/// </summary>
		private float _appTimeSinceDeviceStateUpdateLocked;

		// Reference Counting State. Initialized inline to support
		#pragma warning disable 0414
		private readonly List<WearableRequirement> _wearableRequirements = new List<WearableRequirement>();
		#pragma warning restore 0414

		private WearableProviderBase GetOrCreateProvider(ProviderId providerId)
		{
			switch (providerId)
			{
				case ProviderId.DebugProvider:
					if (_debugProvider == null)
					{
						_debugProvider = new WearableDebugProvider();
					}
					return _debugProvider;

				case ProviderId.WearableDevice:
					if (_deviceProvider == null)
					{
						_deviceProvider = new WearableDeviceProvider();
					}
					return _deviceProvider;

				case ProviderId.MobileProvider:
					if (_mobileProvider == null)
					{
						_mobileProvider = new WearableMobileProvider();
					}
					return _mobileProvider;

				case ProviderId.USBProvider:
					if (_usbProvider == null)
					{
						_usbProvider = new WearableUSBProvider();
					}

					return _usbProvider;

				case ProviderId.WearableProxy:
					if (_proxyProvider == null)
					{
						_proxyProvider = new WearableProxyProvider();
					}

					return _proxyProvider;

				default:
					throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Invokes the <see cref="DeviceConnecting"/> event.
		/// </summary>
		private void OnDeviceConnecting(Device device)
		{
			if (DeviceConnecting != null)
			{
				DeviceConnecting.Invoke(device);
			}
		}

		/// <summary>
		/// Immediately locks the device update capability and invokes the <see cref="DeviceConnected"/> event.
		/// </summary>
		/// <param name="device"></param>
		private void OnDeviceConnected(Device device)
		{
			// When a device has been connected, immediately clear any locks and then re-lock
			// so that we update the state of the device at the end of this frame or the next.
			UnlockDeviceStateUpdate();
			LockDeviceStateUpdate();

			if (DeviceConnected != null)
			{
				DeviceConnected.Invoke(device);
			}
		}

		/// <summary>
		/// Invokes the <see cref="DeviceDisconnected"/> event.
		/// </summary>
		/// <param name="device"></param>
		private void OnDeviceDisconnected(Device device)
		{
			if (DeviceDisconnected != null)
			{
				DeviceDisconnected.Invoke(device);
			}
		}

		/// <summary>
		/// Invokes the <see cref="SensorsUpdated"/> event.
		/// If the frame contains a gesture, also invokes the <see cref="GestureDetected"/> event.
		/// </summary>
		/// <param name="frame"></param>
		private void OnSensorsOrGestureUpdated(SensorFrame frame)
		{
			if (SensorsUpdated != null)
			{
				SensorsUpdated.Invoke(frame);
			}

			if (frame.gestureId != GestureId.None)
			{
				if (GestureDetected != null)
				{
					GestureDetected.Invoke(frame.gestureId);
				}

				switch (frame.gestureId)
				{
					case GestureId.DoubleTap:
						if (DoubleTapDetected != null)
						{
							DoubleTapDetected.Invoke();
						}
						break;
					case GestureId.HeadShake:
						if (HeadShakeDetected != null)
						{
							HeadShakeDetected.Invoke();
						}
						break;
					case GestureId.HeadNod:
						if (HeadNodDetected != null)
						{
							HeadNodDetected.Invoke();
						}
						break;
					case GestureId.None:
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		/// <summary>
		/// Sets the override <see cref="WearableDeviceConfig"/> config that will take priority over any
		/// requirement resolved <see cref="WearableDeviceConfig"/>.
		/// </summary>
		/// <param name="config"></param>
		internal void RegisterOverrideConfig(WearableDeviceConfig config)
		{
			_overrideDeviceConfig = config;
			_isOverridingDeviceConfig = true;

			LockDeviceStateUpdate();
		}

		/// <summary>
		/// Removes the override <see cref="WearableDeviceConfig"/> and triggers a pending device update.
		/// </summary>
		internal void UnregisterOverrideConfig()
		{
			// Don't trigger a pending update if there isn't a device config set for the override.
			if (!_isOverridingDeviceConfig)
			{
				return;
			}

			_isOverridingDeviceConfig = false;

			LockDeviceStateUpdate();
		}

		/// <summary>
		/// Registers the <see cref="WearableRequirement"/> <paramref name="requirement"/>
		/// </summary>
		/// <param name="requirement"></param>
		internal void RegisterRequirement(WearableRequirement requirement)
		{
			if (_wearableRequirements.Contains(requirement))
			{
				return;
			}

			requirement.Updated += LockDeviceStateUpdate;

			_wearableRequirements.Add(requirement);

			LockDeviceStateUpdate();
		}

		/// <summary>
		/// Unregisters the <see cref="WearableRequirement"/> <paramref name="requirement"/>
		/// </summary>
		/// <param name="requirement"></param>
		internal void UnregisterRequirement(WearableRequirement requirement)
		{
			if (!_wearableRequirements.Contains(requirement))
			{
				return;
			}

			requirement.Updated -= LockDeviceStateUpdate;

			_wearableRequirements.Remove(requirement);

			LockDeviceStateUpdate();
		}

		/// <summary>
		/// Sets the <see cref="SensorUpdateInterval"/> <paramref name="newInterval"/>.
		/// </summary>
		private bool SetSensorUpdateIntervalInternal(SensorUpdateInterval newInterval)
		{
			var hasDeviceStateChanged = false;
			if (_wearableDeviceConfig.updateInterval != newInterval)
			{
				_wearableDeviceConfig.updateInterval = newInterval;

				hasDeviceStateChanged = true;
			}

			return hasDeviceStateChanged;
		}

		/// <summary>
		/// Sets the <see cref="RotationSource"/> to <paramref name="newSource"/>
		/// </summary>
		/// <param name="newSource"></param>
		/// <returns></returns>
		private bool SetRotationSourceInternal(RotationSensorSource newSource)
		{
			bool hasDeviceStateChanged = false;
			if (_wearableDeviceConfig.rotationSource != newSource)
			{
				_wearableDeviceConfig.rotationSource = newSource;
				hasDeviceStateChanged = true;
			}

			return hasDeviceStateChanged;
		}

		/// <summary>
		/// Marks the device state update functionality to be unlocked such that device updates can take place again.
		/// </summary>
		private void UnlockDeviceStateUpdate()
		{
			_isDeviceStateUpdateLocked = false;
			_appTimeSinceDeviceStateUpdateLocked = Time.time;
			_hasDeviceUpdateBeenApplied = false;
		}

		/// <summary>
		/// Marks the device state update functionality as locked such that we have indicated a device state
		/// update needs to take place. If an update has already taken place, but is still currently locked,
		/// a pending update flag will be set instead to indicate another device state update is needed.
		/// </summary>
		private void LockDeviceStateUpdate()
		{
			var timeSinceLocked = Time.time - _appTimeSinceDeviceStateUpdateLocked;
			if (_isDeviceStateUpdateLocked && timeSinceLocked > 0f)
			{
				Debug.LogWarning(WearableConstants.OnlyOneSensorFrequencyUpdatePerFrameWarning, this);

				_isDeviceStateUpdatePendingDuringLock = true;
			}
			else
			{
				_isDeviceStateUpdateLocked = true;
				_hasDeviceUpdateBeenApplied = false;
				_appTimeSinceDeviceStateUpdateLocked = Time.time;
			}
		}

		/// <summary>
		/// Updates the device state for sensors and gestures via the current provider. This should only be
		/// done once per frame with an interval of frames/time between subsequent updates.
		/// </summary>
		private void UpdateDeviceFromConfig()
		{
			if (!ConnectedDevice.HasValue)
			{
				return;
			}

			// Resolve the final device state based on all requirements and device configs
			ResolveFinalDeviceConfig();

			// Get the appropriate WearableDeviceConfig
			var deviceConfig = _isOverridingDeviceConfig
				? _overrideDeviceConfig
				: _finalWearableDeviceConfig;

			// If the current device state is the same as our final resolved config, return and do not call
			// any native bridge code.
			if (!ShouldUpdateDeviceState(deviceConfig))
			{
				UnlockDeviceStateUpdate();
				return;
			}

			// Set the rotation source
			_activeProvider.SetRotationSource(deviceConfig.rotationSource);

			// If we have any sensors enabled, set the fastest-able desired speed.
			if (deviceConfig.HasAnySensorsEnabled())
			{
				_activeProvider.SetSensorUpdateInterval(deviceConfig.updateInterval);
			}

			// Update sensor on/off state
			for (var i = 0; i < WearableConstants.SensorIds.Length; i++)
			{
				var sensorConfig = deviceConfig.GetSensorConfig(WearableConstants.SensorIds[i]);
				if (sensorConfig.isEnabled)
				{
					_activeProvider.StartSensor(WearableConstants.SensorIds[i]);
				}
				else
				{
					_activeProvider.StopSensor(WearableConstants.SensorIds[i]);
				}
			}

			// Update gesture on/off state
			for (var i = 0; i < WearableConstants.GestureIds.Length; i++)
			{
				if (WearableConstants.GestureIds[i] == GestureId.None)
				{
					continue;
				}

				var gestureConfig = deviceConfig.GetGestureConfig(WearableConstants.GestureIds[i]);
				if (gestureConfig.isEnabled)
				{
					_activeProvider.EnableGesture(WearableConstants.GestureIds[i]);
				}
				else
				{
					_activeProvider.DisableGesture(WearableConstants.GestureIds[i]);
				}
			}

			_hasDeviceUpdateBeenApplied = true;
		}

		/// <summary>
		/// Resolves all <see cref="WearableRequirement.DeviceConfig"/>'s registered into WearableControl into
		/// a final device config that is an aggregate of all intended state with priority for enabled
		/// sensors/gestures over disabled and faster sensor update interval over slower.
		/// </summary>
		/// <returns></returns>
		private void ResolveFinalDeviceConfig()
		{
			// Reset all state in the final device config to off/slowest speeds.
			ResetFinalDeviceConfig();

			// Process all registered wearable requirement's device config and internal device config to
			// additively update state on the final config
			for (var i = _wearableRequirements.Count - 1; i >= 0; i--)
			{
				var wr = _wearableRequirements[i];
				// If we encounter a destroyed requirement, remove it
				if (ReferenceEquals(wr, null))
				{
					_wearableRequirements.RemoveAt(i);
					continue;
				}

				UpdateFinalDeviceConfig(wr.DeviceConfig);
			}

			UpdateFinalDeviceConfig(_wearableDeviceConfig);

			// Check for the invalid configuration of three or more sensors and TwentyMs and if
			// this is present, throttle back the SensorUpdateInterval to FortyMs.
			if (_finalWearableDeviceConfig.HasThreeOrMoreSensorsEnabled() &&
			    _finalWearableDeviceConfig.updateInterval == SensorUpdateInterval.TwentyMs)
			{
				Debug.LogWarning(WearableConstants.SensorUpdateIntervalDecreasedWarning, this);
				_finalWearableDeviceConfig.updateInterval = SensorUpdateInterval.FortyMs;
			}
		}

		/// <summary>
		/// Resets all sensors/gestures on the final device config to be false and sets the sensor update
		/// interval to the slowest speeds.
		/// </summary>
		private void ResetFinalDeviceConfig()
		{
			_finalWearableDeviceConfig.updateInterval = SensorUpdateInterval.ThreeHundredTwentyMs;

			// Set all sensor state and update intervals
			for (var i = 0; i < WearableConstants.SensorIds.Length; i++)
			{
				var finalSensorConfig = _finalWearableDeviceConfig.GetSensorConfig(WearableConstants.SensorIds[i]);
				finalSensorConfig.isEnabled = false;
			}

			// Set rotation source
			_finalWearableDeviceConfig.rotationSource = WearableConstants.DefaultRotationSource;

			// Set all gesture state
			for (var i = 0; i < WearableConstants.GestureIds.Length; i++)
			{
				if (WearableConstants.GestureIds[i] == GestureId.None)
				{
					continue;
				}

				var finalGestureConfig = _finalWearableDeviceConfig.GetGestureConfig(WearableConstants.GestureIds[i]);
				finalGestureConfig.isEnabled = false;
			}
		}

		/// <summary>
		/// Additively updates the final device config with <see cref="WearableDeviceConfig"/> <paramref name="config"/>
		/// </summary>
		/// <param name="config"></param>
		private void UpdateFinalDeviceConfig(WearableDeviceConfig config)
		{
			// Set all sensor state and update intervals
			for (var i = 0; i < WearableConstants.SensorIds.Length; i++)
			{
				var sensorId = WearableConstants.SensorIds[i];
				var finalSensorConfig = _finalWearableDeviceConfig.GetSensorConfig(sensorId);
				var reqSensorConfig = config.GetSensorConfig(sensorId);

				finalSensorConfig.isEnabled |= reqSensorConfig.isEnabled;
			}

			// Set all gesture state.
			for (var i = 0; i < WearableConstants.GestureIds.Length; i++)
			{
				if (WearableConstants.GestureIds[i] == GestureId.None)
				{
					continue;
				}

				var finalGestureConfig = _finalWearableDeviceConfig.GetGestureConfig(WearableConstants.GestureIds[i]);
				var reqGestureConfig = config.GetGestureConfig(WearableConstants.GestureIds[i]);

				finalGestureConfig.isEnabled |= reqGestureConfig.isEnabled;
			}

			if (config.HasAnySensorsEnabled())
			{
				if (_finalWearableDeviceConfig.updateInterval.IsSlowerThan(config.updateInterval))
				{
					_finalWearableDeviceConfig.updateInterval = config.updateInterval;
				}
			}

			// If the config rotation sensor is enabled and the final config has a lower priority rotation
			// source, override it
			if (_finalWearableDeviceConfig.rotationSource.IsLowerPriority(config.rotationSource))
			{
				_finalWearableDeviceConfig.rotationSource = config.rotationSource;
			}
		}

		/// <summary>
		/// True if the device state needs to be updated because it differs from our the
		/// <see cref="WearableDeviceConfig"/> <paramref name="config"/>, otherwise false.
		/// </summary>
		/// <param name="config"></param>
		/// <returns></returns>
		private bool ShouldUpdateDeviceState(WearableDeviceConfig config)
		{
			// Check all sensors to see if we need to update the device.
			var deviceShouldBeUpdated = false;
			for (var i = 0; i < WearableConstants.SensorIds.Length; i++)
			{
				var sensorId = WearableConstants.SensorIds[i];
				var sensorConfig = config.GetSensorConfig(sensorId);

				if (sensorConfig.isEnabled != GetSensorActive(sensorId))
				{
					deviceShouldBeUpdated = true;
				}
			}

			// Check the sensor update interval to see if we need to update the device.
			if (config.updateInterval != UpdateInterval)
			{
				deviceShouldBeUpdated = true;
			}

			// Check the rotation source to see if we need to update the device.
			if (config.rotationSource != RotationSource)
			{
				deviceShouldBeUpdated = true;
			}

			// Check all gestures to see if we need to update the device state.
			if (!deviceShouldBeUpdated)
			{
				for (var i = 0; i < WearableConstants.GestureIds.Length; i++)
				{
					if (WearableConstants.GestureIds[i] == GestureId.None)
					{
						continue;
					}

					var gestureConfig = config.GetGestureConfig(WearableConstants.GestureIds[i]);
					if (gestureConfig.isEnabled != GetGestureEnabled(WearableConstants.GestureIds[i]))
					{
						deviceShouldBeUpdated = true;
						break;
					}
				}
			}

			return deviceShouldBeUpdated;
		}

		/// <summary>
		/// Starts the <see cref="WearableSensor"/> for <see cref="SensorId"/> <paramref name="sensorId"/>
		/// </summary>
		/// <param name="sensorId"></param>
		private void StartSensor(SensorId sensorId)
		{
			if (StartSensorInternal(sensorId))
			{
				LockDeviceStateUpdate();
			}
		}

		/// <summary>
		/// Start a sensor with a given interval <see cref="SensorId"/>. Returns true if the sensor was started,
		/// otherwise false.
		/// </summary>
		/// <param name="sensorId"></param>
		private bool StartSensorInternal(SensorId sensorId)
		{
			var sensorConfig = _wearableDeviceConfig.GetSensorConfig(sensorId);
			if (sensorConfig.isEnabled)
			{
				return false;
			}

			sensorConfig.isEnabled = true;

			return true;
		}

		/// <summary>
		/// Stops the <see cref="WearableSensor"/> for <see cref="SensorId"/> <paramref name="sensorId"/>
		/// </summary>
		/// <param name="sensorId"></param>
		private void StopSensor(SensorId sensorId)
		{
			if (StopSensorInternal(sensorId))
			{
				LockDeviceStateUpdate();
			}
		}

		/// <summary>
		/// Stop a sensor with a given <see cref="SensorId"/>. Returns true if the sensor was stopped,
		/// otherwise false.
		/// </summary>
		/// <param name="sensorId"></param>
		private bool StopSensorInternal(SensorId sensorId)
		{
			var sensorConfig = _wearableDeviceConfig.GetSensorConfig(sensorId);
			if (!sensorConfig.isEnabled)
			{
				return false;
			}

			sensorConfig.isEnabled = false;

			return true;
		}

		/// <summary>
		/// Returns whether or not a sensor with a given <see cref="SensorId"/> is active or not.
		/// </summary>
		/// <param name="sensorId"></param>
		/// <returns></returns>
		private bool GetSensorActive(SensorId sensorId)
		{
			return _activeProvider.GetSensorActive(sensorId);
		}

		/// <summary>
		/// Enables the <see cref="WearableGesture"/> for <see cref="GestureId"/> <paramref name="gestureId"/>
		/// </summary>
		/// <param name="gestureId"></param>
		private void EnableGesture(GestureId gestureId)
		{
			if (gestureId == GestureId.None)
			{
				throw new Exception(WearableConstants.GestureIdNoneInvalidError);
			}

			if (EnableGestureInternal(gestureId))
			{
				LockDeviceStateUpdate();
			}
		}

		/// <summary>
		/// Start a gesture with a given interval <see cref="GestureId"/>.
		/// </summary>
		/// <param name="gestureId"></param>
		private bool EnableGestureInternal(GestureId gestureId)
		{
			var gestureConfig = _wearableDeviceConfig.GetGestureConfig(gestureId);
			if (gestureConfig.isEnabled)
			{
				return false;
			}

			gestureConfig.isEnabled = true;

			return true;
		}

		/// <summary>
		/// Disables the <see cref="WearableGesture"/> for <see cref="GestureId"/> <paramref name="gestureId"/>
		/// </summary>
		/// <param name="gestureId"></param>
		private void DisableGesture(GestureId gestureId)
		{
			if (gestureId == GestureId.None)
			{
				throw new Exception(WearableConstants.GestureIdNoneInvalidError);
			}

			if (DisableGestureInternal(gestureId))
			{
				LockDeviceStateUpdate();
			}
		}

		/// <summary>
		/// Stop a gesture with a given <see cref="GestureId"/>.
		/// </summary>
		/// <param name="gestureId"></param>
		private bool DisableGestureInternal(GestureId gestureId)
		{
			var gestureConfig = _wearableDeviceConfig.GetGestureConfig(gestureId);
			if (!gestureConfig.isEnabled)
			{
				return false;
			}

			gestureConfig.isEnabled = false;

			return true;
		}

		/// <summary>
		/// Returns whether or not a gesture with a given <see cref="GestureId"/> is enabled.
		/// </summary>
		/// <param name="gestureId"></param>
		/// <returns></returns>
		private bool GetGestureEnabled(GestureId gestureId)
		{
			return _activeProvider.GetGestureEnabled(gestureId);
		}

		protected override void Awake()
		{
			_wearableDeviceConfig = new WearableDeviceConfig();
			_finalWearableDeviceConfig = new WearableDeviceConfig();

			_accelerometerSensor = new WearableSensor(this, SensorId.Accelerometer);
			_gyroscopeSensor = new WearableSensor(this, SensorId.Gyroscope);
			_rotationSensor = new WearableSensor(this, SensorId.Rotation);

			// populate wearable gesture dictionary
			_wearableGestures = new Dictionary<GestureId, WearableGesture>();
			for (var i = 0; i < WearableConstants.GestureIds.Length; ++i)
			{
				if (WearableConstants.GestureIds[i] != GestureId.None)
				{
					_wearableGestures[WearableConstants.GestureIds[i]] =
						new WearableGesture(this, WearableConstants.GestureIds[i]);
				}
			}

			// Activate the default provider depending on the platform
			#if UNITY_EDITOR
			SetActiveProvider(GetOrCreateProvider(_editorDefaultProvider));
			#else
			SetActiveProvider(GetOrCreateProvider(_runtimeDefaultProvider));
			#endif

			base.Awake();
		}

		private void OnValidate()
		{
			// Set using the variable not the method, so the provider doesn't get prematurely initialized
			#if UNITY_EDITOR
			_activeProvider = GetOrCreateProvider(_editorDefaultProvider);
			#else
			_activeProvider = GetOrCreateProvider(_runtimeDefaultProvider);
			#endif
		}

		/// <summary>
		/// When destroyed, stop all sensors and disconnect from the Wearable device.
		/// </summary>
		protected override void OnDestroy()
		{
			if (ConnectedDevice.HasValue)
			{
				for (var i = 0; i < WearableConstants.SensorIds.Length; i++)
				{
					_activeProvider.StopSensor(WearableConstants.SensorIds[i]);
				}

				for (var i = 0; i < WearableConstants.GestureIds.Length; i++)
				{
					if (WearableConstants.GestureIds[i] == GestureId.None)
					{
						continue;
					}

					_activeProvider.DisableGesture(WearableConstants.GestureIds[i]);
				}

				DisconnectFromDevice();
			}

			// Clean up providers
			_activeProvider.OnDisableProvider();

			if (_deviceProvider != null && _deviceProvider.Initialized)
			{
				_deviceProvider.OnDestroyProvider();
			}

			if (_debugProvider != null && _debugProvider.Initialized)
			{
				_debugProvider.OnDestroyProvider();
			}

			base.OnDestroy();
		}

		/// <summary>
		/// When enabled, resume monitoring the device session if necessary.
		/// </summary>
		private void OnEnable()
		{
			if (!_activeProvider.Enabled)
			{
				_activeProvider.OnEnableProvider();
			}
		}

		/// <summary>
		/// When disabled, stop actively searching for devices.
		/// </summary>
		private void OnDisable()
		{
			_activeProvider.OnDisableProvider();
		}

		private void Update()
		{
			if (UpdateMode != UnityUpdateMode.Update)
			{
				return;
			}

			_activeProvider.OnUpdate();
		}

		/// <summary>
		/// For each sensor, prompt them to get their buffer of updates from native code per fixed physics update step.
		/// </summary>
		private void FixedUpdate()
		{
			if (UpdateMode != UnityUpdateMode.FixedUpdate)
			{
				return;
			}

			_activeProvider.OnUpdate();
		}

		/// <summary>
		/// For each sensor, prompt them to get their buffer of updates from native code per late update step.
		/// If there are any device config updates, apply them at the end of the frame during a device update lock.
		/// </summary>
		private void LateUpdate()
		{
			if (_isDeviceStateUpdateLocked)
			{
				var secondsSinceLockStart = Time.time - _appTimeSinceDeviceStateUpdateLocked;

				// If we have not yet applied the device update, execute the update now.
				if (!_hasDeviceUpdateBeenApplied)
				{
					// Execute the update.
					UpdateDeviceFromConfig();
				}
				else if (secondsSinceLockStart >= WearableConstants.NumberOfSecondsToLockSensorFrequencyUpdate)
				{
					// If another pending update during the lock, execute it immediately/refresh the lock
					if (_isDeviceStateUpdatePendingDuringLock)
					{
						_isDeviceStateUpdatePendingDuringLock = false;

						// Execute the update.
						UpdateDeviceFromConfig();

						// Refresh the lock
						_appTimeSinceDeviceStateUpdateLocked = Time.time;
					}
					// Otherwise release the lock.
					else
					{
						UnlockDeviceStateUpdate();
					}
				}
			}

			if (UpdateMode != UnityUpdateMode.LateUpdate)
			{
				return;
			}

			_activeProvider.OnUpdate();
		}

		#endregion
	}
}
