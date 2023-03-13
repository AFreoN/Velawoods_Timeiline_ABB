#if UNITY_EDITOR

#define BUILD_IOS

using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CoreSystem;
using System.Collections;
using UnityEngine.UI;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System;
using UnityEditor.SceneManagement;

public class Bundlemanager : EditorWindow
{
    private static  Dictionary<string, string> KnownNames = new Dictionary<string, string>()
    {
        { "Learner_Apartment_Scene", "apartmentscene"},
        { "CourseTest_Scene", "coursetest"},
        { "ScenarioTest_Scene", "scenariotest"},
        { "LevelTest_Scene", "leveltest"},
        { "ScrapBook_Scene", "scrapbook"}
    };

	private class MetaData
	{
		public string AssetBudleName;
		public string AssetBudleVariant;
		public string FilePath;
		public string FileName;
		public bool IncludeInBundle;
	}

    private enum BuildStates
	{
		SwitchPlatform,
		DisableAllFiles,
        DisableAllFilesClose,
		Building,
        BuildingClose,
        Validate,
		None
	}

    private enum ValidationState
    {
        Open,
        Waiting,
        Validating,
        None
    }
	

#region properties
	private List<MetaData> MetaFiles = new List<MetaData>();
	private string SubDirectory = "\\Assets\\Missions\\Level1";
	private string Filter = "L[0-9]C[0-9]S[0-9]*M[0-9]";
	private bool IgnoreCase = true;
	private bool SearchAllDirectories = true;
	private bool SelectAll = false;
	private Vector2 scrollPos = new Vector2();
	private string[] allFilePaths = new string[] {};
	private BuildTarget buildTarget;

	private BuildStates currentState = BuildStates.None;
    string currentDisableString = "";
    string currentBuildString = "";

    private int currentDisableIndex = 0;
	private int currentBuildIndex = 0;
    private int actualBuildNumber = 0;
	System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

    private bool IsBuildingAll = false;

    private int currentValidationIndex = 0;
    private ValidationState currentValidationState = ValidationState.None;
    private float waitTime = 0.0f;

    StringBuilder errorString = new StringBuilder();

    private bool HUG = true;
    private bool Liam = true;
    private bool Allan = true;

    int errorCount = 0;
    int warningCount = 0;

#endregion

    [MenuItem("AssetBundles/Bundle Manager")]
    public static void ShowWindow()  
    { 
		EditorWindow window = EditorWindow.GetWindow(typeof(Bundlemanager)); 
		window.minSize = new Vector2(512.0f, 256.0f);
    }

    void OnGUI()
    {      
		switch(currentState)
		{
            case BuildStates.SwitchPlatform:
                {
                    GUI.enabled = false;
                    break;
                }
            case BuildStates.DisableAllFiles:
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Clearing Meta Files", currentDisableString, (float)(currentDisableIndex / (float)allFilePaths.Length)))
                    {
                        EditorUtility.ClearProgressBar();
                        currentState = BuildStates.None;
                    }

                    GUI.enabled = false;
                    break;
                }
            case BuildStates.DisableAllFilesClose:
                {
                    EditorUtility.ClearProgressBar();
                    currentState = BuildStates.Building;
                    GUI.enabled = false;
                    break;
                }
            case BuildStates.Building:
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Building Asset Bundle", string.IsNullOrEmpty(currentBuildString) ? "Building first asset bundle" : currentBuildString, (float)(actualBuildNumber / (float)MetaFiles.Where(item => item.IncludeInBundle).Count())))
                    {
                        EditorUtility.ClearProgressBar();
                        currentState = BuildStates.None;
                    }
                    GUI.enabled = false;
                    break;
                }
            case BuildStates.BuildingClose:
                {
                    EditorUtility.ClearProgressBar();
                    
                    if (IsBuildingAll)
                    {
                        switch (buildTarget)
                        {
                            case BuildTarget.StandaloneWindows:
                                {                                 
                                    StartBuild(BuildStates.SwitchPlatform, BuildTarget.Android);
                                    break;
                                }
                            case BuildTarget.Android:
                                {

#if BUILD_IOS
                                    StartBuild(BuildStates.SwitchPlatform, BuildTarget.iOS);
#else
                                    IsBuildingAll = false;
#endif
                                    break;
                                }
                            case BuildTarget.iOS:
                                {
                                    IsBuildingAll = false;
                                    break;
                                }
                        }
                    }
                    else
                    {
                        currentState = BuildStates.None;
                    }
                    
                    GUI.enabled = false;
                    break;
                }
            case BuildStates.None:
                {                 
                    break;
                }
		}

        DrawSelectionButtons();
        DrawFilterOptions();
        DrawSelectionOptions();
        DrawMetaFiles();
        DrawBuildButtons();
        GUI.enabled = true;

        GUILayout.BeginHorizontal();

        HUG = EditorGUILayout.ToggleLeft("Hug", HUG);
        Liam = EditorGUILayout.ToggleLeft("Liam", Liam);
        Allan = EditorGUILayout.ToggleLeft("Allan", Allan);

        GUILayout.EndHorizontal();
        if(GUILayout.Button("Validate"))
        {
            if (currentState == BuildStates.None)
            {
                currentState = BuildStates.Validate;
                currentValidationIndex = 0;
                currentValidationState = ValidationState.Open;
                errorString = new StringBuilder();
                errorCount = 0;
                warningCount = 0;
            }
            //JenkinsBuild("L1C1S1M1", "L1C1S1M2");
        }
    }

    bool[] Courses = new bool[] { false, false, false };
    bool[] PreviousToggles = new bool[] { true, true, true, true };
