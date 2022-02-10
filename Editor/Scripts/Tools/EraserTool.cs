﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;

namespace Packages.PrefabshopEditor
{
	[ToolColor(ToolColorAttribute.ToolUseType.Remove)]
	[ToolKeyCodeAttribute(KeyCode.C)]
	public class EraserTool : Tool
	{
		public bool byPrefabSet;
		Mesh shape;
		Material drawMat;

		public EraserTool() : base()
		{
			var type = this.GetType();

			AddParameter(new SelectToolBar(type));
			AddParameter(new PrefabsSet(type));
			AddParameter(new Radius(type));
			AddParameter(new Tag(type));
			AddParameter(new Layer(type));
			AddParameter(new Ignore(type));
			AddParameter(new CachedGameObjects(type));
			AddParameter(new Mask(type));

			GetParameter<SelectToolBar>().toolBar = new string[] { "By PrefabsSet", "All Prefabs" };
			GetParameter<SelectToolBar>().onChangeToolBar += OnChangeToolBar;
			OnChangeToolBar(GetParameter<SelectToolBar>().idSelect);
			GetParameter<PrefabsSet>().Activate();
		}

		public void OnChangeToolBar(int id)
		{
			switch (id)
			{
				case 0:
					byPrefabSet = true;
					GetParameter<PrefabsSet>().Enable = true;
					break;
				case 1:
					byPrefabSet = false;
					GetParameter<PrefabsSet>().Enable = false;
					break;
			}
		}

		protected override void DrawTool(Ray ray)
		{
			base.DrawTool(ray);
			var casts = Physics.RaycastAll(ray, Mathf.Infinity, ~(GetParameter<Ignore>().layer));
			if (casts.Length > 0)
			{
				var raycastHit = casts[casts.Length - 1];

				if (shape == null)
				{
					var primitiveGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					shape = primitiveGo.GetComponent<MeshFilter>().sharedMesh;
					primitiveGo.hideFlags = HideFlags.DontSave | HideFlags.NotEditable | HideFlags.HideInHierarchy;
					GameObject.DestroyImmediate(primitiveGo);
				}

				Matrix4x4 matrix = new Matrix4x4();
				matrix.SetTRS(raycastHit.point, Quaternion.identity, Vector3.one * GetParameter<Radius>().value * 2);
				if (drawMat == null)
				{
					drawMat = new Material(Shader.Find("Raptorij/BrushShapeZ"));
				}
				drawMat.SetColor("_Color", toolColor);
				drawMat.SetPass(0);
				Graphics.DrawMeshNow(shape, matrix, 0);

				Handles.color = Color.white;
				Handles.DrawWireDisc(raycastHit.point, raycastHit.normal, GetParameter<Radius>().value);
			}
		}

		public override void Paint(RaycastHit drawPointHit)
		{
			base.Paint(drawPointHit);
			var transformArray = GameObject.FindObjectsOfType<GameObject>()
								.Where(t => Vector3.Distance(t.transform.position, drawPointHit.point) < GetParameter<Radius>().value)
								.ToArray();
			List<GameObject> onlyPrefabs = new List<GameObject>();
			foreach (var coll in transformArray)
			{
				if (PrefabUtility.GetPrefabInstanceHandle(coll.gameObject) != null)
				{
					var prefab = PrefabUtility.GetOutermostPrefabInstanceRoot(coll.gameObject);
					if (!onlyPrefabs.Contains(prefab))
					{
						onlyPrefabs.Add(prefab);
					}
				}
			}
			foreach (var go in onlyPrefabs)
			{
				//var prefabInstance = PrefabUtility.GetPrefabInstanceHandle(go);
				GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(go) as GameObject;
				if (byPrefabSet)
				{
					if (GetParameter<PrefabsSet>().selectedPrefabs.Contains(prefabAsset))
					{
						AddToCache(go);
					}
				}
				else
				{
					AddToCache(go);
				}
			}
			onlyPrefabs.Clear();
		}

		private void AddToCache(GameObject go)
		{
			if (GetParameter<Mask>().HaveMask)
			{
				if (GetParameter<Mask>().CheckPoint(go.transform.position))
				{
					go.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
					go.SetActive(false);
					GetParameter<CachedGameObjects>().AddToСache(go);
				}
			}
			else
			{
				go.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
				go.SetActive(false);
				GetParameter<CachedGameObjects>().AddToСache(go);
			}
		}

		protected override void OnEndPaint(RaycastHit endPointHit)
		{
			base.OnEndPaint(endPointHit);
			EditorApplication.update += RemoveCechedObjects;
		}

		void RemoveCechedObjects()
		{
			var cached = GetParameter<CachedGameObjects>().gameObjects;
			for (int i = 0; i < cached.Count; i++)
			{
				if (cached[i] == null)
				{
					cached.RemoveAt(i);
					i--;
				}
			}
			for (int i = 0; i < cached.Count; i++)
			{
				var go = cached[0];
				if (go != null)
				{
					go.hideFlags = HideFlags.None;
					go.SetActive(true);
					Undo.DestroyObjectImmediate(go);
				}
				cached.RemoveAt(0);
			}
			if (cached.Count <= 0)
			{
				EditorApplication.update -= RemoveCechedObjects;
			}
		}
	}
}