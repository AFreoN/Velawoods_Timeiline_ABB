public class MissionStateData
{
	public const string APARTMENT_SCENE = "Learner_Apartment_Scene";
    public const string SCRAPBOOK_SCENE = "ScrapBook_Scene";

	public MissionStateData (string sceneName, SceneType sceneType)
	{
		this.sceneName = sceneName;
		this.sceneType = sceneType;
	}
	public string sceneName;
	public SceneType sceneType;
}

public enum SceneType
{
	Mission,
    Test,
	Other
}