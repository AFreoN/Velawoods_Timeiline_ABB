using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;


public class ProjectUtility : MonoBehaviour
{
    const string ProjectMenuItemString = "VELA/Project/";
    const string PrefabMenuItemString = ProjectMenuItemString + "Prefabs/";
    const string AWSBuildConstantMenuItemString = ProjectMenuItemString + "Build Constants/AWS/";
    const string BundledBuildConstantMenuItemString = ProjectMenuItemString + "Build Constants/Bundled/";

    const string AWSBuildString = "CLIENT_BUILD; AWS_BUILD; FORCE_NO_COMPRESSION_BUNDLES;";
    const string BundledBuildString = "CLIENT_BUILD; BUNDLED_BUILD; FORCE_NO_COMPRESSION_BUNDLES;";
 
    #region Prefabs
    #region Helper Functions
    private static int UpdatePrefabComponent<T>(Func<T, bool> method)
    {
        int changeCount = 0;
        string[] prefabFiles = Directory.GetFiles(Application.dataPath + "/", "*.prefab", SearchOption.AllDirectories);

        foreach (string prefabFile in prefabFiles)
        {
            string assetPath = prefabFile.Replace(Application.dataPath + "/", "Assets\\");
            UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));

            if (prefab)
            {
                GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

                foreach (T component in prefabInstance.GetComponentsInChildren<T>(true))
                {
                    changeCount += method(component) ? 1 : 0;
                    PrefabUtility.ReplacePrefab(prefabInstance, prefab);
                }
                DestroyImmediate(prefabInstance);
            }
            else
            {
                Debug.LogErrorFormat("ProjectUtility::ApplyChangesToPrefab: Failed to load asset at path: {0}", assetPath);
            }
        }

        return changeCount;
    }
    #endregion

    #region MenuItems
    [MenuItem(PrefabMenuItemString + "Update ScrollRect Sensitivities", false, 3001)]
    private static void UpdateScrollRectSensitivity()
    {
        Debug.LogFormat("Updated the ScrollRect in {0} prefabs.", UpdatePrefabComponent<ScrollRect>((scrollRect) => 
        {
            // Check to make sure the sensitivity is the default value before we change. If it is not default,
            // it is assumed the sensitivity is different by design.
            if (scrollRect.scrollSensitivity == 1.0f)
            {
                scrollRect.scrollSensitivity = 50.0f;
                return true;
            }
            return false;
        }));
    }

    [MenuItem(PrefabMenuItemString + "Set Button Navigation Mode to None", false, 3002)]
    static void UpdateButtonsNavigationMode()
    {
        Debug.LogFormat("Updated the Navigation mode of {0} prefabs.", UpdatePrefabComponent<Button>((button) =>
        {
            Navigation newNavigation = button.navigation;
            newNavigation.mode = Navigation.Mode.None;
            button.navigation = newNavigation;

            return true;
        }));
    }
    #endregion
    #endregion

    #region Prefabs
    #region Helper Functions
    private static void SetConstants(string constants)
    {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, constants);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, constants);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, constants + "; BUNDLE_HTTPS");
    }
    #endregion

    #region MenuItems
    [MenuItem(AWSBuildConstantMenuItemString + "Debug", false, 1001)]
    public static void SetConstantsDebug()
    {
        SetConstants(string.Format("{0} {1}", AWSBuildString, "DEBUG_BUILD;"));

        Debug.Log("Build constants set to Debug.");
    }

    [MenuItem(AWSBuildConstantMenuItemString + "Debug Release", false, 1002)]
    public static void SetConstantsDebugRelease()
    {
        SetConstants(string.Format("{0} {1}", AWSBuildString, "DEBUG_BUILD; RELEASE_BUILD;"));

        Debug.Log("Build constants set to Debug Release.");
    }

    [MenuItem(AWSBuildConstantMenuItemString + "Release", false, 1003)]
    public static void SetConstantsRelease()
    {
        SetConstants(string.Format("{0} {1}", AWSBuildString, "RELEASE_BUILD; USE_DEBUG_WRAPPER;"));

        Debug.Log("Build constants set to Release.");
    }

    [MenuItem(BundledBuildConstantMenuItemString + "Debug", false, 2001)]
    public static void SetConstantsBundledDebug()
    {
        SetConstants(string.Format("{0} {1}", BundledBuildString, "DEBUG_BUILD;"));

        Debug.Log("Build constants set to Bundled Debug.");
    }

    [MenuItem(BundledBuildConstantMenuItemString + "Debug Release", false, 2002)]
    public static void SetConstantsBundledDebugRelease()
    {
        SetConstants(string.Format("{0} {1}", BundledBuildString, "DEBUG_BUILD; RELEASE_BUILD;"));

        Debug.Log("Build constants set to Bundled Debug Release.");
    }

    [MenuItem(BundledBuildConstantMenuItemString + "Release", false, 2003)]
    public static void SetConstantsBundledRelease()
    {
        SetConstants(string.Format("{0} {1}", BundledBuildString, "RELEASE_BUILD; USE_DEBUG_WRAPPER;"));

        Debug.Log("Build constants set to Bundled Release.");
    }
    #endregion
    #endregion

