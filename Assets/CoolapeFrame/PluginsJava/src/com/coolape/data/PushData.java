package com.coolape.data;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.json.JSONArray;
import org.json.JSONObject;

import com.coolape.lang.MapEx;

public class PushData {
	public long lastLoginTime;
	public String longTimeOntLoginNotifyMsg;
	public String uid = "";

	public String packageName = "";
	public String pushHost = "";
	public int pushPort;

	public List<String> msg;

	@SuppressWarnings({ "rawtypes", "unchecked" })
	public Map toMap() {
		Map r = new HashMap();
		r.put("uid", uid);
		r.put("lastLoginTime", lastLoginTime);
		r.put("packageName", packageName);
		r.put("pushHost", pushHost);
		r.put("pushPort", pushPort);
		r.put("longTimeOntLoginNotifyMsg", longTimeOntLoginNotifyMsg);
		r.put("msg", msg);
		return r;
	}

	public static PushData parse(@SuppressWarnings("rawtypes") Map map) {
		if (map == null)
			return null;

		PushData r = new PushData();
		r.lastLoginTime = MapEx.getLong(map, "lastLoginTime");
		r.longTimeOntLoginNotifyMsg = MapEx.getString(map,
				"longTimeOntLoginNotifyMsg");
		r.uid = MapEx.getString(map, "uid");
		r.packageName = MapEx.getString(map, "packageName");
		r.pushHost = MapEx.getString(map, "pushHost");
		r.pushPort = MapEx.getInt(map, "pushPort");
		r.msg = (List<String>) MapEx.getList(map, "msg");

		return r;
	}

	public static PushData parse(JSONObject map) {
		if (map == null)
			return null;
		PushData r = new PushData();
		try {
			r.lastLoginTime = map.getLong("lastLoginTime");
			r.longTimeOntLoginNotifyMsg = map
					.getString("longTimeOntLoginNotifyMsg");
			r.uid = map.getString("uid");
			r.packageName = map.getString("packageName");
			r.pushHost = map.getString("pushHost");
			r.pushPort = map.getInt("pushPort");
			JSONArray list = map.getJSONArray("msg");

			r.msg = new ArrayList<String>();
			if (list != null) {
				for (int i = 0; i < list.length(); i++) {
					String item = list.getString(i);
					r.msg.add(item);
				}
			}

			return r;
		} catch (Exception e) {
			return r;
		}
	}

}
