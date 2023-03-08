//----------------------------------------------------------------------------------------------------------------------------
// A FaceFX Controller script to drive bones or morph-based animations created with the FaceFXControllerScriptGUI.cs file
//
// Owner: Doug Perkowski
//----------------------------------------------------------------------------------------------------------------------------
//  License Information
//
// Copyright (c) 2002-2014 OC3 Entertainment, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The person or entity using the Software must first obtain a commercial license to FaceFX Studio Professional or
// FaceFX Unlimited
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of
// the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//----------------------------------------------------------------------------------------------------------------------------

// Mecanim could be used to drive body animations along with the morph and bone poses.  The 
// only conflict would be the head bone which needs to be controlled by mecanim, but is used here to 
// rotate the head realistically with the speech.


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Animation))]
[RequireComponent(typeof(AudioSource))]

public class FaceFXControllerScript_Base : MonoBehaviour
{

		public bool debug = false;
	
	
		protected Animation animationComponent;
		protected AudioSource audioComponent;
	
		// We are initialized on startup, but this prevents us from trying to play
		// an animation before initalization.
		private bool bIsInitialized = false;
	
		// This variable is used to store the audio start time of the animation.
		public float audio_start_time = 0;
	
	
		// Import XML Variables
		// The ScaleFactor should match the scale of your FBX scaling.
		public float ScaleFactor = .01f;
	
		// This string will be appended to the FaceFX animation name to create a new event
		// track animation, but only if there are FaceFX events with non-empty "payload" values.
		// Unity events can't be inserted into the main FaceFX animation because this animation is
		// force-ticked by this script and the unity events are not fired.
		public string EVENT_TRACK_NAME = "_eventtrack";
	
	
		//------------------------------------------
	
		// The audio_clip to play associated with animation_name.  The audio start time is recorded for each animation in laudioStartTime var.
		// Once playing, the audio determines the evaluation time of animation_name to prevent the
		// audio from getting out of synch with the animation.
		private AudioClip audio_clip;
	
		// The name of the animation.  This animation should not move bones directly, but rather drive the children of the facefx_controller.
		private string animation_name;
	
		// This is the layer that the addative bone pose animations are played on.
		const int FACEFX_BONEPOSE_LAYER = 10;
		// This layer drives the FaceFX Controller Script variables
		const int FACEFX_ANIMATION_LAYER = 9;
		// Set the FACEFX_REFERENCEPOSE_LAYER above the layer of your skeleton animation
		// to make facefx overwrite the bone transforms (if your full body animation has
		// facial animation, but you want to ignore it in favor of FaceFX output.)
		const int FACEFX_REFERENCEPOSE_LAYER = 7;
	
	
		// READY - Not playing / Ready to play.
		// PRE_AUDIO - playing animation prior to audio.
		// AUDIO_SYNC - playing with audio.
		// POST_AUDIO - playing after audio ends.
		private enum play_state_enum
		{
				READY,
				PRE_AUDIO,
				AUDIO_SYNC,
				POST_AUDIO}
		;
		private play_state_enum prev_play_state;
		private play_state_enum play_state;
		private play_state_enum onpause_play_state;

	// We don't want to evaluate animations beyond their end time, so we use this to cache the animation evaluation time.
	private float anim_eval_time;
	
		// an inverse_hermite curve is computed and cached because of the way bone poses need to be driven within unity's animation system.
		private static AnimationCurve inverse_hermite;
	
