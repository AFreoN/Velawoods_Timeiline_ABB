
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class BatchReplaceByName : ScriptableWizard
{
	public GameObject     m_newType;
	public string         m_name = "";
	public bool         m_matchCase = true;
	
	[MenuItem("Custom/Batch Replace By Name")]
	
	static void CreateWizard ()
	{
		var replaceGameObjects = ScriptableWizard.DisplayWizard <BatchReplaceByName> ("Replace GameObjects", "Replace");
		replaceGameObjects.m_name = Selection.activeObject.name;                                                        //Prefill the name field with the active object
		
	}
	
	void OnWizardCreate ()
	{
		GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject> ();
		
		List<GameObject> myList = new List<GameObject> ();
		
		foreach (GameObject g in allObjects) {
			if ((m_matchCase ? g.name : g.name.ToUpper ()) == m_name) {
				myList.Add (g);
			}
		}
		
		if (!m_matchCase)
			m_name = m_name.ToUpper ();
		
		for (int i = myList.Count-1; i >= 0; i--) {
			
			GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab (m_newType);
			newObject.transform.parent = myList [i].transform.parent;
			newObject.transform.localPosition = myList [i].transform.localPosition;
			newObject.transform.localRotation = myList [i].transform.localRotation;
			newObject.transform.localScale = myList [i].transform.localScale;
			newObject.name = myList [i].name;
			UnityEngine.Object.DestroyImmediate (myList [i]);
			myList.RemoveAt (i);
		}
		
		
	}
}
