using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Bose.Wearable
{
	public class AudioControl : Singleton<AudioControl>
	{
		[SerializeField]
		private List<AudioSource> _sourcePool;

		[SerializeField]
		private List<AudioSource> _sourcesInUse;

		private readonly int POOL_INIT_SIZE = 20;

		protected override void Awake()
		{
			base.Awake();

			SetupPool();
		}

		private void SetupPool()
		{
			if (_sourcePool == null)
			{
				_sourcePool = new List<AudioSource>(POOL_INIT_SIZE);
			}

			if (_sourcesInUse == null)
			{
				_sourcesInUse = new List<AudioSource>();
			}

			for (int i = 0; i < POOL_INIT_SIZE; ++i)
			{
				CreateSource();
			}
		}

		private void CreateSource()
		{
			GameObject goSource = new GameObject("AudioSource");
			goSource.transform.SetParent(transform);

			AudioSource source = goSource.AddComponent<AudioSource>();
			source.Stop();
			_sourcePool.Add(source);
		}

		private bool ReclaimSource()
		{
			bool result = false;

			for (int i = _sourcesInUse.Count - 1; i >= 0; --i)
			{
				if (_sourcesInUse[i] == null)
				{
					_sourcesInUse.RemoveAt(i);
					continue;
				}

				if (_sourcesInUse[i].isPlaying || _sourcesInUse[i].loop)
				{
					continue;
				}

				// if we find a source that we can reclaim, add it to the pool
				// and ensure it gets re-parented to the singleton.
				var source = _sourcesInUse[i];
				_sourcesInUse.RemoveAt(i);

				_sourcePool.Add(source);

				result = true;
				break;
			}

			return result;
		}

		public AudioSource GetSource(bool permanent = false, bool isThreeD = false)
		{
			AudioSource result = null;

			if (permanent)
			{
				CreateSource();
			}
			else if (_sourcePool.Count == 0)
			{
				if (!ReclaimSource())
				{
					CreateSource();
				}
			}

			result = _sourcePool[0];
			_sourcePool.RemoveAt(0);

			result.playOnAwake = false;
			result.loop = false;
			result.mute = false;
			result.volume = 1f;
			result.spatialBlend = isThreeD ? 1f : 0f;

			if (!permanent)
			{
				_sourcesInUse.Add(result);
			}

			return result;
		}


		public void PlayOneShot(AudioClip clip, float volume = 1f)
		{
			var source = GetSource();

			source.PlayOneShot(clip, volume);
		}

		public void FadeOut(AudioSource source, float duration)
		{
			StartCoroutine(FadeOutInternal(source, duration));
		}

		private IEnumerator FadeOutInternal(AudioSource source, float duration)
		{
			// clamp the max duration to be no more than the remaining amount
			// of the clip if we're not looping.
			if (!source.loop)
			{
				duration = Mathf.Min(duration, source.clip.length - source.time);
			}

			float startVolume = source.volume;
			float remainingDuration = duration;

			while (remainingDuration > 0f)
			{
				source.volume = Mathf.Lerp(startVolume, 0f, 1f - Mathf.Clamp01(remainingDuration / duration));

				remainingDuration -= Time.deltaTime;

				yield return null;
			}

			source.Stop();
		}

		public void FadeIn(AudioSource source, float duration)
		{
			StartCoroutine(FadeInInternal(source, duration));
		}

		private IEnumerator FadeInInternal(AudioSource source, float duration)
		{
			if (!source.isPlaying)
			{
				source.Play();
			}

			// clamp the max duration to be no more than the remaining amount
			// of the clip if we're not looping.
			if (!source.loop)
			{
				duration = Mathf.Min(duration, source.clip.length - source.time);
			}

			float remainingDuration = duration;

			source.volume = 0f;

			while (remainingDuration > 0f)
			{
				source.volume = 1f - Mathf.Clamp01(remainingDuration / duration);

				remainingDuration -= Time.deltaTime;

				yield return null;
			}
		}
	}
}
