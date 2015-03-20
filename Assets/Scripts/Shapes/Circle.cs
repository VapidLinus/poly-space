using UnityEngine;
using System.Collections;

public class Circle : MonoBehaviour
{
	private new MeshRenderer renderer;
	private MeshFilter filter;

	private bool relativeCorners = true;
	private bool fillBack = false;
	private bool isMeshDirty = true;
	public float testy = 10;

	private int edges = 64;
	public int Edges
	{
		get { return edges; }
		set
		{
			edges = Mathf.Max(value, 3);
			isMeshDirty = true;
		}
	}

	private float radius = 1f;
	public float Radius { get { return radius; } set { radius = Mathf.Max(value, float.Epsilon); } }

	public bool FillBack
	{
		get { return fillBack; }
		set
		{
			if (fillBack != value)
			{
				isMeshDirty = true;
				fillBack = value;
			}
		}
	}

	void Awake()
	{
		// References
		renderer = gameObject.GetOrAddComponent<MeshRenderer>();
		filter = gameObject.GetOrAddComponent<MeshFilter>();

		// Default unlit color
		renderer.material = new Material(Shader.Find("Unlit/Color"));
	}

	void Update()
	{
		if (isMeshDirty)
			UpdateMesh();
	}

	private void UpdateMesh()
	{
		// isMeshDirty = false;

		Vector3[] vertices = new Vector3[edges + 1];
		int[] triangles = new int[vertices.Length * 3];
		vertices[0] = Vector2.zero;

		for (int i = 1; i < vertices.Length; i++)
		{
			float angle = (i / (float)edges) * 360f;

			Vector2 direction = new Vector2(
				Mathf.Sin(angle * Mathf.Deg2Rad),
				Mathf.Cos(angle * Mathf.Deg2Rad));

			vertices[i] = direction * radius * (1 + Mathf.PerlinNoise(Time.time, i / testy));
		}

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