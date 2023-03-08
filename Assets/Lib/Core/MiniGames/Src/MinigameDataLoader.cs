using System.Collections.Generic;
using UnityEngine;

namespace CoreLib
{
	public class MinigameDataLoader
	{
		public static List<MinigameSectionData> LoadMinigameData(string activityID)
		{
			List<MinigameSectionData> data = new List<MinigameSectionData> ();

			List<Dictionary<string, string>> widgetQuery = Database.Instance.Select ("*", "Widget", "activityid=" + activityID);
			
			if (widgetQuery.Count != 0) 
			{
				foreach(Dictionary<string, string> widget in widgetQuery) //for all the sections this activity has
				{
					MinigameSectionData sectionData = new MinigameSectionData();
					sectionData.elements = new List<MinigameElementData>();
					if(widget["sequence"] == null)
						widget["sequence"] = "0";
					//Load section id
					string sequence = widget["sequence"];
					if(string.IsNullOrEmpty(sequence) == false)
					{
						sectionData.section = int.Parse(sequence);
					}else sectionData.section = 0;

					//Load subtype
					if(string.IsNullOrEmpty(widget["widgettypeid"]) == false)
					{
						List<Dictionary<string, string>> subTypeQuery = Database.Instance.Select ("*", "WidgetType", "id=" + widget["widgettypeid"]);
						if(subTypeQuery.Count > 0)
						{
							sectionData.subType = subTypeQuery[0]["typename"];
						}
					}

					//Load section character
					sectionData.character = new MinigameCharacter ();
					string characterID = widget ["characterid"];
					if(string.IsNullOrEmpty(characterID) == false && characterID != "0")
					{
						List<Dictionary<string, string>> characterQuery = Database.Instance.Select ("*", "Character", "id=" + characterID);
						if(characterQuery.Count > 0)
						{
							sectionData.character.characterID = characterQuery[0]["characterid"];
							sectionData.character.characterName = characterQuery[0]["charactername"];
						}
					}

					//Load section illustration
					//Load illustration
					sectionData.illustration = new MinigameIllustration ();
					if(widget.ContainsKey("illustrationid")) {
						string illustrationID = widget ["illustrationid"];
						if(string.IsNullOrEmpty(illustrationID) == false && illustrationID != "0")
						{
							List<Dictionary<string, string>> illustrationQuery = Database.Instance.Select ("*", "Illustration", "id=" + illustrationID);
							if(illustrationQuery.Count > 0)
							{
								sectionData.illustration.description = illustrationQuery[0]["description"];
								sectionData.illustration.unityRef = illustrationQuery[0]["unityref"];
                                int.TryParse(illustrationQuery[0]["activityid"], out sectionData.illustration.activityID);
                            }
						}
					}

					string[] boolColumnNames = new string[]{"boolean1", "boolean2"};
					sectionData.bools = new List<bool> ();
					foreach(string boolColumnName in boolColumnNames)
					{
						string dbBoolEntry = widget[boolColumnName];
						bool loadedBool = (dbBoolEntry == "1");
						sectionData.bools.Add(loadedBool);
					}

					//Load text
					string[] textColumnNames = new string[]{"text1", "text2"};
					sectionData.text = new List<string> ();
					foreach(string textColumnName in textColumnNames)
					{
						sectionData.text.Add(widget[textColumnName].Replace("’", "'").Replace("‘", "'"));
					}

					//Load audio
					if(widget.ContainsKey("audiofileid"))
					{
						string audioFileID = widget["audiofileid"];
						
						if(!(string.IsNullOrEmpty(audioFileID) || audioFileID == "0"))
						{
							MinigameAudio loadedAudio = new MinigameAudio();
							List<Dictionary<string, string>> audioQuery = Database.Instance.Select ("*", "Audiofile", "id=" + audioFileID);
							
							if(audioQuery.Count > 0)
							{
								loadedAudio.fileName = audioQuery[0]["filename"];
								loadedAudio.dialog = audioQuery[0]["dialogue"];
							}
							
							sectionData.audio = loadedAudio;
						}
					}

					//Load all elements in this section
					List<Dictionary<string, string>> widgetElementQuery = Database.Instance.Select ("*", "WidgetElement", "widgetid=" + widget["id"], "sequence ASC, id ASC");
					foreach(Dictionary<string, string> widgetElement in widgetElementQuery) //for all the sequences this activity has
					{
						MinigameElementData mgElement = LoadSingleElement(widgetElement);
						sectionData.elements.Add(mgElement);
					}

					string notes = widget["notes"];
					if(string.IsNullOrEmpty(notes) == false && notes != "0")
						sectionData.notes = notes.Replace("’", "'").Replace("‘", "'");;

					string referenceActivityString = widget["referenceactivityid"];
					if (!string.IsNullOrEmpty(referenceActivityString)){
						sectionData.ReferenceActivityID = int.Parse(referenceActivityString);
					}

					string activityReferenceTypeString = widget["activityreferencetypeid"];
					if (!string.IsNullOrEmpty(activityReferenceTypeString)){
						sectionData.ActivityReferenceTypeID = int.Parse(activityReferenceTypeString);
					}
					//Debug.Log ("referenceactivityid " + sectionData.ReferenceActivityID);
					//Debug.Log ("activityreferencetypeid " + sectionData.ActivityReferenceTypeID);

					data.Add(sectionData);
				}
			}

			return data;
		}
		
