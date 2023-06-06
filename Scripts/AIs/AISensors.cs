﻿using GreenMambaStealth.Stealth;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace GreenMambaStealth.AIs
{
	/// <summary>
	/// Used to detect other characters.
	/// </summary>
	[RequireComponent(typeof(SphereCollider))]
	public class AISensors : MonoBehaviour
	{
		/// <summary>
		/// Triggered when an object is in sensor range, in the field of view, lit enough to be seen, and isn't obscured behind an object.
		/// </summary>
		[Tooltip("Triggered when an object is in sensor range, in the field of view, lit enough to be seen, and isn't obscured behind an object.")]
		public DetectableEvent ObjectVisibleHandler { get; set; } = new();
		/// <summary>
		/// Triggered when an object that was previously visible is no longer visible.
		/// </summary>
		[Tooltip("Triggered when an object that was previously visible is no longer visible.")]
		public DetectableEvent ObjectInvisibleHandler { get; set; } = new();

		[Tooltip("The angle within which this AI can see.")]
		[SerializeField]
		private float _fieldOfView = 110;

		[Tooltip("The minimum Visibility for any IDetectable in this AI's detection range.")]
		[SerializeField]
		private float _minVisibilityForDetection = 0.5f;

		[Tooltip("The distance/visibility curve for this AI. The X axis is distance, the Y axis is a multiplier to Visibility. The highest distance in the curve on the X axis is the AI's maximum sensory range.")]
		[SerializeField]
		private AnimationCurve _visibilityCurve = new();

		[Tooltip("A LayerMask representing which layers will be detectable.")]
		[SerializeField]
		private LayerMask _detectionMask;

		[Tooltip("Tags which are treated as invisible by these sensors.")]
		[SerializeField]
		private List<string> _invisibleTags = new();

#if UNITY_EDITOR
		[Tooltip("If enabled, events from these sensors will be displayed in the debug console.")]
		[SerializeField]
		private bool _verbose;
#endif

		private float _sensoryRange;
		private HashSet<IDetectable> _charactersInRange = new();
		private HashSet<IDetectable> _visibleObjects = new();
		private HashSet<IDetectable> _invisibleObjects = new();


		private void Awake()
		{
			_sensoryRange = _visibilityCurve.keys[_visibilityCurve.length - 1].time;
			GetComponent<SphereCollider>().radius = _sensoryRange;
			GetComponent<SphereCollider>().isTrigger = true;
		}

		private void OnTriggerEnter(Collider other)
		{
			var detectable = other.GetComponent<IDetectable>();
			if (detectable != null)
			{
#if UNITY_EDITOR
				if (_verbose)
					Debug.Log(detectable.gameObject.name + " entered sensor range of " + GetHashCode());
#endif

				_charactersInRange.Add(detectable);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			var detectable = other.GetComponent<IDetectable>();
			if (detectable != null)
			{
#if UNITY_EDITOR
				if (_verbose)
					Debug.Log(detectable.gameObject.name + " left sensor range of " + GetHashCode());
#endif

				_charactersInRange.Remove(detectable);
				if (_visibleObjects.Contains(detectable))
				{
					_visibleObjects.Remove(detectable);
					ObjectInvisibleHandler.Invoke(detectable);
				}
				else
				{
					_invisibleObjects.Remove(detectable);
				}
			}
		}

		private void Update()
		{
			CheckForLineOfSight();
		}

		private void CheckForLineOfSight()
		{
			foreach (var inRange in _charactersInRange)
			{
				if (CanSee(inRange))
				{
					if (_invisibleObjects.Contains(inRange))
					{
#if UNITY_EDITOR
						if (_verbose)
							Debug.LogWarning(inRange.gameObject.name + " became visible to " + GetHashCode());
#endif

						_invisibleObjects.Remove(inRange);
						ObjectVisibleHandler.Invoke(inRange);
					}

					_visibleObjects.Add(inRange);
				}
				else
				{
#if UNITY_EDITOR
					if (_verbose)
						Debug.Log(GetHashCode() + " can't see " + inRange.gameObject.name);
#endif

					if (_visibleObjects.Contains(inRange))
					{
#if UNITY_EDITOR
						if (_verbose)
							Debug.LogWarning(inRange.gameObject.name + " became invisible to " + GetHashCode());
#endif

						_visibleObjects.Remove(inRange);
						ObjectInvisibleHandler.Invoke(inRange);
					}

					_invisibleObjects.Add(inRange);
				}
			}

		}

		private bool CanSee(IDetectable detectable)
		{
			if (detectable == null)
			{
				throw new ArgumentNullException("character");
			}

			float distance = Vector3.Distance(detectable.gameObject.transform.position, transform.position);
			float angle = Vector3.Angle(detectable.gameObject.transform.position - transform.position, transform.forward);

			if (angle > _fieldOfView * 0.5f || angle < -(_fieldOfView * 0.5f))
			{
				if (_verbose)
				{
					Debug.Log($"Out of field of view, angle: {angle} at distance {distance}");
				}
				return false;
			}

			bool isVisible = false;
			float visibilityRating = detectable.Visibility * _visibilityCurve.Evaluate(distance);

			if (visibilityRating >= _minVisibilityForDetection)
			{
				foreach (var tag in _invisibleTags)
				{
					if (detectable.gameObject.CompareTag(tag))
					{
						return false;
					}
				}

				isVisible = true;
			}

			if (isVisible)
			{
				Vector3 targetPosition = detectable.gameObject.GetComponent<Collider>().bounds.center;
				Physics.Raycast(new Ray(transform.position, targetPosition - transform.position), out RaycastHit hit, _sensoryRange, _detectionMask.value);

				if (hit.collider == null || hit.collider.GetComponent<IDetectable>() != detectable)
				{
#if UNITY_EDITOR
					if (_verbose && hit.collider != null)
						Debug.Log(GetHashCode() + "'s line of sight to " + detectable.gameObject.name + " is blocked by " + hit.collider.name);
#endif

					isVisible = false;
				}

				if (hit.collider != null)
				{
#if UNITY_EDITOR
					if (_verbose)
						Debug.DrawRay(transform.position, hit.collider.transform.position - transform.position);
#endif

				}
			}

#if UNITY_EDITOR
			if (_verbose)
				Debug.Log($"{GetHashCode()} - Base Visibility: {detectable.Visibility} / Distance Falloff Applied: {visibilityRating} : {isVisible}");
#endif

			return isVisible;
		}

	}
}
