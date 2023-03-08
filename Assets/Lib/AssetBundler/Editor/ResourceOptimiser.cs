using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

public class ResourceOptimiser : ScriptableObject
{
	// Audio optimisation functions
	[MenuItem("VELA/Optimisation/Audio/Optimise Sample Rate and Compress")]
	public static void OptimiseAudio_SampleRateAndCompress()
	{
		string[] guids = AssetDatabase.FindAssets("t: audioclip");
		string path = "";
		foreach(string guid in guids) {
			path = AssetDatabase.GUIDToAssetPath(guid);
			AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
			if(audioImporter == null) {
				Debug.LogWarning("unable to locate audioclip asset " + path);
				continue;
			}
			
			AudioImporterSampleSettings sampleSettings = new AudioImporterSampleSettings();
			sampleSettings.compressionFormat = AudioCompressionFormat.Vorbis;
			sampleSettings.quality = 0.5f;
			sampleSettings.loadType = AudioClipLoadType.CompressedInMemory;
			sampleSettings.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;
			
			audioImporter.defaultSampleSettings = sampleSettings;
			
			AssetDatabase.ImportAsset(path);
		}
	}
	
	
	
	// Texture optimisation functions
	[MenuItem("VELA/Optimisation/Textures/Full and Compress")]
	public static void ResizeTextures_FullAndCompress()
	{
		ResizeTextures(1, true);
	}
	[MenuItem("VELA/Optimisation/Textures/Half and Compress")]
	public static void ResizeTextures_HalfAndCompress()
	{
		ResizeTextures(2, true);
	}
	[MenuItem("VELA/Optimisation/Textures/Quarter and Compress")]
	public static void ResizeTextures_QuarterAndCompress()
	{
		ResizeTextures(4, true);
	}
	[MenuItem("VELA/Optimisation/Textures/Eighth and Compress")]
	public static void ResizeTextures_EighthAndCompress()
	{
		ResizeTextures(8, true);
	}
	

	public static float ResolutionBuffer = 0.15f;

	[MenuItem("VELA/Optimisation/Textures/Get List Of Nearly Square Textures")]
	public static void GetListOfNearlySquareTextures()
	{
		string[] guids = AssetDatabase.FindAssets("t: texture");
		string path = "";

		HashSet<string> allPaths = new HashSet<string>();

		int spriteCount = 0;
		foreach(string guid in guids) 
		{
			path = AssetDatabase.GUIDToAssetPath(guid);
			TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
			if(textureImporter != null && textureImporter.textureType == TextureImporterType.Sprite)
			{
				++spriteCount;

				int width = 0;
				int height = 0;
				
				if (textureImporter != null) 
				{
					object[] args = new object[2] { 0, 0 };
					MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
					mi.Invoke(textureImporter, args);
					
					width = (int)args[0];
					height = (int)args[1];
				}

				if(width != height)
				{
					int largestValue = Mathf.Max(width, height);
					int difference = Mathf.Abs(width - height);


					if(difference < largestValue * ResolutionBuffer)
					{
						allPaths.Add(path);
					}
				}
			}
		}

		Debug.Log("Searched through " + guids.Count() + " Textures, of which " + spriteCount + " are sprites");
		EditorGUIUtility.systemCopyBuffer = string.Join(System.Environment.NewLine, allPaths.ToArray());
		Debug.Log("A List of " + allPaths.Count + " texture paths have been copied to your clipboard");
	}


	private static float ImageSizeBuffer = 0.15f; 

	public static void ResizeTextures(int divideByAmount, bool setCompressed)
	{
		string[] guids = AssetDatabase.FindAssets("t: texture");
		string path = "";

		foreach(string guid in guids) 
		{
			path = AssetDatabase.GUIDToAssetPath(guid);
			TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

			int width = 0;
			int height = 0;

			if (textureImporter != null) 
			{
				object[] args = new object[2] { 0, 0 };
				MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
				mi.Invoke(textureImporter, args);
				
				width = (int)args[0];
				height = (int)args[1];
			}

			int widthAsPowerOfTwo = (int)Mathf.Pow(2, Mathf.Ceil(Mathf.Log(width - (width * ImageSizeBuffer))/ Mathf.Log(2)));
			int heightAsPowerOfTwo = (int)Mathf.Pow(2, Mathf.Ceil(Mathf.Log(height - (height * ImageSizeBuffer))/ Mathf.Log(2)));

			int textureSize = (int)Mathf.Max(widthAsPowerOfTwo, heightAsPowerOfTwo);
			
			if(textureImporter == null) {
				Debug.LogWarning ("textureImporter for asset: " + path + " is null");
				continue;
			}

			if(textureImporter.maxTextureSize > 4096) {
				Debug.LogWarning ("MaxTextureSize is massive for asset: " + path + "("+textureImporter.maxTextureSize+")");
			}
			
			// resize max size
			textureImporter.maxTextureSize = Mathf.Max(textureSize / divideByAmount, 32);
			
			// set the compressed flag
			if(setCompressed) {
				textureImporter.textureFormat = TextureImporterFormat.AutomaticCompressed;
			}
			
			AssetDatabase.ImportAsset(path); 
		}
	}
	
	
	
	// Resources folder
	[MenuItem("VELA/Optimisation/Resources/Stats")]
	public static void ResourcesStats()
	{
		string[] guids = AssetDatabase.FindAssets("");
		string path = "";
		
		int count = 0;
		int textureCount = 0;
		int audioCount = 0;
		int modelCount = 0;
		int movieCount = 0;
		int pluginCount = 0;
		int otherCount = 0;
		
		foreach(string guid in guids) {
			path = AssetDatabase.GUIDToAssetPath(guid);
			if(path.Contains("Resources")) {
				//Debug.Log (path);
				count++;
				
				// check if texture
				TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
				if(textureImporter != null) {
					textureCount++;
					continue;
				}
				
				// check if audio
				AudioImporter audioImporter = AssetImporter.GetAtPath(path) as AudioImporter;
				if(audioImporter != null) {
					audioCount++;
					continue;
				}
				
				// check if model
				ModelImporter modelImporter = AssetImporter.GetAtPath(path) as ModelImporter;
				if(modelImporter != null) {
					modelCount++;
					continue;
				}
				
				// check if movie
				//MovieImporter movieImporter = AssetImporter.GetAtPath(path) as MovieImporter;
				VideoClipImporter movieImporter = AssetImporter.GetAtPath(path) as VideoClipImporter;
				if(movieImporter != null) {
					movieCount++;
					continue;
				}
				
				// check if plugin
				PluginImporter pluginImporter = AssetImporter.GetAtPath(path) as PluginImporter;
				if(pluginImporter != null) {
					pluginCount++;
					continue;
				}
				
				// all other types
				otherCount++;
			}
		}
		Debug.Log ("Found a total of " + count + " resources");
		Debug.Log ("Texture = " + textureCount + " / " + ((textureCount*100) / count) + "%");
		Debug.Log ("Audio = " + audioCount + " / " + ((audioCount*100) / count) + "%");
		Debug.Log ("Models = " + modelCount + " / " + ((modelCount*100) / count) + "%");
		Debug.Log ("Movies = " + movieCount + " / " + ((movieCount*100) / count) + "%");
		Debug.Log ("Plugins = " + pluginCount + " / " + ((pluginCount*100) / count) + "%");
		Debug.Log ("Other = " + otherCount + " / " + ((otherCount*100) / count) + "%");
	}
}
