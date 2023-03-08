using System;

public interface IContentManager
{
	string getString( string id, string languageInitials="" );
	string FormatArabicString(string arabicText);
}