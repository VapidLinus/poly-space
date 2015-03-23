using UnityEngine;
using System.Collections;

public class Planet : MonoBehaviour
{
	public AnimationCurve roundCurve;
	public AnimationCurve curve;
	private Circle circle;
	private Circle backCircle;

	private float[] heightmap = new float[100];
	private float[] watermap = new float[100];

	void Awake()
	{
		circle = Circle.Create(1024, 1f, transform.position, transform);
		circle.Color = new Color(145 / 255f, 97 / 255f, 48 / 255f);

		for (int i = 0; i < heightmap.Length; i++)
		{
			heightmap[i] = 1f;
		}
	}

	void Update()
	{
		for (int i = 0; i < heightmap.Length; i++)
		{
			heightmap[i] = 1 + Mathf.Sin(((i / (float)heightmap.Length) * heightmap.Length) * .25f + Time.time * Time.time) * .5f + Mathf.Cos(Time.time + (i / 100f));
		}

		UpdateHeightmap();
	}

	void UpdateHeightmap()
	{
		for (int i = 0; i < circle.Edges; i++)
		{
			// Point at planet: 0f - 1f
			float point = i / (float)circle.Edges;

													// Example numbers
			float perc = point * heightmap.Length;	// 1.25
			int lowIndex = Mathf.FloorToInt(perc);	// 1
			int highIndex = Mathf.CeilToInt(perc);	// 2

			float low = heightmap[MathUtil.Mod(lowIndex, heightmap.Length)];	// 10
			float high = heightmap[MathUtil.Mod(highIndex, heightmap.Length)];	// 20

			//		10		*		.25			+ 10	= 12.5
			// (high - low) * (perc - lowIndex) + low	= alpha

			// float alpha = (high - low) * (perc - lowIndex) + low;
			float alpha = (high - low) * roundCurve.Evaluate(perc - lowIndex) + low;

			circle.SetRadius(i, alpha);
		}
	}

	/*void Update()
	{
		if (Input.GetMouseButton(0))
		{
			Vector2 direction = ((Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position)).normalized;
			AddHeight(Vector2Util.VectorToAngle(direction) / 360f, Time.deltaTime, .1f);
		}
		if (Input.GetMouseButton(1))
		{
			Vector2 direction = ((Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position)).normalized;
			AddHeight(Vector2Util.VectorToAngle(direction) / 360f, -Time.deltaTime, .1f);
		}
	}*/

	/*void AddHeight(float position, float height, float width)
	{
		position = MathUtil.Mod(position, 1f);

		int center = Mathf.RoundToInt(circle.Edges * position);
		int range = Mathf.RoundToInt(circle.Edges * width);

		for (int i = center - range; i < center + range; i++)
		{
			float add = (1 - curve.Evaluate(Mathf.Abs(i - center) / (float)range)) * height;

			int index = MathUtil.Mod(i, circle.Edges);
			circle.SetRadius(index, circle.GetRadius(index) + add);
		}
	}*/
}