using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace CustomExporter
{
    public static class JsonImporter
    {
        public const string FILENAME = @"\Exported.txt";

        public static ListAnimClipData LoadAnimationTrack(string SAVE_FOLDER)
        {
            string path = SAVE_FOLDER + FILENAME;

            if(!File.Exists(path))
            {
                Debug.Log("Save File Not Found on " + path);
                return null;
            }

            Debug.Log("Loading file from : " + path);

            string LoadedTxt = File.ReadAllText(path);

            ListAnimClipData result = JsonUtility.FromJson<ListAnimClipData>(LoadedTxt);
            return result;
        }
    }

    public class ListAnimClipData
    {
        public string targetObject;
        public List<string> stateNames = new List<string>();
        public List<int> layers = new List<int>();
        public List<double> startTimes = new List<double>();
        public List<double> durations = new List<double>();
    }
}
