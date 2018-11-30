using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpellCircleScript : MonoBehaviour {

	public bool fade = true;
	public Vector3[] triangle;

	public Vector3[] square;

	public Vector3[] octagon;

	public Vector3[] star;

	public Vector3[] hourglass;

	public Material[] circleMaterial;

	public int segments = 2;
	public float segTime = 0.1f;

	private GameObject[] myShapes;
	private GameObject myDagger;

	private int numCirc = 0;
	private string[] circles;

	public GameObject wall;
	public GameObject particles;

	private AudioSource myAudio;

	void Start () {
		circles = new string[3];
		myAudio = GetComponent<AudioSource> ();
		//createSquare ();
		//StartCoroutine (createSquare ());
		//endGame();
	}

	public void InitiateDraw(GameObject dagger){
		myDagger = dagger;

		Vector3 above = gameObject.transform.position;
		above.y += 1.0f;
		StartCoroutine (moveDagger (above));
	}

	void StartDraw(){
		Vector3[] arr = {};

		switch (myDagger.GetComponent<Ingredient> ().value) {
		case "1":
			arr = triangle;
			break;
		case "2":
			arr = octagon;
			break;
		case "3":
			arr = square;
			break;
		case "4":
			arr = star;
			break;
		case "5":
			arr = hourglass;
			break;
		}


		StartCoroutine (createRigidShape(arr));

	}

	IEnumerator moveDagger(Vector3 pos){
		myDagger.GetComponent<Rigidbody> ().isKinematic = true;
		float t = 0.0f;
		while (t <= 2.0f) {
			
			myDagger.transform.position = Vector3.Lerp (myDagger.transform.position, pos, t/2.0f);
			myDagger.transform.rotation = Quaternion.Lerp (myDagger.transform.rotation, Quaternion.Euler (180.0f, 0.0f, 0.0f), t / 2.0f);
			t += Time.deltaTime;
			yield return null;
		}
		StartDraw ();

	}

	IEnumerator createRigidShape(Vector3[] arr){
		GameObject obj;
		LineRenderer tempLine;
		Vector3 tempVec;
		int size = (int)(arr.Length * segments) - 1;
		//Debug.Log (size);
		Vector3[] lineVects = new Vector3[size];
		obj = new GameObject ("spell");
		obj.transform.SetPositionAndRotation(gameObject.transform.position,gameObject.transform.rotation);
		obj.transform.Translate (0.0f, 0.1f * numCirc, -0.1f * numCirc);
		obj.transform.SetParent (gameObject.transform);


		tempLine = obj.AddComponent<LineRenderer> ();
		tempLine.useWorldSpace = false;
		tempLine.widthMultiplier = 0.1f;
		tempLine.material = circleMaterial[numCirc];

		float incr = 1.0f / segments;
		float lval = 0.0f;
		int place = 0;
		int vectorPos = 0;
		Vector3 lastPos = new Vector3(0.0f,0.0f,0.0f);
		bool run = true;

		while(run) {
			tempVec = Vector3.Lerp (arr [place], arr [place + 1], lval);
			tempLine.positionCount = vectorPos + 1;
			tempLine.SetPosition(vectorPos,tempVec);
			myDagger.transform.position += (Quaternion.Euler(90.0f,0.0f,0.0f) * (tempVec - lastPos));
			lastPos = tempVec;

			vectorPos++;
			lval += incr;

			if (place >= arr.Length-2 && lval >= 1.0f) {
				run = false;
			}

			if (lval >= 1.0f) {
				lval = 0;
				place++;
			}
			yield return new WaitForSeconds (segTime);

		}
		tempLine.loop = true;



		circles [numCirc] = myDagger.GetComponent<Ingredient> ().value;
		numCirc++;

		if (numCirc >= 3) {
			Invoke ("checkEnd", 0.5f);
		}


		myDagger.GetComponent<Ingredient> ().value = "0";

		/*
		int t=0;
		while (t <= 5) {

			myDagger.transform.Translate (0.0f, 0.25f, 0.0f);
			t++;
			yield return null;
		}*/

		myDagger.GetComponent<Rigidbody> ().isKinematic = false;
		myDagger.GetComponent<Rigidbody> ().useGravity = true;


		myDagger = null;
	}

	void checkEnd(){
		Debug.Log (circles[0] + circles[1] + circles[2]);
		bool a, b, c;
		a = false;
		b = false;
		c = false;

		for (int i = 0; i < 3; i++) {
			if (circles [i] == "3")
				a = true;
			if (circles [i] == "4")
				b = true;
			if (circles [i] == "5")
				c = true;
		}

		if (a && b && c) {
			particles.SetActive (true);
			StartCoroutine (raiseWall ());

		} else {
			for (int i = 0; i < 3; i++) {
				Destroy(transform.GetChild (i).gameObject);
				numCirc = 0;
				circles = new string[3];
			}

		}

	}

	IEnumerator raiseWall(){
		myAudio.Play ();
		float t = 0.0f;

		while (t <= 5.0f) {

			wall.transform.Translate (0.0f, Time.deltaTime * -1.0f, 0.0f);
			t += Time.deltaTime;
			yield return null;
		}
		yield return new WaitForSeconds (2.5f);

		if (fade)
			SteamVR_Fade.View (Color.black, 0.5f);

		Invoke ("endGame", 0.2f);
	}
	void endGame(){
		
		SceneManager.LoadScene ("ending");

	}


}
