/*
******************************************************************************** 
  *Copyright(C),coolae.net 
  *Author:  chenbin
  *Version:  2.0 
  *Date:  2017-01-09
  *Description:  WWW包装
  *Others:  
  *History:
*********************************************************************************
*/ 

using UnityEngine;
using System.Collections;

namespace Coolape
{
	public class WWWEx  :MonoBehaviour
	{
		public static  Hashtable wwwMapUrl = new Hashtable ();
		public static  Hashtable wwwMap4Check = new Hashtable ();
		public static Hashtable wwwMap4Get = new Hashtable ();

		public static void newWWW (MonoBehaviour go, string url, CLAssetType type, 
		                          float checkProgressSec, float timeOutSec, object finishCallback, 
		                          object exceptionCallback, object timeOutCallback, object orgs)
		{
			if (string.IsNullOrEmpty (url))
				return;
#if UNITY_4_6 || UNITY_5
			UnityEngine.Coroutine cor = go.StartCoroutine (doNewWWW (go, url, null, type, checkProgressSec, timeOutSec, finishCallback, exceptionCallback, timeOutCallback, orgs));
			wwwMap4Get [url] = cor;
#else
#endif
		}

		public static void newWWW (MonoBehaviour go, string url, object mapData,
		                          CLAssetType type, 
		                          float checkProgressSec, float timeOutSec, object finishCallback, 
		                          object exceptionCallback, object timeOutCallback, object orgs)
		{
			if (string.IsNullOrEmpty (url))
				return;
			#if UNITY_4_6 || UNITY_5
			UnityEngine.Coroutine cor = go.StartCoroutine (doNewWWW (go, url, mapData, type, checkProgressSec, timeOutSec, finishCallback, exceptionCallback, timeOutCallback, orgs));
			wwwMap4Get [url] = cor;
			#else
			#endif
		}

		// 因为lua里没有bytes类型，所以不能重载，所以只能通方法名来
		public static void newWWWPostBytes (MonoBehaviour go, string url, byte[] mapData,
			CLAssetType type, 
			float checkProgressSec, float timeOutSec, object finishCallback, 
			object exceptionCallback, object timeOutCallback, object orgs)
		{
			if (string.IsNullOrEmpty (url))
				return;
			#if UNITY_4_6 || UNITY_5
			UnityEngine.Coroutine cor = go.StartCoroutine (doNewWWW (go, url, mapData, type, checkProgressSec, timeOutSec, finishCallback, exceptionCallback, timeOutCallback, orgs));
			wwwMap4Get [url] = cor;
			#else
			#endif
		}

		public static void newWWW (MonoBehaviour go, string url, string jsonMap,
		                          CLAssetType type, 
		                          float checkProgressSec, float timeOutSec, object finishCallback, 
		                          object exceptionCallback, object timeOutCallback, object orgs)
		{
			if (string.IsNullOrEmpty (url))
				return;
			Hashtable mapData = JSON.DecodeMap (jsonMap);
			#if UNITY_4_6 || UNITY_5
			UnityEngine.Coroutine cor = go.StartCoroutine (doNewWWW (go, url, mapData, type, checkProgressSec, timeOutSec, finishCallback, exceptionCallback, timeOutCallback, orgs));
			wwwMap4Get [url] = cor;
			#else
			#endif
		}

		public static IEnumerator doNewWWW (MonoBehaviour go, string url, object mapData, CLAssetType type, 
		                                   float checkProgressSec, float timeOutSec, object finishCallback, 
		                                   object exceptionCallback, object timeOutCallback, object orgs)
		{
			WWW www = null;
			if (mapData != null) {
				if (mapData is Hashtable) {
					WWWForm wf = new WWWForm ();
					foreach (DictionaryEntry cell in (Hashtable)mapData) {
						if (cell is string) {
							wf.AddField (cell.Key.ToString (), cell.Value.ToString ());
						} else if (cell is int) {
							wf.AddField (cell.Key.ToString (), int.Parse (cell.Value.ToString ()));
						} else if (cell is byte[]) {
							wf.AddBinaryData (cell.Key.ToString (), (byte[])(cell.Value));
						} else {
							wf.AddField (cell.Key.ToString (), cell.Value.ToString ());
						}
					}
					www = new WWW (url, wf);
				} else if (mapData is WWWForm) {
					www = new WWW (url, (mapData as WWWForm));
				} else if (mapData is byte[]) {
					www = new WWW (url, (mapData as byte[]));
				}
			} else {
				www = new WWW (url);
			}
			checkWWWTimeout (go, www, checkProgressSec, timeOutSec, timeOutCallback, null);
			wwwMapUrl [url] = www;
			yield return www;
			try {
//			Debug.Log("Location==="  + www.responseHeaders["Location"]);
				uncheckWWWTimeout (go, www);
				if (string.IsNullOrEmpty (www.error)) {
					object content = null;
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
					doCallback (finishCallback, content, orgs);
				} else {
					int retCode = getResponseCode (www);
					if (retCode == 300 || retCode == 301 || retCode == 302) {
						// 重定向处理
						if (www.responseHeaders.ContainsKey ("Location")) {
							string url2 = www.responseHeaders ["Location"];
							if (string.IsNullOrEmpty (url2)) {
								doCallback (exceptionCallback, null, orgs);
							} else {
								newWWW (go, url2, mapData, type, 
									checkProgressSec, timeOutSec, finishCallback, 
									exceptionCallback, timeOutCallback, orgs);
							}
						} else {
							doCallback (exceptionCallback, null, orgs);
						}
					} else {
						doCallback (exceptionCallback, null, orgs);
					}
				}
				www.Dispose ();
				www = null;
				wwwMapUrl.Remove (url);
			} catch (System.Exception e) {
				Debug.LogError (url + ":" + e);
				wwwMapUrl.Remove (url);
				if (www != null) {
					www.Dispose ();
					www = null;
				}
			}
		}

