using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ControllerScript : MonoBehaviour {

	public bool fade = false;
	public Material lineColor;

	private GameObject rayCastLine;
	private GameObject forwardTrack;
	private LineRenderer myLineRenderer;
	public GameObject player;
	public GameObject highlightSpherePrefab;

	private GameObject highlightSphere;
	//private bool lineActive = false;

	private Vector3 myVelocity;
	private Vector3 lastVelocity;
	private Vector3 lastPos;
	private Vector3 setLast;
	//private float timeOut = 0.0f;

	private GameObject highlightedObject;
	private GameObject heldObject;

	private SteamVR_TrackedController myController;
	private SteamVR_Controller.Device device;

	public Material outlineMat;

	private FixedJoint myJoint;

	public bool isLeft;

	private Vector2 padLast;

	void Start () {
		
		Vector3 temp = this.transform.position;
		temp.z -= 0.05f;
		rayCastLine = new GameObject ();
		rayCastLine.transform.SetPositionAndRotation (temp, this.transform.rotation);
		myLineRenderer = rayCastLine.AddComponent<LineRenderer> ();

		Vector3[] ls = { new Vector3 (0.0f, 0.0f, 0.0f), new Vector3 (0.0f, 0.0f, 25.0f) };
		myLineRenderer.SetPositions (ls);
		myLineRenderer.useWorldSpace = false;
		myLineRenderer.material = lineColor;
		myLineRenderer.widthMultiplier = 0.04f;

		rayCastLine.transform.SetParent (this.transform);

		myJoint = GetComponent<FixedJoint> ();

		forwardTrack = new GameObject ();
		forwardTrack.transform.SetPositionAndRotation (temp, Quaternion.identity);
		forwardTrack.transform.SetParent (this.transform);
		forwardTrack.transform.Translate (new Vector3 (0.0f, 0.0f, 1.0f));
		forwardTrack.name = "ForwardTracker";
		rayCastLine.SetActive (false);

		highlightSphere = Instantiate (highlightSpherePrefab, transform.position, Quaternion.identity);
		highlightSphere.SetActive (false);

		myController = GetComponent<SteamVR_TrackedController>();
		myController.TriggerClicked += clickTrigger;
		myController.TriggerUnclicked += unclickTrigger;
		myController.PadTouched += padTouch;
		myController.PadUntouched += padUntouch;
		myController.PadClicked += clickPad;
		myController.Gripped += gripCont;
		device = SteamVR_Controller.Input ((int)GetComponent<SteamVR_TrackedObject> ().index);
		setLast = transform.position;
	}

	void gripCont (object sender, ClickedEventArgs e)
	{
		if (isLeft) {
			player.transform.Rotate (Vector3.up, -12.0f);
		} else {
			player.transform.Rotate (Vector3.up, 12.0f);
		}
	}

	private void padTouch(object sender, ClickedEventArgs e){
		
		rayCastLine.SetActive (true);
		highlightSphere.SetActive (true);

		if (heldObject != null) {
			if (heldObject.GetComponent<interactable> ().usePad) {
				rayCastLine.SetActive (false);
				highlightSphere.SetActive (false);
			}
			heldObject.GetComponent<interactable>().OnPadMove((new Vector2(e.padX,e.padY) - padLast) / Time.deltaTime);
		}
		padLast = new Vector2 (e.padX, e.padY);
		//lineActive = true;
	}

	private void padUntouch(object sender, ClickedEventArgs e){
		rayCastLine.SetActive (false);
		highlightSphere.SetActive (false);
		padLast = new Vector3 (0.0f, 0.0f);
		//lineActive = false;

	}

	private void clickTrigger(object sender, ClickedEventArgs e){
		if (highlightedObject != null) {
			Debug.Log("Pick up " + highlightedObject.name);

			heldObject = highlightedObject;
			if (heldObject.GetComponent<interactable> ().OnGrab (this)) {
				Rigidbody r = highlightedObject.GetComponent<Rigidbody>();
				if (r == null)
					r = highlightedObject.transform.parent.gameObject.GetComponent<Rigidbody>();
				myJoint.connectedBody = r;
				//r.isKinematic = true;
			}

			//heldObject.GetComponent<Rigidbody> ().useGravity = false;

		}
	}
	private void unclickTrigger(object sender, ClickedEventArgs e){
		//bool distanceCheck = true;
		if (heldObject != null && highlightedObject == null) {

			//distanceCheck = false;
			//Debug.LogError (heldObject.name);

		}

		if (heldObject != null) {
			myJoint.connectedBody = null;


			if (heldObject.GetComponent<interactable> ().OnRelease ()) {
				Rigidbody r = highlightedObject.GetComponent<Rigidbody> ();
				if (r == null)

					r = highlightedObject.transform.parent.gameObject.GetComponent<Rigidbody> ();

				r.velocity = myVelocity;
				//r.isKinematic = true;
				r.useGravity = true;
			}
			heldObject = null;
		}
	}

	public void releaseHold(){
		if (heldObject != null) {
			myJoint.connectedBody = null;
			heldObject = null;
			highlightedObject = null;
		}

	}


	private void clickPad(object sender, ClickedEventArgs e){
		Vector3 forward = forwardTrack.transform.position - transform.position;

		//Debug.DrawRay (this.transform.position, forward, Color.black);

		//SteamVR_Fade.View (Color.black, 0);


		RaycastHit hit;
		int mask = (1 << 10);
		//Physics.Raycast (this.transform.position, forward, out hit);
		Physics.Raycast(this.transform.position,forward, out hit,200.0f,mask);

		if (hit.transform != null) {
			if (hit.transform.gameObject.tag == "floor") {

				Vector3 loc = hit.point;
				loc.y += +0.78f;
				if (fade)
					fadeIn (0.4f,0.0f);
				player.transform.position = loc;
			} else if (hit.transform.gameObject.tag == "stairs") {
				Vector3 loc;
				//Debug.Log (hit.transform.position.y + " " + player.transform.position.y);
				if (hit.transform.position.y > player.transform.position.y) {
					loc = hit.transform.gameObject.GetComponent<StairsScript> ().upStairs.transform.position;
				} else {
					loc = hit.transform.gameObject.GetComponent<StairsScript> ().downStairs.transform.position;
				}
				if (fade)
					fadeIn (0.4f,0.0f);
				player.transform.position = loc;

			}
		}


	}

	// Update is called once per frame
	void FixedUpdate () {

		lastPos = setLast;
		setLast = transform.position;
		myVelocity = (setLast - lastPos) / Time.deltaTime;


		if (myVelocity.magnitude < 0.2f) {
			myVelocity = lastVelocity;
		} else {
			lastVelocity = myVelocity;
		}



	
		int mask = 1 << 8;
		Collider[] hits = Physics.OverlapSphere (transform.position, 0.5f, mask);
		/*
		Debug.Log (hits.Length);

		}*/

		Material[] mats;
		if (hits.Length > 0) {
			

			if (highlightedObject != hits [0].transform.gameObject) {
				if (highlightedObject != null) {
					mats = highlightedObject.GetComponent<Renderer> ().materials;
					mats [1] = mats[0];
					highlightedObject.GetComponent<Renderer> ().materials = mats;
				}

				highlightedObject = hits [0].transform.gameObject;

				mats = highlightedObject.GetComponent<Renderer> ().materials;
				mats [1] = outlineMat;
				highlightedObject.GetComponent<Renderer> ().materials = mats;
			}


		} else {
			if (highlightedObject != null) {
				mats = highlightedObject.GetComponent<Renderer> ().materials;
				mats [1] = mats[0];
				highlightedObject.GetComponent<Renderer> ().materials = mats;
				highlightedObject = null;
			}
		}

		Vector3 forward = forwardTrack.transform.position - transform.position;

		//Debug.DrawRay (this.transform.position, forward, Color.black);

		//SteamVR_Fade.View (Color.black, 0);


		RaycastHit hit;
		mask = (1 << 10);
		//Physics.Raycast (this.transform.position, forward, out hit);
		Physics.Raycast(this.transform.position,forward, out hit,200.0f,mask);
		if (hit.transform != null) {
			if (hit.transform.gameObject.tag == "floor") {

				Vector3 loc = hit.point;
				loc.y += -0.0f;

				highlightSphere.transform.position = loc;
			} else if (hit.transform.gameObject.tag == "stairs") {
				Vector3 loc;
				if (hit.transform.position.y > player.transform.position.y) {
					loc = hit.transform.gameObject.GetComponent<StairsScript> ().upStairs.transform.position;
				} else {
					loc = hit.transform.gameObject.GetComponent<StairsScript> ().downStairs.transform.position;
				}
				highlightSphere.transform.position = loc;

			}
		}

	}

	public void fadeIn (float t, float d){
		SteamVR_Fade.Start (Color.black, d);
		Invoke ("fadeOut", t);

	}

	public void fadeOut(){
		SteamVR_Fade.Start (Color.clear, 0.3f);
	}
}
