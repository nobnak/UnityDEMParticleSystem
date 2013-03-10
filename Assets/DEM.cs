using UnityEngine;
using System.Collections;
using System;

public struct DEMParticle {
	public int id;
	public float massInv;
	public float radius;
	public Vector2 position;
	public Vector2 velocity;
	
	public AABB buildAABB() {
		Vector2 vecRadius = new Vector2(radius, radius);
		return new AABB(position - vecRadius, position + vecRadius);
	}
}

public class DEM {
	public float cn = 100.0f;
	public float kn = 10000.0f;
	public float ct = 1.0f;
	public float kt = 1.0f;
	
	public Vector2 externalForce = Vector2.zero;
	
	private ISpacePartitioning<int> spacePartitioner;
	
	public DEM(ISpacePartitioning<int> spacePartitioner) {
		this.spacePartitioner = spacePartitioner;
	}
	
	public DEMParticle[] simulate(DEMParticle[] particles, float t) {
		DEMParticle[] res = new DEMParticle[particles.Length];
		DEMParticle[] neighbors = particles;
		
		for (int iTarget = 0; iTarget < particles.Length; iTarget++) {
			DEMParticle target = particles[iTarget];
#if true
			int[] particleIndices = spacePartitioner.search(target.buildAABB());
			neighbors = new DEMParticle[particleIndices.Length];
			for (int iNeighbor = 0; iNeighbor < neighbors.Length; iNeighbor++)
				neighbors[iNeighbor] = particles[particleIndices[iNeighbor]];
#endif
			res[iTarget] = simulate(target, neighbors, t);
		}
		
		return res;
	}
	
	public DEMParticle simulate(DEMParticle target, DEMParticle[] neighbors, float t) {
		DEMParticle res = target;
		res = target;
		Vector2 f = externalForce;

		for (int iNeighbor = 0; iNeighbor < neighbors.Length; iNeighbor++) {
			DEMParticle neighbor = neighbors[iNeighbor];
			if (target.id == neighbor.id)
				continue;
			
			Vector2 pos = neighbor.position - target.position;
			Vector2 v = neighbor.velocity - target.velocity;
			float r12 = target.radius + neighbor.radius;
			
			float dist2 = Vector2.SqrMagnitude(pos);
			if (dist2 >= (r12 * r12) || dist2 <= Mathf.Epsilon)
				continue;
			
			float dist = Mathf.Sqrt((float)dist2);
			float dxCoeff = ((r12 - dist) / dist);
			f += (-kn * dxCoeff * pos - cn * v);
		}
		Vector2 a = f * target.massInv;
		
		res.position += target.velocity * t;
		res.velocity += a * t;
				
		return res;
	}
}
