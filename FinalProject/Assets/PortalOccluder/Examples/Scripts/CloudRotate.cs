using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudRotate : MonoBehaviour {

    public float rotationSpeed = 0.1f;
    
	// Update is called once per frame
	void Update () {
        transform.rotation *= Quaternion.Euler(0, 0, rotationSpeed * Time.deltaTime);
	}
}
