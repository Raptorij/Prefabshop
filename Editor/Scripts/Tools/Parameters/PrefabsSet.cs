﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    public class PrefabsSet : Parameter
    {
        public PrefabSetInfo setInfo;
        public bool needHaveSelection = true;

        public List<GameObject> setPrefabs = new List<GameObject>();
        public List<GameObject> selectedPrefabs = new List<GameObject>();
        private int previousSelectId;

        private Texture2D blueTexture;
        private Texture2D greyTexture;
        private Texture2D yellowTexture;
        private Texture2D lockTexture;
        private float prefabListHeight;

        private bool selectSet;
        private UnityEngine.Object pickedObj;

        public PrefabsSet(Type toolType) : base(toolType)
        {
            string path = EditorPrefs.GetString("[Prefabshop] PrefabsSetPath");

            if (path != "")
            {
                setInfo = AssetDatabase.LoadAssetAtPath(path, typeof(PrefabSetInfo)) as PrefabSetInfo;
                if (setInfo != null)
                {
                    var prefs = setInfo.brushObjects.ToArray();
                    setPrefabs = prefs.ToList();
                }
            }
        }

        public void Activate()
        {
            blueTexture = new Texture2D(64, 64);
            greyTexture = new Texture2D(64, 64);
            yellowTexture = new Texture2D(64, 64);
            lockTexture = new Texture2D(64, 64);

            for (int y = 0; y < blueTexture.height; y++)
            {
                for (int x = 0; x < blueTexture.width; x++)
                {
                    blueTexture.SetPixel(x, y, new Color(0f, 0.8f, 1f));
                    greyTexture.SetPixel(x, y, new Color(0.125f, 0.125f, 0.125f));
                    yellowTexture.SetPixel(x, y, new Color(0.75f, 0.75f, 0));
                    var lockColor = EditorGUIUtility.isProSkin ? new Color(0, 0, 0, .5f) : new Color(1, 1, 1, .5f);
                    lockTexture.SetPixel(x, y, lockColor);
                }
            }

            blueTexture.Apply();
            greyTexture.Apply();
            yellowTexture.Apply();
            lockTexture.Apply();
        }

        public List<GameObject> GetSelectedPrefabs()
        {
            if (selectedPrefabs.Count == 0 && needHaveSelection)
            {
                EditorWindow.GetWindow<SceneView>().ShowNotification(new GUIContent("Need select some prefabs"));
            }
            return selectedPrefabs;
        }

        public override void DrawParameterGUI()
        {
            base.DrawParameterGUI();
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Load PrefabsSetInfo"))
                {                    
                    EditorGUIUtility.ShowObjectPicker<PrefabSetInfo>(setInfo, false, "", 0);
                }
                if (Event.current.commandName == "ObjectSelectorUpdated")
                {
                    // do a thing relating to the object picker
                    pickedObj = EditorGUIUtility.GetObjectPickerObject();
                    if (pickedObj != null)
                    {
                        setInfo = EditorGUIUtility.GetObjectPickerObject() as PrefabSetInfo;
                        if (setInfo != null)
                        {
                            var prefs = setInfo.brushObjects.ToArray();
                            setPrefabs = prefs.ToList();
                            EditorPrefs.SetString("[Prefabshop] PrefabsSetPath", AssetDatabase.GetAssetPath(setInfo));
                            EditorWindow.GetWindow<Prefabshop>().Repaint();
                        }
                    }
                    else
                    {
                        selectSet = false;
                    }
                }
                GUI.enabled = false;
                if (GUILayout.Button("Save as New..."))
                {
                    //setInfo.brushObjects = setPrefabs;

                }
                GUI.enabled = setInfo != null;
                if (setInfo != null)
                {
                    GUI.backgroundColor = CheckArrays(setInfo.brushObjects.ToArray(), setPrefabs.ToArray()) ? Color.white : Color.yellow;
                }
                else
                {
                    GUI.backgroundColor = Color.white;
                }
                if (GUILayout.Button("Save Changes"))
                {
                    var prefs = setPrefabs.ToArray();
                    setInfo.brushObjects = prefs.ToList();
                    EditorUtility.SetDirty(setInfo);
                    AssetDatabase.SaveAssets();
                }
                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();

            if (setInfo != null)
            {
                GUI.enabled = false;
                EditorGUILayout.ObjectField("Prefabs Set:", setInfo, typeof(PrefabSetInfo), false);
                GUI.enabled = true;
                DisplayPrefabs(setPrefabs);
            }
            else
            {
                DisplayPrefabs(setPrefabs);
            }
            GUILayout.Space(55f + prefabListHeight);
        }

        public void DisplayPrefabs(List<GameObject> prefabs)
        {
            int numberOfPrefabs = prefabs.Count;
            int windowWidth = (int)EditorGUIUtility.currentViewWidth;

            int y = (int)(GUILayoutUtility.GetLastRect().y + EditorGUIUtility.singleLineHeight);
            for (int i = 0; i < numberOfPrefabs; i++)
            {
                var e = Event.current;
                var go = prefabs[i];
                int columns = Mathf.FloorToInt(windowWidth / (50 + 20) + 1);
                int rows = Mathf.FloorToInt(numberOfPrefabs / columns);
                prefabListHeight = rows * 50f;
                int posX = 5 + 50 * (i - (Mathf.FloorToInt(i / columns)) * columns);
                int posY = y + 50 * Mathf.FloorToInt(i / columns);

                Rect r = new Rect(posX, posY, 50, 50);
                Rect border = new Rect(r.x + 2, r.y + 6, r.width - 4, r.height - 4);

                Rect removeRect = new Rect(r.x + r.width - 12, r.y + 6, 10, 10);

                bool onRemoveButton = removeRect.Contains(Event.current.mousePosition);


                if (onRemoveButton && e.type == EventType.MouseDown && e.button == 0)
                {
                    if (selectedPrefabs.Contains(prefabs[i]))
                    {
                        selectedPrefabs.Remove(prefabs[i]);
                    }
                    setPrefabs.Remove(prefabs[i]);
                    EditorWindow.GetWindow<Prefabshop>().Repaint();
                    return;
                }


                if (r.Contains(Event.current.mousePosition) && e.type == EventType.MouseDown && e.button == 0 && !onRemoveButton)
                {
                    if (e.control)
                    {
                        if (selectedPrefabs.Contains(prefabs[i]))
                        {
                            previousSelectId = 0;
                            selectedPrefabs.Remove(prefabs[i]);
                            EditorWindow.GetWindow<Prefabshop>().Repaint();
                            return;
                        }
                        else
                        {
                            if (!selectedPrefabs.Contains(prefabs[i]))
                            {
                                selectedPrefabs.Add(prefabs[i]);
                            }
                            EditorWindow.GetWindow<Prefabshop>().Repaint();
                        }
                    }
                    else if (e.shift)
                    {
                        if (previousSelectId != i)
                        {
                            int side = i > previousSelectId ? 1 : -1;
                            for (int j = 0; j <= Mathf.Abs(i - previousSelectId); j++)
                            {
                                if (!selectedPrefabs.Contains(prefabs[j * side + previousSelectId]))
                                {
                                    selectedPrefabs.Add(prefabs[j * side + previousSelectId]);
                                    EditorWindow.GetWindow<Prefabshop>().Repaint();
                                }
                            }
                        }
                        else
                        {
                            selectedPrefabs.Remove(prefabs[i]);
                            EditorWindow.GetWindow<Prefabshop>().Repaint();
                        }
                    }
                    else
                    {
                        previousSelectId = i;
                        selectedPrefabs.Clear();
                        selectedPrefabs.Add(prefabs[i]);
                        EditorWindow.GetWindow<Prefabshop>().Repaint();
                        return;
                    }
                }

                if (setInfo == null)
                {
                    return;
                }

                if (selectedPrefabs.Contains(prefabs[i]))
                {
                    EditorGUI.DrawPreviewTexture(border, blueTexture, null, ScaleMode.ScaleToFit, 0f);
                }
                else if (!setInfo.brushObjects.Contains(prefabs[i]))
                {
                    EditorGUI.DrawPreviewTexture(border, yellowTexture, null, ScaleMode.ScaleToFit, 0f);
                }
                else
                {
                    EditorGUI.DrawPreviewTexture(border, greyTexture, null, ScaleMode.ScaleToFit, 0f);
                }

                border.x += 2;
                border.y += 2;
                border.width -= 4;
                border.height -= 4;

                var preview = AssetPreview.GetAssetPreview(go);

                if (preview != null)
                {
                    EditorGUI.DrawPreviewTexture(border, preview, null, ScaleMode.ScaleToFit, 0f);
                }
                Color removeButton = new Color(.75f, .125f, .125f, 1);
                EditorGUI.DrawRect(removeRect, removeButton);
                removeRect.y -= 4;
                GUI.Label(removeRect, "-");
                if (!Enable)
                {
                    border.x -= 2;
                    border.y -= 2;
                    border.width += 4;
                    border.height += 4;
                    Color guiColor = GUI.color; // Save the current GUI color
                    GUI.color = Color.clear; // This does the magic
                    EditorGUI.DrawTextureTransparent(border, lockTexture, ScaleMode.ScaleToFit, 0f);
                    GUI.color = guiColor; // Get back to previous GUI color
                }
            }

            int c = Mathf.FloorToInt(windowWidth / (50 + 20) + 1);
            int xRect = 5 + 50 * (numberOfPrefabs - (Mathf.FloorToInt(numberOfPrefabs / c)) * c);
            int yRect = y + 50 * Mathf.FloorToInt(numberOfPrefabs / c);

            DropAreaGUI(new Rect(xRect + 2, yRect + 6, 46, 46));
        }

        public void DropAreaGUI(Rect r)
        {
            var e = Event.current;
            var dropArea = r;
            GUI.Box(dropArea, string.Empty);
            GUI.Label(dropArea, "+", EditorStyles.centeredGreyMiniLabel);
            if (!Enable)
            {
                Color guiColor = GUI.color; // Save the current GUI color
                GUI.color = Color.clear; // This does the magic
                EditorGUI.DrawTextureTransparent(dropArea, lockTexture, ScaleMode.ScaleToFit, 0f);
                GUI.color = guiColor; // Get back to previous GUI color
            }

            switch (e.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(e.mousePosition))
                    {
                        return;
                    }

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (e.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (var dragged_object in DragAndDrop.objectReferences)
                        {
                            if (dragged_object is GameObject)
                            {
                                GameObject go = (GameObject)dragged_object;
                                if (setPrefabs.Contains(go))
                                {
                                    Debug.Log($"<color=yellow>[Prefabshop] </color>Prefab {go.name} already exist in current PrefabsSetInfo.");
                                }
                                else
                                {
                                    setPrefabs.Add(go);
                                }
                            }
                        }
                    }
                    break;
            }
        }

        bool CheckArrays(GameObject[] array1, GameObject[] array2)
        {
            if (array1.Length != array2.Length)
            {
                return false;
            }
            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}