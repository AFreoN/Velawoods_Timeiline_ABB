using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
 
 // --------------------------------------------------
 // Export2Maya ( Version 1.0.1 )
 // By:	Michael Cook
 // --------------------------------------------------
 // COMPLETED:
 //		Display Layers for Lightmap object selection
 //		Added Check for Zero Based UV's from Unity's Tree Creator
 // TO DO LIST:
 //		Maybe break out classes into separate scripts?
 //		Create MayaUtilities class that converts between coordinates Unity -> Maya
 //		Better Texture Export Technique
 //		Better Renaming method using DG paths
 //		Light Export
 //		Option to export objects as instances instead of meshes
 //		Animation Export
 //		Terrain Export
 //		Skinned Mesh Export
 //		Add check for objects that are light mapped but don't have UV2 data (plane)
 
public class Export2Maya : EditorWindow {
	// --------------------------------------------------
	// Export2Maya UI variables
	// --------------------------------------------------
	string[] MayaVersions = new string[]{ "2014", "2013.5", "2013", "2012" };
	int MayaVersionIndex = 0;
	bool ExportNormals = true;
	bool ExportUVs = true;
	bool ExportLightmapUVs = true;
	bool ExportVertexColors = true;
	bool ExportMaterials = true;
	bool ExportTextures = true;
	
	// --------------------------------------------------
	// List of selected GameObjects
	// --------------------------------------------------
	GameObject[] SelectedObjects;
	
	// --------------------------------------------------
	// Maya Name Clashing
	// --------------------------------------------------
	List<MayaName> MayaNameList;
	
	// --------------------------------------------------
	// Object Preprocessing
	// --------------------------------------------------
	List<MayaObject> FlatObjList;
	List<MayaObject> RootObjects;
	
	// --------------------------------------------------
	// File Variables
	// --------------------------------------------------
	string filePath = "";
	string fileName = "";
	float MaxProgress = 0.0f;
	float CurProgress = 0.0f;
	string ProgressTitle = "Exporting Maya Scene File:";
	string ProgressMsg = "{0} ( Writing {1} )";
	
	// --------------------------------------------------
	// Material Variables
	// --------------------------------------------------
	List<MayaMaterial> MayaMaterials;
	int GroupIDCounter = 1;					// Counter used to assign unique names to GroupIDs
	int MaterialInfoCounter = 1;			// Counter used to assign unique names to MaterialInfos
	
	// --------------------------------------------------
	// Texture Variables
	// --------------------------------------------------
	List<MayaFileTexture> MayaTextures;
	int FileTextureCounter = 1;				// Counter used to assign unique names to Texture Nodes
	
	// --------------------------------------------------
	// Display Layer Variables
	// --------------------------------------------------
	List<MayaDisplayLayer> DisplayLayers;
	
	// --------------------------------------------------
	// Maya Connections
	// --------------------------------------------------
	string MayaConnections = "";
	
	// --------------------------------------------------
	// Used to format the data in the Maya ASCII file into columns
	// --------------------------------------------------
	int ColumnDataWidth = 5;
	
	// --------------------------------------------------
	// Debugger UI Variables
	// --------------------------------------------------
	int DebugVert = 0;
	int DebugUV = 0;
	int DebugUV2 = 0;
	int DebugColors = 0;
	int DebugNormals = 0;
	int DebugTris = 0;
	
	#region Memory Cleanup
	// --------------------------------------------------
	// Clear Variables - Free Memory
	// --------------------------------------------------
	// Clears out all the variables we have to
	// free up any memory they were using
	void ResetVariables(){
		// Reset selected objects
		SelectedObjects = new GameObject[0];
		
		// Reset lists
		MayaNameList = new List<MayaName>();
		FlatObjList = new List<MayaObject>();
		RootObjects = new List<MayaObject>();
		
		MayaMaterials = new List<MayaMaterial>();
		GroupIDCounter = 1;
		MaterialInfoCounter = 1;
		
		MayaTextures = new List<MayaFileTexture>();
		FileTextureCounter = 1;
		
		DisplayLayers = new List<MayaDisplayLayer>();
		
		MayaConnections = "";
	
		MaxProgress = 0.0f;
		CurProgress = 0.0f;
	}
	#endregion
	
	// --------------------------------------------------
	// Export2Maya - UI
	// --------------------------------------------------
	[MenuItem ("Window/Export2Maya")]
    static void Init(){
        // Get existing open window, if none then make a new one
        Export2Maya window = (Export2Maya)EditorWindow.GetWindow(typeof(Export2Maya), false, "Export2Maya");
		window.position = new Rect((Screen.width / 2) - 150, (Screen.height) / 2 + 150, 250, 230);
        window.Show();
    }
	
	void OnGUI(){
		GUILayout.Label("Maya Version:", EditorStyles.boldLabel);
			MayaVersionIndex = EditorGUILayout.Popup(MayaVersionIndex, MayaVersions, GUILayout.MaxWidth(100));
		GUILayout.Label("Mesh Settings:", EditorStyles.boldLabel);
			ExportNormals = EditorGUILayout.ToggleLeft(" Export Normals", ExportNormals);
			ExportUVs = EditorGUILayout.ToggleLeft(" Export UVs", ExportUVs);
			ExportLightmapUVs = EditorGUILayout.ToggleLeft(" Export Lightmap UVs", ExportLightmapUVs);
			ExportVertexColors = EditorGUILayout.ToggleLeft(" Export Vertex Colors", ExportVertexColors);
		GUILayout.Label("Material Settings:", EditorStyles.boldLabel);
			ExportMaterials = EditorGUILayout.ToggleLeft(" Export Materials", ExportMaterials);
			GUI.enabled = ExportMaterials;
			ExportTextures = EditorGUILayout.ToggleLeft(" Export Textures", ExportTextures);
			if(!ExportMaterials) ExportTextures = false;
			GUI.enabled = true;
		// Begin export
		if(GUILayout.Button("Export Selection", GUILayout.Height(22))) MayaExporter();
		GUILayout.Label("Mesh Debugger:", EditorStyles.boldLabel);
			GUILayout.Label("Verts: " + DebugVert, EditorStyles.boldLabel);
			GUILayout.Label("UV: " + DebugUV, EditorStyles.boldLabel);
			GUILayout.Label("UV2: " + DebugUV2, EditorStyles.boldLabel);
			GUILayout.Label("Colors: " + DebugColors, EditorStyles.boldLabel);
			GUILayout.Label("Normals: " + DebugNormals, EditorStyles.boldLabel);
			GUILayout.Label("Tris: " + DebugTris, EditorStyles.boldLabel);
    }
	
	void OnSelectionChange(){
		bool isMesh = false;
		if(Selection.gameObjects.Length > 0){
			// If it doesn't have a mesh filter, it is not a mesh
			if(Selection.gameObjects[0].GetComponent<MeshFilter>() != null){
				// Just because there is a mesh filter doesn't mean there is a mesh
				// linked to it. Check that there is actually a shared mesh
				Mesh sharedMeshCheck = Selection.gameObjects[0].GetComponent<MeshFilter>().sharedMesh;
				if(sharedMeshCheck != null){
					isMesh = true;
				}
			}
		}
		
		if(!isMesh){
			DebugVert = 0;
			DebugUV = 0;
			DebugUV2 = 0;
			DebugColors = 0;
			DebugNormals = 0;
			DebugTris = 0;
		}
		else{
			// Get the mesh data
			Mesh m = Selection.gameObjects[0].transform.GetComponent<MeshFilter>().sharedMesh;

			DebugVert = m.vertices.Length;
			DebugUV = m.uv.Length;
			DebugUV2 = m.uv2.Length;
			DebugColors = m.colors.Length;
			DebugNormals = m.normals.Length;
			DebugTris = m.triangles.Length;
		}
		
		Repaint();
	}
	
