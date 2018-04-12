using UnityEngine;

namespace GreenMambaStealth.Stealth
{
	/// <summary>
	/// An object meant to be detectable as part of the stealth system.
	/// </summary>
	public interface IDetectable
	{
		/// <summary>
		/// A float in the range of (0.01, 1) that represents the visibility of the character.
		/// </summary>
		float Visibility { get; }
		GameObject gameObject { get; }
	}
}
