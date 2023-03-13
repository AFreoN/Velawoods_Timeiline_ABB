using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using WellFired;

namespace RouteGames
{
    public class AnimationUtility : MonoBehaviour
    {
        /*
         * Animation controller optimisation. 
         */
        private static string UniversalControllerPath = "Assets/Assets/Animations/UniversalAnimationController/ACTR_Universal.controller";
        private static string HeadMaskPath = "Assets/Assets/Animations/UniversalAnimationController/EyeBones.mask";
        private static string UpperBodyMaskPath = "Assets/Assets/3D/Character/Mask/Body.mask";


        public static void OptimiseAnimationController(string sceneName)
        {
#if BUILD_DEBUG_INFO
            Debug.LogFormat("Route1Games::AnimationUtility: Optimising animation controllers for scene: \n->{0}", sceneName);
#endif

            GameObject sequenceObjects = FindSequenceObjectsGameObject();

            if (sequenceObjects != null)
            {
                string missionString = "";

                if (CheckIfStringContainsMissionString(sceneName, ref missionString))
                {
                    float[] missionIDs = CoreSystem.ActivityTracker.ConvertIDIntoIndividualIDs(missionString);
                    string outputPath = BuildOutputPath(missionIDs);

                    if (CreateDirectory(outputPath))
                    {
                        string controllerPath = string.Format("Assets/{0}/{1}.controller", outputPath, missionString);

                        AnimatorController universalController = AssetDatabase.LoadAssetAtPath(UniversalControllerPath, typeof(AnimatorController)) as AnimatorController;
                        AnimatorController newController = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

#if BUILD_DEBUG_INFO
                        Debug.LogFormat("Route1Games::AnimationUtility: Created animation controller at path : \n->{0}", controllerPath);
#endif
                        CreateLayer(newController, "Body");
                        CreateLayer(newController, "Head", HeadMaskPath);
                        CreateLayer(newController, "UpperBodyMasked", UpperBodyMaskPath);

						AddBlinkAnimations(newController, universalController);

                        AddAnimationsToLayers(newController, universalController);
#if BUILD_DEBUG_INFO
                        Debug.LogFormat("Route1Games::AnimationUtility: Animation controller created for L{0}C{1}S{2}M{3}.", missionIDs[0], missionIDs[1], missionIDs[2], missionIDs[3]);
#endif

                        ReplaceCharacterAnimators(sequenceObjects, newController, universalController);
                    }
                }
                else
                {
#if BUILD_DEBUG_INFO
                    Debug.Log("Route1Games::AnimationUtility: Failed to find mission string (E.G. L1C1S1M1) in scene name.");
#endif
                }
            }
            else
            {
#if BUILD_DEBUG_INFO
                Debug.Log("Route1Games::AnimationUtility: Failed to find Sequence Objects Game Object in scene");
#endif
            }
        }

        private static bool CheckIfStringContainsMissionString(string sceneName, ref string extractedValue)
        {
            Regex regex = new Regex("L[0-9]+C[0-9]+S[0-9]+M[0-9]+", RegexOptions.IgnoreCase);
            Match match = regex.Match(sceneName);

            if(match.Success)
            {
                extractedValue = match.Value;
            }

            return match.Success;
        }

        private static string BuildOutputPath(float[] missionIDs)
        {
            return string.Format("Assets/Missions/Level{0}/Course{1}/Scenario{2}/Mission{3}/AnimationContriller", 
                missionIDs[0], missionIDs[1], missionIDs[2], missionIDs[3]);
        }

        private static bool CreateDirectory(string outputPath)
        {
            string directoryPath = Path.Combine(Application.dataPath, outputPath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
#if BUILD_DEBUG_INFO
                Debug.LogFormat("Route1Games::AnimationUtility: Created directory for animation controller ({0}).", directoryPath);
#endif

                return true;
            }

            return false;
        }

