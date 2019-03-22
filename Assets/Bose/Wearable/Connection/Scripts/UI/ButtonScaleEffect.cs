using System.Collections;
using UnityEngine;

namespace Bose.Wearable
{
	public class ButtonScaleEffect : ButtonScaleBase
	{
		[Header("Animation"), Space(5)]
		[SerializeField]
		[Range(0f, 1f)]
		protected float _duration;
		
		[SerializeField]
		private Vector3 _scaleDown;
		
		[SerializeField]
		private Vector3 _scaleUp;
		
		/// <summary>
		/// Reset the component to default values. Automatically called whenever a component is added.
		/// </summary>
		private void Reset()
		{
			_duration = 0.1f;
			_scaleDown = new Vector3(1.1f, 1.1f, 1.1f);
			_scaleUp = Vector3.one;
		}

		protected override void AnimateDown()
		{
			_effectCoroutine = StartCoroutine(AnimateEffect(_buttonRectTransform, _scaleDown));
		}

		protected override void AnimateUp()
		{
			_effectCoroutine = StartCoroutine(AnimateEffect(_buttonRectTransform, _scaleUp));
		}

		private IEnumerator AnimateEffect(RectTransform rectTransform, Vector3 targetValue)
		{
			var timeLeft = _duration;
			while (timeLeft > 0f)
			{
				rectTransform.localScale = Vector3.Lerp(
					rectTransform.localScale,
					targetValue,
					Mathf.Clamp01(1f - (timeLeft / _duration)));

				timeLeft -= Time.unscaledDeltaTime;
				yield return _cachedWaitForEndOfFrame;
			}

			rectTransform.localScale = targetValue;
		}
	}
}
