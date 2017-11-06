package com.coolape.pushmsg;

import com.coolape.cfg.CBCfg;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.util.Log;

/**
 * 接入器，需要在AndroidManifest.xml中增加如下内容
 * 
 * <uses-permission android:name="android.permission.RECEIVE_BOOT_COMPLETED" />
 * 
 * <receiver android:name=".Receiver" >你的receiver <intent-filter> <action
 * android:name="android.intent.action.BOOT_COMPLETED" /> <action
 * android:name="android.intent.action.CLOSE_SYSTEM_DIALOGS" /> <action
 * android:name="android.intent.action.USER_PRESENT" /> <action
 * android:name="com.googl.YouWillNeverKillMe" /></intent-filter> </receiver>
 * 
 * @author niko
 * 
 */
public class CBPushReceiver extends BroadcastReceiver {

	public static final String BOOT_COMPLETED = "android.intent.action.BOOT_COMPLETED"; // 开机广播
	public static final String ClOSE_SYSTEM_ACTION = Intent.ACTION_CLOSE_SYSTEM_DIALOGS;// 关闭系统dialog的广播，每次点击home键的时候都会发出
	public static final String USER_PRESENT = "android.intent.action.USER_PRESENT";// 监听屏幕解锁事件
	public static final String YouWillNeverKillMe = "com.googl.YouWillNeverKillMe";// YouWillNeverKillMe

	@Override
	public void onReceive(Context context, Intent intent) {
		Log.i("CBPushReceiver", "onReceive====" + intent.getAction());
		// TODO Auto-generated method stub
		if (intent.getAction().equals(BOOT_COMPLETED)
				|| intent.getAction().equals(ClOSE_SYSTEM_ACTION)
				|| intent.getAction().equals(USER_PRESENT)
				|| intent.getAction().equals(YouWillNeverKillMe)) {

			Log.i("CBPushReceiver", "onReceive");
			// CBPushServer.init("192.168.2.184", 26666, "1",
			// "com.cb.pushmsg.ShowActivity");
//			context.startService(new Intent(CBCfg.pushSericeClass));
			context.startService(new Intent(context, CBPushServer.class));
		}

	}
	

}
