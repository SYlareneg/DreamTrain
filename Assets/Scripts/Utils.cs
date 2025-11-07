using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

[System.Serializable]
public class PRS
{
	public Vector3 pos;
	public Quaternion rot;
	public Vector3 scale;

	public PRS(Vector3 pos, Quaternion rot, Vector3 scale)
	{
		this.pos = pos;
		this.rot = rot;
		this.scale = scale;
	}
}

public class Utils
{
	public static Quaternion QI => Quaternion.identity;

	public static Vector3 MousePos
	{
		get
		{
			Vector3 result = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			result.z = -10;
			return result;
		}
	}

	public static Action AllignActions(ref Action targetAction, Type pHighClass, Type pLowClass)
	{
		if (targetAction == null) return null;
		var list = targetAction.GetInvocationList().ToList();
		int pHighIndex = list.FindIndex(d => d.Target != null && GetOwningType(d) == pHighClass);
		int pLowIndex = list.FindIndex(d => d.Target != null && GetOwningType(d) == pLowClass);

		if (pHighIndex < 0 || pLowIndex < 0 || pHighIndex < pLowIndex) return targetAction;

		var pHighAction = list[pHighIndex];
		list.RemoveAt(pHighIndex);
		list.Insert(pLowIndex, pHighAction);
		targetAction = null;
		foreach (var a in list)
		{
			targetAction += (Action)a;
		}
		return targetAction;
	}

	public static Action<T> AllignActions<T>(ref Action<T> targetAction, Type pHighClass, Type pLowClass)
	{
		if (targetAction == null) return null;
		var list = targetAction.GetInvocationList().ToList();
		int pHighIndex = list.FindIndex(d => d.Target != null && GetOwningType(d) == pHighClass);
		int pLowIndex = list.FindIndex(d => d.Target != null && GetOwningType(d) == pLowClass);

		if (pHighIndex < 0 || pLowIndex < 0 || pHighIndex < pLowIndex) return targetAction;

		var pHighAction = list[pHighIndex];
		list.RemoveAt(pHighIndex);
		list.Insert(pLowIndex, pHighAction);
		targetAction = null;
		foreach (var a in list)
		{
			targetAction += (Action<T>)a;
		}
		return targetAction;
	}

	public static Action SwitchActions(ref Action targetAction, Type pHighClass, Type pLowClass)
	{
		if (targetAction == null) return null;
		var list = targetAction.GetInvocationList().ToList();
		int pHighIndex = list.FindIndex(d => d.Target != null && GetOwningType(d) == pHighClass);
		int pLowIndex = list.FindIndex(d => d.Target != null && GetOwningType(d) == pLowClass);

		if (pHighIndex < 0 || pLowIndex < 0) return targetAction;

		var pHighAction = list[pHighIndex];
		list[pHighIndex] = list[pLowIndex];
		list[pLowIndex] = pHighAction;
		targetAction = null;
		foreach (var a in list)
		{
			targetAction += (Action)a;
		}
		return targetAction;
	}

	public static Action<T> SwitchActions<T>(ref Action<T> targetAction, Type pHighClass, Type pLowClass)
	{
		if (targetAction == null) return null;
		var list = targetAction.GetInvocationList().ToList();
		int pHighIndex = list.FindIndex(d => d.Target != null && GetOwningType(d) == pHighClass);
		int pLowIndex = list.FindIndex(d => d.Target != null && GetOwningType(d) == pLowClass);

		if (pHighIndex < 0 || pLowIndex < 0) return targetAction;

		var pHighAction = list[pHighIndex];
		list[pHighIndex] = list[pLowIndex];
		list[pLowIndex] = pHighAction;
		targetAction = null;
		foreach (var a in list)
		{
			targetAction += (Action<T>)a;
		}
		return targetAction;
	}

	public static Type GetOwningType(Delegate d)
	{
		if (d == null) return null;

		Type t = d.Method.DeclaringType;
		while (t != null && IsCompilerGenerated(t))
		{
			t = t.DeclaringType;
		}

		if (t == null && d.Target != null) t = d.Target.GetType();
		return t;
	}
	
	private static bool IsCompilerGenerated(Type t)
    {
        if (t == null) return false;
        if (t.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false)) return true;
        string n = t.FullName ?? t.Name ?? "";
        return n.Contains("<>c") || n.Contains("DisplayClass");
    }
}