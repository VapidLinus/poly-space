using UnityEngine;
using System.Collections;

public class Planet : MonoBehaviour
{
	public float waterFriction = .95f;
	public float waterEnergy = .3f;

	public AnimationCurve roundCurve;
	public AnimationCurve curve;

	private Circle circle;
	private Circle waterCircle;

	private float[] heightmap = new float[100];
	private Vector2[] watermap = new Vector2[100];

	void Awake()
	{
		circle = Circle.Create(256, 1f, transform.position, transform);
		circle.Color = new Color(120 / 255f, 72 / 255f, 0 / 255f);

		waterCircle = Circle.Create(256, 1f, transform.position, transform);
		waterCircle.Color = new Color(167 / 255f, 219 / 255f, 216 / 255f);

		for (int i = 0; i < heightmap.Length; i++)
		{
			heightmap[i] = 1.1f;//.5f + (i / 100f) * .5f;
		}

		for (int i = 0; i < watermap.Length; i++)
		{
			watermap[i] = new Vector2(0f, .1f);
		}
	}

	void Update()
	{
		if (!Input.GetKey(KeyCode.Space))
			SimulateWater();

		if (Input.GetKeyDown(KeyCode.S))
		{
			int pos = Random.Range(10, 90);
			for (int i = pos - 10; i < pos + 10; i++)
			{
				watermap[i].y += (10 - Mathf.Abs(pos - i)) * .3f;
			}
		}

		if (Input.GetKeyDown(KeyCode.D))
		{
			int pos = Random.Range(10, 90);
			for (int i = pos - 10; i < pos + 10; i++)
			{
				heightmap[i] += (10 - Mathf.Abs(pos - i)) * .1f;
			}
		}

		if (Input.GetKeyDown(KeyCode.F))
		{
			for (int i = 0; i < 100; i++)
				watermap[i].x += Random.Range(-1f, 1f);
		}

		for (int i = 0; i < 100; i++)
		{
			Debug.DrawLine(transform.position, transform.position + (Vector3)Vector2Util.AngleToVector(i * 3.6f) * (GetWaterHeight(i) + 1), IsWaterInTerrain(i) ? Color.red : Color.green);
		}

		UpdateHeightmap(heightmap, circle, roundCurve);
		UpdateHeightmap(watermap, heightmap, waterCircle, roundCurve);
	}

	void SimulateWater()
	{
		for (int i = 0; i < 100; i++)
		{
			if (IsWaterInTerrain(i)) continue;

			float left = GetWaterHeight(MathUtil.Mod(i - 1, 100));
			float middle = GetWaterHeight(i);
			float right = GetWaterHeight(MathUtil.Mod(i + 1, 100));

			float velocity = 0;

			if (left < 0) left = 0;
			if (right < 0) right = 0;

			velocity += left - middle;
			velocity += right - middle;

			watermap[i].x -= velocity * waterEnergy;
		}

		float[] move = new float[100];
		for (int i = 0; i < 100; i++)
		{
			if (IsWaterInTerrain(i))
			{
				move[i] = 0;
			}
			else
			{
				move[i] = watermap[i].x;
				watermap[i].x *= waterFriction;
			}
		}

		for (int i = 0; i < 100; i++)
		{
			MoveWater(i, move[i], move[i] * .1f);
		}
	}

	void OnGUI()
	{
		GUI.Label(new Rect(10, 10, 200, 20), "Water Friction");
		waterFriction = GUI.HorizontalSlider(new Rect(10, 30, 200, 20), waterFriction, 0f, 1f);
		GUI.Label(new Rect(10, 40, 200, 20), "Water Energy");
		waterEnergy = GUI.HorizontalSlider(new Rect(10, 60, 200, 20), waterEnergy, 0f, 1f);
	}

	float GetWaterHeight(int i)
	{
		return heightmap[i] + watermap[i].y;
		//return watermap[i].y <= 0 ? 0 : heightmap[i] + watermap[i].y;
	}

	void MoveWater(int i, float dir, float amount)
	{
		int next = MathUtil.Mod(i + (dir > 0 ? 1 : -1), 100);

		// amount = Mathf.Min(amount, watermap[i].y - .001f);

		watermap[i].y -= amount;
		
		if (IsWaterInTerrain(next))
			watermap[next].y = amount;
		else
			watermap[next].y += amount;
	}

	bool IsWaterInTerrain(int i)
	{
		return false;//return watermap[i].y <= 0;
	}

	void SmoothTerrain(int smoothing)
	{
		float[] clone = (float[])heightmap.Clone();
		for (int i = 0; i < heightmap.Length; i++)
		{
			float average = 0;
			for (int j = -smoothing; j <= smoothing; j++)
			{
				average += clone[MathUtil.Mod(i + j, heightmap.Length)];
			}
			average /= smoothing * 2 + 1;
			heightmap[i] = average;
		}
	}

	static void UpdateHeightmap(Vector2[] watermap, float[] heightmap, Circle circle, AnimationCurve roundCurve)
	{
		float[] height = new float[watermap.Length];
		for (int i = 0; i < height.Length; i++)
		{
			height[i] = heightmap[i] + watermap[i].y;
		}
		UpdateHeightmap(height, circle, roundCurve);
	}

	static void UpdateHeightmap(float[] heightmap, Circle circle, AnimationCurve roundCurve)
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
}