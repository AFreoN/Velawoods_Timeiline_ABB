using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CoreSystem
{
    public class CoreHelper : MonoBehaviour
    {
		public struct SFX {
			public const string SELECT = "UI_Select";
			public const string SPEECH_BUBBLE_ON = "Speech_Bubble_On";
			public const string SPEECH_BUBBLE_OFF = "Speech_Bubble_Off";
			public const string UI_SLIDE_1 = "UI_Slide_1";
			public const string UI_SLIDE_2 = "UI_Slide_2";
			public const string CAMERA_SHUTTER = "Camera_Shutter";
			public const string CORRECT_ANSWER = "Correct_Answer";
			public const string WRONG_ANSWER = "Wrong_Answer";
			public const string POSITIVE_ALERT = "Positive_Alert";
			public const string NEGATIVE_ALERT = "Negative_Alert";
			public const string SHORT_BEEP = "Short_Beep";
		}

		public struct COLORS {
			public static int CORRECT_ANSWER = 0x008623;
			public static int WRONG_ANSWER = 0xC70C0C;
		}

		public struct TEXT_COLORS {
			public static string CORRECT_ANSWER = "#008623";
			public static string WRONG_ANSWER = "#C70C0C";
		}

		public static T ParseEnum<T>(string value){
			T resultEnum = default (T);
			try {
				resultEnum = (T) System.Enum.Parse(typeof(T), value, true);
			}catch (System.ArgumentException arg){
				Debug.LogError ("ParseEnum ArgumentException: Couldn't convert [" + value + "] to " + typeof(T) + ".");
                Debug.LogError(arg.Message);
			}

			return resultEnum;
		}

        //-------------------------------------------------------------------------------------
        //useful 3D bezier calculations for 3 point bezier (p0-2)
        //Note: t is distance along the curve where 0.0f = start and 1.0f = end of curve
        // p0 = start point of curve, p1 = control point(central point), p2 = end point
        public static Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;

            Vector3 p = uu * p0;
            p += 2 * u * t * p1;
            p += tt * p2;

            return p;		//point along the provided bezier
        }

        //-------------------------------------------------------
        //eases in and out of value
        public static float Hermite(float start, float end, float value)
        {
            return Mathf.Lerp(start, end, value * value * (3.0f - 2.0f * value));
        }


        //-------------------------------------------------------
        //eases at end near 1
        public static float Sinerp(float start, float end, float value)
        {
            return Mathf.Lerp(start, end, Mathf.Sin(value * Mathf.PI * 0.5f));
        }

		public static Sprite GetSprite(string spriteSheet, string spriteName)
		{
			if(StorageManager.Instance.SpriteExists(spriteName)) return StorageManager.Instance.GetSprite(spriteName, null);

			Sprite[] sprites = Resources.LoadAll<Sprite>(spriteSheet);
			Sprite spriteToReturn = null;

			foreach(Sprite sprite in sprites)
			{
				StorageManager.Instance.StoreSprite(sprite.name, sprite);

				if(sprite.name == spriteName)
				{
					spriteToReturn = sprite;
				}
			}

			return spriteToReturn;
		}

        //-------------------------------------------------------
        //ever decreasing bounce, x = 0 to 1.0f
        public static float Bounce(float x)
        {
            return Mathf.Abs(Mathf.Sin(6.28f * (x + 1f) * (x + 1f)) * (1f - x));
        }

        public static List<T> RandomizeList<T>(List<T> items) 
        {
            List<T> new_list = items;

            //Fisher Yates shuffle algorithm
            for (int i = 0; i < new_list.Count; i++)
            {
                T temp = new_list[i];
                int randomIndex = Random.Range(i, new_list.Count);
                new_list[i] = new_list[randomIndex];
                new_list[randomIndex] = temp;
            }

            return new_list;
        }

		public static T[] RandomizeArray<T> (T[] items)
		{
			T[] new_array = items;

			//Fisher Yates shuffle algorithm
            for (int i = 0; i < new_array.Length; i++)
            {
                T temp = new_array[i];
                int randomIndex = Random.Range(i, new_array.Length);
                new_array[i] = new_array[randomIndex];
                new_array[randomIndex] = temp;
            }

            return new_array;
		}

		/// <summary> Uses the correct destroy method dependant on if the game is running. </summary>
		public static void SafeDestroy(Object obj)
		{
#if UNITY_EDITOR
			if(EditorApplication.isPlaying)
			{
				GameObject.Destroy(obj);
			}
			else
			{
				GameObject.DestroyImmediate(obj, true);
			}
#else
			GameObject.Destroy(obj);
#endif
		}
        
#region general converters

		// Parsing DialogueData and DialogueText tags
		// 1st pass
		public static string[] LEARNER_SHARED_TAGS = new string[] {"LN_", "learner"};
        public static string[] LEARNER_FIRST_NAME_TAGS = new string[] {"[LN_FIRST_NAME]", "[learner's first name]", "(learner's name)", "[learner's name]"};
        public static string[] LEARNER_LAST_NAME_TAGS = new string[] {"[LN_LAST_NAME]", "[learner's last name]", "[Learner's surname]"};
        public static string[] LEARNER_COUNTRY_TAGS = new string[] {"[LN_COUNTRY]", "(learner's country)"};
        public static string[] LEARNER_NATIONALITY_TAGS = new string[] {"[LN_NATIONALITY]"};
        public static string[] LEARNER_HYPHENATED_FIRST_NAME_TAGS = new string[] {"[LN_FIRST_NAME_SPELL]", "[Learner's first name as letters hyphenated]"};
        public static string[] LEARNER_HYPHENATED_LAST_NAME_TAGS = new string[] {"[LN_LAST_NAME_SPELL]", "[Learner's last name as letters hyphenated]"};
		// 2nd pass
		static string[] HIDDEN_TEXT_TAGS = new string[] {"[", "]"};
		//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
	
		public static DialogueEventData.DialogueData ParseDialogueData (DialogueEventData.DialogueData dialogueData)
		{
			for (int i=0; i<dialogueData.dialogueText.Count; i++)
			{
				// Remove model answers in the case of hyphenated names
				bool removeTutorAudio = false;
				removeTutorAudio = ContainsTags (dialogueData.dialogueText [i].text, LEARNER_HYPHENATED_FIRST_NAME_TAGS) || ContainsTags (dialogueData.dialogueText [i].text, LEARNER_HYPHENATED_LAST_NAME_TAGS);
				if (removeTutorAudio)
				{
					Debug.Log ("Removing Model Answers...");
					dialogueData.tutorAudioClips.angela = null;
					dialogueData.tutorAudioClips.jack = null;
					
					DialogueEventData.DialogueText temp = dialogueData.dialogueText [i];
					temp.tutorAudioClips.male = null;
					temp.tutorAudioClips.female = null;
					dialogueData.dialogueText [i] = temp;
				}
				
				// If there are no model answers in the dialogue texts, but there are in the dialogue event data, assign the latter to the dialogue texts
				DialogueEventData.DialogueText temp2 = dialogueData.dialogueText [i];
				if (dialogueData.tutorAudioClips.angela != null && temp2.tutorAudioClips.female == null)
					temp2.tutorAudioClips.female = dialogueData.tutorAudioClips.angela;
				if (dialogueData.tutorAudioClips.jack != null && temp2.tutorAudioClips.male == null)
					temp2.tutorAudioClips.male = dialogueData.tutorAudioClips.jack;
				dialogueData.dialogueText [i] = temp2;
				
				// Parse
				dialogueData.dialogueText [i] = ParseDialogueText (dialogueData.dialogueText [i]);
			}
			return dialogueData;
		}

        /// <summary>
        /// Goes through the DialogueText structure and replaces tags where necessary. Returns a copy of the parsed DialogueText. </summary>
        public static DialogueEventData.DialogueText ParseDialogueText (DialogueEventData.DialogueText dialogueText)
		{
			string LEARNER_FIRST_NAME = "Paulo";
			string LEARNER_LAST_NAME = "Sanchez";
			string LEARNER_COUNTRY = "Spain";
			string LEARNER_NATIONALITY = "_____";
		
#if CLIENT_BUILD
			LEARNER_FIRST_NAME = PlayerProfile.Instance.FirstName;
			LEARNER_LAST_NAME  = PlayerProfile.Instance.LastName;
			LEARNER_COUNTRY = PlayerProfile.Instance.Country.Name;
#endif
			
			try {
				DialogueEventData.DialogueText parsedDialogueText = new DialogueEventData.DialogueText ();
				parsedDialogueText = dialogueText;

				// Check and remove empty carnegie strings
				if (parsedDialogueText.carnegieText != null && parsedDialogueText.carnegieText.Length > 0)
				{
					int validStrings = 0;
					foreach (string str in parsedDialogueText.carnegieText)
						if (!string.IsNullOrEmpty (str))
							validStrings++;
					if (validStrings != parsedDialogueText.carnegieText.Length) {
						string[] parsedCarnegieText = new string [validStrings];
						int count = 0;
						foreach (string str in parsedDialogueText.carnegieText) {
							if (!string.IsNullOrEmpty (str)) {
								parsedCarnegieText [count] = str;
								count ++;
							}
						}
						parsedDialogueText.carnegieText = parsedCarnegieText;
					}
				}
				
				// Initial replacement of characters
                parsedDialogueText.text = parsedDialogueText.text.Replace("’", "'").Replace("‘", "'");
				if (parsedDialogueText.carnegieText != null)
					for (int i=0; i<parsedDialogueText.carnegieText.Length; i++)
                        parsedDialogueText.carnegieText[i] = parsedDialogueText.carnegieText[i].Replace("’", "'").Replace("‘", "'").Replace("&", "and");
			
				// Set original text
				parsedDialogueText.carnegieOriginalText = parsedDialogueText.text;
			
			// 1st PASS
				
				// Check if learner
				bool isLearnerTag = false;
				for (int i=0; i<LEARNER_SHARED_TAGS.Length; i++)
					if (parsedDialogueText.text.ToLower ().Contains (LEARNER_SHARED_TAGS [i].ToLower ()))
						isLearnerTag = true;
						
			    if (isLearnerTag)
				{
					// Switch to learner's name
					parsedDialogueText.text = ReplaceTags (parsedDialogueText.text, LEARNER_FIRST_NAME_TAGS, LEARNER_FIRST_NAME);
					parsedDialogueText.text = ReplaceTags (parsedDialogueText.text, LEARNER_LAST_NAME_TAGS, LEARNER_LAST_NAME);
					parsedDialogueText.text = ReplaceTags (parsedDialogueText.text, LEARNER_COUNTRY_TAGS, LEARNER_COUNTRY);
					parsedDialogueText.text = ReplaceTags (parsedDialogueText.text, LEARNER_NATIONALITY_TAGS, LEARNER_NATIONALITY);
					parsedDialogueText.text = ReplaceTags (parsedDialogueText.text, LEARNER_HYPHENATED_FIRST_NAME_TAGS, HyphenatedText (LEARNER_FIRST_NAME, true, true));
					parsedDialogueText.text = ReplaceTags (parsedDialogueText.text, LEARNER_HYPHENATED_LAST_NAME_TAGS, HyphenatedText (LEARNER_LAST_NAME, true, true));
					
					// Check carnegie text
					if (parsedDialogueText.carnegieText != null && parsedDialogueText.carnegieText.Length > 0)
					{
						// If carnegie text present, remove existing tags
						if (parsedDialogueText.carnegieText != null)
							for (int j=0; j<parsedDialogueText.carnegieText.Length; j++)
							{
								parsedDialogueText.carnegieText [j] = removeRichTextTags (parsedDialogueText.carnegieText [j]);
                                parsedDialogueText.carnegieText[j] = parsedDialogueText.carnegieText[j].Replace("’", "'").Replace("‘", "'"); ;
								parsedDialogueText.carnegieText [j] = ReplaceTags (parsedDialogueText.carnegieText [j], LEARNER_FIRST_NAME_TAGS, "");
								parsedDialogueText.carnegieText [j] = ReplaceTags (parsedDialogueText.carnegieText [j], LEARNER_LAST_NAME_TAGS, "");
								parsedDialogueText.carnegieText [j] = ReplaceTags (parsedDialogueText.carnegieText [j], LEARNER_COUNTRY_TAGS, "");
								parsedDialogueText.carnegieText [j] = ReplaceTags (parsedDialogueText.carnegieText [j], LEARNER_NATIONALITY_TAGS, "");
								parsedDialogueText.carnegieText [j] = ReplaceTags (parsedDialogueText.carnegieText [j], LEARNER_HYPHENATED_FIRST_NAME_TAGS, HyphenatedText (StripAccents (LEARNER_FIRST_NAME), false, true));
								parsedDialogueText.carnegieText [j] = ReplaceTags (parsedDialogueText.carnegieText [j], LEARNER_HYPHENATED_LAST_NAME_TAGS, HyphenatedText (StripAccents (LEARNER_LAST_NAME), false, true));
							}
					}
					else
					{
						// Carnegie text not present, add a copy of the original text and remove tags
						parsedDialogueText.carnegieText = new string[1];
						parsedDialogueText.carnegieText [0] = parsedDialogueText.text;
						parsedDialogueText.carnegieText [0] = removeRichTextTags (parsedDialogueText.carnegieText [0]);
						parsedDialogueText.carnegieText [0] = parsedDialogueText.carnegieText [0].Replace (LEARNER_FIRST_NAME, "");
						parsedDialogueText.carnegieText [0] = parsedDialogueText.carnegieText [0].Replace (LEARNER_LAST_NAME, "");
						parsedDialogueText.carnegieText [0] = parsedDialogueText.carnegieText [0].Replace (LEARNER_COUNTRY, "");
						parsedDialogueText.carnegieText [0] = parsedDialogueText.carnegieText [0].Replace (LEARNER_NATIONALITY, "");
						parsedDialogueText.carnegieText [0] = parsedDialogueText.carnegieText [0].Replace (HyphenatedText (LEARNER_FIRST_NAME, true, true), HyphenatedText (StripAccents (LEARNER_FIRST_NAME), false, true));
						parsedDialogueText.carnegieText [0] = parsedDialogueText.carnegieText [0].Replace (HyphenatedText (LEARNER_LAST_NAME, true, true), HyphenatedText (StripAccents (LEARNER_LAST_NAME), false, true));
					}
					
					parsedDialogueText.carnegieOriginalText = parsedDialogueText.text;
				}
				
				
			// 2nd PASS
				
				// Check if it contains tags for text that is supposed to be hidden
				while (parsedDialogueText.text.Contains (HIDDEN_TEXT_TAGS [0]) && parsedDialogueText.text.Contains (HIDDEN_TEXT_TAGS [1]))
				{
					parsedDialogueText.carnegieOriginalText = ReplaceTags (parsedDialogueText.carnegieOriginalText, HIDDEN_TEXT_TAGS, "");
				
					int startIndex = parsedDialogueText.text.IndexOf (HIDDEN_TEXT_TAGS [0]);
					int endIndex = parsedDialogueText.text.IndexOf (HIDDEN_TEXT_TAGS [1]);
					
					string hiddenText = parsedDialogueText.text.Substring (startIndex, endIndex - startIndex + 1);
					string replaceWith = "";
					for (int textCount=0; textCount<hiddenText.Length; textCount++)
						replaceWith += (hiddenText [textCount] == ' ') ? " " : "_";
					
					parsedDialogueText.text = parsedDialogueText.text.Replace (hiddenText, replaceWith);
					
					// Check carnegie text
					if (parsedDialogueText.carnegieText != null && parsedDialogueText.carnegieText.Length > 0)
					{
						// If carnegie text present, remove existing tags
						if (parsedDialogueText.carnegieText != null)
							for (int j=0; j<parsedDialogueText.carnegieText.Length; j++)
						{
							parsedDialogueText.carnegieText [j] = parsedDialogueText.carnegieText [j].Replace (HIDDEN_TEXT_TAGS [0], "");
							parsedDialogueText.carnegieText [j] = parsedDialogueText.carnegieText [j].Replace (HIDDEN_TEXT_TAGS [1], "");
						}
					}
					else
					{
						// Carnegie text not present, add a copy of the original text and replace underlines with hidden text
						parsedDialogueText.carnegieText = new string[1];
						parsedDialogueText.carnegieText [0] = parsedDialogueText.text.Replace (replaceWith, hiddenText.Substring (HIDDEN_TEXT_TAGS [0].Length, hiddenText.Length-(HIDDEN_TEXT_TAGS [1].Length + 1)));
					}
				}
				
				return parsedDialogueText;
			}
			catch(System.Exception e) 
			{ 
				Debug.LogWarning ("DialogueEvent : " + e); 
				return dialogueText;
			}
		}

        /// <summary>
        /// Replaces the learner data tags with data from the Player Profile.
        /// </summary>
        /// <returns>The original string with tags replaced by data.</returns>
        /// <param name="original">Original.</param>
        public static string ReplaceLearnerDataTags( string original ) {

			string LEARNER_FIRST_NAME = "Paulo";
			string LEARNER_LAST_NAME = "Sanchez";
			string LEARNER_COUNTRY = "Spain";
			string LEARNER_NATIONALITY = "_____";
			
			#if CLIENT_BUILD
			LEARNER_FIRST_NAME = PlayerProfile.Instance.FirstName;
			LEARNER_LAST_NAME  = PlayerProfile.Instance.LastName;
			LEARNER_COUNTRY = PlayerProfile.Instance.Country.Name;
			#endif

            string edited = original.Replace("’", "'").Replace("‘", "'");
			edited = ReplaceTags (edited, LEARNER_FIRST_NAME_TAGS, LEARNER_FIRST_NAME);
			edited = ReplaceTags (edited, LEARNER_LAST_NAME_TAGS, LEARNER_LAST_NAME);
			edited = ReplaceTags (edited, LEARNER_COUNTRY_TAGS, LEARNER_COUNTRY);
			edited = ReplaceTags (edited, LEARNER_NATIONALITY_TAGS, LEARNER_NATIONALITY);
			edited = ReplaceTags (edited, LEARNER_HYPHENATED_FIRST_NAME_TAGS, HyphenatedText (LEARNER_FIRST_NAME, true, true));
			edited = ReplaceTags (edited, LEARNER_HYPHENATED_LAST_NAME_TAGS, HyphenatedText (LEARNER_LAST_NAME, true, true));

			return edited;

		}
	
		public static string StripAccents (string target)
		{
			string result = "";
			for (int i=0; i<target.Length; i++)
			{
				if (UnidecodeSharpFork.Unidecoder.Unidecode (target [i]) == target [i].ToString ())
				{
					result += target [i];
				}
			}
			
			return result;
		}

        public static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
		
		/// <summary>
		/// Returns the hyphenated input string. (Ballbags -> B-a-l-l-b-a-g-s). 
		/// Hyphens being replaced by empty spaces when useHyphens is false. </summary>
		private static string HyphenatedText (string text, bool useHyphens = true, bool allCaps = false)
		{
			string result = "";
			
			for (int i=0; i<text.Length-1; i++)
			{
				result += text[i] + ((useHyphens) ? "-" : " ");
			}
			result += text [text.Length - 1].ToString ();
			
			if (allCaps)
				result = result.ToUpper ();
			
			return result;
		}
		
		/// <summary>
		/// If the target text contains any of the tags in the array, returns true. Otherwise returns false. </summary>
		private static bool ContainsTags (string targetText, string[] tags)
		{
			for (int i=0; i<tags.Length; i++)
				if (targetText.Contains (tags [i]))
					return true;
			return false;
		}
		
		/// <summary>
		/// Replaces an array of strings (tags) within a target string (targetText) with another string (replaceWith). </summary>
		private static string ReplaceTags (string targetText, string[] tags, string replaceWith)
		{
			for (int tagNo=0; tagNo<tags.Length; tagNo++)
				targetText = targetText.Replace (tags [tagNo], replaceWith);
			return targetText;
		}
	
		/// <summary>
		/// Removes html / richtext tags. Ex: input: "<color>text</color>", output: "text"
		/// </summary>
		public static string removeRichTextTags (string richText)
		{
			while (richText.Contains ("<"))
			{
				int startPos = richText.IndexOf ("<");
				int tagLength = 0;
				for (int i=startPos; i<richText.Length; i++) {
					tagLength ++;
					if (richText [i] == '>') break;
				}
				richText = richText.Remove (startPos, tagLength);
			}
			return richText;
		}

		/// <summary> Converts Unity's color to hex string  </summary>
		public static string rgbToHex (Color rgbColor) { return string.Format ("{0:X2}{1:X2}{2:X2}", (int)(rgbColor.r * 255), (int)(rgbColor.g * 255), (int)(rgbColor.b * 255)); }
		/// <summary> Converts RGB values to hex string  </summary>
		public static string rgbToHex (float red, float green, float blue) { return string.Format ("{0:X2}{1:X2}{2:X2}", (int)(red * 255), (int)(green * 255), (int)(blue * 255)); }
		
		/// <summary> Converts Hex value (e.g. 0xFFFFFF) to a Color object</summary>
		public static Color HexToColor( int hex ) { return new Color( ((hex & 0xFF0000) >> 16) / 255.0f, ((hex & 0x00FF00) >> 8) / 255.0f, (hex & 0x0000FF) / 255.0f ); }
	
		/// <summary> Converts Hex value (e.g. 0xFFFFFF) to a Color object</summary>
		public static Color HexToColor( string htmlHex ) { 
			return HexToColor( System.Int32.Parse (htmlHex.Substring (1), System.Globalization.NumberStyles.HexNumber) );
		}
	
		/// <summary> Converts hex string to a binary string. </summary>
		public static string hexToBinary (string hex)
		{
			string bin = "";
			for (int i=0; i<hex.Length; i++)
			{
				bin += charToBinary (hex [i]);
			}
			return bin;
		}
		/// <summary> Converts a character to a 4-letter binary string. </summary>
		public static string charToBinary (char c)
		{
			switch (c.ToString ().ToUpper ())
			{
			case "0" : return "0000"; case "1" : return "0001";
			case "2" : return "0010"; case "3" : return "0011";
			case "4" : return "0100"; case "5" : return "0101";
			case "6" : return "0110"; case "7" : return "0111";
			case "8" : return "1000"; case "9" : return "1001";
			case "A" : return "1010"; case "B" : return "1011";
			case "C" : return "1100"; case "D" : return "1101";
			case "E" : return "1110"; case "F" : return "1111";
			default:
				Debug.LogError ("CoreHelper.cs: charToBinary() error: " + "Argument not recognized: " + c + ".Accepting only [0..9] & [A..F]. ");
				return "0000";
			}
		}
	
		/// <summary>Converts a html character with format &<charName>; to the unicode equivalent.</summary>
		/// <returns>The a converted unicode character.</returns>
		/// <param name="HTML">The html character to be converted.</param>
		public static string ConvertHTMLToUnicode(string HTML)
		{
			HTML = HTML.Replace("&", "").Replace(";", "");
			
			foreach(KeyValuePair<string, string> entry in HTMLTextEntities.UnicodeHTMLList)
			{
				if(entry.Value == HTML)
				{
					return entry.Key;
				}
			}
			
			return HTML;
		}

        /// <summary>
        /// Converts a string from the database which contains tags (e.g. <li>) and character codes (e.g. &amp;)
        /// and return a displayable string.
        /// </summary>
        /// <param name="databaseString"></param>
        public static string ParseDatabaseStringWithTagsAndChars(string databaseString)
        {
            // Replace the <ul> (assuming will be followed by < for either <li> or <\ul>) tags and any following whitespace
            databaseString = Regex.Replace(databaseString, @"<ul>[^<]*", "");
            databaseString = databaseString.Replace("\\r\\n", "\n");

            // Replace tags/characters that we know need to be replaced and not removed
            databaseString = databaseString.Replace("<li>", "\u2022 ");

            // Remove all remaining tags in the text
            databaseString = removeRichTextTags(databaseString);

            // Replace all character codes in the text if we know what they should be replace with
            foreach (Match tagMatch in Regex.Matches(databaseString, @"&[^;]+;"))
            {
                databaseString = databaseString.Replace(tagMatch.Value, HTMLTextEntities.GetUnicodeForHTMLCode(tagMatch.Value));
            }

            // Replace apostrophes that should not be present in strings
            databaseString = databaseString.Replace("’", "'").Replace("‘", "'");

            return databaseString;
        }


		/// <summary> Converts a whole List of Practice Data to String. </summary>
#if CLIENT_BUILD
		public static string PracticeDataToString (List<PracticeActivitySectionData> data){
			string str = "";
			
			str += "[DATA]\n" + "- <i>Count:</i> " + data.Count + "\n";
			
			for (int i=0; i< data.Count; i++) {
				str += "- <i>id:</i> " + data[i].id + "\n";
				str += "- <i>missionDBID:</i> " + data[i].missionDBID + "\n";
				//str += "- <i>Description:</i> " + data[i].description + "\n";
				str += "- <i>Element count:</i> " + data[i].elements.Count + "\n";

				str += "===\n[ELEMENTS]\n";
				
				for (int j=0; j< data[i].elements.Count; j++) {
					str += "(#" + j + ")\n";
					str += "- <i>Character:</i> " + MGCharaToString(data[i].elements[i].character) +"\n";
					str += "- <i>Text:</i> " + ListToString(data[i].elements[j].text) + "\n";
					str += "- <i>Illustration:</i> " + MGIllustToString(data[i].elements[j].illustration) +"\n";
					str += "- <i>Audio:</i> " + MGAListToString(data[i].elements[j].audio) +"\n";
					str += "---\n";
				}
			}
			
			return str;
		}
#endif
		/// <summary> Converts a whole List of MinigameSectionData to String. </summary>
		public static string DataToString (List<MinigameSectionData> data){
			string str = "";
			
			str += "[DATA]\n" + "- <i>Count:</i> " + data.Count + "\n";
			str += "- <i>Section:</i> " + data[0].section + "\n";
			str += "- <i>SubType:</i> " + data[0].subType + "\n";
			str += "- <i>Element Count:</i> " + data[0].elements.Count + "\n";
			str += "- <i>Illustration:</i> " + MGIllustToString(data[0].illustration) +"\n";
			
			str += "- <i>Bools:</i> " + ListToString(data[0].bools) + "\n";
			str += "- <i>Text:</i> " + ListToString(data[0].text) + "\n";
			
			str += "===\n[ELEMENTS]\n";
			
			for (int i=0; i< data[0].elements.Count; i++) {
				str += "(#" + i + ")\n";
				str += "- <i>Character:</i> " + MGCharaToString(data[0].elements[i].character) +"\n";
				str += "- <i>Illustration:</i> " + MGIllustToString(data[0].elements[i].illustration) +"\n";
				str += "- <i>Audio:</i> " + MGAListToString(data[0].elements[i].audio) +"\n";
				str += "- <i>Text:</i> " + ListToString(data[0].elements[i].text) + "\n";
				str += "---\n";
			}
			
			return str;
		}

		/// <summary> Converts a Array of items to String. Returns Array[Length]{1,2,3} </summary>
		public static string ArrayToString<T> (T[] items){
			string str = "Array["+ items.Length + "] { ";
			
			if (items.Length == 0)
				str += "EMPTY ";
			else {
				for (int i = 0; i < items.Length; i++) {
					str += "" + items [i];
					if (i != items.Length - 1)
						str += ",";
					str += " ";
				}
			}
			str += "}";
			
			return str;
		}

		/// <summary> Converts a List of items to String. Returns List[Count]{1,2,3} </summary>
		public static string ListToString<T> (List<T> items){
			string str = "List["+ items.Count + "] { ";
			
			if (items.Count == 0)
				str += "EMPTY ";
			else {
				for (int i = 0; i < items.Count; i++) {
					if (items [i] == null) str += "(NULL)";
					else str += "" + items [i];
					if (i != items.Count - 1)
						str += ",";
					str += " ";
				}
			}
			str += "}";
			
			return str;
		}

		/// <summary> Converts the contents of a MinigameCharacter to String. </summary>
		public static string MGCharaToString (MinigameCharacter mgc){
			if (mgc == null || mgc.characterID == null)
				return "[NULL]";
			else
				return "ID [" + mgc.characterID + "] ¦ Name [" + mgc.characterName + "]";
		}

		/// <summary> Converts the contents of a MinigameIllustration to String. </summary>
		public static string MGIllustToString (MinigameIllustration mgi){
			string str = "unityRef [" + mgi.unityRef + "] ¦ Description [" + mgi.description + "]";
			return str;
		}
		
		/// <summary> Converts the contents of a MinigameAudio to String. </summary>
		public static string MGAudioToString (MinigameAudio mga){
			string str = "filename [" + mga.fileName + "] ¦ dialog [" + mga.dialog + "]\n";
			return str;
		}

		/// <summary> Converts the contents of a List of MinigameAudio to String. </summary>
		public static string MGAListToString (List<MinigameAudio> l){
			if (l == null || l.Count == 0)
				return "[MGAList is EMPTY]";

			string str = "List[" + l.Count + "]{ ";
			for (int i=0; i<l.Count; i++) {
				str += "\n-- MGA #" + i + ": " + MGAudioToString(l[i]);
				if (i != l.Count - 1) str += ",";
			}
			str += "}";
			return str;
		}

		public static void LogCurrentHierarchy (){
			Debug.Log ("<b>Current Hierarchy</b>: L" + ActivityTracker.Instance.Level + " C" + ActivityTracker.Instance.Course + " S" + ActivityTracker.Instance.Scenario + " M" + ActivityTracker.Instance.Mission + " T" + ActivityTracker.Instance.Task + " A" + ActivityTracker.Instance.Activity);
		}

		public static void PlayAudio (string audioName, float volumeScale = 1.0f, float delay = 0f){
			if (AudioManager.Instance != null) 
				AudioManager.Instance.PlayAudio ("Audio/" + audioName, CoreSystem.AudioType.SFX, volumeScale, delay);
		}

		public static void PlayAudioClip (AudioClip au, CoreSystem.AudioType type, float volumeScale = 1f, float delay = 0f){
			if (AudioManager.Instance != null)
				AudioManager.Instance.PlayAudio (au, type, volumeScale, delay);
		}

		public static Sprite GetCharacterCircle (MinigameCharacter chara) {
			return Resources.Load<Sprite> ("UI/CharacterCircles/SPR_" + chara.characterID + "_" + chara.characterName + "_Circle");
		}

		public struct LevelData
		{
			public string name;
			public string id;
		}

		private static List<LevelData> _levelsData = new List<LevelData> ();
		public static List<LevelData> LevelsData {
			get {
				if (_levelsData == null || _levelsData.Count == 0)
				{
					List<Dictionary<string, string>> levelNameQuery = Database.Instance.Select ("*", "Level");
		
					foreach(Dictionary<string, string> levelEntry in levelNameQuery)
					{
						LevelData loadedLevel = new LevelData();
						loadedLevel.name = levelEntry["levelname"];
						loadedLevel.id = "L" + levelEntry["levelid"];
						_levelsData.Add(loadedLevel);
					}
				}
				return _levelsData;
			}
		}
		
		public delegate void OnStreamEndImage (Image image, string spriteName);
		public delegate void OnStreamEndSprite (Sprite sprite, string spriteName);

		public enum StreamingAssetsSpriteFolder
		{
			Minigames, PracticeActivities, CourseTest, ScenarioTest, Notebook, Scrapbook, LessonMap
		}


        public static void LoadStreamingAssetsSprite(string spriteName, MonoBehaviour caller, OnStreamEndSprite OnEnd = null,  StreamingAssetsSpriteFolder spriteFolder = StreamingAssetsSpriteFolder.Minigames)
        {
            caller.StartCoroutine(LoadImage(spriteFolder, false, null, spriteName, caller, null, OnEnd));
        }



		/// <summary>  Loads 'spriteName'.png from the appropriate 'spriteFolder'. A coroutine will be started on the 'caller' behaviour in which a www stream will try to load the sprite into 'image', passing it through the OnEnd(Image) delegate as well. </summary>
		public static void LoadStreamingAssetsSprite (StreamingAssetsSpriteFolder spriteFolder, Image image, string spriteName, MonoBehaviour caller, OnStreamEndImage OnEnd = null, string Level = "A1" )
		{
			caller.StartCoroutine (LoadImage (spriteFolder, true, image, spriteName, caller, OnEnd, null, Level));
		}
		/// <summary>  Loads 'spriteName'.png from the appropriate 'spriteFolder'. A coroutine will be started on the 'caller' behaviour in which a www stream will try to load the sprite and pass it through the OnEnd(Sprite) delegate. </summary>
		public static void LoadStreamingAssetsSprite (StreamingAssetsSpriteFolder spriteFolder, string spriteName, MonoBehaviour caller, OnStreamEndSprite OnEnd = null )
		{
            caller.StartCoroutine (LoadImage (spriteFolder, false, null, spriteName, caller, null, OnEnd));
		}

        private static IEnumerator LoadImage(StreamingAssetsSpriteFolder spriteFolder, bool assignToImage, Image image, string spriteName, MonoBehaviour caller, OnStreamEndImage OnEndImage, OnStreamEndSprite OnEndSprite, string Level = "A1")
		{
			string originalSpriteName = spriteName;

			Sprite textureAsSprite = null;
			
			if (spriteName == null || spriteName.Trim() == "")
			{
				Debug.LogWarning("Unable to load sprite, sprite name string is null or empty!");
			}
			else
			{
				if (spriteName.Contains (".png"))
					spriteName = spriteName.Replace (".png", "");
				if (spriteName.Contains (".jpg"))
					spriteName = spriteName.Replace (".jpg", "");

                string mainFolder = "Minigames/Sprites/";

                // See if the main folder needs to change.
                switch (spriteFolder)
                {
                    case StreamingAssetsSpriteFolder.LessonMap: 
						mainFolder = "LessonMap/";
						break;
                }
			
				// Set level folder
				switch (spriteFolder)
				{
					// Trackable via ActivityTracker
					case StreamingAssetsSpriteFolder.Minigames:
					case StreamingAssetsSpriteFolder.CourseTest:
					case StreamingAssetsSpriteFolder.ScenarioTest:
                        mainFolder += LevelsData [(int)ActivityTracker.Instance.Level - 1].name + "/";
						break;

					// Practice Activities
					case StreamingAssetsSpriteFolder.Notebook:
					case StreamingAssetsSpriteFolder.Scrapbook:
                    case StreamingAssetsSpriteFolder.LessonMap:
#if CLIENT_BUILD
                        mainFolder += Level + "/";
#endif
						break;
					
					// Practice Activities from the Practice Menu don't update the Activity Tracker, so we use this instead
					case StreamingAssetsSpriteFolder.PracticeActivities:
#if CLIENT_BUILD
                        mainFolder += LevelsData[int.Parse(CourseSelectionBar.instance.currentSelectedCourseButton.courseButtonID) - 1].name + "/";
#endif
                        break;
				}
			
				string spritePath = "";
				switch (spriteFolder)
				{
					case StreamingAssetsSpriteFolder.Minigames:
					case StreamingAssetsSpriteFolder.PracticeActivities:
					case StreamingAssetsSpriteFolder.LessonMap:
                        spritePath = mainFolder + spriteName + ".png";
						break;
					case StreamingAssetsSpriteFolder.CourseTest:
						spritePath = mainFolder + "CourseTests/L" + (int)ActivityTracker.Instance.Level + "C" + (int)ActivityTracker.Instance.Course + "/" + spriteName + ".png";
						break;
					case StreamingAssetsSpriteFolder.ScenarioTest:
						spritePath = mainFolder + "ScenarioTests/L" + (int)ActivityTracker.Instance.Level + "C" + (int)ActivityTracker.Instance.Course + "S" + (int)ActivityTracker.Instance.Scenario + "/" + spriteName + ".png";
						break;
					case StreamingAssetsSpriteFolder.Notebook:
						spritePath = mainFolder + "Notebook/" + spriteName + ".png";
						break;
					case StreamingAssetsSpriteFolder.Scrapbook:
						spritePath = mainFolder + "Scrapbook/" + spriteName + ".png";
						break;
					default: 
						Debug.LogErrorFormat ("Streaming Assets Sprite Folder type not recognised! Defaulting to '{0}'", mainFolder);
						spritePath = mainFolder + spriteName + ".png";
						break;
				}
				
#if UNITY_EDITOR
				var path = System.IO.Path.Combine("file://" + Application.streamingAssetsPath, spritePath);
#elif UNITY_ANDROID
				var path = "jar:file://" + Application.dataPath + "!/assets/" + spritePath;
#else 
				string path = System.IO.Path.Combine("file://" + Application.streamingAssetsPath, spritePath);
#endif


#if UNITY_IOS
				// Spaces in filenames will stop iOS loading images
				path = path.Replace(" ", "%20");
#endif
#if DEBUG_BUILD
                if (null != image)
                {
                    GameObject newGameObject;
                    if (image.transform.FindChild("CoreHelperImageDebug(Clone)") == null)
                    {
                        GameObject debugObject = Resources.Load("CoreHelperImageDebug") as GameObject;
                        newGameObject = Instantiate(debugObject, Vector3.zero, Quaternion.identity) as GameObject;
                    }
                    else
                    {
                        newGameObject = image.transform.FindChild("CoreHelperImageDebug(Clone)").gameObject;
                    }

                    newGameObject.SetActive(StephenMenu.ShowImageDebug);

                    RectTransform rectTransform = newGameObject.GetComponent<RectTransform>();
                    rectTransform.SetParent(image.transform);
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.offsetMin = Vector2.zero;
                    rectTransform.offsetMax = Vector2.zero;
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 30.0f);
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 30.0f);
                    rectTransform.localScale = Vector3.one;

                    TMPro.TextMeshProUGUI text = newGameObject.transform.Find("Panel/TextMeshPro Text").GetComponent<TMPro.TextMeshProUGUI>();
                    text.text = string.Format("{0}\n{1}", spriteName, spritePath);
                }
#endif
                

				WWW www = new WWW (path);
				yield return www;
							
				if (www.error != null)
				{
					Debug.LogWarning ("Error while loading sprite <" + spriteName + "> : " + www.error + " ... at path " + spritePath + "\n\n");

                }
				else
				{
#if DEBUG_BUILD
                    if (null != image)
                    {
                        // Delete debug gameObject and enable image element
                        if (image.transform.FindChild("MissingImageDebugText") != null)
                        {
                            DestroyImmediate(image.transform.FindChild("MissingImageDebugText").gameObject);
                        }
                        image.enabled = true;
                    }
#endif

                    textureAsSprite = Sprite.Create (www.texture, new Rect (0, 0, www.texture.width, www.texture.height), new Vector2(www.texture.width/2, www.texture.height/2));
					textureAsSprite.texture.filterMode = FilterMode.Point;
                    textureAsSprite.texture.wrapMode = TextureWrapMode.Clamp;
                    textureAsSprite.texture.anisoLevel = 1;
                    DestroyImmediate(www.texture);
				}
				www.Dispose();
			}
			
			if (assignToImage)
			{
				if (image != null)
					image.sprite = textureAsSprite;
				if (OnEndImage != null)
					OnEndImage (image, originalSpriteName);
			}	
			else
			{
                if (OnEndSprite != null)
                {
                    OnEndSprite(textureAsSprite, originalSpriteName);
                }
			}

            Resources.UnloadUnusedAssets();
		}

#endregion


        public static void ResizeList<T>(int newListSize, List<T> listToRisize) where T : new()
        {
            if (newListSize != listToRisize.Count)
            {
                while (newListSize > listToRisize.Count)
                {
                    listToRisize.Insert(listToRisize.Count, new T());
                }

                while (newListSize < listToRisize.Count)
                {
                    listToRisize.RemoveAt(listToRisize.Count - 1);
                }
            }
        }
    }
}
