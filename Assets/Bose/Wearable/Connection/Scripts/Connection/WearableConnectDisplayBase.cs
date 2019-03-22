using UnityEngine;
using UnityEngine.UI;

namespace Bose.Wearable
{
	/// <summary>
	/// A base class for handling behavior for hiding and showing displays related to device connection
	/// </summary>
	public class WearableConnectDisplayBase : MonoBehaviour
	{
		[SerializeField]
		protected WearableConnectUIPanel _panel;

		[SerializeField]
		protected CanvasGroup _canvasGroup;

		[SerializeField]
		protected Text _messageText;

		protected AudioControl _audioControl;

		protected virtual void Awake()
		{
			Hide();
		}

		protected virtual void SetupAudio()
		{
			_audioControl = AudioControl.Instance;
		}

		protected virtual void TeardownAudio()
		{
		}

		protected virtual void Show()
		{
			_canvasGroup.alpha = 1f;
			_canvasGroup.interactable = _canvasGroup.blocksRaycasts = true;
		}

		protected virtual void Hide()
		{
			_canvasGroup.interactable = _canvasGroup.blocksRaycasts = false;
			_canvasGroup.alpha = 0f;
		}
	}
}