	#region The Main Entry point
	void MayaExporter(){	
		// --------------------------------------------------
		// Reset variables
		// --------------------------------------------------
		ResetVariables();
		
		// --------------------------------------------------
		// Grab the selected objects
		// --------------------------------------------------
		SelectedObjects = Selection.gameObjects;
		 
		// --------------------------------------------------
		// Selected Objects Check
		// --------------------------------------------------
		// Check selection before doing anything
		if(SelectedObjects.Length < 1){
		//	Debug.LogWarning("Nothing selected to export!");
			EditorUtility.DisplayDialog("Nothing to Export!", "Please select the GameObjects you wish to export and try again.", "Ok");
			return;
		}
		
		// --------------------------------------------------
		// File Save Prompt
		// --------------------------------------------------
		// Prompt the user where to save the Maya file
		fileName = EditorUtility.SaveFilePanel("Export Maya Scene File", "", "", "ma");
		if(fileName == "") return;	// If they cancel then abort

		
		// Split out file name and file path
		string[] tokens = fileName.Split('/');
		filePath = "";
		for(int i=0; i<tokens.Length - 1; i++) filePath += tokens[i] + "/";
		fileName = tokens[tokens.Length - 1];
		
		// --------------------------------------------------
		// Start new file
		// --------------------------------------------------
		// Begin new Maya Scene file, add Maya Version info and
		// default cameras
		//
		// Note - If we are overwriting an existing file, any existing
		// data in the file will be erased
		StartNewFile();
		
		// --------------------------------------------------
		// Process selected GameObjects
		// --------------------------------------------------
		// Go through selection and create MayaObjects of all
		// children and parent objects
		//
		// Note - Any child objects will automatically have their
		// shape export set to TRUE. Any parents of the selected
		// objects will only have their transforms exported
		EditorUtility.DisplayProgressBar(ProgressTitle, "Converting Selection to Maya Objects", 0);
		SelectionToMayaObjects();
		
		// --------------------------------------------------
		// Connect MayaObjects
		// --------------------------------------------------
		// Go through selection again, this time populating the
		// MayaObjects connection data ( parents and children )
		EditorUtility.DisplayProgressBar(ProgressTitle, "Connecting Maya Objects", 0);
		ConnectMayaObjects();
		
		// --------------------------------------------------
		// Build root object list
		// --------------------------------------------------
		// Build a list of root only objects. Since we have the
		// parents and children connected together, we will start
		// at the root of each chain of objects and work our way down
		// recursively
		EditorUtility.DisplayProgressBar(ProgressTitle, "Building Root Object List", 0);
		for(int i=0; i<FlatObjList.Count; i++){
			// If there is no parent, it is a root object
			if(FlatObjList[i].Parent == null){
				RootObjects.Add(FlatObjList[i]);
			}
		}
		
		// --------------------------------------------------
		// Rename MayaObjects
		// --------------------------------------------------
		// Recursively go through root objects and children and
		// assign a unique Maya name to avoid name clashes
		EditorUtility.DisplayProgressBar(ProgressTitle, "Renaming Maya Objects", 0);
		for(int i=0; i<RootObjects.Count; i++){
			AssignMayaName(RootObjects[i]);
		}
		
		// --------------------------------------------------
		// Build Material List
		// --------------------------------------------------
		// Go through the FlatObjList, and find which materials
		// are assigned. If we haven't registered the material name
		// then do so
		if(ExportMaterials){
			EditorUtility.DisplayProgressBar(ProgressTitle, "Building Material List", 0);
			BuildMaterialList();
		}
		
		// --------------------------------------------------
		// Build Texture List
		// --------------------------------------------------
		// Go through the materials list and find which textures
		// are being used, and build a texture list from it
		if(ExportTextures){
			EditorUtility.DisplayProgressBar(ProgressTitle, "Building Texture List", 0);
			BuildTextureList();
		}
		
		// --------------------------------------------------
		// Build DisplayLayer List
		// --------------------------------------------------
		// Go through the FlatObjList, and check if the object
		// is set to lightmap static. If it is, create the DisplayLayer 
		// for the lightmap index and add the MayaObject to it
		EditorUtility.DisplayProgressBar(ProgressTitle, "Building Display Layer List", 0);
		for(int i=0; i<FlatObjList.Count; i++){
			// Check if the object has a MeshRenderer
			MeshRenderer MeshRender = FlatObjList[i].UnityObject.gameObject.GetComponent<MeshRenderer>();
			if(MeshRender != null){
				// Check if the mesh renderer lightmap index is set
				//		LightmapIndex -1 = Not Lightmap Static
				// 		LightmapIndex 255 = No Lightmap at all
				// 		LightmapIndex 254 = No Lightmap, but calculate GI
				// 		LightmapIndex 253 and Lower = LightmapIndex
				if(MeshRender.lightmapIndex < 254 && MeshRender.lightmapIndex > -1){
					// Find the DisplayLayer who's lightmap index matches
					int DisplayLayerIndex = -1;
					for(int d=0; d<DisplayLayers.Count; d++){
						if(DisplayLayers[d].LightmapIndex == MeshRender.lightmapIndex){
							DisplayLayerIndex = d;
							break;
						}
					}
					
					// If the DisplayLayer was not found, create it
					if(DisplayLayerIndex == -1){
						DisplayLayers.Add(new MayaDisplayLayer(MeshRender.lightmapIndex));
						
						// Update index to latest index
						DisplayLayerIndex = DisplayLayers.Count - 1;
					}
					
					// Add MayaObject to display layer MayaObject list
					DisplayLayers[DisplayLayerIndex].Objects.Add(FlatObjList[i]);
				}
			}
		}
		// Sort the DisplayLayers based on LightmapIndex property
		DisplayLayers = DisplayLayers.OrderBy(o=>o.LightmapIndex).ToList();
		
		// --------------------------------------------------
		// Process Bar initialization
		// --------------------------------------------------
		MaxProgress = FlatObjList.Count;
		
		// --------------------------------------------------
		// Write MayaObjects to File
		// --------------------------------------------------
		// Now that we have all the objects converted to MayaObjects
		// and sorted correctly, begin writing each object to disk
		for(int i=0; i<RootObjects.Count; i++){
			ProcessTransform(RootObjects[i]);
		}
		
		// --------------------------------------------------
		// Write Display Layer Manage, and Display Layers
		// --------------------------------------------------
		DisplayLayerExport();
		
		// --------------------------------------------------
		// Write lightLinker, renderPartition, and defaultShaderList
		// --------------------------------------------------
		if(ExportMaterials){
			StartMaterialExport();
		}
		
		// --------------------------------------------------
		// Write MayaMaterials
		// --------------------------------------------------
		if(ExportMaterials){
			for(int i=0; i<MayaMaterials.Count; i++){
				string Mat = MayaMaterials[i].GetMel();
				// Write out the material
				AppendToFile(Mat);
			}
		}
		
		// --------------------------------------------------
		// Write defaultRenderUtilityList size
		// --------------------------------------------------
		if(ExportTextures){
			StartTextureExport();
		}
		
		// --------------------------------------------------
		// Write MayaTextures
		// --------------------------------------------------
		if(ExportTextures){
			for(int i=0; i<MayaTextures.Count; i++){
				string Tex = MayaTextures[i].GetMel();
				// Write out the MayaTexture
				AppendToFile(Tex);
			}
		}

		// --------------------------------------------------
		// Build Material Connections
		// --------------------------------------------------
		// We will be going through each material, and writing the
		// connections between material > textures. We will also be
		// writing connections between texture > texturePlacement nodes
		if(ExportMaterials){
			for(int i=0; i<MayaMaterials.Count; i++){
				// Check for MainTex
				if(MayaMaterials[i].MainTex != null){
					MayaConnections += "connectAttr \"" + MayaMaterials[i].MainTex.MayaFileName + ".msg\" \"" + MayaMaterials[i].MaterialInfo + ".t\" -na;\n";
					MayaConnections += "connectAttr \"" + MayaMaterials[i].MainTex.MayaFileName + ".oc\" \"" + MayaMaterials[i].MayaName + ".c\";\n";
				//	MayaConnections += "connectAttr \"" + MayaMaterials[i].MainTex.MayaFileName + ".ot\" \"" + MayaMaterials[i].MayaName + ".it\";\n";
				//	MayaConnections += "connectAttr \"" + MayaMaterials[i].MainTex.MayaFileName + ".oc\" \"" + MayaMaterials[i].MayaName + ".ambc\";\n";
				//	MayaConnections += "connectAttr \"" + MayaMaterials[i].MainTex.MayaFileName + ".oc\" \"" + MayaMaterials[i].MayaName + ".ic\";\n";
				//	MayaConnections += "connectAttr \"" + MayaMaterials[i].MainTex.MayaFileName + ".oc\" \"" + MayaMaterials[i].MayaName + ".sc\";\n";
				}
			}
		}
		
		// --------------------------------------------------
		// Write Maya Connections
		// --------------------------------------------------
		// Write out all the MayaConnections last.
		AppendToFile(MayaConnections);
		
		// --------------------------------------------------
		// Copy Textures
		// --------------------------------------------------
		// Go through our texture list and copy the textures from
		// our Unity project to the destination path
		if(ExportTextures){
			for(int i=0; i<MayaTextures.Count; i++){
				// If the file doesn't already exist in the destination path
				if(!System.IO.File.Exists(MayaTextures[i].DestinationPath)){
					FileUtil.CopyFileOrDirectory(MayaTextures[i].SourcePath, MayaTextures[i].DestinationPath);
				}
			}
		}

		// --------------------------------------------------
		// Clear Progress Bar
		// --------------------------------------------------
		EditorUtility.ClearProgressBar();

		// --------------------------------------------------
		// Clear the variable data
		// --------------------------------------------------
		ResetVariables();
	}
	#endregion
	
