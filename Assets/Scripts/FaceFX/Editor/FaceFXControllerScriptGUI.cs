
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Xml;



[CustomEditor(typeof(FaceFXControllerScript_Base),true)]

public class FaceFXControllerScriptGUI : Editor
{
	
// If the FaceFX reference pose is identical to the default pose in the FBX file, then we can use the FBX file
// to define the reference pose.  This allows us to detect differences between the XML file's reference pose and
// the FBX's reference pose, and adjust the bone poses accordingly to reflect these differences.  Then
// one XML file is sufficient for multiple chararacters that share the same skeleton (for instance a fat 
// vs. skinny character)
// This option must be turned off if the referencve pose is different from the FBX pose.
		private bool UseReferencePoseFromFBX = false;
	
		private bool OverwriteExistingAnimations = true;
		private bool QueryForOverwrite = true;
	
// A class storing a bone transform.  The constructor takes the contents of a <bone> xml body (Space-seperated position rotation scale values).
		public struct BoneTransform
		{
				public float[] Values;
				// Constructs a BoneTransform from a space-separated string (originating from an XML file)
				public BoneTransform (string aValue)
				{
	
						string[] StringValues = aValue.Split ();
						Values = new float[10];
						if (StringValues.Length != 10) {
                UnityEngine.Debug.Log ("Error in XML.  A reference boen has only this many values:" + Values.Length);
                UnityEngine.Debug.Log (aValue);
						} else {
								// Position (x, y, z)
								// @todo - Figure out why Pos.x and Rot.x values need to be negated.
								Values [0] = -float.Parse (StringValues [0]);
								Values [1] = float.Parse (StringValues [1]);
								Values [2] = float.Parse (StringValues [2]);
	
								// Rotation (x, y, z, w but in the XML file it is stored as w,x,y,z)
								Values [3] = -float.Parse (StringValues [4]);
								Values [4] = float.Parse (StringValues [5]);
								Values [5] = float.Parse (StringValues [6]);
								Values [6] = float.Parse (StringValues [3]);
	
								// Scale (x, y, z)
								Values [7] = float.Parse (StringValues [7]);
								Values [8] = float.Parse (StringValues [8]);
								Values [9] = float.Parse (StringValues [9]);
						}
				}
				public BoneTransform (GameObject t)
				{
						Values = new float[10];
						Values [0] = t.transform.localPosition.x;
						Values [1] = t.transform.localPosition.y;
						Values [2] = t.transform.localPosition.z;
						Values [3] = t.transform.localRotation.x;
						Values [4] = t.transform.localRotation.y;
						Values [5] = t.transform.localRotation.z;
						Values [6] = t.transform.localRotation.w;
						Values [7] = t.transform.localScale.x;
						Values [8] = t.transform.localScale.y;
						Values [9] = t.transform.localScale.z;
				}
				public BoneTransform (Vector3 pos, Quaternion rot, Vector3 scale)
				{
						Values = new float[10];
						Values [0] = pos.x;
						Values [1] = pos.y;
						Values [2] = pos.z;
						Values [3] = rot.x;
						Values [4] = rot.y;
						Values [5] = rot.z;
						Values [6] = rot.w;
						Values [7] = scale.x;
						Values [8] = scale.y;
						Values [9] = scale.z;
				}
				public void Print ()
				{
            UnityEngine.Debug.Log ("( " + Values[0] + ", " + Values[1] + ", " + Values[2] + ") (" + Values[3] + ", " + Values[4] + ", " + Values[5] + ", " + Values[6] + ") (" + Values[7] + ", " + Values[8] + ", " + Values[9] + ")");
				}
				public Vector3 GetPos ()
				{
						return new Vector3 (Values [0], Values [1], Values [2]);
				}
				public Quaternion GetRot ()
				{
						return new Quaternion (Values [3], Values [4], Values [5], Values [6]);
				}
				public Vector3 GetScale ()
				{
						return new Vector3 (Values [7], Values [8], Values [9]);
				}
		}
// A class to help manage adding keys to curves and curves to clips.
		struct AnimClipHelper
		{

				AnimationCurve curvePosX;
				AnimationCurve curvePosY;
				AnimationCurve curvePosZ;
				AnimationCurve curveRotX;
				AnimationCurve curveRotY;
				AnimationCurve curveRotZ;
				AnimationCurve curveRotW;
				AnimationCurve curveScaleX;
				AnimationCurve curveScaleY;
				AnimationCurve curveScaleZ;
				public AnimationClip animclip;