#if CLIENT_BUILD
    [MenuItem("VELA/Project/List All Missing Minigame Audio.")]
    static void GetAListOfAllMissingMinigameAudio()
    {
        string selectAllReferencedAudioStatement = @"
SELECT  
	Audiofile.id, 
	Audiofile.filename 
FROM Audiofile 
WHERE Audiofile.id IN (
	SELECT Widget.audiofileid
	FROM Widget
	WHERE 
		Widget.audiofileid IS NOT '' AND
		Widget.audiofileid IS NOT NULL AND
		Widget.audiofileid IS NOT 0
	UNION
	SELECT WidgetElement.audiofileid
	FROM WidgetElement
	WHERE 
		WidgetElement.audiofileid IS NOT '' AND
		WidgetElement.audiofileid IS NOT NULL AND
		WidgetElement.audiofileid IS NOT 0
	UNION
	SELECT WidgetElement.audiofileid2
	FROM WidgetElement
	WHERE 
		WidgetElement.audiofileid2 IS NOT '' AND
		WidgetElement.audiofileid2 IS NOT NULL AND
		WidgetElement.audiofileid2 IS NOT 0
	UNION
	SELECT WidgetElement.audiofileid3
	FROM WidgetElement
	WHERE 
		WidgetElement.audiofileid3 IS NOT '' AND
		WidgetElement.audiofileid3 IS NOT NULL AND
		WidgetElement.audiofileid3 IS NOT 0);";

        List<Dictionary<string, string>> allReferencedAudioFiles = Database.Instance.Query(selectAllReferencedAudioStatement);
        List<string> missingAudio = new List<string>();

        foreach (Dictionary<string, string> audioFile in allReferencedAudioFiles)
        {
            string filename = audioFile["filename"];

            AudioClip clip = Resources.Load<AudioClip>(DialogueAudioHelper.GetDialogueAudioPath(filename, false));

            if (clip == null)
            {
                missingAudio.Add(filename);
            }
        }

        EditorGUIUtility.systemCopyBuffer = String.Join("\n", missingAudio.ToArray());

        Debug.LogFormat("Copied a list of files to the clipboard. The list contains {0} files that are missing after testing {1} total files", missingAudio.Count, allReferencedAudioFiles.Count);
    }
#endif

    [MenuItem("VELA/Project/GetAllResourceFiles.")]
    static void GetListOfResourceFiles()
    {
        List<string> allResources = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories).Where(item => item.Contains("Resources") && (!item.Contains(".meta"))).ToList();
        List<string> allPrefabs = allResources.Where(item => item.Contains(".prefab")).ToList();
        List<string> requiredAssets = AssetDatabase.GetDependencies(allPrefabs.Select(item => item.Remove(0, 39)).ToArray()).ToList();
        List<string> requiredAssetsFixedPaths = allResources.Select(item => item.Remove(0, 39).Replace("\\", "/")).ToList();
        List<string> overlapping = requiredAssetsFixedPaths.Intersect(requiredAssets).ToList();

        Debug.Log(requiredAssets[0]);
        Debug.Log(requiredAssetsFixedPaths[0]);

        Debug.Log(requiredAssets.Count);
        Debug.Log(overlapping.Count);
        Debug.Log(EditorGUIUtility.systemCopyBuffer = String.Join("\n", requiredAssetsFixedPaths.Except(requiredAssets).Where(item => !item.Contains(".prefab")).ToArray()));
    }