	#region MayaObject Creation
	// This will go through the SelectedObjects list and create MayaObject
	// versions of them
	void SelectionToMayaObjects(){
		// Go through selected objects
		for(int i=0; i<SelectedObjects.Length; i++){
			// Recursively find all children of selected objects
			//
			// Note - We will automatically set ExportShape to TRUE
			// for children
			ProcessChildren(SelectedObjects[i].transform);
			
			// Recursively find all parents of selected objects
			// Note - We leave ExportShape to FALSE because we only need
			// the parent transforms
			ProcessParents(SelectedObjects[i].transform);
		}
	}
	
	// Recursively process all child transforms of given transform
	void ProcessChildren(Transform t){
		// Add this GameObject to our FlatObjList
		CreateMayaObject(t, true);
		
		// Find any children of this transform and process them
		foreach(Transform child in t){
			ProcessChildren(child);
		}
	}
	
	// Recursively process all parent transforms of given transform
	void ProcessParents(Transform t){
		// Add this GameObject to our FlatObjList
		CreateMayaObject(t, false);
		
		// If transform has a parent, process it
		if(t.parent != null){
			ProcessParents(t.parent);
		}
	}
	
	// --------------------------------------------------
	// Create MayaObject
	// --------------------------------------------------
	// Given a transform, this will create a MayaObject and link
	// the transform to it ( so we know which GameObject this 
	// MayaObject references )
	//
	// Note - The boolean "ExportShape" lets us know if we want to export
	// the GameObject "shape" data or just the transform. We export "transforms only"
	// for parents of selected GameObjects, since we need them to accurately
	// place the MayaObjects in the Maya scene, but don't want their shape data
	void CreateMayaObject(Transform t, bool ExportShape){
		bool ObjFound = false;
		
		// First check if the object already exists in our FlatObjList
		for(int i=0; i<FlatObjList.Count; i++){
			// OBJECT FOUND! It already exists
			if(FlatObjList[i].UnityObject == t){
				ObjFound = true;
				// We need this here, in case we already had the object but ExportShape
				// was false, this will set it to TRUE since the code below would not be
				// called
				if(ExportShape) FlatObjList[i].ExportShape = true;
				break;
			}
		}
		
		// If we did not find the MayaObject
		if(!ObjFound){
			// Create a new MayaObject
			MayaObject MayaObj = new MayaObject();
			
			// Fill out the initial data
			//
			// We need the Unity GameObject, so we know which object this MayaObject
			// references. We also need to know if we need to export the shape or just the
			// transform for this object
			MayaObj.ExportShape = ExportShape;
			MayaObj.UnityObject = t;

			// Add this object to our flat object list
			FlatObjList.Add(MayaObj);
		}
	}
	#endregion

	#region MayaObject Connections
	// This will go through the SelectedObjects list and the FlatObjList
	// and fill out the connection data between them (parents and children)
	void ConnectMayaObjects(){
		// Go through selected objects
		for(int i=0; i<SelectedObjects.Length; i++){
			// Recursively connect children of MayaObject
			ConnectMayaObjectChildren(Selection.gameObjects[i].transform);
			
			// Recursively connect parents of MayaObject
			ConnectMayaObjectParents(Selection.gameObjects[i].transform);
		}
	}
	
	// Recursively connect child MayaObjects to given MayaObject.
	// Also connect the parents of each object
	void ConnectMayaObjectChildren(Transform t){
		// Get this transform MayaObject index
		int MayaObjIndex = GetMayaObjectFromTransform(t);
		
		// Get the child count of the transform
		int ChildCount = t.childCount;
		
		// Go through each child and find the MayaObject in the FlatObjList
		// and add the MayaObjects as children of this MayaObject
		//
		// Note - We are only getting the immediate children
		for(int i=0; i<ChildCount; i++){
			// Get child index
			int ChildMayaObjIndex = GetMayaObjectFromTransform(t.GetChild(i));
			
			// Add Child MayaObject to this MayaObject if it does not 
			// already exist
			bool ChildFound = false;
			for(int c=0; c<FlatObjList[MayaObjIndex].Children.Count; c++){
				if(FlatObjList[MayaObjIndex].Children[c].UnityObject == FlatObjList[ChildMayaObjIndex].UnityObject){
					ChildFound = true;
					break;
				}
			}
			// If we didn't find it, then add it
			if(!ChildFound){
				FlatObjList[MayaObjIndex].Children.Add(FlatObjList[ChildMayaObjIndex]);
			}
		}
		
		// Get the parent MayaObject index of this transform
		if(t.parent != null){
			int ParentIndex = GetMayaObjectFromTransform(t.parent);
			// Add the parent MayaObject to this MayaObject
			FlatObjList[MayaObjIndex].Parent = FlatObjList[ParentIndex];
		}
		
		// Recursively process the children now
		foreach(Transform child in t){
			ConnectMayaObjectChildren(child);
		}
	}
	
	// Recursively connect parent MayaObject to given MayaObject
	void ConnectMayaObjectParents(Transform t){
		// If this object has a parent
		if(t.parent != null){
			// Get this transform MayaObject index
			int MayaObjIndex = GetMayaObjectFromTransform(t);
			
			// Get parent transform MayaObject index
			int ParentMayaObjIndex = GetMayaObjectFromTransform(t.parent);
			
			// Add the parent MayaObject to this MayaObject
			FlatObjList[MayaObjIndex].Parent = FlatObjList[ParentMayaObjIndex];
			
			// Add this MayaObject as child to parent, if it doesn't exist already
			bool AlreadyExists = false;
			for(int i=0; i<FlatObjList[ParentMayaObjIndex].Children.Count; i++){
				if(FlatObjList[ParentMayaObjIndex].Children[i].UnityObject == t){
					AlreadyExists = true;
					break;
				}
			}
			if(!AlreadyExists){
				FlatObjList[ParentMayaObjIndex].Children.Add(FlatObjList[MayaObjIndex]);
			}
		}
		
		// Recursively process the parents now
		if(t.parent != null){
			ConnectMayaObjectParents(t.parent);
		}
	}
	
	// Given a transform, this will return an index into the FlatObjList
	// of the corresponding MayaObject
	// 
	// Note - It will return -1 if it couldn't find it
	int GetMayaObjectFromTransform(Transform t){
		int Index = -1;
		for(int i=0; i<FlatObjList.Count; i++){
			// If we found the MayaObject
			if(FlatObjList[i].UnityObject == t){
				Index = i;
				break;
			}
		}
		return Index;
	}
	#endregion
	
