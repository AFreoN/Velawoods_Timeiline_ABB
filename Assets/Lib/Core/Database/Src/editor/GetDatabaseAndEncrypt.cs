

using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using SqliteForUnity3D;

public class GetDatabaseAndEncrypt : MonoBehaviour
{
    [MenuItem("VELA/Database/Update and encrypt")]
	public static void UpdateAndEncrypt()
	{
		//delete old
		try {
			FileUtil.DeleteFileOrDirectory("Assets/StreamingAssets/Vela.Exported.db"); 
		}
		catch {
			Debug.LogWarning("GetDatabase: Unable to delete Vela.Exported.db");
		}

		//delete old unencrypted
		try {
			FileUtil.DeleteFileOrDirectory("Assets/StreamingAssets/Vela_Unencrypted.Exported.db"); 
		}
		catch {
			Debug.LogWarning("GetDatabase: Unable to delete Vela_Unencrypted.Exported.db");
		}

		//get new
		try {
			FileUtil.CopyFileOrDirectory("X:/2014/LearnDirect/DataBase/Vela.Exported.db", "Assets/StreamingAssets/Vela.Exported.db"); 
		}
		catch {
			Debug.LogError("GetDatabase: Unable to fetch latest Vela.Exported.db from server!");
		}

		//copy new as unencrypted
		try {
			FileUtil.CopyFileOrDirectory("Assets/StreamingAssets/Vela.Exported.db", "Assets/StreamingAssets/Vela_Unencrypted.Exported.db");
		}
		catch {
			Debug.LogError("GetDatabase: Unable to backup Vela.Exported.db to Vela_Unencrypted.Exported.db");
		}
		
		AssetDatabase.Refresh ();
        
        var dbPath = Application.streamingAssetsPath + "/" + Database.DB_FILENAME;
        var factory = new ConnectionFactory();
        ISQLiteConnection connection = factory.Create(dbPath);
        connection.SetDbKey(Database.DB_PASS);
        connection.Close();

        Debug.Log ("Database fetch successful!");
	}

    [MenuItem("VELA/Database/Encrpyt local")]
    public static void EncryptLocal()
    {
        //delete old
        try
        {
            FileUtil.DeleteFileOrDirectory("Assets/StreamingAssets/Vela.Exported.db");
        }
        catch
        {
            Debug.LogWarning("GetDatabase: Unable to delete Vela.Exported.db");
        }

        //copy new to encrypt
        try
        {
            FileUtil.CopyFileOrDirectory("Assets/StreamingAssets/Vela_Unencrypted.Exported.db", "Assets/StreamingAssets/Vela.Exported.db");
        }
        catch
        {
            Debug.LogError("GetDatabase: Unable to backup  Vela_Unencrypted.Exported.db to Vela.Exported.db");
        }

        AssetDatabase.Refresh();

        var dbPath = Application.streamingAssetsPath + "/" + Database.DB_FILENAME;
        var factory = new ConnectionFactory();
        ISQLiteConnection connection = factory.Create(dbPath);
        connection.SetDbKey(Database.DB_PASS);
        connection.Close();

        Debug.Log("Database encrpyt successful!");
    }
}

