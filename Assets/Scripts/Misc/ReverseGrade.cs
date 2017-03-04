using UnityEngine;
using System.Collections;

public class ReverseGrade : Fuzzy
{
	protected float x0, x1, slope;

	public ReverseGrade (string nm, float x0, float x1): base (nm)
	{
		this.x0 = x0;
		this.x1 = x1;
		this.slope = 1f / (x1 - x0);
	}

	public override float Evaluate(float value)
	{
		if (value <= x0)
			return 1f;
		if (value < x1)
			return 1 - (value - x0) * slope;
		return 0f;
	}
}