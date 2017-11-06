package com.coolape.u3dPlugin;

import android.content.Context;
import android.net.ConnectivityManager;
import android.net.NetworkInfo;
import android.telephony.TelephonyManager;
import com.unity3d.player.UnityPlayer;

public class NetUtl {
	private static final int NETWORK_TYPE_UNAVAILABLE = -1;
	private static final int NETWORK_TYPE_WIFI = -101;
	private static final int NETWORK_CLASS_WIFI = -101;
	private static final int NETWORK_CLASS_UNAVAILABLE = -1;
	private static final int NETWORK_CLASS_UNKNOWN = 0;
	private static final int NETWORK_CLASS_2_G = 1;
	private static final int NETWORK_CLASS_3_G = 2;
	private static final int NETWORK_CLASS_4_G = 3;
	public static final int NETWORK_TYPE_UNKNOWN = 0;
	public static final int NETWORK_TYPE_GPRS = 1;
	public static final int NETWORK_TYPE_EDGE = 2;
	public static final int NETWORK_TYPE_UMTS = 3;
	public static final int NETWORK_TYPE_CDMA = 4;
	public static final int NETWORK_TYPE_EVDO_0 = 5;
	public static final int NETWORK_TYPE_EVDO_A = 6;
	public static final int NETWORK_TYPE_1xRTT = 7;
	public static final int NETWORK_TYPE_HSDPA = 8;
	public static final int NETWORK_TYPE_HSUPA = 9;
	public static final int NETWORK_TYPE_HSPA = 10;
	public static final int NETWORK_TYPE_IDEN = 11;
	public static final int NETWORK_TYPE_EVDO_B = 12;
	public static final int NETWORK_TYPE_LTE = 13;
	public static final int NETWORK_TYPE_EHRPD = 14;
	public static final int NETWORK_TYPE_HSPAP = 15;

	public static boolean isNetworkAvailable(Context context) {
		ConnectivityManager cm = (ConnectivityManager) context
				.getSystemService("connectivity");
		if (cm != null) {
			NetworkInfo[] info = cm.getAllNetworkInfo();
			if (info != null) {
				for (int i = 0; i < info.length; i++) {
					if (info[i].getState() == NetworkInfo.State.CONNECTED) {
						return true;
					}
				}
			}
		}
		return false;
	}

	public static final boolean isConnectNet() {
		ConnectivityManager cm = (ConnectivityManager) UnityPlayer.currentActivity
				.getSystemService("connectivity");
		if (cm == null) {
			return false;
		}
		boolean isCanUse = isNetworkAvailable(UnityPlayer.currentActivity);
		if (!isCanUse) {
			return false;
		}
		NetworkInfo networkINfo = cm.getActiveNetworkInfo();
		if ((networkINfo != null) && (networkINfo.isAvailable())) {
			int ntype = networkINfo.getType();
			switch (ntype) {
			case 0:
			case 1:
				return true;
			}

		}

		return false;
	}

	private static final int getNetworkClass() {
		int networkType = 0;
		try {
			NetworkInfo network = ((ConnectivityManager) UnityPlayer.currentActivity
					.getSystemService("connectivity")).getActiveNetworkInfo();
			if ((network != null) && (network.isAvailable())
					&& (network.isConnected())) {
				int type = network.getType();
				if (type == 1) {
					networkType = -101;
				} else if (type == 0) {
					TelephonyManager telephonyManager = (TelephonyManager) UnityPlayer.currentActivity
							.getSystemService("phone");
					networkType = telephonyManager.getNetworkType();
				}
			} else {
				networkType = -1;
			}
		} catch (Exception localException) {
		}
		return getNetworkClassByType(networkType);
	}

	public static int getNetworkClassByType(int networkType) {
		switch (networkType) {
		case NETWORK_TYPE_UNAVAILABLE:
			return NETWORK_CLASS_UNAVAILABLE;
		case NETWORK_TYPE_WIFI:
			return NETWORK_CLASS_WIFI;
		case NETWORK_TYPE_GPRS:
		case NETWORK_TYPE_EDGE:
		case NETWORK_TYPE_CDMA:
		case NETWORK_TYPE_1xRTT:
		case NETWORK_TYPE_IDEN:
			return NETWORK_CLASS_2_G;
		case NETWORK_TYPE_UMTS:
		case NETWORK_TYPE_EVDO_0:
		case NETWORK_TYPE_EVDO_A:
		case NETWORK_TYPE_HSDPA:
		case NETWORK_TYPE_HSUPA:
		case NETWORK_TYPE_HSPA:
		case NETWORK_TYPE_EVDO_B:
		case NETWORK_TYPE_EHRPD:
		case NETWORK_TYPE_HSPAP:
			return NETWORK_CLASS_3_G;
		case NETWORK_TYPE_LTE:
			return NETWORK_CLASS_4_G;
		default:
			return NETWORK_CLASS_UNKNOWN;
		}
	}

	public static final String getCurrentNetworkType() {
		int networkClass = getNetworkClass();
		String type = "None";
		switch (networkClass) {
		case NETWORK_CLASS_UNAVAILABLE:
			type = "None";
			break;
		case NETWORK_CLASS_WIFI:
			type = "WiFi";
			break;
		case NETWORK_CLASS_2_G:
			type = "2G";
			break;
		case NETWORK_CLASS_3_G:
			type = "3G";
			break;
		case NETWORK_CLASS_4_G:
			type = "4G";
			break;
		case 0:
			type = "Unknown";
		}

		return type;
	}
}
