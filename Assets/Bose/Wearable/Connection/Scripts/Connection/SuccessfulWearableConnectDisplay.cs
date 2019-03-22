using System.Collections;
using UnityEngine;

namespace Bose.Wearable
{
	/// <summary>
	/// Shown when a device connection attempt has succeeded
	/// </summary>
	public class SuccessfulWearableConnectDisplay : WearableConnectDisplayBase
	{
		[Header("Sound Clips")]
		[SerializeField]
		private AudioClip _sfxSuccess;

		protected override void Awake()
		{
			SetupAudio();

			base.Awake();
		}

		private void OnEnable()
		{
			_panel.DeviceConnectSuccess += OnDeviceConnectionSuccess;
		}

		private void OnDisable()
		{
			_panel.DeviceConnectSuccess += OnDeviceConnectionSuccess;
		}

		private void OnDeviceConnectionSuccess()
		{
			StartCoroutine(ShowSuccess());
		}

		private IEnumerator ShowSuccess()
		{
			PlaySuccessSting();

			Show();

			// TODO Add animation and then hide the panel
			yield return new WaitForSecondsRealtime(1.5f);

			Hide();
		}

		protected override void Show()
		{
			_messageText.text = WearableConstants.DeviceConnectSuccessMessage;

			base.Show();
		}

		protected override void Hide()
		{
			if (WearableControl.Instance.ConnectedDevice != null)
			{
				_panel.Hide();
			}

			base.Hide();
		}

		private void PlaySuccessSting()
		{
			_audioControl.PlayOneShot(_sfxSuccess);
		}
	}
}
