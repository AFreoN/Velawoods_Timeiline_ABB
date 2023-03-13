using UnityEngine;
#if UNITY_EDITOR	
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using CoreSystem;

/*
 	In this demo, we demonstrate:
	1.	Automatic asset bundle dependency resolving & loading.
		It shows how to use the manifest assetbundle like how to get the dependencies etc.
	2.	Automatic unloading of asset bundles (When an asset bundle or a dependency thereof is no longer needed, the asset bundle is unloaded)
	3.	Editor simulation. A bool defines if we load asset bundles from the project or are actually using asset bundles(doesn't work with assetbundle variants for now.)
		With this, you can player in editor mode without actually building the assetBundles.
	4.	Optional setup where to download all asset bundles
	5.	Build pipeline build postprocessor, integration so that building a player builds the asset bundles and puts them into the player data (Default implmenetation for loading assetbundles from disk on any platform)
	6.	Use WWW.LoadFromCacheOrDownload and feed 128 bit hash to it when downloading via web
		You can get the hash from the manifest assetbundle.
	7.	AssetBundle variants. A prioritized list of variants that should be used if the asset bundle with that variant exists, first variant in the list is the most preferred etc.
*/

// Loaded assetBundle contains the references count which can be used to unload dependent assetBundles automatically.
public class LoadedAssetBundle
{
	public AssetBundle m_AssetBundle;
	public int m_ReferencedCount;
	
	public LoadedAssetBundle(AssetBundle assetBundle)
	{
		m_AssetBundle = assetBundle;
		m_ReferencedCount = 1;
	}
}

// Class takes care of loading assetBundle and its dependencies automatically, loading variants automatically.
public class AssetBundleManager : MonoBehaviour
{
	private const float PROGRESS_TIME_OUT = 6f;
	static string m_BaseDownloadingURL = "";
	static string[] m_Variants =  {  };
	static AssetBundleManifest m_AssetBundleManifest = null;
	
