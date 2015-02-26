using UnityEngine;
using System.Collections;

public class ResetButton : MonoBehaviour {
	private float heightButtons = 20f;
	private float widthButtons = 200f;

	private float actualWidthButton;
	private float cooldown;
	private float resetTime = 0.1f;

	private Chess chess;

	void Start () {
		actualWidthButton = 0f;
		chess = GameObject.FindObjectOfType<Chess> ();
	}

	void OnGUI () {
		if (GUI.Button (new Rect (0, Screen.height - heightButtons, widthButtons, heightButtons), new GUIContent("Nueva Partida", "Boton")) &&
		    actualWidthButton >= widthButtons){
			chess.Reset();
		}
		
		if(GUI.tooltip == "Boton"){
			actualWidthButton += 5f;
			if(actualWidthButton > widthButtons){
				actualWidthButton = widthButtons;
			}
			GUI.Button (new Rect (0, Screen.height - heightButtons, actualWidthButton, heightButtons), "");
			cooldown = Time.realtimeSinceStartup;
		}
	}

	void Update () {
		if (Time.realtimeSinceStartup - cooldown > resetTime)
			actualWidthButton = 0;
	}
}
