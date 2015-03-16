using UnityEngine;
using System.Collections;

public class TriangleHolder : MonoBehaviour
{
	public Triangle Triangle { get; private set; }
	public Transform[] Points { get; private set; }

	public Transform Point1 { get { return Points[0]; } }
	public Transform Point2 { get { return Points[1]; } }
	public Transform Point3 { get { return Points[2]; } }

	void Update()
	{
		transform.rotation = Quaternion.identity;
		Triangle.Corners = new Vector2[] { Point1.position, Point2.position, Point3.position };
	}

	public static TriangleHolder Create(Transform point1, Transform point2, Transform point3)
	{
		var holder = new GameObject("TriangleHolder").AddComponent<TriangleHolder>();
		holder.Triangle = Triangle.Create(point1.position, point2.position, point3.position, false, false);
		holder.Triangle.transform.parent = holder.transform;
		holder.Points = new Transform[] { point1, point2, point3 };
		return holder;
	}
}