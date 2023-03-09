using System.IO;
using System.Collections.Generic;
using UnityEngine;
using CustomTracks;

namespace CustomExporter
{
    public static class JsonImporter
    {
        public const string FILENAME = @"\Exported.txt";
        public const string FILENAME_LOOKAT = @"\LookAtTrackExported.txt";
        public const string FILENAME_TWEEN = @"\TweenTrackExported.txt";
        public const string FILENAME_DIALOG = @"\DialogTrackExported.txt";
        public const string FILENAME_DIALOGUE = @"\DialogueTrackExported.txt";

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

        public static ListLookAtClipData LoadLookAtTrack(string SAVE_FOLDER)
        {
            string path = SAVE_FOLDER + FILENAME_LOOKAT;

            if (!File.Exists(path))
            {
                Debug.Log("Save File Not Found on " + path);
                return null;
            }

            Debug.Log("Loading file from : " + path);

            string LoadedTxt = File.ReadAllText(path);

            ListLookAtClipData result = JsonUtility.FromJson<ListLookAtClipData>(LoadedTxt);
            return result;
        }

        public static ListTweenClipData LoadTweenTrack(string SAVE_FOLDER)
        {
            string path = SAVE_FOLDER + FILENAME_TWEEN;

            if (!File.Exists(path))
            {
                Debug.Log("Save File Not Found on " + path);
                return null;
            }

            Debug.Log("Loading file from : " + path);

            string LoadedTxt = File.ReadAllText(path);

            ListTweenClipData result = JsonUtility.FromJson<ListTweenClipData>(LoadedTxt);
            return result;
        }

        public static ListDialogClipData LoadDialogTrack(string SAVE_FOLDER)
        {
            string path = SAVE_FOLDER + FILENAME_DIALOG;

            if (!File.Exists(path))
            {
                Debug.Log("Save File Not Found on " + path);
                return null;
            }

            Debug.Log("Loading file from : " + path);

            string LoadedTxt = File.ReadAllText(path);

            ListDialogClipData result = JsonUtility.FromJson<ListDialogClipData>(LoadedTxt);
            return result;
        }

        public static ListDialogueClipData LoadDialogueTrack(string SAVE_FOLDER)
        {
            string path = SAVE_FOLDER + FILENAME_DIALOGUE;

            if (!File.Exists(path))
            {
                Debug.Log("Save File Not Found on " + path);
                return null;
            }

            Debug.Log("Loading file from : " + path);

            string LoadedTxt = File.ReadAllText(path);

            ListDialogueClipData result = JsonUtility.FromJson<ListDialogueClipData>(LoadedTxt);
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

    public class ListLookAtClipData
    {
        public string targetObject;
        public List<string> targetsToLook = new List<string>();
        public List<LookType> lookTypes = new List<LookType>();
        public List<double> startTimes = new List<double>();
        public List<double> durations = new List<double>();
    }

    public class ListTweenClipData
    {
        public string targetObject;
        public List<Vector3> startPositions = new List<Vector3>();
        public List<Vector3> startRotations = new List<Vector3>();
        public List<Vector3> endPositions = new List<Vector3>();
        public List<Vector3> endRotations = new List<Vector3>();
        public List<TweenBehaviour.TranslateType> translateTypes = new List<TweenBehaviour.TranslateType>();
        public List<double> startTimes = new List<double>();
        public List<double> durations = new List<double>();
    }

    public class ListDialogClipData
    {
        public string targetObject;
        public List<string> characterNames = new List<string>();
        public List<string> animationClipNames = new List<string>();
        public List<string> audioClipGuids = new List<string>();
        public List<string> subtitles = new List<string>();
        public List<bool> isTutorials = new List<bool>();
        public List<bool> isLearners = new List<bool>();
        public List<double> startTimes = new List<double>();
        public List<double> durations = new List<double>();
    }

    public class ListDialogueClipData
    {
        public List<DialogueEventData.DialogueData> eventDatas = new List<DialogueEventData.DialogueData>();
        public List<float> startTime = new List<float>();
        public List<float> duration = new List<float>();
        public List<string> characterNames = new List<string>();
    }
}
