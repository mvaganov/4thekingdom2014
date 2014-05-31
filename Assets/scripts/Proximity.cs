using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Proximity : MonoBehaviour {

	public Agent userInProximity;
	public Agent owner;

	public List<Agent> nearbyAgents = new List<Agent>();

	public Agent GetOwner() {
		if(owner == null) {
			owner = transform.parent.GetComponent<Agent>();
		}
		return owner;
	}

	void Start() {
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		Agent a = other.GetComponent<Agent> ();
		if(a) {
			if(a.userControlled) {
				userInProximity = a;
			}
			nearbyAgents.Add(a);
			if(GetOwner().userControlled) {
				a.SetShowingNeeds(true);
			}
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		Agent a = other.GetComponent<Agent> ();
		if(a) {
			if(a.userControlled) {
				userInProximity = null;
			}
			nearbyAgents.Remove(a);
			if(GetOwner().userControlled) {
				a.SetShowingNeeds(false);
			}
		}
	}
}
