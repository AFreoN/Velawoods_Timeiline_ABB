using System.Collections.Generic;

public class MinigameSectionData
{
	public int section;
	public string subType;
	public MinigameCharacter character;
	public List<MinigameElementData> elements;
	public MinigameIllustration illustration;
	public List<bool> bools;
	public List<string> text;
	public MinigameAudio audio;
	public string notes;
	public int ReferenceActivityID = -1;
	public int ActivityReferenceTypeID = 0;
}

public class MinigameElementData
{

	public int sequence;
	public string subType;
	public MinigameCharacter character;
	public List<MinigameIllustration> illustrations;
	public MinigameSundry sundry;
	public MinigameAnimation animation;
	public List<MinigameAudio> audio;
	public List<bool> bools;
	public List<string> text;
	public string notes;

	// Legacy support for minigame activities that used a single illustration.
	// The illustration object always existed even if it was blank so return a blank illustration if
	// there isn't one in the array
	private static MinigameIllustration _blankIllustration;

	public MinigameIllustration illustration {
		get{
			if ( illustrations != null && illustrations.Count > 0 ) {
				return illustrations[0];
			}else{
				if ( _blankIllustration == null ) _blankIllustration = new MinigameIllustration();
				return _blankIllustration;
			}
		}
	}
}

public class MinigameAudio
{
	public string fileName;
	public string dialog;
}

public class MinigameCharacter
{
	public string characterID;
	public string characterName;
}

public class MinigameIllustration
{
	public int activityID;
	public string unityRef;
	public string description;
}

public class MinigameSundry
{
	public string sundryName;
	public string unityRef;
}

public class MinigameAnimation
{
	public string animation;
}