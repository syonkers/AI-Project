using UnityEngine;
using System.Collections;

public abstract class Fuzzy
{
	public string name;

	public Fuzzy(string x)
	{
		name = x;
	}

	public abstract float Evaluate (float value);

	public static float Not(float value)
	{
			return 1f - value;
	}

	public static float And (float v1, float v2) 
	{
		return Mathf.Min (v1, v2);
	}

	public static float Or (float v1, float v2) 
	{
		return Mathf.Max (v1, v2);
	}

	public static float Very (float v)
	{
		return v * v;
	}

	public static float NotVery (float v) 
	{
		return Mathf.Sqrt (v);
	}
}