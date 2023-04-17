using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class CreateAssetBundles
{
    [MenuItem("Assets/Build AssetBundle/All")]
    static void BuildAllAssetBundles()
    {
        BuildForWindows();
        BuildForMac();
        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/Build AssetBundle/Windows")]
    static void BuildAssetBundlesWindows()
    {
        BuildForWindows();
    }

    [MenuItem("Assets/Build AssetBundle/Mac")]
    static void BuildAssetBundlesMac()
    {
        BuildForMac();
    }

    static void BuildForWindows()
    {
        string assetBundleDirectory = "Assets/StreamingAssets/Windows";

        string sceneName = SceneManager.GetActiveScene().name + "_Windows";

        string path = Path.Combine(assetBundleDirectory, sceneName);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.DeterministicAssetBundle, BuildTarget.StandaloneWindows64);
    }

    static void BuildForMac()
    {
        string assetBundleDirectory = "Assets/StreamingAssets/Mac";

        string sceneName = SceneManager.GetActiveScene().name + "_Mac";

        string path = Path.Combine(assetBundleDirectory, sceneName);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.DeterministicAssetBundle, BuildTarget.StandaloneOSX);
    }
}
