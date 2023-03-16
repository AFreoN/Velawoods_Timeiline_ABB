using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
using cakeslice;
using CustomExtensions;
using HighlightingSystem;
using CoreSystem;
using System.Collections;
using UnityEngine.SceneManagement;

namespace CustomTracks
{
    [System.Serializable]
    public class TouchAndJumpBehaviour : PlayableBehaviour
    {
        public List<TouchAndJumpClip.TouchableData> touchables = new List<TouchAndJumpClip.TouchableData>();
        [HideInInspector]
        public List<GameObject> touchableGameObjects
        {
            get
            {
                if (touchables == null) return null;

                List<GameObject> result = new List<GameObject>();
                foreach (TouchAndJumpClip.TouchableData td in touchables)
                    result.Add(td.touchObject);

                return result;
            }
        }

        [HideInInspector] public bool pause = true;
        bool isTriggerered = false;
        bool changingScene = false;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            if (touchables == null || touchables.Count == 0 || isTriggerered) return;

            FireEvent();
            isTriggerered = true;

            /*for (int i = 0; i < touchables.Count; i++)
            {
                TouchAndJumpClip.TouchableData data = touchables[i];

                Outline o = data.touchObject.GetComponent<Outline>();

                if (data.shouldFlash && o == null)
                {
                    data.touchObject.AddComponent<Outline>();
                }

                data.touchObject.executeAction((ObjectClick obj) => UnityEngine.Object.Destroy(obj));
                data.touchObject.AddComponent<ObjectClick>().Initialize(data.touchObject.transform, OnTouch, false);
            }

            isTriggerered = true;
            if (pause)
                TimelineController.instance.PauseTimeline();*/
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            if (playable.isPlayableCompleted(info))
            {
                RemoveMenuListeners();
                CoreEventSystem.Instance.RemoveListener(CoreEventTypes.LEVEL_CHANGE, LevelChange);

                /*for (int i = 0; i < touchables.Count; i++)
                {
                    TouchAndJumpClip.TouchableData data = touchables[i];

                    data.touchObject.executeAction((Outline o) => UnityEngine.Object.Destroy(o));
                    data.touchObject.executeAction((ObjectClick o) => UnityEngine.Object.Destroy(o));
                }*/

                isTriggerered = false;
            }
        }

        public void OnTouch(GameObject _touchedObject)
        {
            int index = touchableGameObjects.IndexOf(_touchedObject);
            TouchAndJumpClip.TouchableData td = touchables[index];
            if (td.skipTo != -1f)
            {
                for (int i = 0; i < touchables.Count; i++)
                {
                    TouchAndJumpClip.TouchableData data = touchables[i];

                    data.touchObject.executeAction((Outline o) => Object.Destroy(o));
                    data.touchObject.executeAction((ObjectClick o) => Object.Destroy(o));
                }

                isTriggerered = false;

                TimelineController.instance.SkipTimeline(td.skipTo, true);
                Camera.main.GetComponent<CameraApartmentController>().ObjectTouched(null);
            }
            //Debug.Log("Calling on touch from : " + _touchedObject.name);
        }

        public override void OnGraphStop(Playable playable)
        {
            if (touchables != null)
                touchables.Clear();
        }

        void FireEvent()
        {
            HandleTouchObjects(true);
            TimelineController.instance.PauseTimeline();

#if CLIENT_BUILD
        CoreEventSystem.Instance.AddListener(MainMenu.Messages.MENU_SHOWING, MenuShow);
		CoreEventSystem.Instance.AddListener(MainMenu.Messages.MENU_HIDING, MenuHide);
#endif
            CoreEventSystem.Instance.AddListener(CoreEventTypes.LEVEL_CHANGE, LevelChange);
        }

        void LevelChange(object parameters)
        {
            changingScene = true;
            HandleTouchObjects(false);
            RemoveMenuListeners();
        }

        void HandleTouchObjects(bool areTouchable)
        {
            FlashColour flashscript;

            for(int i = 0; i < touchables.Count; i++)
            {
                flashscript = touchables[i].touchObject.GetComponent<FlashColour>();

                if(flashscript != null)
                {
                    flashscript.ObjectTouched -= ObjectTouched;
                    Object.Destroy(flashscript);
                }

                if (areTouchable)
                {
                    flashscript = touchables[i].touchObject.AddComponent<FlashColour>();
                    flashscript.ObjectTouched += ObjectTouched;

                    flashscript.ShouldFlash = touchables[i].shouldFlash;
                }
            }
        }

