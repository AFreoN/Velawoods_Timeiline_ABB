using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CustomExtensions;

public class DialogOpener : MonoBehaviour
{
    [SerializeField] Image backgroundImg = null;
    [SerializeField] TMP_Text subtitleText = null;
    [SerializeField] Image arrowImg = null;

    [SerializeField] float openDuration = .3f;
    [SerializeField] float closeDuration = .2f;

    float timer = 1, currentDuration = 0;

    // Start is called before the first frame update
    void Start()
    {
        backgroundImg.ChangeAlpha(0);
        subtitleText.ChangeAlpha(0);
        arrowImg.ChangeAlpha(0);
    }

    private void Update()
    {
        if (timer >= 1)
            return;

        timer += Time.deltaTime / currentDuration;
        timer = Mathf.Clamp01(timer);

        backgroundImg.ChangeAlpha(timer);
        subtitleText.ChangeAlpha(timer);
        arrowImg.ChangeAlpha(timer);
    }

    public void OpenDialog()
    {
        timer = 0;
        currentDuration = openDuration;

        //backgroundImg.CrossFadeAlpha(1, openDuration, true);
        //subtitleText.CrossFadeAlpha(1, openDuration, true);
        //arrowImg.CrossFadeAlpha(1, openDuration, true);
    }

    public void CloseDialog()
    {
        backgroundImg.ChangeAlpha(0);
        subtitleText.ChangeAlpha(0);
        arrowImg.ChangeAlpha(0);
        //timer = 0;
        //currentDuration = closeDuration;
        //backgroundImg.CrossFadeAlpha(0, openDuration, true);
        //subtitleText.CrossFadeAlpha(0, openDuration, true);
        //arrowImg.CrossFadeAlpha(0, openDuration, true);
    }
}
