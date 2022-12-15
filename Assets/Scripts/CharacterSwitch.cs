using UnityEngine;
using CustomExtensions;

public class CharacterSwitch : TimelineBehaviour
{
    GameObject originalGO;
    GameObject clonedGO;

    /// <summary>
    /// Called when the CharacterSwitch track clip starts in timeline
    /// </summary>
    /// <param name="o"></param>
    public override void OnClipStart(object o)
    {
        o.executeAction<CharacterSwitchBehaviour>(switchCharacter);
    }

    public void switchCharacter()   //Duplicate this character that will be controlled independent of timeline, disables timeline character
    {
        originalGO = gameObject;


        clonedGO = Instantiate(gameObject);
        removeComponents();
        setPositions(clonedGO);
        clonedGO.GetComponent<WaypointMovement>().startMoving();
        clonedGO.GetComponent<WaypointMovement>().originalGameobject = gameObject;

        gameObject.SetActive(false);
    }

    void setPositions(GameObject clonedGO)  //Copy the bones position in the original character and paste it in cloned character
    {
        Transform[] originalChilds = gameObject.GetComponentsInChildren<Transform>();
        Transform[] clonedChilds = clonedGO.GetComponentsInChildren<Transform>();

        for (int i = 0; i < clonedChilds.Length; i++)
        {
            clonedChilds[i].localPosition = originalChilds[i].localPosition;
            clonedChilds[i].localRotation = originalChilds[i].localRotation;
            clonedChilds[i].localScale = originalChilds[i].localScale;
        }
    }

    public void enableOriginalCharacter()   //Disable cloned character, enable original timeline character
    {
        if (clonedGO != null) clonedGO.SetActive(false);

        //Implement animation and transform logic here, if required

        originalGO.SetActive(true);
    }

    void removeComponents()     //Remove unwanted components in cloned character
    {
        clonedGO.RemoveComponent<TimelineCharacter>();
        clonedGO.RemoveComponent<AnimPlayer>();
    }
}
