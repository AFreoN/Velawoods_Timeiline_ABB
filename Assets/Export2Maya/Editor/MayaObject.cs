using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// --------------------------------------------------
// MayaObject Class
// --------------------------------------------------
// Helper class for processing and storing Unity objects as Maya objects
public class MayaObject {
	public bool ExportShape;					// Whether this object should export its "shape" data
	public MayaObject Parent;					// The parent transform of this MayaObject
	public List<MayaObject> Children;			// List of transforms parented under this object
	
	public Transform UnityObject;				// The Unity Game Object this Maya Object refers to
	public string MayaName;						// The name that will be written to the Maya file
	
	// --------------------------------------------------
	// Constructor
	// --------------------------------------------------
	public MayaObject(){
		ExportShape = false;					// Default to only export transform.
		Children = new List<MayaObject>();		// Create a new list ready to populate with children
	}
	
	// --------------------------------------------------
	// Report true if this MayaObject is a mesh
	// --------------------------------------------------
	public bool IsMesh(){
		// --------------------------------------------------
		// Perform tests to see if this Transform is a "mesh"
		// --------------------------------------------------
		// If we are not exporting the shape, it is automatically not a mesh
		if(!ExportShape) return false;
		
		// If it doesn't have a mesh filter, it is not a mesh
		if(UnityObject.gameObject.GetComponent<MeshFilter>() == null) return false;
		
		// Just because there is a mesh filter doesn't mean there is a mesh
		// linked to it. Check that there is actually a shared mesh
		Mesh sharedMeshCheck = UnityObject.gameObject.GetComponent<MeshFilter>().sharedMesh;
		if(sharedMeshCheck == null) return false;
		
		// If we passed the tests, its a mesh!
		return true;
	}
}