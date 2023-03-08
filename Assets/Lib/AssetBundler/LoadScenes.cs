using UnityEngine;
using System.Collections;
using System;

public class LoadScenes : BaseLoader {

	public string sceneAssetBundle = "scene.unity3d";
	public string sceneName = "testScene";

	public bool loadLevelAdditive = true;

	public Action ObjectDestroyed = delegate{};

	// Use this for initialization
	IEnumerator Start () {
		yield return StartCoroutine(Initialize(sceneAssetBundle) );

		// Load level.
		yield return StartCoroutine(LoadLevel (sceneAssetBundle, sceneName, loadLevelAdditive) );

		// Unload assetBundles.
		AssetBundleManager.UnloadAssetBundle(sceneAssetBundle);
	}

	void OnDestroy()
	{
		ObjectDestroyed();
	}
}
