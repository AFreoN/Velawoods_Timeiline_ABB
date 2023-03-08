using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

public class AnimDefaultSettingsUpdater : UnityEditor.AssetModificationProcessor
{
    #region Functions

    // AssetModificationProcessor Functions ------------------------------------

    public static void OnWillCreateAsset(string assetPath)
    {
        // Check if asset is an animation file
        if (Regex.IsMatch(assetPath, @"\.anim$"))
        {
            // Run the defaults update
            AnimationDefaultsUpdaterExecutor.ExecuteDefaultsUpdate(assetPath);
        }
    }

    #endregion
}

public class AnimationDefaultsUpdaterExecutor : EditorWindow
{
    #region Attributes

    // Private Static Variables ------------------------------------------------

    private static AnimationDefaultsUpdaterExecutor _window;
    private static string _assetPath;

    #endregion

    #region Functions

    // EditorWindow Functions --------------------------------------------------

    public void Update()
    {
        // Try and find the animation clip at the specified location
        AnimationClip animationClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(_assetPath, typeof(AnimationClip));

        // If a clip was found
        if (animationClip != null)
        {
            // Update the settings
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(animationClip);
            settings.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(animationClip, settings);
        }

        // Close the window so no more update will happen
        _window.Close();
    }

    // Public Static Functions -------------------------------------------------

    public static void ExecuteDefaultsUpdate(string assetPath)
    {
        // Create an instance of this window (make it 1x1 pixel so it cannot be seen)
        _window = GetWindow<AnimationDefaultsUpdaterExecutor>();
        _window.minSize = _window.maxSize = Vector2.one;

        // Initialise values for the update
        _assetPath = assetPath;
    }

    #endregion
}