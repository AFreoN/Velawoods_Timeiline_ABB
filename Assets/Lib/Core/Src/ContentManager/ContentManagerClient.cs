#if CLIENT_BUILD
using System;
using System.Xml;
using System.Collections.Generic;
using UnityEngine;
using ArabicSupport;
using System.Text.RegularExpressions;

namespace CoreLib
{
	public class ContentManagerClient : IContentManager
	{
		Dictionary<string, Dictionary<string, string>> translations;
				
		public ContentManagerClient()
		{
			XmlDocument xml = LoadAndConcatXMLFiles();
			LoadXMLIntoTranslationDictionaries(xml);
		}
		
		private XmlDocument LoadAndConcatXMLFiles()
		{
			// Merge all files in content_xml into one large xml document
			
			TextAsset[] assets = Resources.LoadAll<TextAsset>("content_xml");
			
			XmlDocument xml = new XmlDocument();
			if (assets.Length > 0)
			{
				bool first = true;
				
				XmlDocument xml_part = new XmlDocument();
				XmlNode documentElement = null;
				translations = new Dictionary<string, Dictionary<string, string>>();
				
				foreach (TextAsset asset in assets)
				{
					if (first)
					{
						xml.LoadXml(asset.text);
						documentElement = xml.SelectSingleNode("/content");
						first = false;
					}
					else
					{
						xml_part.LoadXml(asset.text);
						
						foreach (string language in Enum.GetNames(typeof(PlayerProfile.LanguageCode)))
						{
							XmlNode languageNode = xml_part.SelectSingleNode("/content/" + NormaliseLanguageInitials(language));
							
							if (languageNode != null)
							{
								languageNode = xml.ImportNode(languageNode, true);
								documentElement.AppendChild(languageNode);
							}
						}
					}
				}
			}
			else
			{
				Debug.LogWarning("ContentManager: Could not load content.xml");
			}
			return xml;
		}
		
		private void LoadXMLIntoTranslationDictionaries(XmlDocument xml)
		{
			XmlNode contentNode = xml.SelectSingleNode("content");
			foreach (XmlNode languageTranslations in contentNode)
			{
				string normalisedLanguageInitials = NormaliseLanguageInitials(languageTranslations.Name);
				
				if (translations.ContainsKey(normalisedLanguageInitials) == false)
				{
					translations.Add(normalisedLanguageInitials, new Dictionary<string, string>());
				}
				
				foreach (XmlNode translation in languageTranslations)
				{
					XmlAttribute id = translation.Attributes["id"];
					if (id != null)
					{
						translations[normalisedLanguageInitials].Add(id.Value, translation.InnerText);
					}
				}
			}
		}
		
		private string NormaliseLanguageInitials(string languageInitials)
		{
			return languageInitials.ToLower().Replace("_", "-");
		}
		
		public string getString( string id, string languageInitials="" )
		{
			if(languageInitials == "")
			{
				languageInitials = PlayerProfile.Instance.Language.Initials;
			}
			languageInitials = NormaliseLanguageInitials(languageInitials);
			
			if (translations.ContainsKey(languageInitials))
			{
				Dictionary<string, string> languageTranslations = translations[languageInitials];
				if(languageTranslations.ContainsKey(id))
				{
					string localisedText = languageTranslations[id];
					if (languageInitials.ToLower() == "ar")
					{
						localisedText = FormatArabicString(localisedText);
					}
					
					localisedText = Regex.Replace(localisedText, @"[\u2018\u2019]", "'");
					return localisedText;
				}
				else
				{
					Debug.LogWarning("ContentManager: No content found for id: " + id);
					return "";
				}
			}
			else
			{
				Debug.LogWarning("ContentManager: No content found for language:" + languageInitials);
				return "";
			}
		}
		
