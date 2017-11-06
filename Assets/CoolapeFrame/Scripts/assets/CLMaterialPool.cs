/*
******************************************************************************** 
  *Copyright(C),coolae.net 
  *Author:  chenbin
  *Version:  2.0 
  *Date:  2017-01-09
  *Description:   材质球对象池
  *Others:  
  *History:
*********************************************************************************
*/ 

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Coolape
{
	public class CLMaterialPool:CLAssetsPoolBase<Material>
	{
		public static CLMaterialPool pool = new CLMaterialPool ();

		public override string getAssetPath (string name)
		{
			string path = PStr.b ().a (CLPathCfg.self.basePath).a ("/")
				.a (CLPathCfg.upgradeRes).a ("/other/Materials").e ();
			return wrapPath (path, name);
		}

		public override Material _borrowObj (string name)
		{
			ArrayList propNames = null;
			ArrayList texNames = null;
			ArrayList texPaths = null;

			if (getMaterialTexCfg (name, ref propNames, ref texNames, ref texPaths)) {
				if (texNames != null) {
					for (int i = 0; i < texNames.Count; i++) {
						CLTexturePool.borrowObj (texNames [i].ToString ());
					}
				}
			}
			return base._borrowObj (name, true);
		}

		public static bool havePrefab (string name)
		{
			return pool._havePrefab (name);
		}

		public static void clean ()
		{
			pool._clean ();
		}

		public static void setPrefab (string name, object finishCallback)
		{
			setPrefab (name, finishCallback, null, null);
		}

		public static void setPrefab (string name, object finishCallback, object args)
		{
			pool._setPrefab (name, finishCallback, args, null);
		}

		public static void setPrefab (string name, object finishCallback, object args, object progressCB)
		{
			pool._setPrefab (name, finishCallback, args, progressCB);
		}

		public static Material borrowObj (string name)
		{
			return pool._borrowObj (name, true);
		}

		public static void borrowObjAsyn (string name, object onGetCallbak)
		{
			borrowObjAsyn (name, onGetCallbak, null);
		}

		public static void borrowObjAsyn (string name, object onGetCallbak, object orgs)
		{
			pool._borrowObjAsyn (name, onGetCallbak, orgs, null);
		}

		public static void borrowObjAsyn (string name, object onGetCallbak, object orgs, object progressCB)
		{
			pool._borrowObjAsyn (name, onGetCallbak, orgs, progressCB);
		}

		public static void returnObj (string name)
		{
			//return texture
			ArrayList propNames = null;
			ArrayList texNames = null;
			ArrayList texPaths = null;
			if (getMaterialTexCfg (name, ref propNames, ref texNames, ref texPaths)) {
				if (texNames != null) {
					for (int i = 0; i < texNames.Count; i++) {				
						CLTexturePool.returnObj (texNames [i].ToString ());
					}
				}
			}
			// Then return material
			pool._returnObj (name, null);
		}

		public static void cleanTexRef (string name, Material mat)
		{
			ArrayList propNames = null;
			ArrayList texNames = null;
			ArrayList texPaths = null;
			if (CLMaterialPool.getMaterialTexCfg (name, ref propNames, ref texNames, ref texPaths)) {
				if (propNames != null) {
					for (int i = 0; i < propNames.Count; i++) {
						mat.SetTexture (propNames [i].ToString (), null);
					}
				}
			}
		}

		static int texCount = 0;

		public override void sepcProc4Assets (Material mat, object cb, object args, object progressCB)
		{
			#if UNITY_EDITOR
			if (mat != null) {
				mat.shader = Shader.Find (mat.shader.name);
			}
			#endif
			resetTexRef (mat.name, mat, cb, args);
		}

		public static void resetTexRef (string matName, Material mat, object cb, object args)
		{
			texCount = 0;
			ArrayList propNames = null;
			ArrayList texNames = null;
			ArrayList texPaths = null;
			if (getMaterialTexCfg (matName, ref propNames, ref texNames, ref texPaths)) {
				if (propNames != null) {
					ArrayList list = null;
					//取得texture
					int count = propNames.Count;
					for (int i = 0; i < count; i++) {
						list = new ArrayList ();
						list.Add (mat);
						list.Add (propNames [i]);
						list.Add (count);
						list.Add (cb);
						list.Add (args);
						#if UNITY_EDITOR
						if (!CLCfgBase.self.isEditMode || Application.isPlaying) {
//							CLTexturePool.borrowTextureAsyn (texNames [i].ToString (), (Callback)onGetTexture, list);
							CLTexturePool.setPrefab (texNames [i].ToString (), (Callback)onGetTexture, list, null);
						} else {
							string tmpPath = "Assets/" + texPaths [i];
							Texture tex = AssetDatabase.LoadAssetAtPath (
								              tmpPath, typeof(UnityEngine.Object)) as Texture;
							onGetTexture (tex, list);
						}
						#else
//						CLTexturePool.borrowTextureAsyn (texNames [i].ToString (), (Callback)onGetTexture, list);
						CLTexturePool.setPrefab(texNames [i].ToString (), (Callback)onGetTexture, list);
						#endif
					}
				} else {
					pool.finishSetPrefab (mat);
					Utl.doCallback (cb, mat, args);
				}
			} else {
				pool.finishSetPrefab (mat);
				Utl.doCallback (cb, mat, args);
			}
		}

		public static void onGetTexture (params object[] paras)
		{
			string name = "";
			try {
				texCount++;
//				name = paras [0].ToString ();
				Texture tex = paras [0] as Texture;
				name = tex.name;
				ArrayList list = paras [1] as ArrayList;
				Material mat = list [0] as Material;
				string propName = list [1].ToString ();
				int count = (int)(list [2]);

				// 设置material对应属性的texture
				mat.SetTexture (propName, tex);

				if (texCount >= count) {
					pool.finishSetPrefab (mat);
					//finished
					Callback cb = list [3] as Callback;
					object agrs = list [4];
					Utl.doCallback (cb, mat, agrs);
				}
				list.Clear ();
				list = null;
			} catch (System.Exception e) {
				Debug.LogError ("name==========" + name + "==" + e);
			}
		}

		//==========================================================
		// material cfg proc
		//==========================================================
		static Hashtable _materialTexRefCfg = null;

		public static Hashtable materialTexRefCfg {
			get {
				if (_materialTexRefCfg == null) {
					_materialTexRefCfg = readMaterialTexRefCfg ();
				}
				return _materialTexRefCfg;
			}
			set {
				_materialTexRefCfg = value;
			}
		}
		#if UNITY_EDITOR
		public static string materialTexRefCfgPath = PStr.b ().a (Application.dataPath).a ("/").a (CLPathCfg.self.basePath).a ("/upgradeRes4Dev/priority/cfg/materialTexRef.cfg").e ();
		#else
		public static string materialTexRefCfgPath = PStr.b().a(CLPathCfg.self.basePath).a("/upgradeRes/priority/cfg/materialTexRef.cfg").e();
		#endif

		/// <summary>
		/// Gets the material cfg.取得material引用图片的配置
		/// </summary>
		/// <returns><c>true</c>, if material cfg was gotten, <c>false</c> otherwise.</returns>
		/// <param name="matName">Mat name.</param>
		/// <param name="propNames">Property names.</param>
		/// <param name="texNames">Tex names.</param>
		/// <param name="texPaths">Tex paths.</param>
		public static bool getMaterialTexCfg (string matName, ref ArrayList propNames, ref ArrayList texNames, ref ArrayList texPaths)
		{
			Hashtable cfg = MapEx.getMap (materialTexRefCfg, matName);
			bool ret = true;
			if (cfg == null) {
				ret = false;
			} else {
				propNames = cfg ["pp"] as ArrayList;
				texNames = cfg ["tn"] as ArrayList;
				texPaths = cfg ["tp"] as ArrayList;
			}
			return ret;
		}

		public static Hashtable readMaterialTexRefCfg ()
		{
			Hashtable ret = null;
			#if UNITY_EDITOR
			byte[] buffer = File.Exists (materialTexRefCfgPath) ? File.ReadAllBytes (materialTexRefCfgPath) : null;
			#else
			byte[] buffer = FileEx.readNewAllBytes (materialTexRefCfgPath);
			#endif
			if (buffer != null) {
				MemoryStream ms = new MemoryStream ();
				ms.Write (buffer, 0, buffer.Length);
				ms.Position = 0;
				object obj = B2InputStream.readObject (ms);
				if (obj != null) {
					ret = obj as Hashtable;
				}
			}
			ret = ret == null ? new Hashtable () : ret;
			return ret;
		}
	}
}
