using UnityEngine;
using System.Collections;

public interface IMeshShape
{
	Vector2[] Corners { get; set; }
	Vector2[] Center { get; set; }
	bool FillBack { get; set; }
	bool RelativeCorners { get; set; }
	Material Material { get; set; }
}