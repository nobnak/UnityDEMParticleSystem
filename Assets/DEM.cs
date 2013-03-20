using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class DEMParticles {
	public int length = 0;
	public List<float> masses = new List<float>();
	public List<float> massesInv = new List<float>();
	public List<float> radii = new List<float>();
	public List<Vector2> positions = new List<Vector2>();
	public List<Vector2> velocities = new List<Vector2>();
	public Vector2[] forces = new Vector2[0];
	
	public void addParticle(float[] massesAdd, float[] radiiAdd, 
			Vector2[] positionsAdd, Vector2[] velocitiesAdd) {
		length += massesAdd.Length;
		
		masses.AddRange(massesAdd);
		float[] massesInvAdd = new float[massesAdd.Length];
		for (int i = 0; i < massesInvAdd.Length; i++)
			massesInvAdd[i] = 1.0f / massesAdd[i];
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
	public float cn = 1000.0f;
	public float kn = 100000.0f;
	public float ct = 1.0f;
	public float kt = 1.0f;
	
	private ISpacePartitioning<int> spacePartitioner;
	private IForce externalForce;
	private IForce boundaryForce;
	
	public DEM(ISpacePartitioning<int> spacePartitioner, 
			IForce externalForce, IForce boundaryForce) {
		this.spacePartitioner = spacePartitioner;
		this.externalForce = externalForce;
		this.boundaryForce = boundaryForce;
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
			forces[iTarget] += boundaryForce.calcForce(particles, iTarget, t);
		}
		
		for (int iTarget = 0; iTarget < particles.length; iTarget++) {
			Vector2 accel = forces[iTarget] * massesInv[iTarget];
			positions[iTarget] += velocities[iTarget] * t;
			velocities[iTarget] += accel * t;
		}
		
		return particles;
	}
	
	public Vector2 estimateForce(DEMParticles particles, 
			int iTarget, int[] neighbors, float t) {
		Vector2 resForce = externalForce.calcForce(particles, iTarget, t);

		Vector2 targetPosition = particles.positions[iTarget];
		Vector2 targetVelocity = particles.velocities[iTarget];
		float targetRadius = particles.radii[iTarget];

		for (int i = 0; i < neighbors.Length; i++) {
			int iNeighbor = neighbors[i];
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
