using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CoreSystem;
using CustomExtensions;

namespace CustomTracks
{
	[Serializable]
	public class ActivityEventBehaviour : PlayableBehaviour, ITimelineBehaviour
	{
		[SerializeField]
		private float[] ProgressData = new float[6];

		public double startTime { get; set; }
		public double endTime { get; set; }

		[HideInInspector] public bool initialized = false;
		public bool callMissionEnd = false;
		bool isTriggered = false;

		public override void OnBehaviourPlay(Playable playable, FrameData info)
		{
#if UNITY_EDITOR
			if (!Application.isPlaying) return;
#endif

			if (callMissionEnd)
			{
				EndMission();
				return;
			}

			if (!initialized) return;
			PlayableInstance.AddPlayable(this);
			FireEvent();
		}

		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
#if UNITY_EDITOR
			if (!Application.isPlaying) return;
#endif

			if (!initialized) return;

			if (playable.isPlayableCompleted(info))
			{
				PlayableInstance.RemovePlayable(this);
				isTriggered = false;
			}
		}

		/*        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
                {
        #if UNITY_EDITOR
                    if (!Application.isPlaying) return;
        #endif

                    if (!callMissionEnd) return;

                    var t = playable.GetTime();
                    Debug.Log("Current Time : " + t);
                }*/

		public void SetProgressData(float[] map_values)
		{
			for (int index = 0; index < map_values.Length; ++index)
			{
				ProgressData[index] = map_values[index];
			}
			//Debug.Log("Setting progress value");
		}

		public float[] GetProgressData()
		{
			return ProgressData;
		}

		public void FireEvent()
		{
			isTriggered = true;
			ChangeState();
#if CLIENT_BUILD
        CoreEventSystem.Instance.ForcedListenerClear();
#endif
		}

		private void ChangeState()
		{
			bool taskChanged = false;

			if (ProgressData[1] != ActivityTracker.Instance.Task)
			{
				taskChanged = true;
			}

			ActivityTracker.Instance.ChangeProgress(ProgressData);

			if (taskChanged)
			{
				CoreEventSystem.Instance.SendEvent(CoreEventTypes.TASK_CHANGED, ActivityTracker.Instance.PlainProgressString);
			}

			CoreEventSystem.Instance.SendEvent(CoreEventTypes.ACTIVITY_CHANGED, ActivityTracker.Instance.PlainProgressString);
		}

		public void OnSkip()
		{
			ActivityTracker.Instance.ChangeProgress(ProgressData);
		}

		void EndMission()
		{
			CoreEventSystem.Instance.SendEvent(CoreEventTypes.MISSION_END_SEQUENCE);
			AmbientSoundManager ambientAudioManager = UnityEngine.Object.FindObjectOfType<AmbientSoundManager>();


			Camera[] camerasInScene = UnityEngine.Object.FindObjectsOfType<Camera>();

			foreach (Camera camera in camerasInScene)
			{
				if (camera != Camera.main)
				{
					UnityEngine.Object.Destroy(camera);
				}
			}

			if (ambientAudioManager)
			{
				ambientAudioManager.Stop();
			}
		}

		//public void Reset()
		//{

		//}
	}
}
