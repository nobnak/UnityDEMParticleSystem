using UnityEngine;
using System.Collections;

public class CollisionParticleController : MonoBehaviour {
	public int nParticles = 100;
	public float diameter = 5;
	
	private GameObject[] particles;
	private float[] boundary = new float[]{-50, -50, 50, 50};
	private float kn = 100;

	// Use this for initialization
	void Start () {
		particles = new GameObject[nParticles];
		for (int i = 0; i < nParticles; i++) {
			GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			p.transform.localScale = new Vector3(diameter, diameter, diameter);
			p.transform.position = new Vector3(
				Random.Range(boundary[0], boundary[2]),
				Random.Range(boundary[1], boundary[3]),
				0);
			p.AddComponent("Rigidbody");
			particles[i] = p;
		}
	}
	
	// Update is called once per frame
	void Update () {
		for (int i = 0; i < nParticles; i++) {
			GameObject p = particles[i];
			Vector3 position = p.transform.position;
			
			if (position.x < boundary[0]) {
				p.rigidbody.AddForce(kn * (boundary[0] - position.x) * Vector3.right);
			} else if (boundary[2] < position.x) {
				p.rigidbody.AddForce(kn * (boundary[2] - position.x) * Vector3.right);
			}
			
			if (position.y < boundary[1]) {
				p.rigidbody.AddForce(kn * (boundary[1] - position.y) * Vector3.up);
			} else if (boundary[3] < position.y) {
				p.rigidbody.AddForce(kn * (boundary[3] - position.y) * Vector3.up);
			}
		}
	}
}
