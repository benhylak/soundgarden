using System;
using UnityEngine;

namespace Bose.Wearable.Examples
{
	/// <summary>
	/// Processes single-touch drags. Subsequent touches are ignored for the duration of the drag.
	/// </summary>
	public class SingleTouchRecognizer : Singleton<SingleTouchRecognizer>
	{
		/// <summary>
		/// Called when a new touch begins.
		/// </summary>
		public event Action<Touch> TouchBegan;

		/// <summary>
		/// Continuously called when a tracked touch moves.
		/// </summary>
		public event Action<Touch> TouchMoved;

		/// <summary>
		/// Called when a touch ends.
		/// </summary>
		public event Action<Touch> TouchEnded;

		private bool _touching;
		private int _activeTouchId;

		protected override void Awake()
		{
			Input.multiTouchEnabled = true;
			Input.simulateMouseWithTouches = false;
			_touching = false;
			_activeTouchId = -1;

			base.Awake();
		}

		private void Update()
		{
			// Scan through incoming, continuing, and ending touches since last frame
			int touches = Input.touchCount;
			for (int i = 0; i < touches; i++)
			{
				Touch touch = Input.GetTouch(i);
				if (_touching)
				{
					// If we're tracking a touch already, look for that ID in the list
					if (touch.fingerId == _activeTouchId)
					{
						if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
						{
							// Touch ended; cancel tracking
							_activeTouchId = -1;
							_touching = false;
							if (TouchEnded != null)
							{
								TouchEnded(touch);
							}
						}
						else if (touch.phase == TouchPhase.Moved)
						{
							// Touch still occuring and moved
							if (TouchMoved != null)
							{
								TouchMoved(touch);
							}
						}

						break;
					}
				}
				else if (touch.phase == TouchPhase.Began)
				{
					// Otherwise, start tracking the first touch to begin
					_activeTouchId = touch.fingerId;
					_touching = true;

					if (TouchBegan != null)
					{
						TouchBegan(touch);
					}

					break;
				}
			}
		}
	}
}
