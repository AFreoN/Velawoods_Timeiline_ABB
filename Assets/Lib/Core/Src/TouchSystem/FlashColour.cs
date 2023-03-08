using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace CoreLib
{
    public class FlashColour : TouchSystemBaseCommand
    {
        // As the touch and flash functionality is so closely tied and is used heavily in the apartment, this is the easiest method of providing the ability
        // to have markers that don't flash. 
        public bool ShouldFlash = true;

        public bool customColor; // default color yellow, set to true if you would like to use your own
        public Color flashColor; // must have custom color selected to take effect.
        public bool occluderOff = false;
        private TouchComponent touchComponent;
        private FlashComponent flashComponent;

        public bool enableTouch = true;

        public static float flashFrequency { get; private set; }

        static FlashColour()
        {
            flashFrequency = 1.0f;
        }

        void Awake ()
		{
            touchComponent = gameObject.AddComponent<TouchComponent>();
		}

        void Start()
        {
            if (ShouldFlash)
            {
                if (flashColor == Color.clear)
                {
                    flashColor = new Color(0.762f, 0, 0.082f);
                }

                FlashComponent flashScript = flashComponent = gameObject.AddComponent<FlashComponent>();
                flashScript.Init(flashColor, occluderOff, flashFrequency);

                if (gameObject.GetComponent<dummyForceOccluderOff>() != null)
                { // if object holds this component, then it will force the occluder off.
                    turnOccluderOff();
                }
            }

            if(enableTouch)
                touchComponent.OnTouch += OnTouch;
        }

		public void turnOccluderOff()
		{
			FlashComponent flash = gameObject.GetComponent<FlashComponent> ();
			if(flash != null)
			{
				flash.turnOccluderOff ();
			}
		}

		public void turnOccluderOn()
		{
			FlashComponent flash = gameObject.GetComponent<FlashComponent> ();
			if(flash != null)
			{
				flash.turnOccluderOn ();
			}
		}

		private void OnTouch(GameObject touched)
		{
			ObjectTouched(touched);
			Destroy (this);
		}

		public void OnDestroy()
		{
			if(touchComponent) {
				touchComponent.Reset ();
				Destroy (touchComponent);
			}
			if(flashComponent) {
				flashComponent.Reset ();
				Destroy (flashComponent);
			}
		}

		public int flashingCount()
		{
            return flashComponent != null ? flashComponent.flashingCount() : -1;
		}

		public void flashingCount(int count)
		{
            if (flashComponent != null)
            {
                flashComponent.flashingCount(count);
            }
		}

		void OnDisable()
		{
			touchComponent.EnableTouch(false);
		}
		
		void OnEnable()
		{
			touchComponent.EnableTouch(true);
		}

		public override void DoStateChange (){}
	}
}