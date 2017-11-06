using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using XLua;


namespace Coolape
{
	public class Net : CLBaseLua
	{
		public static Net self;

		public Net ()
		{
			self = this;
		}

		public int _SuccessCodeValue = 0;

		public static int SuccessCode {
			get {
				return self._SuccessCodeValue;
			}
		}

		public bool isReallyUseNet = true;
		[HideInInspector]
		public NetWorkType switchNetType = NetWorkType.publish;
		[HideInInspector]
		public string host4Publish = "";
		[HideInInspector]
		public string host4Test1 = "";
		[HideInInspector]
		public string host4Test2 = "";

		// 默认地址
		string _gateHost;

		public string gateHost {     //网关
			get {
				switch (switchNetType) {
				case NetWorkType.publish:
					_gateHost = host4Publish;
					break;
				case NetWorkType.test1:
					_gateHost = host4Test1;
					break;
				case NetWorkType.test2:
					_gateHost = host4Test2;
					break;
				}
				return _gateHost;
			}
			set {
				_gateHost = value;
			}
		}

		public int gatePort;
		//网关
		public int httpPort;
		public string httpFunc = "";
		[HideInInspector]
		public string
			host;
		[HideInInspector]
		public int
			port;
		public Tcp gateTcp = null;
		public Tcp gameTcp = null;


		//=====================begain===================
		public override void setLua ()
		{
			base.setLua ();
			dispatchGate = getLuaFunction ("dispatchGate");
			dispatchGame = getLuaFunction ("dispatchGame");
			dispatchHttp = getLuaFunction ("dispatchHttp");
			dispatchSend = getLuaFunction ("dispatchSend");
		}

		LuaFunction dispatchGate;
		LuaFunction dispatchGame;
		LuaFunction dispatchSend;
		LuaFunction dispatchHttp;
		//===================end=====================
		public Queue netGateDataQueue = new Queue ();
		public Queue netGameDataQueue = new Queue ();
		public Queue netHttpDataQueue = new Queue ();

		public void dispatchGate4Lua (object obj, Tcp tcp)
		{
			netGateDataQueue.Enqueue (obj);
		}

		public void dispatchGame4Lua (object obj, Tcp tcp)
		{
			netGameDataQueue.Enqueue (obj);
		}

		public void dispatchHttp4Lua (object obj, Tcp tcp)
		{
			netHttpDataQueue.Enqueue (obj);
		}

		object netData = null;

		void LateUpdate ()
		{
			if (netHttpDataQueue.Count > 0) {
				netData = netHttpDataQueue.Dequeue ();
				if (netData != null) {
					if (dispatchHttp != null) {
						dispatchHttp.Call (netData);
					}
				}
			}
			if (netGateDataQueue.Count > 0) {
				netData = netGateDataQueue.Dequeue ();
				if (netData != null) {
					if (dispatchGate != null) {
						dispatchGate.Call (netData);
					}
				}
			}
			if (netGameDataQueue.Count > 0) {
				netData = netGameDataQueue.Dequeue ();
				if (netData != null) {
					if (dispatchGame != null) {
						//                  StringBuilder sb = new StringBuilder();
						//                  Utl.MapToString((Hashtable)netData,  sb);
						//                  Debug.Log(sb.ToString());
						dispatchGame.Call (netData);
					}
				}
			}
		}

		//连接网关
		public void connectGate ()
		{
			StartCoroutine (doConnectGate ());
		}

		IEnumerator doConnectGate ()
		{
			yield return null;
			if (gateTcp == null) {
				gateTcp = new Tcp (dispatchGate4Lua);
			}

			if (!gateTcp.connected) {
				gateTcp.connected = true;
				gateTcp.init (gateHost, gatePort);
				gateTcp.connect ();
			} else {
				gateTcp.connectCallback (true);
			}
		}

		public void connectGame (string host, int port)
		{
			StartCoroutine (doConnectGame (host, port));
		}

		IEnumerator doConnectGame (string host, int port)
		{
			yield return null;
			if (gameTcp == null) {
				gameTcp = new Tcp (dispatchGame4Lua);
			}
			this.host = host;
			this.port = port;
			if (!gameTcp.connected) {
				gameTcp.connected = true;
				gameTcp.init (host, port);
				gameTcp.connect ();
			} else {
				gameTcp.connectCallback (true);
			}
		}

		public void sendGate (Hashtable map)
		{
			//      Hashtable newMap = new Hashtable ();
			//      chgMapKey (map, newMap);
			if (gateTcp != null) {
				gateTcp.send (map);
			} else {
				Debug.LogError ("The gate is not connected!");
			}
		}

		public void send (Hashtable map)
		{
			if (isReallyUseNet) {
				if (gameTcp != null) {
					gameTcp.send (map);
				} else {
					Debug.LogError ("The server is not connected!");
				}
			} else {
				dispatchSend.Call (map);
			}
		}

