/*
******************************************************************************** 
  *Copyright(C),coolae.net 
  *Author:  chenbin
  *Version:  2.0 
  *Date:  2017-01-09
  *Description:  热更新处理(处理部分移到lua里了)
  *Others:  
  *History:
*********************************************************************************
*/ 
using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace Coolape
{
	public class CLVerManager : CLBaseLua
	{
		//服务器
		public string baseUrl = "http://your hot fix url";
		[HideInInspector]
		public string platform = "";
		public static CLVerManager self;
		public static string resVer = "resVer";
		public static string versPath = "VerCtl";
		public const string fverVer = "VerCtl.ver";

		//========================
		//本地所有版本的版本信息
		public const string verPriority = "priority.ver";
		public Hashtable localPriorityVer = new Hashtable ();

		//本地优先更新资源
		public const string verOthers = "other.ver";
		public Hashtable otherResVerOld = new Hashtable ();

		//所有资源的版本管理
		public Hashtable otherResVerNew = new Hashtable ();
		//所有资源的版本管理

		public bool haveUpgrade = false;
		public bool is2GNetUpgrade = false;
		public bool is3GNetUpgrade = true;
		public bool is4GNetUpgrade = true;
		[HideInInspector]
		public string mVerverPath = "";
		[HideInInspector]
		public string mVerPrioriPath = "";
		[HideInInspector]
		public string mVerOtherPath = "";

		static string clientVersionPath {
			get {
				return CLPathCfg.persistentDataPath + "/ver.v";       //客户端版本
			}
		}

		public CLVerManager ()
		{
			self = this;
		}

		public string clientVersion {
			get {
				try {
					if (!File.Exists (clientVersionPath)) {
						return "";
					}
					return File.ReadAllText (clientVersionPath);
				} catch (System.Exception e) {
					return "";
				}
			}
			set {
				Directory.CreateDirectory (Path.GetDirectoryName (clientVersionPath));
				File.WriteAllText (clientVersionPath, value);
			}
		}
    
		// 用文件来表示是否已经处理完资源从包里释放出来的状态
		static string hadPoc {
			get {
				return CLPathCfg.persistentDataPath + "/resPoced";
			}
		}

		Callback onFinisInitStreaming;

		/// <summary>
		/// Inits the streaming assets packge.
		/// 将流目录中优先需要加载的资源集解压到可读写目录
		/// </summary>
		/// <param name="onFinisInitStreaming">On finis init streaming.</param>
		public void initStreamingAssetsPackge (Callback onFinisInitStreaming)
		{
			this.onFinisInitStreaming = onFinisInitStreaming;
			// clean cache
#if UNITY_EDITOR
			if (CLCfgBase.self.isEditMode) {
				onFinisInitStreaming ();
				return;
			}
#endif
			// 当版本不同时, clean cache
			if (string.Compare (Application.version, clientVersion) != 0) {
				string path = Application.persistentDataPath;
				// 先删掉目录下的文件
				string[] fEntries = Directory.GetFiles (path);
				foreach (string f in fEntries) {
					File.Delete (f);
				}
				// 再删掉所有文件夹
				string[] dirEntries = Directory.GetDirectories (path);
				foreach (string dir in dirEntries) {
					Directory.Delete (dir, true);
				}
				clientVersion = Application.version;
			}

			// 处理资源释放
			if (!File.Exists (hadPoc)) {
				string path = "priority.r";
				Callback cb = onGetStreamingAssets;
				StartCoroutine (FileEx.readNewAllBytesAsyn (path, cb));
			} else {
				onFinisInitStreaming ();
			}
		}
    
		// 取得"priority.r"
		void onGetStreamingAssets (params object[] para)
		{
			if (para != null && para.Length > 0) {
				byte[] buffer = (byte[])(para [0]);
				if (buffer != null) {
					MemoryStream ms = new MemoryStream ();
					ms.Write (buffer, 0, buffer.Length);
					ms.Position = 0;
					object obj = B2InputStream.readObject (ms);
					if (obj != null) {
						Hashtable map = (Hashtable)(obj);
						string path = "";
						foreach (DictionaryEntry cell in map) {
							try {
								path = CLPathCfg.persistentDataPath + "/" + cell.Key;
								Directory.CreateDirectory (Path.GetDirectoryName (path));
								File.WriteAllBytes (path, (byte[])(cell.Value));
							} catch (System.Exception e) {
								Debug.LogError (e);
							}
						}
						// 处理完包的资源释放，弄个标志
						Directory.CreateDirectory (Path.GetDirectoryName (hadPoc));
						File.WriteAllText (hadPoc, " ");
					}
				}
			}
			onFinisInitStreaming ();
		}

		public  Hashtable toMap (byte[] buffer)
		{
			Hashtable r = new Hashtable ();
			MemoryStream ms = new MemoryStream ();
			if (buffer != null) {
				ms.Write (buffer, 0, buffer.Length);
				ms.Position = 0;
				object obj = B2InputStream.readObject (ms);
				if (obj != null && obj is Hashtable) {
					r = (Hashtable)obj;
				} else {
					r = new Hashtable ();
				}
			} else {
				r = new Hashtable ();
			}
			return r;
		}

		public static Hashtable wwwMap = Hashtable.Synchronized (new Hashtable ());

		/// <summary>
		/// Gets the newest res.取得最新的资源
		/// </summary>
		/// <param name='path'>
		/// Path.资源的相对路径
		/// </param>
		/// <param name='type'>
		/// Type.资源的类型
		/// </param>
		/// <param name='onGetAsset'>
		/// On get asset.取得资源后的回调（回调信息中：
		/// 第一个参数是资源路径，
		/// 第二参数是资源的内容，
		/// 第三个参数传入的原样返回的参数)
		/// </param>
		/// <param name='originals'>
		/// Originals.原样返回的参数
		/// </param>
		public void getNewestRes4Lua (string path, CLAssetType type, object onGetAsset, object originals)
		{
			getNewestRes (path, type, onGetAsset, originals);
		}

		public void getNewestRes (string path, CLAssetType type, object onGetAsset, params object[] originals)
		{
			if (string.IsNullOrEmpty (path)) {
				return;
			}
			string verVal = "";
			if (!MapEx.getBool (wwwMap, path)) {
				bool needSave = false;
				wwwMap [path] = true;
				if (localPriorityVer [path] != null) {  //在优先资源里有
					needSave = false;
				} else {        //则可能在others里
					object obj1 = otherResVerOld [path];
					object obj2 = otherResVerNew [path];
					if (obj1 == null && obj2 != null) { //本地没有，最新有
						verVal = MapEx.getString (otherResVerNew, path);
						needSave = true;
					} else if (obj1 != null && obj2 != null) {
						if (obj1.ToString ().Equals(obj2.ToString ())) {//本地是最新的
							needSave = false;
						} else {    //本地不是最新的
							verVal = MapEx.getString (otherResVerNew, path);
							needSave = true;
						}
					} else if (obj1 != null && obj2 == null) {//本地有，最新没有
						needSave = false;
					} else {    //都没有找到
						needSave = false;
#if UNITY_EDITOR
//                  Debug.LogWarning ("Not found.path==" + path);
#endif
					}
				}
				string url = "";
				if (needSave) {
					if (!string.IsNullOrEmpty(verVal)) {
						url = PStr.begin ().a (baseUrl).a ("/").a (path).a (".").a (verVal).end ();
					} else {
						url = PStr.begin ().a (baseUrl).a ("/").a (path).end ();
					}
				} else {
					url = PStr.begin ().a (CLPathCfg.persistentDataPath).a ("/").a (path).end ();
					if (!File.Exists (url)) {
						url = System.IO.Path.Combine (Application.streamingAssetsPath, path);
						#if !UNITY_ANDROID || UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
						url = PStr.begin ().a ("file://").a (url).end ();
						#endif
					} else {
						url = PStr.begin ().a ("file://").a (url).end ();
					}
				}
				StartCoroutine (doGetContent (path, url, needSave, type, onGetAsset, originals));
			}
		}

		object addWWWcb;
		object rmWWWcb;

		public void setWWWListner (object addWWWcb, object rmWWWcb)
		{
			this.addWWWcb = addWWWcb;
			this.rmWWWcb = rmWWWcb;
		}

		public void addWWW (WWW www, string path, string url)
		{
			Utl.doCallback (addWWWcb, www, path, url);
		}

		public void rmWWW (string url)
		{
			Utl.doCallback (rmWWWcb, url);
		}

		public IEnumerator doGetContent (string path, string url, bool needSave, 
		                                 CLAssetType type, object onGetAsset, params object[] originals)
		{
			WWW www = new WWW (url);
			wwwMap [path] = true;
			if (needSave) {
				ArrayList list = new ArrayList ();
				list.Add (path);
				list.Add (onGetAsset);
				list.Add (originals);
				list.Add (url);
				//监听进度是否超时
				WWWEx.checkWWWTimeout (self, www, 4, 0, (Callback)onGetNewstResTimeOut, list);
				addWWW (www, path, url);
			}

			yield return www;
			WWWEx.uncheckWWWTimeout (self, www);
			object content = null;
			if (string.IsNullOrEmpty (www.error)) {
				switch (type) {
				case CLAssetType.text:
					content = www.text;
					break;
				case CLAssetType.bytes:
					content = www.bytes;
					break;
				case CLAssetType.texture:
					content = www.texture;
					break;
				case CLAssetType.assetBundle:
					content = www.assetBundle;
					break;
				}
            
				if (needSave && www.bytes != null) {
					saveNewRes (path, www.bytes);
				}
			} else {
//#if UNITY_EDITOR
				Debug.LogError ("Get content failed=" + path + "\n" + url);
//#endif
				content = null;
			}
            
			if (needSave) {
				rmWWW (url);
			}

			Utl.doCallback (onGetAsset, path, content, originals);
			if (www.assetBundle != null) {
				www.assetBundle.Unload (false);
			}

			www.Dispose ();
			www = null;
        
			wwwMap [path] = false;
		}

		public void onGetNewstResTimeOut (params object[] args)
		{
			if (args.Length < 2) {
				return;
			}
			ArrayList origs = (ArrayList)(args [1]);
			if (origs.Count < 3)
				return;
			string path = origs [0].ToString ();
			object onGetAsset = origs [1];
			object[] originals = (object[])(origs [2]);
			string url = origs [3].ToString ();
            
//			NAlertTxt.add ("Get Res Time out : " + path, Color.red, 1, 1, false);
			Debug.LogWarning ("Get Res Time out : " + path);

			Utl.doCallback (onGetAsset, path, null, originals);

			rmWWW (url);
			wwwMap [path] = false;
			origs.Clear ();
		}

		/// <summary>
		/// Saves the new res.保存最新取得的资源
		/// </summary>
		/// <param name='path'>
		/// Path.
		/// </param>
		/// <param name='content'>
		/// Content.
		/// </param>
		void saveNewRes (string path, byte[] content)
		{
			string file = PStr.begin ().a (CLPathCfg.persistentDataPath).a ("/").a (path).end ();
			Directory.CreateDirectory (Path.GetDirectoryName (file));
			File.WriteAllBytes (file, content);
			if (otherResVerNew [path] != null) { //优先更新资源已经是最新的了，所以不用再同步
				otherResVerOld [path] = otherResVerNew [path];
				MemoryStream ms = new MemoryStream ();
				B2OutputStream.writeMap (ms, otherResVerOld);
            
				string vpath = PStr.begin ().a (CLPathCfg.persistentDataPath).a ("/").a (mVerOtherPath).end ();
				Directory.CreateDirectory (Path.GetDirectoryName (vpath));
				File.WriteAllBytes (vpath, ms.ToArray ());
			}
		}

		public Texture getAtalsTexture4Edit (string path)
		{
			string url = "";
#if UNITY_EDITOR
			url = "file://" + Application.dataPath + "/" + path;
#endif
			WWW www = new WWW (Utl.urlAddTimes (url));
			while (!www.isDone) {
//				new WaitForSeconds (0.05f);
				System.Threading.Thread.Sleep (50); //比较hacker的做法
			}
			if (string.IsNullOrEmpty (www.error)) {
				Texture texture = www.texture;
				www.Dispose ();
				www = null;
				return texture;
			} else {
				www.Dispose ();
				www = null;
				return null;
			}
		}

		public bool checkNeedDownload (string path)
		{
			if (string.IsNullOrEmpty (path)) {
				return false;
			}
			bool ret = false;
			if (localPriorityVer [path] != null) {  //在优先资源里有
				ret = false;
			} else {        //则可能在others里
				object obj1 = otherResVerOld [path];
				object obj2 = otherResVerNew [path];
				if (obj1 == null && obj2 != null) { //本地没有，最新有
					ret = true;
				} else if (obj1 != null && obj2 != null) {
					if (NumEx.stringToInt (obj1.ToString ()) >= NumEx.stringToInt (obj2.ToString ())) {//本地是最新的
						ret = false;
					} else {    //本地不是最新的
						ret = true;
					}
				} else if (obj1 != null && obj2 == null) {//本地有，最新没有
					ret = false;
				} else {    //都没有找到
//                NAlertTxt.add("Cannot Found the res. Path= \n"+ path, Color.red, 1);
					Debug.LogError ("Cannot Found the res. Path= \n" + path);
					ret = true;
				}
			}
//			if (ret) {
//				if (!Utl.netIsActived ()) {
//					NAlertTxt.add (Localization.Get ("MsgNetWorkCannotReached"), Color.red, 1);
//					reLoadGame ();
//				}
//			}
			return ret;
		}

		/// <summary>
		/// Ises the ver newest.
		/// </summary>
		/// <returns>
		/// The ver newest.
		/// </returns>
		/// <param name='path'>
		/// If set to <c>true</c> path.
		/// </param>
		/// <param name='ver'>
		/// If set to <c>true</c> ver.
		/// </param>
		public bool isVerNewest (string path, string ver)
		{
			object newVer = localPriorityVer [path];
			if (newVer != null) {   //在优先资源里有
				if (!ver.Equals (newVer.ToString ())) {
					return false;
				}
				return true;
			} else {        //则可能在others里
				newVer = otherResVerNew [path];
				if (newVer != null) {
					if (!ver.Equals (newVer.ToString ())) {
						return false;
					}
					return true;
				}
				return true;
			}
		}
	}

	public enum CLAssetType
	{
		text,
		bytes,
		texture,
		assetBundle,
	}
}