	#region Rename MayaObjects
	// --------------------------------------------------
	// Rename MayaObjects
	// --------------------------------------------------
	// Given a linked list of MayaObjects, this will go though and
	// make sure all names are unique by registering each name into the MayaName
	// list. If a name clash occurs, then the name will be prefixed with a number
	void AssignMayaName(MayaObject m){
		// --------------------------------------------------
		// Clean the object name
		// --------------------------------------------------
		string CleanedName = CleanName(m.UnityObject.name);
				
		// --------------------------------------------------
		// Check for name clashes
		// --------------------------------------------------
		m.MayaName = RegisterName(CleanedName);
		
		// If there are children of this MayaObject then
		// recursively assign their names as well
		for(int c=0; c<m.Children.Count; c++){
			AssignMayaName(m.Children[c]);
		}
	}
		
	// --------------------------------------------------
	// Clean Name
	// --------------------------------------------------
	// Given a string, this will remove illegal characters so
	// it fits within Maya naming conventions
	string CleanName(string name){
		// We have to strip out any illegal characters from the name
		// a-z A-Z 0-9 and _ are the only accepted characters
		List<string> CleanedName = new List<string>();
		
		// Convert the name into an array of char
		char[] array = name.ToCharArray();

		// We will be moving backwards through the name, removing numbers
		// and underscores. Once we find a letter, we stop removing numbers
		bool removeNumbers = true;
		for(int i=(array.Length - 1); i>-1; i--){
			if(char.IsLetter(array[i]) && array[i] != '_') removeNumbers = false;
			if(removeNumbers){
				if(char.IsUpper(array[i]) || char.IsLower(array[i])) CleanedName.Add(array[i].ToString());
			}
			else{
				if(char.IsLetterOrDigit(array[i]) || array[i] == '_') CleanedName.Add(array[i].ToString());
			}
		}
		// Since we went backwards through the name, it will be reversed. We will have
		// to reverse the result to make it correct
		CleanedName.Reverse();
		
		// Convert the char array into a string again
		return string.Join("", CleanedName.ToArray());	
	}
	
	// --------------------------------------------------
	// Register Name
	// --------------------------------------------------
	// Given a string name, this will search the MayaName List
	// and see if it was registered already. If it was, it 
	// increments the counter for that name. If not then it
	// registers the name
	string RegisterName(string name){
		// Check the name against our list of names
		bool NameFound = false;
		for(int i=0; i<MayaNameList.Count; i++){
			// If the object name was already registered
			if(name == MayaNameList[i].Name){
				NameFound = true;
				MayaNameList[i].Count++;
				
				// Set the name to the incremented name
				name = MayaNameList[i].GetName();
				break;
			}
		}
		// If we did not find the name, register it
		if(!NameFound){
			MayaName mn = new MayaName(name);
			MayaNameList.Add(mn);
		}
		
		// Return the name
		return name;
	}
	#endregion
	
	#region Transforms
	// --------------------------------------------------
	// Process Transform
	// --------------------------------------------------
	// Given a MayaObject, this will query all the transform data and record it
	void ProcessTransform(MayaObject MayaObj){
		string data = "";
		
		// Update progress bar
		EditorUtility.DisplayProgressBar(ProgressTitle, string.Format(ProgressMsg, MayaObj.MayaName, "Transform"), CurProgress/MaxProgress);
			
		// ------------------------------
		// First process the transform
		// ------------------------------
		// Get the local translation as Maya translation
		Vector3 translate = MayaUtilities.MayaTranslation(MayaObj.UnityObject.localPosition);
//		translate.x *= -1; // Fix for Maya coordinate system
		
		// Get the local rotation as Maya rotation
		Vector3 rotate = MayaUtilities.MayaRotation(MayaObj.UnityObject.localRotation.eulerAngles);
		
		// Get the local scale
		Vector3 scale = MayaObj.UnityObject.localScale;
		
		// If transform has no parent
		if(MayaObj.Parent == null) data += "createNode transform -n \"" + MayaObj.MayaName + "\";\n";
		else data += "createNode transform -n \"" + MayaObj.MayaName + "\" -p \"" + MayaObj.Parent.MayaName + "\";\n";
		
		// Add transformation data
		data += "\tsetAttr \".t\" -type \"double3\"" + translate.x + " " + translate.y + " " + translate.z + ";\n";
		data += "\tsetAttr \".r\" -type \"double3\"" + rotate.x + " " + rotate.y + " " + rotate.z + ";\n";
		data += "\tsetAttr \".s\" -type \"double3\"" + scale.x + " " + scale.y + " " + scale.z + ";\n";
		
		// Set rotation order to ZXY instead of XYZ
		data += "\tsetAttr \".ro\" 2;\n";
		
		// Write Transform data
		AppendToFile(data);
		data = "";
		
		// --------------------------------------------------
		// Process Shape?
		// --------------------------------------------------
		// If the shape is set to process, handle it
		if(MayaObj.IsMesh()) ProcessMesh(MayaObj);
		
		// --------------------------------------------------
		// Recursive Child Search
		// --------------------------------------------------
		foreach(MayaObject child in MayaObj.Children){
			ProcessTransform(child);
		}
		
		// --------------------------------------------------
		// Progress Bar update
		// --------------------------------------------------
		CurProgress += 1;
	}
	#endregion

