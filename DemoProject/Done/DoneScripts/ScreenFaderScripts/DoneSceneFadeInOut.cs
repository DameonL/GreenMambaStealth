using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DoneSceneFadeInOut : MonoBehaviour
{
	public float fadeSpeed = 1.5f;			// Speed that the screen fades to and from black.
	
	
	private bool sceneStarting = true;		// Whether or not the scene is still fading in.
	
	
	
	void Update ()
	{
		// If the scene is starting...
		if(sceneStarting)
			// ... call the StartScene function.
			StartScene();
	}
	
	
	void FadeToClear ()
	{
		// Lerp the colour of the texture between itself and transparent.
		GetComponent<RawImage>().color = Color.Lerp(GetComponent<RawImage>().color, Color.clear, fadeSpeed * Time.deltaTime);
	}
	
	
	void FadeToBlack ()
	{
		// Lerp the colour of the texture between itself and black.
		GetComponent<RawImage>().color = Color.Lerp(GetComponent<RawImage>().color, Color.black, fadeSpeed * Time.deltaTime);
	}
	
	
	void StartScene ()
	{
		// Fade the texture to clear.
		FadeToClear();
		
		// If the texture is almost clear...
		if(GetComponent<RawImage>().color.a <= 0.05f)
		{
			// ... set the colour to clear and disable the GUITexture.
			GetComponent<RawImage>().color = Color.clear;
			GetComponent<RawImage>().enabled = false;
			
			// The scene is no longer starting.
			sceneStarting = false;
		}
	}
	
	
	public void EndScene ()
	{
		// Make sure the texture is enabled.
		GetComponent<RawImage>().enabled = true;
		
		// Start fading towards black.
		FadeToBlack();

		// If the screen is almost black...
		if (GetComponent<RawImage>().color.a >= 0.95f)
			// ... reload the level.
			SceneManager.LoadScene(0);
	}
}