	static Dictionary<string, LoadedAssetBundle> m_LoadedAssetBundles = new Dictionary<string, LoadedAssetBundle> ();
	static Dictionary<string, ActiveDownloadData> m_DownloadingWWWs = new Dictionary<string, ActiveDownloadData> ();
	static Dictionary<string, string> m_DownloadingErrors = new Dictionary<string, string> ();
	static List<AssetBundleLoadOperation> m_InProgressOperations = new List<AssetBundleLoadOperation> ();
	static Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]> ();
	static HashCacher m_HashCacher = new HashCacher ();
	
	static float _currentProgress = 0;
	
	// The base downloading url which is used to generate the full downloading url with the assetBundle names.
	public static string BaseDownloadingURL
	{
		get { return m_BaseDownloadingURL; }
		set { m_BaseDownloadingURL = value; }
	}
	
	// Variants which is used to define the active variants.
	public static string[] Variants
	{
		get { return m_Variants; }
		set { m_Variants = value; }
	}
	
	// AssetBundleManifest object which can be used to load the dependecies and check suitable assetBundle variants.
	public static AssetBundleManifest AssetBundleManifestObject
	{
		set {m_AssetBundleManifest = value; }
	}

	// Get loaded AssetBundle, only return vaild object when all the dependencies are downloaded successfully.
	static public LoadedAssetBundle GetLoadedAssetBundle (string assetBundleName, out string error)
	{
		if (m_DownloadingErrors.TryGetValue(assetBundleName, out error) )
			return null;
		
		LoadedAssetBundle bundle = null;
		m_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
		if (bundle == null)
			return null;
		
		// No dependencies are recorded, only the bundle itself is required.
		string[] dependencies = null;
		if (!m_Dependencies.TryGetValue(assetBundleName, out dependencies) )
			return bundle;
		
		// Make sure all dependencies are loaded
		foreach(var dependency in dependencies)
		{
			if (m_DownloadingErrors.TryGetValue(assetBundleName, out error) )
				return bundle;
			
			// Wait all the dependent assetBundles being loaded.
			LoadedAssetBundle dependentBundle;
			m_LoadedAssetBundles.TryGetValue(dependency, out dependentBundle);
			if (dependentBundle == null)
				return null;
		}
		
		return bundle;
	}
	
	// Load AssetBundleManifest.
	static public AssetBundleLoadManifestOperation Initialize (string manifestAssetBundleName)
	{
		var go = new GameObject("AssetBundleManager", typeof(AssetBundleManager));
		DontDestroyOnLoad(go);
		
		//We want to load new manifest file for every asset bundle to make sure it is the correct one. Unload the old manifest if it exists.
		UnloadAssetBundle (manifestAssetBundleName);
		m_AssetBundleManifest = null;
		
		LoadAssetBundle(manifestAssetBundleName, true);
		var operation = new AssetBundleLoadManifestOperation (manifestAssetBundleName, "AssetBundleManifest", typeof(AssetBundleManifest));
		m_InProgressOperations.Add (operation);
		return operation;
	}
	
	static protected void LoadAssetBundle(string assetBundleName)
	{
        LoadAssetBundle(assetBundleName, false);
	}
	
	// Load AssetBundle and its dependencies.
	static protected void LoadAssetBundle(string assetBundleName, bool isLoadingAssetBundleManifest)
	{
		//Starting new asset bundle download. Reset errors
		m_DownloadingErrors = new Dictionary<string, string> ();

		// Check if the assetBundle has already been processed.
		LoadAssetBundleInternal(assetBundleName, isLoadingAssetBundleManifest);
	}
	
	// Where we actuall call WWW to download the assetBundle.
	static protected bool LoadAssetBundleInternal (string assetBundleName, bool isLoadingAssetBundleManifest)
	{
		// Already loaded.
		LoadedAssetBundle bundle = null;
		m_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
		if (bundle != null && isLoadingAssetBundleManifest == false)
		{
			bundle.m_ReferencedCount++;
			return true;
		}
		
		// @TODO: Do we need to consider the referenced count of WWWs?
		// In the demo, we never have duplicate WWWs as we wait LoadAssetAsync()/LoadLevelAsync() to be finished before calling another LoadAssetAsync()/LoadLevelAsync().
		// But in the real case, users can call LoadAssetAsync()/LoadLevelAsync() several times then wait them to be finished which might have duplicate WWWs.
		if (m_DownloadingWWWs.ContainsKey(assetBundleName) )
			return true;
		
		WWW download = null;
		string url = m_BaseDownloadingURL + assetBundleName;
		
		// For manifest assetbundle, always download it as we don't have hash for it.
		if (isLoadingAssetBundleManifest)
			download = new WWW(url);
		else
		{
			download = CreateAssetBundleDownload(url, assetBundleName);
		}
		if(download != null)
		{
			ActiveDownloadData newDownload = new ActiveDownloadData(isLoadingAssetBundleManifest, assetBundleName, download, new DownloadTimeOut (0, Time.time));
			m_DownloadingWWWs.Add(assetBundleName, newDownload);
		}
		
		return false;
	}

	static private WWW CreateAssetBundleDownload(string url, string assetBundleName)
	{
		WWW download = null;
		#if BUNDLED_BUILD
		download = new WWW(url);
		#else
		
		if(m_AssetBundleManifest == null)
		{
			string hash = m_HashCacher.GetHash(url);
			int hashCode = 0;
			bool hashIsInt = int.TryParse(hash, out hashCode);

			if(hashIsInt && Caching.IsVersionCached(url, hashCode))
			{
				download = WWW.LoadFromCacheOrDownload(url, hashCode, 0);
			}
			else
			{
				Debug.LogError("Trying to load asset bundle locally but it does not exist in the cache");
				CoreEventSystem.Instance.SendEvent(CoreEventTypes.MISSION_LOADING_FAILED);
			}
		}
		else
		{
			Hash128 manifestHash = m_AssetBundleManifest.GetAssetBundleHash(assetBundleName);
            m_HashCacher.AddValue(url, manifestHash.GetHashCode().ToString(), assetBundleName);
            download = WWW.LoadFromCacheOrDownload(url, manifestHash.GetHashCode(), 0);
		}
		#endif

		return download;
	}

	// Unload assetbundle and its dependencies.
	static public void UnloadAssetBundle(string assetBundleName)
	{
        // When wanting to unload asset bundles, make sure all currently loaded are unloaded
        foreach (KeyValuePair<string, LoadedAssetBundle> assetBundle in m_LoadedAssetBundles)
        {
            UnloadAssetBundleInternal(assetBundle.Key);
            UnloadDependencies(assetBundle.Key);
        }
        UnloadSceneObjects();
        m_LoadedAssetBundles.Clear();
    }
	
	static protected void UnloadDependencies(string assetBundleName)
	{
		string[] dependencies = null;
		if (!m_Dependencies.TryGetValue(assetBundleName, out dependencies) )
			return;
		
		// Loop dependencies.
		foreach(var dependency in dependencies)
		{
			UnloadAssetBundleInternal(dependency);
		}
		
		m_Dependencies.Remove(assetBundleName);
	}
	
	static protected void UnloadAssetBundleInternal(string assetBundleName)
	{
		string error;
		LoadedAssetBundle bundle = GetLoadedAssetBundle(assetBundleName, out error);
        if (bundle == null)
            return;

		if (--bundle.m_ReferencedCount == 0)
		{
			bundle.m_AssetBundle.Unload(false);
			//m_LoadedAssetBundles.Remove(assetBundleName);
			//Debug.Log("AssetBundle " + assetBundleName + " has been unloaded successfully");
		}
	}
	
	void Update()
	{
		// Collect all the finished WWWs.
		var keysToRemove = new List<string>();
		
		if (m_DownloadingWWWs.Count == 0)
			_currentProgress = 1;

		foreach (var keyValue in m_DownloadingWWWs)
		{
			WWW download = keyValue.Value.download;

			if(download != null)
			{
				//If progress has remained at same value for too long this is a download error.
				//Will restart download until X amount of times until we reach fail limit
				float currentProgress = download.progress;
				DownloadTimeOut timeOutData = keyValue.Value.timeOutData;
				if(timeOutData.progress == currentProgress)
				{
					if(Time.time - timeOutData.timeOfProgressChange >= PROGRESS_TIME_OUT && keyValue.Value.download.progress < 1f)
					{
						if(keyValue.Value.failCount < ActiveDownloadData.MAX_FAIL_COUNT)
						{
							Debug.Log("Reinit download at " + keyValue.Value.download.progress);
                            //Restart download. Not reached failed count yet
                            string url = keyValue.Value.download.url;

                            AssetBundler_WWWHandler.Instance.AddWWW(keyValue.Value.download);
                            keyValue.Value.download = new WWW(url);

                            keyValue.Value.timeOutData = new DownloadTimeOut(0, Time.time);
							keyValue.Value.failCount++;
						}
						else
						{
							Debug.Log("Dispose download");
							keysToRemove.Add(keyValue.Key);
							m_DownloadingErrors.Add(keyValue.Key, "Download progress unchanged for too long. Download failed");
						}
						continue;
					}
				}
				else
				{
					//Otherwise the progress has updated so change the most recent time of update.
					keyValue.Value.timeOutData.timeOfProgressChange = Time.time;
					keyValue.Value.timeOutData.progress = download.progress;
				}
			}

			// If downloading fails (not timeout).
			if (download.error != null && m_DownloadingErrors.ContainsKey(keyValue.Key) == false)
			{
				m_DownloadingErrors.Add(keyValue.Key, download.error);
				keysToRemove.Add(keyValue.Key);
				continue;
			}
			
			// If downloading succeeds.
			if(download.isDone)
			{
				//Only show "unpacking" if we are loading a full asset bundle.
				if(keyValue.Value.isManifest == false)
				{
					CoreEventSystem.Instance.SendEvent(CoreEventTypes.MISSION_DOWNLOAD_FINISHED);
				}
				m_LoadedAssetBundles.Add(keyValue.Key, new LoadedAssetBundle(download.assetBundle) );
				keysToRemove.Add(keyValue.Key);
			}
			else
			{
				_currentProgress = download.progress;
				CoreEventSystem.Instance.SendEvent(CoreEventTypes.MISSION_LOADING_PROGRESS, _currentProgress);
			}
		}
		
		// Remove the finished WWWs.
		foreach( var key in keysToRemove)
		{
			ActiveDownloadData downloadData = m_DownloadingWWWs[key];
	//		download.download.Dispose();
			m_DownloadingWWWs.Remove(key);
			//downloadData.download.Dispose();	// dispose successfully download object
			AssetBundler_WWWHandler.Instance.AddWWW(downloadData.download);
		}
		
		// Update all in progress operations
		for (int i=0;i<m_InProgressOperations.Count;)
		{
			if (!m_InProgressOperations[i].Update())
			{
				m_InProgressOperations.RemoveAt(i);
			}
			else
				i++;
		}
	}

	// Load asset from the given assetBundle.
	static public AssetBundleLoadAssetOperation LoadAssetAsync (string assetBundleName, string assetName, System.Type type)
	{
		AssetBundleLoadAssetOperation operation = null;

        LoadAssetBundle(assetBundleName);
        operation = new AssetBundleLoadAssetOperationFull(assetBundleName, assetName, type);

        m_InProgressOperations.Add(operation);

		return operation;
	}
	
	// Load level from the given assetBundle.
	static public AssetBundleLoadOperation LoadLevelAsync (string assetBundleName, string levelName, bool isAdditive)
	{
        LoadAssetBundle(assetBundleName);
        AssetBundleLoadOperation operation = new AssetBundleLoadLevelOperation(assetBundleName, levelName, isAdditive);

        m_InProgressOperations.Add(operation);

		return operation;
	}

    private static void UnloadSceneObjects()
    {
        foreach(Transform sceneObject in FindObjectsOfType<Transform>())
        {
            if(sceneObject.parent == null)
            {
                if(!System.Text.RegularExpressions.Regex.IsMatch(sceneObject.name, @"Canvas|Core|SceneLoader|AssetBundleManager|EventSystem|IAPPlugin"))
                {
                    Destroy(sceneObject.gameObject);
                }
            }
        }

#if CLIENT_BUILD
        // Find the USFadeEvent object located in the Maincanvas and delete as it will no longer be needed.
        GameObject UILayer = LayerSystem.Instance.GetLayer(UILayers.UI.ToString());
        if(UILayer != null)
        {
            Transform fadeObject = UILayer.transform.FindChild("USFadeObject");
            if(fadeObject != null)
            {
                Destroy(fadeObject.gameObject);
            }
        }
#endif
    }

	private class DownloadTimeOut
	{
		public DownloadTimeOut(float progress, float timeOfProgressChange)
		{
			this.progress = progress;
			this.timeOfProgressChange = timeOfProgressChange;
		}
		public float progress;
		public float timeOfProgressChange;
	}

	private class ActiveDownloadData
	{
		public const int MAX_FAIL_COUNT = 5;
		public ActiveDownloadData(bool isManifest, string bundleName, WWW download, DownloadTimeOut timeOutData)
		{
			this.isManifest = isManifest;
			this.bundleName = bundleName;
			this.download = download;
			this.timeOutData = timeOutData;
			this.failCount = 0;
		}

		public bool isManifest;
		public string bundleName;
		public int failCount;
		public WWW download;
		public DownloadTimeOut timeOutData;
	}
} // End of AssetBundleManager.