	#region Meshes
	// --------------------------------------------------
	// Process Mesh
	// --------------------------------------------------
	// Given a MayaObject, this will query all the mesh 
	// data, format it to Maya conventions, and write it
	// to the file
	void ProcessMesh(MayaObject MayaObj){
		string data = "";
		
		// Get a reference to the mesh of the MayaObject
		Mesh m = MayaObj.UnityObject.gameObject.GetComponent<MeshFilter>().sharedMesh;
			
		// Update progress bar
		EditorUtility.DisplayProgressBar(ProgressTitle, string.Format(ProgressMsg, MayaObj.MayaName, "Getting Mesh Data"), CurProgress/MaxProgress);
		
		// --------------------------------------------------
		// Gather mesh data
		// --------------------------------------------------
		Vector3[] verts = m.vertices;
		Vector2[] uvs = m.uv;
		Vector2[] uvs2 = m.uv2;
		Color[] colors = m.colors;
		Vector3[] normals = m.normals;
		int[] tris = m.triangles;
		
		// --------------------------------------------------
		// Get the Lightmap tiling and offset if Lightmap UVs exist
		// --------------------------------------------------
		Vector4 tilingOffset = new Vector4();
		if(uvs2.Length > 0) tilingOffset = MayaObj.UnityObject.gameObject.GetComponent<Renderer>().lightmapScaleOffset;
		
		// --------------------------------------------------
		// Perform UV and UV2 Zero Value checks here. With the
		// Tree Creator, it makes lightmap UVs all with Zero values
		// which can mess up our calculations when applying the tiling and
		// offset
		//
		// So check and see if all the UVs have zero values, if so
		// then disable UV or UV2 export
		// --------------------------------------------------
		bool ZeroUV = true;
		bool ZeroUV2 = true;
		for(int i=0; i<uvs.Length; i++){
			if(uvs[i].x != 0){
				ZeroUV = false;
				break;
			}
			if(uvs[i].y != 0){
				ZeroUV = false;
				break;
			}
		}
		for(int i=0; i<uvs2.Length; i++){
			if(uvs2[i].x != 0){
				ZeroUV2 = false;
				break;
			}
			if(uvs2[i].y != 0){
				ZeroUV2 = false;
				break;
			}
		}

		// --------------------------------------------------
		// Write shape attributes
		// --------------------------------------------------
		data += "createNode mesh -n \"" + MayaObj.MayaName + "Shape\" -p \"" + MayaObj.MayaName + "\";\n";
		data += "\tsetAttr -k off \".v\";\n";
		data += "\tsetAttr \".vir\" yes;\n";
		data += "\tsetAttr \".vif\" yes;\n";
		
		// Write Shape data
		AppendToFile(data);
		data = "";
		
		// --------------------------------------------------
		// If the user has chosen to export materials
		// --------------------------------------------------
		if(ExportMaterials){
			// --------------------------------------------------
			// Sub Mesh - Per face material assignment
			// --------------------------------------------------
			// If the sub mesh count is greater than 1, we have
			// per face material assignment. If this is the case, write
			// out the per face assignments
			int SubMeshCount = m.subMeshCount;
			if(SubMeshCount > 1){
				// Set the instObjGroup size to the number of sub meshes
				data += "\tsetAttr -s " + SubMeshCount + " \".iog[0].og\";\n";
				
				// Go through each sub mesh and add its face assignments
				int TotalSubMeshTris = 0;
				for(int i=0; i<SubMeshCount; i++){
					// Get the sub mesh triangle count
					int[] SubTriangles = m.GetTriangles(i);
					data += "\tsetAttr \".iog[0].og[" + i + "].gcl\" -type \"componentList\" 1 \"f[" + ((SubTriangles.Length / 3) > 1 ? (TotalSubMeshTris.ToString() + ":") : "") + (((SubTriangles.Length / 3) - 1) + TotalSubMeshTris) + "]\";\n";
					
					// Increment TotalSubMeshTris
					TotalSubMeshTris += SubTriangles.Length / 3;
				}		
			}
			
			// --------------------------------------------------
			// Material Assignment
			// --------------------------------------------------
			// Get material(s) list for the mesh
			Material[] mats = MayaObj.UnityObject.gameObject.GetComponent<Renderer>().sharedMaterials;

			// If the sub mesh count is greater than 1, then we need to handle
			// per-face assignment
			if(SubMeshCount > 1){
				// Go through sub meshes
				for(int i=0; i<SubMeshCount; i++){
					// Find the material in our MayaMaterials list
					for(int j=0; j<MayaMaterials.Count; j++){
						// If we found the material
						if(MayaMaterials[j].UnityMaterial == mats[i]){
							// Increment GroupID counter
							GroupIDCounter++;
							
							// Create GroupID node
							MayaConnections += "createNode groupId -n \"groupId" + GroupIDCounter + "\";\n";
							MayaConnections +=		"\tsetAttr \".ihi\" 0;\n";		// Is historically interesting
							
							// Connect groupID.id > mesh.instObjGroups
							MayaConnections += "connectAttr \"groupId" + GroupIDCounter + ".id\" \"" + MayaObj.MayaName + "Shape.iog.og[" + i + "].gid\";\n";

							// Connect groupID.message > SG.groupNodes
							MayaConnections += "connectAttr \"groupId" + GroupIDCounter + ".msg\" \"" + MayaMaterials[j].MayaName + "SG.gn\" -na;\n";
							
							// Connect SG.memberWireframeColor > mesh.instObjGroups
							MayaConnections += "connectAttr \"" + MayaMaterials[j].MayaName + "SG.mwc\" \"" + MayaObj.MayaName + "Shape.iog.og[" + i + "].gco\";\n";
							
							// Connect mesh.instObjGroups > SG.dagSetMembers
							MayaConnections += "connectAttr \"" + MayaObj.MayaName + "Shape.iog.og[" + i + "]\" \"" + MayaMaterials[j].MayaName + "SG.dsm\" -na;\n";
							
							// Since we found the material, break out of the loop
							break;
						}			
					}		
				}
			}
			// If the sub mesh count is 1, then we handle
			// per-object assignment
			else{
				// Find the material in our MayaMaterials list
				for(int mat=0; mat<MayaMaterials.Count; mat++){
					// If we found the material
					if(MayaMaterials[mat].UnityMaterial == mats[0]){
						MayaConnections += "connectAttr \"" + MayaObj.MayaName + "Shape.iog\" \"" + MayaMaterials[mat].MayaName + "SG.dsm\" -na;\n";
						
						// Since we found the material, break out of the loop
						break;
					}
				}
			}
		}
		// --------------------------------------------------
		// If the user has chosen to not export materials
		// --------------------------------------------------
		else{
			// Just assign the default lambert1 material to the object
			MayaConnections += "connectAttr \"" + MayaObj.MayaName + "Shape.iog\" \":initialShadingGroup.dsm\" -na;\n";
		}
		
		// --------------------------------------------------
		// Build UV, UV2, Color and Vertex lists
		// --------------------------------------------------
		// Note - From what I've seen, the vertex list, the UV list, the UV2 list and the colors list are
		// always the same size. So as an optimization lets just pick the vertex list as our iterator and
		// fill out all the data in 1 go to save time
		//
		// The only exception is the normals since Maya stores normals per-vertex per-face, where as the
		// other lists get referenced by the face definitions
		string uvData = "";
		string uv2Data = "";
		string colorData = "";
		string vertData = "";
		
		// Update progress bar
		EditorUtility.DisplayProgressBar(ProgressTitle, string.Format(ProgressMsg, MayaObj.MayaName, "UVs, Colors, Vertices"), CurProgress/MaxProgress);
		
		// Column counter used for nicely formatting the data in the Maya file
		// NOTE - We also use it below for the edge list as well
		int ColumnCounter = 0;

		for(int i=0; i<verts.Length; i++){
			// Format UV data
			if(uvs.Length > 0){
				if(ColumnCounter == 0) uvData += "\t\t";
				uvData += uvs[i].x + " " + uvs[i].y + " ";
				if(ColumnCounter == ColumnDataWidth) uvData += "\n";
			}
			
			// Format UV2 data
			if(uvs2.Length > 0){
				if(ColumnCounter == 0) uv2Data += "\t\t";
				uv2Data += ((uvs2[i].x * tilingOffset.x) + tilingOffset.z) + " " + ((uvs2[i].y * tilingOffset.y) + tilingOffset.w) + " ";
				if(ColumnCounter == ColumnDataWidth) uv2Data += "\n";
			}
			
			// Format color data
			if(colors.Length > 0){
				if(ColumnCounter == 0) colorData += "\t\t";
				colorData += colors[i].r + " " + colors[i].g + " " + colors[i].b + " " + colors[i].a + " ";
				if(ColumnCounter == ColumnDataWidth) colorData += "\n";
			}
	
			// Format vertex data
			if(ColumnCounter == 0) vertData += "\t\t";
			Vector3 MayaVert = MayaUtilities.MayaTranslation(verts[i]);
			vertData += (MayaVert.x + " " + MayaVert.y + " " + MayaVert.z + " ");
			if(ColumnCounter == ColumnDataWidth) vertData += "\n";
			
			// Increment column counter
			ColumnCounter++;
			if(ColumnCounter > ColumnDataWidth) ColumnCounter = 0;
		}

		// --------------------------------------------------
		// Write UV data
		// --------------------------------------------------
		if(ExportUVs && !ZeroUV){
			if(uvs.Length > 0){
				data += "\tsetAttr \".uvst[0].uvsn\" -type \"string\" \"map1\";\n";
				data += "\tsetAttr -s " + verts.Length + " \".uvst[0].uvsp[" + (verts.Length > 1 ? "0:" : "") + (verts.Length - 1) + "]\" -type \"float2\" \n" + uvData + ";\n";
				data += "\tsetAttr  \".cuvs\" -type \"string\" \"map1\";\n";	// Set the current uv set to the main uv set
			
				// Write UV data
				AppendToFile(data);
				data = "";	
			}
		}
	
		// --------------------------------------------------
		// Write UV2 data
		// --------------------------------------------------
		if(ExportLightmapUVs && !ZeroUV2){
			if(uvs2.Length > 0){
				data += "\tsetAttr \".uvst[1].uvsn\" -type \"string\" \"lightmap\";\n";
				data += "\tsetAttr -s " + verts.Length + " \".uvst[1].uvsp[" + (verts.Length > 1 ? "0:" : "") + (verts.Length - 1) + "]\" -type \"float2\" \n" + uv2Data + ";\n";
				
				// Write UV2 data
				AppendToFile(data);
				data = "";
			}
		}
			
		// --------------------------------------------------
		// Write Color data ( RGBA )
		// --------------------------------------------------
		if(ExportVertexColors){
			if(colors.Length > 0){
				// Display vertex colors on file load? ON
				data += "\tsetAttr \".dcol\" yes;\n";
				// Set which color channel to display (Ambient + Diffuse)
				data += "\tsetAttr \".dcc\" -type \"string\" \"Ambient+Diffuse\";\n";
				// Set the current color set
				data += "\tsetAttr \".ccls\" -type \"string\" \"colorSet1\";\n";
				data += "\tsetAttr \".clst[0].clsn\" -type \"string\" \"colorSet1\";\n";
				
				data += "\tsetAttr -s " + verts.Length + " \".clst[0].clsp[" + (verts.Length > 1 ? "0:" : "") + (verts.Length - 1) + "]\" " + colorData + ";\n";
				
				// Write Color data
				AppendToFile(data);
				data = "";
			}
		}

		// Write Vertex data
		data += "\tsetAttr -s " + verts.Length + " \".vt[" + (verts.Length > 1 ? "0:" : "") + (verts.Length - 1) + "]\" \n" + vertData + ";\n";
		AppendToFile(data);
		data = "";
		
		// --------------------------------------------------
		// Edge Connections
		// --------------------------------------------------
		// Since Unity stores all polygons as triangles, this will be easy.
		// We will have 3 possible edge connections per face:
		// edgeA:(vert0->vert1) edgeB:(vert1->vert2) edgeC:(vert2->vert0)
		// Note - We have to check that the edge doesn't already exist in our
		// local list before storing the edge. No duplicates allowed!
		List<MayaEdge> EdgeList = new List<MayaEdge>();		// Local edge list
		bool EdgeExists = false;
		
		// Edge Index list:
		// We will fill this guy out as we go so its ready when writing the
		// polygon face data. This will be a list of indices that point to our
		// local edge list which describe the edges of a face
		List<int> EdgeIndexList = new List<int>();
		
		// Update progress bar
		EditorUtility.DisplayProgressBar(ProgressTitle, string.Format(ProgressMsg, MayaObj.MayaName, "Edge List"), CurProgress/MaxProgress);
		
		// Go through every triangle (3 verts)
		for(int v=0; v<tris.Length; v+=3){
			// Edge A
			EdgeExists = false;	// Reset exists value
			for(int i=0; i<EdgeList.Count; i++){
				if(EdgeList[i].Match(tris[v], tris[v+1]) != 0){
					EdgeExists = true;
					EdgeIndexList.Add(i);
					break;
				}
			}
			if(!EdgeExists){
				EdgeList.Add(new MayaEdge(tris[v], tris[v+1]));
				EdgeIndexList.Add(EdgeList.Count - 1);
			}
			
			// Edge B
			EdgeExists = false;	// Reset exists value
			for(int i=0; i<EdgeList.Count; i++){
				if(EdgeList[i].Match(tris[v+1], tris[v+2]) != 0){
					EdgeExists = true;
					EdgeIndexList.Add(i);
					break;
				}
			}
			if(!EdgeExists){
				EdgeList.Add(new MayaEdge(tris[v+1], tris[v+2]));
				EdgeIndexList.Add(EdgeList.Count - 1);
			}
			
			// Edge C
			EdgeExists = false;	// Reset exists value
			for(int i=0; i<EdgeList.Count; i++){
				if(EdgeList[i].Match(tris[v+2], tris[v]) != 0){
					EdgeExists = true;
					EdgeIndexList.Add(i);
					break;
				}
			}
			if(!EdgeExists){
				EdgeList.Add(new MayaEdge(tris[v+2], tris[v]));
				EdgeIndexList.Add(EdgeList.Count - 1);
			}
		}
		
		// --------------------------------------------------
		// Combine edges into single string
		// --------------------------------------------------
		string EdgesStr = "\n";
		ColumnCounter = 0;
		for(int i=0; i<EdgeList.Count; i++){
			if(ColumnCounter == 0) EdgesStr += "\t\t";
			EdgesStr += (EdgeList[i].StartEdge + " " + EdgeList[i].EndEdge + " 0 ");
			if(ColumnCounter == ColumnDataWidth) EdgesStr += "\n";
						
			// Increment column counter
			ColumnCounter++;
			if(ColumnCounter > ColumnDataWidth) ColumnCounter = 0;
		}
		
		// Write data
		data += "\tsetAttr -s " + EdgeList.Count + " \".ed[" + (EdgeList.Count > 1 ? "0:" : "") + (EdgeList.Count - 1) + "]\" " + EdgesStr + ";\n";
		AppendToFile(data);
		data = "";
		
		// --------------------------------------------------
		// Normals
		// --------------------------------------------------
		// The way Unity specifies the normals list is 1 normal per vertex
		// entry. But we need normals per-vertex per-face. So we go through the 
		// triangles list and find what vertices make up the face. We then use 
		// that to index into the normals array to find the normals per face
		if(ExportNormals){
			// Update progress bar
			EditorUtility.DisplayProgressBar(ProgressTitle, string.Format(ProgressMsg, MayaObj.MayaName, "Normals"), CurProgress/MaxProgress);

			string NormalsStr = "";
			for(int v=0; v<tris.Length; v+=3){
				// Get the normals and convert them into Maya translation
				Vector3 normalA = MayaUtilities.MayaTranslation(normals[tris[v]]);
				Vector3 normalB = MayaUtilities.MayaTranslation(normals[tris[v+1]]);
				Vector3 normalC = MayaUtilities.MayaTranslation(normals[tris[v+2]]);
			
				// Not sure why this works, but we have to flip normal C and normal B for them to match
				// correctly in Maya. Weird
				NormalsStr += ("\t\t" + normalA.x + " " + normalA.y + " " + normalA.z + " ");
				NormalsStr += (normalC.x + " " + normalC.y + " " + normalC.z + " ");
				NormalsStr += (normalB.x + " " + normalB.y + " " + normalB.z);
				
				// Add a return character if not the last entry into the normals list,
				// this way the last one has the semicolon next to the number instead of the next line
				if(v+3 < tris.Length - 1) NormalsStr += "\n";
			}
			
			// Write Normal data
			data += "\tsetAttr -s " + tris.Length + " \".n[0:" + (tris.Length - 1) + "]\" -type \"float3\"\n" + NormalsStr + ";\n";
			AppendToFile(data);
			data = "";
		}

		// --------------------------------------------------
		// Faces
		// --------------------------------------------------
		// Now we need to tell Maya which edges make up each face. In Unity you can
		// specify the faces simply by giving 3 indexes into the vertex array, and it will build the 
		// face from that, but Maya is different. Maya needs edges specified in order to define a face.
		// So for each triangle go through the edges list and find the corresponding edges that
		// match the triangle vertices
		EditorUtility.DisplayProgressBar(ProgressTitle, string.Format(ProgressMsg, MayaObj.MayaName, "Faces"), CurProgress/MaxProgress);
		
		// --------------------------------------------------
		// 1 or more faces check!
		// --------------------------------------------------
		// For an object that has more than 1 face, Maya will list the range
		// of faces like so: [0:N] 
		// BUT! if there are objects with just 1 face, Maya will list the range
		// like so: [0]
		// So do a check here and make sure the correct format is used
		string faceFormat = ""; if((tris.Length / 3) > 1) faceFormat = "0:";
		data += "\tsetAttr -s " + (tris.Length / 3) + " \".fc[" + faceFormat + ((tris.Length / 3) - 1) + "]\" -type \"polyFaces\"\n";
		for(int i=0; i<tris.Length; i+=3){
			// --------------------------------------------------
			// Record the polygon face-edge data
			// --------------------------------------------------
			// NOTE! We reverse the order of the edge indices, from ABC to CBA
			// because Maya uses a counter-clockwise winding order for faces and Unity
			// gives us the data in clockwise winding order
			data += "\t\tf 3 " + EdgeIndexList[i+2] + " " + EdgeIndexList[i+1] + " " + EdgeIndexList[i];

			// Record the main UV data per face, if it exists and is requested.
			// Note - We don't completely reverse the order, but swap the second and 
			// last values so it displays correctly in Maya
			if(ExportUVs){
				if(uvs.Length > 0) data += " mu 0 3 " + tris[i] + " " + tris[i+2] + " " + tris[i+1];
			}
			
			// Record the lightmap UV data per face, if it exists and is requested.
			// Same swapping mechanism as the main UV data
			if(ExportLightmapUVs){
				if(uvs2.Length > 0) data += " mu 1 3 " + tris[i] + " " + tris[i+2] + " " + tris[i+1];
			}
		
			// Record vertex color per face, if it exists and is requested
			if(ExportVertexColors){
				if(colors.Length > 0) data += " mc 0 3 " + tris[i] + " " + tris[i+2] + " " + tris[i+1];
			}

			data += "\n";
		}
		// Add trailing semicolon after face setup
		data += ";\n";
		
		// Write Face data
		AppendToFile(data);
		data = "";
	}
	#endregion
	
