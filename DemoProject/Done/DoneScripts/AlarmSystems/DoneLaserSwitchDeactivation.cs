using UnityEngine;
using System.Collections;

public class DoneLaserSwitchDeactivation : MonoBehaviour
{
	public GameObject laser;				// Reference to the laser that can we turned off at this switch.
	public Material unlockedMat;		 	// The screen's material to show the laser has been unloacked.
	
	
	private GameObject player;				// Reference to the player.
	
	
	void Awake ()
	{
		// Setting up the reference.
		player = GameObject.FindGameObjectWithTag(DoneTags.player);
	}
	
	
	void OnTriggerStay (Collider other)
	{
		// If the colliding gameobject is the player...
		if(other.gameObject == player)
			// ... and the switch button is pressed...
			if(Input.GetButton("Switch"))
				// ... deactivate the laser.
				LaserDeactivation();
	}
	
	
	void LaserDeactivation ()
	{
		// Deactivate the laser GameObject.
		laser.SetActive(false);
		
		// Store the renderer component of the screen.
		Renderer screen = transform.Find("prop_switchUnit_screen_001").GetComponent<Renderer>();
		
		// Change the material of the screen to the unlocked material.
		screen.material = unlockedMat;
		
		// Play switch deactivation audio clip.
		GetComponent<AudioSource>().Play();
	}
}
