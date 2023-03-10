using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class CreateAssetBundles
{
    [MenuItem("Assets/Build AssetBundle/Windows")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/StreamingAssets/Windows";

        string sceneName = SceneManager.GetActiveScene().name + "_Windowsbundle";

        string path = Path.Combine(assetBundleDirectory, sceneName);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.DeterministicAssetBundle, BuildTarget.StandaloneWindows64);
    }

    [MenuItem("Assets/Build AssetBundle/Mac")]
    static void BuildAllAssetBundlesMac()
    {
        string assetBundleDirectory = "Assets/StreamingAssets/Mac";

        string sceneName = SceneManager.GetActiveScene().name + "_MacBundle";

        string path = Path.Combine(assetBundleDirectory, sceneName);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.DeterministicAssetBundle, BuildTarget.StandaloneOSX);
    }
}
