using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Bose.Wearable.Examples
{
	public class BasicDemoUIPanel : MonoBehaviour
	{
		[Header("UX Refs")]
		[SerializeField]
		private Button _backButton;

		[SerializeField]
		private Toggle _absoluteToggle;

		[SerializeField]
		private Toggle _referenceToggle;

		[Header("Scene Refs")]

		[Header("Sounds"), Space(5)]
		[SerializeField]
		private AudioClip _buttonPressClip;

		[SerializeField]
		private BasicDemoController _basicDemoController;

		private AudioControl _audioControl;

		private const string MissingBasicDemoComponent =
			"This WearableModel game object is missing a BasicDemoController.";

		private void Awake()
		{
			_backButton.onClick.AddListener(OnBackButtonClicked);
			_absoluteToggle.onValueChanged.AddListener(OnAbsoluteButtonClicked);
			_referenceToggle.onValueChanged.AddListener(OnReferenceButtonClicked);

			_audioControl = AudioControl.Instance;
		}

		private void OnDestroy()
		{
			_backButton.onClick.RemoveAllListeners();
			_absoluteToggle.onValueChanged.RemoveAllListeners();
			_referenceToggle.onValueChanged.RemoveAllListeners();
		}

		private void OnBackButtonClicked()
		{
			LoadingUIPanel.Instance.LoadScene(WearableConstants.MainMenuScene, LoadSceneMode.Single);
		}

		private void OnAbsoluteButtonClicked(bool isOn)
		{
			if (isOn)
			{
				_basicDemoController.SetAbsoluteReference();
				_audioControl.PlayOneShot(_buttonPressClip);
			}
		}

		private void OnReferenceButtonClicked(bool isOn)
		{
			if (isOn)
			{
				_basicDemoController.SetRelativeReference();
				_audioControl.PlayOneShot(_buttonPressClip);
			}
		}
	}
}
