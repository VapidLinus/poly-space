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

		CreateTriangle(AngleToVector(Random.Range(0f, 360f)), AngleToVector(Random.Range(0f, 360f)), AngleToVector(Random.Range(0f, 360f)));

		/*CreateTriangle(
			AngleToVector(120 + Random.Range(-ANGLE_JITTER, ANGLE_JITTER)) * (RADIUS + Random.Range(-RADIUS_JITTER, RADIUS_JITTER)),
			AngleToVector(240 + Random.Range(-ANGLE_JITTER, ANGLE_JITTER)) * (RADIUS + Random.Range(-RADIUS_JITTER, RADIUS_JITTER)),
			AngleToVector(360 + Random.Range(-ANGLE_JITTER, ANGLE_JITTER)) * (RADIUS + Random.Range(-RADIUS_JITTER, RADIUS_JITTER)));*/
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			// Direction to mouse
			Vector2 direction = ((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2)transform.position).normalized;

			// Find point 1 and 2
			Transform point1 = GetSmallestAnglePoint(direction);
			Transform point2 = GetSmallestAnglePoint(direction, new Transform[] { point1 });
			Vector2 point3 = ((Vector2)point1.position + (Vector2)point2.position) / 2f + direction;

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

	private HashSet<TriangleHolder> GetConnectedTriangles(Transform point)
	{
		var connected = new HashSet<TriangleHolder>();
		foreach (var triangle in triangles)
		{
			for (int i = 0; i < triangle.Points.Length; i++)
			{
				if (triangle.Points[i] == point)
				{
					connected.Add(triangle);
					break;
				}
			}
		}
		return connected;
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

	private void CreateTriangle(Vector2 point1, Vector2 point2, Vector2 point3)
	{
		// Sort points in a clockwise order
		Vector2[] clockwise = new Vector2[] { point1, point2, point3 };
		Array.Sort(clockwise, new ClockwiseComparer(Vector2.up));/*(point1 + point2 + point3) / 3f)*/

		Debug.Log("Before:");
		foreach (var v in clockwise) { Debug.Log(v.ToString()); }

		Debug.Log("Center:" + (point1 + point2 + point3) / 3f);
		

		var p = new GameObject("Center");
		p.transform.position = (point1 + point2 + point3) / 3f;
		p.gameObject.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures/Circle");
		p.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
		p.gameObject.GetComponent<SpriteRenderer>().sortingLayerID = 100;

		Debug.Log("After:");
		foreach (var v in clockwise) { Debug.Log(v.ToString()); }

		var p1 = CreateOrGetPoint(point1);
		var p2 = CreateOrGetPoint(point2, new Transform[] { p1 });
		var p3 = CreateOrGetPoint(point3, new Transform[] { p1, p2 });

		// Create triangle
		var holder = TriangleHolder.Create(p1, p2, p3);
		holder.transform.parent = transform;
		triangles.Add(holder);
	}

	private Vector2 AngleToVector(float angle)
	{
		return new Vector2(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad));
	}
}