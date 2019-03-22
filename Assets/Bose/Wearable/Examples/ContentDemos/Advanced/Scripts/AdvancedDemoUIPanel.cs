using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Bose.Wearable.Examples
{
	public class AdvancedDemoUIPanel : MonoBehaviour
	{
		[Header("UX Refs")]
		[SerializeField]
		private Button _backButton;

		[SerializeField]
		private CanvasGroup _hintUICanvasGroup;

		[Header("Animation"), Space(5)]
		[SerializeField]
		private float _fadeDuration = 1f;

		[Header("Control"), Space(5)]
		[SerializeField]
		private float _timeToShowHintUI = 5f;

		[SerializeField]
		[Range(0f, 1f)]
		private float _moveDistanceToHideHintUI = .25f;

		private float _currentHideHintTime;
		private float _screenWidth;
		private float _screenHeight;
		private float _distanceTouchMoved;
		private bool _doHideHintUI;
		private bool _doPermanentlyHideHintUI;
		private Vector3? _priorPosition;
		private Coroutine _hintUICoroutine;

		private void Awake()
		{
			_doHideHintUI = true;
			_screenWidth = Screen.width;
			_screenHeight = Screen.height;
			_hintUICanvasGroup.alpha = 0f;

			_backButton.onClick.AddListener(OnBackButtonClicked);

			SingleTouchRecognizer.Instance.TouchMoved += OnTouchMoved;
			SingleTouchRecognizer.Instance.TouchEnded += OnTouchEnded;
		}

		private void OnDestroy()
		{
			_backButton.onClick.RemoveAllListeners();

			if (SingleTouchRecognizer.Instance != null)
			{
				SingleTouchRecognizer.Instance.TouchMoved -= OnTouchMoved;
				SingleTouchRecognizer.Instance.TouchEnded -= OnTouchEnded;
			}

			if (_hintUICoroutine != null)
			{
				StopCoroutine(_hintUICoroutine);
				_hintUICoroutine = null;
			}
		}

		private void OnBackButtonClicked()
		{
			LoadingUIPanel.Instance.LoadScene(WearableConstants.MainMenuScene, LoadSceneMode.Single);
		}

		/// <summary>
		/// When the touch ends, stop any fading of the hint UI if the touch has moved more than a specific
		/// percentage of the screen horizontally.
		/// </summary>
		/// <param name="touch"></param>
		private void OnTouchEnded(Touch touch)
		{
			if (_doHideHintUI || _doPermanentlyHideHintUI)
			{
				return;
			}

			if (_hintUICoroutine != null && _distanceTouchMoved > _moveDistanceToHideHintUI)
			{
				HideHintUI();
			}

			_distanceTouchMoved = 0;
			_priorPosition = null;
		}

		/// <summary>
		/// On each touch moved, capture the distance the finger moved horizontally in a screen-resolution
		/// independent way.
		/// </summary>
		/// <param name="touch"></param>
		private void OnTouchMoved(Touch touch)
		{
			if (_doHideHintUI || _doPermanentlyHideHintUI)
			{
				_currentHideHintTime = 0f;
				return;
			}

			if (_priorPosition == null)
			{
				_priorPosition = new Vector3(
					touch.position.x / _screenWidth,
					touch.position.y / _screenHeight);
				return;
			}

			var newPos = new Vector3(
				touch.position.x / _screenWidth,
				touch.position.y / _screenHeight);

			_distanceTouchMoved += Mathf.Abs(_priorPosition.Value.x - newPos.x);
			_priorPosition = newPos;
		}

		private void HideHintUI()
		{
			_doHideHintUI = _doPermanentlyHideHintUI = true;

			if (_hintUICoroutine != null)
			{
				StopCoroutine(_hintUICoroutine);
				_hintUICoroutine = null;
			}

			_hintUICoroutine = StartCoroutine(StopFadeHintUI());
		}

		private IEnumerator LoopFadeHintUI()
		{
			var doFadeOut = false;
			var waitForEndOfFrame = new WaitForEndOfFrame();
			var time = 0f;
			while (true)
			{
				time += Time.unscaledDeltaTime;
				if (doFadeOut)
				{
					_hintUICanvasGroup.alpha = 1 - (time / _fadeDuration);
				}
				else
				{
					_hintUICanvasGroup.alpha = time / _fadeDuration;
				}

				if (_hintUICanvasGroup.alpha <= 0 || _hintUICanvasGroup.alpha >= 1)
				{
					doFadeOut = !doFadeOut;
					time = 0f;
				}

				yield return waitForEndOfFrame;
			}
		}

		private IEnumerator StopFadeHintUI()
		{
			var waitForEndOfFrame = new WaitForEndOfFrame();
			var time = 0f;
			while (_hintUICanvasGroup.alpha > 0)
			{
				time += Time.unscaledDeltaTime;
				_hintUICanvasGroup.alpha = 1 - (time / _fadeDuration);
				yield return waitForEndOfFrame;
			}

			_hintUICoroutine = null;
		}

		private void Update()
		{
			if (_doHideHintUI && !_doPermanentlyHideHintUI)
			{
				_currentHideHintTime += Time.unscaledDeltaTime;

				if (_currentHideHintTime >= _timeToShowHintUI)
				{
					_doHideHintUI = false;
					_hintUICoroutine = StartCoroutine(LoopFadeHintUI());
				}
			}
		}
	}
}
