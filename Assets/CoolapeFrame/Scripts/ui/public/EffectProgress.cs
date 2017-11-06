/*
******************************************************************************** 
  *Copyright(C),coolae.net 
  *Author:  canyou
  *Version:  2.0 
  *Date:  2017-01-09
  *Description:   进度条动态显示效果，比如一个slider的值是0，当设置成1时，有一个从0增到1的过程
  *Others:  
  *History:
*********************************************************************************
*/ 
using UnityEngine;
using System.Collections;
using System;

namespace Coolape
{
	[RequireComponent (typeof(UISlider))]
	public class EffectProgress : MonoBehaviour
	{

		UISlider _slider;

		public UISlider slider {
			get {
				if (_slider == null) {
					_slider = gameObject.GetComponent<UISlider> ();
				}
				return _slider;
			}
		}

		public bool isGui = false;
		public AnimationCurve speedCurve = new AnimationCurve (new Keyframe (0f, 0f, 0f, 1f), new Keyframe (1f, 1f, 1f, 0f));
		private float timeAdd = 0.02f;
		private float timeVal = 0f;
		private float fromVal = 0;
		private float toVal = 0;
		float diffVal = 0;
		public float speed = 1;
		private float etime = 0.02f;
		//每次执行时间
		private bool isFirst = true;
		// 首次执行
		private float firstTime = 0.15f;
		//首次执行等待时间

		object callFun;

		// Use this for initialization
	
		// Update is called once per frame
		//	void Update () {
		//
		//	}
		
		void changeNum (float vCurve)
		{
			float val = fromVal + vCurve * diffVal;
			slider.value = val;
		}

		float timeGet {
			get {
				timeVal += timeAdd;
				return timeVal;
			}
		}

		IEnumerator effect ()
		{
			if (isFirst) {
				isFirst = false;
				slider.value = fromVal;
				yield return new WaitForSeconds (firstTime);
			}
			float timeCount = timeGet;
			if (timeCount >= 1) {
				timeCount = 1;
			}
			float vCurve = speedCurve.Evaluate (timeCount) * speed;
			changeNum (vCurve);

			if (timeCount < 1) {
				yield return new WaitForSeconds (etime);
				StartCoroutine (effect ());
			} else {
				StopCoroutine (effect ());
				doCallback ();
			}
			yield return null;
		}

		void doCallback ()
		{
			Utl.doCallback (callFun, this);
		}

		public void effectStart (float to, object back, float delayTime = 0)
		{
			effectStart (slider.value, to, back, delayTime);
		}

		public void effectStart (float from, float to, object back, float delayTime = 0)
		{
			fromVal = from;
			timeVal = 0f;
			isFirst = true;
			callFun = back;
		
			slider.value = fromVal;
			toVal = to;
			diffVal = toVal - fromVal;
			CancelInvoke ("doEffect");
			Invoke ("doEffect", delayTime);
		}

		void doEffect ()
		{
			StopAllCoroutines ();
			StartCoroutine (effect ());
		}

		void OnDisable ()
		{
			StopAllCoroutines ();
			CancelInvoke ();
		}

		#if UNITY_EDITOR
		void OnGUI ()
		{
			if (isGui) {
				Callback cb = backFunTest;
				bool is1 = GUI.Button (new Rect (20, 10, 200, 30), "10000to100");
				if (is1) {
					effectStart (0, 1, cb);
				}
			}
		}

		void backFunTest (params object[] objs)
		{
			Debug.LogWarning ("timeVal:" + timeVal);
		}
		#endif
	}
}