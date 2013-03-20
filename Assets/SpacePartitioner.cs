using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public struct AABB {
	public Vector2 min;
	public Vector2 max;
	
	public AABB(Vector2 min, Vector2 max) {
		this.min = min;
		this.max = max;
	}
}
public struct Bound {
	public int iXMin;
	public int iYMin;
	public int iXMax;
	public int iYMax;
	
	public Bound(int iXMin, int iYMin, int iXMax, int iYMax) {
		this.iXMin = iXMin;
		this.iYMin = iYMin;
		this.iXMax = iXMax;
		this.iYMax = iYMax;
	}
}
public struct GridPoint : IEquatable<GridPoint> {
	public int x;
	public int y;
	public GridPoint(int x, int y) {
		this.x = x;
		this.y = y;
	}
	
	public bool Equals(GridPoint gp) {
		return x == gp.x && y == gp.y;
	}
}
public interface ISpacePartitioning<T> {
	T[] search(AABB box);
	void add(AABB box, T val);
	void move(AABB box, T val);
	void remove(T val);
	void clear();
}
public class NoSP<T> : ISpacePartitioning<T> {
	private HashSet<T> indices;
	
	public NoSP() {
		this.indices = new HashSet<T>();
	}
	
	#region ISpacePartitioning[T] implementation
	public T[] search (AABB box)
	{
		T[] res = new T[indices.Count];
		indices.CopyTo(res);
		return res;
	}

	public void add (AABB box, T val)
	{
		indices.Add(val);
	}

	public void move (AABB box, T val)
	{
		remove(val);
		add(box, val);
	}

	public void remove (T val)
	{
		indices.Remove(val);
	}
	
	public void clear()
	{
		indices.Clear();
	}
	#endregion
}
public class IntEqualityComparer : IEqualityComparer<int> {
	#region IEqualityComparer[System.Int32] implementation
	public bool Equals (int x, int y)
	{
		return x == y;
	}

	public int GetHashCode (int obj)
	{
		return obj;
	}
	#endregion	
}

public class GridSP<T> : ISpacePartitioning<T> {
	private float[] x;
	private float[] y;
	private int nX;
	private int nY;
	private float dxInv;
	private float dyInv;
	private List<T>[,] boxes;
	private Dictionary<T, LinkedList<GridPoint>> invIndices;
	
	public GridSP(Vector2 min, Vector2 max, int nX, int nY, IEqualityComparer<T> eq) {
		invIndices = new Dictionary<T, LinkedList<GridPoint>>(eq);
		boxes = new List<T>[nX, nY];
		x = new float[nX + 1];
		y = new float[nY + 1];
		this.nX = nX;
		this.nY = nY;
		
		Vector2 size = max - min;
		float dx = size.x / nX;
		dxInv = 1.0f / dx;
		float dy = size.y / nY;
		dyInv = 1.0f / dy;
		
		for (int i = 0; i < nX; i++)
			x[i] = min.x + dx * i;
		x[nX] = max.x;
		
		for (int i = 0; i < nY; i++)
			y[i] = min.y + dy * i;
		x[nY] = max.y;		
	}
	
	public Bound boundIndices(AABB box) {
		int iXMin = Mathf.FloorToInt((box.min.x - x[0]) * dxInv);
		int iXMax = Mathf.FloorToInt((box.max.x - x[0]) * dxInv);
		int iYMin = Mathf.FloorToInt((box.min.y - y[0]) * dyInv);
		int iYMax = Mathf.FloorToInt((box.max.y - y[0]) * dyInv);
		
		iXMin = iXMin < 0 ? 0 : (iXMin >= nX ? nX-1 : iXMin);
		iXMax = iXMax < 0 ? 0 : (iXMax >= nX ? nX-1 : iXMax);
		iYMin = iYMin < 0 ? 0 : (iYMin >= nY ? nY-1 : iYMin);
		iYMax = iYMax < 0 ? 0 : (iYMax >= nY ? nY-1 : iYMax);
		
		return new Bound(iXMin, iYMin, iXMax, iYMax);
	}
	
	public T[] search(AABB box) {
		Bound indices = boundIndices(box);
		
		List<T> founds = new List<T>();
		for (int iy = indices.iYMin; iy <= indices.iYMax; iy++) {
			for (int ix = indices.iXMin; ix <= indices.iXMax; ix++) {
				List<T> resInBox = boxes[ix, iy];
				if (resInBox != null)
					foreach (T res in resInBox)
						if (!founds.Contains(res))
							founds.Add(res);
			}
		}
		
		return founds.ToArray();
	}
	
	public void add(AABB box, T val) {
		Bound indices = boundIndices(box);
		
		for (int iy = indices.iYMin; iy <= indices.iYMax; iy++) {
			for (int ix = indices.iXMin; ix <= indices.iXMax; ix++) {
				List<T> valsInBox = boxes[ix, iy];
				LinkedList<GridPoint> boxIndices = null;
				invIndices.TryGetValue(val, out boxIndices);
				if (valsInBox == null) {
					valsInBox = new List<T>();
					boxes[ix, iy] = valsInBox;
				}
				if (boxIndices == null) {
					boxIndices = new LinkedList<GridPoint>();
					invIndices[val] = boxIndices;
				}
				
				valsInBox.Add(val);
				GridPoint gp = new GridPoint(ix, iy);
				if (!boxIndices.Contains(gp)) {
					boxIndices.AddLast(gp);
				}
			}
		}
	}
	
	public void move(AABB box, T val) {
		remove(val);
		add(box, val);
	}
	
	public void remove(T val) {
		LinkedList<GridPoint> boxIndices = invIndices[val];
		foreach (GridPoint gp in boxIndices) {
			List<T> valsInBox = boxes[gp.x, gp.y];
			valsInBox.Remove(val);
		}
	}
	
	public void clear() {
		foreach (LinkedList<GridPoint> boxIndices in invIndices.Values) {
			foreach (GridPoint gp in boxIndices) {
				boxes[gp.x, gp.y].Clear();
			}
		}
		invIndices.Clear();
	}
}