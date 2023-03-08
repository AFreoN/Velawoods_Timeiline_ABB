using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace WellFired
{
	/// <summary>
	/// A custom event that will set a GameObject's specific component enable state at a given time. 
	/// </summary>
	public class EnableTouchType : TimelineBehaviour
	{
		/// <summary>
		/// Should we enable the object at the given time.
		/// </summary>
		public bool enableComponent = false;
		
		public TouchType Component;
        private bool StartingState = true;

        void Awake()
        {
            StartingState = Component.enabled;
        }

        public override void OnClipStart(object o)
        {
            FireEvent();
        }

        public void FireEvent()
		{
            if (!Component)
                return;
            
            Component.enabled = enableComponent;
		}
		
		public override void OnSkip ()
		{
            if (!Component)
                return;
            Component.enabled = enableComponent;
            Component.Skip();
		}
		
		private void ChangeState()
		{
			if(!Component)
				return;
			
			Component.enabled = enableComponent;
		}
		
		//public override void OnReset ()
		//{
  //          Component.enabled = StartingState;
		//}

        void Update()
        {
            if (!Application.isPlaying)
            {
                if (!Component)
                    return;

                Text textComponent = Component.gameObject.GetComponent<Text>();

                if (textComponent)
                {
                    //Duration = Component.gap * textComponent.text.Length + ((Component.gap * textComponent.text.Length) * Component.variance);
                }
            }
        }
	}
}