				public AnimClipHelper (AnimationClip clip)
				{
						animclip = clip;
						curvePosX = new AnimationCurve ();
						curvePosY = new AnimationCurve ();
						curvePosZ = new AnimationCurve ();
						curveRotX = new AnimationCurve ();
						curveRotY = new AnimationCurve ();
						curveRotZ = new AnimationCurve ();
						curveRotW = new AnimationCurve ();
						curveScaleX = new AnimationCurve ();
						curveScaleY = new AnimationCurve ();
						curveScaleZ = new AnimationCurve ();
				}

				public void PreAddKeys ()
				{
						curvePosX = new AnimationCurve ();
						curvePosY = new AnimationCurve ();
						curvePosZ = new AnimationCurve ();
						curveRotX = new AnimationCurve ();
						curveRotY = new AnimationCurve ();
						curveRotZ = new AnimationCurve ();
						curveRotW = new AnimationCurve ();
						curveScaleX = new AnimationCurve ();
						curveScaleY = new AnimationCurve ();
						curveScaleZ = new AnimationCurve ();
				}

				public void AddKeys (float t, BoneTransform values)
				{

						// Position x,y,z
						curvePosX.AddKey (new Keyframe (t, values.Values [0]));
						curvePosY.AddKey (new Keyframe (t, values.Values [1]));
						curvePosZ.AddKey (new Keyframe (t, values.Values [2]));

						// Rotation x,y,z,w
						curveRotX.AddKey (new Keyframe (t, values.Values [3]));
						curveRotY.AddKey (new Keyframe (t, values.Values [4]));
						curveRotZ.AddKey (new Keyframe (t, values.Values [5]));
						curveRotW.AddKey (new Keyframe (t, values.Values [6]));

						// Scale x,y,z
						curveScaleX.AddKey (new Keyframe (t, values.Values [7]));
						curveScaleY.AddKey (new Keyframe (t, values.Values [8]));
						curveScaleZ.AddKey (new Keyframe (t, values.Values [9]));
				}

				public void PostAddKeys (string objectRelativePath)
				{
						animclip.SetCurve (objectRelativePath, typeof(Transform), "localPosition.x", curvePosX);
						animclip.SetCurve (objectRelativePath, typeof(Transform), "localPosition.y", curvePosY);
						animclip.SetCurve (objectRelativePath, typeof(Transform), "localPosition.z", curvePosZ);
						animclip.SetCurve (objectRelativePath, typeof(Transform), "localRotation.x", curveRotX);
						animclip.SetCurve (objectRelativePath, typeof(Transform), "localRotation.y", curveRotY);
						animclip.SetCurve (objectRelativePath, typeof(Transform), "localRotation.z", curveRotZ);
						animclip.SetCurve (objectRelativePath, typeof(Transform), "localRotation.w", curveRotW);
						animclip.SetCurve (objectRelativePath, typeof(Transform), "localScale.x", curveScaleX);
						animclip.SetCurve (objectRelativePath, typeof(Transform), "localScale.y", curveScaleY);
						animclip.SetCurve (objectRelativePath, typeof(Transform), "localScale.z", curveScaleZ);
				}
		}

		public FaceFXControllerScript_Base _target;
		void OnEnable ()
		{
				_target = (FaceFXControllerScript_Base)target;
		}
		public override void OnInspectorGUI ()
		{
				EditorGUILayout.BeginVertical ("box");
				GUILayout.Label ("Scale Factor should match the FBX Scale Factor!");
				float scaleFactor = EditorGUILayout.FloatField ("Scale Factor", _target.ScaleFactor);

		_target.debug = EditorGUILayout.Toggle("Debug", _target.debug);

				EditorGUILayout.EndVertical ();
				if (GUI.changed) {
						if (scaleFactor != _target.ScaleFactor) {
								_target.ScaleFactor = scaleFactor;
                UnityEngine.Debug.Log ("Import XML Actor settings have changed.  Import an XML Actor to use the new settings.");
						}
				}
				if (GUILayout.Button ("Import XML")) {
						QueryForOverwrite = true;

						string path = EditorUtility.OpenFilePanel ("Import FaceFX XML", "", "xml");
						if (path.Length != 0) {
								if (File.Exists (path)) {
                    UnityEngine.Debug.Log ("Importing XML Actor: " + path);
										string file_contents = File.OpenText (path).ReadToEnd ();
										if (!ImportXML (file_contents)) {
												return;
										}
										ImportXMLAnimations (file_contents);
								}
								string prefabWarning = "Would you like to save .anim files " +
										"for the animations that were created by this script?  Then you can manually " +
										"attach the .anim files to your character.";			
								if (EditorUtility.DisplayDialog ("Save .Anim Files", prefabWarning, "Browse for Folder", "Skip Saving .anim files")) {
										SaveAnimFiles ();
								}		
						}		
				}
		}
	