		public string FormatArabicString(string arabicText)
		{
			string[] arabicTextLines = arabicText.Split('\n');
			string returnString = "";
			foreach (string line in arabicTextLines)
			{
				MatchCollection matches = Regex.Matches(line, @"\S+");
				string arabicString = "";
				string latinSubstring = "";
				foreach (Match match in matches)
				{
					if (!Regex.IsMatch(match.Value, RegexCharSet_ar.charSetRegex))
					{
						// If the word is Latin, prepend it to the latinSubstring.
						if (Regex.IsMatch(match.Value, @"[0-9]+-[0-9]+"))
						{
							string reversedAppendingString = "";
							foreach (Group matchGroup in Regex.Match(match.Value, @"([0-9]+)(-)([0-9]+)").Groups)
							{
								if (matchGroup.Value != match.Value)
								{
									reversedAppendingString = matchGroup.Value + reversedAppendingString;
								}
							}
							latinSubstring += (reversedAppendingString + " ");
						}
						else
						{
							latinSubstring += (match.Value + " ");
						}
						
						// Check to see if the current string contains the character to force append it to the arabic string.
						// Could cause layout problems if the appended latin string causes a newline.
						if(latinSubstring[latinSubstring.Length - 2] == '^')
						{
							string reversedString = "";
							foreach(Match wordMatch in Regex.Matches(latinSubstring.TrimEnd(new char[] { ' ', '^' }), @"\S+"))
							{
								reversedString = wordMatch.Value + " " + reversedString;
							}
							
							arabicString += reversedString.TrimEnd(new char[] { ' ' }) + " ";
							latinSubstring = "";
						}
						
						continue;
					}
					else
					{
						// If there is no current latinSubstring, just append to the arabic string.
						if (string.IsNullOrEmpty(latinSubstring))
						{
							MatchCollection subMatches = Regex.Matches(match.Value, @"\<.*?\>|\w+|\S+");
							foreach(Match subMatch in subMatches)
							{
								if(Regex.IsMatch(subMatch.Value, @"\<.*?\>"))
								{
									arabicString += subMatch.Value;
								}
								else
								{
									arabicString += (ArabicFixer.Fix(subMatch.Value));
								}
							}
							arabicString += " ";
						}
						else
						{
							// If there is an latinSubstring, append it before the next arabic word.
							if (Regex.Matches(latinSubstring.TrimEnd(new char[] { ' ' }), @"\w*(?!\s)\W\w+|\w+").Count > 1)
							{
								arabicString += "\n" + latinSubstring.TrimEnd(new char[] { ' ' }) + "\n";
							}
							else
							{
								arabicString += latinSubstring.TrimEnd(new char[] { ' ' }) + " ";
							}
							
							latinSubstring = "";
							
							MatchCollection subMatches = Regex.Matches(match.Value, @"\<.*?\>|\w+|\S+");
							foreach (Match subMatch in subMatches)
							{
								if (Regex.IsMatch(subMatch.Value, @"\<.*?\>"))
								{
									arabicString += subMatch.Value;
								}
								else
								{
									arabicString += (ArabicFixer.Fix(subMatch.Value));
								}
							}
							arabicString += " ";
						}
					}
				}
				if (string.IsNullOrEmpty(latinSubstring) == false)
				{
					// If there is an latinSubstring, append it before the next arabic word.
					if (Regex.Matches(latinSubstring.TrimEnd(new char[] { ' ' }), @"\w*(?!\s)\W\w+|\w+").Count > 1)
					{
						//arabicString += "\n" + latinSubstring.TrimEnd(new char[] { ' ' }) + "\n";
						arabicString += "\n" + latinSubstring.TrimEnd(new char[] { ' ' }) + "\n";
					}
					else
					{
						arabicString += latinSubstring.TrimEnd(new char[] { ' ' }) + " ";
					}
					
					latinSubstring = "";
				}
				// Remove any trailing spaces.
				arabicString = arabicString.TrimEnd(new char[] { ' ' });
				returnString += arabicString.Trim().TrimEnd(new char[] { '\n' }) + "\n";
			}
			
			return returnString.TrimEnd(new char[] { '\n' });
		}
		
		public string FixDatabaseArabicText(string arabicText)
		{
			string[] arabicTextLines = arabicText.Split('\n');
			
			arabicText = "";
			foreach (string line in arabicTextLines)
			{
				if (Regex.IsMatch(line, RegexCharSet_ar.charSetRegex))
				{
					string newLine = "";
					
					MatchCollection matches = Regex.Matches(line, @"\S+");
					foreach (Match match in matches)
					{
						newLine = match.Value + " " + newLine;
					}
					
					arabicText += newLine + "\n";
				}
				else
				{
					arabicText += line + "\n";
				}
			}
			
			return arabicText.TrimEnd(new char[] { '\n' });
		}
	}
}
#endif