        private static void CreateLayer(AnimatorController controller, string name, string maskPath = "")
        {
            AvatarMask mask = null;

            if(maskPath != "")
            {
                mask = AssetDatabase.LoadAssetAtPath(maskPath, typeof(AvatarMask)) as AvatarMask;
            }

            AnimatorControllerLayer layer = new AnimatorControllerLayer()
            {
                name = name,
                defaultWeight = 1.0f,
                iKPass = true,
                avatarMask = mask, 
                stateMachine = new AnimatorStateMachine()
                {
                    name = name,
                    hideFlags = HideFlags.HideInInspector,
                    entryPosition = new Vector3(0.0f, -50.0f, 0.0f),
                    anyStatePosition = new Vector3(250.0f, -50.0f, 0.0f),
                    exitPosition = new Vector3(500.0f, -50.0f, 0.0f)
                }
            };

            controller.AddLayer(layer);

            AssetDatabase.AddObjectToAsset(layer.stateMachine, AssetDatabase.GetAssetPath(controller));

#if BUILD_DEBUG_INFO
            Debug.LogFormat("\t Created animation layer {0} with mask {1}.", name, maskPath);
#endif
        }

        private static void AddAnimationsToLayers(AnimatorController controller, AnimatorController universalController)
        {
#if BUILD_DEBUG_INFO
            Debug.Log("\t Adding animations to layers.");
#endif
            /*List<string>[] addedClips = new List<string>[] { new List<string>(), new List<string>(), new List<string>(), new List<string>() };
            int[] stateCount = new int[] { 0, 1, 0, 1 };
            USTimelineAnimation[] animationTimelines = FindObjectsOfType<USTimelineAnimation>();

            controller.layers[1].stateMachine.AddState("Empty", GetStatePosition(0));
            controller.layers[3].stateMachine.AddState("Empty", GetStatePosition(0));

            foreach (USTimelineAnimation timeline in animationTimelines)
            {
                string affectedObjectName = timeline.AffectedObject.name;

                if (CheckIfCharacterName(affectedObjectName))
                {
                    foreach (AnimationTrack track in timeline.AnimationTracks)
                    {
                        foreach (AnimationClipData animationClipData in track.TrackClips)
                        {
                            foreach (string statename in new string[] { animationClipData.StateName, animationClipData.IdleStateName })
                            {
                                if (addedClips[track.Layer].Contains(statename) == false)
                                {
                                    CreateAnimationState(stateCount[track.Layer], track.Layer, animationClipData.StateName, controller, universalController);
                                    ++stateCount[track.Layer];
                                    addedClips[track.Layer].Add(statename);

#if BUILD_DEBUG_INFO
                                    Debug.LogFormat("\t\t Added state '{0}' to layer '{1}'.", statename, track.Layer);
#endif
                                }
                            }
                        }
                    }
                }
            }*/
        }

        private static bool CheckIfCharacterName(string objectName)
        {
            return objectName.Contains("PRFB_C") && objectName != "PRFB_C101_Mimi" && objectName != "PRFB_C103_Rex";
        }

        private static AnimatorState GetAnimationData(AnimatorStateMachine stateMachine, string stateName)
        {
            for (int i = 0; i < stateMachine.states.Length; i++)
            {
                if (stateMachine.states[i].state.name == stateName)
                {
                    return stateMachine.states[i].state;
                }
            }

            return null;
        }

        private static AnimatorState CreateAnimationState(int stateCount, int trackLayer, string stateName,
           AnimatorController controller, AnimatorController universalController)
        {
            AnimatorState currentState = controller.layers[trackLayer].stateMachine.AddState(stateName, GetStatePosition(stateCount));
            AnimatorState animationData = GetAnimationData(universalController.layers[trackLayer].stateMachine, stateName);

            if (animationData != null)
            {
                currentState.motion = animationData.motion;
                currentState.speed = animationData.speed;
                currentState.mirror = animationData.mirror;
            }

            return animationData;
        }

        private static Vector3 GetStatePosition(int stateCount, int columnHeight = 10)
        {
            return new Vector3(250.0f * (stateCount / columnHeight), (stateCount % columnHeight) * 50.0f, 0.0f);
        }