		public class MorphTargetManager
		{
				public Dictionary<string,MorphTargetLinker> morphTargetDictionary;
				public struct MorphTargetLinker
				{
						public SkinnedMeshRenderer smr;	
						public int index;
						public MorphTargetLinker (SkinnedMeshRenderer renderer, int i)
						{
								smr = renderer;
								index = i;
						}
				}
				public MorphTargetManager ()
				{
						morphTargetDictionary = new Dictionary<string,MorphTargetLinker> ();
				}
				public void AddMorphTarget (string unityBlendShapeName, SkinnedMeshRenderer smr, int i)
				{
						morphTargetDictionary.Add (unityBlendShapeName, new MorphTargetLinker (smr, i));
            UnityEngine.Debug.Log ("Adding " + unityBlendShapeName + " morph target from Unity content.");
				}
				public void SetMorphValue (string unityBlendShapeName, float value)
				{
			
						if (morphTargetDictionary.ContainsKey (unityBlendShapeName)) {	
								MorphTargetLinker mtl = morphTargetDictionary [unityBlendShapeName];
								mtl.smr.SetBlendShapeWeight (mtl.index, value * 100.0f);
						} else {
                UnityEngine.Debug.LogWarning ("Attempted to drive " + unityBlendShapeName + " morph target before adding it. Make sure your controller script has the correct names listed");
						}
				}
		}
		protected MorphTargetManager morphManager;
		
	
		// This should be overridden in the derived FaceFX Controller Script class that is 
		// attached to your object.
		protected virtual ArrayList GetBonePoseArrayList ()
		{
				return new ArrayList ();
		}
		// This should be overridden in the derived FaceFX Controller Script class that is 
		// attached to your object.
		protected virtual ArrayList GetMorphArrayList ()
		{
				return new ArrayList ();
		}
		protected virtual void playPoseAnims ()
		{
		}
		protected virtual void playMorphAnims ()
		{
		}		
	
		// If you deactivate the game object, then reactivate it, the loop animation 
		// stops playing.  Detect this state here, and fix it up.
			
		private void detectAndFixDeactivatedGameObject ()
		{
				// If you deactivate the game object, then reactivate it, the loop animation 
				// stops playing.  Detect this state here, and fix it up.
				if (!animationComponent.IsPlaying ("facefx_loop_anim")) {
						AnimationState loopAnim = animationComponent ["facefx_loop_anim"];
						if (loopAnim) {
								loopAnim.layer = FACEFX_REFERENCEPOSE_LAYER;
								loopAnim.wrapMode = WrapMode.ClampForever;
								animationComponent.Play ("facefx_loop_anim");
						}
						ArrayList bonePoseAnims = GetBonePoseArrayList ();
		
						foreach (string bonepose in bonePoseAnims) {
								AnimationState bonePoseAnim = animationComponent [bonepose];
								if (bonePoseAnim != null) {
										bonePoseAnim.enabled = true;
										bonePoseAnim.weight = 1;
								}			
						}	
				}		
		}


	

		protected void initializeAnims ()
		{
				ArrayList bonePoseAnims = GetBonePoseArrayList ();
		
				ArrayList succesfulInits = new ArrayList ();
				ArrayList unsuccesfulInits = new ArrayList ();
				for (int i = 0; i < bonePoseAnims.Count; ++i) {
						if (initializeAnim (bonePoseAnims [i] as string)) {
								succesfulInits.Add (bonePoseAnims [i] as string);
						} else {
								unsuccesfulInits.Add (bonePoseAnims [i] as string);
						}
				}
				if (succesfulInits.Count == 0) {
            UnityEngine.Debug.LogWarning ("No bone pose animations were found on the character. Did you import an XML?  Have the animatiuons been deleted reverting to the prefab");
				}
				if (unsuccesfulInits.Count > 0) {
						string warning = "The following " + unsuccesfulInits.Count + " pose animations were not present in the Animation component:";
						for (int i = 0; i < unsuccesfulInits.Count; ++i) {
								warning = warning + " \n" + unsuccesfulInits [i];
						}
						warning = warning + "\n Animation data on these channels will have no effect.";
            UnityEngine.Debug.LogWarning (warning);
				}
		}

