using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace Coolape
{
	public class CLBehaviourWithUpdate4Lua : CLBehaviour4Lua
	{
		public LuaFunction flUpdate = null;
		public LuaFunction flLateUpdate = null;
		public LuaFunction flFixedUpdate = null;
		public override void initGetLuaFunc ()
		{
			base.initGetLuaFunc ();
			flUpdate = getLuaFunction ("Update");
			flLateUpdate = getLuaFunction ("LateUpdate");
			flFixedUpdate = getLuaFunction ("FixedUpdate");
		}

		public override void clean ()
		{
			canFixedInvoke = false;
			cancelFixedInvoke4Lua (null);
		}

		public  virtual void LateUpdate ()
		{
			if (flLateUpdate != null) {
				flLateUpdate.Call (gameObject);
			}
		}

		//================================================
		// Fixed invoke 4 lua
		//================================================
		bool _doFixedUpdate = false;

		public bool canFixedInvoke {
			get {
				return _doFixedUpdate;
			}
			set {
				_doFixedUpdate = value;
				if (value) {
					if (fixedInvokeMap == null) {
						fixedInvokeMap = Hashtable.Synchronized (_fixedInvokeMap);
					}
				}
				if (!_doFixedUpdate) {
					frameCounter = 0;
				}
			}
		}
		//	public Dictionary<long, List<LuaFunction>> fixedInvokeMap = new Dictionary<long, List<LuaFunction>> ();
		Hashtable _fixedInvokeMap = new Hashtable ();
		public Hashtable fixedInvokeMap = null;

		public void fixedInvoke4Lua (object luaFunc, float waitSec)
		{
			fixedInvoke (luaFunc, null, waitSec);
		}

		public void fixedInvoke4Lua (object luaFunc, object paras, float waitSec)
		{
			fixedInvoke (luaFunc, paras, waitSec);
		}

		public void fixedInvoke (object callback, object paras, float waitSec)
		{
			if (fixedInvokeMap == null) {
				fixedInvokeMap = Hashtable.Synchronized (_fixedInvokeMap);
			}
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
					if (callback is LuaFunction) {
						((LuaFunction)callback).Call (content [1]);
					} else if (callback is string) {
						LuaFunction func = getLuaFunction (callback.ToString ());
						func.Call (content [1]);
					} else if (callback is Callback) {
						((Callback)callback) (content [1]);
					}
				}
				funcList.Clear ();
				funcList = null;
				fixedInvokeMap.Remove (key);
			}
		}

		//================================================
		// FixedUpdate
		//================================================
		public long frameCounter = 0;
		//帧统计
		public virtual void FixedUpdate ()
		{
			if (flFixedUpdate != null) {
				flFixedUpdate.Call (gameObject);
			}
			if (canFixedInvoke) {
				frameCounter++;
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

		public void invokeByUpdate (object callbakFunc, float sec)
		{
			invokeByUpdate (callbakFunc, null, sec);
		}

		/// <summary>
		/// Invoke4s the lua.
		/// </summary>
		/// <param name="callbakFunc">Callbak func.lua函数</param>
		/// <param name="orgs">Orgs.参数</param>
		/// <param name="sec">Sec.等待时间</param>
		public void invokeByUpdate (object callbakFunc, object orgs, float sec)
		{
			if (callbakFunc == null)
				return;
			NewList list = ObjPool.listPool.borrowObject ();
			list.add (callbakFunc);
			list.add (orgs);
			list.add (Time.unscaledTime + sec);
			invokeByUpdateList.Add (list);
		}

		public void cancelInvokeByUpdate ()
		{
			cancelInvokeByUpdate (null);
		}

		public void cancelInvokeByUpdate (object callbakFunc)
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
				list = (invokeByUpdateList [index]) as NewList;
				if (list == null)
					continue;
				callbakFunc = list [0];
				orgs = list [1];
				sec = (float)(list [2]);
				if (sec <= Time.unscaledTime) {
					if (callbakFunc is string) {
						func = getLuaFunction (callbakFunc.ToString ());
						Utl.doCallback (func, orgs);
					} else if (callbakFunc is LuaFunction) {
						func = (LuaFunction)callbakFunc;
						Utl.doCallback (func, orgs);
					} else if (callbakFunc is Callback) {
						((Callback)callbakFunc) (orgs);
					}
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
			if (flUpdate != null) {
				flUpdate.Call (gameObject);
			}
			if (invokeByUpdateList.Count > 0) {
				doInvokeByUpdate ();
			}
		}
	}
}
