using UnityEngine;
using System.Collections;

// --------------------------------------------------
// MayaUtilities Class
// --------------------------------------------------
// Contains methods to convert translations and rotations between 
// coordinate systems, clean illegal characters out of Maya names, etc
public static class MayaUtilities {
	// --------------------------------------------------
	// Unity -> Maya Translation Conversion
	// --------------------------------------------------
	// Given a Vector3 translation, this will convert it to Maya translation
	public static Vector3 MayaTranslation(Vector3 t){
		return new Vector3(-t.x, t.y, t.z);
	}
	
	// --------------------------------------------------
	// Unity -> Maya Rotation Conversion
	// --------------------------------------------------
	// Given a Vector3 euler rotation, this will convert it to Maya rotation
	public static Vector3 MayaRotation(Vector3 r){
		return new Vector3(r.x, -r.y, -r.z);
	}
}