        void ObjectTouched(object obj)
        {
            RemoveMenuListeners();

            GameObject touchedObj = (GameObject)obj;

            int index = -1;
            for(int i = 0; i < touchables.Count; i++)
            {
                if(touchables[i].touchObject == touchedObj)
                {
                    index = i;
                    break;
                }
            }

            //Check Index
            if(index == -1)
            {
                Debug.LogWarning("TouchAndJump.cs : Touch object not found in the script's _touchObjects array!");
                return;
            }

            if(touchables[index].skipTo != -1)
            {
                TimelineController.instance.SkipTimeline(touchables[index].skipTo);

                HandleTouchObjects(false);
                CoreEventSystem.Instance.SendEvent(CameraApartmentController.MOVE_OBJECT_TOUCHED);

                return;
            }

#if CLIENT_BUILD    // (HUG: sorry!)
            //HACK : get image
            string imageToShow = GetImage(index);
            if(imageToShow != "")
            {
                GameObject iv = ImageViewer.CreateAndShow(imageToShow).gameObject;
                if (iv != null)
                {
                    // Handle scene
                    HandleTouchObjects(false);
                    CoreEventSystem.Instance.SendEvent(CameraApartmentController.MOVE_OBJECT_TOUCHED);

                    // Check for end
                    _coroutineRunner.StartCoroutine(CheckForEndOfPrefab(iv));


                    return;
                }
            }
#endif

            //Get Prefab
            GameObject jumpToPrefab = GetPrefab(index);

            if(jumpToPrefab != null)
            {
                jumpToPrefab = UnityEngine.Object.Instantiate(jumpToPrefab);

#if CLIENT_BUILD
                CoreEventSystem.Instance.SendEvent(PracticeActivityBase.PRACTICE_ACTIVITY_EXTERNAL_START, "");
#endif

                LayerSystem.Instance.AttachToLayer("ImageViewer", jumpToPrefab);

                //Would write this ideally but this would reference a class not in core.
                //LayerSystem.Instance.AttachToLayer (UILayers.ImageViewer.ToString(), jumpToPrefab);

                // Handle scene
                HandleTouchObjects(false);
                CoreEventSystem.Instance.SendEvent(CameraApartmentController.MOVE_OBJECT_TOUCHED);

                // Check for end
                _coroutineRunner.StartCoroutine(CheckForEndOfPrefab(jumpToPrefab));

                return;
            }

#if CLIENT_BUILD
            if(jumpToPrefab != null && touchables[index].jumpToPrefabNames.Trim() != "")
            {
                if(touchables[index].jumpToPrefabNames == "Scrapbook")
                {
                    SceneLoader apartmentScene = new SceneLoader();
                    apartmentScene.LaunchOtherScene("scrapbook", "ScrapBook_Scene");
                }
            }
#endif

            //Debug error message if previous failed
            Debug.LogWarning("TouchAndJump.cs : Event or Prefab to jump to not found! Please double-check data.");
            return;
        }

        private string GetImage(int index)
        {
            string result = "";
            // Check if prefab name is present
            if (touchables.Count >= index || touchables[index] == null)
                return result;

            string prefabName = touchables[index].jumpToPrefabNames;
            if (string.IsNullOrEmpty(prefabName) || string.IsNullOrWhiteSpace(prefabName))
                return result;

            if (prefabName.StartsWith("IMG_"))
            {
                return "DummyScreens/" + touchables[index].jumpToPrefabNames;
            }
            return "";
        }

        /// <summary> 
        /// Get prefab if present </summary>
        private GameObject GetPrefab(int index)
        {
            // Check if prefab name is present
            if (index >= touchables.Count) return null;

            string prefabName = touchables[index].jumpToPrefabNames;
            if (prefabName.Trim() != "")
            {
                // Get prefab
                GameObject prefab = Resources.Load("MiniGames/" + prefabName) as GameObject;
                if (prefab == null)
                {
                    // If no prefab found, try a none MiniGames folder
                    prefab = Resources.Load(prefabName) as GameObject;
                }
                return prefab;
            }
            return null;
        }

        private IEnumerator CheckForEndOfPrefab(GameObject prefab)
        {
            while (prefab != null)
            {
                // Is playing
                yield return null;
            }

            OnPrefabDestroyed();
        }

        private void OnPrefabDestroyed()
        {
            //If in process of changing scene don't reenable prefab touching
            if (changingScene == false)
            {
                // Handle scene when prefab gets destroyed
                HandleTouchObjects(true);
                CoreEventSystem.Instance.SendEvent(CameraApartmentController.FREE_SPIN_ACTIVE);
            }
        }

        void RemoveMenuListeners()
        {
#if CLIENT_BUILD
            CoreEventSystem.Instance.RemoveListener(MainMenu.Messages.MENU_SHOWING, MenuShow);
		    CoreEventSystem.Instance.RemoveListener(MainMenu.Messages.MENU_HIDING, MenuHide);
#endif
        }

        /// <summary> 
        ///  Remove touch events when menu shown</summary>
        private void MenuShow(object parameters)
        {
            HandleTouchObjects(false);
        }

        /// <summary> 
        ///  Add touch events back in when menu hidden</summary>
        private void MenuHide(object parameters)
        {
            HandleTouchObjects(true);
        }

        class CoroutineRunner : MonoBehaviour { }
        CoroutineRunner _coroutineRunner { get { return _coroutineRunner ?? InitCoroutineRunner(); } set { } }
        CoroutineRunner InitCoroutineRunner()
        {
            GameObject instance = new GameObject();
            instance.isStatic = true;
            _coroutineRunner = instance.AddComponent<CoroutineRunner>();
            return _coroutineRunner;
        }
    }
}