	#region DisplayLayers
	void DisplayLayerExport(){
		string data = "";
		
		data += "createNode displayLayerManager -n \"layerManager\";\n";
			string LayerOrder = "";
			for(int i=0; i<DisplayLayers.Count; i++){
				LayerOrder += (i+1) + " ";
			}
			data += "\tsetAttr -s " + DisplayLayers.Count + " \".dli[" + (DisplayLayers.Count > 1 ? "1:" : "") + (DisplayLayers.Count) + "]\" " + LayerOrder + ";\n";
			data += "\tsetAttr -s " + DisplayLayers.Count + " \".dli\";\n";
		data += "connectAttr \"layerManager.dli[0]\" \"defaultLayer.id\";\n";
			
		// Write DisplayLayer Setup data
		AppendToFile(data);
		data = "";
		
		// Write out each display layer and connections
		for(int i=0; i<DisplayLayers.Count; i++){
			data += "createNode displayLayer -n \"Lightmap_Layer_" + i + "\";\n";
				data += "\tsetAttr \".do\" " + (i+1) + ";\n";
			for(int j=0; j<DisplayLayers[i].Objects.Count; j++){
				data += "connectAttr \"Lightmap_Layer_" + i + ".di\" \"" + DisplayLayers[i].Objects[j].MayaName + ".do\";\n";
			}
			data += "connectAttr \"layerManager.dli[" + (i+1) + "]\" \"Lightmap_Layer_" + i + ".id\";\n";
			
			// Write DisplayLayer Setup data
			AppendToFile(data);
			data = "";
		}
	}
	#endregion
	
