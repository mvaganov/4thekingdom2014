using UnityEngine;
using System.Collections;

public class Fall : MonoBehaviour {

	Vector3 startingLocation;
	public float lowestPoint;
	// Use this for initialization
	void Start () {
		startingLocation = transform.position;
	}
	// Update is called once per frame
	void Update () {
		if(transform.position.y <= lowestPoint) {
			transform.position = startingLocation;
		}
	}
}
