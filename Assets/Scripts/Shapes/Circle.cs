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
			isMeshDirty = true;
		}
	}

	private float[] radius;
	public float[] Radius
	{
		get { return radius; }
		set
		{
			if (value.Length != edges) throw new System.ArgumentException("Radius length must be equal to edge count.");
			radius = value;
			isMeshDirty = true;
		}
	}

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
		// If the mesh needs to be rebuilt
		if (isMeshDirty)
			UpdateMesh();
	}

	/// <summary>Set the entire circle's radius.</summary>
	/// <param name="radius">Radius for circle.</param>
	public void SetRadius(float radius)
	{
		for (int i = 0; i < this.radius.Length; i++)
		{
			this.radius[i] = radius;
		}
	}

	private void UpdateMesh()
	{
		// Mesh no longer needs to be rebuilt
		isMeshDirty = false;

		// Initialize arrays
		Vector3[] vertices = new Vector3[edges + 1];
		int[] triangles = new int[vertices.Length * 3];

		// Center point
		vertices[0] = Vector2.zero;

		// Build vertices
		for (int i = 1; i < vertices.Length; i++)
		{
			float radians = (i / (float)edges) * 360f * Mathf.Deg2Rad;

			vertices[i] = new Vector2(
				Mathf.Sin(radians),
				Mathf.Cos(radians)) * radius[i - 1];
		}

		// Build triangles
		for (int i = 0; i < edges; i++)
		{
			triangles[i * 3 + 0] = 0;
			triangles[i * 3 + 1] = i + 1;
			triangles[i * 3 + 2] = Mathf.Max((i + 2) % vertices.Length, 1);
		}

		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = triangles;

		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		filter.mesh = mesh;
	}
}