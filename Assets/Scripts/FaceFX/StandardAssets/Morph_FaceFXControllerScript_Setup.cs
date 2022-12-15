//----------------------------------------------------------------------------------------------------------------------------
// A sample FaceFX Controller script for a morph-based character using the 
// default FaceFX setup.  The logic is in FaceFXControllerScript_Base.
//
// Edit this file with your target names and attach it to your character.
//----------------------------------------------------------------------------------------------------------------------------

// This script drives morph targets in Unity together with bone poses.  The morph names match
// An Autodesk Character Generator character of medium resolution with the facial morph rig.
// The head and eye bones are examples of FaceFX bone poses being driven using Unity's additive
// pose animation.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Morph_FaceFXControllerScript_Setup : FaceFXControllerScript_Base
{
		//Bone Poses listed below
		public float Eyes_Yaw_Neg = 0;
		public float Eyes_Yaw_Pos = 0;
		public float Eyes_Pitch_Pos = 0;
		public float Eyes_Pitch_Neg = 0;
		public float Head_Yaw_Neg = 0;  //The head would need to be controlled by mecanim if we were using it.
		public float Head_Yaw_Pos = 0;
		public float Head_Pitch_Pos = 0;
		public float Head_Pitch_Neg = 0;
		public float Head_Roll_Pos = 0;
		public float Head_Roll_Neg = 0;	
	
		// Morph targets listed below;
		public float m_expressions_AE_AA_m0 = 0;
		public float m_expressions_AO_a_m0 = 0;
		public float m_expressions_Ax_E_m0 = 0;
		public float m_expressions_Chew_m0 = 0;
		public float m_expressions_Chin_m0 = 0;
		public float m_expressions_FV_m0 = 0;
		public float m_expressions_Glotis_m0 = 0;
		public float m_expressions_H_EST_m0 = 0;
		public float m_expressions_JawCompress_m0 = 0;
		public float m_expressions_JawFront_m0 = 0;
		public float m_expressions_KG_m0 = 0;
		public float m_expressions_Kiss_m0 = 0;
		public float m_expressions_Lblow_m0 = 0;
		public float m_expressions_LbrowDown_m0 = 0;
		public float m_expressions_LbrowUp_m0 = 0;
		public float m_expressions_Ldisgust_m0 = 0;
		public float m_expressions_LeyeClose_m0 = 0;
		public float m_expressions_LeyeOpen_m0 = 0;
		public float m_expressions_Ljaw_m0 = 0;
		public float m_expressions_LLbrowDown_m0 = 0;
		public float m_expressions_LLbrowUp_m0 = 0;
		public float m_expressions_LlipCorner_m0 = 0;
		public float m_expressions_LlipDown_m0 = 0;
		public float m_expressions_LlipSide_m0 = 0;
		public float m_expressions_LlipUp_m0 = 0;
		public float m_expressions_LlowLid_m0 = 0;
		public float m_expressions_LmouthSad_m0 = 0;
		public float m_expressions_LneckTension_m0 = 0;
		public float m_expressions_Lnostril_m0 = 0;
		public float m_expressions_Lpityful_m0 = 0;
		public float m_expressions_Lsad_m0 = 0;
		public float m_expressions_LsmileClose_m0 = 0;
		public float m_expressions_LsmileOpen_m0 = 0;
		public float m_expressions_Lsquint_m0 = 0;
		public float m_expressions_MouthOpen_m0 = 0;
		public float m_expressions_MPB_Down_m0 = 0;
		public float m_expressions_MPB_Up_m0 = 0;
		public float m_expressions_Rblow_m0 = 0;
		public float m_expressions_RbrowDown_m0 = 0;
		public float m_expressions_RbrowUp_m0 = 0;
		public float m_expressions_Rdisgust_m0 = 0;
		public float m_expressions_ReyeClose_m0 = 0;
		public float m_expressions_ReyeOpen_m0 = 0;
		public float m_expressions_Rjaw_m0 = 0;
		public float m_expressions_RlipCorner_m0 = 0;
		public float m_expressions_RlipDown_m0 = 0;
		public float m_expressions_RlipSide_m0 = 0;
		public float m_expressions_RlipUp_m0 = 0;
		public float m_expressions_RlowLid_m0 = 0;
		public float m_expressions_RmouthSad_m0 = 0;
		public float m_expressions_RneckTension_m0 = 0;
		public float m_expressions_Rnostril_m0 = 0;
		public float m_expressions_Rpityful_m0 = 0;
		public float m_expressions_RRbrowDown_m0 = 0;
		public float m_expressions_RRbrowUp_m0 = 0;
		public float m_expressions_Rsad_m0 = 0;
		public float m_expressions_RsmileClose_m0 = 0;
		public float m_expressions_RsmileOpen_m0 = 0;
		public float m_expressions_Rsquint_m0 = 0;
		public float m_expressions_S_m0 = 0;
		public float m_expressions_SH_CH_m0 = 0;
		public float m_expressions_Shout_m0 = 0;
		public float m_expressions_TD_I_m0 = 0;
		public float m_expressions_UH_OO_m0 = 0;
		public float m_expressions_UW_U_m0 = 0;	
		public float m_teeth_Compress_tg_m0 = 0;
		public float m_teeth_CurlDown_Out_tg_m0 = 0;
		public float m_teeth_CurlLeft_Out_tg_m0 = 0;
		public float m_teeth_CurlRight_Out_tg_m0 = 0;
		public float m_teeth_CurlUp_Out_tg_m0 = 0;
		public float m_teeth_Left_In_tg_m0 = 0;
		public float m_teeth_LLL_In_tg_m0 = 0;
		public float m_teeth_OutMiddle_tg_m0 = 0;
		public float m_teeth_Right_In_tg_m0 = 0;
		public float m_teeth_RRR_In_tg_m0 = 0;
		public float m_teeth_t_AE_AA_m0 = 0;
		public float m_teeth_t_AO_a_m0 = 0;
		public float m_teeth_t_Ax_E_m0 = 0;
		public float m_teeth_t_Chew_m0 = 0;
		public float m_teeth_t_FV_m0 = 0;
		public float m_teeth_t_H_EST_m0 = 0;
		public float m_teeth_t_JawCompress_m0 = 0;
		public float m_teeth_t_JawFront_m0 = 0;
		public float m_teeth_t_KG_m0 = 0;
		public float m_teeth_t_Ljaw_m0 = 0;
		public float m_teeth_t_MouthOpen_m0 = 0;
		public float m_teeth_t_MPB_m0 = 0;
		public float m_teeth_t_Rjaw_m0 = 0;
		public float m_teeth_t_S_m0 = 0;
		public float m_teeth_t_SH_CH_m0 = 0;
		public float m_teeth_t_Shout_m0 = 0;
		public float m_teeth_t_TD_I_m0 = 0;
		public float m_teeth_t_UH_OO_m0 = 0;
		public float m_teeth_t_UW_U_m0 = 0;
		public float m_teeth_Throat_In_tg_m0 = 0;
		public float m_teeth_Up_tg_m0 = 0;
		
		protected override ArrayList GetBonePoseArrayList ()
		{
		
				ArrayList bonePoseAnims = new ArrayList ();
		
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
		// Update Bone Poses here (2nd location)
		protected override void playPoseAnims ()
		{
				playPoseAnim ("facefx Eyes_Yaw_Neg", Eyes_Yaw_Neg);
				playPoseAnim ("facefx Eyes_Yaw_Pos", Eyes_Yaw_Pos);
				playPoseAnim ("facefx Eyes_Pitch_Pos", Eyes_Pitch_Pos);
				playPoseAnim ("facefx Eyes_Pitch_Neg", Eyes_Pitch_Neg);
				playPoseAnim ("facefx Head_Yaw_Neg", Head_Yaw_Neg);
				playPoseAnim ("facefx Head_Yaw_Pos", Head_Yaw_Pos);
				playPoseAnim ("facefx Head_Pitch_Pos", Head_Pitch_Pos);
				playPoseAnim ("facefx Head_Pitch_Neg", Head_Pitch_Neg);
				playPoseAnim ("facefx Head_Roll_Neg", Head_Roll_Neg);
				playPoseAnim ("facefx Head_Roll_Pos", Head_Roll_Pos);			
		}
	
		// Update Morph targets (2nd and final location)
		protected override void playMorphAnims ()
		{
				// Here we link up the name of a morph target in Unity with the name of the variable
				// at the top of the script that will drive it.  The variables at the top of the script
				// are derived from FaceFX morph target node names, with periods and spaces converted to 
				// underscores. 
				// Also note that there may be subtle differences between a FaceFX morph target name, and the 
				// morph target name in Unity.  
	
				// Note: Many morphs are commented out because FaceFX does not drive them by default.
				// This function will be called every frame.
	
				//morphManager.SetMorphValue("m_expressions.m_expressions_AE_AA_m0", 	m_expressions_AE_AA_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_AO_a_m0", 	m_expressions_AO_a_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_Ax_E_m0", 	m_expressions_Ax_E_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_Chew_m0", 	m_expressions_Chew_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_Chin_m0", 	m_expressions_Chin_m0	);
				morphManager.SetMorphValue ("m_expressions.m_expressions_FV_m0", m_expressions_FV_m0);
				//morphManager.SetMorphValue("m_expressions.m_expressions_Glotis_m0", 	m_expressions_Glotis_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_H_EST_m0", 	m_expressions_H_EST_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_JawCompress_m0", 	m_expressions_JawCompress_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_JawFront_m0", 	m_expressions_JawFront_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_KG_m0", 	m_expressions_KG_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_Kiss_m0", 	m_expressions_Kiss_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_Lblow_m0", 	m_expressions_Lblow_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_LbrowDown_m0", 	m_expressions_LbrowDown_m0	);
				morphManager.SetMorphValue ("m_expressions.m_expressions_LbrowUp_m0", m_expressions_LbrowUp_m0);
				//morphManager.SetMorphValue("m_expressions.m_expressions_Ldisgust_m0", 	m_expressions_Ldisgust_m0	);
				morphManager.SetMorphValue ("m_expressions.m_expressions_LeyeClose_m0", m_expressions_LeyeClose_m0);
				//morphManager.SetMorphValue("m_expressions.m_expressions_LeyeOpen_m0", 	m_expressions_LeyeOpen_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_Ljaw_m0", 	m_expressions_Ljaw_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_LLbrowDown_m0", 	m_expressions_LLbrowDown_m0	);
				morphManager.SetMorphValue ("m_expressions.m_expressions_LLbrowUp_m0", m_expressions_LLbrowUp_m0);
				//morphManager.SetMorphValue("m_expressions.m_expressions_LlipCorner_m0", 	m_expressions_LlipCorner_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_LlipDown_m0", 	m_expressions_LlipDown_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_LlipSide_m0", 	m_expressions_LlipSide_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_LlipUp_m0", 	m_expressions_LlipUp_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_LlowLid_m0", 	m_expressions_LlowLid_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_LmouthSad_m0", 	m_expressions_LmouthSad_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_LneckTension_m0", 	m_expressions_LneckTension_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_Lnostril_m0", 	m_expressions_Lnostril_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_Lpityful_m0", 	m_expressions_Lpityful_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_Lsad_m0", 	m_expressions_Lsad_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_LsmileClose_m0", 	m_expressions_LsmileClose_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_LsmileOpen_m0", 	m_expressions_LsmileOpen_m0	);
				morphManager.SetMorphValue ("m_expressions.m_expressions_Lsquint_m0", m_expressions_Lsquint_m0);
				morphManager.SetMorphValue ("m_expressions.m_expressions_MouthOpen_m0", m_expressions_MouthOpen_m0);
				morphManager.SetMorphValue ("m_expressions.m_expressions_MPB_Down_m0", m_expressions_MPB_Down_m0);
				morphManager.SetMorphValue ("m_expressions.m_expressions_MPB_Up_m0", m_expressions_MPB_Up_m0);
				//morphManager.SetMorphValue("m_expressions.m_expressions_Rblow_m0", 	m_expressions_Rblow_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_RbrowDown_m0", 	m_expressions_RbrowDown_m0	);
				morphManager.SetMorphValue ("m_expressions.m_expressions_RbrowUp_m0", m_expressions_RbrowUp_m0);
				//morphManager.SetMorphValue("m_expressions.m_expressions_Rdisgust_m0", 	m_expressions_Rdisgust_m0	);
				morphManager.SetMorphValue ("m_expressions.m_expressions_ReyeClose_m0", m_expressions_ReyeClose_m0);
				//morphManager.SetMorphValue("m_expressions.m_expressions_ReyeOpen_m0", 	m_expressions_ReyeOpen_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_Rjaw_m0", 	m_expressions_Rjaw_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_RlipCorner_m0", 	m_expressions_RlipCorner_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_RlipDown_m0", 	m_expressions_RlipDown_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_RlipSide_m0", 	m_expressions_RlipSide_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_RlipUp_m0", 	m_expressions_RlipUp_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_RlowLid_m0", 	m_expressions_RlowLid_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_RmouthSad_m0", 	m_expressions_RmouthSad_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_RneckTension_m0", 	m_expressions_RneckTension_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_Rnostril_m0", 	m_expressions_Rnostril_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_Rpityful_m0", 	m_expressions_Rpityful_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_RRbrowDown_m0", 	m_expressions_RRbrowDown_m0	);
				morphManager.SetMorphValue ("m_expressions.m_expressions_RRbrowUp_m0", m_expressions_RRbrowUp_m0);
				//morphManager.SetMorphValue("m_expressions.m_expressions_Rsad_m0", 	m_expressions_Rsad_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_RsmileClose_m0", 	m_expressions_RsmileClose_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_RsmileOpen_m0", 	m_expressions_RsmileOpen_m0	);
				morphManager.SetMorphValue ("m_expressions.m_expressions_Rsquint_m0", m_expressions_Rsquint_m0);
				morphManager.SetMorphValue ("m_expressions.m_expressions_S_m0", m_expressions_S_m0);
				morphManager.SetMorphValue ("m_expressions.m_expressions_SH_CH_m0", m_expressions_SH_CH_m0);
				//morphManager.SetMorphValue("m_expressions.m_expressions_Shout_m0", 	m_expressions_Shout_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_TD_I_m0", 	m_expressions_TD_I_m0	);
				//morphManager.SetMorphValue("m_expressions.m_expressions_UH_OO_m0", 	m_expressions_UH_OO_m0	);
				morphManager.SetMorphValue ("m_expressions.m_expressions_UW_U_m0", m_expressions_UW_U_m0);
				//morphManager.SetMorphValue("m_teeth.m_teeth_Compress_tg_m0", 	m_teeth_Compress_tg_m0	);
				morphManager.SetMorphValue ("m_teeth.m_teeth_CurlDown_Out_tg_m0", m_teeth_CurlDown_Out_tg_m0);
				//morphManager.SetMorphValue("m_teeth.m_teeth_CurlLeft_Out_tg_m0", 	m_teeth_CurlLeft_Out_tg_m0	);
				//morphManager.SetMorphValue("m_teeth.m_teeth_CurlRight_Out_tg_m0", 	m_teeth_CurlRight_Out_tg_m0	);
				//morphManager.SetMorphValue("m_teeth.m_teeth_CurlUp_Out_tg_m0", 	m_teeth_CurlUp_Out_tg_m0	);
				//morphManager.SetMorphValue("m_teeth.m_teeth_Left_In_tg_m0", 	m_teeth_Left_In_tg_m0	);
				morphManager.SetMorphValue ("m_teeth.m_teeth_LLL_In_tg_m0", m_teeth_LLL_In_tg_m0);
				morphManager.SetMorphValue ("m_teeth.m_teeth_OutMiddle_tg_m0", m_teeth_OutMiddle_tg_m0);
				//morphManager.SetMorphValue("m_teeth.m_teeth_Right_In_tg_m0", 	m_teeth_Right_In_tg_m0	);
				//morphManager.SetMorphValue("m_teeth.m_teeth_RRR_In_tg_m0", 	m_teeth_RRR_In_tg_m0	);
				//morphManager.SetMorphValue("m_teeth.m_teeth_t_AE_AA_m0", 	m_teeth_t_AE_AA_m0	);
				//morphManager.SetMorphValue("m_teeth.m_teeth_t_AO_a_m0", 	m_teeth_t_AO_a_m0	);
				//morphManager.SetMorphValue("m_teeth.m_teeth_t_Ax_E_m0", 	m_teeth_t_Ax_E_m0	);
				//morphManager.SetMorphValue("m_teeth.m_teeth_t_Chew_m0", 	m_teeth_t_Chew_m0	);
				morphManager.SetMorphValue ("m_teeth.m_teeth_t_FV_m0", m_teeth_t_FV_m0);
				//morphManager.SetMorphValue("m_teeth.m_teeth_t_H_EST_m0", 	m_teeth_t_H_EST_m0	);
				//morphManager.SetMorphValue("m_teeth.m_teeth_t_JawCompress_m0", 	m_teeth_t_JawCompress_m0	);
				//morphManager.SetMorphValue("m_teeth.m_teeth_t_JawFront_m0", 	m_teeth_t_JawFront_m0	);
				//morphManager.SetMorphValue("m_teeth.m_teeth_t_KG_m0", 	m_teeth_t_KG_m0	);
				//morphManager.SetMorphValue("m_teeth.m_teeth_t_Ljaw_m0", 	m_teeth_t_Ljaw_m0	);
				morphManager.SetMorphValue ("m_teeth.m_teeth_t_MouthOpen_m0", m_teeth_t_MouthOpen_m0);
				morphManager.SetMorphValue ("m_teeth.m_teeth_t_MPB_m0", m_teeth_t_MPB_m0);
				//morphManager.SetMorphValue("m_teeth.m_teeth_t_Rjaw_m0", 	m_teeth_t_Rjaw_m0	);
				morphManager.SetMorphValue ("m_teeth.m_teeth_t_S_m0", m_teeth_t_S_m0);
				morphManager.SetMorphValue ("m_teeth.m_teeth_t_SH_CH_m0", m_teeth_t_SH_CH_m0);
				//morphManager.SetMorphValue("m_teeth.m_teeth_t_Shout_m0", 	m_teeth_t_Shout_m0	);
				//morphManager.SetMorphValue("m_teeth.m_teeth_t_TD_I_m0", 	m_teeth_t_TD_I_m0	);
				//morphManager.SetMorphValue("m_teeth.m_teeth_t_UH_OO_m0", 	m_teeth_t_UH_OO_m0	);
				morphManager.SetMorphValue ("m_teeth.m_teeth_t_UW_U_m0", m_teeth_t_UW_U_m0);
				//morphManager.SetMorphValue("m_teeth.m_teeth_OutMiddle_tg_m0", 	m_teeth_OutMiddle_tg_m0	);
				//morphManager.SetMorphValue("m_teeth.m_teeth_Right_In_tg_m0", 	m_teeth_Right_In_tg_m0	);
				//morphManager.SetMorphValue("m_teeth.m_teeth_RRR_In_tg_m0", 	m_teeth_RRR_In_tg_m0	);
				//morphManager.SetMorphValue("m_teeth.m_teeth_Throat_In_tg_m0", 	m_teeth_Throat_In_tg_m0	);
				morphManager.SetMorphValue ("m_teeth.m_teeth_Up_tg_m0", m_teeth_Up_tg_m0);
		}
}


