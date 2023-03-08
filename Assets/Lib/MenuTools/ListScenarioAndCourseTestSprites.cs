#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using CoreLib;

public class DatabaseSpritesTools : EditorWindow {

	
#if CLIENT_BUILD
	[MenuItem("VELA/Tools/List Missing LOCs contentxml")]
	public static void ListMissingContentXMLlocs ()
	{
		// Print all LOCs that are in EN, but not in other languages.

		int startLoc = 0;
		int endLoc = 999;

		string log = "List of LOCs that are in EN, but missing from other ("+ startLoc + " to " + endLoc +", all languages)" + "\n\n";
		log += "loc id : missing from" + "\n\n";
		
		for (int i = startLoc; i < endLoc; i++)
		{
			string locID = i.ToString();
			
			while (locID.Length < 3)
				locID = locID.Insert(0, "0");
				
			PlayerProfile.LanguageData languageData = new PlayerProfile.LanguageData();
			languageData.Code = new PlayerProfile.LanguageCode();

			string tempLog = "LOC_" + locID + " : ";
			bool found = false;

			for (int j = 0; j < Enum.GetNames(typeof(PlayerProfile.LanguageCode)).Length; j++)
			{
				string languageInitials = languageData.Code.ToString().Replace("_", "-").ToUpper();

				if (string.IsNullOrEmpty(ContentManager.Instance.getString("LOC_" + locID, languageInitials)))
				{
					if (j == 0)
						break;
					tempLog += languageInitials + " | ";
					found = true;
				}
				languageData.Code++;
			}
			if (found)
				log += (tempLog.Substring(0, tempLog.Length-3) + "\n");
		}
		Debug.Log (log);
	}

	[MenuItem("VELA/Tools/ListAllCarnegieCounters")]
	public static void ListAllCarnegieCounters ()
	{
		string log = "CARNEGIE COUNTERS \n\n";

		string levelID = "1";

		List<Dictionary<string, string>> courses = Database.Instance.Select ("*", "Course", "levelid=" + levelID);
		foreach (Dictionary<string, string> courseRow in courses)
		{
			string courseID = courseRow ["id"];
			string courseName = courseRow ["courseid"];

			List<Dictionary<string, string>> scenarios = Database.Instance.Select ("*", "Scenario", "courseid=" + courseID);
			foreach (Dictionary<string, string> scenarioRow in scenarios)
			{
				string scenarioID = scenarioRow ["id"];
				string scenarioName = scenarioRow ["scenarioid"];

				log += "Scenario " + scenarioName + "\n\n";

				List<Dictionary<string, string>> missions = Database.Instance.Select ("*", "Mission", "scenarioid=" + scenarioID);
				foreach (Dictionary<string, string> missionRow in missions)
				{
					string missionID = missionRow ["id"];
					string missionName = missionRow ["missionid"];

					log += "Mission " + missionName + "\n\n";

					List<Dictionary<string, string>> tasks = Database.Instance.Select ("*", "Task", "missionid=" + missionID);
					foreach (Dictionary<string, string> taskRow in tasks)
					{
						string taskID = taskRow ["id"];
						string taskName = taskRow ["taskid"];

						List<Dictionary<string, string>> activities = Database.Instance.Select ("*", "Activity", "taskid=" + taskID);
						foreach (Dictionary<string, string> activityRow in activities)
						{
							string activityName = activityRow["activityid"];

							string carnegieCounter = activityRow ["carnegie_counter"];

							log += "L" + levelID + "C" + courseName + "S" + scenarioName + "M" + missionName + "T" + taskName + "A" + activityName + " -> " + carnegieCounter;
							log += "\n";
						}
						log += "\n";
					}
				}
			}
		}

		Debug.Log (log);
	}

