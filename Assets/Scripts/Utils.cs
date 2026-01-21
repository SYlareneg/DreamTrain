using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.IO;
using UnityEngine.Networking;
using System.Text;

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
			Vector3 mouseScreenPos = Input.mousePosition;
			mouseScreenPos.z = -Camera.main.transform.position.z;
			Vector3 result = Camera.main.ScreenToWorldPoint(mouseScreenPos);
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

	public static Action<T1, T2> AllignActions<T1, T2>(ref Action<T1, T2> targetAction, Type pHighClass, Type pLowClass)
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
			targetAction += (Action<T1, T2>)a;
		}
		return targetAction;
	}

	public static Action<T1, T2, T3> AllignActions<T1, T2, T3>(ref Action<T1, T2, T3> targetAction, Type pHighClass, Type pLowClass)
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
			targetAction += (Action<T1, T2, T3>)a;
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

	public static IEnumerator EnsureCopiedToPersistent(string relativePath)
    {
        var dst = Path.Combine(Application.persistentDataPath, "Data", relativePath);
        var dir = Path.GetDirectoryName(dst);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        // if (File.Exists(dst)) yield break; // 유저 파일이 이미 있으면 그대로 사용

        var src = Path.Combine(Application.streamingAssetsPath, "Data", relativePath);

        using (var req = UnityWebRequest.Get(src))
        {
            yield return req.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                Debug.LogError($"Failed to read default json from StreamingAssets: {src}\n{req.error}");
                yield break;
            }

            File.WriteAllBytes(dst, req.downloadHandler.data);
            Debug.Log($"Copied default json -> persistent: {dst}");
        }
    }

	public static void SaveData(ScriptableObject so, string fileName)
    {
        string json = JsonUtility.ToJson(so, true);   // pretty print
#if UNITY_EDITOR
		string path = Path.Combine(Application.dataPath, "StreamingAssets", "Data", fileName);
#else
        string path = Path.Combine(Application.persistentDataPath, "Data", fileName);
#endif
        File.WriteAllText(path, json);
        Debug.Log($"Exported to: {path}");
    }

	public static IEnumerator LoadData(ScriptableObject target, string filePath)
    {
		string path = Path.Combine(Application.persistentDataPath, "Data", filePath);
		// if (!File.Exists(path))
        	yield return EnsureCopiedToPersistent(filePath);
        if (!File.Exists(path))
        {
            Debug.LogError("JSON file not found: " + path);
            yield break;
        }

        string json = File.ReadAllText(path);

        JsonUtility.FromJsonOverwrite(json, target);

        Debug.Log("Imported JSON to SO: " + path);
    }

	static Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
	public static Sprite LoadSpriteByName(string path, string spriteName)
    {
		if(spriteName == null || spriteName == "") return null;

		spriteName = Path.Combine(path, spriteName);
        if (!spriteCache.ContainsKey(spriteName))
		{
			spriteCache[spriteName] = Resources.Load<Sprite>(spriteName);
		}
		return spriteCache[spriteName];
    }
}


public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;  // 비활성화 (읽기 전용)
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;   // 다시 활성화
    }
}
#endif