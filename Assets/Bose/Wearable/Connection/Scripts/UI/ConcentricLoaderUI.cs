using UnityEngine;
using UnityEngine.UI;

namespace Bose.Wearable
{
	public class ConcentricLoaderUI : MonoBehaviour
	{
		[SerializeField]
		protected AnimationCurve _radiusCurve;

		[SerializeField]
		protected AnimationCurve _opacityCurve;

		[SerializeField]
		protected float _spacing;

		[SerializeField]
		protected float _period;

		[SerializeField]
		protected float _speed;

		[SerializeField]
		protected Color _lightColor;

		[SerializeField]
		protected Color _darkColor;

		private float _time;
		private Transform[] _childTransforms;
		private Image[] _childImages;

		private void Start ()
		{
			_radiusCurve.postWrapMode = WrapMode.Clamp;
			_opacityCurve.postWrapMode = WrapMode.Clamp;

			_childTransforms = new Transform[transform.childCount];
			_childImages = new Image[transform.childCount];
			for (int i = 0; i < _childTransforms.Length; i++)
			{
				_childTransforms[i] = transform.GetChild(i);
				_childImages[i] = _childTransforms[i].GetComponent<Image>();
				_childImages[i].color = (i % 2 == 0) ? _lightColor : _darkColor;
			}
		}

		private void Update ()
		{
			_time += Time.unscaledDeltaTime * _speed;

			if (_time > _period * _speed)
			{
				_time -= _period * _speed;
			}

			float t = _time;
			for (int i = 0; i < _childTransforms.Length; i++)
			{
				_childTransforms[i].localScale = Vector3.one * _radiusCurve.Evaluate(t);
				_childImages[i].color = Color.Lerp(_darkColor, (i % 2 == 0) ? _lightColor : _darkColor, _opacityCurve.Evaluate(t));
				t += _spacing;
			}
		}
	}
}
