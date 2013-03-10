using UnityEngine;
using System.Collections;

public interface IAction1<T> {
	void call(T target);
	
}

public class DefaultAction1<T> : IAction1<T> {
	public void call(T target) {}
}