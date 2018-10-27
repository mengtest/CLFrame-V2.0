/*
******************************************************************************** 
  *Copyright(C),coolae.net 
  *Author:  chenbin
  *Version:  2.0 
  *Date:  2017-01-09
  *Description:  socke,封装c# socketNumber据传输协议
  *Others:  
  *History:
*********************************************************************************
*/ 
using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Text;
using System.Threading;

namespace Coolape
{
	public delegate void NetCallback (USocket socket, object obj);
	//	public delegate void NetCallbackRobot (object obj, int code);

	public class USocket
	{
		public string host;
		public int port;
		public Socket mSocket;
		public int mTmpBufferSize;
		public byte[] mTmpBuffer;
		public MemoryStream mBuffer;
		private IPEndPoint ipe;
		NetCallback connectCallbackFunc;
		NetCallback OnReceiveCallback;
		private int failTimes = 0;
		public bool isActive = false;
		public Timer timeoutCheckTimer;

		public USocket (string ihost, int iport)
		{
			host = ihost;
			port = iport;
			IPAddress ip = null;
			try {
				// is ip xx.xx.xx.xx
				ip = IPAddress.Parse (host);
			} catch (Exception e) {
				// may be is dns
				try {
					IPAddress[] address = Dns.GetHostAddresses (host);
					if (address.Length > 0) {
						ip = address [0];
					}
				} catch (Exception e2) {
					Debug.LogError (e2);
					return;
				}
			}
			if (ip == null) {
				Debug.LogError ("Get ip is null");
				return;
			}
			ipe = new IPEndPoint (ip, port);

			if (ip.AddressFamily == AddressFamily.InterNetworkV6) {
				Debug.LogWarning ("is ipv6");
				mSocket = new Socket (AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
			} else {
				Debug.LogWarning ("is ipv4");
				mSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			}

			mTmpBufferSize = 1024;
			mTmpBuffer = new byte[mTmpBufferSize];
			mBuffer = new MemoryStream ();
		}

		public void connect ()
		{
			try {
				mSocket.Connect (ipe);
			} catch (Exception e) {
				Debug.Log (e);
			}
		}

		// 异步模式//////////////////////////////////////////////////////////////////
		// 异步模式//////////////////////////////////////////////////////////////////
		public  bool IsConnectionSuccessful = false;
		public  int timeoutMSec = 10000;
        public int maxTimeoutTimes = 3;
        //毫秒
        //public ManualResetEvent TimeoutObject = null;
        Timer connectTimeout = null;
		NetCallback offLineCallback;

		public void connectAsync (NetCallback callback, NetCallback offLineCallback)
		{
			if (ipe == null) {
				callback (this, false);
				return;
			}
			this.offLineCallback = offLineCallback;
			IsConnectionSuccessful = false;
			connectCallbackFunc = callback;

			mSocket.BeginConnect (ipe, (AsyncCallback)connectCallback, this);
            if(connectTimeout != null) {
                connectTimeout.Dispose();
                connectTimeout = null;
            }
            connectTimeout = TimerEx.schedule((TimerCallback)connectTimeOut, null, timeoutMSec);
        }

        void connectTimeOut(object orgs)
        {
            if (IsConnectionSuccessful) {
            } else {
                connectCallbackFunc(this, false);
            }
        }

        public void close ()
		{
			if (mSocket != null)
				mSocket.Close ();
			isActive = false;
			//mSocket = null;
		}

		private void connectCallback (IAsyncResult ar)
		{
			// 从stateobject获取socket.
			USocket client = (USocket)ar.AsyncState;
			try {
				if (client.mSocket.Connected) {
					// 完成连接.
					client.mSocket.EndConnect (ar);
					client.IsConnectionSuccessful = true;
					client.isActive = true;

					// isOpen始接Number据
					// client.ReceiveAsync ();
				
					client.connectCallbackFunc (client, true);
					client.failTimes = 0;
				} else {
					client.connectCallbackFunc (client, false);
					client.close ();
				}
			} catch (Exception e) {
				client.IsConnectionSuccessful = false;
				Debug.Log ("connect faile:" + e);
				client.failTimes++;
				client.connectCallbackFunc (client, false);
				client.close ();
			} finally {
                if(connectTimeout != null) {
                    connectTimeout.Dispose();
                    connectTimeout = null;
                }
			}
		}

		public void ReceiveAsync (NetCallback callback)
		{
			OnReceiveCallback = callback;
			// 从远程target接收Number据.
			this.mSocket.BeginReceive (mTmpBuffer, 0, mTmpBufferSize, 0,
				(AsyncCallback)ReceiveCallback, this);
		}

		private void ReceiveCallback (IAsyncResult ar)
		{
			USocket client = (USocket)ar.AsyncState;
			try {
				if (client.timeoutCheckTimer != null) {
					client.timeoutCheckTimer.Dispose ();
					client.timeoutCheckTimer = null;
				}
				if (client.isActive)
                {
                    client.failTimes = 0;
                    //从远程设备读取Number据
                    int bytesRead = client.mSocket.EndReceive (ar);
					if (bytesRead > 0) {
//					Debug.Log ("receive len==" + bytesRead);
						// 有Number据，存储.
						client.mBuffer.Write (client.mTmpBuffer, 0, bytesRead);
						OnReceiveCallback (client, client.mBuffer);
					} else if (bytesRead < 0) {
						if (client.offLineCallback != null) {
							client.offLineCallback (client, null);
						}
						client.connectCallbackFunc (client, false);
						client.close ();
					} else {
						// 所有Number据读取完毕.
						Debug.Log ("receive zero=====" + bytesRead);
						if (client.offLineCallback != null) {
							client.offLineCallback (client, null);
						}
						client.connectCallbackFunc (client, false);
						client.close ();
						return;
					}

					// 继续读取.
					if (client.mSocket.Connected) {
						client.mSocket.BeginReceive (client.mTmpBuffer, 0, client.mTmpBufferSize, 0,
							(AsyncCallback)ReceiveCallback, client);
					}
				} else {
					if (client.offLineCallback != null) {
						client.offLineCallback (client, null);
					}
					client.connectCallbackFunc (client, false);
					client.close ();
				}
			} catch (Exception e) {
				if (client.offLineCallback != null) {
					client.offLineCallback (client, null);
				}
				client.connectCallbackFunc (client, false);
				client.close ();
				Debug.Log (e);
			}
		}

		public void SendAsync (byte[] data)
		{
			try {
				if (data == null)
					return;
				// isOpen始发送Number据到远程设备.
				if (this.timeoutCheckTimer == null) {
					this.timeoutCheckTimer = TimerEx.schedule ((TimerCallback)sendTimeOut, null, timeoutMSec);
				}
				mSocket.BeginSend (data, 0, data.Length, 0,
					(AsyncCallback)SendCallback, this);
			} catch (System.Exception e) {
				Debug.LogError ("socket:" + e);
				if (offLineCallback != null) {
					offLineCallback (this, null);
				}
				close ();
			}
		}

		public void sendTimeOut (object orgs)
		{
            failTimes++;
            if (failTimes > maxTimeoutTimes)
            {
                if (offLineCallback != null)
                {
                    offLineCallback(this, null);
                }
                close();
            }
		}

		private void SendCallback (IAsyncResult ar)
		{
			USocket client = (USocket)ar.AsyncState;
			// 完成Number据发送.
			int bytesSent = client.mSocket.EndSend (ar);
			if (bytesSent <= 0) { //发送失败
				if (client.offLineCallback != null) {
					client.offLineCallback (client, null);
				}
				client.close ();
			}
			client.failTimes = 0;
		}

	}
}