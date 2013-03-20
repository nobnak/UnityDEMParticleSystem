using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class MeshFallController : MonoBehaviour {
	public int nParticles = 100;
	public float fuzzy = 5;
	public Vector3 transition = new Vector3(0, -10, 0);
	public float[] rangeRadius = new float[]{ 0.1f, 1f };
	public float[] rangeX = new float[]{ -80, 80 };
	public float[] rangeY = new float[]{ -80, 80 };
	
	private MeshFilter meshFilter;
	private Mesh mesh;
	private MeshFallModel fallModel;
	private int[] particleIndices;
	private Vector3[] positions;

	// Use this for initialization
	void Start () {
		meshFilter = (MeshFilter) GetComponent("MeshFilter");
		mesh = new Mesh();
		meshFilter.sharedMesh = mesh;
		fallModel = new MeshFallModel(mesh);
		
		particleIndices = new  int[nParticles];
		positions = new Vector3[nParticles];
		float[] radiuss = new float[nParticles];
		for (int i = 0; i < nParticles; i++) {
			particleIndices[i] = i;
			positions[i] = new Vector3(
				Random.Range(rangeX[0], rangeX[1]),
				Random.Range(rangeY[0], rangeY[1]),
				0);
			radiuss[i] = Random.Range(rangeRadius[0], rangeRadius[1]);
		}
		fallModel.addParticle(positions, radiuss);
	}
	
	// Update is called once per frame
	void Update () {
		float dT = Time.deltaTime;
		for (int i = 0; i < nParticles; i++) {
			Vector3 pos = positions[i];
			pos += transition * dT;
			if (pos.y < rangeY[0] || rangeY[1] < pos.y) {
				pos.y = rangeY[1] + Random.value * fuzzy;
			}
			positions[i] = pos;
		}
		fallModel.moveParticle(particleIndices, positions);
	}
	
	void OnDisable() {
		Destroy(mesh);
	}
}