		public void SaveAnimFiles ()
		{
		
				string folderpath = EditorUtility.OpenFolderPanel ("Load .anim Save dir", "/", "");
				string relpath = FileUtil.GetProjectRelativePath (folderpath);
	
				Animation anim = _target.GetComponent<Animation> ();
				foreach (AnimationState animClip in anim) {
						if (animClip != null) {
								string filepath = string.Format (relpath + "/{0}.anim", animClip.name);
								string illegalfilecharacters = "*\":<>\\/|";
								if (animClip.name.LastIndexOfAny (illegalfilecharacters.ToCharArray ()) < 0) {
										FileUtil.DeleteFileOrDirectory (filepath);
										AssetDatabase.CreateAsset (animClip.clip, filepath);
								} else {
                    UnityEngine.Debug.LogWarning (animClip.name + " animation not saved because it contained " +
												"illegal characters for filenames. If you need to save this .anim file, try " +
												"editing the FaceFX asset and reimporting the XML.  Or modify the XML directly.");
								}
						}
				}
		}
	
		// The FaceFX Controller Script has public variables for all bone poses and morphs
		// that are driven.  Variable names can't contain spaces or other illegal characters, so 
		// this function will replace illegal characters with an underscore.
		string convertBonePoseNameToVarName (string s)
		{
				s = s.Replace (" ", "_");
				s = s.Replace (".", "_");
				return s;
		}
	
		private float GetFirstKeyOrEventNegativeTime (XmlNode curvesNode, XmlNode eventsNode)
		{
				float firstKeyOrEventTime = 0;
				int j = 0;
				if (null != curvesNode) {
						for (j = 0; j < curvesNode.ChildNodes.Count; ++j) {
								XmlNode curveFirstKeyNode = curvesNode.ChildNodes.Item (j);
								int first_keytime_end = curveFirstKeyNode.InnerText.IndexOf (" ");
								if (first_keytime_end > -1) {
										string first_keytime_string = curveFirstKeyNode.InnerText.Substring (0, first_keytime_end);
										float first_keytime = float.Parse (first_keytime_string);
										if (first_keytime < firstKeyOrEventTime) {
												firstKeyOrEventTime = first_keytime;
										}
								}
						}
				}
				if (null != eventsNode) {
						for (j = 0; j < eventsNode.ChildNodes.Count; ++j) {
								XmlNode eventNode = eventsNode.ChildNodes.Item (j);
								if (eventNode.Attributes ["start_time"] != null) {
										float start_time = float.Parse (eventNode.Attributes ["start_time"].Value);
										if (start_time < firstKeyOrEventTime) {
												firstKeyOrEventTime = start_time;
										}
								}
						}
				}
				return firstKeyOrEventTime;
		}
	
