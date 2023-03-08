using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace Route1Games
{
    public class InspectorList : EditorWindow
    {


        private static string DRAG_DATA_KEY = "InspectorDragKey";

        private InspectorListData InspectorData;
        private List<InspectorItem> tempDataItems = new List<InspectorItem>();

        private int CurrentListSize = 0;
        private int NewListSize = 0;

        private int TempCurrentListSize = 0;
        private int TempNewListSize = 0;

        private bool DisplayKeyItems = false;
        private bool DisplayTempItems = false;
        
        [MenuItem("VELA/Tools/InspectorList")]
        public static void ShowWindow()
        {
            InspectorList window = EditorWindow.GetWindow(typeof(InspectorList)) as InspectorList;
            window.minSize = new Vector2(256.0f, 128.0f);
            window.titleContent = new GUIContent("Inspector List");
            window.currentScene = EditorApplication.currentScene;
        }

        private string currentScene;

        void OnHierarchyChange()
        {
            if (currentScene != EditorApplication.currentScene)
            {
                GetInspectorData();
                CurrentListSize = InspectorData.DataItems.Count;
                currentScene = EditorApplication.currentScene;
            }
        }

        [MenuItem("GameObject/TagAsKey", false, 0)]
        public static void TagAsKey(MenuCommand menuCommand)
        {
            InspectorList window = EditorWindow.GetWindow(typeof(InspectorList)) as InspectorList;
            if (null != window.InspectorData)
            {
                bool HasBeenAdded = false;

                foreach (InspectorItem item in window.InspectorData.DataItems)
                {
                    if(null == item.ControlItem)
                    {
                        item.ControlItem = menuCommand.context as GameObject;
                        HasBeenAdded = true;
                        break;
                    }
                }

                if (false == HasBeenAdded)
                {
                    window.InspectorData.DataItems.Add(new InspectorItem() { ControlItem = (menuCommand.context as GameObject) });
                    ++window.CurrentListSize;
                    window.NewListSize = window.CurrentListSize;
                }
            }
        }

        [MenuItem("GameObject/TagAsTemp", false, 0)]
        public static void TagAsTemp(MenuCommand menuCommand)
        {
            InspectorList window = EditorWindow.GetWindow(typeof(InspectorList)) as InspectorList;
            if (null != window.InspectorData)
            {
                bool HasBeenAdded = false;

                foreach(InspectorItem item in window.tempDataItems)
                {
                    if(null == item.ControlItem)
                    {
                        item.ControlItem = menuCommand.context as GameObject;
                        HasBeenAdded = true;
                        break;
                    }
                }

                if (false == HasBeenAdded)
                {
                    window.tempDataItems.Add(new InspectorItem() { ControlItem = (menuCommand.context as GameObject) });
                    ++window.TempCurrentListSize;
                    window.TempNewListSize = window.TempCurrentListSize;
                }
            }
        }


        void OnInspectorUpdate()
        {
            Repaint();
        }

        private void GetInspectorData()
        {
            if (null == InspectorData)
            {
                InspectorData = FindObjectOfType<InspectorListData>() as InspectorListData;

                if (null == InspectorData)
                {
                    CoreLib.Core coreScript = FindObjectOfType<CoreLib.Core>();

                    if (coreScript && coreScript.gameObject)
                    {
                        InspectorData = coreScript.gameObject.AddComponent<InspectorListData>();
                    }
                    else
                    {
                        this.Close();
                    }
                }
            }
        }

        Vector2 ScrollPosition = Vector2.zero;

        void OnGUI()
        {
            

            // Make sure we have some data in the scene so that it will be serialised and persist.
            GetInspectorData();

            ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);
            DisplayKeyItems = EditorGUILayout.Foldout(DisplayKeyItems, "Key Items");

            if (DisplayKeyItems)
            {
                
                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
                {
                    CurrentListSize = NewListSize;
                }

                EditorGUILayout.Space();

                int tempListSize = EditorGUILayout.IntField("Size", CurrentListSize);

                if (tempListSize != CurrentListSize)
                {
                    NewListSize = tempListSize;
                }

                CoreLib.CoreHelper.ResizeList<InspectorItem>(CurrentListSize, InspectorData.DataItems);
                DrawControls(InspectorData.DataItems);
            }

            DisplayTempItems = EditorGUILayout.Foldout(DisplayTempItems, "Temp Items");

            if (DisplayTempItems)
            {
                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
                {
                    TempCurrentListSize = TempNewListSize;
                }

                EditorGUILayout.Space();

                int tempListSize = EditorGUILayout.IntField("Size", TempCurrentListSize);

                if (tempListSize != TempCurrentListSize)
                {
                    TempNewListSize  = tempListSize;
                }

                CoreLib.CoreHelper.ResizeList<InspectorItem>(TempCurrentListSize, tempDataItems);
                DrawControls(tempDataItems);
                
            }
            EditorGUILayout.EndScrollView();
            List<InspectorItem> allItems = new List<InspectorItem>();

            if(DisplayKeyItems)
            {
                allItems.AddRange(InspectorData.DataItems);
            }

            if(DisplayTempItems)
            {
                allItems.AddRange(tempDataItems);
            }

			Vector2 currentMousePosition = Event.current.mousePosition + new Vector2 (0.0f, ScrollPosition.y);

            if (allItems.Count > 0)
            {
                // Handle dragging and dropping.
                switch (Event.current.type)
                {
                    case EventType.DragUpdated:
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;

                            foreach (InspectorItem item in allItems)
                            {
								if (item.ControlRect.Contains(currentMousePosition))
                                {
                                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                                    break;
                                }
                            }

                            break;
                        }
                    case EventType.DragPerform:
                        {
                            DragAndDrop.AcceptDrag();

                            foreach (UnityEngine.Object unityObject in DragAndDrop.objectReferences)
                            {
                                if (unityObject is GameObject)
                                {
                                    foreach (InspectorItem item in allItems)
                                    {
										if (item.ControlRect.Contains(currentMousePosition))
                                        {
                                            item.ControlItem = unityObject as GameObject;
                                            break;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case EventType.MouseDown:
                        {
                            if (0 == Event.current.button)
                            {
                                if (Event.current.control)
                                {
                                    foreach (InspectorItem item in allItems)
                                    {
										if (item.ControlRect.Contains(currentMousePosition))
                                        {
                                            if (item.ControlItem != null)
                                            {
                                                Selection.activeGameObject = item.ControlItem;
                                                Selection.activeTransform = item.ControlItem.transform;
                                                EditorGUIUtility.PingObject(item.ControlItem);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (InspectorItem item in allItems)
                                    {
										if (item.ControlRect.Contains(currentMousePosition))
                                        {
                                            DragAndDrop.PrepareStartDrag();

                                            DragAndDrop.SetGenericData(DRAG_DATA_KEY, item.ControlItem);

                                            UnityEngine.Object[] objectReferences = new UnityEngine.Object[1] { item.ControlItem };
                                            DragAndDrop.objectReferences = objectReferences;

                                            Event.current.Use();
                                            break;
                                        }
                                    }
                                }
                            }
                            else if(1 == Event.current.button)
                            {
                                InspectorItem itemToRemove = null;

                                foreach (InspectorItem item in allItems)
                                {
									if (item.ControlRect.Contains(currentMousePosition))
                                    {
                                        itemToRemove = item;
                                        break;
                                    }
                                }

                                if(null != itemToRemove)
                                {
                                    if (InspectorData.DataItems.Contains(itemToRemove))
                                    {
                                        InspectorData.DataItems.Remove(itemToRemove);
                                        --CurrentListSize;
                                        NewListSize = CurrentListSize;
                                    }
                                    else if (tempDataItems.Contains(itemToRemove))
                                    {
                                        tempDataItems.Remove(itemToRemove);
                                        --TempCurrentListSize;
                                        TempNewListSize = TempCurrentListSize;
                                    }
                                }
                            }

                            break;
                        }

                    case EventType.MouseDrag:
                        {
                            GameObject existingDragData = DragAndDrop.GetGenericData(DRAG_DATA_KEY) as GameObject;

                            if (existingDragData != null)
                            {
                                DragAndDrop.StartDrag("Dragging List ELement");
                                Event.current.Use();
                            }

                            break;
                        }
                }
            }
        }
        
        void DrawControls(List<InspectorItem> controlItems)
        {
            // Draw all of the items.
            foreach (InspectorItem item in controlItems)
            {
                EditorGUILayout.Space();
                DrawControlRect(item);
            }
        }

        void DrawControlRect(InspectorItem item)
        {
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.textColor = Color.white;

            Rect rect = EditorGUILayout.GetControlRect(false, 18.0f);
            GUI.Box(rect, item.ControlItem ? item.ControlItem.name : "", boxStyle);

            item.ControlRect = rect;
        }
    }
}