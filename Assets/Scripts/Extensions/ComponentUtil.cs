﻿using UnityEngine;

public static class GameObjectUtil 
{
	public static T GetOrAddComponent<T>(this GameObject go) where T : Component
	{
		T component = go.GetComponent<T>();
		if (component == null) component = go.AddComponent<T>();
		return component;
	}
}