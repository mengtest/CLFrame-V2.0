/*
******************************************************************************** 
  *Copyright(C),coolae.net 
  *Author:  chenbin
  *Version:  2.0 
  *Date:  2017-01-09
  *Description:  tcp
  *Others:  
  *History:
*********************************************************************************
*/ 

using UnityEngine;
using System.Collections;
using System.IO;
using XLua;

namespace Coolape
{
	public delegate void TcpDispatchCallback (object obj, Tcp tcp);
	public delegate byte[] TcpPackMessageAndSendFunc (object obj, Tcp tcp);
	public class Tcp
	{
		public string host;
		public int port;
		public bool connected = false;
		//是否连接
		public bool isStopping = false;
		const int MaxReConnectTimes = 0;
		public static int __maxLen = 1024 * 1024;

		System.Threading.Timer timer;
		public USocket socket;
		int reConnectTimes = 0;
		public const string CONST_Connect = "connectCallback";
		public const string CONST_OutofNetConnect = "outofNetConnect";
		TcpDispatchCallback mDispatcher;
		//消息组包函数，需要返回bytes
		public object msgPackAndSendFunc;
		//消息解包函数，第一个入参数是socket，第二个入参在是MemoryStream, 返回解包后得到的对象
		public object msgUnpackFunc;

		public Tcp ()
		{
		}

		public Tcp (TcpDispatchCallback dispatcher)
		{
			mDispatcher = dispatcher;
		}

		public Tcp (TcpDispatchCallback dispatcher, object msgPackAndSendFunc, object msgUnpackFunc)
		{
			mDispatcher = dispatcher;
			this.msgPackAndSendFunc = msgPackAndSendFunc;
			this.msgUnpackFunc = msgUnpackFunc;
		}

		public void init (string host, int port)
		{
			this.host = host;
			this.port = port;
		}

		public void init (string host, int port, object msgPackAndSendFunc, object msgUnpackFunc)
		{
			this.host = host;
			this.port = port;
			this.msgPackAndSendFunc = msgPackAndSendFunc;
			this.msgUnpackFunc = msgUnpackFunc;
		}

		public void init (string host, int port, TcpDispatchCallback dispatcher, object msgPackAndSendFunc, object msgUnpackFunc)
		{
			mDispatcher = dispatcher;
			this.msgPackAndSendFunc = msgPackAndSendFunc;
			this.msgUnpackFunc = msgUnpackFunc;
			this.host = host;
			this.port = port;
		}

		public void connect (object obj = null)
		{
			isStopping = false;
			socket = new USocket (host, port);
			#if UNITY_EDITOR
			Debug.Log ("connect ==" + host + ":" + port);
			#endif
			socket.connectAsync (connectCallback, outofLine);
		}

		public void connectCallback (USocket s, object result)
		{
			if (this.socket == null || (this.socket != null && !this.socket.Equals (s))) {
				return;
			}
			if ((bool)result) {
				//connectCallback
				#if UNITY_EDITOR
				Debug.Log ("connectCallback    success");
				#endif
				connected = true;
				reConnectTimes = 0;
				if (mDispatcher != null) {
					mDispatcher (CONST_Connect, this);
				}
				socket.ReceiveAsync (onReceive);
			} else {
				Debug.LogWarning ("connectCallback    fail" + host + ":" + port + "," + isStopping);
				connected = false;
				if (!isStopping) {
					outofNetConnect ();
				}
			}
		}

		void outofNetConnect ()
		{
			if (isStopping)
				return;
			if (reConnectTimes < MaxReConnectTimes) {
				reConnectTimes++;
				if (timer != null) {
					timer.Dispose ();
				}
				timer = TimerEx.schedule (connect, null, 5000);
			} else {
				if (timer != null) {
					timer.Dispose ();
				}
				timer = null;
				outofLine (socket, null);
			}
		}

		public void stop ()
		{
			isStopping = true;
			connected = false;
			if (socket != null) {
				socket.close ();
			}
			socket = null;
		}

		void outofLine (USocket s, object obj)
		{
			if (this.socket == null || (this.socket != null && !this.socket.Equals (s))) {
				return;
			}
			if (!isStopping) {
				CLMainBase.self.onOffline ();
				try {
					if (mDispatcher != null)
						mDispatcher (CONST_OutofNetConnect, this);
				} catch (System.Exception e1) {
					Debug.Log (e1);
				}
			}
		}

		//==========================================
		public void send (object obj)
		{
			if (socket == null) {
				Debug.LogWarning ("Socket is null");
				return;
			}
			object ret = packMessage (obj);

			if (ret == null || isStopping || !connected) {
				return;
			}
			socket.SendAsync (ret as byte[]);
		}

