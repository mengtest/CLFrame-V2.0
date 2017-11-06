﻿using UnityEditor;
using UnityEngine;
using System.Collections;
using Coolape;
using System.Collections.Generic;
using System.IO;
using UnityEditorHelper;

[CustomEditor (typeof(CLSharedAssets), true)]
public class CLSharedAssetsInspector : Editor
{
	CLSharedAssets instance;
	bool state1 = true;
	bool state2 = true;
	string tabName = "";
	static Hashtable MaterialMap = new Hashtable ();
	static Hashtable MeshMap = new Hashtable ();

	void OnEnable ()
	{
		instance = target as CLSharedAssets;
		EditorPrefs.SetBool("CLS0", EventDelegate.IsValid(instance.onFinshLoad));
	}

	public override void OnInspectorGUI ()
	{
		instance = target as CLSharedAssets;
		ECLEditorUtl.BeginContents (); 
		{
			GUI.color = Color.yellow;
			instance.isDonnotResetAssets = EditorGUILayout.Toggle ("is Donnot Reset Assets", instance.isDonnotResetAssets);
			instance.isSkipModel = EditorGUILayout.Toggle ("is Skip Manage Model", instance.isSkipModel);
			GUI.color = Color.white;
		}
		ECLEditorUtl.EndContents ();

		if (state1) {
			tabName = "Click Close Materials";
		} else {
			tabName = "Click Open Materials";
		}
		using (new FoldableBlock (ref state1, tabName, null)) {
			if (state1) {
				for (int i = 0; i < instance.materials.Count; i++) {
					ECLEditorUtl.BeginContents (); 
					{
						GUILayout.BeginHorizontal ();
						{
							EditorGUILayout.ObjectField ("Render", (Object)(instance.materials [i].render), typeof(Renderer), true);
							GUI.color = Color.red;
							if (GUILayout.Button ("Delete", GUILayout.Width (60))) {
								if (EditorUtility.DisplayDialog ("Alert", "Really want to delete?", "Okay", "Cancel")) {
									instance.materials.RemoveAt (i);
									break;
								}
							}
							GUI.color = Color.white;
						}
						GUILayout.EndHorizontal ();
//				EditorGUILayout.IntField ("Index", instance.materials [i].index);
						EditorGUILayout.TextField ("Material Name", instance.materials [i].materialName);
						EditorGUILayout.ObjectField ("Material", (Object)(getMaterial (instance.materials [i].materialName)), typeof(Material), true);
					}
					ECLEditorUtl.EndContents ();
				}
			}
		}


		if (state2) {
			tabName = "Click Close Meshs";
		} else {
			tabName = "Click Open Meshs";
		}
		using (new FoldableBlock (ref state2, tabName, null)) {
			if (state2) {
				for (int i = 0; i < instance.meshs.Count; i++) {
					ECLEditorUtl.BeginContents (); 
					{
						GUILayout.BeginHorizontal ();
						{
							if (instance.meshs [i].meshFilter != null) {
								EditorGUILayout.ObjectField ("Mesh Fiter", (Object)(instance.meshs [i].meshFilter), typeof(MeshFilter), true);
							} else {
								EditorGUILayout.ObjectField ("Skinned Mesh", (Object)(instance.meshs [i].skinnedMesh), typeof(SkinnedMeshRenderer), true);
							}
							GUI.color = Color.red;
							if (GUILayout.Button ("Delete", GUILayout.Width (60))) {
								if (EditorUtility.DisplayDialog ("Alert", "Really want to delete?", "Okay", "Cancel")) {
									instance.meshs.RemoveAt (i);
									break;
								}
							}
							GUI.color = Color.white;
						}
						GUILayout.EndHorizontal ();
						EditorGUILayout.TextField ("Model Name", instance.meshs [i].modelName);
						EditorGUILayout.ObjectField ("Mesh", (Object)(getMesh (instance.meshs [i].modelName)), typeof(Mesh), true);
					}
					ECLEditorUtl.EndContents ();
				}
			}
		}

		GUILayout.BeginHorizontal ();
		{
			if (GUILayout.Button ("Get")) {
				getAssets (instance, instance.transform);
				EditorUtility.SetDirty (instance);
				EditorUtility.DisplayDialog ("Success", "Finish get Assets", "Okay");
			}

			if (GUILayout.Button ("Clean")) {
				cleanMaterialInfor ();
				EditorUtility.DisplayDialog ("Success", "Finish clean Assets", "Okay");
			}

			if (GUILayout.Button ("Reset")) {
				instance.reset ();
				instance.resetAssets ();
				EditorUtility.DisplayDialog ("Success", "Finish reset Assets", "Okay");
			}
		}
		GUILayout.EndHorizontal ();

//		base.OnInspectorGUI ();

		GUILayout.Space(3f);
		NGUIEditorTools.SetLabelWidth(80f);
		bool minimalistic = NGUISettings.minimalisticLook;
		DrawEvents("CLS0", "On Finish Load", instance.onFinshLoad, minimalistic);
	}


