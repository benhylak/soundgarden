using System.Collections;
using UnityEngine;

namespace Bose.Wearable
{
	/// <summary>
	/// Used internally whenever a coroutine has to wait for a period of time.
	/// Similar to UnityEngine.WaitForSecondsRealtime, but can be reused multiple times via Reset.
	/// See: https://forum.unity.com/threads/cant-reuse-waitforsecondsrealtime.539533/
	/// </summary>
	internal sealed class WaitForSecondsRealtimeCacheable : IEnumerator
	{
		private float _duration;
		private float _targetTime;

		internal WaitForSecondsRealtimeCacheable(float duration)
		{
			_duration = duration;
			Reset();
		}

		#region IEnumerator

		public object Current
		{
			get { return null; }
		}

		public bool MoveNext()
		{
			return (Time.realtimeSinceStartup < _targetTime);
		}

		public void Reset()
		{
			_targetTime = Time.realtimeSinceStartup + _duration;
		}

		public IEnumerator Restart()
		{
			Reset();
			return this;
		}

		#endregion
	}
}
