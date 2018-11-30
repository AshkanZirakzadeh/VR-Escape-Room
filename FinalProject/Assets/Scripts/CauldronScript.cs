using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class potionRecipe{


	public string recip;
	public GameObject result;


	public potionRecipe(string a, GameObject res){
		recip = a;
		result = res;

	}

	public bool compare(string a){
		if (a == recip) {
			return true;
		}
		return false;

	}


}

public class CauldronScript : MonoBehaviour {
	public GameObject daggerPrefab;
	public GameObject water;
	public GameObject circle;
	private string currentItem;

	public potionRecipe[] myRecipes;


	// Use this for initialization
	void Start () {
		currentItem = "";
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter(Collider other) {
		//Debug.Log ("Collide with" + other.name);
		if (other.tag == "mixable") {
			
			addIngredient (other.GetComponent<Ingredient> ());
			other.GetComponent<interactable> ().onDestroy();

		} else if (other.tag == "dagger") {
			
			addIngredient (other.GetComponent<Ingredient> ());
			other.GetComponent<interactable> ().onDestroy();

		} else {
			//other.attachedRigidbody.AddExplosionForce (2.0f, transform.position, 5.0f);
		}
	}


	void addIngredient(Ingredient ingr){
		//Debug.Log ("added " + ingr.value);
		if (currentItem.Length > 1) {
			Debug.LogError ("There is already 2 objects inside the cauldron. Should not happen");
		} else if (currentItem.Length > 0) {
			if (string.Compare (currentItem, ingr.value) < 0) {
				currentItem = currentItem + ingr.value;
			} else {
				currentItem = ingr.value + currentItem;
			}

			resolvePotion ();
			Color oldColor = water.GetComponent<MeshRenderer> ().material.GetColor ("_RefrColor");
			water.GetComponent<MeshRenderer> ().material.SetColor ("_RefrColor", Color.Lerp (oldColor, ingr.myColor, 0.5f));

		} else {
			Color oldColor = water.GetComponent<MeshRenderer> ().material.GetColor ("_RefrColor");
			water.GetComponent<MeshRenderer> ().material.SetColor ("_RefrColor", Color.Lerp (oldColor, ingr.myColor, 0.5f));
			currentItem = currentItem + ingr.value;
		}

	}

	void resolvePotion(){
		foreach (potionRecipe p in myRecipes) {
			if (p.compare(currentItem)){
				createPotion (p);
				Debug.Log ("creating " + p.result.name);
				return;
			}
		}
		if (currentItem [0] == '0') {
			Vector3 spawnLoc = transform.position;
			spawnLoc+= new Vector3(0.0f,2.0f,0.0f);
			GameObject bottle = Instantiate (daggerPrefab, spawnLoc, Quaternion.identity);
			bottle.GetComponent<Rigidbody> ().useGravity = false;
		}
		currentItem = "";
		water.GetComponent<MeshRenderer> ().material.SetColor ("_RefrColor", Color.white);
	}

	void createPotion(potionRecipe p){

		Vector3 spawnLoc = transform.position;
		spawnLoc+= new Vector3(0.0f,2.0f,0.0f);

		GameObject bottle = Instantiate (p.result, spawnLoc, Quaternion.identity);

		bottle.GetComponent<Rigidbody> ().useGravity = false;

		if (currentItem [0] == '0') {
			circle.GetComponent<SpellCircleScript> ().InitiateDraw (bottle);
			Debug.Log ("Calls InitiateDraw");
		}



		currentItem = "";
	}
}
