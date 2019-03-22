using UnityEngine;
using UnityEngine.EventSystems;

namespace Bose.Wearable
{
	public abstract class ButtonScaleBase : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler
	{
		[Header("UX Refs")]
		[SerializeField]
		protected RectTransform _buttonRectTransform;

		[Header("Animation"), Space(5)]
		[SerializeField]
		private AudioClip _sfxClick;

		private AudioControl _audioControl;
		protected Coroutine _effectCoroutine;
		protected WaitForEndOfFrame _cachedWaitForEndOfFrame;

		private void Start()
		{
			_audioControl = AudioControl.Instance;
			_cachedWaitForEndOfFrame = new WaitForEndOfFrame();
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			AnimateUpInternal();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			AnimateDownInternal();
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			PlayClickSting();
		}

		private void AnimateUpInternal()
		{
			if (_effectCoroutine != null)
			{
				StopCoroutine(_effectCoroutine);
				_effectCoroutine = null;
			}
			
			AnimateUp();
		}
		
		protected abstract void AnimateUp();
		
		private void AnimateDownInternal()
		{
			if (_effectCoroutine != null)
			{
				StopCoroutine(_effectCoroutine);
				_effectCoroutine = null;
			}
			
			AnimateDown();
		}

		protected abstract void AnimateDown();

		private void PlayClickSting()
		{
			if (_sfxClick != null)
			{
				_audioControl.PlayOneShot(_sfxClick);
			}
		}
	}
}