#if CLIENT_BUILD
    [MenuItem("VELA/Project/List All Redundant Images.")]
    static void GetAListOfRedundantImages()
    {
        string selectAllReferencedImagesStatement = @"
	SELECT 
		Illustration.unityref
	FROM 
		Illustration
	INNER JOIN 
		WidgetElement
	ON 
		Illustration.id = WidgetElement.illustrationid OR
		Illustration.id = WidgetElement.illustrationid2 OR
		Illustration.id = WidgetElement.illustrationid3 OR
		Illustration.id = WidgetElement.illustrationid4
UNION	
	SELECT 
		Illustration.unityref
	FROM 
		Illustration
	INNER JOIN 
		PracticeActivityWidgetElement 
	ON 
		Illustration.id = PracticeActivityWidgetElement.illustrationid OR
		Illustration.id = PracticeActivityWidgetElement.illustrationid2 OR
		Illustration.id = PracticeActivityWidgetElement.illustrationid3 OR
		Illustration.id = PracticeActivityWidgetElement.illustrationid4
UNION	
	SELECT 
		Illustration.unityref
	FROM 
		Illustration
	INNER JOIN 
		ScrapbookElement 
	ON 
		Illustration.id = ScrapbookElement.illustrationid1 OR
		Illustration.id = ScrapbookElement.illustrationid2 OR
		Illustration.id = ScrapbookElement.illustrationid3 OR
		Illustration.id = ScrapbookElement.illustrationid4 OR
		Illustration.id = ScrapbookElement.illustrationid5
UNION	
	SELECT 
		Illustration.unityref
	FROM 
		Illustration
	INNER JOIN 
		ScrapbookCategory 
	ON 
		Illustration.id = ScrapbookCategory.illustrationid
UNION
    SELECT 
		Illustration.unityref
	FROM 
		Illustration
	INNER JOIN 
		ScrapbookCategory 
	ON 
		Illustration.id = ScrapbookCategory.illustrationid
UNION
	SELECT 
		WidgetElement.text1 
	FROM 
		WidgetElement 
	WHERE 
		WidgetElement.text1 
	like 
		'spr_%'
UNION
	SELECT 
		WidgetElement.text2 
	FROM 
		WidgetElement 
	WHERE 
		WidgetElement.text2 
	like 
		'spr_%'
UNION
	SELECT 
		WidgetElement.text3 
	FROM 
		WidgetElement 
	WHERE 
		WidgetElement.text3 
	like 
		'spr_%'
UNION
	SELECT 
		WidgetElement.text4 
	FROM 
		WidgetElement 
	WHERE 
		WidgetElement.text4 
	like 
		'spr_%'
UNION
	SELECT 
		WidgetElement.text5 
	FROM 
		WidgetElement 
	WHERE 
		WidgetElement.text5 
	like 
		'spr_%';";

        List<string> allReferencedImageFiles = Database.Instance.Query(selectAllReferencedImagesStatement).ConvertAll(new Converter<Dictionary<string, string>, string>(item => item["Illustration.unityref"]));
        List<string> allFiles = System.IO.Directory.GetFiles(Application.streamingAssetsPath + "/Minigames/Sprites", "*.*", SearchOption.AllDirectories)
            .Where(item => item.Contains(".meta") == false)
            .Select(item => Path.GetFileNameWithoutExtension(item))
            .ToList();

        string[] redundantImages = allFiles.Except(allReferencedImageFiles).Distinct().OrderBy(item => item).ToArray();
        string[] missingImages = allReferencedImageFiles.Except(allFiles).Distinct().OrderBy(item => item).ToArray();

        EditorGUIUtility.systemCopyBuffer = String.Join("\n", redundantImages.ToArray()) + "\n" + new String('-', 80) + "\n" + String.Join("\n", missingImages.ToArray());

        Debug.LogFormat("Copied a list of files to the clipboard. The list contains {0} files that are redundant and {1} that are missing.", redundantImages.Length, missingImages.Length);
    }
