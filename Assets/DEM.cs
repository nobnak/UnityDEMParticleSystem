using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class DEMParticles {
	public int length = 0;
	public List<float> massesInv = new List<float>();
	public List<float> radii = new List<float>();
	public List<Vector2> positions = new List<Vector2>();
	public List<Vector2> velocities = new List<Vector2>();
	public Vector2[] forces = new Vector2[0];
	
	public void addParticle(float[] massesInvAdd, float[] radiiAdd, 
			Vector2[] positionsAdd, Vector2[] velocitiesAdd) {
		length += massesInvAdd.Length;
		massesInv.AddRange(massesInvAdd);
		radii.AddRange(radiiAdd);
		positions.AddRange(positionsAdd);
		velocities.AddRange(velocitiesAdd);
		forces = new Vector2[length];
	}
	
	public AABB buildAABB(int index) {
		float radius = radii[index];
		Vector2 vecRadius = new Vector2(radius, radius);
		Vector2 position = positions[index];
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
	
	public DEMParticles simulate(DEMParticles particles, float t) {
		Vector2[] forces = particles.forces;
		List<Vector2> positions = particles.positions;
		List<Vector2> velocities = particles.velocities;
		List<float> massesInv = particles.massesInv;
		
		for (int iTarget = 0; iTarget < particles.length; iTarget++) {
			AABB boundingBox = particles.buildAABB(iTarget);
			int[] neighborIndices = spacePartitioner.search(boundingBox);
			forces[iTarget] = estimateForce(particles, iTarget, neighborIndices, t);
		}
		
		for (int iTarget = 0; iTarget < particles.length; iTarget++) {
			Vector2 accel = forces[iTarget] * massesInv[iTarget];
			positions[iTarget] += velocities[iTarget];
			velocities[iTarget] += accel * t;
		}
		
		return particles;
	}
	
	public Vector2 estimateForce(DEMParticles particles, 
			int iTarget, int[] neighbors, float t) {
		Vector2 resForce = externalForce;

		Vector2 targetPosition = particles.positions[iTarget];
		Vector2 targetVelocity = particles.velocities[iTarget];
		float targetRadius = particles.radii[iTarget];

		for (int iNeighbor = 0; iNeighbor < neighbors.Length; iNeighbor++) {
			if (iTarget == iNeighbor)
				continue;
			
			Vector2 pos = particles.positions[iNeighbor] - targetPosition;
			Vector2 v = particles.velocities[iNeighbor] - targetVelocity;
			float r12 = particles.radii[iNeighbor] + targetRadius;
			
			float dist2 = Vector2.SqrMagnitude(pos);
			if (dist2 >= (r12 * r12) || dist2 <= Mathf.Epsilon)
				continue;
			
			float dist = Mathf.Sqrt((float)dist2);
			float dxCoeff = ((r12 - dist) / dist);
			resForce += (-kn * dxCoeff * pos - cn * v);
		}
		return resForce;
	}
}
