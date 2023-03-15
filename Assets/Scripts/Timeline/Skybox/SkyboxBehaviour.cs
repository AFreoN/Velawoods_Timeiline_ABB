using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


namespace CustomTracks
{
    [Serializable]
    public class SkyboxBehaviour : PlayableBehaviour
    {
        public Material skyMaterial;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (skyMaterial)
            {
                RenderSettings.skybox = skyMaterial;
            }
        }
    }
}
