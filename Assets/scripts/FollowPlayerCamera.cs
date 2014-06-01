using UnityEngine;
using System.Collections;

public class FollowPlayerCamera : MonoBehaviour {

	public GameObject player;

	public float percentBuffer = .1f;
	private float pixelBuffer;

	static FollowPlayerCamera s_playerCamera;

	public GUISkin guiskin;

	public static FollowPlayerCamera GetGlobal() {
		if(s_playerCamera == null) {
			Object[] objects = Resources.FindObjectsOfTypeAll(typeof(GameObject));
			for (int i = 0; s_playerCamera == null && i < objects.Length; ++i) {
				if (objects[i] is GameObject) {
					GameObject go = (GameObject)objects[i];
					s_playerCamera = go.GetComponent<FollowPlayerCamera>();
				}
			}
		}
		return s_playerCamera;
	}

	Vector3 min, max, center;

	void CalcMinMax() {
		Camera cam = GetComponent<Camera> ();
		Ray minray = cam.ScreenPointToRay(new Vector2 (0, 0));
		Ray maxray = cam.ScreenPointToRay (new Vector2 (Screen.width, Screen.height));
		float w = maxray.origin.x - minray.origin.x;
		float h = maxray.origin.y - minray.origin.y;
		Vector3 bufferadjust = new Vector3 (w * percentBuffer, h * percentBuffer, 0);
		min = minray.origin + bufferadjust;
		max = maxray.origin - bufferadjust;
		min.z = 0;
		max.z = 0;
		center = (min + max) / 2;
	}

	// Use this for initialization
	void Start () {
		CalcMinMax ();
//		Debug.Log (min + "  " + max);
	}

	GameObject diagonal;
	Vector3 steering;
	
	// Update is called once per frame
	void Update () {
		CalcMinMax ();
		Vector3 p = player.transform.position;
//		Lines.Make (ref diagonal, Color.red, min, max, 1, 0);
		bool tooFarLeft = p.x < min.x;
		bool tooFarRight = p.x > max.x;
		bool tooFarUp = p.y > max.y;
		bool tooFarDown = p.y < min.y;
//		if(tooFarLeft) { Debug.Log("OOB left"); }
//		if(tooFarRight) { Debug.Log("OOB right"); }
//		if(tooFarUp) { Debug.Log("OOB up"); }
//		if(tooFarDown) { Debug.Log("OOB down"); }
		bool movePlz = tooFarLeft || tooFarRight || tooFarUp || tooFarDown;
		if(movePlz) {
			Vector3 delta = p-center;
			rigidbody2D.velocity = delta;
		} else {
			//steering = Vector3.zero;
			rigidbody2D.velocity = Vector2.zero;
		}
		if(steering != Vector3.zero) {
//			steering /= 2;
//			if(steering.x < .125f && steering.y < .125f) {
//				steering = Vector3.zero;
//				rigidbody2D.velocity = Vector3.zero;
//			}
			rigidbody2D.velocity += (Vector2)steering * Time.deltaTime;
		}

	}

	void OnGUI () {
		GUI.skin = guiskin;
		GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height));
//		GUILayout.BeginHorizontal ();
//		GUILayout.BeginVertical ();
		GUILayout.Box ("score: ");
		Agent a = player.GetComponent<Agent> ();
		GUILayout.Label (""+a.attention.Count);
//		GUILayout.EndVertical ();
//		GUILayout.EndHorizontal ();
		GUILayout.EndArea ();
	}
}
