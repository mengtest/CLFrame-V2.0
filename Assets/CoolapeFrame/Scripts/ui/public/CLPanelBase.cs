/*
******************************************************************************** 
  *Copyright(C),coolae.net 
  *Author:  chenbin
  *Version:  2.0 
  *Date:  2017-01-09
  *Description:   页面基类
  *Others:  
  *History:
*********************************************************************************
*/ 

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Coolape
{
	public abstract class CLPanelBase : CLBaseLua
	{
		//	[HideInInspector]
		bool _isActive = false;

		public bool isActive {
			get {
				return _isActive;
			}
			set {
				_isActive = value;
			}
		}

		UIPanel _panel = null;

		public UIPanel panel {
			get {
				if (_panel == null) {
					_panel = gameObject.GetComponent<UIPanel>();
					if (_panel == null) {
						_panel = gameObject.AddComponent<UIPanel>();
					}
				}
				return _panel;
			}
		}

		public bool isNeedBackplate = true;
		//是否需要遮罩
		public bool destroyWhenHide = false;
		public bool isNeedResetAtlase = true;
		[HideInInspector]
		public bool isNeedMask4Init = false;
		[HideInInspector]
		public bool isNeedMask4InitOnlyOnce = true;
		public bool isHideWithEffect = true;
		public bool isRefeshContentWhenEffectFinish = false;
		public Transform EffectRoot;
		//特效root节点
		public enum EffectType
		{
			synchronized,
			//同步执行
			ordered,
			//顺序执行
		}

		public EffectType effectType = EffectType.ordered;
		//特效类型
		public List<UITweener> EffectList = new List<UITweener>();

		static public int SortByName(UITweener a, UITweener b)
		{
			return string.Compare(a.name, b.name);
		}

		bool isFinishStart = false;

		public virtual void Start()
		{
			if (isFinishStart)
				return;

			getSubPanelsDepth ();
			UITweener[] tws = null;
			if (EffectRoot != null) {
				tws = EffectRoot.GetComponents<UITweener>();
				sortTweeners(tws);
				for (int j = 0; j < tws.Length; j++) {
					if (tws [j] != null) {
						tws [j].ResetToBeginning();
						tws [j].enabled = false;
						EffectList.Add(tws [j]);
					}
				}
				for (int i = 0; i < EffectRoot.childCount; i++) {
					tws = EffectRoot.GetChild(i).GetComponents<UITweener>();
					sortTweeners(tws);
					for (int j = 0; j < tws.Length; j++) {
						if (tws [j] != null) {
							tws [j].ResetToBeginning();
							tws [j].enabled = false;
							EffectList.Add(tws [j]);
						}
					}
				}
				EffectList.Sort(SortByName);
			}
		
			for (int i = EffectList.Count - 1; i >= 0; i--) {
				if (EffectList [i] == null) {
					EffectList.RemoveAt(i);
				} else {
					EffectList [i].ResetToBeginning();
				}
			}
			isFinishStart = true;
		}

		/// <summary>
		/// 冒泡排序法1
		/// </summary>
		/// <param name="list"></param>
		public void sortTweeners(UITweener[] list)
		{
			UITweener temp = null;
			for (int i = 0; i < list.Length; i++) {
				for (int j = i; j < list.Length; j++) {
					if (list [i].exeOrder < list [j].exeOrder) {
						temp = list [i];
						list [i] = list [j];
						list [j] = temp;
					}
				}
			}
		}

		object finishShowingCallback = null;

		public void showWithEffect(object finishShowingCallback = null)
		{
			this.finishShowingCallback = finishShowingCallback;
			isActive = true;
			isMoveOut = false;
			if (!gameObject.activeSelf) {
				NGUITools.SetActive(gameObject, true);
				CLPanelManager.showingPanels [gameObject.name] = this;
			}
			if (!isRefeshContentWhenEffectFinish) {
				callFinishShowingCallback();
			}
			playEffect(true);
		}

		void callFinishShowingCallback()
		{
			Utl.doCallback(finishShowingCallback, this);
		}

		/// <summary>
		/// Plaies the effect.
		/// 播放ui特效
		/// </summary>
		/// <param name='forward'>
		/// Forward.
		/// </param>
		int EffectIndex = 0;
		bool EffectForward = true;

		void playEffect(bool forward)
		{
			if (!isFinishStart) {
				Start();
			}
			if (EffectList.Count <= 0) {
				if (!forward) {
					onFinishHideWithEffect();
				}
				return;
			}
			if (forward) {
				EffectIndex = 0;
			} else {
				EffectIndex = EffectList.Count - 1;
			}
		
			EffectForward = forward;
			UITweener tw = null;
		
			if (effectType == EffectType.synchronized) {
				for (int i = 0; i < EffectList.Count; i++) {
					tw = EffectList [i];
					if (forward && !isHideWithEffect) {
						tw.ResetToBeginning();
					}
				
					if (!forward && i == EffectList.Count - 1) {
						tw.callWhenFinished = "onFinishHideWithEffect";
						tw.eventReceiver = gameObject;
					} else {
						tw.callWhenFinished = "";
					}
					tw.Play(forward);
				}
			} else {
				tw = EffectList [EffectIndex];
				if (forward && !isHideWithEffect) {
					tw.ResetToBeginning();
				}
				tw.eventReceiver = gameObject;
				tw.callWhenFinished = "onFinishEffect";
				tw.Play(forward);
			}
		}

		void onFinishEffect(UITweener tweener)
		{
			tweener.callWhenFinished = "";
			tweener.eventReceiver = gameObject;
		
			if (EffectForward) {
				EffectIndex++;
			} else {
				EffectIndex--;
			}
			if (EffectIndex < 0) {
				if (!EffectForward) {
					onFinishHideWithEffect();
				}
			} else if (EffectIndex >= EffectList.Count) {
				if (isRefeshContentWhenEffectFinish) {
					callFinishShowingCallback();
				}
			} else {
				UITweener tw = EffectList [EffectIndex];
				if (EffectForward && !isHideWithEffect) {
					tw.ResetToBeginning();
				}
				tw.eventReceiver = gameObject;
				tw.callWhenFinished = "onFinishEffect";
				tw.Play(EffectForward);
			}
		}

		bool isMoveOut = true;

		public void hideWithEffect(bool moveOut = false)
		{
			isMoveOut = moveOut;
			if (isMoveOut) {
				Vector3 newPos = transform.localPosition;
				newPos.z = -250;
				transform.localPosition = newPos;
				playEffect(false);
			} else {
				onFinishHideWithEffect();
			}
		}

		void onFinishHideWithEffect(UITweener tweener = null)
		{
			isActive = false;
			CLPanelManager.showingPanels.Remove(gameObject.name);
			finishMoveOut();
			if (destroyWhenHide) {
				Invoke("destroySelf", 3);
			}
		}

		void finishMoveOut()
		{
			Vector3 newPos = transform.localPosition;
			newPos.z = -100;
			transform.localPosition = newPos;
			NGUITools.SetActive(gameObject, false);
		}

		void finishMoveIn(UITweener tween)
		{
			if (CLPanelManager.topPanel == this) {
				callFinishShowingCallback();
				Vector3 newPos = transform.localPosition;
				newPos.z = -200;
				transform.localPosition = newPos;
			
				if (effectType != EffectType.synchronized) {
					playEffect(true);
				}
			}
		}

		void destroySelf()
		{
			if (isActive) {
				return;
			}
			//		CLPanelManager.panelBuff [name] = null;
			//		NGUITools.Destroy (gameObject);
			CLPanelManager.destroyPanel(this);
		}

//		OnNetWorkData tmpNetWorkData = null;
//		// Update is called once per frame
//		public override void Update()
//		{
//			base.Update();
//			if (isOnNetWork) {
//				isOnNetWork = false;
//				if (networkQueue.Count > 0) {
//					tmpNetWorkData = networkQueue.Dequeue();
//					if (tmpNetWorkData != null) {
//						procNetwork(tmpNetWorkData.interfaceName, tmpNetWorkData.succ, tmpNetWorkData.msg, tmpNetWorkData.objs);
//					}
//				
//					if (tmpNetWorkData.succ != Net.SuccessCode) {
//						Debug.LogWarning("Net Failed:" + tmpNetWorkData.succ + ":" + tmpNetWorkData.interfaceName);
//					}
//				
//					if (networkQueue.Count > 0) {
//						isOnNetWork = true;
//					}
//				}
//			}
//		}
//
//		bool isOnNetWork = false;
//		[HideInInspector]
//		public static Queue<OnNetWorkData>
//			networkQueue = new Queue<OnNetWorkData>();
//
//		public class OnNetWorkData
//		{
//			public string interfaceName;
//			//接口函数的名字
//			public int succ = 0;
//			//接口的返回结果
//			public string msg = "";
//			//接口的返回结果
//			public object objs = null;
//			//接口的返回内容
//		}
//
//		/// <summary>
//		/// Ons the network.
//		/// 网络接口返回时，需要调用NPanelManager.topPanel.onNetwork()
//		/// </summary>
//		/// <param name='fname'>
//		/// Fname.
//		/// </param>
//		/// <param name='succ'>
//		/// Succ.
//		/// </param>
//		/// <param name='obj'>
//		/// Object.
//		/// </param>
//		public void onNetwork(string fname, int succ, string msg, object pars)
//		{
//			OnNetWorkData data = new CLPanelBase.OnNetWorkData();
//			data.interfaceName = fname;
//			data.succ = succ;
//			data.msg = msg;
//			data.objs = pars;
//			networkQueue.Enqueue(data);
//			isOnNetWork = true;
//		}

		public virtual void procNetwork(string fname, int succ, string msg, object pars)
		{
		}
	
		//	[HideInInspector]
		public bool
			isFinishInit = false;

		public virtual void init()
		{
			getSubPanelsDepth();
			_isActive = false;
			if (Application.isPlaying) {
				isFinishInit = true;
			}

			//		getSubPanelsDepth(); //此时页面还没有显示，通过 GetComponentsInChildren取不到
		}

		bool isFinishGetSubPanelsDepth = false;
		Hashtable subPanelsDepth = new Hashtable();

		public void getSubPanelsDepth()
		{
			if (isFinishGetSubPanelsDepth) {
				return;
			}
			isFinishGetSubPanelsDepth = true;
			UIPanel[] ps = gameObject.GetComponentsInChildren<UIPanel>();
			int count = ps.Length;
			for (int i = 0; i < count; i++) {
				if (ps [i] == panel)
					continue;
				subPanelsDepth [ps [i]] = ps [i].depth;
			}
		}

		public void setSubPanelsDepth()
		{
			foreach (DictionaryEntry cell in subPanelsDepth) {
				((UIPanel)(cell.Key)).depth = ((int)(cell.Value)) + panel.depth;
			}
		}

		public abstract void setData(object pars);

		public virtual void show(object pars)
		{
			setData(pars);
			show();
		}

		public virtual void onTopPanelChange(CLPanelBase p) {
			//TODO:
		}

		public virtual void show()
		{
			if (!isFinishInit) {
				init();
			}
			showWithEffect();
			getSubPanelsDepth();
			refresh();
			setSubPanelsDepth();
		}

		public abstract void refresh();

		/// <summary>
		/// Raises the press back event.
		/// 当点击返回键
		/// 当返回true时表明已经关闭了最顶层页面
		/// 当返回false时，表明不能关闭最顶层页面，其时可能需要弹出退程序的确认
		/// </summary>
		public virtual bool hideSelfOnKeyBack()
		{
			return false;
		}

		public virtual void hide()
		{
			hideWithEffect(isHideWithEffect);
		}

		public int depth {
			get {
				return panel.depth;
			}
			set {
				panel.depth = value;
			}
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			isFinishInit = false;
		}
	}
}
