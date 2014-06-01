using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameWorld : MonoBehaviour {

	static private GameWorld gw;

	public AudioClip connection, points;

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
	public enum Needs { NONE, heart, money, sun, book, exercise, dogcat};//friendship, food, healthcare, education, job, money, purpose, brokenheart };
	public GameObject[] prefab_needs = new GameObject[0];

	public SpriteData[] playerSprites;

	public GameObject moveToken;
	public ParticleSystem scoreToken;

	private static int NEED_TYPES_COUNT = 0;
	public Needs GenerateRandomNeed() {
		if(NEED_TYPES_COUNT == 0) {
			NEED_TYPES_COUNT = System.Enum.GetNames (typeof(Needs)).Length;
		}
		return (Needs)Random.Range (1, NEED_TYPES_COUNT);
	}

	public int agentGenerationCount = 10;

	public GameObject youWinMessage;

	Vector2 min, max;

	public List<Agent> agents = new List<Agent>();

	bool haveWon = false;
	float explosionTimer = 0;

	public int PeopleWithoutCommunity() {
		int count = 0;
		for(int i = 0; i < agents.Count; ++i) {
			if(agents[i].flock.Count == 0)
				count++;
		}
		return count;
	}

	public void WinCheck() {
		if(PeopleWithoutCommunity() == 0) {
			youWinMessage.SetActive(true);
			haveWon = true;
		}
	}
	
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
		a.spriteData = playerSprites [Random.Range (1, playerSprites.Length)];
		return a;
	}

	// Use this for initialization
	void Start () {
		youWinMessage.SetActive(false);
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
		if(Input.GetKeyDown(KeyCode.Escape)) {
			Application.Quit();
		}
		if(haveWon) {
			explosionTimer -= Time.deltaTime;
			if(explosionTimer <= 0) {
				int index = Random.Range(0, agents.Count);
				Vector3 loc = agents[index].transform.position;
				loc.z = -5;
				scoreToken.transform.position = loc;
				scoreToken.Emit (10);
				explosionTimer += Random.Range(.1f, .2f);
			}
		}
	}
}
