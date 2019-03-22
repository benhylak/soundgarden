using System;
using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bose.Wearable.Examples
{
	/// <summary>
	/// Supports gracefully transitions between scenes by way of a loading scene.
	/// </summary>
	[RequireComponent(typeof(CanvasGroup))]
	public class LoadingUIPanel : Singleton<LoadingUIPanel>
	{
		[Header("UX Refs")]
		[SerializeField]
		private Canvas _canvas;

		[SerializeField]
		private CanvasGroup _canvasGroup;

		[SerializeField]
		private CanvasGroup _iconCanvasGroup;

		[SerializeField]
		private RectTransform _iconRectTransform;

		[Header("Animation"), Space(5)]
		[SerializeField]
		private AnimationCurve _fadeInCurve;

		[SerializeField]
		private AnimationCurve _fadeOutCurve;

		[SerializeField]
		[Range(0, float.MaxValue)]
		private float _bgFadeDuration = 1f;

		[SerializeField]
		[Range(0, float.MaxValue)]
		private float _iconFadeDuration = 0.33f;

		[SerializeField]
		[Range(0, float.MaxValue)]
		private float _minimumLoadingDuration = 1f;

		[SerializeField]
		private float _iconRotationSpeed = 50f;

		protected override void Awake()
		{
			base.Awake();

			_canvas.enabled = false;
			_canvasGroup.alpha = _iconCanvasGroup.alpha = 0f;
		}

		/// <summary>
		/// Loads a scene with a loading screen gracefully fading in and out to cover the transition.
		/// </summary>
		/// <param name="sceneName"></param>
		/// <param name="mode"></param>
		/// <param name="onComplete"></param>
		public void LoadScene(string sceneName, LoadSceneMode mode, Action onComplete = null)
		{
			StartCoroutine(TransitionScene(sceneName, mode, onComplete:onComplete));
		}

		/// <summary>
		/// Loads a scene with the loading screen appearing immediately
		/// </summary>
		/// <param name="sceneName"></param>
		/// <param name="mode"></param>
		/// <param name="onComplete"></param>
		public void LoadSceneWithoutFadeOut(string sceneName, LoadSceneMode mode, Action onComplete = null)
		{
			StartCoroutine(TransitionScene(sceneName, mode, doFadeIn:false, onComplete:onComplete));
		}

		/// <summary>
		/// Transitions a scene into the app
		/// </summary>
		/// <param name="sceneName"></param>
		/// <param name="mode"></param>
		/// <param name="doFadeIn"></param>
		/// <param name="onComplete"></param>
		/// <returns></returns>
		private IEnumerator TransitionScene(string sceneName, LoadSceneMode mode, bool doFadeIn = true, Action onComplete = null)
		{
			_canvas.enabled = true;

			if (doFadeIn)
			{
				yield return FadeCanvasGroup(_canvasGroup, _bgFadeDuration, _fadeOutCurve);
				yield return FadeCanvasGroup(_iconCanvasGroup, _iconFadeDuration, _fadeOutCurve);
			}
			else
			{
				_canvasGroup.alpha = _iconCanvasGroup.alpha = 1f;
			}

			var time = 0f;
			var waitForEndOfFrame = new WaitForEndOfFrame();
			var asyncOp = SceneManager.LoadSceneAsync(sceneName, mode);
			while (asyncOp != null && (!asyncOp.isDone || time < _minimumLoadingDuration))
			{
				time += Time.unscaledDeltaTime;

				_iconRectTransform.Rotate(0f, 0f, -_iconRotationSpeed * Time.deltaTime);

				yield return waitForEndOfFrame;
			}

			yield return FadeCanvasGroup(_iconCanvasGroup, _iconFadeDuration, _fadeInCurve);
			yield return FadeCanvasGroup(_canvasGroup, _bgFadeDuration, _fadeInCurve);

			_canvas.enabled = false;

			if (onComplete != null)
			{
				onComplete();
			}
		}

		/// <summary>
		/// Fades a canvas group over time where <paramref name="curve"/> dictates the value set.
		/// </summary>
		/// <param name="canvasGroup"></param>
		/// <param name="duration"></param>
		/// <param name="curve"></param>
		/// <returns></returns>
		private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float duration, AnimationCurve curve)
		{
			var waitForEndOfFrame = new WaitForEndOfFrame();
			var time = 0f;
			while (time <= duration)
			{
				canvasGroup.alpha = curve.Evaluate(time / duration);
				time += Time.unscaledDeltaTime;

				yield return waitForEndOfFrame;
			}

			canvasGroup.alpha = curve.Evaluate(1);
		}
	}
}
