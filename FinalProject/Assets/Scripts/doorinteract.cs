using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CirrusPlay.PortalLibrary.Examples;

public class doorinteract : interactable {

	private Door myDoor; 

	void Start(){
		myDoor = GetComponentInParent<Door> ();
	}

	public override bool OnGrab(ControllerScript mholder){
		myDoor.grab ();
		holder = mholder;
		return moveable;
	}
}
