using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CoreSystem;
using System.Collections.Generic;
using HighlightingSystem;

namespace CustomTracks
{
	[TrackColor(0.5176471f, 0.1607843f, 0.1882353f)]
	[TrackClipType(typeof(ActivityEventClip))]
	public class ActivityEventTrack : TrackAsset
	{
		public int missionid = -1;
		public int scenarioid = -1;
		public int courseid = -1;
		public int levelid = -1;

		public bool minigames_active = true;

		private bool HasBeenFired = false;

		public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
		{
			if (Application.isPlaying)
            {
				HasBeenFired = false;
				FireEvent();
            }

			return ScriptPlayable<ActivityEventBehaviour>.Create(graph, inputCount);
		}

		public void FireEvent()
		{
			//Debug.Log("Has Fired : " + HasBeenFired);
			float[] map_values;

#if CLIENT_BUILD
		map_values = new float[]{1, 1, ActivityTracker.Instance.Mission, 
			ActivityTracker.Instance.Scenario, ActivityTracker.Instance.Course, ActivityTracker.Instance.Level };
#else
			map_values = new float[] { 1, 1, missionid, scenarioid, courseid, levelid };
#endif
			int taskBeforeResetID = (int)ActivityTracker.Instance.Task;
			int activityBeforeResetID = (int)ActivityTracker.Instance.Activity;

			if (HasBeenFired)
			{
				ActivityTracker.Instance.SetUp(map_values);
			}
			else
			{
				CoreEventSystem.Instance.SendEvent(CoreEventTypes.MISSION_SETUP);
				CoreEventSystem.Instance.SendEvent(CoreEventTypes.MISSION_START);//, Sequence);

				HasBeenFired = true;

				ActivityTracker.Instance.SetUp(map_values);

				SetUpActivityChangeEvents(map_values);

				ActivityTracker.Instance.ChangeProgress(map_values);

				MiniGameManager.Instance.SetEnabled(minigames_active);


				if (!ObjectStateManager.Instance.HasBeenCached())
				{
					ObjectStateManager.Instance.CacheObjectInformation();
				}

#if CLIENT_BUILD
			//Clear background layer on mission change
			LayerSystem.Instance.ClearLayer (UILayers.Background.ToString ());
#endif

				//If chosen task from learning map then jump straight to that task
				//if(taskBeforeResetID != 1)
				{
					SequenceManager.Instance.SkipToActivity(taskBeforeResetID, activityBeforeResetID);
				}

                //Inject glow material scripts onto camera
                if ((Camera.main.GetComponent<HighlightingRenderer>()) == null)
                {
                    Camera.main.gameObject.AddComponent<HighlightingRenderer>();
                }

                HighlightingRenderer renderer = Camera.main.gameObject.GetComponent<HighlightingRenderer>();
                Vector2 idealSize = new Vector2(2048, 1536);
                Vector2 currentSize = new Vector2(Screen.width, Screen.height);
                float widthRatio = currentSize.x / idealSize.x;
                float heightRatio = currentSize.y / idealSize.y;
                float ratio = Mathf.Max(widthRatio, heightRatio);

                renderer.iterations = 2;
                renderer.blurMinSpread = 0.75f * ratio;
                renderer.blurSpread = 1.3f * ratio;
                renderer._blurIntensity = 0.3f * ratio;
            }
		}

		private void SetUpActivityChangeEvents(float[] map_values)
		{
			/*
			 * Gore: Update all of the activity change events to know the activity they trigger.
			 */

			// Get all of the events in the time line.
			//ActivityChangeEvent[] allChangeEvents = SequenceManager.Instance.MainSequence.GetComponentsInChildren<ActivityChangeEvent>();
			//ActivityEventClip[] clips = GetClips();

			List<ActivityEventClip> allChangeEvents = new List<ActivityEventClip>();
			List<TimelineClip> allClips = new List<TimelineClip>();
			foreach (var c in GetClips())
			{
				allClips.Add(c);
			}

			allClips.Sort((x, y) => x.start.CompareTo(y.start));

			foreach (TimelineClip c in allClips)
			{
				ActivityEventClip aec = (ActivityEventClip)c.asset;
				if(aec.behaviour.callMissionEnd == false)
					allChangeEvents.Add(aec);
			}

            //ActivityChangeTriggerMG[] allTriggerEvents = SequenceManager.Instance.MainSequence.GetComponentsInChildren<ActivityChangeTriggerMG>();

            // Combine all of the events.
            //USEventBase[] AllChangeEvents = allChangeEventsAsBase.Concat(allTriggerEventsAsBase).ToArray();

            // Rewritten without Linq.


            float[] newValues = (float[])map_values.Clone();


            for (int index = 0; index < allChangeEvents.Count; ++index)
            {
                ActivityEventClip changeEvent = allChangeEvents[index];

                ActivityTracker.Instance.NextActivity();

                newValues[0] = ActivityTracker.Instance.Activity;
                newValues[1] = ActivityTracker.Instance.Task;

                if (changeEvent)
                {
                    changeEvent.behaviour.SetProgressData(newValues);
					changeEvent.behaviour.initialized = true;
                }
                //else
                //{
                //    ActivityChangeTriggerMG changeTriggerMG = AllChangeEvents[index] as ActivityChangeTriggerMG;

                //    if (changeTriggerMG)
                //    {
                //        changeTriggerMG.SetProgressData(newValues);
                //    }
                //}
            }
        }
	}
}