#region drawfunctions

    private void DrawSelectionButtons()
    {
        GUILayout.BeginHorizontal();
        bool[] previousValues = (bool[])Courses.Clone();

        Courses[0] = EditorGUILayout.ToggleLeft("Course1", Courses[0]);
        Courses[1] = EditorGUILayout.ToggleLeft("Course2", Courses[1]);
        Courses[2] = EditorGUILayout.ToggleLeft("Course3", Courses[2]);

        for (int courseIndex = 0; courseIndex < previousValues.Length; ++courseIndex)
        {
            if(previousValues[courseIndex] != Courses[courseIndex])
            {
                SelectFilesForCourse(courseIndex, Courses[courseIndex]);

                for (int toggleIndex = 0; toggleIndex < PreviousToggles.Length; ++toggleIndex)
                {
                    PreviousToggles[toggleIndex] = !(Courses[0] || Courses[1] || Courses[2]);
                }
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        for (int index = 0; index < 4;  ++index)
        {
            int ScenarioID = index+1;
            if (GUILayout.Button(("Scenario" + ScenarioID), GUILayout.MaxWidth(288)))
            {
                SelectFilesForScenarios(ScenarioID, PreviousToggles[index]);
                PreviousToggles[index] = !PreviousToggles[index];
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button(("All"), GUILayout.MaxWidth(288)))
        {
            for (int index = 0; index < 4; ++index)
            {
                int ScenarioID = index + 1;
                SelectFilesForScenarios(ScenarioID, true);
                PreviousToggles[index] = false;
            }
        }
        if (GUILayout.Button(("None"), GUILayout.MaxWidth(288)))
        {
            for (int index = 0; index < 4; ++index)
            {
                int ScenarioID = index + 1;
                SelectFilesForScenarios(ScenarioID, false);
                PreviousToggles[index] = true;
            }
        }

        GUILayout.EndHorizontal();
    }

    private string BuildLevelName(int scenario, int course, int mission)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("L").Append(1).Append("C").Append(1).Append("S").Append(scenario + (course * 3)).Append("M").Append(mission);
        return sb.ToString();
    }

    private void SelectFiles(HashSet<string> files, bool toBuild)
    {
        string toRemoveFromList = null;

        foreach (MetaData file in MetaFiles)
        {
            if (toRemoveFromList != null)
            {
                files.Remove(toRemoveFromList);
                toRemoveFromList = null;
            }

            foreach (string level in files)
            {
                if (file.FileName.Contains(level))
                {
                    MarkFileToBuild(file, toBuild);

                    toRemoveFromList = level;

                    break;
                }
            }
        }
    }

    private void SelectFilesForCourse(int courseIndex, bool toBuild)
    {
        HashSet<string> levelList = new HashSet<string>();

        // Loop through one more time if we are in course 3.
        int numberOfScenarios = PreviousToggles.Length - (courseIndex == 2 ? 0 : 1);

        for (int scenarioIndex = 0; scenarioIndex < numberOfScenarios; ++scenarioIndex)
        {
            levelList.Add(BuildLevelName(scenarioIndex + 1, courseIndex, 1));
            levelList.Add(BuildLevelName(scenarioIndex + 1, courseIndex, 2));
            levelList.Add(BuildLevelName(scenarioIndex + 1, courseIndex, 3));
            levelList.Add(BuildLevelName(scenarioIndex + 1, courseIndex, 4));
        }

        SelectFiles(levelList, toBuild);
    }

    private void SelectFilesForScenarios(int scenario, bool toBuild)
    {
        HashSet<string> levelList = new HashSet<string>();

        for (int index = scenario == 4 ? 2 :0; index < Courses.Length; ++index)
        {
            if (Courses[index])
            {
                levelList.Add(BuildLevelName(scenario, index, 1));
                levelList.Add(BuildLevelName(scenario, index, 2));
                levelList.Add(BuildLevelName(scenario, index, 3));
                levelList.Add(BuildLevelName(scenario, index, 4));
            }
            else
            {
                levelList.Remove(BuildLevelName(scenario, index, 1));
                levelList.Remove(BuildLevelName(scenario, index, 2));
                levelList.Remove(BuildLevelName(scenario, index, 3));
                levelList.Remove(BuildLevelName(scenario, index, 4));
            }
        }

        SelectFiles(levelList, toBuild);
    }

	private void DrawFilterOptions()
	{
		EditorGUILayout.HelpBox("The filter controls will default to finding all missions in level 1.", MessageType.Info);
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Assets");
		SubDirectory = EditorGUILayout.TextField(SubDirectory, GUILayout.MaxWidth(288));
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Filter");
		Filter = EditorGUILayout.TextField(Filter, GUILayout.MaxWidth(288));
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Ignore Case");
		IgnoreCase = EditorGUILayout.Toggle(IgnoreCase, GUILayout.MaxWidth(288));
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Search All Directories");
		SearchAllDirectories = EditorGUILayout.Toggle(SearchAllDirectories, GUILayout.MaxWidth(288));
		GUILayout.EndHorizontal();
	}
	
	private void DrawSelectionOptions()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("Meta Files");
		if (GUILayout.Button("Fetch", GUILayout.MaxWidth(288))) 
		{ 
			GetMetaFiles(); 
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Select All");
		
		bool oldValue = SelectAll;
		SelectAll = GUILayout.Toggle(SelectAll, "", GUILayout.MaxWidth(288));
		
		if(oldValue != SelectAll)
		{
			ToggleSelect(SelectAll);
		}
		GUILayout.EndHorizontal();
	}
	
	public void DrawMetaFiles()
	{
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, true);
		
		foreach(MetaData file in MetaFiles)
		{
			GUILayout.BeginHorizontal();
			
			GUILayout.Label(file.FileName);
            if (file.IncludeInBundle)
            {
                file.AssetBudleName = EditorGUILayout.TextField(file.AssetBudleName, GUILayout.MaxWidth(256));
            }
            else
            {
                bool wasEnabled = GUI.enabled;
                GUI.enabled = false;
                EditorGUILayout.TextField("", GUILayout.MaxWidth(256));
                GUI.enabled = wasEnabled;
            } 


			bool newValue = EditorGUILayout.Toggle(file.IncludeInBundle, GUILayout.MaxWidth(32));

			if(newValue != file.IncludeInBundle)
			{
				MarkFileToBuild(file, newValue);
			}
			
			GUILayout.EndHorizontal();
		}
		
		EditorGUILayout.EndScrollView();
	}

    private void StartBuild(BuildStates state, BuildTarget target)
    {
         if(MetaFiles.Count > 0)
         {
            buildTarget = target;
            currentState = state;
            currentDisableIndex = 0;
            currentBuildIndex = 0;
            actualBuildNumber = 0;
         }
    }
	
	private void DrawBuildButtons()
	{
		GUILayout.BeginHorizontal();
		
        if (GUILayout.Button("All", GUILayout.MaxWidth(288)))
        {
            IsBuildingAll = true;
            StartBuild(BuildStates.SwitchPlatform, BuildTarget.StandaloneWindows);
        }
		if (GUILayout.Button("Current", GUILayout.MaxWidth(288))) 
		{
            StartBuild(BuildStates.DisableAllFiles, EditorUserBuildSettings.activeBuildTarget);
        }
		if (GUILayout.Button("Windows", GUILayout.MaxWidth(288))) 
		{
            StartBuild(BuildStates.SwitchPlatform, BuildTarget.StandaloneWindows);
		}
		if (GUILayout.Button("Android", GUILayout.MaxWidth(288))) 
		{
            StartBuild(BuildStates.SwitchPlatform, BuildTarget.Android);
		}

        if (GUILayout.Button("OSX", GUILayout.MaxWidth(288)))
        {
            StartBuild(BuildStates.SwitchPlatform, BuildTarget.StandaloneOSX);
        }

#if !BUILD_IOS
        bool previousValue = GUI.enabled;

        GUI.enabled = false;
#endif
        if (GUILayout.Button("iOS", GUILayout.MaxWidth(288)))
        {
            StartBuild(BuildStates.SwitchPlatform, BuildTarget.iOS);
        }

#if !BUILD_IOS
        GUI.enabled = previousValue;
#endif

        GUILayout.EndHorizontal();
	}

	private void Update()
	{
       

       /* switch (currentState)
        {
            case BuildStates.SwitchPlatform:
                {
                    Debug.Log("Switching to platform to " + buildTarget.ToString());
                    EditorUserBuildSettings.SwitchActiveBuildTarget(buildTarget);
                    currentState = BuildStates.DisableAllFiles;
                    break;
                }
            case BuildStates.DisableAllFiles:
                {
                    if (currentDisableIndex < allFilePaths.Length)
                    {
                        currentDisableString = "Removing " + allFilePaths[currentDisableIndex] + " from current build";
                        DisableFile(allFilePaths[currentDisableIndex]);
                    }
                    else
                    {
                        currentState = BuildStates.DisableAllFilesClose;
                        AssetDatabase.Refresh();
                    }

                    ++currentDisableIndex;
                    break;
                }
            case BuildStates.Building:
                {
                    if (currentBuildIndex < MetaFiles.Count)
                    {
                        if (MetaFiles[currentBuildIndex].IncludeInBundle)
                        {
                            watch.Reset();
                            watch.Start();
                            EnableFile(MetaFiles[currentBuildIndex]);
                            RefreshFile(MetaFiles[currentBuildIndex].FilePath);
                            
                            BuildAssetBundlesForPlatform(buildTarget, MetaFiles[currentBuildIndex].AssetBudleName);
                            //Debug.Log("Building: " + MetaFiles[currentBuildIndex].AssetBudleName + " Platform: " + buildTarget.ToString());

                            DisableFile(MetaFiles[currentBuildIndex].FilePath);
                            RefreshFile(MetaFiles[currentBuildIndex].FilePath);
                            watch.Stop();
                            currentBuildString = "Asset Bundle:" + MetaFiles[currentBuildIndex].AssetBudleName + " built in " + watch.Elapsed.Minutes + " Minutes and " + watch.Elapsed.Seconds + " seconds";
                            ++actualBuildNumber;
                        }
                    }
                    else
                    {
                        currentState = BuildStates.BuildingClose;
                    }

                    ++currentBuildIndex;

                    break;
                }
            case BuildStates.Validate:
                {
                    if (currentValidationIndex < MetaFiles.Count)
                    {
                        if (MetaFiles[currentValidationIndex].IncludeInBundle)
                        {
                            switch (currentValidationState)
                            {
                                case ValidationState.Open:
                                    {
                                        // 6 is the length of Assets 
                                        string scenePath = @"\" + MetaFiles[currentValidationIndex].FilePath.Remove(0, Application.dataPath.Count() - 6);
                                        // Remove the .meta from the end.
                                        scenePath = scenePath.Remove(scenePath.Length - 5, 5);

                                        EditorApplication.OpenScene(scenePath);

                                        currentValidationState = ValidationState.Waiting;
                                        break;
                                    }
                                case ValidationState.Waiting:
                                    {
                                        if (waitTime > 5.0f)
                                        {
                                            currentValidationState = ValidationState.Validating;
                                        }
                                        else
                                        {
                                            waitTime += 0.016f;
                                        }

                                        break;
                                    }
                                case ValidationState.Validating:
                                    {
                                        //UtilityUtility.DialogueTest();
                                        *//*USSequencer sequencer = FindObjectOfType(typeof(WellFired.USSequencer)) as USSequencer;
                                        ValidationWindow.ValidateTimelines(sequencer);

                                        ++currentValidationIndex;
                                        currentValidationState = ValidationState.Open;

                                        string ErrorColor = "style= \" color: red; \"";
                                        string WarningColor = "style= \" color: orange; \"";
                                        
                                        string currentColor = "";

                                        errorString.Append("<li>" + sequencer.name);



                                        errorString.Append("<ul>");
                                        foreach (ValidationStatus error in sequencer.ValidationErrors)
                                        {
                                            if (error.Message != "PLACEHOLDER")
                                            {
                                                if (error.State == WellFired.ValidationState.WARNING)
                                                {
                                                    currentColor = WarningColor;
                                                    ++warningCount;
                                                }
                                                else
                                                {
                                                    currentColor = ErrorColor;
                                                    ++errorCount;
                                                }

                                                errorString.Append("<li><b " + currentColor + ">" + error.Message + "</b></li>");
                                            }
                                        }
                                        errorString.Append("</ul>");

                                        //if (sequencer.ValidationErrors.Count > 0)
                                        {
                                            errorString.Append("<ul>");
                                            foreach (USTimelineContainer container in sequencer.TimelineContainers)
                                            {
                                                if (container.ValidationErrors.Count > 0)
                                                {
                                                    errorString.Append("\n\t<li>" + container.name + "\n\t\t<ul>");

                                                    errorString.Append("\n\t\t\t<ul>");
                                                    foreach (ValidationStatus error in container.ValidationErrors)
                                                    {
                                                        if (error.Message != "PLACEHOLDER")
                                                        {
                                                            if (error.State == WellFired.ValidationState.WARNING)
                                                            {
                                                                currentColor = WarningColor;
                                                                ++warningCount;
                                                            }
                                                            else
                                                            {
                                                                currentColor = ErrorColor;
                                                                ++errorCount;
                                                            }

                                                            errorString.Append("\n\t\t\t\t<li><b " + currentColor + ">" + error.Message + "</b></li>");
                                                        }
                                                    }
                                                    errorString.Append("\n\t\t\t</ul>");

                                                    foreach (USTimelineBase timeline in container.Timelines)
                                                    {
                                                        if (timeline.ValidationErrors.Count > 0)
                                                        {
                                                            errorString.Append("\n\t\t\t<li>" + timeline.name + "\n\t\t\t\t<ul>");

                                                            errorString.Append("\n\t\t\t\t\t<ul>");
                                                            foreach (ValidationStatus error in timeline.ValidationErrors)
                                                            {
                                                                if (error.Message != "PLACEHOLDER")
                                                                {
                                                                    if (error.State == WellFired.ValidationState.WARNING)
                                                                    {
                                                                        currentColor = WarningColor;
                                                                        ++warningCount;
                                                                    }
                                                                    else
                                                                    {
                                                                        currentColor = ErrorColor;
                                                                        ++errorCount;
                                                                    }

                                                                    errorString.Append("\n\t\t\t\t\t\t<li><b " + currentColor + ">" + error.Message + "</b></li>");
                                                                }
                                                            }
                                                            errorString.Append("\n\t\t\t\t\t</ul>");

                                                            if (timeline is USTimelineAnimation)
                                                            {
                                                                USTimelineAnimation animationTimeline = timeline as USTimelineAnimation;

                                                                foreach (AnimationTrack track in animationTimeline.AnimationTracks)
                                                                {
                                                                    if (track.ValidationErrors.Count > 0)
                                                                    {
                                                                        errorString.Append("\n\t\t\t\t\t<li>" + "Animation Track" + "\n\t\t\t\t\t\t<ul>");
                                                                        errorString.Append("\n\t\t\t\t\t\t<ul>");

                                                                        foreach (ValidationStatus error in track.ValidationErrors)
                                                                        {
                                                                            if (error.Message != "PLACEHOLDER")
                                                                            {
                                                                                if (error.State == WellFired.ValidationState.WARNING)
                                                                                {
                                                                                    currentColor = WarningColor;
                                                                                    ++warningCount;
                                                                                }
                                                                                else
                                                                                {
                                                                                    currentColor = ErrorColor;
                                                                                    ++errorCount;
                                                                                }

                                                                                errorString.Append("\n\t\t\t\t\t\t\t<li><b " + currentColor + ">" + error.Message + "</b></li>");
                                                                            }
                                                                        }

                                                                        errorString.Append("\n\t\t\t\t\t\t</ul>");

                                                                        foreach (AnimationClipData clip in track.TrackClips)
                                                                        {
                                                                            if (clip.ValidationErrors.Count > 0)
                                                                            {
                                                                                errorString.Append("\n\t\t\t\t\t\t\t<ul>" + clip.name );

                                                                                foreach (ValidationStatus error in clip.ValidationErrors)
                                                                                {
                                                                                    if (error.Message != "PLACEHOLDER")
                                                                                    {
                                                                                        if (error.State == WellFired.ValidationState.WARNING)
                                                                                        {
                                                                                            currentColor = WarningColor;
                                                                                            ++warningCount;
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            currentColor = ErrorColor;
                                                                                            ++errorCount;
                                                                                        }

                                                                                        errorString.Append("\n\t\t\t\t\t\t\t\t<li><b " + currentColor + ">" + error.Message + "</b></li>");
                                                                                    }    
                                                                                }
                                                                                errorString.Append("\n\t\t\t\t\t\t\t</ul>");
                                                                            }
                                                                        }
                                                                        errorString.Append("\n\t\t\t\t\t\t</ul> \n\t\t\t\t\t</li>");
                                                                    }
                                                                }
                                                            }
                                                            else if (timeline is USTimelineEvent)
                                                            {
                                                                USTimelineEvent eventTimeline = timeline as USTimelineEvent;

                                                                foreach (USEventBase eventBase in eventTimeline.Events)
                                                                {
                                                                    if (eventBase.ValidationErrors.Count > 0)
                                                                    {
                                                                        errorString.Append("\n\t\t\t\t\t<li>" + eventBase.name + "\n\t\t\t\t\t\t<ul>");

                                                                        foreach (ValidationStatus error in eventBase.ValidationErrors)
                                                                        {
                                                                            if (error.Message != "PLACEHOLDER")
                                                                            {
                                                                                if (error.State == WellFired.ValidationState.WARNING)
                                                                                {
                                                                                    currentColor = WarningColor;
                                                                                    ++warningCount;
                                                                                }
                                                                                else
                                                                                {
                                                                                    currentColor = ErrorColor;
                                                                                    ++errorCount;
                                                                                }

                                                                                errorString.Append("\n\t\t\t\t\t\t\t\t<li><b " + currentColor + ">" + error.Message + "</b></li>");
                                                                            }
                                                                        }
                                                                        errorString.Append("\n\t\t\t\t\t\t</ul>\n\t\t\t\t\t</li>");
                                                                    }
                                                                }
                                                            }
                                                            errorString.Append("\n\t\t\t\t</ul>\n\t\t\t</li>");
                                                        }
                                                    }
                                                    errorString.Append("\n\t\t</ul>\n\t</li>");
                                                }
                                            }
                                            errorString.Append("\n</ul>");
                                        }
                                        
                                        errorString.Append("</li>");
                                        *//*
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            ++currentValidationIndex;
                        }
                    }
                    else
                    {
                        currentValidationState = ValidationState.None;
                        currentState = BuildStates.None;

                        MailMessage mail = new MailMessage();
                        mail.IsBodyHtml = true;
                        mail.To.Add("s.gore@route1games.com");
                        mail.To.Add("Route1Validation@gmail.com");

                        if(HUG)
                        {
                            mail.To.Add("J.Huggins@route1games.com");
                        }

                        if(Allan)
                        {
                            mail.To.Add("a.speed@route1games.com");
                        }

                        if(Liam)
                        {
                            mail.To.Add("l.bower@route1games.com");
                        }

                        mail.Subject = "Validation for " + DateTime.Now;
                        Debug.Log("Error Count: " + errorCount + " Warning Count: " + warningCount);

                        mail.Body = string.Format("<p>Validation Version: {0}</p><p>Error Count: <span style=\"color: red;\">{1}</span></p> <p> WarningCount: <span style=\"color: orange;\">{2}</span></p>", ValidationWindow.CURRENT_VALIDATION_VERSION, errorCount, warningCount) + errorString.ToString();

                        SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
                        smtpServer.Credentials = new System.Net.NetworkCredential("Route1Validation@gmail.com", "COTED'AZURTOUTELANNEE") as ICredentialsByHost;
                        smtpServer.Port = 587;
                        smtpServer.EnableSsl = true;
                        ServicePointManager.ServerCertificateValidationCallback =
                            delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                            { return true; };
                        smtpServer.Send(mail);

                        Debug.Log("Validation Finished");
                    }
                        
                    break;
                }
        }*/

        Repaint();
	}
	#endregion
	
#region ButtonCallbacks
	private void GetMetaFiles()
	{
		// Clear current files.
		MetaFiles.Clear();
		
		// Get all scene meta files.
		allFilePaths = Directory.GetFiles(Application.dataPath, "*.unity.meta", SearchOption.AllDirectories);
		
		// Find all meta files in the chosen directory
		string[] filteredFilePaths = Directory.GetFiles(Application.dataPath + SubDirectory, "*.unity.meta", SearchAllDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

		foreach(string file in allFilePaths)
		{
			if(filteredFilePaths.Contains(file))
			{
				string[] splitFilePath = file.Split(new char[]{'\\'});
				
				if(!(Filter.Length > 0) || System.Text.RegularExpressions.Regex.IsMatch(splitFilePath.Last(), Filter, IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None))
				{
					MetaData data = GetMetaFile(file,  splitFilePath.Last().Split(new char[] {'.'}).First());

                    if(data != null)
                    {
                        Debug.Log(file);
                        MetaFiles.Add(data);
                    }
				}
			}
		}
	}
	
	private void ToggleSelect (bool Select)
	{
		foreach(MetaData file in MetaFiles)
		{
			MarkFileToBuild(file, Select);
		}
	}
#endregion

#region MetaFileList
	private static MetaData GetMetaFile(string path, string filename)
	{
		string[] lines = File.ReadAllLines(path);
		MetaData filedata = new MetaData();
		char[] SplitCharacters = new char[]{':'};
		
		bool assetBundleNameLineFound = false;
		bool assetBundleVarientLineFound = false;
		
		for(int index = 0; index < lines.Length; ++index)
		{
			if(lines[index].Contains("assetBundleName:"))
			{
				assetBundleNameLineFound = true;
				filedata.AssetBudleName = lines[index].Split(SplitCharacters)[1];
			}
			else if(lines[index].Contains("assetBundleVariant"))
			{
				assetBundleVarientLineFound = true;
				filedata.AssetBudleVariant = lines[index].Split(SplitCharacters)[1];
			}
		}
		
		if(assetBundleNameLineFound && assetBundleVarientLineFound)
		{
			filedata.FilePath = path;
			filedata.FileName = filename;
			filedata.IncludeInBundle = filedata.AssetBudleName.Replace(" ", "").Replace("\t", "").Length > 0;

            return filedata;
		}
		else
		{
			Debug.LogWarning("Failed to find assetBundleName or assetBundleVariant tag in meta file: " + path);
		}

        return null;
	}


	private static bool MarkFileToBuild(MetaData file, bool toBuild)
	{
		string[] lines = File.ReadAllLines(file.FilePath);
		
		bool assetBundleNameLineFound = false;
		bool assetBundleVarientLineFound = false;
		
		for(int index = 0; index < lines.Length; ++index)
		{
			if(lines[index].Contains("assetBundleName:"))
			{
				assetBundleNameLineFound = true;
			}
			else if(lines[index].Contains("assetBundleVariant"))
			{
				assetBundleVarientLineFound = true;
			}
		}
		
		if(assetBundleNameLineFound && assetBundleVarientLineFound)
		{
			file.IncludeInBundle = toBuild;

            if(toBuild)
            {
                if (string.IsNullOrEmpty(file.AssetBudleName) || file.AssetBudleName == " ")
                {
                    if (file.FileName.Any(char.IsDigit))
                    {
                        // This could be a mission so parse it as a mission.
                        file.AssetBudleName = file.FileName.ToLower().Split(new char[] { '_' })[0];
                    }
                    else
                    { 
                        // Check if this is a know name and use that otherwise default to the file name.
                        if(KnownNames.Keys.Contains<string>(file.FileName))
                        {
                            file.AssetBudleName = KnownNames[file.FileName];
                        }
                        else
                        {
                             file.AssetBudleName =  file.FileName;
                        }
                    }
                }
            }

			return true;
		}
		else
		{
			Debug.LogError("Failed to find assetBundleName or assetBundleVariant tag in meta file: " + file.FileName);
			
			return false;
		}
	}
#endregion

#region MetaFile
    private static void RefreshFile(string file)
    {
        Debug.Log("Assets" + file.Replace(Application.dataPath, "").Replace(".meta", ""));
        AssetDatabase.ImportAsset("Assets" + file.Replace(Application.dataPath, "").Replace(".meta", ""), ImportAssetOptions.ForceUpdate);
    }

	private static bool EnableFile(MetaData file)
	{
		string[] lines = File.ReadAllLines(file.FilePath);
		
		bool assetBundleNameLineFound = false;
		bool assetBundleVarientLineFound = false;
		
		for(int index = 0; index < lines.Length; ++index)
		{
			if(lines[index].Contains("assetBundleName:"))
			{
				assetBundleNameLineFound = true;
                lines[index] = "  assetBundleName: " + file.AssetBudleName;
			}
			else if(lines[index].Contains("assetBundleVariant"))
			{
				assetBundleVarientLineFound = true;
				lines[index] = "  assetBundleVariant: asset";
			}
		}
		
		if(assetBundleNameLineFound && assetBundleVarientLineFound)
		{
			File.WriteAllLines(file.FilePath, lines);
			//AssetDatabase.ImportAsset("Assets" + file.FilePath.Replace(Application.dataPath, "").Replace(".meta", ""), ImportAssetOptions.ForceUpdate);
			return true;
		}
		else
		{
			//Debug.LogError("Failed to find assetBundleName or assetBundleVariant tag in meta file: " + file.FileName);

			return false;
		}
	}
	
	private static bool DisableFile(string path)
	{
		string[] lines = File.ReadAllLines(path);
		
		bool assetBundleNameLineFound = false;
		bool assetBundleVarientLineFound = false;
		
		for(int index = 0; index < lines.Length; ++index)
		{
			if(lines[index].Contains("assetBundleName:"))
			{
				assetBundleNameLineFound = true;
				lines[index] = "  assetBundleName:";
			}
			else if(lines[index].Contains("assetBundleVariant"))
			{
				assetBundleVarientLineFound = true;
				lines[index] = "  assetBundleVariant:";
			}
		}
		
		if(assetBundleNameLineFound && assetBundleVarientLineFound)
		{
			File.WriteAllLines(path, lines);
			//AssetDatabase.ImportAsset("Assets" + path.Replace(Application.dataPath, "").Replace(".meta", ""), ImportAssetOptions.ForceUpdate);
			return true;
		}
		else
		{
			//Debug.LogError("Failed to find assetBundleName or assetBundleVariant tag in meta file: " + path);
			
			return false;
		}
	}
	#endregion

	public static void BuildAssetBundlesForPlatform(BuildTarget target, string path)
	{
        if(string.IsNullOrEmpty(path))
        {
            Debug.LogError("Invalid assetBundleName");

            return;
        }

        path = path.TrimStart(new char[] { ' ' });

        // Assume that any string with a digit is a mission. 
        if (path.Any(char.IsDigit))
        {
            float[] levelIDS = ActivityTracker.ConvertIDIntoIndividualIDs(path.ToUpper());

            path = Path.Combine(Path.Combine(Path.Combine("L" + levelIDS[0], "C" + levelIDS[1]), "S" + levelIDS[2]), "M" + levelIDS[3]);
        }

        // Choose the output path according to the build target.
        string outputPath = Path.Combine(Path.Combine("AssetBundles", BaseLoader.GetPlatformFolderForAssetBundles(target)), path);

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
		Debug.Log (outputPath);
        BuildPipeline.BuildAssetBundles(outputPath, 0, target);
	}

    [MenuItem("Jenkins/AssetBundles/TEST_JENKINS")]
	public static void JenkinsBuildWindowsM1()
	{
		List<BuildTarget> listOfPlatforms = new List<BuildTarget>();
		listOfPlatforms.Add(BuildTarget.StandaloneWindows);
		
		JenkinsBuild("L1C1S1M1", "L1C1S1M2", listOfPlatforms, false);
	}

    [MenuItem("Jenkins/AssetBundles/BuildScrapBook Windows")]
    public static void JenkinsBuildScrapBookWindows()
    {
        List<BuildTarget> listOfPlatforms = new List<BuildTarget>();
        listOfPlatforms.Add(BuildTarget.StandaloneWindows);

        JenkinsBuild(null, null, listOfPlatforms, false, false, false, true);
    }

    [MenuItem("Jenkins/AssetBundles/BuildScrapBook Android")]
    public static void JenkinsBuildScrapBookAndroid()
    {
        List<BuildTarget> listOfPlatforms = new List<BuildTarget>();
        listOfPlatforms.Add(BuildTarget.Android);

        JenkinsBuild(null, null, listOfPlatforms, false, false, false, true);
    }

    [MenuItem("Jenkins/AssetBundles/BuildScrapBook OSX")]
    public static void JenkinsBuildScrapBookOSX()
    {
        List<BuildTarget> listOfPlatforms = new List<BuildTarget>();
		listOfPlatforms.Add(BuildTarget.StandaloneOSX);

        JenkinsBuild(null, null, listOfPlatforms, false, false, false, true);
    }

    [MenuItem("Jenkins/AssetBundles/BuildScrapBook IOS")]
    public static void JenkinsBuildScrapBookIOS()
    {
        List<BuildTarget> listOfPlatforms = new List<BuildTarget>();
        listOfPlatforms.Add(BuildTarget.iOS);

        JenkinsBuild(null, null, listOfPlatforms, false, false, false, true);
    }

    [MenuItem("Jenkins/AssetBundles/BuildApartment Windows")]
    public static void JenkinsBuildApartmentWindows()
    {
        List<BuildTarget> listOfPlatforms = new List<BuildTarget>();
        listOfPlatforms.Add(BuildTarget.StandaloneWindows);

        JenkinsBuild(null, null, listOfPlatforms, true, false, false, false);
    }

    [MenuItem("Jenkins/AssetBundles/BuildApartment Android")]
    public static void JenkinsBuildApartmentAndroid()
    {
        List<BuildTarget> listOfPlatforms = new List<BuildTarget>();
        listOfPlatforms.Add(BuildTarget.Android);

        JenkinsBuild(null, null, listOfPlatforms, true, false, false, false);
    }

    [MenuItem("Jenkins/AssetBundles/BuildApartment OSX")]
    public static void JenkinsBuildApartmentOSX()
    {
        List<BuildTarget> listOfPlatforms = new List<BuildTarget>();
		listOfPlatforms.Add(BuildTarget.StandaloneOSX);

        JenkinsBuild(null, null, listOfPlatforms, true, false, false, false);
    }

    [MenuItem("Jenkins/AssetBundles/BuildApartment IOS")]
    public static void JenkinsBuildApartmentIOS()
    {
        List<BuildTarget> listOfPlatforms = new List<BuildTarget>();
        listOfPlatforms.Add(BuildTarget.iOS);

        JenkinsBuild(null, null, listOfPlatforms, true, false, false, false);
    }


    public static string StartingString = "L1C1S1M1";
    public static string EndingString = "L1C1S1M1";
	public static string StartingStringA2 = "L3C1S1M2";
	public static string EndingStringA2 = "L3C1S1M2";
    private static bool IncludeAppartmentInBuild = false;
    private static bool IncludeScrapbookInBuild = false;
    private static bool OptimiseAnimations = true;
	private static bool OptimiseFaceFX = true;


	/**
	 * A1
	 */

	[MenuItem("Jenkins/AssetBundles/A1/Android")]
	public static void JenkinsBuildAndroid()
	{
        List<BuildTarget> listOfPlatforms = new List<BuildTarget>();
        listOfPlatforms.Add(BuildTarget.Android);

        JenkinsBuild(StartingString, EndingString, listOfPlatforms, IncludeAppartmentInBuild, OptimiseAnimations, OptimiseFaceFX, IncludeScrapbookInBuild);
	}

    [MenuItem("Jenkins/AssetBundles/A1/Windows")]
    public static void JenkinsBuildWindows()
    {
        List<BuildTarget> listOfPlatforms = new List<BuildTarget>();
        listOfPlatforms.Add(BuildTarget.StandaloneWindows);

        JenkinsBuild(StartingString, EndingString, listOfPlatforms, IncludeAppartmentInBuild, OptimiseAnimations, OptimiseFaceFX, IncludeScrapbookInBuild);
    }

    [MenuItem("Jenkins/AssetBundles/A1/OSX64")]
    public static void JenkinsBuildOSX()
    {
        List<BuildTarget> listOfPlatforms = new List<BuildTarget>();
		listOfPlatforms.Add(BuildTarget.StandaloneOSX);

        JenkinsBuild(StartingString, EndingString, listOfPlatforms, IncludeAppartmentInBuild, OptimiseAnimations, OptimiseFaceFX, IncludeScrapbookInBuild);
    }

    [MenuItem("Jenkins/AssetBundles/A1/iOS")]
    public static void JenkinsBuildIOS()
    {
        List<BuildTarget> listOfPlatforms = new List<BuildTarget>();
        listOfPlatforms.Add(BuildTarget.iOS);

        JenkinsBuild(StartingString, EndingString, listOfPlatforms, IncludeAppartmentInBuild, OptimiseAnimations, OptimiseFaceFX, IncludeScrapbookInBuild);
    }

	/**
	 * A2
	 */

	[MenuItem("Jenkins/AssetBundles/A2/Windows")]
	public static void JenkinsBuildWindowsA2()
	{
		List<BuildTarget> listOfPlatforms = new List<BuildTarget>();
		listOfPlatforms.Add(BuildTarget.StandaloneWindows);

        JenkinsBuild(StartingStringA2, EndingStringA2, listOfPlatforms, false, OptimiseAnimations, OptimiseFaceFX);
	}

	[MenuItem("Jenkins/AssetBundles/A2/Android")]
	public static void JenkinsBuildAndroidA2()
	{
		List<BuildTarget> listOfPlatforms = new List<BuildTarget>();
		listOfPlatforms.Add(BuildTarget.Android);

        JenkinsBuild(StartingStringA2, EndingStringA2, listOfPlatforms, false, OptimiseAnimations, OptimiseFaceFX);
	}

	[MenuItem("Jenkins/AssetBundles/A2/OSX64")]
	public static void JenkinsBuildOSXA2()
	{
		List<BuildTarget> listOfPlatforms = new List<BuildTarget>();
		listOfPlatforms.Add(BuildTarget.StandaloneOSX);

        JenkinsBuild(StartingStringA2, EndingStringA2, listOfPlatforms, false, OptimiseAnimations, OptimiseFaceFX);
	}

	[MenuItem("Jenkins/AssetBundles/A2/iOS")]
	public static void JenkinsBuildIOSA2()
	{
		List<BuildTarget> listOfPlatforms = new List<BuildTarget>();
		listOfPlatforms.Add(BuildTarget.iOS);

        JenkinsBuild(StartingStringA2, EndingStringA2, listOfPlatforms, false, OptimiseAnimations, OptimiseFaceFX);
	}

	/**
	 * 
	 * 
	 */


    public static void JenkinsBuild(string startingMission, string endingMission, List<BuildTarget> listOfPlatforms, bool IncludeAppartmentScene = false, bool OptimiseAnimations = false, bool OptimiseFaceFX = false, bool includeScrapbookInBuild = false)
    {
        string[] filepaths = Directory.GetFiles(Application.dataPath, "*Scene.unity.meta", SearchOption.AllDirectories);
        List<MetaData> files = new List<MetaData>();

        // Get a list of all of the scenes in the project. 
        foreach (string file in filepaths)
        {
            string[] splitFilePath = file.Split(new char[] { '\\' });

            MetaData data = GetMetaFile(file, splitFilePath.Last().Split(new char[] { '.' }).First());

            if (data != null)
            {
                files.Add(data);
            }
        }

        /*if(listOfPlatforms.Count == 1)
        {
            // Assume that we are doing a command line build and grab the database. 
            EditorUserBuildSettings.SwitchActiveBuildTarget(listOfPlatforms[0]);
            GetDatabaseAndEncrypt.UpdateAndEncrypt();
        }*/

        // Select the scenes to build.
        List<string> includedDatabaseIDs = new List<string>();

        if (null != startingMission && null != endingMission)
        {
            // Get the data base IDs for the starting and ending mission.
            float[] starting = ActivityTracker.ConvertIDIntoIndividualIDs(startingMission.ToUpper());
            float[] ending = ActivityTracker.ConvertIDIntoIndividualIDs(endingMission.ToUpper());

            string startingDatabaseMissionID = Database.Instance.GetID(starting.Take(4).ToArray());
            string endingDatabaseMissionID = Database.Instance.GetID(ending.Take(4).ToArray());

            includedDatabaseIDs.Add(startingDatabaseMissionID);

            if (startingDatabaseMissionID != endingDatabaseMissionID)
            {
                includedDatabaseIDs.Add(endingDatabaseMissionID);

                string nextDatabaseMissionID = DatabaseHelperFunctions.GetNextMissionDBID(startingDatabaseMissionID);

                if (nextDatabaseMissionID == "-1")
                {
                    Debug.Log("Failed to find next database ID for mission.");
                    return;
                }

                // Make sure that we are not building two missions.
                if (nextDatabaseMissionID != endingDatabaseMissionID)
                {
                    do
                    {
                        includedDatabaseIDs.Add(nextDatabaseMissionID);
                        nextDatabaseMissionID = DatabaseHelperFunctions.GetNextMissionDBID(nextDatabaseMissionID);

                        if (nextDatabaseMissionID == "-1")
                        {
                            Debug.Log("Failed to find next database ID for mission.");
                            return;
                        }
                    } while (nextDatabaseMissionID != endingDatabaseMissionID);
                }
            }
        }

        foreach (MetaData file in files)
        {
            file.IncludeInBundle = false;
        }

        // Set the meta files that we need to mark to build.
        foreach(string id in includedDatabaseIDs)
        {
            float[] missionIDs = DatabaseHelperFunctions.GetVisualIDsForMission(id);
            missionIDs = ActivityTracker.ConvertActualIDsToMissionNameID(missionIDs);
            string missionIDString = "L" + missionIDs[0] + "C" + missionIDs[1] + "S" + missionIDs[2] + "M" + missionIDs[3];

            foreach (MetaData file in files)
            {
                if(file.FileName.Contains(missionIDString))
                {

//					Debug.Log ("Including " + missionIDString + " from " + file.FileName + " at " + file.FilePath );

                    file.IncludeInBundle = true;
                    break;
                }
            }
        }

        if (IncludeAppartmentScene)
        {
            foreach (MetaData file in files)
            {
                if (file.FileName.Contains("Learner_Apartment_Scene"))
                {
                  file.IncludeInBundle = true;
                    break;
                }
            }
        }

        if (includeScrapbookInBuild)
        {
            foreach (MetaData file in files)
            {
                if (file.FileName.Contains("ScrapBook"))
                {
                    file.IncludeInBundle = true;
                    break;
                }
            }
        }

        if (OptimiseAnimations || OptimiseFaceFX)
        {
            foreach (MetaData file in files)
            {
                if (file.IncludeInBundle)
                {
//					@"\" +
//                    // 6 is the length of Assets 
                    string scenePath =  file.FilePath.Remove(0, Application.dataPath.Count() - 6);
				
                    // Remove the .meta from the end.
					scenePath = scenePath.Remove(scenePath.Length - 5, 5);
				
					Debug.Log (scenePath);
					EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Single);

                    if(OptimiseAnimations) RouteGames.AnimationUtility.OptimiseAnimationController(scenePath);
                    if(OptimiseFaceFX) UtilityUtility.OptimiseFaceFX();

                    /*USSequencer sequencer = FindObjectOfType<USSequencer>();

                    if(sequencer)
                    {
                        sequencer.BuildTime = DateTime.Now.ToBinary();
                    }
                    else
                    {
                        Debug.Log("Failed to find sequence in scene file.");
                    }*/

                    EditorApplication.SaveScene(scenePath);
                    //EditorSceneManager.SaveOpenScenes();
                }
            }
        }
        
        // Build all of the selected scenes
       foreach(BuildTarget target in listOfPlatforms)
        {
            // Switch to the  target platform. 
            EditorUserBuildSettings.SwitchActiveBuildTarget(target);

            // Make sure all files are set to not build.
            foreach(string path in filepaths)
            {
                DisableFile(path);
            }
            
            // Refresh all assets in the database.
            AssetDatabase.Refresh();

            // Build each file individually. 
            foreach(MetaData file in files)
            {
                if (file.IncludeInBundle)
                {
                    MarkFileToBuild(file, true);
                    EnableFile(file);
                    RefreshFile(file.FilePath);

                    BuildAssetBundlesForPlatform(target, file.AssetBudleName);

                    DisableFile(file.FilePath);
                    RefreshFile(file.FilePath);
                }
            }
        }
    }
}
#endif