		void Start ()
		{
				if (!bIsInitialized) {
						initalizeController ();
				}
		}	
		private void initializeMorphs ()
		{ 
				morphManager = new MorphTargetManager ();
				Component[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer> ();
				foreach (SkinnedMeshRenderer smr in skinnedMeshRenderers) {
						Mesh m = smr.sharedMesh;
						for (int i = 0; i < m.blendShapeCount; ++i) {
								morphManager.AddMorphTarget (m.GetBlendShapeName (i), smr, i);
						}
				}	
		
		}
		private void initalizeController ()
		{
				bIsInitialized = true;
        if (debug)
        {
			UnityEngine.Debug.Log ("Initializing FaceFX Controller.");
        }
				play_state = play_state_enum.READY;
				animationComponent = GetComponent<Animation> ();
				audioComponent = GetComponent<AudioSource> ();
				if (inverse_hermite == null) {
						inverse_hermite = new AnimationCurve ();
						AnimationCurve hermiteCurve = new AnimationCurve (new Keyframe (0, 0), new Keyframe (1, 1));

						// The "step" here defines how accurate the inverse_hermite curve is.
						for (float i = 0; i <= 1; i = i + (float).01) {
								inverse_hermite.AddKey (hermiteCurve.Evaluate (i), i);
						}
				}
		
				initializeAnims ();
				initializeMorphs ();
		
				// The loop anim is created from the XML import.  We need a non-additive animation to play,
				// And it just has the reference pose.
				AnimationState loopAnim = animationComponent ["facefx_loop_anim"];
				if (loopAnim != null) {
						loopAnim.layer = FACEFX_REFERENCEPOSE_LAYER;
						loopAnim.wrapMode = WrapMode.ClampForever;
						animationComponent.Play ("facefx_loop_anim");
				} else {
            UnityEngine.Debug.LogError ("No facefx_loop_anim animation found for " + name + ".  Did you forget to import a FaceFX XML file?");
				}		
		}
	
		private bool initializeAnim (string animName)
		{

				AnimationState bonePoseAnim = animationComponent [animName];
				if (bonePoseAnim != null) {
						// Keep bone pose animations in their own layer with additive blending
						// and ClampForever wrapping.  Enable them and set the weight to 1.
						// We are then prepared to manually adjust the "time" of the animation
						// in the Update function to control the amount the bone pose is blended in.
						bonePoseAnim.layer = FACEFX_BONEPOSE_LAYER;
						bonePoseAnim.blendMode = AnimationBlendMode.Additive;
						bonePoseAnim.wrapMode = WrapMode.ClampForever;
						bonePoseAnim.enabled = true;
						bonePoseAnim.weight = 1;
			
						bonePoseAnim.normalizedSpeed = 0;
						return true;
				}
				return false;
		}


		// Stops animation and audio from playing.  Resets the states so animations can be
		// played again from the start.
		public void StopAnim ()
		{
				// If we have stopped this animation prematurely, we need to stop the aduio.
				audioComponent.Stop ();
				// Setting this to 0 means we are ready to play another animation.
				play_state = play_state_enum.READY;
		}

		// Facial animations  frequently start before the corresponding audio becuase the mouth needs to
		// move into the correct position to form the first sound.  
		public void PlayAudioFunction ()
		{
				if (audioComponent.isPlaying) {
            UnityEngine.Debug.Log ("Audio is already playing!");
				} else {
						audioComponent.clip = audio_clip;
						audioComponent.Play ();
				}
		}
		private IEnumerator PlayAnimCoroutine (string animName, AudioClip animAudio)
		{
				if (!bIsInitialized) {
						initalizeController ();
				}		
				anim_eval_time = 0;
				audio_start_time = 0;
				if (null == animAudio) {
            UnityEngine.Debug.Log ("Audio is null!");
				}
				animation_name = animName;
				audio_clip = animAudio;

				if (animName != null) {

						AnimationState animState = animationComponent [animation_name];
						if (animState != null) {
                if (debug)
                {
					UnityEngine.Debug.Log ("playing anim " + animName);
                }
								animState.speed = 0;
								animState.time = 0;
								animState.layer = FACEFX_ANIMATION_LAYER;
								animationComponent.Play (animName);
				
								// The event track stores things like events that trigger mecanim animations.
								if (animationComponent [animation_name + EVENT_TRACK_NAME] != null) {
										animationComponent.Play (animation_name + EVENT_TRACK_NAME);
								}
				
								// yeild one frame to let the audio_start_time variable update.
								yield return null;
								play_state = play_state_enum.PRE_AUDIO;

						} else {
                UnityEngine.Debug.Log ("No AnimationState for animation:" + animation_name + " on player " + name);
						}
				} else {
            UnityEngine.Debug.Log ("No animation passed into PlayAnim.  Playing audio.");
						PlayAudioFunction ();
				}				
		}
	
		// An animation name and an audio clip are passed to the  function
		// to start playing a facial animation.
		public void PlayAnim (string animName, AudioClip animAudio)
		{
				StartCoroutine (PlayAnimCoroutine (animName, animAudio));
		}

		public void PauseAnim()
		{
			if (audioComponent != null)
			{
				audioComponent.Pause();
			}

			if (animationComponent != null)
			{
				animationComponent[animation_name].enabled = false;
				onpause_play_state = play_state;
				play_state = play_state_enum.READY;
			}
		}

		public void ResumeAnim()
		{
			if (audioComponent != null)
			{
				audioComponent.UnPause();
			}

			if (animationComponent != null)
			{
				animationComponent[animation_name].enabled = true;
				play_state = onpause_play_state;
			}
		}

	public void Update ()
		{
				if (play_state > 0) {
						AnimationState animState = animationComponent [animation_name];
						if (animState != null) {
								// We calculate the animation evaluation time here.  It is overridden by the audio-based time if audio is playing.
								anim_eval_time = anim_eval_time + Time.deltaTime;

								if (play_state == play_state_enum.PRE_AUDIO) {
										if (animState.time >= audio_start_time) {
												PlayAudioFunction ();
												play_state = play_state_enum.AUDIO_SYNC;
										}
								}
								if (play_state == play_state_enum.AUDIO_SYNC) {
										// audio.isPlaying is not a reliable test alone because audio stops when you loose focus.
										// But without it, the audio.time can reset to 0 when audio is finished.
										if (audioComponent.isPlaying && audioComponent.time < audio_clip.length) {
												// While audio is playing, assume control of animation playback and force synch it to the audio.
												anim_eval_time = audioComponent.time + audio_start_time;
										} else if (!audioComponent.isPlaying) {						
												play_state = play_state_enum.POST_AUDIO;
										}
								}
								if (play_state == play_state_enum.POST_AUDIO) {
										if (anim_eval_time >= animState.length) {
												play_state = play_state_enum.READY;
										}
								}
								// Only "tick" the animation if it wouldn't put us over the animation bounds.
								if (anim_eval_time <= animState.length) {
										animState.time = anim_eval_time;
								}
		
								detectAndFixDeactivatedGameObject ();
								playPoseAnims ();
								playMorphAnims ();
				
						}
						// To support audio playback without animation, reset the state if the animation is 
						// null and the audio is finished playing.
						//else if( !audioComponent.isPlaying )
						//{
						//	Debug.Log("audio with no animation case");
						//	play_state = play_state_enum.READY;
						//}
				}
		}
	
		protected void playPoseAnim (string animName, float val)
		{
				AnimationState anim = animationComponent [animName];
				if (anim != null) {
						//The normalized time of the animation is from 0-1.
						//At 1, the bone pose is fully driven.  At 0, it is the reference pose.
						// Unfortunately, the interpolation from 0-1 is a hermite curve, not linear. So we use the inverse_hermite
						// curve to figure out what value we need to pass into the hermite curve evaluation to drive the bone pose.
						anim.normalizedTime = inverse_hermite.Evaluate (val);

						// Remove shaking by setting normalized speed to 0.
						anim.normalizedSpeed = 0;
				}
		}

		public int GetPlayState ()
		{
				return (int)play_state;
		}

}


