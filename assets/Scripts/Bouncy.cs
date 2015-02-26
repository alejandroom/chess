using UnityEngine;
using System.Collections;

public class Bouncy : MonoBehaviour {
	private bool bouncing;
	private float cooldown;

	void Start () {
		bouncing = false;
	}
	
	public void bounce(){
		if(!bouncing){
			bouncing = true;
			cooldown = Time.realtimeSinceStartup;
		}
	}
	
	public void stopBounce(){
		bouncing = false;
	}

	void Update () {
		if (!bouncing)
			gameObject.transform.position = new Vector3(gameObject.transform.position.x, 0.1f, gameObject.transform.position.z);
		else{
			if((Time.realtimeSinceStartup - cooldown) % 1f < 0.5f){
				gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y+(0.2f*Time.deltaTime), gameObject.transform.position.z);
			}else{
				gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y-(0.2f*Time.deltaTime), gameObject.transform.position.z);
			}
		}
	}
}
