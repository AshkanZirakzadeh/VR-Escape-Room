using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class interactable : MonoBehaviour {
	public bool moveable = true;
	public bool destroyParent = true;
	public bool usePad = false;
	protected ControllerScript holder = null;



	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	public virtual bool OnGrab(ControllerScript mholder){
		holder = mholder;
		return moveable;
	}
	public virtual bool OnRelease(){
		return moveable;
	}
	public virtual void OnMove(Vector3 vel){

	}
	public virtual void OnPadMove(Vector2 vel){

	}

	public void onDestroy(){
		if (holder != null) {
			holder.releaseHold ();
		}
		if (destroyParent) {
			Destroy (gameObject.transform.parent.gameObject);
		} else {
			Destroy (gameObject);
		}
	}
}
