using UnityEngine;
using System.Collections;

public class DEMController : MonoBehaviour {
	public GameObject[] spheres;
	public float rho = 998;
	
	private DEM dem;
	private ISpacePartitioning<int> spacePartitioner;
	public DEMParticle[] particles;

	// Use this for initialization
	void Start () {
		this.particles = new DEMParticle[spheres.Length];
		this.spacePartitioner = new GridSP<int>(
			new Vector2(-50, -50), new Vector2(50, 50), 
			10, 10, new IntEqualityComparer());
		this.dem = new DEM(spacePartitioner);
		this.dem.externalForce = new Vector2(0, -9800);
		
		for (int iSphere = 0; iSphere < spheres.Length; iSphere++) {
			GameObject sphere = spheres[iSphere];
			
			DEMParticle particle = new DEMParticle();
			particle.id = iSphere;
			particle.radius =  0.5f * sphere.transform.localScale.x;
			particle.massInv = 1.0f / (rho * Mathf.PI * particle.radius);
			particle.position = new Vector2(
				sphere.transform.position.x, sphere.transform.position.y);
			
			particles[iSphere] = particle;
			spacePartitioner.add(particle.buildAABB(), iSphere);
		}
	}
	
	// Update is called once per frame
	void Update () {
		DEMParticle[] particlesNext = dem.simulate(particles, Time.deltaTime);
		
		for (int iSphere = 0; iSphere < spheres.Length; iSphere++) {
			GameObject sphere = spheres[iSphere];
			DEMParticle particle = particlesNext[iSphere];
			sphere.transform.position = 
				new Vector3(particle.position.x, particle.position.y);
			spacePartitioner.move(particle.buildAABB(), iSphere);
		}
		
		particles = particlesNext;
	}
}

