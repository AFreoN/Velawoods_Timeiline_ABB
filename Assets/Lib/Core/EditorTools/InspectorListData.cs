using UnityEngine;
using System.Collections.Generic;
using System;

namespace Route1Games
{
    [Serializable()]
    public class InspectorItem
    {
        public Rect ControlRect;
        public GameObject ControlItem;
    }

    [Serializable()]
    public class InspectorListData : MonoBehaviour 
    {
        [SerializeField()]
        [HideInInspector()]
        public List<InspectorItem> DataItems = new List<InspectorItem>();
    }
}