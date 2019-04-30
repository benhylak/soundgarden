using UnityEngine;
using UnityEngine.UI;

namespace Bose.Wearable
{
	/// <summary>
	/// Shown when a device connection attempt has failed
	/// </summary>
	public sealed class FailedWearableConnectDisplay : WearableConnectDisplayBase
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
			_panel.DeviceSearchFailure += OnDeviceSearchFailure;
			_panel.DeviceConnectFailure += OnDeviceConnectFailure;

			_searchButton.onClick.AddListener(OnSearchButtonClicked);
		}

		private void OnDisable()
		{
			_panel.DeviceSearchFailure -= OnDeviceSearchFailure;
			_panel.DeviceConnectFailure -= OnDeviceConnectFailure;

			_searchButton.onClick.RemoveAllListeners();
		}

		private void OnDeviceSearchFailure()
		{
			_messageText.text = WearableConstants.DeviceConnectSearchFailureMessage;

			Show();
		}

		private void OnDeviceConnectFailure()
		{
			_messageText.text = WearableConstants.DeviceConnectFailureMessage;

			Show();
		}

		private void OnSearchButtonClicked()
		{
			_panel.CheckForPermissionsAndTrySearch();

			Hide();
		}

		protected override void Show()
		{
			PlayFailureSting();

			base.Show();
		}

		private void PlayFailureSting()
		{
			_audioControl.PlayOneShot(_sfxConnectFailed);
		}
	}
}
