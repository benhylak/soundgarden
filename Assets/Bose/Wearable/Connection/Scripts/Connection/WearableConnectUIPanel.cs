using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Bose.Wearable
{
	/// <summary>
	/// Represents the connection screen UI and allows for easy connecting to devices.
	/// </summary>
	[RequireComponent(typeof(CanvasGroup), typeof(Canvas))]
	public class WearableConnectUIPanel : Singleton<WearableConnectUIPanel>, ISelectionController<Device>
	{
		/// <summary>
		/// Invoked when a device search has started.
		/// </summary>
		public event Action DeviceSearching;

		/// <summary>
		/// Invoked when a device search started locally results in devices found.
		/// </summary>
		public event Action<Device[]> DevicesFound;

		/// <summary>
		/// Invoked when an attempt has been made to connect to a device.
		/// </summary>
		public event Action DeviceConnecting;

		/// <summary>
		/// Invoked when a device has been successfully connected to.
		/// </summary>
		public event Action DeviceConnectSuccess;

		/// <summary>
		/// Invoked when a device connection attempt has failed.
		/// </summary>
		public event Action DeviceConnectFailure;

		/// <summary>
		/// Invoked when a device has become disconnected.
		/// </summary>
		public event Action<Device> DeviceDisconnected;

		/// <summary>
		/// The Canvas on the root UI element.
		/// </summary>
		[Header("UI Elements")]
		[SerializeField]
		private Canvas _canvas;

		/// <summary>
		/// The CanvasGroup on the root UI element of this Canvas.
		/// </summary>
		[SerializeField]
		private CanvasGroup _canvasGroup;

		[SerializeField]
		private bool _showOnStart;

		private Action _onClose;
		private WearableControl _wearableControl;
		private EventSystem _eventSystem;
		
		private const string CannotFindEventSystemWarning = "[Bose Wearable] Cannot find an EventSystem. WearableConnectUIPanel will not detect any input.";

		/// <summary>
		/// Initialize any local state or listeners
		/// </summary>
		protected override void Awake()
		{
			base.Awake();

			_wearableControl = WearableControl.Instance;

			_canvas.enabled = false;
			_canvasGroup.alpha = 0f;

			ToggleLockScreen(false);
		}

		private void Start()
		{
			if (_showOnStart)
			{
				Show();
			}
		}

		private void OnEnable()
		{
			_wearableControl.DeviceDisconnected += OnDeviceDisconnected;
		}

		private void OnDisable()
		{
			_wearableControl.DeviceDisconnected -= OnDeviceDisconnected;
		}

		/// <summary>
		/// Called when a device has become disconnected.
		/// </summary>
		/// <param name="device"></param>
		private void OnDeviceDisconnected(Device device)
		{
			if (DeviceDisconnected != null)
			{
				DeviceDisconnected(device);
			}
		}

		/// <summary>
		/// Show the UI and kick off the search for devices; optional parameters are provided with <paramref name="onClose"/>
		/// providing a way to know when the panel has closed and <paramref name="allowExitWithoutDevice"/> allowing for
		/// exiting the panel without having a connected device.
		/// </summary>
		/// <param name="onClose"></param>
		/// <param name="allowExitWithoutDevice"></param>
		public void Show(Action onClose = null, bool allowExitWithoutDevice = false)
		{
			WarnIfNoEventSystemPresent();
			
			_onClose = onClose;
			_canvas.enabled = true;
			_canvasGroup.alpha = 1f;

			StartSearch();
		}

		/// <summary>
		/// Show the Connection UI Panel without immediately searching.
		/// </summary>
		public void ShowWithoutSearching()
		{
			WarnIfNoEventSystemPresent();

			_canvas.enabled = true;
			_canvasGroup.alpha = 1f;

			ToggleLockScreen(true);
		}

		/// <summary>
		/// Attempt to reconnect to a Device.
		/// </summary>
		/// <param name="device"></param>
		public void ReconnectToDevice(Device? device)
		{
			if (device.HasValue)
			{
				OnSelect(device.Value);
			}
		}

		public void StartSearch()
		{
			if (DeviceSearching != null)
			{
				DeviceSearching();
			}

			_wearableControl.SearchForDevices(OnDevicesUpdated);

			ToggleLockScreen(true);
		}

		/// <summary>
		/// On receiving new device updates, update the list of devices shown.
		/// </summary>
		/// <param name="devices"></param>
		private void OnDevicesUpdated(Device[] devices)
		{
			if (DevicesFound != null)
			{
				DevicesFound(devices);
			}
		}

		/// <summary>
		/// Hides the UI and stops the search for devices.
		/// </summary>
		public void Hide()
		{
			_canvas.enabled = false;
			_canvasGroup.alpha = 0f;

			_wearableControl.StopSearchingForDevices();

			ToggleLockScreen(false);

			if (_onClose != null)
			{
				_onClose();
			}

			_onClose = null;
		}

		/// <summary>
		/// Enables or disables user input to this UI panel.
		/// </summary>
		/// <param name="isInteractable"></param>
		private void ToggleLockScreen(bool isInteractable)
		{
			_canvasGroup.interactable = isInteractable;
		}

		private void WarnIfNoEventSystemPresent()
		{
			if (_eventSystem == null)
			{
				_eventSystem = FindObjectOfType<EventSystem>();

				if (_eventSystem == null)
				{
					Debug.LogWarning(CannotFindEventSystemWarning, this);
				}
			}
		}

		#region ISelectionController

		public void OnSelect(Device value)
		{
			if (DeviceConnecting != null)
			{
				DeviceConnecting();
			}

			_wearableControl.StopSearchingForDevices();
			_wearableControl.ConnectToDevice(value, OnDeviceConnectSuccess, OnDeviceConnectFailure);

			ToggleLockScreen(false);
		}

		private void OnDeviceConnectSuccess()
		{
			if (DeviceConnectSuccess != null)
			{
				DeviceConnectSuccess();
			}

			ToggleLockScreen(true);
		}

		private void OnDeviceConnectFailure()
		{
			if (DeviceConnectFailure != null)
			{
				DeviceConnectFailure();
			}

			ToggleLockScreen(true);
		}

		#endregion
	}
}
