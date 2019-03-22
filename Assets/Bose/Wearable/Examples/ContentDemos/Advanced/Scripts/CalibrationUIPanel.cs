using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bose.Wearable.Examples
{
	public class CalibrationUIPanel : MonoBehaviour
	{
		[Header("Scene Refs")]
		[SerializeField]
		private AdvancedDemoController _advancedDemoController;

		[Header("UX Refs"), Space(5)]
		[SerializeField]
		private Canvas _canvas;

		[SerializeField]
		private CanvasGroup _panelCanvasGroup;

		[SerializeField]
		private Text _labelText;

		[Header("UX Refs"), Space(5)]
		[SerializeField]
		[Range(0, float.MaxValue)]
		private float _fadeDuration = 0.66f;

		private Coroutine _fadeCoroutine;

		private void Awake()
		{
			_labelText.text = WearableConstants.WaitForCalibrationMessage;

			_advancedDemoController.CalibrationCompleted += OnCalibrationCompleted;
		}

		private void OnDestroy()
		{
			_advancedDemoController.CalibrationCompleted -= OnCalibrationCompleted;

			if (_fadeCoroutine != null)
			{
				StopCoroutine(_fadeCoroutine);
				_fadeCoroutine = null;
			}
		}

		private void OnCalibrationCompleted()
		{
			_fadeCoroutine = StartCoroutine(FadeHidePanelUI());
		}

		private IEnumerator FadeHidePanelUI()
		{
			var waitForEndOfFrame = new WaitForEndOfFrame();
			var time = 0f;
			while (_panelCanvasGroup.alpha > 0f)
			{
				time += Time.unscaledDeltaTime;
				_panelCanvasGroup.alpha = Mathf.Clamp01(1f - (time / _fadeDuration));

				yield return waitForEndOfFrame;
			}

			_panelCanvasGroup.interactable = _panelCanvasGroup.blocksRaycasts = false;
			_canvas.enabled = false;
		}
	}
}
