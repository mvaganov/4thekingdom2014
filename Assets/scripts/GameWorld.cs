using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameWorld : MonoBehaviour {

	static private GameWorld gw;

	public static GameWorld GetGlobal() {
		if(gw == null) {
			Object[] objects = Resources.FindObjectsOfTypeAll(typeof(GameObject));
			for (int i = 0; gw == null && i < objects.Length; ++i) {
				if (objects[i] is GameObject) {
					GameObject go = (GameObject)objects[i];
					gw = go.GetComponent<GameWorld>();
				}
			}
			if (gw == null) {
				GameObject gameWorld = new GameObject("gameworld (auto generated)");
				gw = gameWorld.AddComponent<GameWorld>();
			}
		}
		return gw;
	}

	public GameObject prefab_textBubble;

	public GameObject prefab_agent;
	public enum Needs { NONE, heart, money, sun, book};//friendship, food, healthcare, education, job, money, purpose, brokenheart };
	public GameObject[] prefab_needs = new GameObject[0];

	private static int NEED_TYPES_COUNT = 0;
	public Needs GenerateRandomNeed() {
		if(NEED_TYPES_COUNT == 0) {
			NEED_TYPES_COUNT = System.Enum.GetNames (typeof(Needs)).Length;
		}
		return (Needs)Random.Range (1, NEED_TYPES_COUNT);
	}

	public int agentGenerationCount = 10;

	Vector2 min, max;

	public List<Agent> agents = new List<Agent>();

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
//			a.transform.parent = this.gameObject.transform;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
