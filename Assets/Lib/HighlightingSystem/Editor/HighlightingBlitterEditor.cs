using UnityEngine;
using UnityEditor;
using System.Collections;
using HighlightingSystem;

[CustomEditor(typeof(HighlightingBlitter))]
public class HighlightingBlitterEditor : Editor
{
	protected HighlightingBlitter hb;
	
	// 	
	protected virtual void OnEnable(){}

	// 
	public override void OnInspectorGUI()
	{

	}
}
