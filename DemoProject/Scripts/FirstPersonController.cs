using UnityEngine;

namespace GreenMambaStealth.DemoProject
{
	public class FirstPersonController : MonoBehaviour
	{
		public bool Sneaking { get; private set; }
		public bool Crouching { get; private set; }

		[SerializeField]
		private float moveSpeed = .15f;
		[SerializeField]
		private float strafeSpeed = .15f;
		[SerializeField]
		private float sneakMovementModifier = 0.25f;
		[SerializeField]
		private float strafeSneakModifier = 0.25f;
		[SerializeField]
		private float standingScale = 1f;
		[SerializeField]
		private float crouchedScale = 0.5f;
		[SerializeField]
		private float crouchSpeed = 1f;
		[SerializeField]
		private float crouchMovementModifier = 0.25f;

		new private Rigidbody rigidbody = null;

		private int collisionMask = 0;

		private void Awake()
		{
			collisionMask = LayerMask.GetMask("Default");
			rigidbody = GetComponent<Rigidbody>();
		}

		private void Update()
		{
			if (Input.GetButtonDown("Crouch"))
			{
				Crouching = true;
			}
			else if (Input.GetButtonUp("Crouch"))
			{
				Crouching = false;
			}
			else
			{
				AnimateCrouch();
			}

			if (Input.GetButtonDown("Sneak"))
			{
				Sneaking = true;
			}
			else if (Input.GetButtonUp("Sneak"))
			{
				Sneaking = false;
			}
		}

		private void FixedUpdate()
		{
			Move();
		}

		private void AnimateCrouch()
		{
			if (Crouching)
			{
				if (Mathf.Approximately(transform.localScale.y, crouchedScale))
				{
					return;
				}
			}
			else if (!Crouching)
			{
				if (Mathf.Approximately(transform.localScale.y, standingScale))
				{
					return;
				}
			}

			float newScale = Mathf.MoveTowards(transform.localScale.y, Crouching ? crouchedScale : standingScale, Time.deltaTime * crouchSpeed);
			transform.localScale = new Vector3(transform.localScale.x, newScale, transform.localScale.z);
		}

		private void Move()
		{
			float strafeInput = Input.GetAxis("Horizontal");
			float forwardInput = Input.GetAxis("Vertical");

			if (strafeInput == 0 && forwardInput == 0)
			{
				return;
			}

			var forwardMovement = forwardInput * moveSpeed * transform.forward;
			var strafeMovement = strafeInput * strafeSpeed * transform.right;
			if (Sneaking)
			{
				forwardMovement *= sneakMovementModifier;
				strafeMovement *= sneakMovementModifier;
			}

			if (Crouching)
			{
				forwardMovement *= crouchMovementModifier;
				strafeMovement *= crouchMovementModifier;
			}

			var movement = forwardMovement + strafeMovement;
			var newPosition = transform.position + movement;
			var collision = Physics.Raycast(transform.position, newPosition - transform.position, movement.magnitude * 2, collisionMask, QueryTriggerInteraction.Ignore);
			if (collision)
			{
				return;
			}

			rigidbody.MovePosition(newPosition);
		}
	}
}
