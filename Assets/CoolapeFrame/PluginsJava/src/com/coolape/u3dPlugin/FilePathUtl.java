package com.coolape.u3dPlugin;

import android.app.Activity;
import android.os.Environment;
import com.unity3d.player.UnityPlayer;

public class FilePathUtl {
	public static final boolean isHasSdk() {
		return Environment.getExternalStorageState().equals("mounted");
	}

	public static final String getPath() {
		Activity activity = UnityPlayer.currentActivity;
		return activity.getFilesDir().getPath();
	}

	public static final String getSDKPath() {
		if (!isHasSdk())
			return "";
		return Environment.getExternalStorageDirectory().getPath();
	}
}
