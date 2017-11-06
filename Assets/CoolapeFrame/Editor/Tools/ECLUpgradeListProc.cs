using UnityEditor;
using UnityEngine;
using System.Collections;
using Coolape;
using System.IO;
using System.Threading;

public class ECLUpgradeListProc : EditorWindow
{
	public static ECLUpgradeListProc self;

	public static  ArrayList mList = null;
	public static string cfgPath = "";
	Hashtable item;
	Vector2 scrollPos = Vector2.zero;
	static bool isFinishUpload = false;
	static bool uploadResult = false;
	static float uploadProgerss = 0;
	static bool isUploading = false;
	static string selectedPackageName = "";
	static bool isSelectMod = false;
	static Callback onSelectedCallback;
	static object selectedCallbackParams;

	public ECLUpgradeListProc ()
	{
		self = this;
		EditorApplication.update += OnUpdate;
	}

	void OnUpdate ()
	{
		if (isFinishUpload) {
			isFinishUpload = false;
			//TODO:
			Debug.Log ("finished");
			EditorUtility.ClearProgressBar ();
			if (uploadResult) {
				// success
				updateState (selectedPackageName);
				EditorUtility.DisplayDialog ("Success", "Success !", "Okey");
			} else {
				EditorUtility.DisplayDialog ("Fail", "Failed !", "Okey");
			}
		}
		if (isUploading) {
			EditorUtility.DisplayProgressBar ("UpLoad", "Uploading....!", uploadProgerss);	
		}
	}

	void OnGUI ()
	{
		if (!ECLProjectSetting.isProjectExit (ECLProjectManager.self)) {
			GUIStyle style = new GUIStyle ();
			style.fontSize = 20;
			style.normal.textColor = Color.yellow;
			GUILayout.Label ("The scene is not ready, create it now?", style);
			if (GUILayout.Button ("Show Project Manager")) {
				EditorWindow.GetWindow<ECLProjectManager> (false, "CoolapeProject", true);
			}
			Close ();
			return;
		}

		EditorGUILayout.BeginHorizontal ();
		{
			GUI.color = Color.green;
			if (GUILayout.Button ("Refresh", GUILayout.Height (40f))) {
				setData ();
			}
			GUI.color = Color.white;
			if (!isSelectMod) {
				if (GUILayout.Button ("Save", GUILayout.Height (40f))) {
					if (mList == null || mList.Count == 0) {
						Debug.LogWarning ("Nothing need to save!");
						return;
					}
					string str = JSON.JsonEncode (mList);
					File.WriteAllText (Application.dataPath + "/" + cfgPath, str);
				}
			}
		}
		EditorGUILayout.EndHorizontal ();

		ECLEditorUtl.BeginContents ();
		{
			EditorGUILayout.BeginHorizontal ();
			{
				EditorGUILayout.LabelField ("Package Name", GUILayout.Width (160));
				EditorGUILayout.LabelField ("MD5", GUILayout.Width (250));
				EditorGUILayout.LabelField ("Exist?", GUILayout.Width (40));
				EditorGUILayout.LabelField ("Upload?", GUILayout.Width (60));
				EditorGUILayout.LabelField ("...", GUILayout.Width (60));
				EditorGUILayout.LabelField ("Notes");
			}
			EditorGUILayout.EndHorizontal ();
			if (mList == null) {
				return;
			}
			scrollPos = EditorGUILayout.BeginScrollView (scrollPos, GUILayout.Width (position.width), GUILayout.Height (position.height - 75));
			{
				for (int i = mList.Count - 1; i >= 0; i--) {
					item = ListEx.getMap (mList, i);

					EditorGUILayout.BeginHorizontal ();
					{
//					GUI.enabled = false;
						EditorGUILayout.TextField (MapEx.getString (item, "name"), GUILayout.Width (160));
//						EditorGUILayout.TextField (getUpgradePackageMd5 (MapEx.getString (item, "name")), GUILayout.Width (250));
						EditorGUILayout.TextField (MapEx.getString (item, "md5"), GUILayout.Width (250));

						if (!MapEx.getBool (item, "exist")) {
							GUI.color = Color.red;
						}
						EditorGUILayout.TextField (MapEx.getBool (item, "exist") ? "Yes" : "No", GUILayout.Width (40));
						GUI.color = Color.white;
						if (!MapEx.getBool (item, "upload")) {
							GUI.color = Color.red;
						}
						EditorGUILayout.TextField (MapEx.getBool (item, "upload") ? "Yes" : "No", GUILayout.Width (60));
						GUI.color = Color.white;
						if (MapEx.getBool (item, "exist")) {
							GUI.enabled = true;
						} else {
							GUI.enabled = false;
						}
						GUI.color = Color.yellow;
						if (isSelectMod) {
							if (GUILayout.Button ("select", GUILayout.Width (60f))) {
								Close ();
								Utl.doCallback(onSelectedCallback, item, selectedCallbackParams);
							}
						} else {
							if (GUILayout.Button ("upload", GUILayout.Width (60f))) {
								if (EditorUtility.DisplayDialog ("Alert", "Really want to upload the upgrade package?", "Okey", "cancel")) {
									selectedPackageName = MapEx.getString (item, "name");
									uploadUpgradePackage (MapEx.getString (item, "name"));
								}	
							}
						}

						GUI.color = Color.white;
						GUI.enabled = true;
						item ["remark"] = EditorGUILayout.TextArea (MapEx.getString (item, "remark"));

						GUILayout.Space (5);
					}
					EditorGUILayout.EndHorizontal ();
				}
			}
			EditorGUILayout.EndScrollView ();
		}
		ECLEditorUtl.EndContents ();
	}

