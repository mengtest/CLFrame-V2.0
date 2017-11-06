using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace Coolape
{
	public class InvokeEx : MonoBehaviour
	{

		public static InvokeEx self;

		public InvokeEx ()
		{
			self = this;
		}

		public long frameCounter = 0;
		//================================================
		// Fixed invoke 4 lua
		//================================================
		Hashtable _fixedInvokeMap = new Hashtable ();

		public Hashtable fixedInvokeMap {
			get {
				if (_fixedInvokeMap == null) {
					_fixedInvokeMap = Hashtable.Synchronized (new Hashtable ());
				}
				return _fixedInvokeMap;
			}
		}

		public static void invokeByFixedUpdate (object luaFunc, float waitSec)
		{
			if (self == null) {
				Debug.LogError ("Must attach InvokeEx on some gameObject!");
				return;
			}
			self._fixedInvoke (luaFunc, null, waitSec);
		}

		public static void invokeByFixedUpdate (object luaFunc, object paras, float waitSec)
		{
			if (self == null) {
				Debug.LogError ("Must attach InvokeEx on some gameObject!");
				return;
			}
			self._fixedInvoke (luaFunc, paras, waitSec);
		}

		public void fixedInvoke4Lua (object luaFunc, float waitSec)
		{
			_fixedInvoke (luaFunc, null, waitSec);
		}

		public void fixedInvoke4Lua (object luaFunc, object paras, float waitSec)
		{
			_fixedInvoke (luaFunc, paras, waitSec);
		}

		void _fixedInvoke (object callback, object paras, float waitSec)
		{
			int waiteFrame = Mathf.CeilToInt (waitSec / Time.fixedDeltaTime);
			waiteFrame = waiteFrame <= 0 ? 1 : waiteFrame; //至少有帧
			long key = frameCounter + waiteFrame; 
			object[] content = new object[2];
			//		print (waiteFrame + "===" + key +"====" + luaFunc);
			List<object[]> funcList = (List<object[]>)(fixedInvokeMap [key]);
			if (funcList == null) {
				funcList = new List<object[]> ();
			}
			content [0] = callback;
			content [1] = paras;
			funcList.Add (content);
			fixedInvokeMap [key] = funcList;
		}

		public static void cancelInvokeByFixedUpdate ()
		{
			cancelInvokeByFixedUpdate (null);
		}

		public static void cancelInvokeByFixedUpdate (object func)
		{
			self.cancelFixedInvoke4Lua (func);
		}

		public void cancelFixedInvoke4Lua ()
		{
			cancelFixedInvoke4Lua (null);
		}

		public void cancelFixedInvoke4Lua (object func)
		{
			if (func == null) {
				if (fixedInvokeMap != null) {
					fixedInvokeMap.Clear ();
				}
				return;
			}
			List<object[]> list = null;
			int count = 0;
			object[] content = null;
			foreach (DictionaryEntry item in fixedInvokeMap) {
				list = (List<object[]>)(item.Value);
				count = list.Count;
				for (int i = count - 1; i >= 0; i--) {
					content = list [i];
					if (func.Equals (content [0])) {
						list.RemoveAt (i);
					}
				}
			}
		}

		void doFixedInvoke (long key)
		{
			if (fixedInvokeMap == null && fixedInvokeMap.Count <= 0)
				return;
			object[] content = null;
			List<object[]> funcList = (List<object[]>)(fixedInvokeMap [key]);
			object callback = null;
			if (funcList != null) {
				for (int i = 0; i < funcList.Count; i++) {
					content = funcList [i];
					callback = content [0];
					Utl.doCallback (callback, content [1]);
				}
				funcList.Clear ();
				funcList = null;
				fixedInvokeMap.Remove (key);
			}
		}

		//================================================
		// FixedUpdate
		//================================================
		//帧统计
		public virtual void FixedUpdate ()
		{
			frameCounter++;
			if (fixedInvokeMap != null && fixedInvokeMap.Count > 0) {
				doFixedInvoke (frameCounter);
			}
		}

		//================================================
		// Update
		//================================================
		ArrayList _invokeByUpdateList = null;

		ArrayList invokeByUpdateList {
			get {
				if (_invokeByUpdateList == null) {
					_invokeByUpdateList = ArrayList.Synchronized (new ArrayList ());
				}
				return _invokeByUpdateList;
			}
		}

		/// <summary>
		/// Invoke4s the lua.
		/// </summary>
		/// <param name="callbakFunc">Callbak func.lua函数</param>
		/// <param name="orgs">Orgs.参数</param>
		/// <param name="sec">Sec.等待时间</param>
		public static void invokeByUpdate (object callbakFunc, float sec)
		{
			self.updateInvoke (callbakFunc, sec);
		}

		public static void invokeByUpdate (object callbakFunc, object orgs, float sec)
		{
			self.updateInvoke (callbakFunc, orgs, sec);
		}

		public void updateInvoke (object callbakFunc, float sec)
		{
			updateInvoke (callbakFunc, null, sec);
		}

		public void updateInvoke (object callbakFunc, object orgs, float sec)
		{
			if (callbakFunc == null)
				return;
			NewList list = ObjPool.listPool.borrowObject ();
			list.add (callbakFunc);
			list.add (orgs);
			list.add (Time.unscaledTime + sec);
			invokeByUpdateList.Add (list);
		}

		public static void cancelInvokeByUpdate ()
		{
			self.cancelUpdateInvoke ();
		}

		public static void cancelInvokeByUpdate (object callbakFunc)
		{
			self.cancelUpdateInvoke (callbakFunc);
		}

		public void cancelUpdateInvoke ()
		{
			cancelUpdateInvoke (null);
		}

		public void cancelUpdateInvoke (object callbakFunc)
		{
			NewList list = null;
			int count = invokeByUpdateList.Count;
			if (callbakFunc == null) {
				for (int i = 0; i < count; i++) {
					list = (NewList)(invokeByUpdateList [i]);
					ObjPool.listPool.returnObject (list);
				}
				list = null;
				invokeByUpdateList.Clear ();
				return;
			}
			for (int i = count - 1; i >= 0; i--) {
				list = (NewList)(invokeByUpdateList [i]);
				if (callbakFunc.Equals (list [0])) {
					invokeByUpdateList.RemoveAt (i);
					ObjPool.listPool.returnObject (list);
				}
			}
			list = null;
		}

		void doInvokeByUpdate ()
		{
			int count = invokeByUpdateList.Count;
			NewList list = null;
			object callbakFunc;
			object orgs;
			float sec;
			int index = 0;
			LuaFunction func = null;
			while (index < invokeByUpdateList.Count) {
				list = (NewList)(invokeByUpdateList [index]);
				callbakFunc = list [0];
				orgs = list [1];
				sec = (float)(list [2]);
				if (sec <= Time.unscaledTime) {
					Utl.doCallback (callbakFunc, orgs);
					invokeByUpdateList.RemoveAt (index);
					ObjPool.listPool.returnObject (list);
				} else {
					index++;
				}
			}
			list = null;
		}

		public virtual void Update ()
		{
			if (invokeByUpdateList.Count > 0) {
				doInvokeByUpdate ();
			}
		}
	}
}
