using CoreSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DatabaseHelperFunctions
{
	public static int GetNumberOfActivitiesInMission(float levelID, float courseID, float scenarioID, float missionID)
	{
		string missionDBID = Database.Instance.GetID (new float[]{levelID, courseID, scenarioID, missionID});
		List<Dictionary<string, string>> activityCount = Database.Instance.Query ("SELECT count(*) FROM Activity as A, Task as T WHERE A.taskid = T.id AND T.missionid = " + missionDBID);
		return int.Parse(activityCount[0]["count(*)"]);
	}

	public static int GetNumberOfActivitiesInScenario(float levelID, float courseID, float scenarioID)
	{
		string scenarioDBID = Database.Instance.GetID (new float[]{levelID, courseID, scenarioID});
		List<Dictionary<string, string>> activityCount = Database.Instance.Query ("SELECT count(*) FROM Activity as A, Task as T, Mission as M WHERE A.taskid = T.id AND T.missionid = M.id AND M.scenarioid = " + scenarioDBID);
		return int.Parse(activityCount[0]["count(*)"]);
	}

	public static int GetNumberOfActivitiesInCourse(float levelID, float courseID)
	{
		string courseDBID = Database.Instance.GetID (new float[]{levelID, courseID});
		List<Dictionary<string, string>> activityCount = Database.Instance.Query ("SELECT count(*) FROM Activity as A, Task as T, Mission as M, Scenario as S WHERE A.taskid = T.id AND T.missionid = M.id AND M.scenarioid = S.id AND S.courseid = " + courseDBID);
		return int.Parse(activityCount[0]["count(*)"]);
	}

	public static int GetNumberOfActivitiesInLevel(float levelID)
	{
		string levelDBID = Database.Instance.GetID (new float[]{levelID});
		List<Dictionary<string, string>> activityCount = Database.Instance.Query ("SELECT count(*) FROM Activity as A, Task as T, Mission as M, Scenario as S, Course as C WHERE A.taskid = T.id AND T.missionid = M.id AND  M.scenarioid = S.id AND S.courseid = C.id AND C.levelid = " + levelDBID);
		return int.Parse(activityCount[0]["count(*)"]);
	}

	public static int GetNumberOfMissionsInLevel(float levelID)
	{
		string levelDBID = Database.Instance.GetID (new float[]{levelID});
		List<Dictionary<string, string>> activityCount = Database.Instance.Query ("SELECT count(*) FROM Mission as M, Scenario as S, Course as C WHERE  M.scenarioid = S.id AND S.courseid = C.id AND C.levelid = " + levelDBID + " AND M.missionname <> \"Scenario Test\" AND M.missionname <> \"Course Test\" AND M.missionname <> \"Level Test\"");
		return int.Parse(activityCount[0]["count(*)"]);
	}

	public static int GetNumberOfMissionsInCourse(float levelID, float courseID)
	{
		string courseDBID = Database.Instance.GetID (new float[]{levelID, courseID});
		List<Dictionary<string, string>> activityCount = Database.Instance.Query ("SELECT count(*) FROM Mission as M, Scenario as S WHERE  M.scenarioid = S.id AND S.courseid = " + courseDBID + " AND M.missionname <> \"Scenario Test\" AND M.missionname <> \"Course Test\" AND M.missionname <> \"Level Test\"");
        return int.Parse(activityCount[0]["count(*)"]);
	}

	public static int GetNumberOfActivitiesInGame()
	{
		// TODO: this will only work if ALL of the activities are in the database!!! We should count them and hardcode the number temporarily.
		return 3600;
		/*
		List<Dictionary<string, string>> activityCount = Database.Instance.Query ("SELECT count(*) FROM Level as L JOIN Course as C JOIN Scenario as S JOIN Mission as M JOIN Task as T JOIN Activity as A WHERE L.id=C.levelid AND C.id=S.courseid AND S.id=M.scenarioid AND M.id=T.missionid AND T.id = A.taskid;");
		return int.Parse(activityCount[0]["count(*)"]);
		*/
	}

	public static int GetNumberOfMissionsInScenario(float levelID, float courseID, float scenarioID)
	{
		string scenarioDBID = Database.Instance.GetID (new float[]{levelID, courseID, scenarioID});
        List<Dictionary<string, string>> missionCount = Database.Instance.Query("SELECT count(*) FROM Mission WHERE scenarioid = " + scenarioDBID + " AND missionname <> \"Scenario Test\" AND missionname <> \"Course Test\" AND M.missionname <> \"Level Test\"");
		return int.Parse(missionCount[0]["count(*)"]);
	}

	public static int GetNumberOfActivitiesInTask(float levelID, float courseID, float scenarioID, float missionID, float taskID)
	{
		string taskDBID = Database.Instance.GetID (new float[]{levelID, courseID, scenarioID, missionID, taskID});
		List<Dictionary<string, string>> activityCount = Database.Instance.Query ("SELECT count(*) FROM Activity as A WHERE A.taskid = " + taskDBID);
		return int.Parse(activityCount[0]["count(*)"]);
	}
	
	public static int GetNumberOfTasksInMission(float levelID, float courseID, float scenarioID, float missionID)
	{
		string missionDBID = Database.Instance.GetID (new float[]{levelID, courseID, scenarioID, missionID});
		List<Dictionary<string, string>> taskCount = Database.Instance.Query ("SELECT count(*) FROM Task as T WHERE T.missionid = " + missionDBID);
		return int.Parse(taskCount[0]["count(*)"]);
	}
	
	public static int GetNumberOfScenariosInCourse(float levelID, float courseID)
	{
		string courseDBID = Database.Instance.GetID (new float[]{levelID, courseID});
		List<Dictionary<string, string>> scenarioCount = Database.Instance.Query ("SELECT count(*) FROM Scenario as S WHERE S.courseid = " + courseDBID);
		return int.Parse(scenarioCount[0]["count(*)"]);
	}
	
	public static int GetNumberOfCoursesInLevel(float levelID)
	{
		string levelDBID = Database.Instance.GetID (new float[]{levelID});
		List<Dictionary<string, string>> courseCount = Database.Instance.Query ("SELECT count(*) FROM Course as C WHERE C.levelid = " + levelDBID);
		return int.Parse(courseCount[0]["count(*)"]);
	}

	private static List<Dictionary<string, string>> GetOrderedListOfMissions(bool includeTests = true)
	{
		//Get ascending list of missions in format L1C1S1M1
		string query = "SELECT M.id as missionid FROM Mission as M, Scenario as S, Course as C, Level as L " +
            "WHERE  M.scenarioid = S.id AND S.courseid = C.id AND C.levelid = L.id " + (includeTests ? "" : "AND M.missionname <> 'Scenario Test' AND M.missionname <> 'Course Test' AND M.missionname <> 'Level Test'") +
				"ORDER BY L.levelid, C.courseid, S.scenarioid, M.missionid";
		List<Dictionary<string, string>> missions = Database.Instance.Query (query);
		return missions;
	}

	public static string GetNextMissionDBID(string currentMissionDBID)
	{
		List<Dictionary<string, string>> missions = GetOrderedListOfMissions ();
		for(int i = 0; i < missions.Count - 1; i++)
		{
			if(missions[i]["missionid"] == currentMissionDBID)
			{
				return missions[i + 1]["missionid"];
			}
		}
		return "-1";
	}


    public static string GetNextMissionDBIDExcludingTests(string currentMissionDBID)
    {
        List<Dictionary<string, string>> missions = GetOrderedListOfMissions(false);
        for (int i = 0; i < missions.Count - 1; i++)
        {
            if (missions[i]["missionid"] == currentMissionDBID)
            {
                return missions[i + 1]["missionid"];
            }
        }
        return "-1";
    }


	public static string GetPreviousMissionDBID (string currentMissionDBID)
	{
		List<Dictionary<string, string>> missions = GetOrderedListOfMissions ();
		for(int i = 1; i < missions.Count; i++)
		{
			if(missions[i]["missionid"] == currentMissionDBID)
			{
				return missions[i - 1]["missionid"];
			}
		}
		return "-1";
	}

	private static List<Dictionary<string, string>> GetOrderedListOfActivities()
	{
		//Get ascending list of missions in format L1C1S1M1
		string query = "SELECT A.id as activityid FROM Activity as A, Task as T, Mission as M, Scenario as S, Course as C, Level as L " +
			"WHERE A.taskid = T.id AND T.missionid = M.id AND M.scenarioid = S.id AND S.courseid = C.id AND C.levelid = L.id " +
				"ORDER BY L.levelid, C.courseid, S.scenarioid, M.missionid, T.taskid, A.activityid";
		List<Dictionary<string, string>> activities = Database.Instance.Query (query);
		return activities;
	}

    public static List<Dictionary<string, string>> GetOrderedListOfActivitiesFromMissionDBID(string missionDBID)
    {
        //Get ascending list of missions in format L1C1S1M1
        string query = "SELECT A.activityid as activityid, T.taskid as taskid FROM Activity as A, Task as T " +
            "WHERE A.taskid = T.id AND T.missionid = " + missionDBID + " " +
                "ORDER BY T.taskid, A.activityid";
        List<Dictionary<string, string>> activities = Database.Instance.Query(query);
        return activities;
    }

    public static string GetCurrentActivityDBID (string currentActivityID)
	{
		List<Dictionary<string, string>> activities = GetOrderedListOfActivities ();
		for(int i = 1; i < activities.Count; i++)
		{
			if(activities[i]["activityid"] == currentActivityID)
			{
				return activities[i]["activityid"];
			}
		}
		return "-1";
	}

	public static string GetPreviousActivityDBID (string currentActivityID)
	{
		List<Dictionary<string, string>> activities = GetOrderedListOfActivities ();
		for(int i = 1; i < activities.Count; i++)
		{
			if(activities[i]["activityid"] == currentActivityID)
			{
				return activities[i - 1]["activityid"];
			}
		}
		return "-1";
	}

	public static float[] GetVisualIDsForMission(string missionDBID)
	{
		string query = "SELECT L.levelid as visualLevelID, C.courseid as visualCourseID, S.scenarioid as visualScenarioID, M.missionid as visualMissionID, M.id " +
			"FROM Mission as M, Scenario as S, Course as C, Level as L " +
				"WHERE M.id = " + missionDBID + " AND M.scenarioid = S.id AND S.courseid = C.id AND C.levelid = L.id";
		List<Dictionary<string, string>> mission = Database.Instance.Query (query);
		float[] result = new float[]{float.Parse(mission[0]["visualLevelID"]), float.Parse(mission[0]["visualCourseID"]),
			float.Parse(mission[0]["visualScenarioID"]), float.Parse(mission[0]["visualMissionID"])};
		return result;
	}

    public static bool GetTaskAndActivityID(int TaskDataBaseID, out int TaskID, out int ActivityID)
    {
        string query = "SELECT Task.taskid as TaskNumber, Activity.activityid as ActivityNumber FROM Activity INNER JOIN Task ON Activity.taskid = Task.id WHERE Activity.id = " + TaskDataBaseID;

        try
        {
            List<Dictionary<string, string>> queryResults = Database.Instance.Query(query);

            if(queryResults.Count > 0)
            {
                TaskID = int.Parse(queryResults[0]["TaskNumber"]);
                ActivityID = int.Parse(queryResults[0]["ActivityNumber"]);
                return true;
            }
            else
            {
                TaskID = -1;
                ActivityID = -1;
            }
        }
        catch (SystemException e)
        {
            TaskID = -1;
            ActivityID = -1;
            Debug.LogException(e);
            Debug.LogError("DATABASE ERROR: No data retrieved for task - " + TaskDataBaseID);
        }

        return false;
    }


	private static List<Dictionary<string, string>> GetOrderedListOfScenarios()
	{
		//Get ascending list of scenarios in format L1C1S1
		string query = "SELECT S.id as scenarioid FROM Scenario as S, Course as C, Level as L " +
			"WHERE S.courseid = C.id AND C.levelid = L.id " +
				"ORDER BY L.levelid, C.courseid, S.scenarioid";
		List<Dictionary<string, string>> scenarios = Database.Instance.Query (query);
		return scenarios;
	}

    public static float[] GetVisualIDsForActivity(string activityDBID)
    {
        string query = "SELECT L.levelid as visualLevelID, C.courseid as visualCourseID, S.scenarioid as visualScenarioID, " +
            "M.missionid as visualMissionID, T.taskid as visualTaskID, A.activityid as visualActivityID, A.id " +
            "FROM Activity as A, Task as T, Mission as M, Scenario as S, Course as C, Level as L " +
                "WHERE A.id = " + activityDBID + " AND A.taskid = T.id AND T.missionid = M.id AND M.scenarioid = S.id AND S.courseid = C.id AND C.levelid = L.id";
        List<Dictionary<string, string>> activity = Database.Instance.Query(query);

		float[] result = new float[]{1f,0f,0f,0f,0f,0f};

		if (activity.Count != 0){
			result = new float[]{float.Parse(activity[0]["visualLevelID"]), float.Parse(activity[0]["visualCourseID"]),
            float.Parse(activity[0]["visualScenarioID"]), float.Parse(activity[0]["visualMissionID"]),
            float.Parse(activity[0]["visualTaskID"]), float.Parse(activity[0]["visualActivityID"])};
		}
		return result;
    }

    public static string GetPreviousScenarioDBID(string currentScenarioDBID)
	{
		List<Dictionary<string, string>> scenarios = GetOrderedListOfScenarios ();
		for(int i = 1; i < scenarios.Count; i++)
		{
			if(scenarios[i]["scenarioid"] == currentScenarioDBID)
			{
				return scenarios[i - 1]["scenarioid"];
			}
		}
		return "-1";
	}

	public static float[] GetVisualIDsForScenario(string scenarioDBID)
	{
		string query = "SELECT L.levelid as visualLevelID, C.courseid as visualCourseID, S.scenarioid as visualScenarioID, S.id " +
			"FROM Scenario as S, Course as C, Level as L " +
				"WHERE S.id = " + scenarioDBID + " AND S.courseid = C.id AND C.levelid = L.id";
		List<Dictionary<string, string>> scenario = Database.Instance.Query (query);
		float[] result = new float[]{float.Parse(scenario[0]["visualLevelID"]), float.Parse(scenario[0]["visualCourseID"]),
			float.Parse(scenario[0]["visualScenarioID"])};
		return result;
	}

	public static float[] GetHighestActivityInMission(string missionDBID)
	{
		string query = "SELECT A.activityid as activityID, T.taskid as taskID, M.missionid as missionID, S.scenarioid as scenarioID, C.courseid as courseID, L.levelid as levelID " +
			"FROM Activity as A, Task as T, Mission as M, Scenario as S, Course as C, Level as L " +
			"WHERE A.taskid = T.id AND T.missionid = " + missionDBID + " AND T.missionid = M.id AND M.scenarioid = S.id AND S.courseid = C.id AND C.levelid = L.id " +
			"ORDER BY T.taskid desc, A.activityid desc limit 1";

		List<Dictionary<string, string>> highestActivity = Database.Instance.Query (query);

		float[] IDs = new float[]{ 1, 1, 1, 1, 1, 1};

		if(highestActivity.Count > 0)
		{
			IDs[0] = float.Parse(highestActivity[0]["levelID"]);
			IDs[1] = float.Parse(highestActivity[0]["courseID"]);
			IDs[2] = float.Parse(highestActivity[0]["scenarioID"]);
			IDs[3] = float.Parse(highestActivity[0]["missionID"]);
			IDs[4] = float.Parse(highestActivity[0]["taskID"]);
			IDs[5] = float.Parse(highestActivity[0]["activityID"]);
		}

		return IDs;
	}

	public static int GetDBIDForLanguage( string code ) {

		//TODO: Get this from the database

		switch ( code ) {
			case "en":
				return 1;
			case "es":
				return 2;
			case "pt_br":
				return 3;
            case "ar":
                return 4;
            case "jp":
                return 5;
            case "tl":
                return 6;
        }

		// Default to english
		return 1;

	}

    public static bool CheckIfActivityIsConversation(string activityID, out int databaseActivityID)
    {
        if (int.TryParse(activityID, out databaseActivityID) && databaseActivityID > -1)
        {
            int activityTypeID = Database.Instance.GetActivityTypeID(databaseActivityID);

            if (activityTypeID > 0 && (activityTypeID == 4 || activityTypeID == 5 || activityTypeID == 8 || activityTypeID == 10 || activityTypeID == 13))
            {
                return true;
            }
        }

        databaseActivityID = -1;
        return false;
    }


    public class TaskPositionID
    {
        public int Task = 1;
        public int Activity = 1;
    }

    public class MissionPositionID
    {
        public List<TaskPositionID> Tasks = new List<TaskPositionID>();

        public int Mission = 1;
        public int Scenario = 1;
        public int Course = 1;
        public int Level = 1;
    }

    public static MissionPositionID[] GetAllMissionPositionIDsInLevel(int levelDBID)
    {
        List<Dictionary<string, string>> missions = Database.Instance.Query(string.Format(

        @"SELECT 	
	        Level.levelid,
	        Course.courseid,
	        Scenario.scenarioid,
	        Mission.missionid, 
	        Task.taskid,
	        Activity.activityid
        FROM 
	        Level
        INNER JOIN
	        Course
        ON
	        Level.id = Course.levelid
        INNER JOIN
	        Scenario
        ON
	        Course.id = Scenario.courseid
        INNER JOIN 
	        Mission
        ON
	        Scenario.id = Mission.scenarioid
        INNER JOIN 
	        Task
        ON
	        Mission.id = Task.missionid
        INNER JOIN
	        Activity
        ON
	        Task.id = Activity.taskid
        WHERE 
	        Level.id = {0}  AND 
	        Mission.missionname <> 'Scenario Test' AND
	        Mission.missionname <> 'Course Test' AND 
	        Mission.missionname <> 'Level Test'
        ORDER BY
	        Level.levelid,
	        Course.courseid,
	        Scenario.scenarioid,
	        Mission.missionid,
	        Task.taskid,
	        Activity.activityid", levelDBID));

        List<MissionPositionID> AllMissions = new List<MissionPositionID>();

        int currentIndex = -1;

        for (int missionIndex = 0; missionIndex < missions.Count; ++missionIndex)
        {
            int currentMissionID = int.Parse(missions[missionIndex]["missionid"]);
            int currentScenarioID = int.Parse(missions[missionIndex]["scenarioid"]);

            if (currentIndex < 0 || currentMissionID != AllMissions[currentIndex].Mission || currentScenarioID != AllMissions[currentIndex].Scenario)
            {
                AllMissions.Add(new MissionPositionID()
                    {
                        Mission = currentMissionID,
                        Scenario = currentScenarioID,
                        Course = int.Parse(missions[missionIndex]["courseid"]),
                        Level = int.Parse(missions[missionIndex]["levelid"])
                    });

                ++currentIndex;
            }

            AllMissions[currentIndex].Tasks.Add(new TaskPositionID()
                {
                    Task = int.Parse(missions[missionIndex]["taskid"]),
                    Activity = int.Parse(missions[missionIndex]["activityid"])
                });
        }

        return AllMissions.ToArray();
    }
}