	#region MayaMaterials
	// --------------------------------------------------
	// Build MayaMaterial List
	// --------------------------------------------------
	// This will go through and create MayaMaterial equivalents
	// of the Materials it finds on all the MayaObjects in the FlatObjList
	void BuildMaterialList(){
		// Go through each MayaObject
		for(int i=0; i<FlatObjList.Count; i++){
			// Perform checks before operating on the MayaObj
			if(!FlatObjList[i].IsMesh()) continue;

			// Get materials
			Material[] mats = FlatObjList[i].UnityObject.gameObject.GetComponent<Renderer>().sharedMaterials;
			
			// Go through each material and check if we already have it in
			// our MayaMaterials list. If we don't, then add it
			bool MatFound = false;
			for(int m=0; m<mats.Length; m++){
				for(int c=0; c<MayaMaterials.Count; c++){
					// If we found the material
					if(MayaMaterials[c].UnityMaterial == mats[m]){
						MatFound = true;
						break;
					}
				}
				
				// If we did not find the material
				if(!MatFound){
					// Create the material
					MayaMaterial NewMat = new MayaMaterial();
					
					// Clean the name
					// Note - For some reason the materials names have (Instance) in
					// it. If I find a better way to get the real name, we can remove
					// the part that strips it off.
					string NewName = CleanName(mats[m].name.Replace(" (Instance)",""));
					
					// Register the name
					NewName = RegisterName(NewName);
					
					// Set the MayaMaterial variables
					NewMat.MayaName = NewName;
					NewMat.UnityMaterial = mats[m];
					NewMat.MaterialInfo = "materialInfo"+MaterialInfoCounter;
					MaterialInfoCounter++;
					
					// Add the material to the list
					MayaMaterials.Add(NewMat);
				}
			}
		}
	}
	
	// --------------------------------------------------
	// Start Material Export
	// --------------------------------------------------
	// This will write out the lightLinker, renderPartition
	// and defaultShaderList. We need these when dealing with
	// materials
	void StartMaterialExport(){
		string data = "";
		
		// --------------------------------------------------
		// Create light linker
		// --------------------------------------------------
		// Note - We set these values to the number of materials + 1
		// (or in this case, we can just use the size of the MayaMaterials list)
		// because the initial particleCloud1 shader gets included into this list
		data += "createNode lightLinker -s -n \"lightLinker1\";\n";
			data += "\tsetAttr -s " + MayaMaterials.Count + " \".lnk\";\n";
			data += "\tsetAttr -s " + MayaMaterials.Count + " \".slnk\";\n";

		// --------------------------------------------------
		// Set render partition size
		// --------------------------------------------------
		// Note - Same thing as light linker
		data += "select -ne :renderPartition;\n";
			data += "\tsetAttr -s " + MayaMaterials.Count + " \".st\";\n";
			
		// --------------------------------------------------
		// Set default shader list size
		// --------------------------------------------------
		// Note - Same thing as light linker
		data += "select -ne :defaultShaderList1;\n";
			data += "\tsetAttr -s " + MayaMaterials.Count + " \".s\";\n";	
	
		// Write Material Setup data
		AppendToFile(data);
		data = "";
	}
	#endregion
	
	#region Textures
	// --------------------------------------------------
	// Build Texture List
	// --------------------------------------------------
	// This will go through the materials list and build a
	// file texture list from it
	void BuildTextureList(){
		// Go through each MayaMaterial
		for(int i=0; i<MayaMaterials.Count; i++){
			// _MainTex
			Texture MainTex = MayaMaterials[i].UnityMaterial.GetTexture("_MainTex");
			if(MainTex){
				// Check that this texture doesn't already exist in our list
				bool found = false;
				for(int t=0; t<MayaTextures.Count; t++){
					if(MayaTextures[t].UnityTexture == MainTex){
						found = true;
						
						// Add a link to this texture on the material
						MayaMaterials[i].MainTex = MayaTextures[t];
						
						break;
					}
				}
				// If it doesn't then make it
				if(!found){
					// Get current path to texture
					string currentTexturePath = GetFullTexturePath(MainTex);
					
					// Get texture asset name
					string assetName = GetTextureName(MainTex);
				
					// Create MayaFileTexture
					MayaFileTexture mayaTexture = new MayaFileTexture(MainTex, FileTextureCounter, currentTexturePath, filePath + assetName);
					
					// Add texture tiling
					mayaTexture.Tiling = MayaMaterials[i].UnityMaterial.GetTextureScale("_MainTex");
					
					// Add texture offset
					mayaTexture.Offset = MayaMaterials[i].UnityMaterial.GetTextureOffset("_MainTex");

					// Add the MayaFileTexture to our list
					MayaTextures.Add(mayaTexture);
					
					// Add a link to this texture on the material
					MayaMaterials[i].MainTex = mayaTexture;
					
					// Increment file texture counter
					FileTextureCounter++;
				}
			}
		}
	}
	
	// --------------------------------------------------
	// Get Full Texture Path
	// --------------------------------------------------
	// Given a Texture, this will return the full file path
	string GetFullTexturePath(Texture t){
		// Get the application data path
		// Note - This will be a path to the assets folder
		string dataPath = Application.dataPath;
		string[] tokens = dataPath.Split('/');
		dataPath = "";
		// Go through and rebuild path
		// Note - We go through Length - 1 since we want
		// to strip off the extra Assets token
		for(int i=0; i<tokens.Length - 1; i++){
			dataPath += tokens[i] + "/";
		}
		
		// Get the local path
		// Note - this will be a path FROM the assets folder
		// to the texture
		string localPath = AssetDatabase.GetAssetPath(t);
		
		// Return the full texture path
		return (dataPath + localPath);
	}
	
