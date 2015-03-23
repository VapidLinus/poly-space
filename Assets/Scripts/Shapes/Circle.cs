using UnityEngine;
using System.Collections;

public class Circle : MonoBehaviour
{
	private new MeshRenderer renderer;
	private MeshFilter filter;

	private bool relativeCorners = true;
	private bool fillBack = false;
	private bool isMeshDirty = true;

	private int edges;
	public int Edges
	{
		get { return edges; }
		set
		{
			edges = Mathf.Max(value, 3);
			float[] newRadius = new float[edges];
			for (int i = 0; i < newRadius.Length; i++)
			{
				newRadius[i] = radius.Length > i ? radius[i] : 1f;
			}
			radius = newRadius;
			isMeshDirty = true;
		}
	}

	public Color Color
	{
		get { return renderer.material.color; }
		set { renderer.material.color = value; }
	}

	private float[] radius;

	void Awake()
	{
		// References
		renderer = gameObject.GetOrAddComponent<MeshRenderer>();
		filter = gameObject.GetOrAddComponent<MeshFilter>();

		// Default unlit color
		renderer.material = new Material(Shader.Find("Unlit/Color"));

		// Setup variables
		edges = 32;
		radius = new float[edges];
		SetRadius(1f);
	}

	void Update()
	{
		// Rebuild the mesh if it's dirty
		if (isMeshDirty) BuildMesh();
	}

	private void BuildMesh()
	{
		// Mesh no longer needs to be rebuilt
		isMeshDirty = false;

		// Initialize arrays
		Vector3[] vertices = new Vector3[edges + 1];
		Vector2[] uv = new Vector2[edges + 1];
		int[] triangles = new int[vertices.Length * 3];

		// Center point
		uv[0] = vertices[0] = Vector2.zero;

		// Build vertices
		for (int i = 1; i < vertices.Length; i++)
		{
			float radians = (i / (float)edges) * 360f * Mathf.Deg2Rad;

			float rad = radius[i - 1];

			uv[i] = vertices[i] = new Vector2(
				Mathf.Sin(radians),
				Mathf.Cos(radians)) * rad;
		}

		// Build triangles
		for (int i = 0; i < edges; i++)
		{
			triangles[i * 3 + 0] = 0;
			triangles[i * 3 + 1] = i + 1;
			triangles[i * 3 + 2] = Mathf.Max((i + 2) % vertices.Length, 1);
		}

		// Build mesh
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = triangles;

		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		filter.mesh = mesh;
	}

	/// <summary>Set the entire circle's radius.</summary>
	/// <param name="radius">Radius for circle.</param>
	public void SetRadius(float radius)
	{
		for (int i = 0; i < this.radius.Length; i++)
		{
			this.radius[i] = radius;
		}
		isMeshDirty = true;
	}

	public void SetRadius(int edge, float radius)
	{
		this.radius[edge] = radius;
		isMeshDirty = true;
	}

	public float GetRadius(int edge)
	{
		return radius[edge];
	}

	public static Circle Create(int edges, float radius, Vector2 position, Transform parent = null)
	{
		var circle = new GameObject("Circle").AddComponent<Circle>();
		circle.Edges = edges;
		circle.SetRadius(radius);

		circle.transform.position = position;
		circle.transform.parent = parent;

		return circle;
	}
}