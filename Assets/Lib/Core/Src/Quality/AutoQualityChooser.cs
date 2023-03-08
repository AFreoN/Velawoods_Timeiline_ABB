#define QUALITY_DEBUG

using UnityEngine;
using System.Collections.Generic;

namespace CoreLib
{
	public class AutoQualityChooser : MonoBehaviour
	{
		//minimal framerate (if current FPS is lower, quality should decrease immediately)
		public float minAcceptableFramerate = 30;
		//current quality (as text, visible in inspector)
		public string currentQuality;
		//current framerate (calculated while component is running)
		public float currentFramerate;
		//disable component if user changed quality manually (for example in menu)
		public bool forceBestQualityOnStart = true;
		//how many times per second framerate should be checked
		public float updateRate = 1f;  // how much updates per second.
		//Guard avoiding changing quality backwards and forwards
		//If threshold is set to X, it means that quality won't increase until framerate
		//will be higher than minAcceptableFramerate+X
		public float threshold = 5;
		//current quality number
		private int currQuality;
		private int frameCount = 0;
		private float nextUpdate = 0.0f;
		private bool ignoreOneIteration = true;
        private List<GameObject> shadowGameObjects;
		
		void Start () 
		{
			if(forceBestQualityOnStart)
			{
				QualitySettings.SetQualityLevel(QualitySettings.names.Length - 1);
				currQuality = QualitySettings.GetQualityLevel();
				currentQuality = "" + currQuality + " (" + QualitySettings.names[currQuality] + ")";
				QualityLog("Quality on start: " + currentQuality);
			}
			else
			{
				aproxQuality();
			}
			restartComponent();
			nextUpdate = Time.realtimeSinceStartup + 1.0f / updateRate;
		}
		
		private void aproxQuality()
		{
            Debug.LogError("AutoQualityChooser::aproxQuality This script is actually being used, please fix the deprecated variables.");

            return;
			/*var fillrate = SystemInfo.graphicsPixelFillrate;
			var shaderLevel = SystemInfo.graphicsShaderLevel;
			var videoMemory = SystemInfo.graphicsMemorySize;
			var processors = SystemInfo.processorCount;
			if (fillrate < 0)
			{
				if (shaderLevel < 10) fillrate = 1000;
				else if (shaderLevel < 20) fillrate = 1300;
				else if (shaderLevel < 30) fillrate = 2000;
				else fillrate = 3000;
				if (processors >= 6) 	fillrate *= 3;
				else if (processors >= 3) fillrate *= 2;
				if (videoMemory >= 512) 	fillrate *= 2;
				else if (videoMemory <= 128) fillrate /= 2;
			}
			float fillneed = (Screen.width * Screen.height + 400 * 300) * (minAcceptableFramerate / 1000000.0f);
			float[] levelmult = new float[]{5.0f, 30.0f, 80.0f, 130.0f, 200.0f, 320.0f};
			int level = 0;
			while ((level < QualitySettings.names.Length - 1) && fillrate > fillneed * levelmult[level + 1])
			{
				++level;
			}
			QualitySettings.SetQualityLevel(level);
			currQuality = QualitySettings.GetQualityLevel();
			currentQuality= "" + currQuality + " (" + QualitySettings.names[currQuality] + ")";
			QualityLog("Quality on start: " + currentQuality);*/
		}
		
		public void restartComponent()
		{
			currQuality = QualitySettings.GetQualityLevel();
            shadowGameObjects = new List<GameObject>();
    }
		
		void Update () 
		{
			frameCount++;
			if (Time.realtimeSinceStartup > nextUpdate)
			{
				nextUpdate = Time.realtimeSinceStartup + 1.0f / updateRate;
				currentFramerate = frameCount * updateRate;
				frameCount = 0;
				if(currQuality != QualitySettings.GetQualityLevel())
				{
					currQuality = QualitySettings.GetQualityLevel();
					currentQuality= "" + currQuality + " ("+QualitySettings.names[currQuality] + ")";
				}
				currQuality = QualitySettings.GetQualityLevel();
				if(ignoreOneIteration)
				{
					ignoreOneIteration = false;
					return;
				}
				
				if(currentFramerate < minAcceptableFramerate)
				{
					decreaseQuality();
				}
				else if(currentFramerate - threshold > minAcceptableFramerate)
				{
					increaseQuality();
				}
			}
		}
		
		public void increaseQuality()
		{
			changeQuality(1);   
		}
		
		public void decreaseQuality()
		{
			changeQuality(-1);
		}
		
		private void changeQuality(int amount)
		{
            UpdateCharacterShadows(currQuality + amount);

			if(amount > 0)
			{
				if(currQuality + amount >= QualitySettings.names.Length)return;
			}
			else
			{
				if(currQuality + amount<0)return;
			}
			QualitySettings.SetQualityLevel(currQuality+amount);
			currQuality = QualitySettings.GetQualityLevel();
			currentQuality = "" + currQuality + " (" + QualitySettings.names[currQuality] + ")";
			ignoreOneIteration = true;
            
            if (amount > 0)
			{
				QualityLog("Quality increased to "+currQuality+", framerate: "+currentFramerate);
			}
			else
			{
				QualityLog("Quality decreased to "+currQuality+", framerate: "+currentFramerate);
			}
		}

        private void UpdateCharacterShadows(int quality)
        {
            if (quality < 3)
            {
                //Turn off character shadows
                ToggleCharacterShadows(false);
            }
            else if (quality >= 4)
            {
                //Turn on character shadows
                ToggleCharacterShadows(true);
            }
            
        }

        private void ToggleCharacterShadows(bool enable)
        {
            if(enable)
            {
                foreach(GameObject shadow in shadowGameObjects)
                {
                    shadow.SetActive(true);
                }
                shadowGameObjects.Clear();
            }
            else
            {
                Projector[] shadows = FindObjectsOfType<Projector>();
                foreach (Projector shadow in shadows)
                {
                    shadow.gameObject.SetActive(enable);
                    if(shadowGameObjects.Contains(shadow.gameObject) == false)
                    {
                        shadowGameObjects.Add(shadow.gameObject);
                    }
                }
            }
        }

		private void QualityLog(string logString)
		{
#if QUALITY_DEBUG
			Debug.Log(logString);
#endif
		}
	}
}

