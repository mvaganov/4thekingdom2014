using UnityEngine;
using System.Collections;

public class Agent : MonoBehaviour {

	public bool userControlled = false;

	public Proximity prox;

	public float wanderTimer;
	public float wanderDelay = 5;

	public float maxSteering = 1;

	public Vector2 steering; // acceleration force

	// Use this for initialization
	void Start () {
	
	}

	public static Vector2 RandomUnitVector() {
		Vector2 randomUnitVector;
		do {
			randomUnitVector = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
		} while(randomUnitVector == Vector2.zero);
		return randomUnitVector;
	}
	
	// Update is called once per frame
	void Update () {
		// wander code
		wanderTimer -= Time.deltaTime;
		if(wanderTimer <= 0) {
			steering = RandomUnitVector();
			steering *= maxSteering;
			wanderTimer = wanderDelay;
		}

		// general steering behavior code
		rigidbody2D.velocity = rigidbody2D.velocity + (steering * Time.deltaTime); // += won't work for properties
	}
}
