using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpookySound : MonoBehaviour {

	private AudioSource myAudio;
	private bool played = false;
	// Use this for initialization
	void Start () {
		myAudio = GetComponent<AudioSource> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter(Collider other){
		if (!played) {
			//Debug.Log (other.name + "enter");
			if (other.tag == "Player") {
				myAudio.Play ();
				played = true;
			}
		}

	}
}
