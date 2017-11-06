/*
******************************************************************************** 
  *Copyright(C),coolae.net 
  *Author:  chenbin
  *Version:  2.0 
  *Date:  2017-01-09
  *Description:  管理代理回调，目的是为了先把回调根据某个k管理起来，然后调用时方便取得
  *Others:  
  *History:
*********************************************************************************
*/ 
using UnityEngine;
using System.Collections;

namespace Coolape
{
	public class CLDelegate
	{
		public Hashtable delegateInfro = new Hashtable ();

		public void add (string key, object callback, object orgs)
		{
			ArrayList list = MapEx.getList (delegateInfro, key);
			if (list == null) {
				list = new ArrayList ();
			}
			ArrayList infor = new ArrayList ();
			infor.Add (callback);
			infor.Add (orgs);
			list.Add (infor);
			delegateInfro [key] = list;
		}

		public void removeDelegates (string key)
		{
			if (delegateInfro [key] != null) {
				ArrayList list = MapEx.getList (delegateInfro, key);
				list.Clear ();
				list = null;
			}
			delegateInfro.Remove (key);
		}

		public ArrayList getDelegates (string key)
		{
			return MapEx.getList (delegateInfro, key);
		}
	}
}