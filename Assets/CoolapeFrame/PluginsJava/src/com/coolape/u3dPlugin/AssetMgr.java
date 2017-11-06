package com.coolape.u3dPlugin;

import android.app.Activity;
import android.content.res.AssetManager;
import android.graphics.Path;
import android.util.Log;

import java.io.InputStream;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Hashtable;
import java.util.List;

import com.unity3d.player.UnityPlayer;

/**
 * Created by Sean on 2015/7/9.
 */
public class AssetMgr {
	private static AssetManager sAssetManager;
	private static boolean isInit = false;
	private static Activity sActivity = null;
	private static Hashtable<String, Boolean> mFileTable = new Hashtable<String, Boolean>();
	public static void init() {
		if (!isInit) {
			sActivity = UnityPlayer.currentActivity;
			sAssetManager = sActivity.getAssets();
			isInit = true;
		}
	}

	public static boolean isFileExists(String path) {
		boolean ret = false;
		if (mFileTable.containsKey(path))
			return (boolean) mFileTable.get(path);
		init();

		if (sAssetManager != null) {
			InputStream input = null;
			try {
				input = sAssetManager.open(path);
				ret = true;
				mFileTable.put(path, true);
				input.close();
				input = null;
			} catch (Exception e) {
				mFileTable.put(path, false);
			}
		}
		return ret;
	}

	public static byte[] getBytes(String path) {
		init();
		byte[] mBytes = null;
		if (sAssetManager != null) {
			InputStream input = null;
			try {
				input = sAssetManager.open(path);
				if (input == null) {
					if (!mFileTable.containsKey(path)) {
						mFileTable.put(path, false);
					}
				} else {
					int length = input.available();
					if (length > 0) {
						mBytes = new byte[length];
						input.read(mBytes);
						input.close();
						if (!mFileTable.containsKey(path)) {
							mFileTable.put(path, true);
						}
					} else{
						if (!mFileTable.containsKey(path)) {
							mFileTable.put(path, false);
						}
					}
				}
			} catch (Exception e) {
				if (!mFileTable.containsKey(path)) {
					mFileTable.put(path, false);
				}
			}
		}
		return mBytes;
	}

	public static String getString(String path) {
		init();
		byte[] mBytes = getBytes(path);
		if (mBytes != null) {
			try {
				return new String(mBytes, "utf-8");
			} catch (Exception e) {

			}
			return "";
		}
		return "";
	}
}
