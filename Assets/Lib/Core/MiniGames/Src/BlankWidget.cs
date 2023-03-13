using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WellFired;
using CoreSystem;
using UnityEngine.UI;

[ExecuteInEditMode()]
public class BlankWidget : MiniGameBase
{
    public Text _textObj;

    public GameObject referenceTokenButton;

    private GameObject _referenceTokenObject;
    private ReferenceToken _referenceToken;

    protected override void Init()
    {
 	    base.Init();

        string activityid = ActivityTracker.Instance.ActualActivityID;

        List<Dictionary<string, string>> data = Database.Instance.Select("*", "Activity JOIN ActivityType ON Activity.activitytypeid = ActivityType.id", "Activity.id=" + activityid);

        if (data.Count > 0)
            _textObj.text = data[0]["activityname"];
	}

    protected override void ParseData(List<MinigameSectionData> data)
    {
        if(data.Count > 0)
        {
            MinigameSectionData sectionData = data[0];

            if(sectionData.ReferenceActivityID > 0 &&
                sectionData.ActivityReferenceTypeID == 2)
            {
                Debug.Log("Activity has a reference token set in database");
                referenceTokenButton.SetActive(true);

                if (_designerAssignedData.Length > 0)
                {
                    _referenceTokenObject = _designerAssignedData[0];
                    if (_referenceTokenObject != null)
                    {
                        _referenceToken = _referenceTokenObject.GetComponent<ReferenceToken>();
                        _referenceToken.PrefabAnimator.gameObject.SetActive(true);
                        _referenceToken.ContentAnimator.gameObject.SetActive(true);
                    }
                }
            }
        }
    }

    public override void Skip(object paramters)
    {
        base.Skip(paramters);

		End(false);

		GetComponentInChildren<Button>().interactable = false;
    }

	public void OnClick()
	{
		GetComponentInChildren<Button>().interactable = false;

		End(false);
	}

    public void OnReferenceTokenButtonOnClick()
    {
        if (!_referenceTokenObject.activeSelf)
        {
            _referenceTokenObject.SetActive(true);
        }

        if (_referenceToken != null)
        {
            _referenceToken.Open();
        }
        else
        {
            // Display error
            Debug.LogError("GenericMisc_AssistArea::DisplayImageReference() - No reference object set.");
        }
    }
}
