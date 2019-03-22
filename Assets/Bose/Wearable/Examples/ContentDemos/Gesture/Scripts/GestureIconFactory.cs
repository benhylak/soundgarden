using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.Linq;
#endif

namespace Bose.Wearable.Examples
{
	/// <summary>
	/// <see cref="GestureIconFactory"/> is a Sprite factory where a <see cref="GestureId"/> has a 1:1 mapping
	/// to a <see cref="Sprite"/> asset in the project.
	/// </summary>
	public sealed class GestureIconFactory : ScriptableObject
	{
		[Serializable]
		private class GestureToIcon
		{
			#pragma warning disable 0649
			public GestureId gestureId;

			public Sprite gestureSprite;
			#pragma warning restore 0649
		}

		[SerializeField]
		private List<GestureToIcon> _gestureToIcons;

		private Dictionary<GestureId, GestureToIcon> _gestureIconLookup;

		private const string GestureMappingMissingWarningFormat =
			"[Bose Wearable] A gesture mapping is missing for gesture [{0}] on GestureIconFactory instance.";
		private const string GestureIconUnassignedWarningFormat =
			"[Bose Wearable] An icon is not assigned for gesture [{0}] on GestureIconFactory instance.";
		private const string GestureIconMappingIsDuplicated =
			"[Bose Wearable] There is more than one icon mapping for gesture [{0}]  on GestureIconFactory instance.";

		private void OnEnable()
		{
			_gestureIconLookup = new Dictionary<GestureId, GestureToIcon>();
			for (var i = 0; i < _gestureToIcons.Count; i++)
			{
				if (_gestureIconLookup.ContainsKey(_gestureToIcons[i].gestureId))
				{
					continue;
				}

				_gestureIconLookup.Add(_gestureToIcons[i].gestureId, _gestureToIcons[i]);
			}
		}

		/// <summary>
		/// Returns true if a mapping is found for <see cref="GestureId"/> <paramref name="gestureId"/> to a
		/// <see cref="Sprite"/>, otherwise false.
		/// </summary>
		/// <param name="gestureId"></param>
		/// <param name="sprite"></param>
		/// <returns></returns>
		public bool TryGetGestureIcon(GestureId gestureId, out Sprite sprite)
		{
			sprite = null;

			GestureToIcon gestureToIcon;
			if (!_gestureIconLookup.TryGetValue(gestureId, out gestureToIcon))
			{
				Debug.LogWarningFormat(this, GestureMappingMissingWarningFormat, gestureId);
				return false;
			}

			sprite = gestureToIcon.gestureSprite;

			return true;
		}

		#if UNITY_EDITOR

		private void OnValidate()
		{
			// Iterate through all gestures and ensure there is a single icon mapping for each one.
			// Flag any duplicate icon mappings for gestures.
			for (var i = 0; i < WearableConstants.GestureIds.Length; i++)
			{
				var gestureId = WearableConstants.GestureIds[i];

				// Skip this invalid gesture type.
				if (gestureId == GestureId.None)
				{
					continue;
				}

				// If we have an existing mapping for this gesture, skip it.
				if (_gestureToIcons.Any(x => x.gestureId == gestureId))
				{
					if (_gestureToIcons.Count(x => x.gestureId == gestureId) > 1)
					{
						Debug.LogWarningFormat(this, GestureIconMappingIsDuplicated, gestureId);
					}

					continue;
				}

				// Where we do not find a mapping for this gesture, add one.
				_gestureToIcons.Add(new GestureToIcon
				{
					gestureId = gestureId
				});
			}

			// Ensure all icon mappings have a sprite assigned.
			for (var i = 0; i < _gestureToIcons.Count; i++)
			{
				if (_gestureToIcons[i].gestureSprite != null)
				{
					continue;
				}

				Debug.LogWarningFormat(this, GestureIconUnassignedWarningFormat, _gestureToIcons[i].gestureId);
			}
		}

		private void Reset()
		{
			_gestureToIcons = new List<GestureToIcon>();
			for (var i = 0; i < WearableConstants.GestureIds.Length; i++)
			{
				if (WearableConstants.GestureIds[i] == GestureId.None)
				{
					continue;
				}

				_gestureToIcons.Add(new GestureToIcon
				{
					gestureId = WearableConstants.GestureIds[i]
				});
			}
		}

		#endif
	}
}
