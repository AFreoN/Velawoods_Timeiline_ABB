using UnityEngine;
using System.Collections;

/*
 * Gore: Some utility methods to check that a Quaternion is returning valid results. 
 */
public static class QuaternionExtension
{
	/// <summary>
	/// Determines if a Quaternion has an element that is not a number.
	/// </summary>
	public static bool IsNAN(this Quaternion quaternion)
	{
		return float.IsNaN(quaternion.x) || float.IsNaN(quaternion.y) || float.IsNaN(quaternion.z);
	}
	
	/// <summary>
	/// Determines if a Quaternion has an element that is at either negative or positive infinity.
	/// </summary>
	public static bool IsInfinity(this Quaternion quaternion)
	{
		return float.IsInfinity(quaternion.x) || float.IsInfinity(quaternion.y) || float.IsInfinity(quaternion.z);
	}
	
	/// <summary>
	/// Determines if a Quaternion has not got any elements that are either infinity or NAN.
	/// </summary>
	public static bool IsValid(this Quaternion quaternion)
	{
		return !(quaternion.IsNAN() || quaternion.IsInfinity());
	}
}