		public void ImportXMLAnimations (string xmltext)
		{
				XmlDocument doc_reader = new XmlDocument ();
				doc_reader.Load (new StringReader (xmltext));

				ArrayList importedAnimations = new ArrayList ();
				ArrayList importedEventTrackAnimations = new ArrayList ();
				ArrayList existingAnimations = new ArrayList ();
				ArrayList existingEventTrackAnimations = new ArrayList ();
		
				XmlNodeList animGroupNodeList = doc_reader.SelectNodes ("/actor/animation_groups/animation_group");
				for (int gi = 0; gi < animGroupNodeList.Count; ++gi) {
						XmlNode animGroupNode = animGroupNodeList.Item (gi);
						string animGroupName = animGroupNode.Attributes ["name"].Value;
						if (animGroupName == "FBX@Animations" || 
								animGroupName == "PunctuationEvents" ||
								animGroupName == "EmoticonEvents") {
								// Most of the time, these animation groups have no use in Unity
								// Skip them so we don't clog up the animation component.
								continue;
						}
						XmlNodeList animNodeList = animGroupNode.SelectNodes ("animation");
						for (int i = 0; i < animNodeList.Count; ++i) {
								float lastKeyTime = 0;
								XmlNode animNode = animNodeList.Item (i);
								XmlNode eventsNode = animNode.SelectSingleNode ("event_take");
								XmlNode curvesNode = animNode.SelectSingleNode ("curves");
								float firstKeyOrEventTime = GetFirstKeyOrEventNegativeTime (curvesNode, eventsNode);
								string animName = animNode.Attributes ["name"].Value;
				
								if (null != curvesNode) {
										AnimClipHelper controllerAnimHelper = new AnimClipHelper (new AnimationClip ());

										ArrayList curveArray = new ArrayList ();
										ArrayList curveNameArray = new ArrayList ();
					
										int j = 0;
										for (j = 0; j < curvesNode.ChildNodes.Count; ++j) {
												XmlNode curveNode = curvesNode.ChildNodes.Item (j);
												string curveName = curveNode.Attributes ["name"].Value;
												int numKeys = int.Parse (curveNode.Attributes ["num_keys"].Value);
												string curveNodeBodyString = curveNode.InnerText;
												string[] curveKeys = curveNodeBodyString.Split ();
		
												if (curveKeys.Length >= numKeys * 4) {
														AnimationCurve bonePoseAnimCurve = new AnimationCurve ();
														float keytime = 0;
														float keyvalue = 0;
														float keyslopeIn = 0;
														float keyslopeOut = 0;
														int k = 0;
														for (k = 0; k < numKeys; ++k) {
																int keyI = k * 4;
																keytime = float.Parse (curveKeys [keyI + 0]);
																keyvalue = float.Parse (curveKeys [keyI + 1]);
																keyslopeIn = float.Parse (curveKeys [keyI + 2]);
																keyslopeOut = float.Parse (curveKeys [keyI + 3]);
		
																// Shift the entire animation by the firstKeyOrEventTime, which is negative or 0.
																// Then all key times are >= 0
																keytime -= firstKeyOrEventTime;
		
																bonePoseAnimCurve.AddKey (new Keyframe (keytime, keyvalue, keyslopeIn, keyslopeOut));
														}
														if (keytime > lastKeyTime) {
																lastKeyTime = keytime;
														}
		
														curveArray.Add (bonePoseAnimCurve);
														curveNameArray.Add (curveName);
												} else {
                            UnityEngine.Debug.Log ("There is an error in the XML file.  There are insufficient keys.");
												}
										}
										for (j = 0; j < curveArray.Count; ++j) {
												AnimationCurve bonePoseCurve = curveArray [j] as AnimationCurve;
												// Unity doesn't like evaluating curves before or after the first/last key, so make sure each curve has
												// keys at the boundaries of the animations.
												int keyCount = bonePoseCurve.keys.Length;
												if (keyCount > 0) {
														if (bonePoseCurve.keys [0].time > 0) {
																bonePoseCurve.AddKey (new Keyframe (0, bonePoseCurve.Evaluate (0)));
														}
														if (bonePoseCurve.keys [keyCount - 1].time < lastKeyTime) {
																bonePoseCurve.AddKey (new Keyframe (lastKeyTime, bonePoseCurve.Evaluate (lastKeyTime)));
														}
												}
												controllerAnimHelper.animclip.SetCurve ("", typeof(FaceFXControllerScript_Base), convertBonePoseNameToVarName (curveNameArray [j] as string), bonePoseCurve);
										}
										AnimationCurve audioStartTimeCurve = new AnimationCurve ();		
										audioStartTimeCurve.AddKey (new Keyframe (0, -firstKeyOrEventTime));
										audioStartTimeCurve.AddKey (new Keyframe (lastKeyTime, -firstKeyOrEventTime));
										controllerAnimHelper.animclip.SetCurve ("", typeof(FaceFXControllerScript_Base), "audio_start_time", audioStartTimeCurve);
					
										if (AddOrReplaceClip (controllerAnimHelper.animclip, animGroupName + "_" + animName)) {
												importedAnimations.Add (animGroupName + "_" + animName);
										} else {
												existingAnimations.Add (animGroupName + "_" + animName);
										}
								}
								if (null != eventsNode) {
										int k = 0;
										int numEvents = 0;
										for (k = 0; k < eventsNode.ChildNodes.Count; ++k) {
												XmlNode eventNode = eventsNode.ChildNodes.Item (k);
												if (eventNode.Attributes ["payload"] != null) {
														string payload = eventNode.Attributes ["payload"].Value;
														if (payload.IndexOf ("game: playanim ") >= 0) {
																numEvents++;
														}
												}
										}
										string eventTrackAnimName = animGroupName + "_" + animName + _target.EVENT_TRACK_NAME;
		
										if (numEvents > 0) {
												if (AddOrReplaceClip (new AnimationClip (), eventTrackAnimName)) {
														importedEventTrackAnimations.Add (eventTrackAnimName);
												} else {
														existingEventTrackAnimations.Add (eventTrackAnimName);
												}
						
												AnimationState animState = _target.GetComponent<Animation>() [eventTrackAnimName];
		
												int eventIndex = 0;
												AnimationEvent [] evts = new AnimationEvent[numEvents];
												for (k = 0; k < eventsNode.ChildNodes.Count; ++k) {
														XmlNode eventNode = eventsNode.ChildNodes.Item (k);
														if (eventNode.Attributes ["payload"] != null) {
																string payload = eventNode.Attributes ["payload"].Value;
								
																// This type of payload triggers body animations in FaceFX.
																// We can use them to trigger body animations in Unity.
																if (payload.IndexOf ("game: playanim ") >= 0) {
									
																		// remove "game: playanim "
																		string useful_payload = payload.Substring (15);
									
																		// Add magnitude and duration scale to string
																		string magnitude = "";
																		string duration = "";
																		if (eventNode.Attributes ["magnitude_scale"] != null) {
																				magnitude = eventNode.Attributes ["magnitude_scale"].Value;
																		}
																		if (eventNode.Attributes ["duration_scale"] != null) {
																				duration = eventNode.Attributes ["duration_scale"].Value;
																		}						
									
																		float start_time = float.Parse (eventNode.Attributes ["start_time"].Value);
																		// If we shifted the animation forward due to negative frames, 
																		// adjust the animation start time.							
																		start_time -= firstKeyOrEventTime;
									
																		AnimationEvent ev = new AnimationEvent ();
																		ev.time = start_time;
									
																		// If the original event was .4 magnitude, .6 duration, and played
																		// the arm-wave animation, the new payload will look like:
																		// ".4|.6|arm-wave"
																		ev.stringParameter = magnitude + "|" + duration + "|" + useful_payload;
									
																		ev.functionName = "HandleFaceFXPayloadEvent";
																		evts [eventIndex] = ev;
																		eventIndex++;
																}
																// Other payloads could be handled here.  
														}
												}
												AnimationUtility.SetAnimationEvents (animState.clip, evts);
										}
								}
						}
				}

				if (importedAnimations.Count > 0) {
						string importedAnimationsLog = "The following " + importedAnimations.Count + " animations were imported: ";
						for (int i = 0; i < importedAnimations.Count; ++i) {
								importedAnimationsLog = importedAnimationsLog + "\n " + (importedAnimations [i] as string);
						}
            UnityEngine.Debug.Log (importedAnimationsLog);			
				} else {
            UnityEngine.Debug.LogWarning ("No facial animations imported.");
				}
				if (existingAnimations.Count > 0) {
						string existingAnimationsLog = "The following " + existingAnimations.Count + " animations already existed in the Animation component and were not imported: ";
						for (int i = 0; i < existingAnimations.Count; ++i) {
								existingAnimationsLog = existingAnimationsLog + "\n " + (existingAnimations [i] as string);
						}
            UnityEngine.Debug.Log (existingAnimationsLog);			
				}
				if (importedEventTrackAnimations.Count > 0) {
						string importedEventTrackAnimationsLog = "The following " + importedEventTrackAnimations.Count + " event track animations were imported: ";
						for (int i = 0; i < importedEventTrackAnimations.Count; ++i) {
								importedEventTrackAnimationsLog = importedEventTrackAnimationsLog + "\n " + (importedEventTrackAnimations [i] as string);
						}
            UnityEngine.Debug.Log (importedEventTrackAnimationsLog);			
				}
				if (existingEventTrackAnimations.Count > 0) {
						string existingEventTrackAnimationsLog = "The following " + existingEventTrackAnimations.Count + " event track animations already existed in the Animation component and were not imported: ";
						for (int i = 0; i < existingEventTrackAnimations.Count; ++i) {
								existingEventTrackAnimationsLog = existingEventTrackAnimationsLog + "\n " + (existingEventTrackAnimations [i] as string);
						}
            UnityEngine.Debug.Log (existingEventTrackAnimationsLog);			
				}		
		}
		// Searches the object this script is attached to recursively to find a match.  We can't use GameObject.Find because that searches the whole scene.  Transform.Find searches one level.
		Transform RecursiveFind (Transform trans, string searchName)
		{
				foreach (Transform child in trans) {
						if (child.name == searchName) {
								return child;
						}
						Transform returnTransform = RecursiveFind (child, searchName);
						if (returnTransform != null) {
								return returnTransform;
						}
				}
				return null;
		}
		public bool ImportXML (string xmltext)
		{
				XmlDocument doc_reader = new XmlDocument ();
				doc_reader.Load (new StringReader (xmltext));
		
				XmlNodeList linkList = doc_reader.SelectNodes ("/actor/face_graph/links/link");
				if (linkList.Count > 0) {
            UnityEngine.Debug.LogError ("This actor contains link functions that can not be evaluated in Unity.  Be sure to collapse your actor with the fgcollapse command before exporting the XML!");
						return false;
				}		
		
				// Test to see if this is a FaceFX XML file
				XmlNodeList faceGraphNodes = doc_reader.SelectNodes ("/actor/face_graph");
				if (faceGraphNodes.Count > 0) {
						// Use the scale factor from the XML file if it exists.
						if (faceGraphNodes.Item (0).ParentNode.Attributes ["scalefactor"] != null) {
								_target.ScaleFactor = float.Parse (faceGraphNodes.Item (0).ParentNode.Attributes ["scalefactor"] .Value);
                UnityEngine.Debug.Log ("Using scale factor from XML file:" + _target.ScaleFactor);
						} else {
                UnityEngine.Debug.Log ("Using scale factor from Unity Settings:" + _target.ScaleFactor);
						}

						XmlNodeList refBoneList = doc_reader.SelectNodes ("/actor/face_graph/bones/bone");

						Hashtable myRefBoneIndexTable = new Hashtable ();

						ArrayList myRefBoneFileTransforms = new ArrayList ();
						ArrayList myRefBoneNames = new ArrayList ();
						ArrayList myRefBoneGameObjectTransforms = new ArrayList ();
						ArrayList myRefBoneGameObjectBoneTransforms = new ArrayList ();

						ArrayList myRefBoneFilePositions = new ArrayList ();
						ArrayList myRefBoneFileRotations = new ArrayList ();
						ArrayList myRefBoneFileScales = new ArrayList ();
						int i = 0;
						for (i = 0; i < refBoneList.Count; ++i) {
								XmlNode refBone = refBoneList.Item (i);
								string refBoneName = refBone.Attributes ["name"].Value;
								myRefBoneNames.Add (refBoneName);

								myRefBoneIndexTable [refBoneName] = i;

								Transform refBoneObjectTransform = RecursiveFind (_target.transform, refBoneName);
								myRefBoneGameObjectTransforms.Add (refBoneObjectTransform);
								if (refBoneObjectTransform == null) {
                    UnityEngine.Debug.Log ("Warning: Couldn't find refbone: " + refBoneName);
										refBoneObjectTransform = _target.transform;
								}


								BoneTransform trans = new BoneTransform (refBone.InnerText);
								Vector3 myRefBonePos = trans.GetPos ();
								Quaternion myRefBoneQuat = trans.GetRot ();
								Vector3 myRefBoneScale = trans.GetScale ();

								// Scale position by ScaleFactor
								Vector3 myScaledRefBonePos = Vector3.Scale (myRefBonePos, new Vector3 (_target.ScaleFactor, _target.ScaleFactor, _target.ScaleFactor));

								myRefBoneGameObjectBoneTransforms.Add (new BoneTransform (refBoneObjectTransform.localPosition, refBoneObjectTransform.localRotation, refBoneObjectTransform.localScale));

								myRefBoneFileTransforms.Add (new BoneTransform (myScaledRefBonePos, myRefBoneQuat, myRefBoneScale));
								myRefBoneFilePositions.Add (myScaledRefBonePos);
								myRefBoneFileRotations.Add (myRefBoneQuat);
								myRefBoneFileScales.Add (myRefBoneScale);

						}

						ArrayList sucessfulBonePoses = new ArrayList ();
						ArrayList unsucessfulBonePoses = new ArrayList ();
						ArrayList existingBonePoses = new ArrayList ();
						XmlNodeList nodeList = doc_reader.SelectNodes ("/actor/face_graph/nodes/node/bones");
						for (i = 0; i < nodeList.Count; ++i) {
								XmlNode bonesNode = nodeList.Item (i);
								string bonePoseName = bonesNode.ParentNode.Attributes ["name"].Value;
								string bonePoseAnimationName = "facefx " + bonePoseName;
					
								if (bonesNode.ChildNodes.Count == 0) {
										unsucessfulBonePoses.Add (bonePoseAnimationName);
								} else {
										AnimClipHelper bonePoseHelper = new AnimClipHelper (new AnimationClip ());
										int j = 0;
										for (j = 0; j < bonesNode.ChildNodes.Count; ++j) {
												bonePoseHelper.PreAddKeys ();
	
												XmlNode boneNode = bonesNode.ChildNodes.Item (j);
												string boneName = boneNode.Attributes ["name"].Value;
												if (! myRefBoneIndexTable.ContainsKey (boneName)) {
                            UnityEngine.Debug.Log ("Warning! Bone not in reference pose! " + boneName);
												} else {
														int refboneIndex = (int)myRefBoneIndexTable [boneName];
														Transform boneObject = myRefBoneGameObjectTransforms [refboneIndex] as Transform;
														if (boneObject) {
																string bodyString = boneNode.InnerText;
																BoneTransform boneTrans = new BoneTransform (bodyString);
	
																// Scale bone poses by ScaleFactor
																Vector3 boneTransValues = new Vector3 (boneTrans.Values [0], boneTrans.Values [1], boneTrans.Values [2]);
																Vector3 boneTransPos = Vector3.Scale (boneTransValues, new Vector3 (_target.ScaleFactor, _target.ScaleFactor, _target.ScaleFactor));
																boneTrans.Values [0] = boneTransPos.x;
																boneTrans.Values [1] = boneTransPos.y;
																boneTrans.Values [2] = boneTransPos.z;
	
																if (UseReferencePoseFromFBX) {
																		// Calculate the difference between the reference pose in the xml file and the bone pose, then apply the difference to what's in the FBX
																		Vector3 pos = boneTrans.GetPos () - (Vector3)myRefBoneFilePositions [refboneIndex] + boneObject.transform.localPosition;
									
																		Quaternion quat = Quaternion.Inverse ((Quaternion)myRefBoneFileRotations [refboneIndex]) * boneTrans.GetRot ();
																		quat = boneObject.transform.localRotation * quat;
	
																		// Probably overkill...I'm not sure if non-uniform scale is even supported in Unity.
																		Vector3 fp = boneTrans.GetScale ();
																		Vector3 fr = (Vector3)myRefBoneFileScales [refboneIndex];
																		Vector3 gr = boneObject.transform.localScale;
																		Vector3 scale = new Vector3 (fp.x * gr.x / fr.x, fp.y * gr.y / fr.y, fp.z * gr.z / fr.z);
	
																		//Vector3 scale = boneObject.transform.localScale;
																		bonePoseHelper.AddKeys (0, (BoneTransform)myRefBoneGameObjectBoneTransforms [refboneIndex]);
																		bonePoseHelper.AddKeys (1, new BoneTransform (pos, quat, scale));
	
																} else {
																		bonePoseHelper.AddKeys (0, (BoneTransform)myRefBoneFileTransforms [refboneIndex]);
																		bonePoseHelper.AddKeys (1, (BoneTransform)boneTrans);
																}
																if (boneObject != null) {
																		string objectRelativePath = GetRelativePath (boneObject);
																		bonePoseHelper.PostAddKeys (objectRelativePath);
																}	
														}
												}
										}
										if (AddOrReplaceClip (bonePoseHelper.animclip, bonePoseAnimationName)) {
												sucessfulBonePoses.Add (bonePoseAnimationName);
										} else {
												existingBonePoses.Add (bonePoseAnimationName);
										}
								}
						}
						// Create an animation with only the reference pose to play in the background.
						AnimClipHelper loopAnim = new AnimClipHelper (new AnimationClip ());
						for (i = 0; i < refBoneList.Count; ++i) {
								if (myRefBoneGameObjectTransforms [i] != null) {
										loopAnim.PreAddKeys ();
										if (UseReferencePoseFromFBX) {
												loopAnim.AddKeys (0, (BoneTransform)myRefBoneGameObjectBoneTransforms [i]);
												loopAnim.AddKeys (1, (BoneTransform)myRefBoneGameObjectBoneTransforms [i]);
										} else {
												loopAnim.AddKeys (0, (BoneTransform)myRefBoneFileTransforms [i]);
												loopAnim.AddKeys (1, (BoneTransform)myRefBoneFileTransforms [i]);
										}
										string objectRelativePath = GetRelativePath ((Transform)myRefBoneGameObjectTransforms [i]);
										loopAnim.PostAddKeys (objectRelativePath);
								}
						}
						AddOrReplaceClip (loopAnim.animclip, "facefx_loop_anim");
			
						if (sucessfulBonePoses.Count > 0) {
								string addedBonePoseLog = "Imported the following " + sucessfulBonePoses.Count + " additive bone pose animations:";
								for (i = 0; i < sucessfulBonePoses.Count; ++i) {
										addedBonePoseLog = addedBonePoseLog + "\n " + (sucessfulBonePoses [i] as string);
								}
                UnityEngine.Debug.Log (addedBonePoseLog);
						} else {
                UnityEngine.Debug.LogWarning ("No bone pose animations were imported.");
						}
						if (unsucessfulBonePoses.Count > 0) {
								string zeroBoneBonePoseLog = "Failed to import the following " + unsucessfulBonePoses.Count + " additive bone pose animations because they contained no bones:";
								for (i = 0; i < unsucessfulBonePoses.Count; ++i) {
										zeroBoneBonePoseLog = zeroBoneBonePoseLog + "\n " + (unsucessfulBonePoses [i] as string);
								}
                UnityEngine.Debug.Log (zeroBoneBonePoseLog);
						}
						if (existingBonePoses.Count > 0) {
								string existingBonePoseLog = "The following " + existingBonePoses.Count + " bone pose animations were already present and were not replaced: ";
								for (i = 0; i < existingBonePoses.Count; ++i) {
										existingBonePoseLog = existingBonePoseLog + "\n " + (existingBonePoses [i] as string);
								}
                UnityEngine.Debug.Log (existingBonePoseLog);
						}
				} else {
            UnityEngine.Debug.LogWarning ("No face graph nodes existed in the XML file!");
				}
				return true;
		}
		private bool AddOrReplaceClip (AnimationClip clip, string name)
		{
				clip.legacy = true;
				bool bAdded = false;
				if (_target.GetComponent<Animation>() [name]) {
						if (QueryForOverwrite == true) {
								OverwriteExistingAnimations = EditorUtility.DisplayDialog ("Overwrite Warning", "Overwrite Existing Animations with the same name?", "Yes", "No");
								QueryForOverwrite = false;
						}
						if (OverwriteExistingAnimations == true) {
								_target.GetComponent<Animation>().AddClip (clip, name);
								bAdded = true;	
						}	
				} else {
						_target.GetComponent<Animation>().AddClip (clip, name);
						bAdded = true;				
				}
	
				return bAdded;
		}
		public string GetRelativePath (Transform obj)
		{
				if (obj != null) {
						string objectRelativePath = obj.name;
						Transform curObject = obj.transform;
						while (curObject.parent && curObject.parent.gameObject != _target.gameObject) {
								objectRelativePath = curObject.parent.name + "/" + objectRelativePath;
								curObject = curObject.parent;
						}
						if (curObject == null) {
                UnityEngine.Debug.LogWarning ("No relative path exists for unrelated object!");
								return "";
						}
						return objectRelativePath;
				} else {
            UnityEngine.Debug.LogWarning ("No relative path exists for NULL object!");
				}
				return "";
		}
}
