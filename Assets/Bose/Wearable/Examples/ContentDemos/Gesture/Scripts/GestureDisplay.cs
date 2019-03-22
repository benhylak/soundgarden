using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Bose.Wearable.Examples
{
	/// <summary>
	/// <see cref="GestureDisplay"/> is used to display a sprite representing a specific Gesture
	/// </summary>
	public sealed class GestureDisplay : MonoBehaviour
	{
		[Header("UX Refs")]
		[SerializeField]
		private GestureDetector _gestureDetector;

		[SerializeField]
		private Image _gestureIcon;

		[SerializeField]
		private Image _gestureGlowIcon;

		[Header("Animation"), Space(5)]
		[SerializeField]
		private float _animationScale;

		[SerializeField]
		private AnimationCurve _effectsAnimationCurve;

		[SerializeField]
		private float _animationDuration;

		[Header("Audio"), Space(5)]
		[SerializeField]
		private AudioClip _sfxClip;

		private Coroutine _animateCoroutine;

		/// <summary>
		/// Sets the appropriate <see cref="GestureId"/> <paramref name="gestureId"/> to detect and display.
		/// </summary>
		/// <param name="gestureId"></param>
		/// <param name="gestureSpriteIcon"></param>
		/// <param name="gestureGlowSpriteIcon"></param>
		public void Set(GestureId gestureId, Sprite gestureSpriteIcon, Sprite gestureGlowSpriteIcon)
		{
			Assert.IsFalse(gestureId == GestureId.None, string.Format(WearableConstants.NoneIsInvalidGesture, GetType()));

			_gestureDetector.Gesture = gestureId;

			_gestureIcon.sprite = gestureSpriteIcon;
			_gestureGlowIcon.sprite = gestureGlowSpriteIcon;
		}

		/// <summary>
		/// Upon the appropriate gesture triggering, animate this display.
		/// </summary>
		public void OnGestureAnimate()
		{
			if (_animateCoroutine != null)
			{
				StopCoroutine(_animateCoroutine);

				_animateCoroutine = null;
			}

			_animateCoroutine = StartCoroutine(Animate());
		}

		/// <summary>
		/// Animate the gesture icon with a scale up then down animation.
		/// </summary>
		/// <returns></returns>
		private IEnumerator Animate()
		{
			AudioControl.Instance.PlayOneShot(_sfxClip);

			Color color;
			var currentTime = 0f;
			var finalScale = _gestureIcon.rectTransform.localScale * _animationScale;
			while (currentTime <= _animationDuration)
			{
				var eval = _effectsAnimationCurve.Evaluate(currentTime / _animationDuration);
				var scale = Vector3.Lerp(
					Vector3.one,
					finalScale,
					eval);
				_gestureIcon.rectTransform.localScale = scale;
				_gestureGlowIcon.rectTransform.localScale = scale;

				color = _gestureGlowIcon.color;
				color.a = eval;

				_gestureGlowIcon.color = color;

				currentTime += Time.unscaledDeltaTime;
				yield return null;
			}

			_gestureIcon.rectTransform.localScale = Vector3.one;
			_gestureGlowIcon.rectTransform.localScale = Vector3.one;

			color = _gestureGlowIcon.color;
			color.a = 0f;
			_gestureGlowIcon.color = color;

			_animateCoroutine = null;
		}

		#if UNITY_EDITOR

		private void Reset()
		{
			_animationScale = 1.3f;
			_animationDuration = 0.5f;
		}

		#endif
	}
}
