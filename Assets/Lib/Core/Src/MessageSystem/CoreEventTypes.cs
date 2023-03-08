public class CoreEventTypes
{
	//Application
	public const string ON_APPLICATION_QUIT = "ApplicationQuit";
	public const string ON_APPLICATION_PAUSE = "ApplicationPaused";
	public const string ON_APPLICATION_RESUME = "ApplicationResumed";
	//Activity
	public const string ACTIVITY_SKIP = "ActivitySkip";
	public const string ACTIVITY_REVERSE = "ActivityReverse";
	public const string ACTIVITY_SKIP_FINISHED = "ActivitySkipFinished";
	public const string ACTIVITY_QUIT = "ActivityQuit";
	public const string ACTIVITY_CHANGED = "ActivityChanged";
	public const string CARNEGIE_ACTIVITY_END = "CarnegieActivityEnd";
	//USequencer
	public const string SEQUENCE_EDITING_SKIP = "SequenceSkippedToBlueLine";
	//Task
	public const string TASK_CHANGED = "TaskChanged";
	public const string TASK_BAR_SHOWN = "TaskBarShown";
	//Tutorial
	public const string TUTORIAL_HIDDEN = "TutorialHidden";
	//Mission
	public const string MISSION_START = "MissionStart";
	public const string MISSION_END = "MissionEnd";
	public const string MISSION_SETUP = "MissionSetUp";
	public const string MISSION_END_SEQUENCE = "MissionEndSequence";
	//Mission loading
	public const string MISSION_LOADING_PROGRESS = "MissionLoadingProgress";
	public const string MISSION_DOWNLOAD_FINISHED = "MissionDownloadFinished";
	public const string MISSION_LOADING_FINISHED = "MissionLoadingFinished";
	public const string MISSION_LOADING_FAILED = "MissionLoadingFailed";
	public const string MISSION_LOADING_RETRY = "MissionLoadingRetry";
	public const string MISSION_DATA_LOADED = "MissionDataLoaded";
	//Minigames
	public const string MINIGAME_CREATED = "MinigameCreated";
	public const string MINIGAME_FAIL = "MinigameFailed";
	public const string MINIGAME_COMPLETE = "MinigameComplete";
	public const string MINIGAME_START = "MinigameStart";
	public const string MINIGAME_END = "MinigameEnd";
	public const string MINIGAME_SUCCESSFUL_SKIP = "MinigameSuccessfulSkip";
	//UI
	public const string WEBVIEW_CLOSED = "WebViewClosed";
	//Misc
	public const string LOGIN_SUCCESS = "LoginSuccess";
	public const string LOADING_SCREEN_COMPLETE = "LoadingScreenComplete";
	public const string LOAD_LEVEL = "LoadLevel";
	public const string DO_NOT_LOAD_LEVEL = "DoNotLoadLevel";
	public const string LEVEL_CHANGE = "LevelChange";
	public const string BOOKMARK_ACTIVITY = "BookmarkActivity";
	public const string SCENE_OBJECT_TOUCHED = "3DObjectTouched";
	public const string ACTIVITY_NAVIGATION_HIDE = "ActivityButtonsHide";
	public const string OBJECTIVE_SCREEN_CLOSE = "ObjectiveScreenClose";
    public const string CURRENT_LANGUAGE_UPDATED = "CurrentLanguageUpdated";
    public const string PRODUCT_STORED_INFO_UPDATED = "ProductStoredInfoUpdated";
	//Carnegie
	public const string CARNEGIE_SUCCESS = "CarnegieSuccess";
	public const string CARNEGIE_FAIL = "CarnegieFail";
	public const string CARNEGIE_SKIP = "CarnegieSkip";
	//Network
	public const string NETWORK_CONNECTED = "NetworkConnected";
	public const string NETWORK_CLOSED = "NetworkClosed";
	//Features
	public const string FEATURE_START = "FeatureStart";
	public const string FEATURE_END = "FeatureEnd";
	//IStackableUI
	public const string STACKABLE_UI_OPEN = "StackableUIOpen";
	public const string STACKABLE_UI_CLOSE = "StackableUIClose";
}
