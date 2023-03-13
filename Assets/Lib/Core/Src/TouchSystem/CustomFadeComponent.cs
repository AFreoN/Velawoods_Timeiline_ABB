using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using CoreSystem;

public class CustomFadeComponent : MonoBehaviour
{
	public Action OnFadeComplete = delegate {};

	private List<FlashColourDataBase> _fadeObjects;
	private float _targetFadeValue;

	void Awake()
	{
		 _fadeObjects = new List<FlashColourDataBase> ();
		InitialiseFadeObjects (gameObject);
	}

	private void InitialiseFadeObjects(GameObject flashObject)
	{
		//Add self
		Renderer selfRenderer = flashObject.GetComponent<Renderer> ();
		ParticleSystem particleSystem = flashObject.GetComponent<ParticleSystem> ();
		//Particle systems turn themselves off, they don't fade in the same way
		if(particleSystem != null)
		{
			ParticleData particleSystemData = new ParticleData();
			particleSystemData.gameObject = flashObject;
			
			_fadeObjects.Add(particleSystemData);
		}
		else if(selfRenderer != null)
		{
			FlashColourData flashData = new FlashColourData();
			flashData.gameObject = flashObject;
			flashData.renderer = selfRenderer;
			flashData.diffuseTexture = selfRenderer.material.mainTexture;
			flashData.oldMat = selfRenderer.material;
			
			AttachFlashTouchMaterial(flashObject);
			
			flashData.mat = selfRenderer.material;
			flashData.mat.mainTexture = flashData.diffuseTexture;

			_fadeObjects.Add(flashData);
		}
		else
		{
			ContainerData container = new ContainerData();
			container.gameObject = flashObject;

			_fadeObjects.Add(container);
		}
		//Recursively add children
		foreach(Transform child in flashObject.transform)
		{
			InitialiseFadeObjects(child.gameObject);
		}
	}

	private void AttachFlashTouchMaterial(GameObject flashObject)
	{
		flashObject.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/FlashAndTouch");
	}
	
	public void FadeObject(float from, float to, float time, float delay, bool disableOnEnd=false)
	{
		//Stop any fades already happening
		iTween.Stop(gameObject);

		Hashtable ht = iTween.Hash("from",from,"to",to,"time",time,"onupdate","SetFadeValue", "delay", delay, "oncomplete", "OnComplete");

		//make iTween call:
		iTween.ValueTo(gameObject,ht);

		_targetFadeValue = to;

		SetParticleSystemsEnabled (to > 0);
	}

	private void SetParticleSystemsEnabled(bool enabled)
	{
		if(_fadeObjects != null)
		{
			foreach(FlashColourDataBase fadeObject in _fadeObjects)
			{
				if(fadeObject is ParticleData)
				{
					ParticleData particleSystem = (ParticleData)fadeObject;
					if(enabled)
					{
						particleSystem.gameObject.GetComponent<ParticleSystem>().Play();
					}
					else
					{
						particleSystem.gameObject.GetComponent<ParticleSystem>().Stop();
					}
				}	
			}
		}
	}
	
	public void OnComplete()
	{
		//Set all faded out objects to inactive
		if(_targetFadeValue == 0)
		{
			// Dave said only deactivate the parent object. 
			_fadeObjects[0].gameObject.SetActive(false);

			//foreach(FlashColourDataBase fadeObject in _fadeObjects)
            for (int index = 0; index < _fadeObjects.Count; ++ index)
            {
                Reset();
            }
		}

		OnFadeComplete ();
	}

	public void SetFadeValue(float alphaVal)
	{
		foreach(FlashColourDataBase fadeObject in _fadeObjects)
		{
			if(fadeObject is FlashColourData)
			{
				FlashColourData flashObject = (FlashColourData)fadeObject;
				flashObject.mat.SetFloat("_Alpha", alphaVal);
			}
		}
	}
	
	public void Reset()
	{
		iTween.Stop(gameObject);
		
		if(_fadeObjects != null)
		{
			foreach(FlashColourDataBase flashObjectBase in _fadeObjects)
			{
				FlashColourData flashObject = flashObjectBase as FlashColourData;

				if (flashObject!=null)
				{
					flashObject.mat.SetFloat("_Flash", 0.0f);
					flashObject.mat.SetFloat("_Alpha", 1.0f);
					flashObject.renderer.material = flashObject.oldMat;
				}
			}
		}
	}

	void OnDestroy()
	{
		Reset ();
	}
	
	public class FlashColourDataBase
	{
		public GameObject gameObject;
	}

	public class FlashColourData : FlashColourDataBase
	{
		public Renderer renderer;
		public Material mat;
		public Material oldMat;
		public Texture diffuseTexture;
	}

	public class ParticleData : FlashColourDataBase{}
	
	public class ContainerData : FlashColourDataBase{}
}
