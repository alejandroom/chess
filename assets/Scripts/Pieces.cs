using UnityEngine;
using System.Collections;

public class Pieces : MonoBehaviour {

	public void hide(){
		gameObject.renderer.material.color = new Color(1,1,1,0);
		
		foreach(Renderer r in gameObject.GetComponentsInChildren<Renderer> ())
			r.material.color = new Color(1,1,1,0);
	}
	
	public void show(){
		gameObject.renderer.material.color = new Color(1,1,1,1);
		
		foreach(Renderer r in gameObject.GetComponentsInChildren<Renderer> ())
			r.material.color = new Color(1,1,1,1);
	}
	
	public void translucent(){
		gameObject.renderer.material.color = new Color(1,1,1,0.5f);
		
		foreach(Renderer r in gameObject.GetComponentsInChildren<Renderer> ())
			r.material.color = new Color(1,1,1,0.5f);
	}
	
	public void shine(){
		gameObject.renderer.material.color = new Color(2,2,2,2f);
		
		foreach(Renderer r in gameObject.GetComponentsInChildren<Renderer> ())
			r.material.color = new Color(2,2,2,2f);
	}
}
