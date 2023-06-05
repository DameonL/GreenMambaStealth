using UnityEngine;

namespace GreenMambaStealth.DemoProject
{
	public static class Vector3Extensions
	{
		public static bool Approximately(this Vector3 source, Vector3 target)
		{
			return Mathf.Approximately(source.x, target.x) && Mathf.Approximately(source.y, target.y) && Mathf.Approximately(source.z, target.z);
		}
	}

	public class MouseLook : MonoBehaviour
	{
		[SerializeField]
		private float sensitivity = 15;
		[SerializeField]
		private Vector3 minAngle = new(-90, -360, 0);
		[SerializeField]
		private Vector3 maxAngle = new(90, 360, 0);
		[SerializeField]
		new private Camera camera = null;

		new private Rigidbody rigidbody = null;
		private float verticalRotation = 0;

		private void Awake()
		{
			rigidbody = GetComponent<Rigidbody>();
		}

		private void OnEnable()
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		private void OnDisable()
		{
			Cursor.lockState -= CursorLockMode.Locked;
			Cursor.visible = true;
		}

		private void Update()
		{
			var mouseX = Input.GetAxis("Mouse X");
			var mouseY = Input.GetAxis("Mouse Y");
			if (Mathf.Approximately(mouseX, 0) && Mathf.Approximately(mouseY, 0))
			{
				return;
			}

			Vector3 horizontalDelta = new(0, mouseX * sensitivity, 0);

			var rotation = transform.rotation * Quaternion.Euler(horizontalDelta);
			transform.rotation = rotation;
			rigidbody.MoveRotation(rotation);

			verticalRotation += mouseY * sensitivity;
			verticalRotation = Mathf.Clamp(verticalRotation, minAngle.x, maxAngle.x);
			camera.transform.localRotation = Quaternion.Euler(new Vector3(verticalRotation, 0));
		}
	}
}
