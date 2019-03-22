using UnityEngine;
using UnityEngine.UI;

namespace Bose.Wearable
{
	/// <summary>
	/// Shown when a device connection attempt has failed
	/// </summary>
	public class FailedWearableConnectDisplay : WearableConnectDisplayBase
	{
		[SerializeField]
		private Button _searchButton;

		[Header("Sound Clips")]
		[SerializeField]
		private AudioClip _sfxConnectFailed;

		protected override void Awake()
		{
			SetupAudio();

			base.Awake();
		}

		private void OnEnable()
		{
			_panel.DeviceConnectFailure += OnDeviceConnectionFailure;

			_searchButton.onClick.AddListener(OnSearchButtonClicked);
		}

		private void OnDisable()
		{
			_panel.DeviceConnectFailure += OnDeviceConnectionFailure;

			_searchButton.onClick.RemoveAllListeners();
		}

		private void OnDeviceConnectionFailure()
		{
			Show();
		}

		private void OnSearchButtonClicked()
		{
			_panel.StartSearch();

			Hide();
		}

		protected override void Show()
		{
			_messageText.text = WearableConstants.DeviceConnectFailureMessage;

			PlayFailureSting();

			base.Show();
		}

		private void PlayFailureSting()
		{
			_audioControl.PlayOneShot(_sfxConnectFailed);
		}
	}
}