	[MenuItem("VELA/Tools/Load Did You Notice (Image) sprites from Streaming Assets")]
	public static void LoadDidYouNoticeSpritesFromStreamingAssets()
	{
		GameObject[] activities = Resources.LoadAll<GameObject> ("MiniGames/X3.3/Conversations");
		Debug.Log ("ACTIVITIES COUNT : " + activities.Length);

		string log = "";

		foreach (GameObject activity in activities)
		{
			GameObject activityObj = Instantiate<GameObject> (activity);
			Transform eventParent = activityObj.transform.Find ("Timelines for Dialog").Find ("Text Timeline").transform;
			for (int i=0; i<eventParent.childCount; i++)
			{
				AnimatedTextEventData eventData = eventParent.GetChild(i).GetComponent<AnimatedTextEventData> ();

				if (eventData == null) { continue; }
				if (string.IsNullOrEmpty(eventData.data.imageRef)) { continue; }
				else
				{
					Debug.Log ("Found");
					if (eventData.data.imageRef == "N/A")
						log += (activity.name + " - " + "N/A") + "\n";
				}

				CoreHelper.LoadStreamingAssetsSprite (CoreHelper.StreamingAssetsSpriteFolder.Minigames, eventData.data.imageRef, GameObject.Find ("Core").GetComponent<Core> ());
			}
			DestroyImmediate (activityObj);
		}

		Debug.Log (log);

		/*
		string widgetTypeID = Database.Instance.Select ("*", "WidgetType", @"typename=""X3.3_Image""") [0] ["id"];

		List<Dictionary<string, string>> widgetRows = Database.Instance.Select ("*", "Widget", "widgettypeid=" + widgetTypeID);

		foreach (Dictionary<string, string> widgetRow in widgetRows)
		{
			string widgetID = widgetRow["id"];

			List<Dictionary<string, string>> elementRows = Database.Instance.Select ("*", "WidgetElement", "widgetid=" + widgetID);

			foreach (Dictionary<string, string> elementRow in elementRows)
			{
				if (elementRow ["boolean2"] == "0")
					continue;

				string illustrationID = elementRow ["illustrationid"];
				
				if (string.IsNullOrEmpty (illustrationID) || illustrationID == "0") continue;

				string illustrationRef = Database.Instance.Select ("*", "Illustration", "id=" + illustrationID) [0] ["unityref"];

				Debug.Log (illustrationID);
				CoreHelper.LoadStreamingAssetsSprite (CoreHelper.StreamingAssetsSpriteFolder.Minigames, illustrationRef, GameObject.Find ("Core").GetComponent<Core> ());
			}
		}
		*/
	}
#endif
	[MenuItem("VELA/Tools/List Practice Activities DB Sprite References")]
	public static void ListAndLoadAllPracticeActivitiesImages()
	{
		List<Dictionary<string, string>> TestRows = Database.Instance.Select ("*", "PracticeActivityWidgetElement");
		string log = "PRACTICE ACTIVITIES" + "\n\n";

		foreach (Dictionary<string, string> row in TestRows)
		{
			int activityid = int.Parse (row["practiceactivitywidgetid"]);
			int illustrationID = int.Parse(row["illustrationid"]);
			
			// Scrapbook activity data, loaded from resources. ignore for now.
			switch (activityid)
			{
				case 21:
				case 167:
				case 168:
				case 258:
					continue;
			}

			if (illustrationID > 0)
			{
				string unityRef = Database.Instance.Select ("*", "Illustration", "id=" + row["illustrationid"]) [0] ["unityref"];

				// Ignore circle images - loaded from resources.
				if (unityRef.ToLower ().Contains ("circle"))
					continue;

				log += unityRef + "\n";
				CoreHelper.LoadStreamingAssetsSprite (CoreHelper.StreamingAssetsSpriteFolder.Minigames, unityRef, GameObject.Find ("core").GetComponent<Core> ());
			}
		}

		Debug.Log (log);
	}