		public object packMessage (object obj)
		{
			try {
				if (msgPackAndSendFunc != null) {
					if (msgPackAndSendFunc is TcpPackMessageAndSendFunc) {
						((TcpPackMessageAndSendFunc)msgPackAndSendFunc) (obj, this);
					} else {
						Utl.doCallback (msgPackAndSendFunc, obj, this);
					}
					return null;
				} else {
					return defaultPackMessage (obj);
				}
			} catch (System.Exception e) {
				Debug.LogError (e);
				return null;
			}
		}

		MemoryStream os = new MemoryStream ();
		MemoryStream os2 = new MemoryStream ();

		public byte[] defaultPackMessage (object obj)
		{
			os.Position = 0;
			os2.Position = 0;

			B2OutputStream.writeObject (os, obj);
			int len = (int)os.Position;
			B2OutputStream.writeInt (os2, len);
			os2.Write (os.ToArray (), 0, len);
			int pos = (int)os2.Position;
			byte[] result = new byte[pos];
			os2.Position = 0;
			os2.Read (result, 0, pos);
			return result;
		}

		//==========================================
		void onReceive (USocket s, object obj)
		{
			if (this.socket == null || (this.socket != null && !this.socket.Equals (s))) {
				return;
			}
			try {
				unpackMsg (s, s.mBuffer, mDispatcher);
			} catch (System.Exception e) {
				Debug.Log (e);
			}
		}

		public void unpackMsg (USocket s, MemoryStream buffer, TcpDispatchCallback dispatcher)
		{
            try
            {
                bool isLoop = true;
                while (isLoop)
                {
                    long totalLen = buffer.Position;
                    if (totalLen > 2)
                    {
                        buffer.SetLength(totalLen);
                        object o = null;
                        buffer.Position = 0;
                        if (msgUnpackFunc != null)
                        {
                            object[] objs = Utl.doCallback(msgUnpackFunc, s, buffer);
                            if (objs != null && objs.Length > 0)
                            {
                                o = objs[0];
                            }
                        }
                        else
                        {
                            o = defaultUnpackMsg(s, buffer);
                        }
                        if (o != null && dispatcher != null)
                        {
                            dispatcher(o, this);
                        }
                        long usedLen = buffer.Position;
                        if (usedLen > 0)
                        {
                            long leftLen = totalLen - usedLen;
                            if (leftLen > 0)
                            {
                                byte[] lessBuff = new byte[leftLen];
                                buffer.Read(lessBuff, 0, (int)leftLen);
                                buffer.Position = 0;
                                buffer.Write(lessBuff, 0, (int)leftLen);
                                buffer.SetLength((int)leftLen);
                            }
                            else
                            {
                                buffer.Position = 0;
                                buffer.SetLength(0);
                                isLoop = false;
                            }
                        }
                        else
                        {
                            buffer.Position = totalLen;
                            isLoop = false;
                        }
                    }
                    else
                    {
                        isLoop = false;
                    }
                }
            } catch(System.Exception e) {
                Debug.LogError(e);
            }
		}

		private object defaultUnpackMsg (USocket s, MemoryStream buffer)
		{
			object ret = null;
			long oldPos = buffer.Position;
			long tatalLen = buffer.Length;
			long needLen = B2InputStream.readInt (buffer);
			if (needLen <= 0 || needLen > __maxLen) {
				// 网络Number据错误。断isOpen网络
				outofLine (s, false);
				s.close ();
				return null;
			}
			long usedLen = buffer.Position;
			if (usedLen + needLen <= tatalLen) {
				ret = B2InputStream.readObject (buffer);
			} else {
				//说明长度不够
				buffer.Position = oldPos;
			}
			return ret;
		}
		//		private void defaultUnpackMsg (USocket s, MemoryStream buffer, TcpDispatchCallback dispatcher)
		//		{
		//			bool isLoop = true;
		//			while (isLoop) {
		//				long currentPostion = buffer.Position;
		//				if (currentPostion > 4) {
		//					buffer.Position = 0;
		//					long len = B2InputStream.readInt (buffer);
		//					if (len <= 0 || len > __maxLen) {
		//						// 网络Number据错误。断isOpen网络
		//						outofLine (s, false);
		//						s.close ();
		//						isLoop = false;
		//					} else {
		//						long cp2 = buffer.Position;
		//						if (cp2 + len <= currentPostion) {
		//							object o = B2InputStream.readObject (buffer);
		//							if (dispatcher != null) {
		//								dispatcher (o, this);
		//							}
		//							long cp3 = buffer.Position;
		//							long less = currentPostion - cp3;
		//							if (less > 0) {
		//								byte[] lessBuff = new byte[less];
		//								buffer.Read (lessBuff, 0, (int)less);
		//								buffer.Position = 0;
		//								buffer.Write (lessBuff, 0, (int)less);
		//							} else {
		//								buffer.Position = 0;
		//								isLoop = false;
		//							}
		//						} else {
		//							buffer.Position = currentPostion;
		//							isLoop = false;
		//						}
		//					}
		//				} else {
		//					isLoop = false;
		//				}
		//			}
		//		}
	}
}
