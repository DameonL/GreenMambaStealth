using UnityEngine;
using UnityEngine.UI;

namespace GreenMambaStealth.UI
{
	public class Dial : MonoBehaviour
	{
		[SerializeField]
		private Image _hand;
		[SerializeField]
		private float _value;
		public float Value
		{
			get
			{
				return _value;
			}

			set
			{
				_value = value;
				OnValueUpdated();
			}
		}
		[SerializeField]
		private float _maxValue;
		public float MaxValue { get { return _maxValue; } private set { _maxValue = value; } }

		private float _visibleValue;
		private RectTransform _rectTransform;

		private void OnValueUpdated()
		{
			if (_value > _maxValue)
			{
				_value = _maxValue;
			}
			if (_value < 0)
			{
				_value = 0;
			}
			var rotation = -((180 * (_value / _maxValue)) - 90);
			if (!float.IsNaN(rotation))
				_hand.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));
		}

	}
}