	// --------------------------------------------------
	// Get Texture Name
	// --------------------------------------------------
	// Given a Texture, this will return the name of the texture asset
	string GetTextureName(Texture t){
		// Get the local path
		// Note - this will be a path FROM the assets folder
		// to the texture
		string localPath = AssetDatabase.GetAssetPath(t);
		
		// Split the string
		string[] tokens = localPath.Split('/');
		
		return tokens[tokens.Length - 1];
	}
	
	// --------------------------------------------------
	// Start Texture Export
	// --------------------------------------------------
	// This will write out the defaultRenderUtilityList and set
	// the size of it to our number of File Textures + Bump Map Nodes
	void StartTextureExport(){
		string data = "";
		
		data += "select -ne :defaultTextureList1;\n";

		// The defaultRenderUtilityList contains all texture placement nodes
		// as well as all bump nodes. Set this value to the total combined number of
		// these nodes
		data += "select -ne :defaultRenderUtilityList1;\n";
			data += "\tsetAttr -s " + MayaTextures.Count + " \".u\";\n";
	
		// Write Texutre Setup data
		AppendToFile(data);
		data = "";
	}
	#endregion
	
	#region File Writing
	// --------------------------------------------------
	// Begin writing to a file
	// --------------------------------------------------
	// Note - First we pass the file name to the StreamWriter and tell it to NOT
	// append data. This will then erase any contents that were previously inside
	// the file
	void StartNewFile(){
		// Erase the file contents
		using (StreamWriter writer = new StreamWriter(filePath + fileName, false)){
			writer.Write("");
		}
		
		string data = "";
		
		// --------------------------------------------------
		// Adds Maya Scene File Header Info
		// --------------------------------------------------
		// This is required for any Maya Scene File.
		// This is also where you specify which version of Maya
		// the scene file should open on
		data += "//Maya ASCII " + MayaVersions[MayaVersionIndex] + " scene\n";
		data += "requires maya \"" + MayaVersions[MayaVersionIndex] + "\";\n";
		data += "currentUnit -l centimeter -a degree -t film;\n";
		data += "fileInfo \"application\" \"maya\";\n";
		
		// --------------------------------------------------
		// Adds Maya Default Cameras (persp, front, top, side)
		// --------------------------------------------------
		// You don't NEED this, but if you don't the organization in the Outliner
		// becomes very confusing, with all the objects listed before the cameras.
		// This will force the cameras to be created first and makes the objects
		// show up in the correct order
		data += "createNode transform -s -n \"persp\";\n";
			data += "\tsetAttr \".v\" no;\n";
			data += "\tsetAttr \".t\" -type \"double3\" 57 43 57 ;\n";
			data += "\tsetAttr \".r\" -type \"double3\" -28.076862662266123 44.999999999999986 8.9959671327898901e-015 ;\n";
		data += "createNode camera -s -n \"perspShape\" -p \"persp\";\n";
			data += "\tsetAttr -k off \".v\" no;\n";
			data += "\tsetAttr \".fl\" 34.999999999999993;\n";
			data += "\tsetAttr \".fcp\" 1000;\n";
			data += "\tsetAttr \".coi\" 91.361917668140052;\n";
			data += "\tsetAttr \".imn\" -type \"string\" \"persp\";\n";
			data += "\tsetAttr \".den\" -type \"string\" \"persp_depth\";\n";
			data += "\tsetAttr \".man\" -type \"string\" \"persp_mask\";\n";
			data += "\tsetAttr \".hc\" -type \"string\" \"viewSet -p %camera\";\n";
		data += "createNode transform -s -n \"top\";\n";
			data += "\tsetAttr \".v\" no;\n";
			data += "\tsetAttr \".t\" -type \"double3\" 0 100.1 0 ;\n";
			data += "\tsetAttr \".r\" -type \"double3\" -89.999999999999986 0 0 ;\n";
		data += "createNode camera -s -n \"topShape\" -p \"top\";\n";
			data += "\tsetAttr -k off \".v\" no;\n";
			data += "\tsetAttr \".rnd\" no;\n";
			data += "\tsetAttr \".fcp\" 1000;\n";
			data += "\tsetAttr \".coi\" 100.1;\n";
			data += "\tsetAttr \".ow\" 30;\n";
			data += "\tsetAttr \".imn\" -type \"string\" \"top\";\n";
			data += "\tsetAttr \".den\" -type \"string\" \"top_depth\";\n";
			data += "\tsetAttr \".man\" -type \"string\" \"top_mask\";\n";
			data += "\tsetAttr \".hc\" -type \"string\" \"viewSet -t %camera\";\n";
			data += "\tsetAttr \".o\" yes;\n";
		data += "createNode transform -s -n \"front\";\n";
			data += "\tsetAttr \".v\" no;\n";
			data += "\tsetAttr \".t\" -type \"double3\" 0 0 100.1 ;\n";
		data += "createNode camera -s -n \"frontShape\" -p \"front\";\n";
			data += "\tsetAttr -k off \".v\" no;\n";
			data += "\tsetAttr \".rnd\" no;\n";
			data += "\tsetAttr \".fcp\" 1000;\n";
			data += "\tsetAttr \".coi\" 100.1;\n";
			data += "\tsetAttr \".ow\" 30;\n";
			data += "\tsetAttr \".imn\" -type \"string\" \"front\";\n";
			data += "\tsetAttr \".den\" -type \"string\" \"front_depth\";\n";
			data += "\tsetAttr \".man\" -type \"string\" \"front_mask\";\n";
			data += "\tsetAttr \".hc\" -type \"string\" \"viewSet -f %camera\";\n";
			data += "\tsetAttr \".o\" yes;\n";
		data += "createNode transform -s -n \"side\";\n";
			data += "\tsetAttr \".v\" no;\n";
			data += "\tsetAttr \".t\" -type \"double3\" 100.1 0 0 ;\n";
			data += "\tsetAttr \".r\" -type \"double3\" 0 89.999999999999986 0 ;\n";
		data += "createNode camera -s -n \"sideShape\" -p \"side\";\n";
			data += "\tsetAttr -k off \".v\" no;\n";
			data += "\tsetAttr \".rnd\" no;\n";
			data += "\tsetAttr \".fcp\" 1000;\n";
			data += "\tsetAttr \".coi\" 100.1;\n";
			data += "\tsetAttr \".ow\" 30;\n";
			data += "\tsetAttr \".imn\" -type \"string\" \"side\";\n";
			data += "\tsetAttr \".den\" -type \"string\" \"side_depth\";\n";
			data += "\tsetAttr \".man\" -type \"string\" \"side_mask\";\n";
			data += "\tsetAttr \".hc\" -type \"string\" \"viewSet -s %camera\";\n";
			data += "\tsetAttr \".o\" yes;\n";
			
		// Write Header data
		AppendToFile(data);
		data = "";
	}
	
	// --------------------------------------------------
	// Write to File - Append Data
	// --------------------------------------------------
	// When writing a file, this will append data to the file
	void AppendToFile(string s){
		using (StreamWriter writer = new StreamWriter(filePath + fileName, true)){
			writer.Write(s);
		}
	}
	#endregion

	#region Mesh Debugger
	void MeshDebuger(){
		// Get the mesh data
		Mesh m = Selection.gameObjects[0].transform.GetComponent<MeshFilter>().sharedMesh;
		
		Vector3[] verts = m.vertices; 
		Vector2[] uvs = m.uv;
		Vector2[] uvs2 = m.uv2;
		Color[] colors = m.colors;
		Vector3[] normals = m.normals;
		int[] tris = m.triangles;
		
		Debug.Log("---------- Number of verts: " + verts.Length);
		Debug.Log("---------- Number of uvs: " + uvs.Length);
		Debug.Log("---------- Number of uvs2: " + uvs2.Length);
		Debug.Log("---------- Number of colors: " + colors.Length);
		Debug.Log("---------- Number of normals: " + normals.Length);
		Debug.Log("---------- Number of tris: " + tris.Length);
		
		for(int i=0; i<uvs2.Length; i++){
			Debug.Log("UV " + i + " Value: " + uvs2[i]);
		}
	}
	#endregion
}




