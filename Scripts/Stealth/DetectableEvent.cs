using System;
using UnityEngine.Events;

namespace DrunkenMambaStealth.Stealth
{
	/// <summary>
	/// An event that passes a single argument, which is the Detectable which the event is based around.
	/// </summary>
	[Serializable]
	public class DetectableEvent : UnityEvent<IDetectable>
	{
	}
}