        public static MinigameElementData LoadSingleElement(Dictionary<string, string> widgetElement)
		{
			MinigameElementData minigameElement = new MinigameElementData ();

			//Get the sequence Id of this minigame
			string sequence = widgetElement["sequence"];
			if(string.IsNullOrEmpty(sequence) == false)
			{
				minigameElement.sequence = int.Parse(sequence);
			}else minigameElement.sequence = 0;

			//Load audio
			//Can have up to 3 audio files attached per minigame element
			string[] audioFileIDColumnNames = new string[]{"audiofileid", "audiofileid2", "audiofileid3"};
			minigameElement.audio = new List<MinigameAudio> ();
			foreach(string audioFileIDColumn in audioFileIDColumnNames)
			{
				string audioFileID = widgetElement[audioFileIDColumn];

				if(string.IsNullOrEmpty(audioFileID) || audioFileID == "0")
				{
					continue;
				}

				MinigameAudio loadedAudio = new MinigameAudio();

				List<Dictionary<string, string>> audioQuery = Database.Instance.Select ("*", "Audiofile", "id=" + audioFileID);

				if(audioQuery.Count > 0)
				{
					loadedAudio.fileName = audioQuery[0]["filename"];
					loadedAudio.dialog = audioQuery[0]["dialogue"];
				}

				minigameElement.audio.Add(loadedAudio);
			}

			//Load bools
			string[] boolColumnNames = new string[]{"boolean1", "boolean2", "boolean3", "boolean4", "boolean5"};
			minigameElement.bools = new List<bool> ();
			foreach(string boolColumnName in boolColumnNames)
			{
				string dbBoolEntry = widgetElement[boolColumnName];
				bool loadedBool = (dbBoolEntry == "1");
				minigameElement.bools.Add(loadedBool);
			}

			//Load text
			string[] textColumnNames = new string[]{"text1", "text2", "text3", "text4", "text5", "text6"};
			minigameElement.text = new List<string> ();
			foreach(string textColumnName in textColumnNames)
			{
				minigameElement.text.Add(widgetElement[textColumnName].Replace("’", "'").Replace("‘", "'").Replace("\u00a0", " ")); // Replacing \u00a0 (nbsp) with a normal space
			}

			//Load character
			minigameElement.character = new MinigameCharacter ();
			string characterID = widgetElement ["characterid"];
			if(string.IsNullOrEmpty(characterID) == false && characterID != "0")
			{
				List<Dictionary<string, string>> characterQuery = Database.Instance.Select ("*", "Character", "id=" + characterID);
				if(characterQuery.Count > 0)
				{
					minigameElement.character.characterID = characterQuery[0]["characterid"];
					minigameElement.character.characterName = characterQuery[0]["charactername"];
				}
			}

			//Load illustration
			string[] illustrationColumnNames = new string[]{"illustrationid", "illustrationid2", "illustrationid3", "illustrationid4" };
			minigameElement.illustrations = new List<MinigameIllustration> ();

			foreach ( string illustrationColumn in illustrationColumnNames ) {

				string illustrationID = widgetElement[ illustrationColumn ];

				if(string.IsNullOrEmpty(illustrationID) == false && illustrationID != "0")
				{
					List<Dictionary<string, string>> illustrationQuery = Database.Instance.Select ("*", "Illustration", "id=" + illustrationID);
					if(illustrationQuery.Count > 0)
					{
						MinigameIllustration illustration = new MinigameIllustration();
						illustration.description = illustrationQuery[0]["description"].Replace("’", "'").Replace("‘", "'");
						illustration.unityRef = illustrationQuery[0]["unityref"];
                        int.TryParse(illustrationQuery[0]["activityid"], out illustration.activityID);
						minigameElement.illustrations.Add( illustration );
					}
				}
			}

			//Load sundry
			minigameElement.sundry = new MinigameSundry ();
			string sundryID = widgetElement ["sundryid"];
			if(string.IsNullOrEmpty(sundryID) == false && sundryID != "0")
			{
				List<Dictionary<string, string>> sundryQuery = Database.Instance.Select ("*", "Sundry", "id=" + sundryID);
				if(sundryQuery.Count > 0)
				{
					minigameElement.sundry.sundryName = sundryQuery[0]["sundryname"];
					minigameElement.sundry.unityRef = sundryQuery[0]["unityref"];
				}
			}

			//Load animation
			minigameElement.animation = new MinigameAnimation ();
			string animationID = widgetElement ["animationid"];
			if(string.IsNullOrEmpty(animationID) == false && animationID != "0")
			{
				List<Dictionary<string, string>> animationQuery = Database.Instance.Select ("*", "Animation", "id=" + animationID);
				if (animationQuery.Count > 0)
				{
					minigameElement.animation.animation = animationQuery [0]["animationname"];
				}
			}

			string notes = widgetElement ["notes"];
			if(string.IsNullOrEmpty(notes) == false && notes != "0")
				minigameElement.notes = notes.Replace("’", "'").Replace("‘", "'");;

			return minigameElement;
		}

