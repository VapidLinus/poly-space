using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Asteroid : MonoBehaviour
{
	private HashSet<TriangleHolder> triangles;
	private HashSet<Transform> points;

	void Awake()
	{
		triangles = new HashSet<TriangleHolder>();
		points = new HashSet<Transform>();
	}

	void Start()
	{
		const float RADIUS = .5f;
		const float RADIUS_JITTER = .1f;
		const float ANGLE_JITTER = 50f;

		CreateTriangle(
			AngleToVector(120 + Random.Range(-ANGLE_JITTER, ANGLE_JITTER)) * (RADIUS + Random.Range(-RADIUS_JITTER, RADIUS_JITTER)),
			AngleToVector(240 + Random.Range(-ANGLE_JITTER, ANGLE_JITTER)) * (RADIUS + Random.Range(-RADIUS_JITTER, RADIUS_JITTER)),
			AngleToVector(360 + Random.Range(-ANGLE_JITTER, ANGLE_JITTER)) * (RADIUS + Random.Range(-RADIUS_JITTER, RADIUS_JITTER)));
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			// Direction to mouse
			Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector2 direction = (mousePosition - (Vector2)transform.position).normalized;

			// Find point 1 and 2
			Transform point1 = GetClosestPoint(mousePosition + direction);
			Transform point2 = null;

			// Ignore list
			var ignore = new HashSet<Transform>();
			ignore.Add(point1);

			while (point2 == null || !IsTriangleNeighbours(point1, point2)) 
			{
				Transform[] ignoreList = new Transform[ignore.Count];
				ignore.CopyTo(ignoreList);

				point2 = GetClosestPoint(mousePosition + direction, ignoreList);
				if (!IsTriangleNeighbours(point1, point2)) 
				{
					ignore.Add(point2);
					point2 = null;
				}
			}

			Vector2 point3;
			point3 = ((Vector2)point1.position + (Vector2)point2.position) / 2f + direction;

			CreateTriangle(point1.position, point2.position, point3);
		}
	}

	private Transform GetSmallestAnglePoint(Vector2 direction, Transform[] ignore = null)
	{
		Transform closest = null;
		float closestAngle = float.MaxValue;

		foreach (Transform point in points)
		{
			// Ignore objects in the ignore list
			if (ignore != null && Array.IndexOf(ignore, point) != -1) continue;

			// If angle is smaller than smallest recorded
			float angle = Vector2.Angle(point.position - transform.position, direction);
			if (angle < closestAngle)
			{
				closest = point;
				closestAngle = angle;
			}
		}

		return closest;
	}

	private TriangleHolder FindTriangleWithPoints(Transform point1, Transform point2)
	{
		foreach (var triangle in triangles)
		{
			bool foundPoint1 = false;
			bool foundPoint2 = false;

			for (int i = 0; i < triangle.Points.Length; i++)
			{
				if (!foundPoint1 && triangle.Points[i] == point1) foundPoint1 = true;
				if (!foundPoint2 && triangle.Points[i] == point1) foundPoint2 = true;
			}

			if (foundPoint1 && foundPoint2) return triangle;
		}

		return null;
	}

	private Transform GetClosestPoint(Vector2 position, Transform[] ignore = null)
	{
		Transform closest = null;
		float closestDistance = float.MaxValue;

		foreach (var point in points)
		{
			// Ignore objects in the ignore list
			if (ignore != null && Array.IndexOf(ignore, point) != -1) continue;

			float distanceSquared = Vector2Util.DistanceSquared(position, point.position);
			if (distanceSquared < closestDistance)
			{
				closest = point;
				closestDistance = distanceSquared;
			}
		}

		return closest;
	}

	private HashSet<Transform> GetConnectedPoints(TriangleHolder triangle)
	{
		var connected = new HashSet<Transform>();
		for (int i = 0; i < triangle.Points.Length; i++)
		{
			connected.Add(triangle.Points[i]);
		}
		return connected;
	}

	private Transform CreateOrGetPoint(Vector2 position, Transform[] ignore = null)
	{
		const float MAX_DISTANCE = .01f;

		// Find close point
		foreach (var point in points)
		{
			// Ignore objects in the ignore list
			if (ignore != null && Array.IndexOf(ignore, point) != -1) continue;

			// Return this point if closer than max distance
			Vector2 pointPosition = point.transform.position;

			float xDiff = pointPosition.x - position.x;
			float yDiff = pointPosition.y - position.y;

			if (xDiff * xDiff + yDiff * yDiff < MAX_DISTANCE * MAX_DISTANCE)
				return point;
		}

		// Create point
		var p = new GameObject("Point").transform;
		p.position = position;
		p.parent = transform;

		// DEBUG
		p.gameObject.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures/Circle");
		p.gameObject.GetComponent<SpriteRenderer>().color = Color.blue;
		p.gameObject.GetComponent<SpriteRenderer>().sortingLayerID = 100;

		points.Add(p);
		return p;
	}

	/// <summary>Check if any of the points' triangles are neighbours.</summary>
	/// <param name="point1"></param>
	/// <param name="point2"></param>
	/// <returns>Whether any of the points' triangles are neighbours.</returns>
	private bool IsTriangleNeighbours(Transform point1, Transform point2)
	{
		// Get all triangles connected to each point
		var point1Triangles = GetConnectedTriangles(point1);
		var point2Triangles = GetConnectedTriangles(point2);

		// If the two lists have a shared triangle, then the point's triangles are neighbours
		foreach (var triangle1 in point1Triangles)
		{
			foreach (var triangle2 in point2Triangles)
			{
				if (triangle1 == triangle2) return true;
			}
		}

		return false; // None found :(
	}

	/// <summary> Returns all triangles that use the specified point.</summary>
	/// <param name="point"></param>
	/// <returns>All triangles that use point.</returns>
	private HashSet<TriangleHolder> GetConnectedTriangles(Transform point)
	{
		var connected = new HashSet<TriangleHolder>();
		foreach (var triangle in triangles)
		{
			// Loop through triangle's points
			for (int i = 0; i < triangle.Points.Length; i++)
			{
				// If triangle has a point we're searching for
				if (triangle.Points[i] == point)
				{
					// Add it and skip this triangle to prevent it being in list twice
					connected.Add(triangle);
					break;
				} 
			}
		}
		return connected;
	}

	private void CreateTriangle(Vector2 point1, Vector2 point2, Vector2 point3)
	{
		// Sort points in a clockwise order
		Vector2[] clockwise = new Vector2[] { point1, point2, point3 };
		Array.Sort(clockwise, new ClockwiseComparer((point1 + point2 + point3) / 3f));

		var p1 = CreateOrGetPoint(clockwise[0]);
		var p2 = CreateOrGetPoint(clockwise[1], new Transform[] { p1 });
		var p3 = CreateOrGetPoint(clockwise[2], new Transform[] { p1, p2 });

		// Create triangle
		var holder = TriangleHolder.Create(p1, p2, p3);
		holder.Triangle.Color = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
		holder.transform.parent = transform;
		triangles.Add(holder);
	}

	private Vector2 AngleToVector(float angle)
	{
		return new Vector2(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad));
	}
}