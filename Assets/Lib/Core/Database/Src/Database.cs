using CoreLib;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using SQLite4Unity3d;
using System.Text;

public class Database : MonoSingleton<Database>
{
    public const string DB_PASS = "B3o90197NLWhw8J";
    public const string DB_FILENAME = "Vela.Exported.db"; //File name of database

	// File name of database without encryption
    public const string DB_UNENCRYPTED_FILENAME = "Vela_Unencrypted.Exported.db";

    private ISQLiteConnection _connection;

    protected override void Init()
    {
        if (_connection == null)
        {
            var factory = new ConnectionFactory();

#if UNITY_EDITOR || !UNITY_ANDROID

			if ( Database.ShouldUseEncryption() ) {

                //var dbPath = Application.streamingAssetsPath + "/" + DB_FILENAME;
                //_connection = factory.Create(dbPath);
                //_connection.Key(DB_PASS);
                var dbPath = Application.streamingAssetsPath + "/" + DB_UNENCRYPTED_FILENAME;
                _connection = factory.Create(dbPath);

            }
            else{

				var dbPath = Application.streamingAssetsPath + "/" + DB_UNENCRYPTED_FILENAME;
				_connection = factory.Create(dbPath);

			}

#else 
			var filepath = string.Format("{0}/{1}", Application.persistentDataPath, DB_FILENAME);

            if (!File.Exists(filepath))
            {
                //Copy DB from streaming assets to persistent data path. Can open from persistent path but not in streaming assets.
				var streamAssetPath = "jar:file://" + Application.dataPath + "!/assets/";

				// For when Android does use encryption
				if ( Database.ShouldUseEncryption() ) {
					streamAssetPath += DB_FILENAME;
				}else{
					streamAssetPath += DB_UNENCRYPTED_FILENAME;
				}

                var loadDb = new WWW(streamAssetPath);  
                while (!loadDb.isDone) { }
				File.WriteAllBytes(filepath, loadDb.bytes);
            }

            var dbPath = filepath;
            
            _connection = factory.Create(dbPath);
#endif

            CoreEventSystem.Instance.AddListener(CoreEventTypes.ON_APPLICATION_QUIT, Close);
        }	
	}

	public static bool ShouldUseEncryption() {
		
		#if UNITY_EDITOR
		
			UnityEditor.BuildTarget[] targetsWithEncryption = new UnityEditor.BuildTarget[] {
				UnityEditor.BuildTarget.StandaloneWindows,
				UnityEditor.BuildTarget.StandaloneWindows64
			};
			
			return Array.IndexOf( targetsWithEncryption, UnityEditor.EditorUserBuildSettings.activeBuildTarget ) > -1;
		
		#else

			#if UNITY_STANDALONE_WIN
				return true;
			#else
				return false;
			#endif

			/*RuntimePlatform[] targetsWithEncryption = new RuntimePlatform[] {
				RuntimePlatform.WindowsPlayer,
			};

			return Array.IndexOf( targetsWithEncryption, Application.platform ) > -1;*/
		
		#endif
		
	}
	
	private object asyncLock = new object();
	public List<Dictionary<string, string>> Query(string query)
	{
		lock(asyncLock)
		{
			Init();
			
			List<Dictionary<string, string>> result = _connection.Query(query);

			return result;
		}
	}

