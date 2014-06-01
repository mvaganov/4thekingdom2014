using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Proximity : MonoBehaviour {

	public Agent userInProximity;
	Agent owner;

	public List<Agent> nearbyAgents = new List<Agent>();

//	GameObject test;

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
	// TODO move this to the Agent some time
	public Agent.Attention FindAttention(Agent self, Agent a) {
		for(int i = 0; i < self.attention.Count; ++i) {
			if(self.attention[i].focus == a) {
				return self.attention[i];
			}
		}
		return null;
	}

	void IsBeingIntroducedBy(Agent introducer, Agent self, Agent other) {
		// TODO move this into the Agent at some point
		if(introducer && IsAgentSharingNeeds(self, other) != GameWorld.Needs.NONE) {
			Agent.Attention atMe = FindAttention(introducer, self);
			Agent.Attention atHim = FindAttention(introducer, other);
			if(atMe != null && atHim != null) {
//				Debug.Log("adding to flocks");
				if(!self.flock.Contains(other))
					self.flock.Add(other);
				if(!other.flock.Contains(self))
					other.flock.Add (self);
				other.flock = self.flock;
				introducer.score++;
				// doing this would be bad for memory...
				// and we'll addeach other's friends too!
//				if(other.flock.Count > 0)
//					self.flock.Add (other.flock);
//				if(self.flock.Count > 0)
//					other.flock.Add (self.flock);

//				other.renderer.material.color = Color.red;
//				self.renderer.material.color = Color.red;
			}
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		Agent a = other.GetComponent<Agent> ();
		if(a) {
			Agent self = GetOwner();
//			Lines.Make(ref test, Color.magenta, transform.position, a.transform.position, 0.1f, 0.1f);
			if(a.userControlled) {
				userInProximity = a;
				self.SetShowingNeeds(true);
//				if(self.renderer.material.color != Color.red)
//					self.renderer.material.color = Color.cyan;
			}
			nearbyAgents.Add(a);
			CheckIntroduction(self);
		}
	}

	public void CheckIntroduction() {
		CheckIntroduction (GetOwner ());
	}
	void CheckIntroduction(Agent self) {
		if(!self.userControlled && userInProximity) {
			for(int i = 0; i < nearbyAgents.Count; ++i) {
				IsBeingIntroducedBy(userInProximity, self, nearbyAgents[i]);
			}
		}
	}
	
	void OnTriggerExit2D(Collider2D other)
	{
		Agent a = other.GetComponent<Agent> ();
		if(a) {
			Agent self = GetOwner();
			if(a.userControlled) {
				userInProximity = null;
				self.SetShowingNeeds(false);
//				if(self.renderer.material.color != Color.red)
//					self.renderer.material.color = Color.white;
			}
			CheckIntroduction(self);
			nearbyAgents.Remove(a);
		}
	}
}
