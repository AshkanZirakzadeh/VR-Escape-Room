using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialScript : interactable {


	public GameObject safe;
	public int dial;
	private SafeScript safeScript;
	// Use this for initialization
	void Start () {
		safeScript = safe.GetComponent<SafeScript> ();
	}
	
	// Update is called once per frame
	public override void OnPadMove(Vector2 vel){
		//Debug.Log("vel dot up: " + Vector3.Dot (vel, Vector3.up) +"||| vel dotdown "+ Vector3.Dot (vel, Vector3.down));
		if (Vector3.Dot (vel, Vector3.up) > Vector3.Dot (vel, Vector3.down)) {
			safeScript.AcceptInput (dial, true);
			StartCoroutine (turn (-30.0f));
		} else {
			safeScript.AcceptInput (dial, false);
			StartCoroutine (turn (30.0f));
		}
	}
	IEnumerator turn(float angle){
		float t = 0.0f;

		while (t <= 0.5f) {
			transform.parent.Rotate (Vector3.up, Time.deltaTime * angle);
			t += Time.deltaTime;
			yield return null;
		}


	}
}
