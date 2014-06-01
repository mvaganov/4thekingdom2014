using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Agent : MonoBehaviour {

	public bool userControlled = false;

	public Proximity prox;

	public float maxSteering = 1;
	public float maxVelocity = 1;

	public Vector2 steering; // acceleration force

	public GameObject needsDisplay;
	bool mustShowNeeds = false;

	public List<Agent> flock = new List<Agent>(); // TODO when agents meet, they add each other to the flock, and begin flocking behavior

	public Vector2 userTarget;

	private GameObject velocityLine, steeringLine;

	public class Attention {
		public Agent focus;
		public float durationOfAttention;
		public float maxDurationOfAttention = 5;
		public float GetAttentionLeftAsPercent() {
			if(durationOfAttention > maxDurationOfAttention) {
				return 0;
			}
			return 1 - (durationOfAttention / maxDurationOfAttention);
		}
		public GameObject attentionLine;
		public void Update(GameObject me) {
			durationOfAttention += Time.deltaTime;
			Vector3 start = me.transform.position, end = focus.transform.position;
			start.z = -2;
			end.z = -2;
			Lines.Make (ref attentionLine, Color.white, start, end, 0.1f, 0.1f);
		}
	}

	public Attention FindAttention(Agent a) {
		for(int i = 0; i < attention.Count; ++i) {
			if(attention[i].focus == a) {
				return attention[i];
			}
		}
		Attention att = new Attention ();
		att.focus = a;
		attention.Add (att);
		return att;
	}
	public List<Attention> attention = new List<Attention>();

	public bool IsShowingNeeds() { return needsDisplay.activeInHierarchy; }
	public void SetShowingNeeds(bool showNeeds) {
		mustShowNeeds = showNeeds;
		if(needsDisplay) {
			needsDisplay.SetActive (showNeeds);
		}
	}

	public GameWorld.Needs[] myNeed;

	public void GenerateNeeds() {
		GameWorld gw = GameWorld.GetGlobal ();
		needsDisplay = (GameObject)Instantiate (gw.prefab_textBubble);
		needsDisplay.transform.position = transform.position;
		myNeed = new GameWorld.Needs[2];
		if(userControlled) {
			myNeed[0] = GameWorld.Needs.NONE;
			myNeed[1] = GameWorld.Needs.NONE;
		} else {
			myNeed[0] = gw.GenerateRandomNeed();
			myNeed[1] = gw.GenerateRandomNeed();
		}
		GameObject n0 = (GameObject)Instantiate (gw.prefab_needs[(int)myNeed[0]]);
		GameObject n1 = (GameObject)Instantiate (gw.prefab_needs[(int)myNeed[1]]);
		n0.transform.parent = needsDisplay.transform;
		n0.transform.position = needsDisplay.transform.position + new Vector3 (-.2f, 0, -.1f);
		n1.transform.parent = needsDisplay.transform;
		n1.transform.position = needsDisplay.transform.position + new Vector3 (.2f, 0, -.2f);
		needsDisplay.transform.position = needsDisplay.transform.position + new Vector3 (.5f, 1f, -2);
		needsDisplay.transform.parent = transform;
		needsDisplay.SetActive (mustShowNeeds);
	}

	// Use this for initialization
	void Start () {
		GenerateNeeds ();
	}

	public static Vector2 RandomUnitVector() {
		Vector2 randomUnitVector;
		do {
			randomUnitVector = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
		} while(randomUnitVector == Vector2.zero);
		return randomUnitVector.normalized;
	}

	float wanderCooldown;
	public float wanderDelay = 5;	
	public void WanderBehavior() {
		wanderCooldown -= Time.deltaTime;
		if(wanderCooldown <= 0) {
			steering = RandomUnitVector();
			steering *= maxSteering;
			wanderCooldown = wanderDelay;
		}
	}
	GameObject flockArrow;
	// Update is called once per frame
	void Update () {
		if(flock.Count > 0) {
//			Debug.Log ("FLOCKING!");
			// insert flock behavior here
			int numPeers = 1;
			int numCrowding = 0;
			Vector3 groupCenter = transform.position;//Vector3.zero;
			for (int i = 0; i < flock.Count; i++) {
				Agent a = flock[i];
				if (a != this) {
					groupCenter += a.transform.position;
					numPeers++;
				}
			}
			//Vector2 targetLocation = this.transform.position;
			if (numPeers > 0) {
				groupCenter = groupCenter / numPeers;
				SteerAt (groupCenter, 0.5f); // sets steering
			}
			Vector3 delta = groupCenter - transform.position;
//			Debug.Log("-------------"+delta);
			Lines.Make(ref flockArrow, Color.black, transform.position, groupCenter, .1f, 0);
		} else if(!userControlled) {
			WanderBehavior();
		} else {
			if(Input.GetMouseButtonDown(0)) {
				UserClick();
			}
			Vector2 targetVelocity = userTarget - (Vector2)transform.position;
			if(targetVelocity != Vector2.zero) {
				targetVelocity.Normalize();
				targetVelocity *= maxSteering;
				Vector2 requiredAcceleration = targetVelocity - rigidbody2D.velocity;
				requiredAcceleration.Normalize();
				steering = requiredAcceleration * maxSteering;
			} else {
				steering = Vector2.zero;
			}
		}
		// general steering behavior code
		rigidbody2D.velocity = rigidbody2D.velocity + (steering * Time.deltaTime); // += won't work for properties
		if(rigidbody2D.velocity != Vector2.zero) {
			float currentSpeed = rigidbody2D.velocity.magnitude;
			if(currentSpeed > maxVelocity) {
				rigidbody2D.velocity = rigidbody2D.velocity.normalized * maxVelocity;
			}
		}
		Vector3 p = transform.position;
		Vector3 v = p + (Vector3)rigidbody2D.velocity;
		Vector3 s = v + (Vector3)steering;
		p.z = -3;
		v.z = -3;
		s.z = -3;
		Color moveColor = (flock.Count == 0) ? Color.green : Color.blue;
		Lines.Make (ref velocityLine, moveColor, p, v, .1f, .1f);
		Lines.Make (ref steeringLine, Color.red, v, s, .05f, .05f);

		for(int i = 0; i < attention.Count; ++i) {
			float attentionLeft = attention[i].GetAttentionLeftAsPercent();
			if(attentionLeft > 0) {
				attention[i].Update(gameObject);
				attention[i].focus.SteerAt (transform.position, attentionLeft);
			} else {
				attention[i].focus = this;
				attention[i].Update(gameObject);
				attention.RemoveAt(i);
				i--;
			}
		}
	}

	public void SteerAt(Vector3 position, float strength) {
		Vector3 delta = position - transform.position;
		delta.Normalize ();
		delta *= maxSteering;
		delta = Vector3.Lerp (steering, delta, strength);
		steering = delta;
	}

	public void UserClick() {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		userTarget.x = ray.origin.x; // if the camera angle ever changes, this code will break!
		userTarget.y = ray.origin.y;
		Collider2D[] hits = Physics2D.OverlapPointAll(userTarget);
		if(hits.Length > 0) {
			for(int i = 0; i < hits.Length; ++i) {
				Agent a = hits[i].GetComponent<Agent>();
				if(a != null){
					FindAttention(a);
				}
			}
		}
	}

	void OnCollisionEnter2D(Collision2D other) {
		if(other.gameObject.GetComponent<BoxCollider2D>()) {
			steering *= -1;
			rigidbody2D.velocity *= -1;
		}
	}

}
