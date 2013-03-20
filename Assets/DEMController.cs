using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DEMController : MonoBehaviour {
	public GameObject[] spheres;
	public float rho = 998;
	
	private DEM dem;
	private ISpacePartitioning<int> spacePartitioner;
	public DEMParticles particles;

	// Use this for initialization
	void Start () {
		this.particles = new DEMParticles();
		this.spacePartitioner = new GridSP<int>(
			new Vector2(-50, -50), new Vector2(50, 50), 
			10, 10, new IntEqualityComparer());
		this.dem = new DEM(spacePartitioner);
		this.dem.externalForce = new Vector2(0, -9800);
		
		int length = spheres.Length;
		float[] radii = new float[length];
		float[] massesInv = new float[length];
		Vector2[] positions = new Vector2[length];
		Vector2[] velocities = new Vector2[length];
		for (int i = 0; i < length; i++) {
			GameObject sphere = spheres[i];
			float radius = 0.5f * sphere.transform.localScale.x;
			radii[i] = radius;
			massesInv[i] = 1.0f / (rho * Mathf.PI * radius);
			positions[i] = new Vector2(
				sphere.transform.position.x, sphere.transform.position.y);
			velocities[i] = Vector2.zero;
		}
		
		particles.addParticle(massesInv, radii, positions, velocities);
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

