using UnityEngine;
using System.Collections;

public class Steering {
	public static float CLOSE_ENOUGH = 1.0f/256.0f;
	/// <summary>the direction that should be accelerated toward</summary>
	/// <returns>The steering direction.</returns>
	/// <param name="position">Position.</param>
	/// <param name="velocity">Velocity.</param>
	/// <param name="desiredSpeed">Desired speed.</param>
	/// <param name="desiredLocation">Desired location.</param>
	public static Vector3 Seek(Vector3 position, Vector3 velocity, float desiredSpeed, Vector3 desiredLocation) {
		Vector3 delta = desiredLocation - position;
		float distance = delta.magnitude;
		Vector3 desiredVelocity = delta.normalized * desiredSpeed;
		Vector3 velocityDelta = desiredVelocity - velocity;
		if(velocityDelta.sqrMagnitude < CLOSE_ENOUGH) {
			return Vector3.zero;
		}
		return velocityDelta.normalized;
	}

	public static float BrakeDistance(float speed, float acceleration) {
		return (speed*speed)/(2*acceleration);
	}

	public static Vector3 Arrive(Vector3 position, Vector3 velocity, float maxVelocity, float maxSteering, Vector3 targetLocation) {
		Vector3 delta = targetLocation - position;
		float distanceFromTarget = delta.magnitude;
		float speed = velocity.magnitude;
		float brakeDistanceNeeded = (speed*speed)/(2*maxSteering);
		if(distanceFromTarget < CLOSE_ENOUGH) {
			return Vector3.zero;
		}
		if(distanceFromTarget <= brakeDistanceNeeded) {
			return -velocity; // stop!
		}
		return Seek (position,velocity, maxVelocity, targetLocation);
	}
}
