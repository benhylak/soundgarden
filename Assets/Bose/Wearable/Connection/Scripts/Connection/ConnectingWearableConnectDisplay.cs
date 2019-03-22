using UnityEngine;

namespace Bose.Wearable
{
	/// <summary>
	/// Shown when a device connection attempt is made
	/// </summary>
	public class ConnectingWearableConnectDisplay : WearableConnectDisplayBase
	{
		[Header("Sound Clips")]
		[SerializeField]
		private AudioClip _sfxConnecting;

		// Audio
		private AudioSource _srcConnecting;
		private const float TIME_BACKGROUND_FADE = 0.5f;

		protected override void Awake()
		{
			SetupAudio();

			_panel.DeviceConnecting += OnDeviceConnecting;
			_panel.DeviceConnectFailure += OnDeviceConnectEnded;
			_panel.DeviceConnectSuccess += OnDeviceConnectEnded;

			base.Awake();
		}

		private void OnDestroy()
		{
			_panel.DeviceConnecting -= OnDeviceConnecting;
			_panel.DeviceConnectFailure -= OnDeviceConnectEnded;
			_panel.DeviceConnectSuccess -= OnDeviceConnectEnded;

			TeardownAudio();
		}

		private void OnDeviceConnecting()
		{
			Show();
		}

		private void OnDeviceConnectEnded()
		{
			Hide();
		}

		protected override void Show()
		{
			_messageText.text = WearableConstants.DeviceConnectionUnderwayMessage;

			base.Show();

			StartConnectingLoop();
		}

		protected override void Hide()
		{
			base.Hide();

			StopConnectingLoop();
		}

		private void StartConnectingLoop()
		{
			if (_srcConnecting == null)
			{
				_srcConnecting = _audioControl.GetSource(true);
				_srcConnecting.clip = _sfxConnecting;
				_srcConnecting.loop = true;
			}

			_audioControl.FadeIn(_srcConnecting, TIME_BACKGROUND_FADE);
		}

		private void StopConnectingLoop()
		{
			if (_srcConnecting == null)
			{
				return;
			}

			_audioControl.FadeOut(_srcConnecting, TIME_BACKGROUND_FADE);
		}

		protected override void TeardownAudio()
		{
			if (_srcConnecting != null)
			{
				Destroy(_srcConnecting.gameObject);
			}
		}
	}
}
