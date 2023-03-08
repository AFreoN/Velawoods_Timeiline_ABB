using UnityEngine;
using System.Collections;

/*
 * Gore: Some utility methods to check that a vector is returning valid results. 
 */
public static class VectorExtension
{
	/// <summary>
	/// Determines if a vector has an element that is not a number.
	/// </summary>
	public static bool IsNAN(this Vector3 vector)
	{
		return float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z);
	}

	/// <summary>
	/// Determines if a vector has an element that is at either negative or positive infinity.
	/// </summary>
	public static bool IsInfinity(this Vector3 vector)
	{
		return float.IsInfinity(vector.x) || float.IsInfinity(vector.y) || float.IsInfinity(vector.z);
	}

	/// <summary>
	/// Determines if a vector has not got any elements that are either infinity or NAN.
	/// </summary>
	public static bool IsValid(this Vector3 vector)
	{
		return !(vector.IsNAN() || vector.IsInfinity());
	}
}