	[MenuItem ("VELA/Tools/List Course and Scenario Test DB Sprite References")]
	public static void ListAllScenarioCourseTestImages ()
	{
		List<Dictionary<string, string>> TestRows = new List<Dictionary<string, string>> (0);
		string log = "";

		for (int i=0; i<2; i++)
		{
			List<string> illustrationReferences = new List<string> ();

			// Set Scenario or Course test fields
			switch (i)
			{
				case 0: log = "SCENARIO TEST" + "\n"; TestRows = Database.Instance.Select ("*", "Mission", @"missionname=""Scenario Test"""); break;
				case 1: log = "COURSE TEST" + "\n";   TestRows = Database.Instance.Select ("*", "Mission", @"missionname=""Course Test"""); break;
			}
				
			// For each row, get mission ID
			foreach (Dictionary<string,string> testRow in TestRows)
			{
				string scenarioID = testRow ["scenarioid"];
				string missionID = testRow ["id"];
				string missionName = testRow ["missionid"];
				List<Dictionary<string, string>> tasks = Database.Instance.Select ("*", "Task", "missionid="+missionID);

				// For each missionID, go through all tasks and get task IDs
				foreach (Dictionary<string, string> taskRow in tasks)
				{
					string taskID = taskRow ["id"];
					string taskName = taskRow ["taskid"];
					List<Dictionary<string,string>> activities = Database.Instance.Select ("*", "Activity", "taskid="+taskID);

					// For each task ID, go through all activities and get MiniGameData
					foreach (Dictionary<string, string> activityRow in activities)
					{
						string activityLog = "";
						bool found = false;

						string activityID = activityRow ["id"];
						string activityName = activityRow ["activityid"];
						List<MinigameSectionData> data = MinigameDataLoader.LoadMinigameData (activityID);

						// Go through all MiniGameData of this activity
						foreach (MinigameSectionData sectionData in data)
						{
							// Print activity info
							activityLog += "*******************************************" + "\n";
							activityLog += "ACTIVITY: " + "S" + scenarioID + " M" + missionName + " T" + taskName + " A" + activityName + "\n";
							activityLog += (sectionData.subType != null && sectionData.subType.Length>1) ? "SUBTYPE: " + sectionData.subType + "\n" : "";
							activityLog += "\n";


							// Go through main minigame section and search for illustrations and text sprite references

							//activityLog += "Main" + "\n";
							if (sectionData.illustration != null && sectionData.illustration.unityRef != null)
							{
								string unityRef = sectionData.illustration.unityRef;
								activityLog += "ILLUSTRATION: " + unityRef + "  (Description: " + sectionData.illustration.description + ")" + "\n";
								illustrationReferences.Add (unityRef);
								found = true;
							}
							if (sectionData.text != null)
								foreach (string targetText in sectionData.text)
								{
									List<string> SPRs = GetSPRReferences(targetText);
									foreach (string spr in SPRs)
									{
										activityLog += "TEXT: " + spr + "\n";
										illustrationReferences.Add (spr);
										found = true;
									}
								}
							

							// Go through all elements in this minigameSection, print all illustrations and text sprite references

							//activityLog += "Elements" + "\n";
							foreach (MinigameElementData elementData in sectionData.elements)
							{
								if (elementData.illustration != null && elementData.illustration.unityRef != null)
								{
									string unityRef = elementData.illustration.unityRef;
									activityLog += "ILLUSTRATION: " + unityRef + "  (Description: " + elementData.illustration.description + ")" + "\n";
									illustrationReferences.Add (unityRef);
									found = true;
								}
								if (elementData.text != null)
									foreach (string targetText in elementData.text)
									{
										List<string> SPRs = GetSPRReferences(targetText);
										foreach (string spr in SPRs)
										{
											activityLog += "TEXT: " + spr + "\n";
											illustrationReferences.Add (spr);
											found = true;
										}
									}
							}
						}
						// If any images found, commit to log
						if (found)
							log += activityLog + "\n\n"; 

					}
				}
			}
			// Print results
			Debug.Log (log);

			// Try to find sprites in Streaming Assets (if not found, an error will be displayed by the stream - recommended to run this in play mode, in edit mode the console doesn't automatically update unless there's a change in the scene)
			if (GameObject.Find ("Core") != null)
			{
				foreach (string unityRef in illustrationReferences)
				{
					switch (i)
					{
						case 0: CoreHelper.LoadStreamingAssetsSprite (CoreHelper.StreamingAssetsSpriteFolder.ScenarioTest, unityRef, GameObject.Find ("Core").GetComponent<Core> ()); break;
						case 1: CoreHelper.LoadStreamingAssetsSprite (CoreHelper.StreamingAssetsSpriteFolder.CourseTest, unityRef, GameObject.Find ("Core").GetComponent<Core> ()); break;
					}
				}
			}
			else
				Debug.LogWarning ("Drag Core in the scene to try to load the found sprites.");
		}
	}

	// Get a list of text marked with '<Sprite>...</Sprite>', '<Photo>...</Photo>', 'SPR_' from the input 'targetText'
	private static List<string> GetSPRReferences (string targetText)
	{
		List<string> result = new List<string> ();

		string entry = "";

		string tag = "<Sprite>";
		string tag2 = "</Sprite>";

		while (targetText.Contains (tag) && targetText.Contains (tag2))
		{
			entry = targetText.Substring (targetText.IndexOf (tag), targetText.IndexOf (tag2) + tag2.Length - targetText.IndexOf (tag));
			result.Add (entry.Replace (tag, "").Replace (tag2, ""));
			targetText = targetText.Replace (entry, "");
		}

		tag = "<Photo>";
		tag2 = "</Photo>";

		while (targetText.Contains (tag) && targetText.Contains (tag2))
		{
			entry = targetText.Substring (targetText.IndexOf (tag), targetText.IndexOf (tag2) + tag2.Length - targetText.IndexOf (tag));
			result.Add (entry.Replace (tag, "").Replace (tag2, ""));
			targetText = targetText.Replace (entry, "");
		}

		tag = "[picture";
		tag2 = "]";

		while (targetText.Contains (tag) && targetText.Contains (tag2))
		{
			entry = targetText.Substring (targetText.IndexOf (tag), targetText.IndexOf (tag2) + tag2.Length - targetText.IndexOf (tag));
			result.Add (entry);
			targetText = targetText.Replace (entry, "");
		}

		tag = "[ picture";
		tag2 = "]";

		while (targetText.Contains (tag) && targetText.Contains (tag2))
		{
			entry = targetText.Substring (targetText.IndexOf (tag), targetText.IndexOf (tag2) + tag2.Length - targetText.IndexOf (tag));
			result.Add (entry);
			targetText = targetText.Replace (entry, "");
		}

		tag = "SPR_";
		int maxLength = 25;

		while (targetText.Contains (tag))
		{
			entry = targetText.Substring (targetText.IndexOf (tag), (targetText.IndexOf (tag) + maxLength < targetText.Length) ? maxLength : targetText.Length - targetText.IndexOf (tag));
			if (entry.Contains ("<"))
				entry = entry.Substring (0, entry.IndexOf ("<"));
			if (entry.Contains (" "))
				entry = entry.Substring (0, entry.IndexOf (" "));
			result.Add (entry);
			targetText = targetText.Replace (entry, "");
		}

		return result;
	}
	
}
#endif
