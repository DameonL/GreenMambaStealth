﻿using GreenMambaStealth.Stealth;
using UnityEngine;

namespace GreenMambaStealth.UI
{
	public class VisibilityMeter : MonoBehaviour
	{
		[SerializeField]
		private DetectableCharacter _detectable;

		private Dial _dial;

		private void Awake()
		{
			_dial = GetComponent<Dial>();
		}

		private void Update()
		{
			if (_detectable != null)
				_dial.Value = _detectable.Visibility;
		}
	}
}
