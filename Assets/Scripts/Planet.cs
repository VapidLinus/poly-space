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

	private float shakeProgress = 0;
	private float shake = 0f;

	void Awake()
	{
		circle = Circle.Create(128, 1f, transform.position, transform);
		circle.Color = new Color(27 / 255f, 226 / 255f, 21 / 255f);

		for (int i = 0; i < heightmap.Length; i++)
		{
			heightmap[i] = 1f;
		}
	}

	void Update()
	{
		float acceleration = (Input.acceleration.magnitude - .8f);

		shake = Mathf.Lerp(shake, 0, Time.deltaTime * .55f) + acceleration * Time.deltaTime * 1.2f;
		Debug.Log(Input.acceleration.magnitude);
		if (Input.GetKeyDown(KeyCode.Space)) shake += 1;

		shakeProgress += acceleration * Time.deltaTime;

		for (int i = 0; i < heightmap.Length; i++)
		{
			heightmap[i] = .5f + Mathf.PerlinNoise(i / (10f - shake) - shakeProgress * 2, Time.time + i / 10f + shakeProgress) * shake;
		}

		float[] clone = (float[])heightmap.Clone();
		for (int i = 0; i < heightmap.Length; i++)
		{
			const int SMOOTHING = 5;

			float average = 0;
			for (int j = -SMOOTHING; j <= SMOOTHING; j++)
			{
				average += clone[MathUtil.Mod(i + j, heightmap.Length)];
			}
			average /= SMOOTHING * 2 + 1;
			heightmap[i] = average;
		}

		int r = Random.Range(0, heightmap.Length);

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