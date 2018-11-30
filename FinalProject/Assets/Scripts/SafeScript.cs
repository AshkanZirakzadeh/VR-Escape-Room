using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SafeScript : MonoBehaviour {
	
	public Text num1;
	public Text num2;
	public Text num3;



	public string password = "156";

	public GameObject door;
	public GameObject pivot;

	private bool inputLock = false;
	private bool open=false;

	void Start () {
		num1.text = "1";
		num2.text = "1";
		num3.text = "1";
	}

	void resetLock(){
		inputLock = false;
	}

	void Update(){
		/*
		if (!inputLock) {
			if (Input.GetKey ("q") || Input.GetKey ("a") || Input.GetKey ("w") || Input.GetKey ("s") || Input.GetKey ("e") || Input.GetKey ("d")) {
				inputLock = true;
				Invoke ("resetLock", 0.1f);
			}

			if (Input.GetKey ("q"))
				buttonU1 ();
			if (Input.GetKey ("a"))
				buttonD1 ();
			if (Input.GetKey ("w"))
				buttonU2 ();
			if (Input.GetKey ("s"))
				buttonD2 ();
			if (Input.GetKey ("e"))
				buttonU3 ();
			if (Input.GetKey ("d"))
				buttonD3 ();
		}*/
	}

	public void AcceptInput(int dial, bool up){
		if (dial == 1)
			button1 (up);
		else if (dial == 2)
			button2 (up);
		else if (dial == 3)
			button3 (up);
		else
			Debug.LogError (dial + " should not exist");

	}
	

	void button1(bool up){
		int temp = int.Parse (num1.text);
		if (up) {

			temp = (temp + 1) % 10;
		} else {
			
			temp = (temp - 1);
			if (temp < 0) {
				temp = 9;
			}
		}
		num1.text = temp.ToString ();
		checkPass ();
	}

	//#######################
	void button2(bool up){
		int temp = int.Parse (num2.text);
		if (up) {

			temp = (temp + 1) % 10;

		} else {

			temp = (temp - 1);
			if (temp < 0) {
				temp = 9;
			}
		}
		num2.text = temp.ToString ();
		checkPass ();
	}

	//#######################
	void button3(bool up){
		int temp = int.Parse (num3.text);
		if (up) {

			temp = (temp + 1) % 10;

		} else {

			temp = (temp - 1);
			if (temp < 0) {
				temp = 9;
			}
		}
		num3.text = temp.ToString ();
		checkPass ();
	}

	//#######################
	void checkPass(){
		if (num1.text + num2.text + num3.text == password) {
			//Debug.Log ("You got the password");
			if (!open) {
				open = true;
				StartCoroutine (opendoor ());
			}
		}

	}

	IEnumerator opendoor(){

		float t = 0.0f;

		while (t <= 2.0f) {
			door.transform.RotateAround (pivot.transform.position, Vector3.up, 45 * Time.deltaTime);
			t+=Time.deltaTime;
			yield return null;
		}

	}
}
