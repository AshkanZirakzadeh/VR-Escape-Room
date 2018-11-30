using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoofRain : MonoBehaviour {

    public Transform target = null;
	
	// Update is called once per frame
	void Update () {
		if (target != null)
        {
            transform.position = new Vector3(
                target.transform.position.x, 
                this.transform.position.y, 
                target.transform.position.z
                );
        }
	}
}
