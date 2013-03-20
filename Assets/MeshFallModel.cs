using UnityEngine;
using System.Collections;
using System;

public class MeshFallModel {
	private Mesh mesh;
	private Vector3[] vertices;
	int[] triangles;
	Vector2[] uvs;
	float[] radiuss;
	
	public MeshFallModel(Mesh mesh) {
		this.mesh = mesh;
		
		vertices = new Vector3[0];
		triangles = new int[0];
		uvs = new Vector2[0];
		radiuss = new float[0];
	}
	
	public void addParticle(Vector3[] possAdded, float[] radiussAdded) {
		int nExistentParticles = radiuss.Length;
		int nAddingParticles = radiussAdded.Length;
		int nTotalParticles = nExistentParticles + nAddingParticles;
		
		Array.Resize<Vector3>(ref vertices, nTotalParticles * 4);
		Array.Resize<int>(ref triangles, nTotalParticles * 6);
		Array.Resize<Vector2>(ref uvs, nTotalParticles * 4);
		Array.Resize<float>(ref radiuss, nTotalParticles);
		
		for (int i = nExistentParticles; i < nTotalParticles; i++) {
			int iVertex = i * 4;
			int iTriangle = i * 6;
			Vector3 pos = possAdded[i - nExistentParticles];
			float radius = radiussAdded[i - nExistentParticles];
			
			vertices[iVertex] =		pos + new Vector3(-radius, -radius, 0);
			vertices[iVertex + 1] =	pos + new Vector3(radius, -radius, 0);
			vertices[iVertex + 2] =	pos + new Vector3(radius, radius, 0);
			vertices[iVertex + 3] =	pos + new Vector3(-radius, radius, 0);			
			
			triangles[iTriangle] = iVertex;
			triangles[iTriangle + 1] = iVertex + 1;
			triangles[iTriangle + 2] = iVertex + 3;
			triangles[iTriangle + 3] = iVertex + 1;
			triangles[iTriangle + 4] = iVertex + 2;
			triangles[iTriangle + 5] = iVertex + 3;
			
			uvs[iVertex] = Vector2.zero;
			uvs[iVertex + 1] = Vector2.right;
			uvs[iVertex + 2] = new Vector2(1, 1);
			uvs[iVertex + 3] = Vector2.up;			
			
			radiuss[i] = radius;
		}
		
		mesh.Clear();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}
	
	public void moveParticle(int[] iParticles, Vector3[] poss) {
		int nParticles = iParticles.Length;
		for (int i = 0; i < nParticles; i++) {
			Vector3 pos = poss[i];
			int iParticle = iParticles[i];

			int iVertex = iParticle * 4;
			float radius = radiuss[iParticle];
			
			vertices[iVertex] =		pos + new Vector3(-radius, -radius, 0);
			vertices[iVertex + 1] =	pos + new Vector3(radius, -radius, 0);
			vertices[iVertex + 2] =	pos + new Vector3(radius, radius, 0);
			vertices[iVertex + 3] =	pos + new Vector3(-radius, radius, 0);
		}
		
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();		
	}
}
