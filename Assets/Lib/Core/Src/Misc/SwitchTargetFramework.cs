#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using System;

/*
 THIS FILE WILL AUTOMATICALLY CHECK AND UPDATE THE PROJECT'S TARGET FRAMEWORK TO MONODEVELOP 4.5.1
 So that we can compile our default parameters in MonoDevelop anytime, anyhow.
** Pretty kewwl!

* got it from: https://gist.github.com/MattRix/daf67d66227c61501397
 */

class SwitchTargetFramework : AssetPostprocessor 
{
	private static void OnGeneratedCSProjectFiles() //secret method called by unity after it generates the solution
	{
		string currentDir = Directory.GetCurrentDirectory();
		string[] csprojFiles = Directory.GetFiles(currentDir, "*.csproj");
		
		foreach(var filePath in csprojFiles)
		{
			FixProject(filePath);
		}
	}
	
	static bool FixProject(string filePath)
	{
		string content = File.ReadAllText(filePath);
		
		string searchString = "<TargetFrameworkVersion>v3.5</TargetFrameworkVersion>";
		string replaceString = "<TargetFrameworkVersion>v4.0</TargetFrameworkVersion>";
		
		if(content.IndexOf(searchString) != -1)
		{
			content = Regex.Replace(content,searchString,replaceString);
			File.WriteAllText(filePath,content);
			return true;
		}
		else 
		{
			return false;
		}
	}
	
}

#endif