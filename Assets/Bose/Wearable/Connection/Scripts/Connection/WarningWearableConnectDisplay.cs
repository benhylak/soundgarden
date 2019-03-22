using UnityEngine;
using UnityEngine.UI;

namespace Bose.Wearable
{
	public class WarningWearableConnectDisplay : WearableConnectDisplayBase
	{
		[SerializeField]
		private Button _searchButton;

		[SerializeField]
		private Button _reconnectButton;

		[Header("Sound Clips"), Space(5)]
		[SerializeField]
		private AudioClip _sfxConnectFailed;

		private Device? _device;

		protected override void Awake()
		{
			SetupAudio();

			base.Awake();
		}

		private void OnEnable()
		{
			_panel.DeviceDisconnected += OnDeviceDisconnected;
			_panel.DeviceSearching += OnDeviceSearching;
			_panel.DeviceConnecting += OnDeviceConnecting;

			_searchButton.onClick.AddListener(OnSearchButtonClicked);
			_reconnectButton.onClick.AddListener(OnReconnectButtonClicked);
		}


		private void OnDisable()
		{
			_panel.DeviceDisconnected -= OnDeviceDisconnected;
			_panel.DeviceSearching -= OnDeviceSearching;
			_panel.DeviceConnecting -= OnDeviceConnecting;

			_searchButton.onClick.RemoveAllListeners();
			_reconnectButton.onClick.RemoveAllListeners();
		}

		private void OnDeviceDisconnected(Device device)
		{
			_device = device;
			_messageText.text = WearableConstants.DeviceDisconnectionMessage;

			_panel.ShowWithoutSearching();

			Show();
		}

		private void OnDeviceSearching()
		{
			Hide();
		}

		private void OnDeviceConnecting()
		{
			Hide();
		}

		private void OnReconnectButtonClicked()
		{
			if (_device != null)
			{
				_panel.ReconnectToDevice(_device);
				_device = null;
			}
		}

		private void OnSearchButtonClicked()
		{
			_panel.StartSearch();
		}
	}
}
