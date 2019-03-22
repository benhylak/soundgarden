using System.Collections;
using UnityEngine;

namespace Bose.Wearable
{
	public class ButtonSizeEffect : ButtonScaleBase
	{
		[Header("Animation"), Space(5)]
		[SerializeField]
		[Range(0f, 1f)]
		protected float _duration;
		
		[SerializeField]
		private Vector2 _sizeDown;
		
		[SerializeField]
		private Vector2 _sizeUp;
		
		/// <summary>
		/// Reset the component to default values. Automatically called whenever a component is added.
		/// </summary>
		private void Reset()
		{
			_duration = 0.1f;
			_sizeDown = new Vector2(30f, 20f);
			_sizeUp = Vector2.zero;
		}

		protected override void AnimateDown()
		{
			_effectCoroutine = StartCoroutine(AnimateEffect(_buttonRectTransform, _sizeDown));
		}

		protected override void AnimateUp()
		{
			_effectCoroutine = StartCoroutine(AnimateEffect(_buttonRectTransform, _sizeUp));
		}

		private IEnumerator AnimateEffect(RectTransform rectTransform, Vector3 targetValue)
		{
			var timeLeft = _duration;
			while (timeLeft > 0f)
			{
				rectTransform.sizeDelta = Vector2.Lerp(
					rectTransform.sizeDelta,
					targetValue,
					Mathf.Clamp01(1f - (timeLeft / _duration)));

				timeLeft -= Time.unscaledDeltaTime;
				yield return _cachedWaitForEndOfFrame;
			}
			
			rectTransform.sizeDelta = targetValue;
		}
	}
}
