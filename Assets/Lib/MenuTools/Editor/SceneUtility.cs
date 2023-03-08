using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using WellFired;
using System;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using UnityEditor.Animations;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine.Timeline;

public class UtilityUtility : MonoBehaviour
{
	[MenuItem("VELA/Scene/DeleteAudioManagerItem")]
	public static void DeleteAudioManagerItem()
	{
		GameObject sequenceObjects = GameObject.Find("3DFootStepObject");

		if (sequenceObjects) 
		{
			DestroyImmediate(sequenceObjects);
		}
	}

    public static bool GetMissionDatabaseID(out int missionDatabaseID)
    {
        /*MissionSetUp missionSetupEvent = FindObjectOfType<MissionSetUp>();

        if (missionSetupEvent)
        {
            if (int.TryParse(Database.Instance.GetID(new float[] { missionSetupEvent.levelid, missionSetupEvent.courseid, missionSetupEvent.scenarioid, missionSetupEvent.missionid }), out missionDatabaseID) && missionDatabaseID > 0)
            {
                return true;
            }
        }
        else
        {
            Debug.LogError("Failed to find the <color=cyan>MissionSetUp</color> event in the scene.");
        }*/

        missionDatabaseID = -1;
        return false;
    }

    public static List<TimelineClip> GetListOfActivityChanges()
    {
        /*List<USEventBase> activityChanges = new List<USEventBase>();

        activityChanges.Add(FindObjectOfType<MissionSetUp>());
        activityChanges.AddRange(FindObjectsOfType<ActivityChangeEvent>());
        activityChanges.AddRange(FindObjectsOfType<ActivityChangeTriggerMG>());
        activityChanges.Add(FindObjectOfType<MissionEndSequence>());

        return activityChanges.OrderBy(item => item.FireTime).ToList();*/
        return null;
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

    [MenuItem("VELA/Scene/Animations/OptimiseFaceFX")]
    public static void OptimiseFaceFX()
    {
        DialogueEvent[] dialogueEvents = FindObjectsOfType<DialogueEvent>();

        List<GameObject> usedCharacters = new List<GameObject>();
        List<GameObject> allCharacters = new List<GameObject>();


        foreach(DialogueEvent dialogueEvent in dialogueEvents)
        {
            if (dialogueEvent != null)
            {
                DialogueEventData eventData = dialogueEvent.Data;

                if (eventData != null)
                { 
                    GameObject character = eventData.dialogueData.character;

                    if (character != null && usedCharacters.Contains(character) == false)
                    {
                        usedCharacters.Add(character);
                    }
                }
            }
        }

        GameObject sequenceObjects = GameObject.Find("SequenceObjects");

        if (sequenceObjects == null)
        {
            Debug.Log("Failed to find the SequenceObjects object in the scene, this may cause issues in other parts of the game. ");
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

        if (TimelineController.instance)
        {
            foreach (FaceAnim faceFX in sequenceObjects.GetComponentsInChildren<FaceAnim>(true))
            {
                UnityEngine.Animation animation = faceFX.gameObject.GetComponent<UnityEngine.Animation>();

                if (animation != null)
                {
                    allCharacters.Add(faceFX.gameObject);
                }
            }
        }
        else
        {
            Debug.Log("Failed to find the SequenceObjects object in the scene. Skipping face FX Optimisation.");

            return;
        }

        foreach(GameObject gameObject in allCharacters)
        {
            UnityEngine.Animation animation = gameObject.GetComponent<UnityEngine.Animation>();

            List<string> clipNames = new List<string>();

            foreach (AnimationState clip in animation)
            {
                clipNames.Add(clip.name);
            }

            if(usedCharacters.Contains(gameObject))
            {
                PrefabUtility.DisconnectPrefabInstance(gameObject);
                foreach(DialogueEvent dialogueEvent in dialogueEvents.Where(item => item.Data.dialogueData.character == gameObject))
                {
                    if (dialogueEvent.Data.dialogueData.animationClip != null)
                    {
                        string name = dialogueEvent.Data.dialogueData.animationClip.name;

                        if (clipNames.Contains(name))
                        {
                            clipNames.Remove(name);
                        }
                    }
                }


                List<string> defaultAnims = new List<string>();

                defaultAnims.Add("facefx open");
                defaultAnims.Add("facefx W");
                defaultAnims.Add("facefx ShCh");
                defaultAnims.Add("facefx PBM");
                defaultAnims.Add("facefx FV");             
                defaultAnims.Add("facefx wide");
                defaultAnims.Add("facefx O");
                defaultAnims.Add("facefx tBack");
                defaultAnims.Add("facefx tRoof");
                defaultAnims.Add("facefx tTeeth");
                defaultAnims.Add("facefx Blink");
                defaultAnims.Add("facefx Eyebrow Raise");
                defaultAnims.Add("facefx Squint");
                defaultAnims.Add("facefx Head_Pitch_Pos");
                defaultAnims.Add("facefx Head_Yaw_Pos");
                defaultAnims.Add("facefx Head_Roll_Pos");
                defaultAnims.Add("facefx Eyes_Pitch_Pos");
                defaultAnims.Add("facefx Eyes_Yaw_Pos");
                defaultAnims.Add("facefx Head_Pitch_Neg");
                defaultAnims.Add("facefx Head_Yaw_Neg");
                defaultAnims.Add("facefx Head_Roll_Neg");
                defaultAnims.Add("facefx Eyes_Pitch_Neg");
                defaultAnims.Add("facefx Eyes_Yaw_Neg");
                defaultAnims.Add("facefx_loop_anim");
                
                foreach (string animName in defaultAnims)
                {
                    if (clipNames.Contains(animName))
                    {
                        clipNames.Remove(animName);
                    }
                }
            }

            foreach (string clipName in clipNames)
            {
                animation.RemoveClip(clipName);
            }

            Debug.LogFormat("Optimised FaceFX for {0}.", gameObject.name);
        }
    }

    [MenuItem("VELA/Scene/Animations/CreateAnimationController")]
    public static void CreateAnimationControllerForScene()
    {
        Route1Games.AnimationUtility.OptimiseAnimationController(EditorApplication.currentScene);
    }

    /*[MenuItem("VELA/DialogueTest")]
    public static void DialogueTest()
    {
        MissionSetUp setup = FindObjectOfType<MissionSetUp>();

        if (setup)
        {
            *//*string result = Database.Instance.GetActivityID(setup.levelid, setup.courseid, setup.scenarioid, setup.missionid, 1, 1);
            int databaseTaskID = -1;
            
            if(int.TryParse(result, out databaseTaskID) && databaseTaskID > -1)
            {
                int activityTypeID = Database.Instance.GetActivityTypeID(databaseTaskID);

                if (activityTypeID > 0 && (activityTypeID == 4 || activityTypeID == 5 || activityTypeID == 8))
                {
                    List<Dictionary<string, string>> queryResult = Database.Instance.Query("select dialogueid, dialogue, characterid, audiofileid from dialogue where activityid = " + databaseTaskID);

                    Debug.Log(queryResult[0]["dialogue"]);
                }
            }*//*


            List<USEventBase> activityChanges = new List<USEventBase>();
            List<DialogueEvent> dialogueEvents = FindObjectsOfType<DialogueEvent>().ToList();

            activityChanges.Add(setup);
            activityChanges.AddRange(FindObjectsOfType<ActivityChangeEvent>());
            activityChanges.AddRange(FindObjectsOfType<ActivityChangeTriggerMG>());
            activityChanges.Add(FindObjectOfType<MissionEndSequence>());

            activityChanges = activityChanges.OrderBy(item => item.FireTime).ToList();
            dialogueEvents = dialogueEvents.OrderBy(item => item.FireTime).ToList();

            int missionID = -1;

            string MissionResults = "";

            if (int.TryParse(Database.Instance.GetID(new float[] { setup.levelid, setup.courseid, setup.scenarioid, setup.missionid }), out missionID) && missionID > 0)
            {
                Debug.Log(missionID);
                List<Dictionary<string, string>> queryResult = Database.Instance.Query("SELECT Task.taskid, Activity.activityid, Activity.id from Task, Activity where Activity.taskid = Task.id AND Task.missionid = " + missionID + " ORDER BY Task.taskid, Activity.activityid");

                if (queryResult.Count == (activityChanges.Count - 1))
                {
                    for (int activityIndex = 0; activityIndex < activityChanges.Count - 1; ++activityIndex)
                    {
                        int databaseTaskID = -1;

                        if (int.TryParse(queryResult[activityIndex]["id"], out databaseTaskID) && databaseTaskID > -1)
                        {
                             int activityTypeID = Database.Instance.GetActivityTypeID(databaseTaskID);

                             if (activityTypeID > 0 && (activityTypeID == 4 || activityTypeID == 5 || activityTypeID == 8))
                             {
                                 List<Dictionary<string, string>> dialogueResults = Database.Instance.Query("select id, dialogueid, dialogue, characterid, audiofileid from dialogue where activityid = " + databaseTaskID);

                                 List<DialogueEvent> activityDialogueEvents =  dialogueEvents.Where(item => (item.FireTime > activityChanges[activityIndex].FireTime && item.FireTime < activityChanges[activityIndex + 1].FireTime)).ToList();

                                 int CurrentText = 0;
                                 int previousIndex = -1;
                                 foreach (Dictionary<string, string> dialogue in dialogueResults)
                                 {
                                     int dialogueEventIndex = int.Parse(dialogue["dialogueid"]) - 1;
                                     string databaseText = dialogue["dialogue"];
                                     string databaseID = dialogue["id"];



                                     if (dialogueEventIndex < activityDialogueEvents.Count)
                                     {
                                         StringBuilder result = new StringBuilder();

                                         if (previousIndex == dialogueEventIndex)
                                         {
                                             ++CurrentText;
                                         }
                                         else
                                         {
                                             CurrentText = 0;
                                         }

                                         if (CurrentText < activityDialogueEvents[dialogueEventIndex].Data.dialogueData.dialogueText.Count)
                                         {
                                             string dialogueText = activityDialogueEvents[dialogueEventIndex].Data.dialogueData.dialogueText[CurrentText].text;

                                             int count = Mathf.Min(dialogueText.Length, databaseText.Length);

                                             bool isDifferent = false;

                                             for (int characterIndex = 0; characterIndex < count; ++characterIndex)
                                             {
                                                 if (dialogueText[characterIndex] == databaseText[characterIndex])
                                                 {
                                                     if (isDifferent)
                                                     {
                                                         result.Append("</span>");
                                                     }

                                                     result.Append(dialogueText[characterIndex]);

                                                     isDifferent = false;
                                                 }
                                                 else
                                                 {
                                                     if (!isDifferent)
                                                     {
                                                         result.Append("<span style=\"color:red;\">");
                                                     }

                                                     result.Append(dialogueText[characterIndex]);

                                                     isDifferent = true;
                                                 }
                                             }

                                             if (isDifferent)
                                             {
                                                 result.Append("</span>");
                                             }

                                             string output = "\nDatabase ID: " + databaseID + "\n                   " + result + "\nDialogue Text:     " + dialogueText + "\nDatabase Text:     " + databaseText;

                                             if (output.Contains("<span"))
                                             {
                                                 MissionResults += output + "\n";
                                             }

                                             Debug.Log(output);
                                         }
                                         else
                                         {
                                             Debug.LogError("Missing text from dialogue event.");
                                         }
                                     }
                                     else
                                     {
                                         Debug.LogError("Not enough dialogue events.");
                                     }

                                     previousIndex = dialogueEventIndex;
                                 }
                             }
                        }                       
                    }
                }
            }

            MailMessage mail = new MailMessage();
            mail.IsBodyHtml = true;
            mail.To.Add("s.gore@route1games.com");

            mail.Subject = "Dialogue Data for " + EditorApplication.currentScene;

            mail.Body = string.Format("<pre><font face=\"consolas\">" +MissionResults + "</font></pre>");

            SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
            smtpServer.Credentials = new System.Net.NetworkCredential("Route1Validation@gmail.com", "COTED'AZURTOUTELANNEE") as ICredentialsByHost;
            smtpServer.Port = 587;
            smtpServer.EnableSsl = true;
            ServicePointManager.ServerCertificateValidationCallback =
                delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                { return true; };
            smtpServer.Send(mail);

            Debug.Log(MissionResults);
        }
    }*/



/*    [MenuItem("VELA/Scene/Events/Delete LookAtObjectIK Scripts")]
    static void CleanAllIkObjects()
    {
        UnityEngine.Object[] AllScripts = Resources.FindObjectsOfTypeAll(typeof(LookAtObjectIK));

        Debug.Log("A total of " + AllScripts.Length + " LookAtObjectIK were deleted.");

        System.Array.ForEach(AllScripts, x => DestroyImmediate(x));
    }*/

    [MenuItem("VELA/Scene/Events/Clean Up Footstep Sounds")]
    static void CleanUpFootStepSounds()
    {
        UnityEngine.Object[] AllScripts = Resources.FindObjectsOfTypeAll(typeof(FaceAnim));

        Debug.Log("A total of " + AllScripts.Length + " FaceFX_VELA_PlayAnims were cleaned.");

        System.Array.ForEach(AllScripts, x => ((FaceAnim)x)._FootStepSound = null);
    }

/*    [MenuItem("VELA/Scene/Animations/Fix All State Times")]
    static void FixAllStateTimes()
    {
        foreach (WellFired.USTimelineAnimation timeline in Resources.FindObjectsOfTypeAll(typeof(WellFired.USTimelineAnimation)))
        {
            foreach (WellFired.AnimationTrack track in timeline.AnimationTracks)
            {
                foreach (WellFired.AnimationClipData clip in track.TrackClips)
                {
                    clip.StateDuration = MecanimAnimationUtility.GetStateDuration(clip.StateName, timeline.AffectedObject.gameObject);
                }
            }
        }
    }*/

	[MenuItem("VELA/Scene/Object/List all Scrollrect")]
	static void ListAllScrollRect()
	{
		ScrollRect[] scrollRects = FindObjectsOfType<ScrollRect> ();

		foreach (ScrollRect scroll in scrollRects) {
			Debug.Log(scroll.gameObject.name);
		}
	}
}
