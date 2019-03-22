using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Bose.Wearable.Examples
{
	/// <summary>
	/// <see cref="GestureDemoUIPanel"/> is used to represent all gesture types as displays that animate when
	/// the user makes the correlating gesture.
	/// </summary>
	[RequireComponent(typeof(Canvas))]
	public sealed class GestureDemoUIPanel : MonoBehaviour
	{
		[Header("UX Refs")]
		[SerializeField]
		private Button _backButton;

		[SerializeField]
		private Transform _rootTransform;

		[Header("Prefab/Factory Refs"), Space(5)]
		[SerializeField]
		private GestureIconFactory _gestureIconFactory;

		[SerializeField]
		private GestureIconFactory _gestureGlowIconFactory;

		[SerializeField]
		private GestureDisplay _gestureDisplay;

		private const string GestureIconNotFoundFormat = "[Bose Wearable] Skipped creating a GestureDisplay " +
		                                                 "for gesture [{0}].";

		private void Start ()
		{
			_backButton.onClick.AddListener(OnBackButtonClicked);

			var rootChildCount = _rootTransform.childCount;
			for (var i = rootChildCount - 1; i >= 0; i--)
			{
				var childGameObject = _rootTransform.GetChild(i);
				Destroy(childGameObject.gameObject);
			}

			for (var i = 0; i < WearableConstants.GestureIds.Length; i++)
			{
				if (WearableConstants.GestureIds[i] == GestureId.None)
				{
					continue;
				}

				Sprite sprite;
				if (!_gestureIconFactory.TryGetGestureIcon(WearableConstants.GestureIds[i], out sprite))
				{
					Debug.LogWarningFormat(this, GestureIconNotFoundFormat, WearableConstants.GestureIds[i]);
					continue;
				}

				Sprite glowSprite;
				if (!_gestureGlowIconFactory.TryGetGestureIcon(WearableConstants.GestureIds[i], out glowSprite))
				{
					Debug.LogWarningFormat(this, GestureIconNotFoundFormat, WearableConstants.GestureIds[i]);
					continue;
				}
				
				var gestureDisplay = Instantiate(_gestureDisplay, _rootTransform, false);
				gestureDisplay.Set(WearableConstants.GestureIds[i], sprite, glowSprite);
			}
		}

		private void OnDestroy()
		{
			_backButton.onClick.RemoveAllListeners();
		}

		private void OnBackButtonClicked()
		{
			LoadingUIPanel.Instance.LoadScene(WearableConstants.MainMenuScene, LoadSceneMode.Single);
		}
	}
}
