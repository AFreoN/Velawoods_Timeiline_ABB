using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;



public class AssetbundlesMenuItems : ScriptableObject
{
	const string kSimulateAssetBundlesMenu = "AssetBundles/Simulate AssetBundles";
	
	private const string DefaultCacheServerIPAddress = "192.168.113.93";
	
	public static void SetupEditorPrefs()
	{
		#if UNITY_EDITOR
		
		Debug.Log ("EditorPrefs: Old values ["+Application.platform+"]->");
		Debug.Log ("key: AndroidSdkRoot value: " + EditorPrefs.GetString("AndroidSdkRoot"));
		Debug.Log ("key: JdkPath value: " + EditorPrefs.GetString("JdkPath"));
		Debug.Log ("key: CacheServerEnabled value: " + EditorPrefs.GetBool("CacheServerEnabled"));
		Debug.Log ("key: CacheServerIPAddress value: " + EditorPrefs.GetString("CacheServerIPAddress"));
		
		EditorPrefs.SetBool("CacheServerEnabled", true);
		string EnvCacheServer = Environment.GetEnvironmentVariable("UNITY_CACHE_SERVER");
		if (EnvCacheServer != null && EnvCacheServer.Length > 0) {
			EditorPrefs.SetString("CacheServerIPAddress", EnvCacheServer);
		}
		else {
			EditorPrefs.SetString("CacheServerIPAddress", DefaultCacheServerIPAddress);
		}
		
		if (Application.platform == RuntimePlatform.OSXEditor) {
			// fix osx editor prefs
			EditorPrefs.SetString("AndroidSdkRoot", "/Users/jenkins/Library/Android/sdk/");
			EditorPrefs.SetString("JdkPath", "/Library/Java/JavaVirtualMachines/jdk1.8.0_20.jdk/Contents/Home/");
		}
		else if (Application.platform == RuntimePlatform.WindowsEditor) {
			// fix osx editor prefs
			EditorPrefs.SetString("AndroidSdkRoot", "D:\\Android\\sdk\\");
			EditorPrefs.SetString("JdkPath", "C:\\Program Files\\Java\\jsk1.8.0_65\\");
		}
		
		Debug.Log ("EditorPrefs: New values ["+Application.platform+"]->");
		Debug.Log ("key: AndroidSdkRoot value: " + EditorPrefs.GetString("AndroidSdkRoot"));
		Debug.Log ("key: JdkPath value: " + EditorPrefs.GetString("JdkPath"));
		Debug.Log ("key: CacheServerEnabled value: " + EditorPrefs.GetBool("CacheServerEnabled"));
		Debug.Log ("key: CacheServerIPAddress value: " + EditorPrefs.GetString("CacheServerIPAddress"));
		
		#endif
	}
  
	[MenuItem ("VELA/Cache/Clean AssetBundle Cache")]
	static void CleanAssetBundleCache()
	{
		Caching.ClearCache();
	}
	
	[MenuItem ("AssetBundles/Build AssetBundles")]
	static public void BuildAssetBundles ()
	{
		BuildScript.BuildAssetBundles();
	}

	[MenuItem ("AssetBundles/Build Player")]
	static void BuildPlayer ()
	{
		BuildScript.BuildPlayer();
	}
	
	// Asset Bundles
	
	// optimised bundles
	// build full set
	public static void BuildAssetBundlesAllPlatforms_Full()
	{
		ResourceOptimiser.ResizeTextures_FullAndCompress();
		BuildAssetBundlesAllPlatforms();
	}
	
	// build half set
	public static void BuildAssetBundlesAllPlatforms_Half()
	{
		ResourceOptimiser.ResizeTextures_HalfAndCompress();
		BuildAssetBundlesAllPlatforms();
	}
	
	// build quarter set
	public static void BuildAssetBundlesAllPlatforms_Quarter()
	{
		ResourceOptimiser.ResizeTextures_QuarterAndCompress();
		BuildAssetBundlesAllPlatforms();
	}
	
	
	// build all required asset bundles
	[MenuItem ("VELA/Build/AssetBundles/Build All Platforms")]
	public static void BuildAssetBundlesAllPlatforms()
	{
		BuildAssetBundlesForPlatform(BuildTarget.StandaloneWindows);
		BuildAssetBundlesForPlatform(BuildTarget.Android);
		/*
		BuildAssetBundlesForPlatform(BuildTarget.iOS);
		*/
		
		// No need for Mac specific bundles
		//BuildAssetBundlesForPlatform(BuildTarget.StandaloneOSXIntel);
	}
	[MenuItem ("VELA/Build/AssetBundles/Build Current Platform")]
	public static void BuildAssetBundlesCurrentPlatform()
	{
		BuildAssetBundlesForPlatform(EditorUserBuildSettings.activeBuildTarget);
	}
	public static void BuildAssetBundlesForPlatform(BuildTarget buildTarget)
	{
		Debug.Log ("Building AssetBundles for platform " + buildTarget.ToString());
		
		// ensure Editor Prefs setup correctly on Mac OSX Editor
		SetupEditorPrefs();
		
		EditorUserBuildSettings.SwitchActiveBuildTarget(buildTarget);
		
		// Choose the output path according to the build target.
		string outputPath = Path.Combine(BuildScript.kAssetBundlesOutputPath,  BaseLoader.GetPlatformFolderForAssetBundles(buildTarget) );
		if (!Directory.Exists(outputPath) )
			Directory.CreateDirectory (outputPath);
		
		BuildPipeline.BuildAssetBundles (outputPath, 0, buildTarget);
	}
}
























