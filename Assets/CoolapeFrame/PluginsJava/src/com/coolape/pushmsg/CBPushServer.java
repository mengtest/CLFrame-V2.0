package com.coolape.pushmsg;

import java.io.FileOutputStream;
import java.util.Date;
import java.util.ArrayList;
import java.util.Map;

import org.json.JSONObject;

import com.coolape.cfg.CBCfg;
import com.coolape.data.PushData;
import com.coolape.lang.NumEx;
import com.coolape.tool.FileHelper;

import android.annotation.SuppressLint;
import android.app.AlarmManager;
import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.Service;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.pm.PackageManager;
import android.os.Binder;
import android.os.IBinder;
import android.os.SystemClock;
import android.util.Log;

@SuppressLint("NewApi")
public class CBPushServer extends Service {
	private static String tag = "CBPushServer:";
	private NotificationManager mNM;
	// 当点击通知时，启动该contentIntent关联的activity
	// public String activityNmae = "";
	// public String pushHost;
	// public int pushPort;
	// 上次登陆时间
	// public long lastLoginTime;
	// 用户唯一码
	// public String uid;

	// 通知唯一标示，在通知开始和结束使用
	private int NOTIFICATION_ID = 0x0012;

	// private Tcp pushTcp;

	public static PushData data = null;

	// 与界面交互的类，由于service跟界面总是运行在同一程序里，所以不用处理IPC
	public class LocalBinder extends Binder {
		CBPushServer getService() {
			return CBPushServer.this;
		}
	}

	/**
	 * 初始化服务
	 * 
	 * @param host
	 *            推送服务器
	 * @param port
	 *            推送服务器
	 * @param uid
	 *            用户唯一码
	 * @param activityNmae
	 *            要启动的activity
	 */

	public static void init(String uid, String longTimeOntLoginNotifyMsg,
			String activityName, Context context) {
		init("", 0, uid, longTimeOntLoginNotifyMsg, activityName, context);
	}

	public static void init(String host, int port, String uid,
			String longTimeOntLoginNotifyMsg, String packageName,
			Context context) {

		// 数据记录到本地
		long currTime = System.currentTimeMillis();
		Log.i(tag, tag + "init" + " " + packageName);
		// refreshCfgData(host, port, uid, currTime, activityNmae);

		if (data == null) {
			data = getCfgData();
		}
		data.pushHost = host;
		data.pushPort = port;
		data.packageName = packageName;
		data.lastLoginTime = currTime;
		data.longTimeOntLoginNotifyMsg = longTimeOntLoginNotifyMsg;
		data.uid = uid;
		refreshCfgData();

		// context.startService(new Intent(CBCfg.pushSericeClass));
		CBPushReceiver pr = new CBPushReceiver();
		registerConnectReceiver(pr, context);
	}

	public static void setMsg(String msg, long fireSeconds) {
		long now = new Date().getTime();
		now += fireSeconds * 1000;
		String str = msg + "@@" + now;
		if (data == null) {
			data = getCfgData();
		}
		if (data.msg == null) {
			data.msg = new ArrayList<String>();
		}
		data.msg.add(str);
		refreshCfgData();
	}

	public static void cleanAllMsg() {
		if (data == null) {
			data = getCfgData();
		}
		if (data.msg == null) {
			data.msg = new ArrayList<String>();
		}
		data.msg.clear();
		refreshCfgData();
	}

	public static void cancelNotification(String msg) {
		try {
			if (data == null) {
				data = getCfgData();
			}
			boolean isdel = false;
			for (String m : data.msg) {
				if (m.indexOf(msg) >= 0) {
					data.msg.remove(m);
					isdel = true;
					break;
				}
			}
			if (isdel) {
				refreshCfgData();
			}
		} catch (Exception e) {
		}
	}

	public static void registerConnectReceiver(BroadcastReceiver mReciver,
			Context context) {
		try {
			IntentFilter filter = new IntentFilter();
			filter.addAction(Intent.ACTION_SCREEN_OFF);
			filter.addAction(Intent.ACTION_CLOSE_SYSTEM_DIALOGS);
			filter.addAction(Intent.ACTION_BOOT_COMPLETED);
			filter.setPriority(1000);
			context.registerReceiver(mReciver, filter);
		} catch (Throwable e) {
			e.printStackTrace();
		}
	}

