using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Proximity : MonoBehaviour {

	public Agent userInProximity;
	Agent owner;

	public List<Agent> nearbyAgents = new List<Agent>();

	GameObject test;

	public Agent GetOwner() {
		if(owner == null) {
			owner = transform.parent.GetComponent<Agent>();
		}
		return owner;
	}

	void Start() {
	}

	// TODO move this to the Agent some time
	public GameWorld.Needs IsAgentSharingNeeds(Agent a, Agent b) {
		for(int i = 0; i < a.myNeed.Length; ++i) {
			for(int j = 0; j < b.myNeed.Length; ++j) {
				if(a.myNeed[i] == b.myNeed[j]) {
					return a.myNeed[i];
				}
			}
		}
		return GameWorld.Needs.NONE;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		Agent a = other.GetComponent<Agent> ();
		if(a) {
			Lines.Make(ref test, Color.magenta, transform.position, a.transform.position, 0.1f, 0.1f);
			if(a.userControlled) {
				userInProximity = a;
				GetOwner().SetShowingNeeds(true);
			} else {

			}
			nearbyAgents.Add(a);
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		Agent a = other.GetComponent<Agent> ();
		if(a) {
			if(a.userControlled) {
				userInProximity = null;
				GetOwner().SetShowingNeeds(false);
			}
			nearbyAgents.Remove(a);
		}
	}
}
