using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CirrusPlay.PortalLibrary.Examples;
using UnityEngine.SceneManagement;

public class GameStart : interactable {
	private bool fade = true;
	private Door myDoor; 

	void Start(){
		myDoor = GetComponentInParent<Door> ();
	}

	public override bool OnGrab(ControllerScript mholder){
		myDoor.grab ();
		holder = mholder;

		if (fade)
			holder.fadeIn (5.0f, 0.5f);
		Invoke ("startGame", 0.6f);

		return moveable;
	}
	void startGame(){
		SceneManager.LoadScene ("main");
	}
}