	@Override
	public void onCreate() {
		mNM = (NotificationManager) getSystemService(NOTIFICATION_SERVICE);

		notifTitle = this.getApplicationInfo().name;

		// 创建定时器
		AlarmManager alarmManager = (AlarmManager) getSystemService(CBPushServer.ALARM_SERVICE);
		// 指定ChangeWallpaperService的PendingIntent对象
		PendingIntent pendingIntent = PendingIntent.getService(this, 0,
				new Intent(this, CBPushServer.class), 0);

		// 定时执行服务
		alarmManager.setRepeating(AlarmManager.ELAPSED_REALTIME_WAKEUP,
				SystemClock.elapsedRealtime() + CBCfg.Interval, CBCfg.Interval,
				pendingIntent);

	}

	// 兼容2.0以前版本
	@Override
	public void onStart(Intent intent, int startId) {

	}

	// 在2.0以后的版本如果重写了onStartCommand，那onStart将不会被调用，注：在2.0以前是没有onStartCommand方法
	@Override
	public int onStartCommand(Intent intent, int flags, int startId) {

		Log.i(tag, tag + "Received start id " + startId + ": " + intent);
		// 如果服务进程在它启动后(从onStartCommand()返回后)被kill掉, 那么让他呆在启动状态但不取传给它的intent.
		// 随后系统会重写创建service，因为在启动时，会在创建新的service时保证运行onStartCommand
		// 如果没有任何开始指令发送给service，那将得到null的intent，因此必须检查它.
		// 该方式可用在开始和在运行中任意时刻停止的情况，例如一个service执行音乐后台的重放

		doPush();

		return START_STICKY;
	}

	// 在service开始时，将icon图标放到通知任务栏
	private void showNotification() {
		if (data.packageName == null || data.packageName.equals("")) {
			return;
		}
		// Notification notification = new Notification(
		// this.getApplicationInfo().icon, notifContent,
		// System.currentTimeMillis());

		// 当点击通知时，启动该contentIntent关联的activity
		try {
			PackageManager tmxx = this.getPackageManager();
//			this.getPackageName()
			Intent intent2 = tmxx.getLaunchIntentForPackage(data.packageName);
			if(intent2 == null) {
				return;
			}
			PendingIntent notifIntent = PendingIntent.getActivity(this, 0,
					intent2, 0);

			Notification notification = new Notification.Builder(this)
					.setContentTitle(notifTitle).setContentText(notifContent)
					.setSmallIcon(this.getApplicationInfo().icon)
					.setContentIntent(notifIntent)
					.setWhen(System.currentTimeMillis())
					.setDefaults(Notification.DEFAULT_ALL).build();

			// 在通知栏上显示标题和内容
			// notification.setLatestEventInfo(this, notifTitle, notifContent,
			// notifIntent);

			mNM.notify(NOTIFICATION_ID, notification);
		} catch (Exception e) {
			e.printStackTrace();
			Log.i(tag, tag + "erro==" + e.toString());
		}
	}

	public String notifTitle = "";
	public String notifContent = "";

	/**
	 * 判断是否需要推送消息，如果需要的话还要设置如下内容： notifTitle： 消息titile notifContent： 消息的详细内容
	 */
	@SuppressWarnings("unchecked")
	public boolean getNotifInfor() {
		Log.i(tag, tag + "  getNotifInfor");
		if (data == null)
			data = getCfgData();
		notifContent = "";
		// 长时未登陆，提示用户登陆
		long currTime = System.currentTimeMillis();
		Log.i(tag, tag + "@@@@@@====" + (currTime - data.lastLoginTime)
				+ "            " + CBCfg.pushOfflineTime);
		if (data.longTimeOntLoginNotifyMsg != null
				&& !data.longTimeOntLoginNotifyMsg.isEmpty()
				&& currTime - data.lastLoginTime > CBCfg.pushOfflineTime) {
			data.lastLoginTime = currTime;
			refreshCfgData();

			notifTitle = this.getApplicationInfo().name;
			notifContent = data.longTimeOntLoginNotifyMsg;
			return true;
		}

		if (data.msg != null && data.msg.size() > 0) {
			@SuppressWarnings("rawtypes")
			ArrayList list = new ArrayList();
			long pushTime = 0;
			for (Object m : data.msg) {
				String str = m.toString();
				Log.i(tag, tag + "   " + str);
				String[] strs = str.split("@@");
				if (strs != null && strs.length > 1) {
					pushTime = NumEx.stringToLong(strs[1]);
					if (currTime >= pushTime) {
						notifContent += strs[0];
					} else {
						list.add(m);
					}
				}
			}
			if (notifContent != null && !notifContent.isEmpty()) {
				data.msg.clear();
				data.msg = list;
				refreshCfgData();
			}

		}
		if (notifContent.isEmpty())
			return false;
		else
			return true;
	}

