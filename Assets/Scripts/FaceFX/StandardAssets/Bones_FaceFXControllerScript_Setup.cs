//----------------------------------------------------------------------------------------------------------------------------
// A sample FaceFX Controller script for a bones-based character using the 
// default FaceFX setup.  The logic is in FaceFXControllerScript_Base.
//
// Edit this file with your target names and attach it to your character.
//----------------------------------------------------------------------------------------------------------------------------
using UnityEngine;
using System.Collections;

public class Bones_FaceFXControllerScript_Setup : FaceFXControllerScript_Base
{

		// If you need to add a new target, you must add it in 3 places:
		
		// 1.
		public float open = 0; 
		public float W = 0; 
		public float ShCh = 0; 
		public float FV = 0;
		public float PBM = 0; 
		public float wide = 0; 

		public float tBack = 0;
		public float tTeeth = 0; 
		public float tRoof = 0; 
	
		public float Eyebrow_Raise = 0; // Convert spaces to underscores.
		public float Blink = 0; 
		public float Squint = 0; 

		// The head needs to be controlled by mecanim for humanoid characters.
		// Use a second head bone if you want FaceFX to control the head too.
		public float Head_Pitch_Pos = 0; 
		public float Head_Pitch_Neg = 0; 
		public float Head_Yaw_Pos = 0;
		public float Head_Yaw_Neg = 0;
		public float Head_Roll_Pos = 0;
		public float Head_Roll_Neg = 0;
	
		public float Eyes_Yaw_Neg = 0;
		public float Eyes_Yaw_Pos = 0;
		public float Eyes_Pitch_Pos = 0;
		public float Eyes_Pitch_Neg = 0;

		//public float Mouth_Sad = 0;
		//public float Mouth_Anger = 0;
		//public float Mouth_Happy = 0;
		//public float Mouth_Snarl = 0;
		//public float Mouth_Pain = 0;
		//public float Mouth_Pout = 0;
		//public float Brows_Sad = 0;
		//public float Brows_Anger = 0;
		//public float Brows_Happy = 0;
		//public float Brows_Pain = 0;
	
		// 2..	
		protected override ArrayList GetBonePoseArrayList ()
		{

				ArrayList bonePoseAnims = new ArrayList ();
				bonePoseAnims.Add ("facefx open");
				bonePoseAnims.Add ("facefx W");
				bonePoseAnims.Add ("facefx PBM");
				bonePoseAnims.Add ("facefx FV");
				bonePoseAnims.Add ("facefx ShCh");
				bonePoseAnims.Add ("facefx wide");
				bonePoseAnims.Add ("facefx tBack");
				bonePoseAnims.Add ("facefx tRoof");	
				bonePoseAnims.Add ("facefx tTeeth");
				bonePoseAnims.Add ("facefx Eyebrow Raise");
				bonePoseAnims.Add ("facefx Blink");
				bonePoseAnims.Add ("facefx Squint");
			
				bonePoseAnims.Add ("facefx Head_Pitch_Pos");
				bonePoseAnims.Add ("facefx Head_Pitch_Neg");
				bonePoseAnims.Add ("facefx Head_Yaw_Pos");
				bonePoseAnims.Add ("facefx Head_Yaw_Neg");
				bonePoseAnims.Add ("facefx Head_Roll_Pos");
				bonePoseAnims.Add ("facefx Head_Roll_Neg");
	
				bonePoseAnims.Add ("facefx Eyes_Yaw_Neg");
				bonePoseAnims.Add ("facefx Eyes_Yaw_Pos");
				bonePoseAnims.Add ("facefx Eyes_Pitch_Pos");
				bonePoseAnims.Add ("facefx Eyes_Pitch_Neg");
						
				return 	bonePoseAnims;	
					
		}
	
		// 3..
		protected override void playPoseAnims ()
		{
				playPoseAnim ("facefx open", open);
				playPoseAnim ("facefx W", W);
				playPoseAnim ("facefx PBM", PBM);
				playPoseAnim ("facefx FV", FV);
				playPoseAnim ("facefx ShCh", ShCh);
				playPoseAnim ("facefx wide", wide);
				playPoseAnim ("facefx tBack", tBack);
				playPoseAnim ("facefx tRoof", tRoof);	
				playPoseAnim ("facefx tTeeth", tTeeth);
		
				playPoseAnim ("facefx Eyebrow Raise", Eyebrow_Raise);
				playPoseAnim ("facefx Blink", Blink);
				playPoseAnim ("facefx Squint", Squint);
		
				playPoseAnim ("facefx Head_Pitch_Pos", Head_Pitch_Pos);
				playPoseAnim ("facefx Head_Pitch_Neg", Head_Pitch_Neg);
				playPoseAnim ("facefx Head_Yaw_Pos", Head_Yaw_Pos);
				playPoseAnim ("facefx Head_Yaw_Neg", Head_Yaw_Neg);
				playPoseAnim ("facefx Head_Roll_Pos", Head_Roll_Pos);
				playPoseAnim ("facefx Head_Roll_Neg", Head_Roll_Neg);
		
				playPoseAnim ("facefx Eyes_Yaw_Neg", Eyes_Yaw_Neg);
				playPoseAnim ("facefx Eyes_Yaw_Pos", Eyes_Yaw_Pos);
				playPoseAnim ("facefx Eyes_Pitch_Pos", Eyes_Pitch_Pos);
				playPoseAnim ("facefx Eyes_Pitch_Neg", Eyes_Pitch_Neg);
		
				//playPoseAnim("facefx Mouth_Sad", Mouth_Sad);
				//playPoseAnim("facefx Mouth_Anger", Mouth_Anger);
				//playPoseAnim("facefx Mouth_Happy", Mouth_Happy);
				//playPoseAnim("facefx Mouth_Snarl", Mouth_Snarl);
				//playPoseAnim("facefx Mouth_Pain", Mouth_Pain);
				//playPoseAnim("facefx Mouth_Pout", Mouth_Pout);
				//playPoseAnim("facefx Brows_Sad", Brows_Sad);
				//playPoseAnim("facefx Brows_Anger", Brows_Anger);
				//playPoseAnim("facefx Brows_Happy", Brows_Happy);
				//playPoseAnim("facefx Brows_Pain", Brows_Pain);

		}

}


