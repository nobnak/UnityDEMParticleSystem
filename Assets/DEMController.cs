using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DEMController : MonoBehaviour {
	public GameObject[] spheres;
	
	private float rho = 998;
	private float kn = 100000;
	private float cn = 100000;
	private Vector2 boundBL = new Vector2(-50, -50);
	private Vector2 boundTR = new Vector2(50, 50);
	
	private DEM dem;
	private ISpacePartitioning<int> spacePartitioner;
	private IForce gravitationalForce;
	private IForce boundaryForce;
	private DEMParticles particles;

	// Use this for initialization
	void Start () {
		this.particles = new DEMParticles();
		this.spacePartitioner = new GridSP<int>(
			boundBL, boundTR, 10, 10, new IntEqualityComparer());
		this.gravitationalForce = new GravitationalForce(new Vector2(0, -9.8f));
		this.boundaryForce = new BoundaryForce(
			kn, cn, boundBL.x, boundBL.y, boundTR.x, boundTR.y);
		this.dem = new DEM(spacePartitioner, gravitationalForce, boundaryForce);
		
		int length = spheres.Length;
		float[] radii = new float[length];
		float[] masses = new float[length];
		Vector2[] positions = new Vector2[length];
		Vector2[] velocities = new Vector2[length];
		for (int i = 0; i < length; i++) {
			GameObject sphere = spheres[i];
			float radius = 0.5f * sphere.transform.localScale.x;
			radii[i] = radius;
			masses[i] = (rho * Mathf.PI * radius);
			positions[i] = new Vector2(
				sphere.transform.position.x, sphere.transform.position.y);
			velocities[i] = Vector2.zero;
		}
		
		particles.addParticle(masses, radii, positions, velocities);
		for (int i = 0; i < length; i++) {
			spacePartitioner.add(particles.buildAABB(i), i);
		}
	}
	
	// Update is called once per frame
	void Update () {
		dem.simulate(particles, Time.deltaTime);
		List<Vector2> positions = particles.positions;
		
		for (int i = 0; i < particles.length; i++) {
			GameObject sphere = spheres[i];
			Vector2 position = positions[i];
			sphere.transform.position = new Vector3(position.x, position.y);
			spacePartitioner.move(particles.buildAABB(i), i);
		}
	}
}