	public static void refreshCfgData() {
		try {
			Log.i(tag, tag + "refreshCfgData");
			if (data == null)
				return;
			FileHelper fh = new FileHelper();
			if (!fh.existsSDFile(CBCfg.PushCfgDataDir)) {
				fh.creatSDDir(CBCfg.PushCfgDataDir);
			}

			FileOutputStream fos = fh.writeSDFile(CBCfg.PushCfgDataFile);

			@SuppressWarnings("unchecked")
			Map<String, Object> map = data.toMap();

			JSONObject jo = new JSONObject(map);
			fos.write(jo.toString().getBytes("UTF-8"));
			fos.close();
			Log.i(tag, tag + "refreshCfgData end");
		} catch (Exception e) {
			Log.i(tag, tag + "refreshCfgData push cfgdata faile:" + e);
		}
	}

	static Thread pushThread = null;

	public void doPush() {
		try {
			// 取得配置信息
			// if (!getCfgData())
			// return;

			// 判断连接服务器是否断开
			// checkTcp();

			// 取得推送信息
			if (getNotifInfor()) {
				Log.i(tag, tag + "取得推送信息==" + notifContent);
				showNotification();
			}
		} catch (Exception e) {

		} finally {

		}
	}

	// void checkTcp() {
	// try {
	// if (data.uid == null || data.uid.equals(""))
	// return;
	// if (pushTcp == null) {
	// if (data.pushHost != null && !data.pushHost.equals("")) {
	// pushTcp = new Tcp(data.pushHost, data.pushPort,
	// new NetDataComplateI() {
	//
	// @Override
	// public void onRecive(DataInputStream dis)
	// throws Exception {
	// if (dis == null)
	// return;
	// // int len = NumEx.readSwappedInteger(dis);
	// int len = dis.readInt();
	// byte[] buff2 = new byte[len];
	// dis.readFully(buff2);
	// notifContent = new String(buff2);
	// showNotification();
	// }
	// });
	// }
	// }
	//
	// if (pushThread == null) {
	// pushThread = new Thread(pushTcp);
	// }
	// if (pushThread != null && !pushThread.isAlive()) {
	// pushThread.start();
	// }
	// Map<String, Object> map = new HashMap<String, Object>();
	// map.put("cmd", "login");
	// map.put("userId", NumEx.stringToInt(data.uid));
	// pushTcp.addToSend(map);
	// } catch (Exception e) {
	// if (pushTcp != null) {
	// pushTcp.close();
	// }
	// pushThread = null;
	// e.printStackTrace();
	// }
	// }

	/**
	 * 取得推送配置信息
	 * 
	 * @return
	 */
	public static PushData getCfgData() {
		try {

			Log.i(tag, tag + "getCfgData");
			FileHelper fh = new FileHelper();

			if (!fh.existsSDFile(CBCfg.PushCfgDataDir)) {
				fh.creatSDDir(CBCfg.PushCfgDataDir);
			}
			PushData r = null;
			if (fh.existsSDFile(CBCfg.PushCfgDataFile)) {
				byte[] fbytes = FileHelper.readFileFully(fh
						.openSDFile(CBCfg.PushCfgDataFile));

				JSONObject jo = new JSONObject(new String(fbytes, "UTF-8"));
				r = PushData.parse(jo);
			}
			// 当点击通知时，启动该contentIntent关联的activity
			// 用户唯一码
			// 上次登陆时间

			if (r == null) {
				r = new PushData();
				r.msg = new ArrayList<String>();
			}
			Log.i(tag, tag + r.pushHost + "    " + r.pushPort + "     " + r.uid
					+ "    " + r.lastLoginTime + "    " + r.packageName);
			return r;
		} catch (Exception e) {
			// TODO: handle exception
			PushData r = new PushData();
			r.msg = new ArrayList<String>();
			e.printStackTrace();
			Log.i(tag, tag + ":" + e.getMessage());
			return r;
		}
	}

	@Override
	public void onDestroy() {
		Intent in = new Intent();
		in.setAction("com.googl.YouWillNeverKillMe");
		sendBroadcast(in);
		Log.d(tag, "onDestroy()...");
		try {
			mNM.cancel(NOTIFICATION_ID);

			// if (pushTcp != null) {
			// pushTcp.stopListner();
			// pushTcp.close();
			// }
			refreshCfgData();
		} catch (Exception e) {
			e.printStackTrace();
		}
	}

	@Override
	public IBinder onBind(Intent intent) {
		return mBinder;
	}

	private final IBinder mBinder = new LocalBinder();

}
