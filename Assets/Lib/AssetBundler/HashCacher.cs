using UnityEngine;
using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;

public class HashCacher
{
	public static string FILE_PATH = "/Hashes.xml";
	private List<HashEntry> hashes;
	
	public HashCacher ()
	{
		hashes = new List<HashEntry> ();
		LoadData ();
	}
	
	public void LoadData()
	{
		string data;

		if(File.Exists(Application.persistentDataPath + FILE_PATH) == false)
		{	
			SaveHashData();
			return;
		}
		
		data = File.ReadAllText(Application.persistentDataPath + FILE_PATH);

		if(string.IsNullOrEmpty(data))
		{
			SaveHashData();
			return;
		}
		
		try
		{
			XmlDocument xmlDoc = new XmlDocument ();
			xmlDoc.LoadXml(data);
			
			XmlNode xmlHashes = xmlDoc.SelectSingleNode("Hashes");
			XmlNodeList xmlHashesList = xmlHashes.SelectNodes("Asset");
			foreach(XmlNode xmlHash in xmlHashesList)
			{
				string url = xmlHash.SelectSingleNode("url").InnerText;
				string hash = xmlHash.SelectSingleNode("Hash").InnerText;
				HashEntry newHash = new HashEntry(url, hash);
				
				hashes.Add(newHash);
			}
		}
		catch(Exception)
		{
			Debug.LogError("Error in opening hash file");
			SaveHashData();
		}
	}

	public void AddValue(string url, string hash, string assetBundleName)
	{
		foreach(HashEntry existingHash in hashes)
		{
			if(existingHash.url == url)
			{
				existingHash.hash = hash;
				SaveHashData();
				return;
			}
		}

        // If not in standalone, clear cache and hashes as we only want one asset bundle in memory.
#if !UNITY_STANDALONE
        if (!StreamingAssetsBundleList.ShouldBeLoadedFromStreamingAssets(assetBundleName))
        {
            ClearCache();
        }
#endif

        HashEntry newHash = new HashEntry(url, hash);
		hashes.Add(newHash);
		SaveHashData();
	}

	public string GetHash(string url)
	{
		foreach(HashEntry existingHash in hashes)
		{
			if(existingHash.url == url)
			{
				return existingHash.hash;
			}
		}

		return "";
	}

    public void ClearCache()
    {
		Caching.ClearCache();

        hashes = new List<HashEntry>();
        SaveHashData();
    }

    public List<HashEntry> GetHashes()
    {
        return hashes;
    }
	
	private void SaveHashData()
	{
		XmlDocument xmlDoc = new XmlDocument();
		XmlNode xmlHashes = xmlDoc.CreateNode (XmlNodeType.Element, "Hashes", "");
		foreach(HashEntry hash in hashes)
		{
			XmlNode xmlAsset = xmlDoc.CreateNode (XmlNodeType.Element, "Asset", "");
			
			XmlNode xmlHashURl = xmlDoc.CreateNode (XmlNodeType.Element, "url", "");
			xmlHashURl.InnerText = hash.url;
			xmlAsset.AppendChild(xmlHashURl);
			
			XmlNode xmlHash = xmlDoc.CreateNode (XmlNodeType.Element, "Hash", "");
			xmlHash.InnerText = hash.hash;
			xmlAsset.AppendChild(xmlHash);
			
			xmlHashes.AppendChild(xmlAsset);
		}
		xmlDoc.AppendChild (xmlHashes);
		xmlDoc.Save(Application.persistentDataPath + FILE_PATH);
	}

	public class HashEntry
	{
		public HashEntry(string url, string hash)
		{
			this.url = url;
			this.hash = hash;
		}
		public string url;
		public string hash;
	}
}