using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameWorld : MonoBehaviour {

	static private GameWorld gw;

	static GameWorld GetGlobal() {
		return gw;
	}

	public GameObject prefab_agent;
	public int agentGenerationCount = 10;

	Vector2 min, max;

	public List<Agent> agents;

	public Vector2 randomPosition() {
		return new Vector2 (Random.Range (min.x, max.x), Random.Range (min.y, max.y));
	}

	public Agent CreateAgent() {
		Vector2 randPos = randomPosition ();
		GameObject go = (GameObject)Instantiate (prefab_agent, randPos, Quaternion.identity);
		Agent a = go.GetComponent<Agent> ();
		if(a == null) {
			throw new System.Exception("agent prefab must have an agent component");
		}
		return a;
	}

	// Use this for initialization
	void Start () {
		gw = this;
		BoxCollider2D box = GetComponent<BoxCollider2D> ();
		min = box.center - box.size / 2;
		max = box.center + box.size / 2;
		for(int i = 0; i < agentGenerationCount; ++i) {
			Agent a = CreateAgent();
			agents.Add(a);
			a.transform.parent = this.gameObject.transform;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
