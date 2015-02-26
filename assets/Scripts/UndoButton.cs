using UnityEngine;
using System.Collections;

public class UndoButton : MonoBehaviour {
	private Chess chess;
	private bool insideButton;
	private bool insideMenu;
	private float cooldown;
	
	void Start(){
		chess = GameObject.FindObjectOfType<Chess> ();
		insideMenu = false;
		insideButton = false;
	}

	void OnGUI () {
		if(chess.getStarting())
			return;

		if (chess.getEnemyPlaying() || chess.getHistory().Count == 0)
			GUI.Box (new Rect (Screen.width - 160, 32, 150, 22), "Deshacer!");
		else if (GUI.Button (new Rect (Screen.width - 160, 32, 150, 22), "Deshacer!"))
			chess.undo ();
		
		if (insideMenu || insideButton) {
			for(int i = 1; i <= chess.getHistory().Count; i++){
				if (GUI.Button (new Rect (Screen.width - 130, 32+22*i, 120, 22), (chess.getHistory().Count-i+1) + ". " + ((ChessMove)chess.getHistory()[chess.getHistory().Count-i]).ToString()))
					chess.undo (i);
			}
			for(int i = 1; i <= chess.getHistory().Count; i++){
				if(chess.isWhiteTurn() && i%2==1)
					GUI.Box (new Rect (Screen.width - 152, 32+22*i, 22, 22), chess.getBlackTex());
				else if(!chess.isWhiteTurn() && i%2==0)
					GUI.Box (new Rect (Screen.width - 152, 32+22*i, 22, 22), chess.getBlackTex());
				else
					GUI.Box (new Rect (Screen.width - 152, 32+22*i, 22, 22), chess.getWhiteTex());
			}
		}
	}
	
	void Update(){
		if (chess.getEnemyPlaying () || chess.getHistory ().Count == 0) {
			insideButton = false;
			insideMenu = false;
			return;
		}

		if (Input.mousePosition.y < (Screen.height-32) && Input.mousePosition.y > (Screen.height-54) &&
		    Input.mousePosition.x > Screen.width - 160 && Input.mousePosition.x < Screen.width - 10){
			insideButton = true;
			cooldown = Time.realtimeSinceStartup;
		}

		if (insideButton && Input.mousePosition.y < (Screen.height-54) && Input.mousePosition.y > (Screen.height-(32+22*(chess.getHistory().Count+1))) &&
		    Input.mousePosition.x > Screen.width - 152 && Input.mousePosition.x < Screen.width - 10){
			insideMenu = true;
			cooldown = Time.realtimeSinceStartup;
		}

		if (Time.realtimeSinceStartup - cooldown > 0.1f) {
			insideButton = false;
			insideMenu = false;
		}
	}
}
