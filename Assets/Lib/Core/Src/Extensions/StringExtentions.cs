using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

public static class StringExtentions
{
    public static string ReplaceInstance(this string s, string replace, string substitute, int index = 0)
    {
        MatchCollection matches = Regex.Matches(s, Regex.Escape(replace));

        if(matches.Count > index)
        {
            return s.Substring(0, matches[index].Index) + substitute + s.Substring(matches[index].Index + replace.Length);
        }
        else
        {
            return s;
        }
    }
}
