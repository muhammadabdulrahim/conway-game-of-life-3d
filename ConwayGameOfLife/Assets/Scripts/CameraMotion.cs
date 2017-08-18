using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//	TODO: Should have some way to hook up the Grid to the Camera to initialize sizes
public class CameraMotion : MonoBehaviour
{
	public Vector3 rotationSpeeds;
	private float deltaTime;
	
	void Update ()
	{
		transform.Rotate(Vector3.right * Time.deltaTime * rotationSpeeds.x);
		transform.Rotate(-Vector3.up * Time.deltaTime * rotationSpeeds.y);
		transform.Rotate(-Vector3.forward * Time.deltaTime * rotationSpeeds.z);
	}
}
