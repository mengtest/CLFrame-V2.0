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

		public enum NetWorkType
		{
			publish,
			test1,
			test2,
		}

		public int _SuccessCodeValue = 0;

		// 成功的返回值
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
			dispatchSend = getLuaFunction ("dispatchSend");
			packMsgFunc = getLuaFunction ("packMsg");
			unPackMsgFunc = getLuaFunction ("unpackMsg");
		}

		LuaFunction dispatchGate;
		LuaFunction dispatchGame;
		LuaFunction dispatchSend;
		LuaFunction packMsgFunc;
		LuaFunction unPackMsgFunc;
		//===================end=====================
		public Queue netGateDataQueue = new Queue ();
		public Queue netGameDataQueue = new Queue ();

		public void dispatchGate4Lua (object obj, Tcp tcp)
		{
			netGateDataQueue.Enqueue (obj);
		}

		public void dispatchGame4Lua (object obj, Tcp tcp)
		{
			netGameDataQueue.Enqueue (obj);
		}

		object netData = null;

		void LateUpdate ()
		{
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
				gateTcp = new Tcp (dispatchGate4Lua, packMsgFunc, unPackMsgFunc);
			}

			if (!gateTcp.connected) {
				gateTcp.init (gateHost, gatePort);
				gateTcp.connect ();
			} else {
				gateTcp.connectCallback (gateTcp.socket, true);
			}
		}

		public void connectGame (string host, int port)
		{
			try{
				StartCoroutine (doConnectGame (host, port));
			} catch(System.Exception e) {
				Debug.LogError (e);
			}
		}

		IEnumerator doConnectGame (string host, int port)
		{
			yield return null;
			if (gameTcp == null) {
				gameTcp = new Tcp (dispatchGame4Lua, packMsgFunc, unPackMsgFunc);
			}
			this.host = host;
			this.port = port;
			if (!gameTcp.connected) {
				gameTcp.init (host, port);
				gameTcp.connect ();
			} else {
				gameTcp.connectCallback (gameTcp.socket, true);
			}
		}

		public void sendGate (object data)
		{
			if (gateTcp != null) {
				gateTcp.send (data);
			} else {
				Debug.LogError ("The gate is not connected!");
			}
		}

		public void send (object data)
		{
			if (isReallyUseNet) {
				if (gameTcp != null) {
					gameTcp.send (data);
				} else {
					Debug.LogError ("The server is not connected!");
				}
			} else {
				dispatchSend.Call (data);
			}
		}

	}
}
