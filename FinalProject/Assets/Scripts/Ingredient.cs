using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Ingredient : MonoBehaviour {
	public string value;

	[HideInInspector]
	public Color myColor;

	void Start(){

		try{

		myColor = gameObject.transform.parent.gameObject.GetComponent<BottleSmash> ().color;
		} catch(NullReferenceException e) {
			myColor = Color.black;
		}
	}
}
