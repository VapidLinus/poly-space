using UnityEngine;
using System.Collections;

public static class Vector2Util 
{
	public static float DistanceSquared(Vector2 v1, Vector2 v2)
	{
		float xDiff = v1.x - v2.x;
		float yDiff = v1.y - v2.y;

		return xDiff * xDiff + yDiff * yDiff;
	}
}