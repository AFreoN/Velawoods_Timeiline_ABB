using UnityEngine;
using System.Collections;
using CoreSystem;
using System.IO;

#if UNITY_EDITOR	
using UnityEditor;
#endif

public class BaseLoader : MonoBehaviour 
{

	// Use this for initialization.
	IEnumerator Start ()
	{
		yield return StartCoroutine(Initialize() );
	}

	// Initialize the downloading url and AssetBundleManifest object.
	protected IEnumerator Initialize(string bundleName = "")
	{
		// Don't destroy the game object as we base on it to run the loading script.
		DontDestroyOnLoad(gameObject);

		string fullBundlePath; 
		#if BUNDLED_BUILD
		fullBundlePath = GetBundledPath(bundleName);
		#else
		fullBundlePath = GetExternalPath(bundleName);
		#endif

		AssetBundleManager.BaseDownloadingURL = fullBundlePath;
		Debug.Log("AssetBundles: BaseDownloadingURL = " + AssetBundleManager.BaseDownloadingURL);

		// Initialize AssetBundleManifest which loads the AssetBundleManifest object.

		var request = AssetBundleManager.Initialize(GetManifestName(bundleName));
		if (request != null)
			yield return StartCoroutine(request);

	}

	#if UNITY_EDITOR
	public static string GetPlatformFolderForAssetBundles(BuildTarget target)
	{
		switch(target)
		{
		case BuildTarget.Android:
			return "Android";
		case BuildTarget.iOS:
			return "iOS";
		case BuildTarget.StandaloneWindows:
		case BuildTarget.StandaloneWindows64:
			return "Windows";
		case BuildTarget.StandaloneOSXIntel:
		case BuildTarget.StandaloneOSXIntel64:
		case BuildTarget.StandaloneOSX:
			return "OSX";

			// Add more build targets for your own.
			// If you add more targets, don't forget to add the same platforms to GetPlatformFolderForAssetBundles(RuntimePlatform) function.
		default:
			return null;
		}
	}
	#endif

	static string GetPlatformFolderForAssetBundles(RuntimePlatform platform)
	{
		switch(platform)
		{
		case RuntimePlatform.Android:
			return "Android";
		case RuntimePlatform.IPhonePlayer:
			return "iOS";
		case RuntimePlatform.WindowsPlayer:
			return "Windows";
		case RuntimePlatform.OSXPlayer:
			return "OSX";
			// Add more build platform for your own.
			// If you add more platforms, don't forget to add the same targets to GetPlatformFolderForAssetBundles(BuildTarget) function.
		default:
			return null;
		}
	}

	protected IEnumerator Load (string assetBundleName, string assetName)
	{
		Debug.Log("Start to load " + assetName + " at frame " + Time.frameCount);

		// Load asset from assetBundle.
		AssetBundleLoadAssetOperation request = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(GameObject) );
		if (request == null)
			yield break;
		yield return StartCoroutine(request);

		// Get the asset.
		GameObject prefab = request.GetAsset<GameObject> ();
		Debug.Log(assetName + (prefab == null ? " isn't" : " is")+ " loaded successfully at frame " + Time.frameCount );

		if (prefab != null)
			GameObject.Instantiate(prefab);
	}

	protected IEnumerator LoadLevel (string assetBundleName, string levelName, bool isAdditive)
	{
		// Load level from assetBundle.
		AssetBundleLoadOperation request = AssetBundleManager.LoadLevelAsync(assetBundleName, levelName, isAdditive);
		if (request == null)
			yield break;
		yield return StartCoroutine(request);

		if(request.HasCompletedSuccessfully())
		{
			CoreEventSystem.Instance.SendEvent(CoreEventTypes.MISSION_LOADING_FINISHED, levelName);
		}
	}

	static private string GetPlatformPath()
	{
		#if UNITY_EDITOR
		return GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
		#else
		return GetPlatformFolderForAssetBundles(Application.platform);
		#endif
	}

	static public string GetBundledPath(string assetBundleName)
	{
		string localBundleLocation;
		#if UNITY_ANDROID && !UNITY_EDITOR
		localBundleLocation = Application.streamingAssetsPath + "/AssetBundles/";
		#else
		localBundleLocation = "file://" + Application.streamingAssetsPath + "/AssetBundles/";
		#endif

		string fullBundlePath = localBundleLocation + GetPlatformPath() + "/" + GetNewPath(assetBundleName) + "/";

		return fullBundlePath;
	}

	static public string GetManifestName(string assetBundleName)
	{
		string assetName = assetBundleName.Split(new char[] { '.' })[0];

		if (!StreamingAssetsBundleList.ShouldBeLoadedFromStreamingAssets(assetBundleName))
		{
			float[] levelIDS = ActivityTracker.ConvertIDIntoIndividualIDs(assetName.ToUpper());

			return "M" + levelIDS[3];
		}
		else
		{
			return assetName;
		}    
	}

	static public string GetNewPath(string assetBundleName)
	{
		//missions/ClientVersionNum/Platform/BuildType/Compression/Level(L*)/Course(C*)/Scenario(S*)/Mission(M*)/bundle.asset

		string assetName = assetBundleName.Split(new char[] { '.' })[0];

		if(!StreamingAssetsBundleList.ShouldBeLoadedFromStreamingAssets(assetBundleName))
		{
			float[] levelIDS = ActivityTracker.ConvertIDIntoIndividualIDs(assetName.ToUpper());

			return "L" + levelIDS[0] + "/C" + levelIDS[1] + "/S" + levelIDS[2] + "/M" + levelIDS[3];
		}
		else
		{
			return assetName;
		}    


	}

	static public string GetExternalPath(string assetBundleName)
	{
		string assetBundlePath;

		#if BUNDLE_HTTPS
		assetBundlePath = "https://";
		#else
		assetBundlePath = "http://";
		#endif

		#if CDN_BUILD
		assetBundlePath += "d89lfpri1kb5x.cloudfront.net/missions/";
		#else
		assetBundlePath += "s3-eu-west-1.amazonaws.com/testing-assetbundles/missions/";
		#endif

		string fullBundlePath = "";
		//Force load some files always from streaming assets
		if(StreamingAssetsBundleList.ShouldBeLoadedFromStreamingAssets(assetBundleName))
		{
			fullBundlePath = GetBundledPath(assetBundleName);
		}
		else
		{
			//Cant access config from designer side
			#if CLIENT_BUILD
			//Live Amazom folder structure



			fullBundlePath = assetBundlePath + Config.AssetBundleVersion + "/" + GetPlatformPath() + "/" + Config.BuildType +
			"/" + DeviceCapabilities.Compression.ToString() + "/" + GetNewPath(assetBundleName) + "/";
			#endif
		}

		return fullBundlePath;
	}

	private string GetAssetBundleBasePath(string assetBundleName)
	{
		string assetBundlePath;

		#if BUNDLED_BUILD
		assetBundlePath = GetBundledPath(assetBundleName);
		#else
		assetBundlePath = GetExternalPath(assetBundleName);
		#endif

		return assetBundlePath;
	}
}