	void DrawEvents (string key, string text, List<EventDelegate> list, bool minimalistic)
	{
		if (!NGUIEditorTools.DrawHeader(text, key, false, minimalistic)) return;
		NGUIEditorTools.BeginContents();
		EventDelegateEditor.Field(instance, list, null, null, minimalistic);
		NGUIEditorTools.EndContents();
	}

	public Mesh getMesh (string path)
	{
		if (MeshMap [path] == null) {
			string matPath = PStr.b ().a (CLPathCfg.self.basePath).a ("/")
				.a ("upgradeRes4Dev").a ("/other/model/").a (path.Replace (".", "/")).a (".FBX").e ();
			Debug.Log (matPath);
			MeshMap [path] = ECLEditorUtl.getObjectByPath (matPath);
		}
		GameObject mi = MeshMap [path] as GameObject;
		if (mi != null) {
			MeshFilter mf = mi.GetComponentInChildren<MeshFilter>();
			if (mf != null) {
				return mf.sharedMesh;
			} else {
				SkinnedMeshRenderer smr = mi.GetComponentInChildren<SkinnedMeshRenderer>();
				if (smr != null) {
					return smr.sharedMesh;
				}
			}
		}
		return null;
	}

	public Object getMaterial (string path)
	{
		if (MaterialMap [path] == null) {
			string matPath = PStr.b ().a (CLPathCfg.self.basePath).a ("/")
			.a ("upgradeRes4Dev").a ("/other/Materials/").a (path.Replace (".", "/")).a (".mat").e ();
			Debug.Log (matPath);
			Object obj = ECLEditorUtl.getObjectByPath (matPath);
			MaterialMap [path] = obj;
			return obj;
		} else {
			return (Object)(MaterialMap [path]);
		}
	}

	public void cleanMaterialInfor ()
	{
		instance.cleanRefAssets ();
	}

	public static bool getAssets (CLSharedAssets sharedAsset, Transform tr)
	{
		bool ret1 = false;
		//fbx
		sharedAsset.meshs.Clear ();
		if (!sharedAsset.isSkipModel) {
			ret1 = getMeshRef (sharedAsset, tr);
		}

		//sound

		//material
		sharedAsset.materials.Clear ();
		return getMaterialRef (sharedAsset, tr) || ret1;
	}

	/// <summary>
	/// Ises the can add shared asset proc. 判断是需要共享资源的处理
	/// </summary>
	/// <returns><c>true</c>, if can add shared asset proc was ised, <c>false</c> otherwise.</returns>
	/// <param name="go">Go.</param>
	public static bool isCanAddSharedAssetProc (GameObject go)
	{
		Object[] res = EditorUtility.CollectDependencies (new Object[] { go });
		if (res == null || res.Length == 0) {
			return false;
		} else {
			for (int i = 0; i < res.Length; i++) {
				// 如何只引用了ui相关的图集与font也认为是没有引用资源
				if (res [i].name != "EmptyAtlas" &&
				    res [i].name != "EmptyFont" &&
				    res [i].name != "atlasAllReal") {
					return true;
				}
			}
			return false;
		}
	}

	/// <summary>
	/// Gets the material reference.
	/// </summary>
	/// <returns><c>true</c>, 说明有新资源移动到开发目录 <c>false</c> otherwise.</returns>
	/// <param name="sharedAsset">Shared asset.</param>
	/// <param name="tr">Tr.</param>
	public static bool getMaterialRef (CLSharedAssets sharedAsset, Transform tr)
	{
		bool ret = false;
		bool ret1 = false;
		bool ret2 = false;
		bool ret3 = false;
		Renderer[] rds = tr.GetComponents<Renderer> ();
		Renderer rd = null;
		for (int r = 0; r < rds.Length; r++) {
			rd = rds [r];
			if (rd == null)
				continue;
			if (rd.sharedMaterials != null && rd.sharedMaterials.Length > 0) {
				for (int i = 0; i < rd.sharedMaterials.Length; i++) {
					if (rd.sharedMaterials [i] == null)
						continue;
					Coolape.CLSharedAssets.CLMaterialInfor clMat = new Coolape.CLSharedAssets.CLMaterialInfor ();
					clMat.render = rd;
					clMat.index = i;
					ret1 = ECLEditorUtl.moveAsset4Upgrade (rd.sharedMaterials [i]) || ret1 ? true : false;
					string materialName = ECLEditorUtl.getAssetName4Upgrade (rd.sharedMaterials [i]);
					clMat.materialName = materialName;
					sharedAsset.materials.Add (clMat);

					// save to cfg file
					ArrayList propNames = new ArrayList ();
					ArrayList texNames = new ArrayList ();
					ArrayList texPaths = new ArrayList ();
					ret2 = ECLEditorUtl.getTexturesFromMaterial (rd.sharedMaterials [i], ref propNames, ref texNames, ref texPaths) || ret2 ? true : false;
					saveMaterialTexCfg (materialName, propNames, texNames, texPaths);
				}
			}

		}

		for (int i = 0; i < tr.childCount; i++) {
			ret3 = getMaterialRef (sharedAsset, tr.GetChild (i)) || ret3 ? true : false;
		}
		return ret1 || ret2 || ret3;
	}


