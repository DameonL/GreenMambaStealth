using GreenMambaStealth.Stealth;
using UnityEngine;
using UnityEditor;

namespace GreenMambaStealth.Editor.Stealth
{
	[CustomEditor(typeof(DetectableCharacter))]
	public class DetectableCharacterEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			DetectableCharacter detectable = target as DetectableCharacter;
			if (GUILayout.Button(new GUIContent("Calibrate Stealth Rig", "Searches this object's heirarchy to find LightDetectors, and sizes and positions them for best results.\nUse this the first time you add the DetectableCharacter component, or when you change the scale of your character.")))
			{
				detectable.CalibrateStealthRig();
			}
		}
	}
}
