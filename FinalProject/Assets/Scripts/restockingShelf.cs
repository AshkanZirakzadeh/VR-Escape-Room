using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class restockingShelf : MonoBehaviour {


	public GameObject[] myPotions;
	public GameObject[] myPrefabs;

	private Vector3[] myPos;


	void Start () {
		myPos = new Vector3[6];
		for (int i = 0; i < myPotions.Length; i++) {
			myPos [i] = myPotions [i].transform.position;
		}
	}
	

	void Update () {
		
	}

	void OnTriggerExit(Collider other){
		Debug.Log (other.name);
		if (other.tag == "potion") {
			for (int i = 0; i < myPotions.Length; i++){
				if (myPotions[i] == other.gameObject){
					myPotions[i] = null;
					Invoke ("respawnPotions", 1.5f);
				}
			}

		}

	}

	void respawnPotions(){
		for (int i = 0; i < myPotions.Length; i++){
			if (myPotions[i] == null){
				myPotions [i] = Instantiate (myPrefabs [i], myPos [i], Quaternion.identity, transform);
				Vector3 t = myPotions [i].transform.lossyScale;
				myPotions [i].transform.localScale = new Vector3 (1.5f/t.x, 1.5f/t.y, 1.5f/t.z);
			}
		}

	}


}
