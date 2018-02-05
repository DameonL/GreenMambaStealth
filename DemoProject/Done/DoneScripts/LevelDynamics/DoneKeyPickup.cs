using UnityEngine;
using System.Collections;

public class DoneKeyPickup : MonoBehaviour
{
	public AudioClip keyGrab;							// Audioclip to play when the key is picked up.
	
	
	private GameObject player;							// Reference to the player.
	private DonePlayerInventory playerInventory;		// Reference to the player's inventory.
	
	
	void Awake ()
	{
		// Setting up the references.
		player = GameObject.FindGameObjectWithTag(DoneTags.player);
		playerInventory = player.GetComponent<DonePlayerInventory>();
	}
	
	
    void OnTriggerEnter (Collider other)
    {
		// If the colliding gameobject is the player...
		if(other.gameObject == player)
		{
			// ... play the clip at the position of the key...
			AudioSource.PlayClipAtPoint(keyGrab, transform.position);
			
			// ... the player has a key ...
			playerInventory.hasKey = true;
			
			// ... and destroy this gameobject.
        	Destroy(gameObject);
		}
    }
}