		/// <summary>
		/// Checks the WWW timeout.
		/// </summary>
		/// <param name="go">Go.</param>
		/// <param name="www">Www.</param>
		/// <param name="checkProgressSec">Check progress sec.</param>
		/// <param name="timeOutSec">Time out sec.</param>
		/// <param name="timeoutCallback">Timeout callback.</param>
		public static void checkWWWTimeout (MonoBehaviour go, WWW www, float checkProgressSec, float timeOutSec, object timeoutCallback, object orgs)
		{
			if (www == null || www.isDone)
				return;
			if (go == null)
				return;
			checkProgressSec = checkProgressSec <= 0 ? 2 : checkProgressSec;
			#if UNITY_4_6 || UNITY_5
			UnityEngine.Coroutine cor = go.StartCoroutine (doCheckWWWTimeout (go, www, checkProgressSec, timeOutSec, timeoutCallback, 0, 0, orgs));
			wwwMap4Check [www] = cor;
			#else
			go.StartCoroutine (doCheckWWWTimeout (go, www, checkProgressSec, timeOutSec, timeoutCallback, 0, 0, orgs));
			wwwMap4Check [www] = true;
			#endif
		}

		public static IEnumerator doCheckWWWTimeout (MonoBehaviour go, WWW www, float checkProgressSec, 
		                                            float timeOutSec, object timeoutCallback, float oldProgress, float totalCostSec, object orgs)
		{
			yield return new WaitForSeconds (checkProgressSec);
			totalCostSec = totalCostSec + checkProgressSec;
			try {
				if (www != null) {
					if (www.isDone) {
						wwwMap4Check.Remove (www);
					} else {
						if (www.progress == oldProgress) { //说明没有变化，可能网络不给力
							wwwMap4Check.Remove (www);
							string url = www.url;
							UnityEngine.Coroutine corout = (UnityEngine.Coroutine)(wwwMap4Get [url]);
							if (corout != null) {
								wwwMap4Get.Remove (url);
								go.StopCoroutine (corout);
							}
							www.Dispose ();
							www = null;
							doCallback (timeoutCallback, null, orgs);
						} else if (timeOutSec > 0 && totalCostSec >= timeOutSec) {
							wwwMap4Check.Remove (www);
							string url = www.url;
							UnityEngine.Coroutine corout = (UnityEngine.Coroutine)(wwwMap4Get [url]);
							if (corout != null) {
								wwwMap4Get.Remove (url);
								go.StopCoroutine (corout);
							}
							www.Dispose ();
							www = null;
							doCallback (timeoutCallback, null, orgs);
						} else {
							#if UNITY_4_6 || UNITY_5
							UnityEngine.Coroutine cor = go.StartCoroutine (doCheckWWWTimeout (go, www, checkProgressSec, timeOutSec, timeoutCallback, www.progress, totalCostSec, orgs));
							wwwMap4Check [www] = cor;
							#else
							go.StartCoroutine (doCheckWWWTimeout (go, www, checkProgressSec, timeOutSec, timeoutCallback, www.progress, totalCostSec, orgs));
							wwwMap4Check [www] = true;
							#endif
						}
					}
				}
			} catch (System.Exception e) {
				go.StopCoroutine ("doCheckWWWTimeout");
				Debug.LogError (e);
			}
		}

		public static void uncheckWWWTimeout (MonoBehaviour go, WWW www)
		{
			#if UNITY_4_6 || UNITY_5
			UnityEngine.Coroutine cor = (UnityEngine.Coroutine)(wwwMap4Check [www]);
			if (cor != null) {
				go.StopCoroutine (cor);
			}
			#else
			go.StopCoroutine ("doCheckWWWTimeout");
			#endif
			wwwMap4Check.Remove (www);
			wwwMap4Get.Remove (www.url);
		}

		public static void doCallback (object callback, object obj, object orgs)
		{
			if (callback == null)
				return;
			Utl.doCallback (callback, obj, orgs);
		}

		public static WWW getWwwByUrl (string Url)
		{
			if (string.IsNullOrEmpty (Url))
				return null;
			return (WWW)(wwwMapUrl [Url]);
		}

		public static int getResponseCode (WWW request)
		{
			int ret = 0;
			if (request.responseHeaders == null) {
				Debug.LogError ("no response headers.");
			} else {
				if (!request.responseHeaders.ContainsKey ("STATUS")) {
					Debug.LogError ("response headers has no STATUS.");
				} else {
					ret = parseResponseCode (request.responseHeaders ["STATUS"]);
				}
			}

			return ret;
		}

		public static int parseResponseCode (string statusLine)
		{
			int ret = 0;
			string[] components = statusLine.Split (' ');
			if (components.Length < 3) {
				Debug.LogError ("invalid response status: " + statusLine);
			} else {
				if (!int.TryParse (components [1], out ret)) {
					Debug.LogError ("invalid response code: " + components [1]);
				}
			}
			return ret;
		}
	}
}
