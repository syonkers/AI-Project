using UnityEngine;
using System.Collections;

public class Grade : Fuzzy 
{
	protected float x0, x1, slope;

	public Grade (string nm, float x0, float x1): base (nm)
	{
		this.x0 = x0;
		this.x1 = x1;
		this.slope = 1f / (x1 - x0);
	}

	public override float Evaluate(float value)
	{
		if (value <= x0)
			return 0f;
		if (value < x1)
			return (value - x0) * slope;
		return 1f;
	}
}