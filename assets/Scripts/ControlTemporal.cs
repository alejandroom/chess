using UnityEngine;

public class ControlTemporal : MonoBehaviour {	
	private Texture2D playTex;
	private Texture2D pauseTex;
	private Texture2D halfTex;
	private Texture2D doubleTex;
	
	private Texture2D playOnTex;
	private Texture2D pauseOnTex;
	private Texture2D halfOnTex;
	private Texture2D doubleOnTex;
	
	private Texture2D playOffTex;
	private Texture2D pauseOffTex;
	private Texture2D halfOffTex;
	private Texture2D doubleOffTex;

	public bool activo;
	
	void Start(){
		playOnTex = (Texture2D)Resources.Load("TimeControls/PlayOn", typeof(Texture2D));
		pauseOnTex = (Texture2D)Resources.Load("TimeControls/PauseOn", typeof(Texture2D));
		halfOnTex = (Texture2D)Resources.Load("TimeControls/HalfOn", typeof(Texture2D));
		doubleOnTex = (Texture2D)Resources.Load("TimeControls/DoubleOn", typeof(Texture2D));
		playOffTex = (Texture2D)Resources.Load("TimeControls/PlayOff", typeof(Texture2D));
		pauseOffTex = (Texture2D)Resources.Load("TimeControls/PauseOff", typeof(Texture2D));
		halfOffTex = (Texture2D)Resources.Load("TimeControls/HalfOff", typeof(Texture2D));
		doubleOffTex = (Texture2D)Resources.Load("TimeControls/DoubleOff", typeof(Texture2D));
		Play ();

		activo = false;
	}

	void OnGUI()
	{
		if(activo){
			GUI.BeginGroup (new Rect (Screen.width-150, 60, 150, 25));
			if (GUI.Button (new Rect (0, 0, 25, 25), halfTex, new GUIStyle ()))
				Half ();
			if (GUI.Button (new Rect (35, 0, 25, 25), pauseTex, new GUIStyle ()))
				Pause ();
			if (GUI.Button (new Rect (70, 0, 25, 25), playTex, new GUIStyle ()))
				Play ();
			if (GUI.Button (new Rect (105, 0, 25, 25), doubleTex, new GUIStyle ()))
				Double ();
			GUI.EndGroup ();
		}
	}

	void Play(){
		playTex = playOnTex;
		pauseTex = pauseOffTex;
		doubleTex = doubleOffTex;
		halfTex = halfOffTex;
		Time.timeScale = 1f;
	}

	void Pause(){
		playTex = playOffTex;
		pauseTex = pauseOnTex;
		doubleTex = doubleOffTex;
		halfTex = halfOffTex;
		Time.timeScale = 0f;
	}

	void Half(){
		playTex = playOffTex;
		pauseTex = pauseOffTex;
		doubleTex = doubleOffTex;
		halfTex = halfOnTex;
		Time.timeScale = 0.5f;
	}

	void Double(){
		playTex = playOffTex;
		pauseTex = pauseOffTex;
		doubleTex = doubleOnTex;
		halfTex = halfOffTex;
		Time.timeScale = 2f;
	}
}
