#if CLIENT_BUILD
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using CoreLib;

public abstract class ContentLocaliser : MonoBehaviour 
{
    public string LOCID;

    private void Start()
    {
		CoreEventSystem.Instance.AddListener(PlayerProfile.Messages.PLAYER_LANGUAGE_CHANGED, PrepareLocalisation);
		Init ();
		PrepareLocalisation ();
    }

    void OnDestroy()
    {
		CoreEventSystem.Instance.RemoveListener(PlayerProfile.Messages.PLAYER_LANGUAGE_CHANGED, PrepareLocalisation);
    }

	private void PrepareLocalisation(object param = null)
	{
		string localised = ContentManager.Instance.getString(LOCID);
		LocaliseText (localised);
	}

	protected abstract void Init();
	protected abstract void LocaliseText(string localised);
}
#endif