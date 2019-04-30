using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bose.Wearable
{
	/// <summary>
	/// Represents the connection screen UI and allows for easy connecting to devices.
	/// </summary>
	[RequireComponent(typeof(CanvasGroup), typeof(Canvas))]
	public sealed class WearableConnectUIPanel : Singleton<WearableConnectUIPanel>, ISelectionController<Device>
	{
		/// <summary>
		/// Invoked when a device search has started.
		/// </summary>
		public event Action DeviceSearching;

		/// <summary>
		/// Invoked when a device search cannot be started.
		/// </summary>
		public event Action DeviceSearchFailure;

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
		
		/// <summary>
		/// The shared text field used to provide context for the current panel.
		/// </summary>
		[SerializeField]
		private Text _messageText;

		[SerializeField]
		private bool _showOnStart;

		private Coroutine _checkForPermissionsAndTrySearchCoroutine;

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
			_messageText.text = string.Empty;

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
		/// Called when devices cannot be searched for.
		/// </summary>
		private void OnDeviceSearchFailure()
		{
			if (DeviceSearchFailure != null)
			{
				DeviceSearchFailure();
			}

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

			CheckForPermissionsAndTrySearch();
		}

		/// <summary>
		/// Show the Connection UI Panel without immediately searching.
		/// </summary>
		internal void ShowWithoutSearching()
		{
			WarnIfNoEventSystemPresent();

			_canvas.enabled = true;
			_canvasGroup.alpha = 1f;

			ToggleLockScreen(true);
		}

		/// <summary>
		/// Enables or disables user input to this UI panel.
		/// </summary>
		/// <param name="isInteractable"></param>
		private void ToggleLockScreen(bool isInteractable)
		{
			_canvasGroup.interactable = isInteractable;
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
		/// Checks for appropriate platform permissions, and if granted begins a search for devices.
		/// </summary>
		internal void CheckForPermissionsAndTrySearch()
		{
			if (_checkForPermissionsAndTrySearchCoroutine != null)
			{
				StopCoroutine(_checkForPermissionsAndTrySearchCoroutine);
				_checkForPermissionsAndTrySearchCoroutine = null;
			}

			_checkForPermissionsAndTrySearchCoroutine = StartCoroutine(CheckForPermissionsAndTrySearchCoroutine());
		}

		/// <summary>
		/// Checks for appropriate platform permissions, and if granted begins a search for devices.
		/// </summary>
		private IEnumerator CheckForPermissionsAndTrySearchCoroutine()
		{
			_canvas.enabled = true;
			_canvasGroup.alpha = 1f;
			
			// Certain providers require permission, check for those first.
			bool permissionsGranted = true;
			
			#if UNITY_ANDROID && UNITY_2018_3_OR_NEWER && !UNITY_EDITOR
			UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.CoarseLocation);

			// We have discovered that the RequestUserPermission method does not block further execution of the
			// application, however we have discovered that Unity does seem to block further rendering at the
			// completion of the frame. 
			// Additionally, the OS often needs a little bit of time to record the result of the modal permissions
			// dialog that is present to the user. Since Unity does not present the functionality to poll
			// or asynchronously receive a callback upon the result of this request, we have currently settled on an
			// arbitrary amount of frames (tested across both old and new devices) to wait in order to re-query the
			// permission before continuing. A further improvement to this process would be to add platform-specific
			// hooks that allow us to receive more concrete information as to when this process could continue.
	
			var wait = new WaitForEndOfFrame();
			yield return wait;
			yield return wait;
			yield return wait;

			permissionsGranted = UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.CoarseLocation);
			#endif

			// If permissions have been granted, start the search for devices, otherwise log an error that explains
			// the necessity of the permissions to continue.
			if (permissionsGranted)
			{
				StartSearch();
			}
			else
			{
				Debug.LogError(WearableConstants.DeviceConnectionPermissionError);
				OnDeviceSearchFailure();
			}

			_checkForPermissionsAndTrySearchCoroutine = null;
			yield break;
		}
		
		private void StartSearch()
		{
			if (DeviceSearching != null)
			{
				DeviceSearching();
			}

			_wearableControl.SearchForDevices(OnDevicesUpdated);

			ToggleLockScreen(true);
		}

		/// <summary>
		/// Attempt to reconnect to a Device.
		/// </summary>
		/// <param name="device"></param>
		internal void ReconnectToDevice(Device? device)
		{
			if (device.HasValue)
			{
				OnSelect(device.Value);
			}
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
