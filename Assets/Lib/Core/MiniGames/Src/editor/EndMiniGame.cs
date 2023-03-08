using UnityEngine;
using System.Collections;
using UnityEditor;

public class EndMiniGame : MonoBehaviour {

	[MenuItem("Window/Route1/Core/EndMiniGame %M")]
	static void EndMethod()
	{
		MiniGameManager.Instance.EndMinigame();
	}
}
