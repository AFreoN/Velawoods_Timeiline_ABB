using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CoreLib
{
	public class ActivityTracker : MonoSingleton<ActivityTracker> 
	{
		// State
		public const string STATE_SEQUENCE_MISSION = "StateSequenceMission";
		public const string STATE_APARTMENT_SCENE = "StateApartmentScene";
		public const string STATE_OTHER_SCENE = "StateOtherScene";
		
		// Activity
		private float _currentActivity;
		public float Activity { get { return _currentActivity; } }
		
		// Task
		private float _currentTask;
		public float Task { get { return _currentTask; } }
		
		// Mission
		private float _currentMission;
		public float Mission { get { return _currentMission; } }
		
		// Scenario
		private float _currentScenario;
		public float Scenario { get { return _currentScenario; } }
		
		// Course
		private float _currentCourse;
		public float Course { get { return _currentCourse; } }
		
		// Level
		private float _currentLevel;
		public float Level { get { return _currentLevel; } }


        public float[] CurrentMission
        {
            get
            {
                return new float[] { Level, Course, Scenario, Mission };
            }
        }
		
		// Activity database ID
		private string _currentActualActivityId; 
		public string ActualActivityID { get { return _currentActualActivityId; } }
		
		// Current progress array
		public float[] Progress { get { 
				return new float[] {
					_currentLevel, _currentCourse, _currentScenario, 
					_currentMission, _currentTask, _currentActivity }; }
		}


		public string ActivityProgressString
		{
			get
			{
				return "T" + _currentTask + "A" + _currentActivity;
			}
		}

		// Current progress string (ex: Activity_L1C1S1M1T1A1)
		public string ProgressString { get {
				return  "Activity_" + 
						"L" + _currentLevel + 
						"C" + _currentCourse + 
						"S" + _currentScenario + 
						"M" + _currentMission + 
						"T" + _currentTask + 
						"A" + _currentActivity;
			}
		}

		// Current progress string without activity prepended (ex: L1C1S1M1T1A1)
		public string PlainProgressString { get {
				return  "L" + _currentLevel + 
						"C" + _currentCourse + 
						"S" + _currentScenario + 
						"M" + _currentMission + 
						"T" + _currentTask + 
						"A" + _currentActivity;
			}
		}
		
		// Current Sequence string (ex : L1C1S1M1_Sequence)
		public string SequenceString { get {
				return  "L" + _currentLevel +
						"C" + _currentCourse +
						"S" + _currentScenario +
						"M" + _currentMission +
						"_Sequence";
			}
		}

        public string CurrentMissionString
        {
            get
            {
                return "L" + _currentLevel +
                        "C" + _currentCourse +
                        "S" + _currentScenario +
                        "M" + _currentMission;
            }
        }
		
		// Current mission state
		private MissionStateData _missionState;
		public MissionStateData MissionState {
			set {
				_missionState = value;
			}
			get {
				if(_missionState == null)
					_missionState = new MissionStateData("", SceneType.Mission);
				return _missionState;
			}
		}
		
//--
		// Activity IDs (cached)
		private Dictionary<string, string> _cachedActivityIDs;
		
		// Tasks List, each entry containing an Activities List 
		// Tasks<Activities<activity DB id>>
		private Dictionary<string, List<string>> _tasks; 
		
		// Point we're at ...
		private int _taskIndex = 0; // ..in the _tasks list
		private int _activityIndex = 0; // ..in the _tasks[_taskIndex] activity list
		
		
	//-Interface------------------------------------------------------------------------------------------------------------
		
		// Init on first call
		protected override void Init ()
		{
			base.Init ();

			_currentTask = -1;
			_currentActivity = -1;
            
            // This will be controlled by a menu in the future (?)
            _currentMission = 1;
			_currentScenario = 1;
			_currentCourse = 1;
			_currentLevel = 1;

            _tasks = new Dictionary<string, List<string>> ();
            _cachedActivityIDs = new Dictionary<string, string> ();
            _currentActualActivityId = "-1";

			CoreEventSystem.Instance.AddListener (CoreEventTypes.LEVEL_CHANGE, SceneChanged);
		}

		protected override void Dispose ()
		{
			CoreEventSystem.Instance.RemoveListener (CoreEventTypes.LEVEL_CHANGE, SceneChanged);
			base.Dispose ();
		}

		// Set up new level
		public void SetUp(float[] levelDepth)
		{
			ChangeProgress (levelDepth); // Set level
			GetMissionData (); // Get data
			
			// Reset indexes
			_activityIndex = 0;
			_taskIndex = 0;
			
			CoreEventSystem.Instance.SendEvent(CoreEventTypes.MISSION_DATA_LOADED, _tasks);
			CoreEventSystem.Instance.SendEvent(CoreEventTypes.ACTIVITY_CHANGED, PlainProgressString);
			CoreEventSystem.Instance.SendEvent(CoreEventTypes.TASK_CHANGED, PlainProgressString);
		}

        public void NextActivity()
        {
			if (_tasks.Count == 0)
				GetMissionData ();

			if (_currentTask == -1)
			{
				NextTask();
				return;
			}

			_activityIndex++;

			List<string> activities = _tasks [_currentTask.ToString ()];

			if (_activityIndex < activities.Count) 
			{
				UpdateActivity();
			}
			else 
			{
				//At End of Task
				NextTask();
			}
        }
        
		/// <summary>
		/// Go back one activity. If the activity reaches < 0 then
		/// we attempt to go back one task
		/// </summary>
		public void PreviousActivity()
		{
			_activityIndex--;

			//Debug.Log ("ACTIVITY INDEX : " + _activityIndex);

			if (_activityIndex < 0)
			{
				_activityIndex = -1;
				PreviousTask ();
			} 
			else 
			{
				UpdateActivity();
			}
		}
		
		/// <summary>
		/// Changes where the user is in the game.
		/// The length of the array decides the level you change.
		/// 
		/// This works Activity -> Task -> Mission -> Scenario ->
		/// Course -> Level
		/// 
		/// [1] = Changes an Activity progress
		/// [1,2] = Changes the Activity and Task Progress
		/// [1,2,1] = Changes the Activity, Task and Mission Progress
		/// And so on to Level.
		/// </summary> 
		public void ChangeProgress(float[] new_progress)
		{
			if (new_progress == null) return;

			for (int i=0; i<new_progress.Length; i++)
			{
				switch (i) 
				{
				case 0: 
					_currentActivity = (int)new_progress[i];
					break;
				case 1:
					_currentTask = (int)new_progress[i];
					break;
				case 2: 
					_currentMission = (int)new_progress[i]; 
					break;
				case 3: 
					_currentScenario = (int)new_progress[i]; 
					break;
				case 4: 
					_currentCourse = (int)new_progress[i]; 
					break;
				case 5: 
					_currentLevel = (int)new_progress[i]; 
					break;
				}
			}

			//Debug.Log ("User Progress: " + ProgressString);
			
			UpdateActualActivityId();
        }
		
		
//-Privates--------------------------------------------------------------------------------------------------------------------
		
		private void NextTask()
		{
			_taskIndex++;
			
			if (_taskIndex < _tasks.Count)
			{
				UpdateTask();
			}
			else
			{
				//no more tasks for this mission
				//end mission
				Debug.Log("Reached End Of Mission");
			}
		}
		
		/// <summary>
		/// Go back one task.
		/// </summary>
		private void PreviousTask()
		{
			_taskIndex--;
			
			if (_taskIndex < 1) 
			{
				//nothing todo at start of tasks
				_taskIndex = -1;
				_activityIndex = -1;
				_currentTask = -1;
				_currentActivity = -1;
			} 
			else 
			{
				UpdateTask ();
			}
		}
		
		private void UpdateTask()
		{
			_activityIndex = -1;
			
			int counter = -1;
			
			foreach(string key in _tasks.Keys)
			{
				counter++;
				
				if(counter == _taskIndex)
				{
					_currentTask = float.Parse(key);
					NextActivity();
					break;
				}
			}
		}
		
		private void UpdateActivity()
		{
			List<string> activities = _tasks [_currentTask.ToString ()];
			
			//New Activity
			ChangeProgress(new float[]{ float.Parse (activities[_activityIndex]) });
		}
		
		// Change mission state
		private void SceneChanged(object sceneInfo)
		{
			MissionState = (MissionStateData)sceneInfo;
		}
		
		/// <summary>
		/// Gets the mission data including
		/// the tasks and activities' database IDs </summary>
        private void GetMissionData()
        {
			_tasks.Clear ();
			_cachedActivityIDs.Clear ();
			
			if(_currentMission > -1)
			{
				string missionDB_id = Database.Instance.GetID(new float[4] { _currentLevel, _currentCourse, _currentScenario, _currentMission });
				
				string actualActivityKeyBase = "Activity_L" + _currentLevel + "C" + _currentCourse + "S" + _currentScenario + "M" + _currentMission;
				if(missionDB_id != "-1")
				{
					List<Dictionary<string, string>> mission_data = Database.Instance.Select("*", "Task", "missionid=" + missionDB_id, "taskid ASC");
					
					float task_counter = 0;
					foreach(Dictionary<string, string> row in mission_data)
					{
						task_counter++;
						string task_id = row["id"];
						List<string> activities = new List<string>();
						
						string taskVisualID = row["taskid"];
						
						List<Dictionary<string, string>> task_data = Database.Instance.Select("*", "Activity", "taskid="+task_id, "activityid ASC");
						
						foreach(Dictionary<string, string> activity_row in task_data)
						{
							string activityVisualID = activity_row["activityid"];
							activities.Add(activityVisualID);
							string actualActivityKey = actualActivityKeyBase + "T" + taskVisualID + "A" + activityVisualID;
							_cachedActivityIDs[actualActivityKey] = activity_row["id"];
						}
						
						_tasks.Add(task_counter.ToString(), activities);
					}
				}
			}
            //PrintData ();
		}

		/// <summary> Gets what activity id is in the database currently </summary>
		private void UpdateActualActivityId()
		{
			_currentActualActivityId = Database.Instance.GetID (Progress);
			//DW: Below causing issues. Commented out until have time to look at cache fix.
			/*if(_cachedActivityIDs != null && _cachedActivityIDs.ContainsKey(ProgressString))
			{
				_currentActualActivityId = _cachedActivityIDs [ProgressString];
			}*/
		}
		
		/*
		private void PrintData ()
		{
			string debugLog = "";
			for (int i=0; i<_tasks.Count; i++)
			{
				for (int j=0; j<_tasks[i].Count; j++)
				{
					debugLog += _tasks[i][j] + " // ";
				}
				debugLog += "\n";
			}
			Debug.Log (debugLog);
		}*/

        /// <summary>
        /// This will take a float array such as [1, 1, 4, 1] and return [1, 2, 4, 1] which then matches the database. 
        /// </summary>
        /// <param name="splitIDs"></param>
        /// <returns></returns>
        public static float[] ConvertMissionNameIDToActualIDs(float[] splitIDs)
        {
            if(splitIDs.Length == 4)
            {
                float[] NewIDs = new float[4];

                NewIDs[0] = splitIDs[0];
                NewIDs[2] = splitIDs[2];
                NewIDs[3] = splitIDs[3];

                // Gore: I have spoken to Hug and he says that it is not just for level 1.
                //if(splitIDs[0] == 1)
                {
                    if (splitIDs[2] < 4)
                    {
                        NewIDs[1] = 1;
                    }
                    else if (splitIDs[2] > 6)
                    {
                        NewIDs[1] = 3;
                    }
                    else
                    {
                        NewIDs[1] = 2;
                    }

                    return NewIDs;
                }
                /*else
                {
                    NewIDs[1] = splitIDs[1];
                    return NewIDs;
                }*/
            }
            else
            {
                Debug.LogError("Incorrect Data Format");
                return new float[] { };
            }
        }

        public static float[] ConvertActualIDsToMissionNameID(float[] splitIDs)
        {
            if (splitIDs.Length == 4)
            {
                float[] NewIDs = new float[4];

                NewIDs[0] = splitIDs[0];
                NewIDs[1] = 1;
                NewIDs[2] = splitIDs[2];
                NewIDs[3] = splitIDs[3];

                return NewIDs;
            }
            else
            {
                Debug.LogError("Incorrect Data Format");
                return new float[] { };
            }
        }

        public static float[] ConvertIDIntoIndividualIDs(string ID)
        {
            int indexOfCourse = ID.IndexOf('C');
            int indexOfScenario = ID.IndexOf('S');
            int indexOfMission = ID.IndexOf('M');
            int indexOfTask = ID.IndexOf('T');
            int indexOfActivity = ID.IndexOf('A');

            float[] missionIDs = new float[6];
            if (indexOfCourse == -1)
            {
                missionIDs[0] = float.Parse(ID.Substring(1, ID.Length - 1));
                return missionIDs;
            }
            missionIDs[0] = float.Parse(ID.Substring(1, indexOfCourse - 1));

            if (indexOfScenario == -1)
            {
                missionIDs[1] = float.Parse(ID.Substring(indexOfCourse + 1, ID.Length - indexOfCourse - 1));
                return missionIDs;
            }
            missionIDs[1] = float.Parse(ID.Substring(indexOfCourse + 1, indexOfScenario - indexOfCourse - 1));

            if (indexOfMission == -1)
            {
                missionIDs[2] = float.Parse(ID.Substring(indexOfScenario + 1, ID.Length - indexOfScenario - 1));
                return missionIDs;
            }
            missionIDs[2] = float.Parse(ID.Substring(indexOfScenario + 1, indexOfMission - indexOfScenario - 1));

            if (indexOfTask == -1)
            {
                missionIDs[3] = float.Parse(ID.Substring(indexOfMission + 1, ID.Length - indexOfMission - 1));
                return missionIDs;
            }
            missionIDs[3] = float.Parse(ID.Substring(indexOfMission + 1, indexOfTask - indexOfMission - 1));

            if (indexOfActivity == -1)
            {
                missionIDs[4] = float.Parse(ID.Substring(indexOfTask + 1, ID.Length - indexOfTask - 1));
                return missionIDs;
            }
            missionIDs[4] = float.Parse(ID.Substring(indexOfTask + 1, indexOfActivity - indexOfTask - 1));
            missionIDs[5] = float.Parse(ID.Substring(indexOfActivity + 1, ID.Length - indexOfActivity - 1));

            return missionIDs;
        }

        /// <summary>
        /// Returns a boolean representing whether the current activity is the last one in the current mission.
        /// </summary>
        /// <returns>
        /// bool - Whether the current activity is the last in the mission.
        /// </returns>
        public bool IsLastActivity()
        {
			try
			{
				if (_currentTask == _tasks.Count)
				{
					List<string> activities = _tasks[_currentTask.ToString()];
					if(_currentActivity.ToString() == activities[activities.Count - 1])
					{
						return true;
					}                
				}

				return false;
			}
            catch (System.Exception e)
			{
				string log = "(!!!) Error Occured at ActivityTracker.cs, IsLastActivity (); Debug Info: \n" +
					"currentTask: " + _currentTask + "\n" +
					"currentActivity: " + _currentActivity + "\n" +
					"taskCount: " + _tasks.Count + "\n";

				log += "\n\n Tasks (" + ((_tasks == null) ? "null" : _tasks.Count.ToString ()) + "): \n";
				if (_tasks != null)
					foreach (KeyValuePair<string, List<string>> entry in _tasks)
					{
						log += entry.Key + " : ";
						foreach (string str in entry.Value)
							log += str + " | ";
						log += "\n";
					}
				log += "\n\n" + e;
				
				Debug.LogError (log);
				return true;
			}
        }
    }
}