        private static void AddBlinkAnimations(AnimatorController controller, AnimatorController universalController)
        {
#if BUILD_DEBUG_INFO
            Debug.Log("\t Adding blink animation states and transitions.");
#endif

            AnimatorState blinkState1 = controller.layers[2].stateMachine.AddState("Empty", GetStatePosition(10));
            AnimatorState blinkState2 = controller.layers[2].stateMachine.AddState("Blink trans 0", GetStatePosition(1));
            AnimatorState blinkState3 = controller.layers[2].stateMachine.AddState("Blink trans 2", GetStatePosition(21));

            AnimatorState animationData = GetAnimationData(universalController.layers[2].stateMachine, "Empty");

            if (animationData != null)
            {
                blinkState1.speed = animationData.speed;
                blinkState1.mirror = animationData.mirror;
                blinkState1.cycleOffset = animationData.cycleOffset;
                blinkState1.motion = animationData.motion;

                blinkState2.speed = animationData.speed;
                blinkState2.mirror = animationData.mirror;
                blinkState2.cycleOffset = animationData.cycleOffset;
                blinkState2.motion = animationData.motion;

                blinkState3.speed = animationData.speed;
                blinkState3.mirror = animationData.mirror;
                blinkState3.cycleOffset = animationData.cycleOffset;
                blinkState3.motion = animationData.motion;

                controller.layers[2].stateMachine.AddEntryTransition(blinkState1);

                AnimatorStateTransition OneToTwoTransition = blinkState1.AddTransition(blinkState2);
                OneToTwoTransition.exitTime = 0.7699795f;
                OneToTwoTransition.hasFixedDuration = false;
                OneToTwoTransition.duration = 0.0157814f;
                OneToTwoTransition.offset = 0.2761207f;
                OneToTwoTransition.interruptionSource = TransitionInterruptionSource.None;
                OneToTwoTransition.orderedInterruption = true;
                OneToTwoTransition.hasExitTime = true;

                AnimatorStateTransition TwoToThreeTransition = blinkState2.AddTransition(blinkState3);
                TwoToThreeTransition.exitTime = 0.7680433f;
                TwoToThreeTransition.hasFixedDuration = false;
                TwoToThreeTransition.duration = 0.03835773f;
                TwoToThreeTransition.offset = 0.2814246f;
                TwoToThreeTransition.interruptionSource = TransitionInterruptionSource.None;
                TwoToThreeTransition.orderedInterruption = true;
                TwoToThreeTransition.canTransitionToSelf = true;
                TwoToThreeTransition.hasExitTime = true;

                AnimatorStateTransition ThreeToOneTransition = blinkState3.AddTransition(blinkState1);
                ThreeToOneTransition.exitTime = 0.772966f;
                ThreeToOneTransition.hasFixedDuration = false;
                ThreeToOneTransition.duration = 0.2121971f;
                ThreeToOneTransition.offset = 0.0f;
                ThreeToOneTransition.interruptionSource = TransitionInterruptionSource.None;
                ThreeToOneTransition.orderedInterruption = true;
                ThreeToOneTransition.hasExitTime = true;
            }
        }

        private static void ReplaceCharacterAnimators(GameObject sequenceObjects, AnimatorController controller, AnimatorController universalController)
        {
#if BUILD_DEBUG_INFO
            Debug.Log("Route1Games::AnimationUtility: Replacing controllers for all characters in scene.");
#endif
            foreach (Animator animator in sequenceObjects.GetComponentsInChildren<Animator>(true))
            {
                if (animator.runtimeAnimatorController == universalController)
                {
                    animator.runtimeAnimatorController = controller;
#if BUILD_DEBUG_INFO
                    Debug.LogFormat("\t Animator replaced on Object: {0}", animator.gameObject.name);
#endif
                   
                }
            }
        }

        private static GameObject FindSequenceObjectsGameObject()
        {
           GameObject sequenceObjects = GameObject.Find("SequenceObjects");

           if (sequenceObjects == null)
           {
               sequenceObjects = GameObject.Find("SequenceObject");
           }

           if (sequenceObjects == null)
           {
               sequenceObjects = GameObject.Find("Sequence_Objects");
           }

           if (sequenceObjects == null)
           {
               sequenceObjects = GameObject.Find("Sequence_Object");
           }

           return sequenceObjects;
        }

        /*
         * FaceFX Optimisation. 
         */
    }
} 