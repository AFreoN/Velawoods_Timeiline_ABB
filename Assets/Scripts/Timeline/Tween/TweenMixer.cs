using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

public class TweenMixer : PlayableBehaviour
{
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Transform t = playerData as Transform;
        Vector3 finalPos = Vector3.zero;
        Vector3 finalRot = Vector3.zero;
        if (!t) return;

        int inputCount = playable.GetInputCount();
        Debug.Log("Input count = " + inputCount);

        if (inputCount == 0) return;

        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            ScriptPlayable<TweenBehaviour> inputPlayable = (ScriptPlayable<TweenBehaviour>)playable.GetInput(i);
            //Debug.Log("Lead time = " + inputPlayable.GetPreviousTime());
            TweenBehaviour tweenInput = inputPlayable.GetBehaviour();

            finalPos += tweenInput.startPosition * inputWeight;
            finalRot += tweenInput.startRotation * inputWeight;
        }

        Debug.Log("Final Pos = " + finalPos);
        //Debug.Log("Final Rot = " + finalRot);

        t.position = finalPos;
        t.rotation = Quaternion.Euler(finalRot);
    }
}
