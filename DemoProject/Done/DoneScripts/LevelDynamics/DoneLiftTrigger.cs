using UnityEngine;
using System.Collections;

public class DoneLiftTrigger : MonoBehaviour
{
	public float timeToDoorsClose = 2f;					// Time since the player entered the lift before the doors close.
	public float timeToLiftStart = 3f;					// Time since the player entered the lift before it starts to move.
	public float timeToEndLevel = 6f;					// Time since the player entered the lift before the level ends.
	public float liftSpeed = 3f;						// The speed at which the lift moves.
	
	
	private GameObject player;							// Reference to the player.
	private Animator playerAnim;						// Reference to the players animator component.
	private DoneHashIDs hash;							// Reference to the HashIDs script.
	private DoneCameraMovement camMovement;				// Reference to the camera movement script.
	private DoneSceneFadeInOut sceneFadeInOut;			// Reference to the SceneFadeInOut script.
	private DoneLiftDoorsTracking liftDoorsTracking;	// Reference to LiftDoorsTracking script.
	private bool playerInLift;							// Whether the player is in the lift or not.
	private float timer;								// Timer to determine when the lift moves and when the level ends.
	
	
	void Awake ()
	{
		// Setting up references.
		player = GameObject.FindGameObjectWithTag(DoneTags.player);
		playerAnim = player.GetComponent<Animator>();
		hash = GameObject.FindGameObjectWithTag(DoneTags.gameController).GetComponent<DoneHashIDs>();
		camMovement = Camera.main.gameObject.GetComponent<DoneCameraMovement>();
		sceneFadeInOut = GameObject.FindGameObjectWithTag(DoneTags.fader).GetComponent<DoneSceneFadeInOut>();
		liftDoorsTracking = GetComponent<DoneLiftDoorsTracking>();
	}
	
	
	void OnTriggerEnter (Collider other)
	{
		// If the colliding gameobject is the player...
		if(other.gameObject == player)
			// ... the player is in the lift.
			playerInLift = true;
	}
	
	
	void OnTriggerExit (Collider other)
	{
		// If the player leaves the trigger area...
		if(other.gameObject == player)
		{
			// ... reset the timer, the player is no longer in the lift and unparent the player from the lift.
			playerInLift = false;
			timer = 0;
		}
	}
	
	
	void Update ()
	{
		// If the player is in the lift...
		if(playerInLift)
			// ... activate the lift.
			LiftActivation();
		
		// If the timer is less than the time before the doors close...
		if(timer < timeToDoorsClose)
			// ... the inner doors should follow the outer doors.
			liftDoorsTracking.DoorFollowing();
		else
			// Otherwise the doors should close.
			liftDoorsTracking.CloseDoors();
	}
	
	
	void LiftActivation ()
	{
		// Increment the timer by the amount of time since the last frame.
		timer += Time.deltaTime;
		
		// If the timer is greater than the amount of time before the lift should start...
		if(timer >= timeToLiftStart)
		{
			// ... stop the player and the camera moving and parent the player to the lift.
			playerAnim.SetFloat(hash.speedFloat,0f);
			camMovement.enabled = false;
			player.transform.parent = transform;
			
			// Move the lift upwards.
			transform.Translate(Vector3.up * liftSpeed * Time.deltaTime);
			
			// If the audio clip isn't playing...
			if(!GetComponent<AudioSource>().isPlaying)
				// ... play the clip.
				GetComponent<AudioSource>().Play();
			
			// If the timer is greater than the amount of time before the level should end...
			if(timer >= timeToEndLevel)
				// ... call the EndScene function.
				sceneFadeInOut.EndScene();
		}
	}
}