		private static void DebugPrintElement(MinigameElementData data)
		{
			Debug.Log ("MG ELEMENT ---------------------");
			Debug.Log ("Sequence: " + data.sequence);
			Debug.Log ("Subtype: " + data.subType);
			Debug.Log ("-- bools --");
			foreach(bool mgBool in data.bools)
			{
				Debug.Log ("Bool: " + mgBool);
			}
			Debug.Log ("--------");
			Debug.Log ("-- text --");
			foreach(string mgText in data.text)
			{
				Debug.Log ("Text: " + mgText);
			}
			Debug.Log ("--------");
			Debug.Log ("-- Audio --");
			foreach(MinigameAudio mgAudio in data.audio)
			{
				Debug.Log ("Audio: " + mgAudio.fileName + ", " + mgAudio.dialog);
			}
			Debug.Log ("--------");
			Debug.Log ("Anim: " + data.animation.animation);
			Debug.Log ("Character: " + data.character.characterID + " " + data.character.characterName);
			Debug.Log ("Sundry: " + data.sundry.sundryName + " " + data.sundry.unityRef);
			Debug.Log ("Illustration: " + data.illustration.description + " " + data.illustration.unityRef);
			Debug.Log ("Notes: " + data.notes);
			Debug.Log ("--------------------------------");
		}
	}
}

