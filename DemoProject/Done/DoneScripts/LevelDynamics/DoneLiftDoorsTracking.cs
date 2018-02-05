using UnityEngine;
using System.Collections;

public class DoneLiftDoorsTracking : MonoBehaviour
{
	public float doorSpeed = 7f;			// How quickly the inner doors will track the outer doors.
	
	
	private Transform leftOuterDoor;		// Reference to the transform of the left outer door.
	private Transform rightOuterDoor;		// Reference to the transform of the right outer door.
	private Transform leftInnerDoor;		// Reference to the transform of the left inner door.
	private Transform rightInnerDoor;		// Reference to the transform of the right inner door.
	private float leftClosedPosX;			// The initial x component of position of the left doors.
	private float rightClosedPosX;			// The initial x component of position of the right doors.
	
	
	void Awake ()
	{
		// Setting up the references.
		leftOuterDoor = GameObject.Find("door_exit_outer_left_001").transform;
		rightOuterDoor = GameObject.Find("door_exit_outer_right_001").transform;
		leftInnerDoor = GameObject.Find("door_exit_inner_left_001").transform;
		rightInnerDoor = GameObject.Find("door_exit_inner_right_001").transform;
		
		// Setting the closed x position of the doors.
		leftClosedPosX = leftInnerDoor.position.x;
		rightClosedPosX = rightInnerDoor.position.x;
	}
	
	
	void MoveDoors (float newLeftXTarget, float newRightXTarget)
	{
		// Create a float that is a proportion of the distance from the left inner door's x position to it's target x position.
		float newX = Mathf.Lerp(leftInnerDoor.position.x, newLeftXTarget, doorSpeed * Time.deltaTime);
		
		// Move the left inner door to it's new position proportionally closer to it's target.
		leftInnerDoor.position = new Vector3(newX, leftInnerDoor.position.y, leftInnerDoor.position.z);
		
		// Reassign the float for the right door's x position.
		newX = Mathf.Lerp(rightInnerDoor.position.x, newRightXTarget, doorSpeed * Time.deltaTime);
		
		// Move the right inner door similarly.
		rightInnerDoor.position = new Vector3(newX, rightInnerDoor.position.y, rightInnerDoor.position.z);
	}
	
	
	public void DoorFollowing ()
	{
		// Move the inner doors towards the outer doors.
		MoveDoors(leftOuterDoor.position.x, rightOuterDoor.position.x);
	}
	
	
	public void CloseDoors ()
	{
		// Move the inner doors towards their closed position.
		MoveDoors(leftClosedPosX, rightClosedPosX);
	}
}
