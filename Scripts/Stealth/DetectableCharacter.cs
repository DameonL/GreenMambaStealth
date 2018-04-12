using System.Collections.Generic;
using UnityEngine;

namespace GreenMambaStealth.Stealth
{
	/// <summary>
	/// A character which is detectable by sensors.
	/// </summary>
	public class DetectableCharacter : MonoBehaviour, IDetectable
	{
		[Tooltip("The LightDetectors for the Stealth Rig. Use the \"Calibrate Stealth Rig\" button to autofill.")]
		[SerializeField]
		private List<LightDetector> _lightDetectors = new List<LightDetector>();

		[Tooltip("How much to multiply the character's stealth rating by. Default of 0.5 is best for most lighting.\nTurn this up in dark environments, or down in bright environments.")]
		[SerializeField]
		private float _stealthMultiplier = 0.5f;

		/// <summary>
		/// A float in the range of (0.01, 1) that represents the visibility of the character.
		/// </summary>
		public float Visibility
		{
			get
			{
				float stealthRating = (_lightDetectors.Count > 0) ? AverageLightLevel() : 1;

				return stealthRating * _stealthMultiplier;
			}
		}

		private float AverageLightLevel()
		{
			float level = 0;

			for (var i = 0; i < _lightDetectors.Count; i++)
			{
				level += _lightDetectors[i].Intensity;
			}

			level /= _lightDetectors.Count;
			return level;
		}

		/// <summary>
		/// Locates all light detectors that are children, and orders them to calibrate themselves.
		/// </summary>
		public void CalibrateStealthRig()
		{
			_lightDetectors.Clear();
			CrawlForDetectors(transform);

			foreach (var detector in _lightDetectors)
			{
				detector.Calibrate();
			}
		}

		private void CrawlForDetectors(Transform rootTransform)
		{
			_lightDetectors.AddRange(rootTransform.GetComponents<LightDetector>());
			if (string.Compare(rootTransform.name, "Stealth Rig") == 0)
			{
				rootTransform.localScale = Vector3.one;
			}

			for (var i = 0; i < rootTransform.childCount; i++)
			{
				CrawlForDetectors(rootTransform.GetChild(i));
			}
		}

	}
}
