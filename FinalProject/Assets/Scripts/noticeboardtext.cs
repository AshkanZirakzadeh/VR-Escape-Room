using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class noticeboardtext : MonoBehaviour {

	public Texture imageToDisplay;

	private Renderer myRenderer;
	private MaterialPropertyBlock myBlock;

	// Use this for initialization
	void Start () {
		myRenderer = GetComponent<Renderer> ();
		myBlock = new MaterialPropertyBlock ();
	}
	
	// Update is called once per frame
	void Update () {
		myRenderer.GetPropertyBlock (myBlock);

		myBlock.SetTexture ("_MainTex", imageToDisplay);

		myRenderer.SetPropertyBlock (myBlock);
		
	}
}
