using UnityEngine;
using System.Collections;

public class Square : MonoBehaviour {
	public Color baseColor;
	public Color rangeColor;
	public Color attackColor;

	private bool activo;
	private Chess chess;	

	void Start(){
		chess = GameObject.FindObjectOfType<Chess> ();

		activo = false;
	}

	public void resetColor(){
		gameObject.GetComponent<Renderer>().material.color = baseColor;
		activo = false;
	}
	
	public void attack(){
		gameObject.GetComponent<Renderer>().material.color = attackColor;
		activo = true;
	}
	
	public void range(){
		gameObject.GetComponent<Renderer>().material.color = rangeColor;
		activo = true;
	}
	
	void OnMouseEnter(){
		if (activo) {
			chess.lightAttackers(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z), true);
		}
	}
	
	void OnMouseExit(){
		if (activo) {
			chess.lightAttackers(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z), false);
		}
	}
	
	void OnMouseDown(){
		if (activo) {
			chess.lightAttackers(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z), false);
			chess.moveUser(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
		}else{
			chess.selectUser(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
		}
	}
}
