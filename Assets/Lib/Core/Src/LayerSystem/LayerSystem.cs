//#define LAYER_SYSTEM_LOG

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace CoreLib
{
    public class LayerSystem : MonoSingleton<LayerSystem>
    {
        private GameObject _mainCanvas;
        private Dictionary<string, GameObject> _layers;
        private List<string> _mainLayers;
		private Vector2 _refResolution;

        protected override void Init()
        {
			Log("LayerSystem: Init()");

            try
            {
				_layers = new Dictionary<string, GameObject>();
				_mainLayers = new List<string>();

				GetCanvas();

				_refResolution = _mainCanvas.GetComponent<CanvasScaler>().referenceResolution;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

		private void GetCanvas()
		{
			GameObject scene_canvas = GameObject.Find("MainCanvas");
			
			if(scene_canvas == null)
			{
				_mainCanvas = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("LayerSystem/MainCanvas"));
				_mainCanvas.name = "MainCanvas";
			}
			else
			{
				_mainCanvas = scene_canvas;
			}
		}

		public Vector2 RefResolution
		{
			get{ return _refResolution; }
		}

        public GameObject MainCanvas
        {
            get { return _mainCanvas; }
        }

		public void Destroy()
		{
			if (_mainCanvas != null) 
			{
				CoreHelper.SafeDestroy(_mainCanvas);
			}
		}

        /// <summary>
        /// Changes the layers position in the layer order.
        /// </summary>
        /// <param name="layer_name"></param>
        /// <param name="layer_order"></param>
        public void ChangeLayerOrder(string layer_name, int layer_order)
        {
			Log("LayerSystem: Changing Layer " + layer_name + " to layer number " + layer_order);

            GameObject layer = GetLayer(layer_name);
            layer.transform.SetSiblingIndex(layer_order);
        }

        /// <summary>
        /// Sets the layer to either the front or the back of the layers
        /// 
        /// true = front
        /// false = back
        /// </summary>
        /// <param name="layer_name"></param>
        /// <param name="front"></param>
        public void SwitchLayerOrder(string layer_name, bool front)
        {
			Log("LayerSystem: Bringing Layer " + layer_name + " to the front");

            GameObject layer = GetLayer(layer_name);

            if(front)
            {
                layer.transform.SetAsFirstSibling();
            }
            else
            {
                layer.transform.SetAsLastSibling();
            }
        }

        private bool IsMainLayer(string layer_name)
        {
            foreach(string layer in _mainLayers)
            {
                if (layer == layer_name)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the layer and all its children from the game.
        /// 
        /// If the layer is one of the main layers for the game it will not be 
        /// deleted.
        /// </summary>
        /// <param name="layer_name"></param>
        public void RemoveLayer(string layer_name)
        {
            if(LayerExists(layer_name) && !IsMainLayer(layer_name))
            {
				CoreHelper.SafeDestroy(_layers[layer_name]);
                _layers[layer_name] = null;
                _layers.Remove(layer_name);

				Log("LayerSystem: Removed Layer " + layer_name);
            }
        }

        private void ChangeParent(Transform your_object, Transform attach_to)
        {
            your_object.SetParent(attach_to, false);
        }
        private void ChangeParent(Transform your_object, Transform attach_to, bool keep_world_position)
        {
            your_object.SetParent(attach_to, keep_world_position);
        }
        public void SetDestroyable(string layer_name, bool canBeDestroyed)
		{
			if (_layers.ContainsKey (layer_name))
			{
				if(canBeDestroyed)
				{
					if(_mainLayers.Contains(layer_name))
					{
						_mainLayers.Remove(layer_name);

						Log("LayerSystem: Layer " + layer_name + " can now be destoryed");
					}
				}
				else if(!_mainLayers.Contains(layer_name))
				{
					_mainLayers.Add(layer_name);

					Log("LayerSystem: Layer " + layer_name + " cannot be destoryed");
				}
			}
		}

        /// <summary>
        /// Creates a layer with the specified name.
        /// </summary>
        /// <param name="layer_name"></param>
        public void CreateLayer(string layer_name, bool canBeDestroyed=true)
		{
			if(_mainCanvas == null) GetCanvas();

            if(!LayerExists(layer_name))
            {
                GameObject new_layer = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("LayerSystem/Layer"));

				if(new_layer == null) 
				{
					Log("LayerSystem: No Layer Object found in Resources", "Error");
					return;
				}

                new_layer.name = layer_name;

                ChangeParent(new_layer.transform, _mainCanvas.transform);

                _layers.Add(layer_name, new_layer);

				SetDestroyable(layer_name, canBeDestroyed);

				Log("LayerSystem: Created Layer " + layer_name);
            }
            else
            {
                Log("LayerSystem: Layer already exists: " + layer_name);
            }
        }




        /// <summary>
        /// Attaches an object to the layer specified. This method 
        /// will create the layer if it does not exist.
        /// </summary>
        /// <param name="layer_name"></param>
        /// <param name="your_object"></param>
        public void AttachToLayer(string layer_name, GameObject your_object)
        {
			if (_mainCanvas == null) {
				Init();
			}

            if(your_object == null)
            {
               Log("LayerSystem: Your Object was null when passed in AttachToLayer()", "Error");
                return;
            }

            ChangeParent(your_object.transform, GetLayer(layer_name).transform);

			Log("LayerSystem: Attaching " + your_object.name + " to Layer " + layer_name);
        }

        public void AttachToLayer(string layer_name, GameObject your_object, bool keep_world_position)
        {
            if (_mainCanvas == null)
            {
                Init();
            }

            if (your_object == null)
            {
                Log("LayerSystem: Your Object was null when passed in AttachToLayer()", "Error");
                return;
            }

            ChangeParent(your_object.transform, GetLayer(layer_name).transform, keep_world_position);

            Log("LayerSystem: Attaching " + your_object.name + " to Layer " + layer_name);
        }




        /// <summary>
        /// Attaches an object to the layer at the intended layer_order.
        /// if the layer_order does not exist it will be placed on the top layer.
        /// 
        /// If the layer order is below 0 then the bottom layer will be used
        /// </summary>
        /// <param name="layer_name"></param>
        /// <param name="layer_order"></param>
        public void AttachToLayer(int layer_order, GameObject your_object)
        {
            if (your_object == null)
            {
               Log("LayerSystem: Your Object was null when passed in AttachToLayer()", "Error");
                return;
            }

            if(layer_order < 0)
            {
                layer_order = 0;
            }
            else if(layer_order > (_mainCanvas.transform.childCount-1))
            {
                layer_order = _mainCanvas.transform.childCount-1;
            }

            GameObject layer = _mainCanvas.transform.GetChild(layer_order).gameObject;

            your_object.transform.SetParent(layer.transform);

			Log("LayerSystem: Attaching " + your_object.name + " to Layer " + layer.name);
        }

        /// <summary>
        /// Does the layer exist
        /// </summary>
        /// <param name="layer_name"></param>
        /// <returns></returns>
        public bool LayerExists(string layer_name)
        {
            return _layers.ContainsKey(layer_name);
        }

        /// <summary>
        /// Gets a layer and if it does not exist it will
        /// create it.
        /// </summary>
        /// <param name="layer_name"></param>
        /// <returns></returns>
        public GameObject GetLayer(string layer_name)
        {
            if (!LayerExists(layer_name))
            {
                CreateLayer(layer_name);
            }

            return _layers[layer_name];
        }



        /// <summary>
        /// Changes a layers active property to the value
        /// given
        /// </summary>
        /// <param name="layer_name"></param>
        /// <param name="value"></param>
        public void ChangeVisible(string layer_name, bool value)
        {
            if(LayerExists(layer_name))
            {
                _layers[layer_name].SetActive(value);

				if(value)
					Log("LayerSystem: Layer " + layer_name + " is now visible");
				else
					Log("LayerSystem: Layer " + layer_name + " is now not visible");
            }
        }

		public void PrintLayers()
		{
			Log("LayerSystem: Printing Layers");

			if(_layers != null)
			{
				foreach(string layer_name in _layers.Keys)
				{
					Log("LayerSystem: Layer Name - " + layer_name);
				}
			}

			Log("LayerSystem: Finished Printing Layers");
		}

		public void ClearLayer(string delete_layer_name)
		{
			if(_layers != null)
			{
				foreach(string layer_name in _layers.Keys)
				{
					if(layer_name == delete_layer_name)
					{
						GameObject delete_layer = _layers[layer_name];
						foreach(Transform obj in delete_layer.transform)
						{
							GameObject.Destroy(obj.gameObject);
						}
					}
				}
			}
		}

        public void Log(string log, string type = null)
        {

#if LAYER_SYSTEM_LOG
            if (type == "Error")
            {
                Debug.LogError(log);
            }
            else
            {
                Debug.Log(log);
            }
#endif
        }


		//TOM: Functions for resizing the mini game layer for using objects ie. Magazine and mobiles, as backgrounds for Minigames.
		
		// Used to set mini game layer to custom size
		public void SetDimensionsOnMinigameLayer(float xmin = 0.0f, float xmax = 1.0f, float ymin = 0.0f, float ymax = 1.0f, RectTransform minigameZone = null)
        {
			GameObject minigameLayer = GetLayer ("MiniGames");
			if (minigameLayer != null)
            {
                RectTransform trans = minigameLayer.GetComponent<RectTransform>();
                if (minigameZone != null)
                {
                    trans.pivot = minigameZone.pivot;
                    trans.anchorMin = minigameZone.anchorMin;
                    trans.anchorMax = minigameZone.anchorMax;

                    trans.anchoredPosition = minigameZone.anchoredPosition;

                    Rect rect = trans.rect;
                    rect.xMin = trans.rect.xMin;
                    rect.xMax = trans.rect.xMax;
                    rect.yMin = trans.rect.yMin;
                    rect.yMax = trans.rect.yMax;

                    trans.sizeDelta = minigameZone.sizeDelta;
                }
                else
                {
                    trans.pivot = new Vector2(0.5f, 0.5f);
                    trans.anchorMin = new Vector2(xmin, ymin);
                    trans.anchorMax = new Vector2(xmax, ymax);

                    trans.anchoredPosition = new Vector2(0.0f, 0.0f);

                    Rect rect = trans.rect;
                    rect.xMin = 0.0f;
                    rect.xMax = 0.0f;
                    rect.yMin = 0.0f;
                    rect.yMax = 0.0f;

                    trans.sizeDelta = Vector2.zero;
                }
			}
		}
		
		// checks to see if the mini game layer is currently full screen.
		public bool CheckIfForcedDimensions(){
			GameObject minigameLayer = GameObject.Find ("MiniGames");
			if (minigameLayer != null) {
				RectTransform trans = minigameLayer.GetComponent<RectTransform>();
				if((trans.anchorMin.x == 0.0f) && (trans.anchorMin.y == 0.0f)&&(trans.anchorMax.x == 1.0f) && (trans.anchorMax.y == 1.0f))
					return(false);
			}
			return(true);
		}

    }
}