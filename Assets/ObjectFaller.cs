using UnityEngine;
using System.Collections;

public class ObjectFaller : MonoBehaviour {
	public int nObjects = 1000;
	public Vector3 size = new Vector3(1, 1, 1);
	public Vector3 transition = new Vector3(0, -10, 0);
	public float[] rangeX = new float[]{-30, 30};
	public float[] rangeY = new float[]{-30, 30};
	
	private GameObject[] objects;

	// Use this for initialization
	void Start () {
		objects = new GameObject[nObjects];
		for (int i = 0; i < nObjects; i++) {
			GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			objects[i] = obj;
			obj.transform.localScale = size;
			obj.transform.position = new Vector3(
				Random.Range(rangeX[0], rangeX[1]), 
				Random.Range(rangeY[0], rangeY[1]), 
				0);
		}
		
	}
	
	// Update is called once per frame
	void Update () {
		for (int i = 0; i < nObjects; i++) {
			GameObject obj = objects[i];
			Vector3 pos = obj.transform.position + transition * Time.deltaTime;
			if (pos.y < rangeY[0] || rangeY[1] < pos.y) {
				pos.y = rangeY[1];
			}
			obj.transform.position = pos;
		}
	}
}