		string _baseUrl = "";

		public string baseUrl {
			get {
				if (string.IsNullOrEmpty (_baseUrl)) {
					_baseUrl = "http://" + gateHost + ":" + httpPort;
				}
				return _baseUrl;
			}
		}

		public void sendHttpJson (Hashtable map)
		{
			StartCoroutine (doSendHttpJson (httpFunc, map));
		}

		public IEnumerator doSendHttpJson (string func, Hashtable map)
		{
			string url = baseUrl + "/" + func + getParas (map) + "&t_sign_flag=" + DateEx.now;
			#if UNITY_EDITOR
			Debug.Log (System.Uri.UnescapeDataString (url));
			#endif
			url = System.Uri.EscapeUriString (url);
//			Debug.Log (url);
			//        string result = Toolkit.HttpEx.get2str(url);
			WWW www = new WWW (url);
			yield return www;
			Hashtable result = new Hashtable ();
			if (string.IsNullOrEmpty (www.error)) {
				#if UNITY_EDITOR
				Debug.Log ("result==[" + www.text + "]");
				#endif
				result = JSON.DecodeMap (www.text.Trim ());
			} else {
				Debug.Log (www.error);
				Debug.Log (www.text);
				result ["error"] = -99999;
				result ["message"] = www.error;
				result ["api"] = MapEx.getString (map, "api");
			}
			www.Dispose ();
			www = null;
			dispatchHttp4Lua (result, null);
			//        map.Clear();
			//        map = null;
		}

		public string getParas (Hashtable map)
		{
			string paras = "";
			if (map != null && map.Count > 0) {
				int count = map.Count;
				int index = 0;
				foreach (DictionaryEntry cell in map) {
//					Debug.Log (cell.Key);
//					Debug.Log (cell.Value);
					object value = cell.Value;
					if (index == 0) {
						paras = PStr.b ().a (paras).a ("?").a (cell.Key).a ("=").a (System.Uri.EscapeDataString (value == null ? "" : value.ToString ())).e ();
						//                    paras = PStr.b().a(paras).a("?").a(cell.Key).a("=").a(cell.Value).e();
					} else {
						paras = PStr.b ().a (paras).a (cell.Key).a ("=").a (System.Uri.EscapeDataString (value == null ? "" : value.ToString ())).e ();
						//                    paras = PStr.b().a(paras).a(cell.Key).a("=").a(cell.Value).e();
					}
					if (index < count - 1) { //last one
						paras = PStr.b ().a (paras).a ("&").e ();
					}
					index++;
				}
			}
			return paras;
		}

		public void sendHttp (Hashtable map)
		{
			StartCoroutine (doSendHttp (baseUrl + "/" + httpFunc, map));
		}

		public void sendHttp2 (string url, Hashtable map)
		{
			StartCoroutine (doSendHttp (url, map));
		}

		public IEnumerator doSendHttp (string url, Hashtable map)
		{
//        Debug.LogError("url==" + url);
			MemoryStream ms = new MemoryStream ();
			B2OutputStream.writeMap (ms, map);
			WWW www = new WWW (url, ms.ToArray ());
			yield return www;
			Hashtable result = null;
			if (!string.IsNullOrEmpty (www.error)) {
				Debug.Log (www.error + "url==" + url);
				result = new Hashtable ();
				result ["retCode"] = -99991;
				result ["retMsg"] = www.error;
				result ["func_id"] = MapEx.getString (map, "func_id");
			} else {
				byte[] bs = www.bytes;
				ms.Position = 0;
				ms.Write (bs, 0, bs.Length);
				ms.Position = 0;
				result = (Hashtable)(B2InputStream.readObject (ms));
			}
			www.Dispose ();
			www = null;
			// Debug.LogError("http result===" + Utl.MapToString(result));
			dispatchHttp4Lua (result, null);
		}

		public void chgMapKey (Hashtable inMap, Hashtable outMap)
		{
			foreach (DictionaryEntry cell in inMap) {
				if (cell.Value is Hashtable) {
					Hashtable map = new Hashtable ();
					chgMapKey ((Hashtable)(cell.Value), map);
					outMap [(int)(cell.Key)] = map;
				} else if (cell.Value is ArrayList) {
					ArrayList list = (ArrayList)(cell.Value);
					ArrayList _list = new ArrayList ();
					for (int i = 0; i < list.Count; i++) {
						if (list [i] is Hashtable) {
							Hashtable map = new Hashtable ();
							chgMapKey ((Hashtable)(list [i]), map);
							_list.Add (map);
						} else {
							_list.Add (list [i]);
						}
					}
					outMap [NumEx.toInt (cell.Key)] = _list;
				} else {
					outMap [NumEx.toInt (cell.Key)] = cell.Value;
				}
			}
		}

		public enum NetWorkType
		{
			publish,
			test1,
			test2,
		}
	}
}
