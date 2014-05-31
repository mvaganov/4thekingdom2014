using UnityEngine;
using System.Collections;

public class Agent : MonoBehaviour {

	public bool userControlled = false;

	public Proximity prox;

	float wanderCooldown;
	public float wanderDelay = 5;

	public float maxSteering = 1;
	public float maxVelocity = 1;

	public Vector2 steering; // acceleration force

	public GameObject needsDisplay;
	bool mustShowNeeds = false;

	public Vector2 userTarget;

	private GameObject velocityLine, steeringLine;

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
		myNeed[0] = gw.GenerateRandomNeed();
		myNeed[1] = gw.GenerateRandomNeed();
		GameObject n0 = (GameObject)Instantiate (gw.prefab_needs[(int)myNeed[0]]);
		GameObject n1 = (GameObject)Instantiate (gw.prefab_needs[(int)myNeed[1]]);
		n0.transform.parent = needsDisplay.transform;
		n0.transform.position = needsDisplay.transform.position + new Vector3 (-.2f, 0, -.1f);
		n1.transform.parent = needsDisplay.transform;
		n1.transform.position = needsDisplay.transform.position + new Vector3 (.2f, 0, -.2f);
		needsDisplay.transform.position = needsDisplay.transform.position + new Vector3 (.75f, 1.5f, -2);
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
	
	// Update is called once per frame
	void Update () {
		if(!userControlled) {
			// wander code
			wanderCooldown -= Time.deltaTime;
			if(wanderCooldown <= 0) {
				steering = RandomUnitVector();
				steering *= maxSteering;
				wanderCooldown = wanderDelay;
			}
		} else {
			if(Input.GetMouseButtonDown(0)) {
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				userTarget.x = ray.origin.x; // if the camera angle ever changes, this code will break!
				userTarget.y = ray.origin.y;
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
		Lines.Make (ref velocityLine, Color.green, p, v, .1f, .1f);
		Lines.Make (ref steeringLine, Color.red, v, s, .05f, .05f);
	}

	void OnCollisionEnter2D(Collision2D other) {
		if(other.gameObject.GetComponent<BoxCollider2D>()) {
			steering *= -1;
			rigidbody2D.velocity *= -1;
		}
	}

}