	public void uploadUpgradePackage (string name)
	{
		if (!Utl.netIsActived ()) {
			EditorUtility.DisplayDialog ("Alert", "The net work is not connected!", "Okay");
			return;
		}
		string localDir = getUpgradePackagePath (name);
		ThreadEx.exec (new System.Threading.ParameterizedThreadStart (doUploadUpgradePackage), localDir);
//		doUploadUpgradePackage (localDir);
	}

	void onSftpProgress (params object[] pars)
	{
		uploadProgerss = (float)(pars [0]);
	}

	void onftpFinish (params object[] pars)
	{
		isUploading = false;
		isFinishUpload = true;
		bool ret = (bool)(pars [0]);
		ECLUpgradeListProc.uploadResult = ret;
	}

	public void doUploadUpgradePackage (object localDir)
	{
		isUploading = true;
		if (ECLProjectManager.data.useSFTP) {
			SFTPHelper sftp = new SFTPHelper (ECLProjectManager.data.host4UploadUpgradePackage,
				                  ECLProjectManager.data.port4UploadUpgradePackage,
				                  ECLProjectManager.data.ftpUser, 
				                  ECLProjectManager.data.ftpPassword);
			if (sftp.Connect ()) {
				sftp.PutDir (localDir.ToString (), ECLProjectManager.data.RemoteBaseDir, (Callback)onSftpProgress, (Callback)onftpFinish);
				sftp.Exit ();
				sftp = null;
			} else {
				Utl.doCallback ((Callback)onftpFinish, false);
			}
		} else {
			bool ret = FTP.UploadDir (localDir.ToString (), 
				           ECLProjectManager.data.host4UploadUpgradePackage, 
				           ECLProjectManager.data.ftpUser, 
				           ECLProjectManager.data.ftpPassword, 
				           ECLProjectManager.data.RemoteBaseDir, false);
			Utl.doCallback ((Callback)onftpFinish, ret);
		}
	}

	public void setData ()
	{
		if (string.IsNullOrEmpty (cfgPath)) {
			cfgPath = ECLProjectManager.ver4UpgradeList;
		}
		string str = "";
		string p = Application.dataPath + "/" + cfgPath;
		if (File.Exists (p)) {
			str = File.ReadAllText (p); 
		}
		ArrayList list = JSON.DecodeList (str);
		list = list == null ? new ArrayList () : list;
		ECLUpgradeListProc.mList = list;
		refreshData ();
	}

	public void refreshData ()
	{
		if (mList == null)
			return;
		Hashtable item = null;

		for (int i = 0; i < mList.Count; i++) {
			item = ListEx.getMap (mList, i);
			if (Directory.Exists (getUpgradePackagePath (MapEx.getString (item, "name")))) {
				item ["exist"] = true;
			} else {
				item ["exist"] = false;
			}
		}
	}

	public void updateState (string name)
	{
		if (mList == null) {
			return;
		}
		Hashtable item = null;

		for (int i = 0; i < mList.Count; i++) {
			item = ListEx.getMap (mList, i);
			if (name.Equals (MapEx.getString (item, "name"))) {
				item ["upload"] = true;
				break;
			}
		}

		string str = JSON.JsonEncode (mList);
		File.WriteAllText (Application.dataPath + "/" + cfgPath, str);
	}

	public string getUpgradePackageMd5 (string name)
	{
		string p = getUpgradePackagePath (name);
		p = PStr.b ().a (p).a ("/").a (CLPathCfg.self.basePath).a ("/resVer/").a (CLPathCfg.self.platform).e ();
		if (Directory.Exists (p)) {
			string[] files = Directory.GetFiles (p);
			string fileName = "";
			for (int i = 0; i < files.Length; i++) {
				fileName = Path.GetFileName (files [i]);
				if (fileName.StartsWith ("VerCtl.ver")) {
					return Utl.MD5Encrypt (File.ReadAllBytes (files [i]));
				}
			}
		}
		return "";
	}

	public string getUpgradePackagePath (string name)
	{
		string p = Path.Combine (Application.dataPath, name);
//		p = Path.Combine (p, CLPathCfg.self.basePath);
		p = p.Replace ("/Assets/", "/Assets4Upgrade/");
		return p;
	}

	public static void show4UpgradeList (string cfgPath)
	{
		if (string.IsNullOrEmpty (cfgPath)) {
			cfgPath = ECLProjectManager.ver4UpgradeList;
		}
		isSelectMod = false;
		string str = "";
		string p = Application.dataPath + "/" + cfgPath;
		if (File.Exists (p)) {
			str = File.ReadAllText (p); 
		}
		ArrayList list = JSON.DecodeList (str);
		if (list == null || list.Count == 0) {
			EditorUtility.DisplayDialog ("Alert", "no data to show!", "Okay");
			return;
		}

		ECLUpgradeListProc window = ECLUpgradeListProc.GetWindow<ECLUpgradeListProc> (true, "Upgrade Res List", true);
		if (window == null) {
			window = new ECLUpgradeListProc ();
		}
//		Vector2 size = Handles.GetMainGameViewSize ();
		Rect rect = window.position;
		rect.x = -Screen.width - Screen.width / 4;
		rect.y = Screen.height / 2 - Screen.height / 4;
		rect.width = Screen.width;
		rect.height = Screen.height / 2;

//		rect = new Rect (-size.x/2, size.y / 2 - size.y / 4, size.x / 2, size.y / 2);
		window.position = rect;
		window.title = "Upgrade资源包列表";
		ECLUpgradeListProc.mList = list;
		window.refreshData ();
		ECLUpgradeListProc.cfgPath = cfgPath;
		window.ShowPopup ();
	}


	public static void popup4Select (Callback cb, object orgs)
	{
		show4UpgradeList (null);
		isSelectMod = true;
		onSelectedCallback = cb;
		selectedCallbackParams = orgs;
	}
}
