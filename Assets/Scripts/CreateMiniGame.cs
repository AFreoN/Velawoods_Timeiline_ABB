using UnityEngine;
using TMPro;
using UnityEngine.UI;
using CustomExtensions;

public class CreateMiniGame : TimelineBehaviour
{
    [SerializeField] GameObject miniGamePanel = null;
    [SerializeField] TMP_Text titleText = null;
    [SerializeField] Button skipButton = null;

    private void Start()
    {
        miniGamePanel.SetActive(false);
    }

    private void OnEnable()
    {
        skipButton.onClick.AddListener(playTimeline);
    }

    private void OnDisable()
    {
        skipButton.onClick.RemoveListener(playTimeline);
    }

    public override void OnClipStart(object o)
    {
        o.executeAction((CreateMiniGameBehaviour cmg) =>
        {
            ShowMiniGame(cmg.title, cmg.pauseOnFire);
        });
    }

    void ShowMiniGame(string title, bool pause)
    {
        Debug.Log("Showing minigame : " + title);
        titleText.text = title;
        miniGamePanel.SetActive(true);

        if (pause)
            TimelineController.instance.PauseTimeline();
    }

    void playTimeline()
    {
        TimelineController.instance.PlayTimeline();
        miniGamePanel.SetActive(false);
    }
}
