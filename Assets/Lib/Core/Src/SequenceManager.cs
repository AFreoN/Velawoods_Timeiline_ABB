using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace CoreSystem
{
    public class SequenceManager : MonoSingleton<SequenceManager>
    {
        private PlayableDirector _mainSequence;
		private bool _previousStateWasPlaying;

		public PlayableDirector MainSequence 
		{
			get
			{ 
				if(_mainSequence == null)
				{
					_mainSequence = GetMainSequence();

					return _mainSequence;
				}

				return _mainSequence; 
			}
		}

        protected override void Init()
        {
            _mainSequence = GetMainSequence();

			// Add a listener to handle the back button being pressed
			CoreEventSystem.Instance.AddListener (CoreEventTypes.ACTIVITY_SKIP, SkipToNextActivity );
			CoreEventSystem.Instance.AddListener (CoreEventTypes.ACTIVITY_REVERSE, SkipToPreviousActivity );
#if CLIENT_BUILD
            CoreEventSystem.Instance.AddListener (MainMenu.Messages.MENU_SHOWING, MenuActive );
			CoreEventSystem.Instance.AddListener (MainMenu.Messages.MENU_HIDING, MenuInactive );
#endif
            CoreEventSystem.Instance.AddListener (CoreEventTypes.LEVEL_CHANGE, LockOutPlay );
        }

		protected override void Dispose ()
		{
			CoreEventSystem.Instance.RemoveListener (CoreEventTypes.ACTIVITY_SKIP, SkipToNextActivity );
			CoreEventSystem.Instance.RemoveListener (CoreEventTypes.ACTIVITY_REVERSE, SkipToPreviousActivity );
#if CLIENT_BUILD
            CoreEventSystem.Instance.RemoveListener (MainMenu.Messages.MENU_SHOWING, MenuActive );
			CoreEventSystem.Instance.RemoveListener (MainMenu.Messages.MENU_HIDING, MenuInactive );
#endif
            CoreEventSystem.Instance.RemoveListener (CoreEventTypes.LEVEL_CHANGE, LockOutPlay );
			base.Dispose ();
		}

		private void MenuActive(object menuParams)
		{
			_previousStateWasPlaying = IsPlaying;
			Pause ();
		}
		
		private void MenuInactive(object menuParams = null)
		{
			bool resumeSequence = true;
			if(menuParams != null)
				resumeSequence = (bool)menuParams;

			if(_previousStateWasPlaying && resumeSequence)
			{
				Play();
			}
			else
			{
				//Was already paused when menu activated. Dont start playing.
			}
		}

		//Level changing so don't allow sequence to be played.
		private void LockOutPlay(object menuParams)
		{
			_previousStateWasPlaying = false;
		}

		public void Pause()
		{
			if (TimelineController.instance)
				TimelineController.instance.PauseTimeline();
		}

		public void Play()
		{
			if (TimelineController.instance)
				TimelineController.instance.PlayTimeline();
		}

		public bool IsPlaying
        {
            get {
				if(MainSequence != null)
					return MainSequence.playableGraph.IsPlaying();
					else
						return false;
				}
        }

		public void MissionSetUpCallback(object obj)
		{
			_mainSequence = GetMainSequence();
		}

        private PlayableDirector GetMainSequence()
        {
			GameObject sequence_obj = null;

			if(Core.Instance._sequence == null)
				sequence_obj = GameObject.Find (Core.Instance._activityTracker.SequenceString);
			else
				sequence_obj = Core.Instance._sequence;

			//Last ditch resort. Look for any sequence at all in the scene.
			if (sequence_obj == null)
			{
				PlayableDirector sequenceScript = GameObject.FindObjectOfType<PlayableDirector>();
				if(sequenceScript != null)
				{
					sequence_obj = sequenceScript.gameObject;
				}
			}

			//Failed in finding a sequence. Return.
			if(sequence_obj == null)
			{
				return null;
			}

            PlayableDirector main_sequence = sequence_obj.GetComponent<PlayableDirector>();
            
            if(main_sequence != null)
            {
                return main_sequence;
            }
            
            Debug.LogError("SequenceManager: No Main Sequence Found");
            return null;
        }



		public void SkipToBlueSequencer( object param )
		{
			// Close any current minigames.
			if (MiniGameManager.Instance.CurrentGame != null) 
			{
				MiniGameManager.Instance.CurrentGame.GetComponent<MiniGameBase> ().SendSkip ( null );
				MiniGameManager.Instance.CompletedMinigameEndTransition();
			}
			
			// Clear any queued events.
			//MainSequence.ClearAllActiveEvents();
			
			// Stop all sounds when skipping
			AudioManager.Instance.StopAudio(CoreSystem.AudioType.Dialogue);
			AudioManager.Instance.StopAudio(CoreSystem.AudioType.Music);
			AudioManager.Instance.StopAudio(CoreSystem.AudioType.SFX);

			// Stop all current facefx animations.
			foreach (Bones_FaceFXControllerScript_Setup controlScript in Object.FindObjectsOfType<Bones_FaceFXControllerScript_Setup>())
			{
				controlScript.StopAnim();
			}
			
			MainSequence.time = 0.0f;
			
			if(TimelineController.instance)
			{
				TimelineController.instance.PlayTimeline();
			}
		}


		/* Note: Skipping backwards then forwards within the same frame will cause the timeline to skip to activity 2 within the current mission.
		 * This is because running time is set instantly in skip backwards then it is flagged that USSequencer needs to skip next frame.
		 * Then skipping forwards reads a running time of 0 and jumps to activity 2. As USSequencer runs skip on a couroutine only once per frame
		 * and skipping back then forwards is impossible to a user it is left as is.
		 */

        public bool SkipToNextActity(bool getMinigameEvent)
        {
            // Only allow skip timeline if no minigame active - otherwise just skip the minigame
            if (MiniGameManager.Instance.CurrentGame != null)
            {
                MiniGameManager.Instance.CurrentGame.GetComponent<MiniGameBase>().SendSkip(null);
                MiniGameManager.Instance.CompletedMinigameEndTransition();

                // Hug: This should skip to the next activity and not just hide the mini game.
                //return;
            }

            // Stop all sounds when skipping
            AudioManager.Instance.StopAudio(CoreSystem.AudioType.Dialogue);
            AudioManager.Instance.StopAudio(CoreSystem.AudioType.Music);
            AudioManager.Instance.StopAudio(CoreSystem.AudioType.SFX);

            foreach (Bones_FaceFXControllerScript_Setup controlScript in Object.FindObjectsOfType<Bones_FaceFXControllerScript_Setup>())
            {
                controlScript.StopAnim();
            }

			float next;

            if (getMinigameEvent)
            {
                next = TimelineController.getNextMinigameTime((float)MainSequence.time);
            }
            else
            {
                next = TimelineController.getNextActivityTime((float)MainSequence.time);
            }

            if (next != -1)
            {
                SkipTo(MainSequence, next, false);

                if (TimelineController.instance)
                {
                    TimelineController.instance.PlayTimeline();
                }

                return true;
            }

            return false;
        }

		
		public void SkipToNextActivity( object param )
		{
            SkipToNextActity(false);
		}
	
		public void SkipToPreviousActivity( object param )
        {    
			// Only allow skip timeline if no minigame active - otherwise just skip the minigame
			if (MiniGameManager.Instance.CurrentGame != null) {
				MiniGameManager.Instance.CurrentGame.GetComponent<MiniGameBase> ().SendSkip ( null );
				MiniGameManager.Instance.CompletedMinigameEndTransition();
				// Hug: This should skip to the previous activity and not just hide the mini game.
				//return;
			}
            
			float currentRunningTime = (float)MainSequence.time;

			float skipToTime = TimelineController.getPreviousActivityTime(currentRunningTime);

            // Stop all sounds when skipping
            AudioManager.Instance.StopAudio(CoreSystem.AudioType.Dialogue);
			AudioManager.Instance.StopAudio(CoreSystem.AudioType.Music);
			AudioManager.Instance.StopAudio(CoreSystem.AudioType.SFX);

            foreach (Bones_FaceFXControllerScript_Setup controlScript in Object.FindObjectsOfType<Bones_FaceFXControllerScript_Setup>())
            {
                controlScript.StopAnim();
            }

            SkipTo(MainSequence, skipToTime, false);

            if (TimelineController.instance)
				TimelineController.instance.PlayTimeline();

        }

        public void SkipToTask(float skipTaskID)
        {
            float skipTime = GetTimeTaskStarts(skipTaskID);
            SkipTo(MainSequence, skipTime, false);

            if (TimelineController.instance)
            {
                TimelineController.instance.PlayTimeline();
            }
        }

        public void SkipToActivity(float skipTaskID, float activityID)
        {
            float skipTime = GetTimeTaskStarts(skipTaskID, activityID);
            SkipTo(MainSequence, skipTime, false);

            if (TimelineController.instance)
            {
                TimelineController.instance.PlayTimeline();
            }
        }

        private float GetTimeTaskStarts(float skipTaskID, float skipActivityID = 1.0f)
		{
			List<(CustomTracks.ActivityEventClip, float, float)> allClips = TimelineController.getAllClipsFromTrack<CustomTracks.ActivityEventClip>(TimelineController.TRACK_ACTIVITY);

			foreach(var v in allClips)
            {
				float taskID = v.Item1.behaviour.GetProgressData()[1];
				float activityID = v.Item1.behaviour.GetProgressData()[0];

				if (taskID == skipTaskID && activityID == skipActivityID)
					return v.Item2;
            }

			return 0;
		}

		private bool CheckIfAlreadyAtThisTask(float[] first, float[] second)
		{
			return first[0] == second[5] && first[1] == second[4] && first[2] == second[3] && first[3] == second[2] && 
				first[4] == second[1] && first[5] == second[0];
		}

        public void SkipTo(PlayableDirector sequence, float skipTime, bool forceProcessEvents)
        {
            // Find all animator and store the current culling modes and set it to always animate.
            Dictionary<Animator, AnimatorCullingMode> animatorCullingModes = new Dictionary<Animator, AnimatorCullingMode>();
            foreach (Animator animator in GameObject.FindObjectsOfType<Animator>())
            {
                animatorCullingModes.Add(animator, animator.cullingMode);
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }

            bool shouldResetScene = sequence == _mainSequence;

            // Reset all the events that have happened in the time line.
			//PlayableInstance.ResetAll();

            // Restore the state of each object. 
            if (shouldResetScene && ObjectStateManager.Instance.HasBeenCached())
            {
                ObjectStateManager.Instance.RestoreGameObjects();
            }

            // Clear any events that have been queued up to fire.

            // Skip to before the dialogue event.
			if(TimelineController.instance)
				TimelineController.instance.SkipTimeline(skipTime);

            if (shouldResetScene)
            {
                // Cause the animators to update so that the last target is set in the IK script. 
                foreach (Animator a in GameObject.FindObjectsOfType<Animator>())
                {
                    a.Update(0.01f);
                }

                AmbientSoundManager soundManager = GameObject.FindObjectOfType<AmbientSoundManager>();

                if (soundManager)
                {
                    soundManager.Reset();
                }
            }

            // Revert animator culling modes.
            foreach (KeyValuePair<Animator, AnimatorCullingMode> animatorCullingMode in animatorCullingModes)
            {
                // Check if it is still present
                if (animatorCullingMode.Key != null)
                {
                    animatorCullingMode.Key.cullingMode = animatorCullingMode.Value;
                }
            }
        }
    }
}
