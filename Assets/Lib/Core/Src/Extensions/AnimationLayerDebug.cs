#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using CoreLib;
using UnityEngine.UI;
using UnityEditor;

public class AnimationLayerDebug : MonoBehaviour {


	UnityEngine.UI.Text text;
	Animator animator;

	public bool DebugRotation = false;
    public bool DebugStatePosition = false;
    public int LayerToMonitorState = 0;
	// Use this for initialization
	void Start () 
	{
		if(Application.isEditor)
		{
			animator = GetComponent<Animator>();

			GameObject textObject = new GameObject();

			text = textObject.AddComponent<UnityEngine.UI.Text>();

			textObject.transform.SetParent(LayerSystem.Instance.MainCanvas.transform, true);

			text.fontSize = 70;

			textObject.GetComponent<RectTransform>().transform.localPosition = new Vector3(-500.0f, 500.0f, 0.0f);
			textObject.GetComponent<RectTransform>().transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

			textObject.GetComponent<RectTransform>().sizeDelta = new Vector2(800.0f, 400.0f);


			text.font =  AssetDatabase.LoadAssetAtPath<Font>(@"Assets\Lib\TextMesh Pro\Fonts\ARIAL.TTF");
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Application.isEditor)
		{
			if(DebugRotation)
			{
				text.text = string.Format("rootPosition:{0}\nrootRotation: {1}", 
				                          animator.rootPosition, animator.rootRotation.eulerAngles);
			}
            else if(DebugStatePosition)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(LayerToMonitorState);
                //AnimationClipInfo[] test = animator.GetCurrentAnimationClipState(LayerToMonitorState);
                
                //

                text.text = string.Format("Statename: {0}\nProgress: {1}", stateInfo.fullPathHash, stateInfo.normalizedTime % 1.0f);
            }
			else
			{
				text.text = string.Format("Layer 0:{0}\nLayer1: {1}\nLayer2: {2}\nLayer3: {3}", 
			                          	animator.GetLayerWeight(0), animator.GetLayerWeight(1), 
		                          		animator.GetLayerWeight(2), animator.GetLayerWeight(3));
			}

		}
	}
}
#endif