	public static bool getMeshRef (CLSharedAssets sharedAsset, Transform tr)
	{
		bool ret = false;
		bool ret1 = false;
		bool ret2 = false;
		bool ret3 = false;

		MeshFilter[] mfs = tr.GetComponents<MeshFilter> ();
		Mesh mesh = null;
		for (int i = 0; i < mfs.Length; i++) {
			mesh = (mfs [i]).sharedMesh;
			if (mesh != null) {
				Coolape.CLSharedAssets.CLMeshInfor clMesh = new Coolape.CLSharedAssets.CLMeshInfor ();
				clMesh.meshFilter = mfs [i];
				ret1 = ECLEditorUtl.moveAsset4Upgrade (mfs [i].sharedMesh) || ret1 ? true : false;
				string modelName = ECLEditorUtl.getAssetName4Upgrade (mfs [i].sharedMesh);
				setModelProp (modelName);
				clMesh.modelName = modelName;
				sharedAsset.meshs.Add (clMesh);
			}
		}

		SkinnedMeshRenderer[] smrs = tr.GetComponents<SkinnedMeshRenderer> ();
		for (int i = 0; i < smrs.Length; i++) {
			mesh = (smrs [i]).sharedMesh;
			if (mesh != null) {
				Coolape.CLSharedAssets.CLMeshInfor clMesh = new Coolape.CLSharedAssets.CLMeshInfor ();
				clMesh.skinnedMesh = smrs [i];
				ret2 = ECLEditorUtl.moveAsset4Upgrade (smrs [i].sharedMesh) || ret2 ? true : false;
				string modelName = ECLEditorUtl.getAssetName4Upgrade (smrs [i].sharedMesh);
				setModelProp (modelName);
				clMesh.modelName = modelName;
				sharedAsset.meshs.Add (clMesh);
			}
		}

		for (int i = 0; i < tr.childCount; i++) {
			ret3 = getMeshRef (sharedAsset, tr.GetChild (i)) || ret3 ? true : false;
		}

		return ret1 || ret2 || ret3;
	}

	public static void setModelProp(string modelName) {
		string matPath = PStr.b ().a (CLPathCfg.self.basePath).a ("/")
			.a ("upgradeRes4Dev").a ("/other/model/").a (modelName.Replace (".", "/")).a (".FBX").e ();
		ModelImporter mi = ModelImporter.GetAtPath("Assets/"+matPath) as ModelImporter;
		if (mi != null) {
			mi.importMaterials = false;
			mi.isReadable = false;
			AssetDatabase.ImportAsset(matPath);
			AssetDatabase.Refresh();
		}
		GameObject go = ECLEditorUtl.getObjectByPath (matPath) as GameObject;
		if(go != null) {
			MeshRenderer mf = go.GetComponentInChildren<MeshRenderer> ();
			if (mf != null) {
				mf.sharedMaterial = null;
			}
			SkinnedMeshRenderer smr = go.GetComponentInChildren<SkinnedMeshRenderer> ();
			if (smr != null) {
				smr.sharedMaterial = null;
			}
			AssetDatabase.ImportAsset(matPath);
			AssetDatabase.Refresh();
		}
	}

	public static void saveMaterialTexCfg (string matName, ArrayList propNames, ArrayList texNames, ArrayList texPaths)
	{
		if (propNames == null || propNames.Count <= 0) {
			Debug.Log ("There is no textures");
			return;
		}
		Hashtable map = MapEx.getMap (CLMaterialPool.materialTexRefCfg, matName);
		if (map == null) {
			map = new Hashtable ();
		}
		map ["pp"] = propNames;
		map ["tn"] = texNames;
		map ["tp"] = texPaths;
		CLMaterialPool.materialTexRefCfg [matName] = map;

		MemoryStream ms = new MemoryStream ();
		B2OutputStream.writeObject (ms, CLMaterialPool.materialTexRefCfg);
		Directory.CreateDirectory (Path.GetDirectoryName (CLMaterialPool.materialTexRefCfgPath));
		File.WriteAllBytes (CLMaterialPool.materialTexRefCfgPath, ms.ToArray ());
	}
}
