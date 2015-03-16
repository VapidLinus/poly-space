using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class Triangle : MonoBehaviour
{
	/// Just a test thing, ignooooree
	public Gradient gradient = new Gradient() { colorKeys = new GradientColorKey[] { new GradientColorKey(Color.white, 0f) } };

	#region Variables
	private new MeshRenderer renderer;
	private MeshFilter filter;

	private bool relativeCorners = true;
	private bool fillBack = false;
	private bool shouldRebuildMesh = true;
	#endregion

	#region Properties
	private Vector2[] corners;
	public Vector2[] Corners
	{
		get { return corners; }
		set
		{
			if (value.Length != 3) throw new ArgumentException("Must be 3 corners.");
			corners = value;
		}
	}

	/// <summary>
	/// Whether the triangle should be visible from both sides.
	/// If true, 6 vertices and triangles will be used. Otherwise, 3.
	/// </summary>
	public bool FillBack
	{
		get { return fillBack; }
		set
		{
			if (fillBack != value)
			{
				shouldRebuildMesh = true;
				fillBack = value;
			}
		}
	}

	/// <summary>
	/// Whether the triangle's corner's position should be relative to the transform position.
	/// </summary>
	public bool RelativeCorners
	{
		get { return relativeCorners; }
		set { relativeCorners = value; }
	}

	/// <summary>
	/// Gets the center of the triangle, by adding all corner locations and deviding by 3.
	/// </summary>
	public Vector2 Center
	{
		get
		{
			Vector2 center = Vector2.zero;
			for (int i = 0; i < Corners.Length; i++)
			{
				center += Corners[i];
			}
			return center / Corners.Length;
		}
	}

	/// <summary>
	/// Triangle's color.
	/// </summary>
	public Color Color
	{
		get { return renderer.material.color; }
		set { renderer.material.color = value; }
	}

	/// <summary>
	/// Triangle's material.
	/// </summary>
	public Material Material
	{
		get { return renderer.material; }
		set { renderer.material = value; }
	}
	#endregion

	void Awake()
	{
		// References
		renderer = GetComponent<MeshRenderer>();
		filter = GetComponent<MeshFilter>();

		// Default shape
		corners = new Vector2[] { new Vector2(0, 0), new Vector2(.5f, .8f), new Vector2(1, 0) };

		// Default unlit color
		renderer.material = new Material(Shader.Find("Unlit/Color"));
	}

	void Update()
	{
		// The gradient is just a temporary test for the color
		if (gradient != null)
			Color = gradient.Evaluate(Mathf.PingPong(Time.time, 1));

		// Do the fun part!
		UpdateMesh();
	}

	private void UpdateMesh()
	{
		// If the mesh needs to be rebuilt
		if (shouldRebuildMesh)
		{
			shouldRebuildMesh = false; // It no longer does!

			// Generate mesh
			Mesh mesh = new Mesh()
			{
				vertices = GenerateVertices(),
				triangles = fillBack ? new int[] { 0, 1, 2, 2, 1, 0 } : new int[] { 0, 1, 2 }
			};

			// Assign
			mesh.RecalculateNormals();
			filter.mesh = mesh;
		}
		else
		{
			// Just update the vertices
			filter.mesh.vertices = GenerateVertices();
		}
		// Calculate new bounds, so the mesh stops rendering when out of screen.
		filter.mesh.RecalculateBounds();
	}

	private Vector3[] GenerateVertices()
	{
		Vector2 position = relativeCorners ? Vector2.zero : (Vector2)transform.position;

		return fillBack ?
			new Vector3[] { 
				(Vector2)corners[0] - position,
				(Vector2)corners[1] - position,
				(Vector2)corners[2] - position,
				(Vector2)corners[0] - position,
				(Vector2)corners[1] - position,
				(Vector2)corners[2] - position
			} :
			new Vector3[] { 
				(Vector2)corners[0] - position,
				(Vector2)corners[1] - position,
				(Vector2)corners[2] - position
			};
	}

	public static Triangle Create(Vector2 point1, Vector2 point2, Vector2 point3, bool fillBack = false, bool relativeCorners = true)
	{
		var triangle = new GameObject("Triangle").AddComponent<Triangle>();
		triangle.Corners = new Vector2[] { point1, point2, point3 };
		triangle.FillBack = fillBack;
		triangle.relativeCorners = relativeCorners;
		return triangle;
	}
}