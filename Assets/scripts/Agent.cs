﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Agent : MonoBehaviour {
	public static bool showSteeringLines = true;

	public bool userControlled = false;

	public Proximity prox;
	public float score = 0;

	public float maxSteering = 1;
	public float maxVelocity = 1;

	/// <summary>acceleration force</summary>
	public Vector3 steering;

	public GameObject needsDisplay;
	bool mustShowNeeds = false;

	public List<Agent> flock = new List<Agent>(); // TODO when agents meet, they add each other to the flock, and begin flocking behavior
	public float flockRepelRadius = 2;
	public Vector3 targetLocation;

	private GameObject velocityLine, steeringLine, flockCircle, boundaryCircle, targetline;

	public SpriteRenderer spriteRenderer;
	public SpriteData spriteData;
	SpriteData.Instance sprite;

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
		sprite = spriteData.CreateInstance (spriteRenderer);
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

		if(userControlled && Input.GetMouseButtonDown(0)) {
			UserClick();
		}
		if(flock.Count > 0) {
//			Debug.Log ("FLOCKING!");
			// insert flock behavior here
			int numPeers = 1;
			int numCrowding = 1;
			Vector3 groupCenter = transform.position;
			Vector3 crowdingCenter = transform.position;
			Vector3 averageSteering = steering;
			for (int i = 0; i < flock.Count; i++) {
				Agent a = flock[i];
				if (a != this) {
					groupCenter += a.transform.position;
					averageSteering += a.steering;
					numPeers++;
					if (Vector3.Distance (transform.position, a.transform.position) < flockRepelRadius) {
						crowdingCenter += a.transform.position;
						numCrowding++;
					}
				}
			}

			if (numPeers > 1) {
				groupCenter = groupCenter / numPeers;
				averageSteering = averageSteering / numPeers;
				SteerAt (groupCenter, 0.5f); // sets steering
				SteerAt (transform.position + (Vector3)averageSteering, 0.3f);
				if (numCrowding > 1) {
					crowdingCenter = crowdingCenter / numCrowding;
					Vector3 toCrowd = crowdingCenter - transform.position;
					Vector3 fleeTarget = transform.position - (2 * toCrowd);
					fleeTarget += 0.1f * (Vector3)RandomUnitVector();
					SteerAt(fleeTarget, 0.7f);
				}
			}
//			Vector3 delta = groupCenter - transform.position;
//			Debug.Log("-------------"+delta);
			Lines.Make(ref flockArrow, Color.black, transform.position, groupCenter, .1f, 0);
		} else if(!userControlled) {
			WanderBehavior();
		} else {
			steering = Steering.Arrive(transform.position, rigidbody2D.velocity, maxVelocity, maxSteering, targetLocation);
			if(steering == Vector3.zero) {
				if(rigidbody2D.velocity.sqrMagnitude < 0.5f) {
					rigidbody2D.velocity = Vector3.zero;
				}
			} else {
				steering = steering.normalized * maxSteering;
			}
		}
		// general steering behavior code
		rigidbody2D.velocity = rigidbody2D.velocity + (Vector2)(steering * Time.deltaTime); // += won't work for properties
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
		if(showSteeringLines) {
			Lines.MakeArc(ref boundaryCircle, Color.blue, transform.position, Vector3.forward, Vector3.right * GetComponent<CircleCollider2D>().radius, 360, 24, .05f, .05f);
			Lines.Make(ref targetline, Color.blue, transform.position, targetLocation, .01f, .01f);
			Color moveColor = (flock.Count == 0) ? Color.green : Color.blue;
			Lines.Make (ref velocityLine, moveColor, p, v, .1f, .1f);
			Lines.Make (ref steeringLine, Color.red, v, s, .05f, .05f);
			if(flock.Count > 0) {
				Lines.MakeArc(ref flockCircle, Color.blue, transform.position, Vector3.forward, Vector3.right * flockRepelRadius, 360, 24, .1f, .1f);
			}
		}
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
		if(rigidbody2D.velocity.magnitude < 0.1) {
			if(sprite.animation != 0)
				sprite.SetAnimation(0);
		} else {
			if(sprite.animation != 1)
				sprite.SetAnimation(1);
		}
		sprite.Update (Time.deltaTime);
	}

	public void SteerAt(Vector3 position, float strength) {
		Vector3 delta = position - transform.position;
		delta.Normalize ();
		delta *= maxSteering;
		steering = Vector3.Lerp (steering, delta, strength);
	}

	public void UserClick() {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		targetLocation.x = ray.origin.x; // if the camera angle ever changes, this code will break!
		targetLocation.y = ray.origin.y;
		Collider2D[] hits = Physics2D.OverlapPointAll(targetLocation);
		if(hits.Length > 0) {
			for(int i = 0; i < hits.Length; ++i) {
				Agent a = hits[i].GetComponent<Agent>();
				if(a != null){
					FindAttention(a);
					a.prox.CheckIntroduction();
					PlaySound.Play(GameWorld.GetGlobal().connection, transform);
				}
			}
		}
		ParticleSystem token = GameWorld.GetGlobal ().moveToken.GetComponent<ParticleSystem> ();
		token.transform.position = targetLocation;
		token.Emit (2);
	}

	void OnCollisionEnter2D(Collision2D other) {
		if(other.gameObject.GetComponent<BoxCollider2D>()) {
			steering *= -1;
			rigidbody2D.velocity *= -1;
		}
	}

}
