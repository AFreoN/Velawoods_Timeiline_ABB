public class StreamingAssetsBundleList
{
	/*
	A list of asset bundles that will always be included in the app
	We should load these locally, never from Amazon.
	 */
	private static string[] streamingAssetList = new string[]
	{
		"apartmentscene.asset",
		"coursetest.asset",
		"scenariotest.asset",
        "leveltest.asset",
        "scrapbook.asset"
	};

	public static bool ShouldBeLoadedFromStreamingAssets(string assetBundle)
	{
		foreach(string streaming in streamingAssetList)
		{
			if(assetBundle == streaming)
			{
				return true;
			}
		}

		return false;
	}
}