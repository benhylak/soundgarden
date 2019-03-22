
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Bose.Wearable.Examples
{
	public class MainMenuUIPanel : MonoBehaviour
	{
		[SerializeField]
		private CanvasGroup _canvasGroup;

		[SerializeField]
		private Button _basicDemoButton;

		[SerializeField]
		private Button _gestureDemoButton;

		[SerializeField]
		private Button _advancedDemoButton;

		private void Awake()
		{
			_basicDemoButton.onClick.AddListener(OnBasicDemoButtonClicked);
			_advancedDemoButton.onClick.AddListener(OnAdvancedDemoButtonClicked);
			_gestureDemoButton.onClick.AddListener(OnGestureDemoButtonClicked);

			ToggleInteractivity(true);
		}

		private void OnDestroy()
		{
			_basicDemoButton.onClick.RemoveAllListeners();
			_advancedDemoButton.onClick.RemoveAllListeners();
			_gestureDemoButton.onClick.RemoveAllListeners();
		}

		private void OnAdvancedDemoButtonClicked()
		{
			LoadingUIPanel.Instance.LoadScene(WearableConstants.AdvancedDemoScene, LoadSceneMode.Single);

			ToggleInteractivity(false);
		}

		private void OnBasicDemoButtonClicked()
		{
			LoadingUIPanel.Instance.LoadScene(WearableConstants.BasicDemoScene, LoadSceneMode.Single);

			ToggleInteractivity(false);
		}

		private void OnGestureDemoButtonClicked()
		{
			LoadingUIPanel.Instance.LoadScene(WearableConstants.GestureDemoScene, LoadSceneMode.Single);

			ToggleInteractivity(false);
		}

		private void ToggleInteractivity(bool isOn)
		{
			_canvasGroup.interactable = isOn;
		}
	}
}
