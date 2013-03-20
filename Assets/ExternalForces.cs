using UnityEngine;
using System.Collections;

public interface IForce {
	Vector2 calcForce(DEMParticles particles, int iParticle, float dt);
}

public class ZeroForce : IForce {
	public Vector2 calcForce(DEMParticles particles, int iParticle, float dt) {
		return Vector2.zero;
	}
}

public class GravitationalForce : IForce {
	private Vector2 gravity;
	
	public GravitationalForce(Vector2 gravity) {
		this.gravity = gravity;
	}
	
	public Vector2 calcForce(DEMParticles particles, int iParticle, float dt) {
		return particles.masses[iParticle] * gravity;
	}
}

public class BoundaryForce : IForce {
	private float kn;
	private float cn;
	private float[] boundary;
	
	public BoundaryForce(float kn, float cn, float minX, float minY, float maxX, float maxY) {
		this.kn = kn;
		this.cn = cn;
		this.boundary = new float[]{ minX, minY, maxX, maxY};
	}
	
	public Vector2 calcForce(DEMParticles particles, int iParticle, float dt) {
		Vector2 position = particles.positions[iParticle];
		Vector2 velocity = particles.velocities[iParticle];
		
		Vector2 dx = new Vector2(
			Mathf.Min(0, position.x - boundary[0]) + Mathf.Max(0, position.x - boundary[2]),
			Mathf.Min(0, position.y - boundary[1]) + Mathf.Max(0, position.y - boundary[3]));
		Vector2 dv = new Vector2((dx.x == 0 ? 0 : velocity.x), (dx.y == 0 ? 0 : velocity.y));
		return - kn * dx - cn * dv;
	}
}
