using System.Collections;
using UnityEngine;

namespace Bose.Wearable.Examples
{
	/// <summary>
	/// Plays reactive layered audio corresponding to the player's closeness to the target. 
	/// </summary>
	public class TargetSFX : MonoBehaviour
	{
		/// <summary>
		/// Audio layer with chord accompaniment.
		/// </summary>
		[SerializeField]
		private AudioSource _audioChords;

		/// <summary>
		/// The audio layer to apply when closest to the target.
		/// </summary>
		[SerializeField]
		private AudioSource _audioClose;

		/// <summary>
		/// The audio layer to apply when approaching the target.
		/// </summary>
		[SerializeField]
		private AudioSource _audioMiddle;

		/// <summary>
		/// The audio layer to apply when far from the target.
		/// </summary>
		[SerializeField]
		private AudioSource _audioFar;

		/// <summary>
		/// Audio layer to apply when filling the target.
		/// </summary>
		[SerializeField]
		private AudioSource _audioFill;
		
		/// <summary>
		/// The audio clip to play when a new target spawns.
		/// </summary>
		[SerializeField]
		private AudioClip _sfxSpawn;

		/// <summary>
		/// The audio clip to play when a target is locked.
		/// </summary>
		[SerializeField]
		private AudioClip _sfxLock;

		/// <summary>
		/// The audio clip to play when a target is successfully collected.
		/// </summary>
		[SerializeField]
		private AudioClip _sfxCollect;
		
		[SerializeField]
		private AnimationCurve _farVolumeCurve;
		
		[SerializeField]
		private AnimationCurve _farCutoffCurve;
		
		[SerializeField]
		private AnimationCurve _middleVolumeCurve;
		
		[SerializeField]
		private AnimationCurve _middleCutoffCurve;
		
		[SerializeField]
		private AnimationCurve _closeVolumeCurve;
		
		[SerializeField]
		private AnimationCurve _closeCutoffCurve;

		[SerializeField]
		private AnimationCurve _chordsVolumeCurve;
		
		[SerializeField]
		private AnimationCurve _chordsCutoffCurve;

		[SerializeField]
		private AnimationCurve _fillVolumeCurve;

		[SerializeField]
		private float _fadeInTime;
		
		[SerializeField]
		private float _fadeOutTime;

		[SerializeField]
		private AnimationCurve _fadeCurve;
		
		/// <summary>
		/// The closeness to the target, from 0 to 1. Typically set by the <see cref="TargetController"/>.
		/// </summary>
		public float Closeness
		{
			get { return _closeness; }
			set { _closeness = value; }
		}

		private AudioLowPassFilter _audioChordsFilter;
		private AudioHighPassFilter _audioCloseFilter;
		private AudioHighPassFilter _audioMiddleFilter;
		private AudioLowPassFilter _audioFarFilter;

		private AudioControl _audioControl;

		private float _fillLevel;
		private float _closeness;
		private float _globalGain;

		private void Awake()
		{
			_audioControl = AudioControl.Instance;
		}

		private void Start()
		{
			_audioFarFilter = _audioFar.GetComponent<AudioLowPassFilter>();
			_audioMiddleFilter = _audioMiddle.GetComponent<AudioHighPassFilter>();
			_audioCloseFilter = _audioClose.GetComponent<AudioHighPassFilter>();
			_audioChordsFilter = _audioChords.GetComponent<AudioLowPassFilter>();

			// Schedule the audio clips to play slightly in the future so they have time to sync.
			double startTime = AudioSettings.dspTime + 0.01;
			_audioClose.PlayScheduled(startTime);
			_audioMiddle.PlayScheduled(startTime);
			_audioFar.PlayScheduled(startTime);
			_audioChords.PlayScheduled(startTime);
			_audioFill.PlayScheduled(startTime);

			_globalGain = 0.0f;
			_fillLevel = 0.0f;
		}

		private void Update()
		{
			_audioFill.volume = _globalGain * _fillVolumeCurve.Evaluate(_fillLevel);
			
			_audioChords.volume = _globalGain * _chordsVolumeCurve.Evaluate(_closeness);
			_audioChordsFilter.cutoffFrequency = _chordsCutoffCurve.Evaluate(_closeness);
			
			_audioClose.volume = _globalGain * _closeVolumeCurve.Evaluate(_closeness);
			_audioCloseFilter.cutoffFrequency = _closeCutoffCurve.Evaluate(_closeness);
			
			_audioMiddle.volume = _globalGain * _middleVolumeCurve.Evaluate(_closeness);
			_audioMiddleFilter.cutoffFrequency = _middleCutoffCurve.Evaluate(_closeness);
			
			_audioFar.volume = _globalGain * _farVolumeCurve.Evaluate(_closeness);
			_audioFarFilter.cutoffFrequency = _farCutoffCurve.Evaluate(_closeness);


			// Keep sync between each layer using _audioClose as the master
			if (_audioClose.timeSamples != _audioMiddle.timeSamples)
			{
				_audioMiddle.timeSamples = _audioClose.timeSamples;
			}

			if (_audioClose.timeSamples != _audioFar.timeSamples)
			{
				_audioFar.timeSamples = _audioClose.timeSamples;
			}
		}

		private void OnValidate()
		{
			if (_fadeInTime <= 0.0f)
			{
				_fadeInTime = 0.0001f;
			}
			
			if (_fadeOutTime <= 0.0f)
			{
				_fadeOutTime = 0.001f;
			}
		}

		/// <summary>
		/// Set the charge level, which influences the "fill" sound effect
		/// </summary>
		/// <param name="level"></param>
		public void SetChargeLevel(float level)
		{
			_fillLevel = level;
		}

		/// <summary>
		/// Plays a "target spawn" effect
		/// </summary>
		public void PlaySpawnSting()
		{
			_audioControl.PlayOneShot(_sfxSpawn);
		}

		/// <summary>
		/// Plays a "target locked" effect
		/// </summary>
		public void PlayLockSting()
		{
			_audioControl.PlayOneShot(_sfxLock);
		}

		/// <summary>
		/// Plays a "target collected" string
		/// </summary>
		public void PlayCollectSting()
		{
			_audioControl.PlayOneShot(_sfxCollect);
		}

		public void FadeInAudio()
		{
			StartCoroutine(FadeInternal(false));
		}
		
		/// <summary>
		/// Begins fading out the global gain
		/// </summary>
		public void FadeOutAudio()
		{
			StartCoroutine(FadeInternal(true));
		}
		
		private IEnumerator FadeInternal(bool fadeOut)
		{
			_globalGain = fadeOut ? 1.0f : 0.0f;
			float divisor = 1.0f / (fadeOut ? _fadeOutTime : _fadeInTime);
			for (float amount = 0.0f; amount < 1.0f; amount += Time.deltaTime * divisor)
			{
				_globalGain = _fadeCurve.Evaluate(fadeOut ? 1.0f - amount : amount);
				yield return null;
			}
			_globalGain = fadeOut ? 0.0f : 1.0f;
		}
	}
}