    public void Close(object parameters)
    {
        lock (asyncLock)
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection = null;
            }
            CoreEventSystem.Instance.RemoveListener(CoreEventTypes.ON_APPLICATION_QUIT, Close);
        }
    }
	
	/// <summary>
	/// I build a string for you to complete a select statment from the database.
	/// 
	/// SELECT "select" FROM "from" WHERE "where" ORDER BY "order_by" ;
	/// 
	/// if "where" or "order_by" are equal to "" they will not be included e.g:
	/// SELECT "select" FROM "from" ;
	/// 
	/// </summary>
	/// <param name="select">Select.</param>
	/// <param name="from">From.</param>
	/// <param name="where">Where.</param>
	/// <param name="order_by">Order_by.</param>
	/// 
	/// 
	public List<Dictionary<string, string>> Select(string select, string from)
	{
		return Select (select, from, "", "");
	}
	public List<Dictionary<string, string>> Select(string select, string from, string where)
	{
		return Select (select, from, where, "");
	}
	public List<Dictionary<string, string>> Select(string select, string from, string where, string order_by)
	{
		Init();

        StringBuilder queryStringBuilder = new StringBuilder();

        queryStringBuilder.Append("SELECT ");
        queryStringBuilder.Append(select);
        queryStringBuilder.Append(" FROM ");
        queryStringBuilder.Append(from);

        if (!String.IsNullOrEmpty(where))
        {
            queryStringBuilder.Append(" WHERE ");
            queryStringBuilder.Append(where);
        }

        if (!String.IsNullOrEmpty(order_by))
        {
            queryStringBuilder.Append(" ORDER BY ");
            queryStringBuilder.Append(order_by);
        }

        queryStringBuilder.Append(" ;");

        return Query(queryStringBuilder.ToString());
	}
	
	public Dictionary<string, string> GetActivityFlag(string activityID, string flagType)
	{
		try
		{
			return Select("flagvalue", "ActivityFlag", "activityflagtypeid=" + flagType + " AND activityid=" + activityID)[0];
		}
		catch (SystemException e)
		{
			Debug.LogException(e);
			Debug.Log("Database Error: No activity flags for activityid - " + activityID);
			
			Dictionary<string, string> data = new Dictionary<string, string>();
			data["flagvalue"] = "";
			return data;
		}
	}
	
	public string GetActivityID(int levelID, int courseID, int scenarioID, int missionID, int taskID, float activityID)
	{
		return GetID(new float[]{(float)levelID, (float)courseID, (float)scenarioID, (float)missionID, (float)taskID, activityID});  
	}
	
	public string GetActivityType(string actualActivityID)
	{
		try
		{
			return Query("SELECT ActivityType.activitytypeid FROM Activity JOIN ActivityType ON ActivityType.id = Activity.activitytypeid WHERE Activity.id=" + actualActivityID)[0]["activitytypeid"];
		}
		catch (SystemException e)
		{
			Debug.LogException(e);
			Debug.LogError("DATABASE ERROR: No data retrieved for activity - " + actualActivityID);
			return "NULL";
		}
	}

    public string GetActivityWidgetType(string actualActivityID)
    {
        try
        {
            List<Dictionary<string, string>> queryResults = Query(string.Format("SELECT WidgetType.typename FROM Activity JOIN WidgetType ON WidgetType.id IN (SELECT Widget.widgettypeid FROM Widget WHERE widget.activityid = Activity.id) WHERE Activity.id = {0}", actualActivityID));

            if(queryResults.Count > 0)
            {
                return queryResults[0]["typename"];
            }
            else
            {
                return null;
            }
        }
        catch (SystemException e)
        {
            Debug.LogException(e);
            return null;
        }
    }

    public bool CheckIfMissionHasData(float[] ids)
    {
        string query = "SELECT id as ActualMissionID FROM Mission WHERE missionid = " + ids[3] + " AND scenarioid = " +
                        "(SELECT id as ActualScenarioID FROM Scenario WHERE scenarioid = " + ids[2] + " AND courseid = " +
                            "(SELECT id as ActualCourseID FROM Course WHERE courseid = " + ids[1] + " AND levelid = " +
                            "(SELECT id as ActualLevelID FROM Level where levelid = " + ids[0] + ")));​";
        return Query(query).Count > 0;
    }
	
	/// <summary>
	/// Gets the ID depending on the amount of floats in the given array.
	/// 
	/// [1] = Get the Actual Level ID
	/// [2] = Get the actual Course ID
	/// [3] = Get the actual Scenario ID
	/// [4] = Get the actual Mission ID
	/// [5] = Get the actual Task ID
	/// [6] = Get the actual Activity ID
	/// 
	/// These ids have changed with the new database format and
	/// are not the same as 1.1.1.1.1 does not mean task 1 has an id of 1.
	/// 1.1.1.1 task id is 2 in the database (as from 03/02/2015) and using this method
	/// will help you get the right id number.
	/// </summary>
	/// <returns>The I.</returns>
	/// <param name="ids">Identifiers.</param>
	public string GetID(float[] ids)
	{
		try
		{
			switch (ids.Length)
			{
			case 1:
			{
				string query = "SELECT id as ActualLevelID FROM Level where levelid = " + ids[0];
				return Query(query)[0]["ActualLevelID"];
			}
				
			case 2:
			{
				string query = "SELECT id as ActualCourseID FROM Course WHERE courseid = " + ids[1] + " AND levelid = " +
					"(SELECT id as ActualLevelID FROM Level where levelid = " + ids[0] + ");​";
				return Query(query)[0]["ActualCourseID"];
			}
				
			case 3:
			{
				string query = "SELECT id as ActualScenarioID FROM Scenario WHERE scenarioid = " + ids[2] + " AND courseid = " +
					"(SELECT id as ActualCourseID FROM Course WHERE courseid = " + ids[1] + " AND levelid = " +
						"(SELECT id as ActualLevelID FROM Level where levelid = " + ids[0] + "));​";
				return Query(query)[0]["ActualScenarioID"];
			}
				
			case 4:
			{
				string query = "SELECT id as ActualMissionID FROM Mission WHERE missionid = " + ids[3] + " AND scenarioid = " +
					"(SELECT id as ActualScenarioID FROM Scenario WHERE scenarioid = " + ids[2] + " AND courseid = " +
						"(SELECT id as ActualCourseID FROM Course WHERE courseid = " + ids[1] + " AND levelid = " +
						"(SELECT id as ActualLevelID FROM Level where levelid = " + ids[0] + ")));​";
				return Query(query)[0]["ActualMissionID"];
			}
				
			case 5:
			{
				string query = "SELECT id as ActualTaskID FROM Task WHERE taskid= " + ids[4] + " AND missionid = " +
					"(SELECT id as ActualMissionID FROM Mission WHERE missionid = " + ids[3] + " AND scenarioid = " +
						"(SELECT id as ActualScenarioID FROM Scenario WHERE scenarioid = " + ids[2] + " AND courseid = " +
						"(SELECT id as ActualCourseID FROM Course WHERE courseid = " + ids[1] + " AND levelid = " +
						"(SELECT id as ActualLevelID FROM Level where levelid = " + ids[0] + "))));​";
				return Query(query)[0]["ActualTaskID"];
			}
				
			case 6:
			{
				string query = "SELECT id as ActualActivityID FROM Activity WHERE activityid= " + ids[5] + " AND taskid = " +
					"(SELECT id as ActualTaskID FROM Task WHERE taskid= " + ids[4] + " AND missionid = " +
						"(SELECT id as ActualMissionID FROM Mission WHERE missionid = " + ids[3] + " AND scenarioid = " +
						"(SELECT id as ActualScenarioID FROM Scenario WHERE scenarioid = " + ids[2] + " AND courseid = " +
						"(SELECT id as ActualCourseID FROM Course WHERE courseid = " + ids[1] + " AND levelid = " +
						"(SELECT id as ActualLevelID FROM Level where levelid = " + ids[0] + ")))));​";
				return Query(query)[0]["ActualActivityID"];
			}
			}
		}
		catch(SystemException e)
		{
			Debug.LogException(e);
			Debug.LogError("DATABASE ERROR: No data retrieved");
			return "-1";
		}
		
		return "-1";
	}

    public int GetActivityTypeID(int actualActivityID)
    {
        try
        {
            string result = Query("select activitytypeid from Activity where id = " + actualActivityID)[0]["activitytypeid"];

            return int.Parse(result);
        }
        catch (SystemException e)
        {
            Debug.LogException(e);
            Debug.LogError("DATABASE ERROR: No data retrieved for activity - " + actualActivityID);
            return -1;
        }
    }
}