#endif



    public struct FileInformation
    {
        public string FilePath;
        public long FileSizeInBytes;
        public long FileSizeInGame;
    }

    private static string[] KnownTextureTypes = new string[] { ".png", ".jpg" };
    private static string[] KnownAudioTypes = new string[] { ".wav" };
    private static string[] KnownMeshTypes = new string[] { ".fbx" };
    private static string[] KnownShadersTypes = new string[] { ".shader" };
    private static string[] KnownAnimationTypes = new string[] { ".anim" };


    [MenuItem("VELA/Project/List Resource Files")]
    static void GetListOfAllResourceFilesInProject()
    {
        List<FileInformation> Textures = new List<FileInformation>();
        List<FileInformation> Audio = new List<FileInformation>();
        List<FileInformation> Meshes = new List<FileInformation>();
        List<FileInformation> Shaders = new List<FileInformation>();
        List<FileInformation> Animations = new List<FileInformation>();
        List<FileInformation> Others = new List<FileInformation>();

        // Get a list of all of the resource folders in the project.
        string[] directoryPaths = Directory.GetDirectories(Application.dataPath, "Resources", SearchOption.AllDirectories);

        foreach (string path in directoryPaths)
        {
            // Find a list of all files within the current folder.
            string[] filePaths = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

            foreach (string file in filePaths)
            {
                // Get the extension for each file and sort it into the correct list.
                string extension = Path.GetExtension(file);

                if (extension == ".meta")
                {
                    continue;
                }

                FileInfo fileInfo = new FileInfo(file);
                FileInformation info = new FileInformation() { FilePath = file, FileSizeInBytes = fileInfo.Length };

                if (KnownTextureTypes.Contains(extension))
                {
                    Textures.Add(info);
                    continue;
                }

                if (KnownAudioTypes.Contains(extension))
                {
                    Audio.Add(info);
                    continue;
                }

                if (KnownMeshTypes.Contains(extension))
                {
                    Meshes.Add(info);
                    continue;
                }

                if (KnownShadersTypes.Contains(extension))
                {
                    Shaders.Add(info);
                    continue;
                }

                if (KnownAnimationTypes.Contains(extension))
                {
                    Animations.Add(info);
                    continue;
                }

                Others.Add(info);
            }
        }

        int TexturesCount = Textures.Count;
        int AudioCount = Audio.Count;
        int MeshCount = Meshes.Count;
        int ShaderCount = Shaders.Count;
        int AnimationCount = Animations.Count;
        int OtherCount = Others.Count;

        float TotalOfSizeTextures = Textures.Aggregate((first, second) => { return new FileInformation() { FilePath = "", FileSizeInBytes = first.FileSizeInBytes + second.FileSizeInBytes }; }).FileSizeInBytes / 1024.0f;
        float TotalOfSizeAudio = Audio.Aggregate((first, second) => { return new FileInformation() { FilePath = "", FileSizeInBytes = first.FileSizeInBytes + second.FileSizeInBytes }; }).FileSizeInBytes / 1024.0f;

        float TotalOfSizeMeshs = 0;

        if (Meshes.Count > 1)
        {
            TotalOfSizeMeshs = Meshes.Aggregate((first, second) => { return new FileInformation() { FilePath = "", FileSizeInBytes = first.FileSizeInBytes + second.FileSizeInBytes }; }).FileSizeInBytes / 1024.0f;
        }
        else if (Meshes.Count == 1)
        {
            TotalOfSizeMeshs = Meshes[0].FileSizeInBytes / 1024.0f;
        }


        float TotalOfSizeShaders = Shaders.Aggregate((first, second) => { return new FileInformation() { FilePath = "", FileSizeInBytes = first.FileSizeInBytes + second.FileSizeInBytes }; }).FileSizeInBytes / 1024.0f;
        float TotalOfSizeAnimations = Animations.Aggregate((first, second) => { return new FileInformation() { FilePath = "", FileSizeInBytes = first.FileSizeInBytes + second.FileSizeInBytes }; }).FileSizeInBytes / 1024.0f;
        float TotalOfSizeOthers = Others.Aggregate((first, second) => { return new FileInformation() { FilePath = "", FileSizeInBytes = first.FileSizeInBytes + second.FileSizeInBytes }; }).FileSizeInBytes / 1024.0f;

        float TotalSize = TotalOfSizeTextures + TotalOfSizeAudio + TotalOfSizeMeshs + TotalOfSizeShaders + TotalOfSizeAnimations + TotalOfSizeOthers;

        float TextureShare = TotalOfSizeTextures / TotalSize;
        float AudioShare = TotalOfSizeAudio / TotalSize;
        float MeshShare = TotalOfSizeMeshs / TotalSize;
        float ShaderShare = TotalOfSizeShaders / TotalSize;
        float AnimationShare = TotalOfSizeAnimations / TotalSize;
        float OthersShare = TotalOfSizeOthers / TotalSize;

        //Debug.Log(string.Format("(Textures) Total: {0} TotalSize: {1} SizeShare: {2} AverageSize: {3}", Textures.Count, TotalOfSizeTextures, TextureShare, (TextureShare / TexturesCount)));
        //Debug.Log(string.Format("(Audio) Total: {0} TotalSize: {1} SizeShare: {2} AverageSize: {3}", Audio.Count, TotalOfSizeAudio, AudioShare, (AudioShare / AudioCount)));
        //Debug.Log(string.Format("(Meshs) Total: {0} TotalSize: {1} SizeShare: {2} AverageSize: {3}", Meshs.Count, TotalOfSizeMeshs, MeshShare, (MeshShare / MeshCount)));
        //Debug.Log(string.Format("(Shaders) Total: {0} TotalSize: {1} SizeShare: {2} AverageSize: {3}", Shaders.Count, TotalOfSizeShaders, ShaderShare, (ShaderShare / ShaderCount)));
        //Debug.Log(string.Format("(Animations) Total: {0} TotalSize: {1} SizeShare: {2} AverageSize: {3}", Animations.Count, TotalOfSizeAnimations, AnimationShare, (AnimationShare / AnimationCount)));
        //Debug.Log(string.Format("(Others) Total: {0} TotalSize: {1} SizeShare: {2} AverageSize: {3}", Others.Count, TotalOfSizeOthers, OthersShare, (OthersShare / OtherCount)));


        List<string> outputFile = new List<string>();

        outputFile.Add("Asset Type          TotalSize        Share      Number of files");
        outputFile.Add(string.Format("Textures            {0:00000000.00}{1}    {2:00.00}%    {3}", TotalOfSizeTextures, "MB", TextureShare * 100, TexturesCount));
        outputFile.Add(string.Format("Audio               {0:00000000.00}{1}    {2:00.00}%    {3}", TotalOfSizeAudio, "MB", AudioShare * 100, AudioCount));
        outputFile.Add(string.Format("Meshes              {0:00000000.00}{1}    {2:00.00}%    {3}", TotalOfSizeMeshs, "MB", MeshShare * 100, MeshCount));
        outputFile.Add(string.Format("Shaders             {0:00000000.00}{1}    {2:00.00}%    {3}", TotalOfSizeShaders, "MB", ShaderShare * 100, ShaderCount));
        outputFile.Add(string.Format("Animations          {0:00000000.00}{1}    {2:00.00}%    {3}", TotalOfSizeAnimations, "MB", AnimationShare * 100, AnimationCount));
        outputFile.Add(string.Format("Other Assets        {0:00000000.00}{1}    {2:00.00}%    {3}", TotalOfSizeOthers, "MB", OthersShare * 100, OtherCount));


        Dictionary<string, FileInformation> allfiles = new Dictionary<string, FileInformation>();

        //Path.GetExtension

        foreach (string path in directoryPaths)
        {
            string[] filePaths = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

            foreach (string file in filePaths)
            {
                FileInfo fileInfo = new FileInfo(file);

                string test = file.Split(new string[] { "Resources" }, System.StringSplitOptions.None)[1].Remove(0, 1).Split(new char[] { '.' })[0];

                UnityEngine.Object obj = Resources.Load(test);

                int size = -1;

                if (obj && obj is Texture2D)
                {
                    size = ResourceChecker.CalculateTextureSizeBytes(obj as Texture2D);
                }

                allfiles.Add(file, new FileInformation() { FileSizeInBytes = fileInfo.Length, FileSizeInGame = size });
            }
        }

        List<string> outputLines = new List<string>();

        foreach (KeyValuePair<string, FileInformation> file in allfiles.OrderBy(item => item.Value.FileSizeInBytes))
        {
            if (file.Value.FileSizeInBytes > 1024)
            {
                if (file.Value.FileSizeInBytes > 1048576)
                {
                    outputLines.Add(((file.Value.FileSizeInBytes / 1024.0f) / 1024.0f) + "MB \t " + ((file.Value.FileSizeInGame / 1024.0f) / 1024.0f) + "MB \t " + file.Key);
                }
                else
                {
                    outputLines.Add((file.Value.FileSizeInBytes / 1024.0f) + "KB \t  " + (file.Value.FileSizeInGame / 1024.0f) + "KB \t " + file.Key);
                }
            }
            else
            {
                outputLines.Add(file.Value.FileSizeInBytes + "B \t" + file.Value.FileSizeInGame + "B \t " + file.Key);
            }

        }

        System.IO.File.WriteAllLines(@"Resources.txt", outputFile.ToArray());
        Debug.Log("File Written");
    }




    
}
