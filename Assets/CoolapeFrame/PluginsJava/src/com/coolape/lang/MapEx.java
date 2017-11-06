package com.coolape.lang;

import java.util.ArrayList;
import java.util.Map;

@SuppressWarnings("rawtypes")
public class MapEx {
	public static final String getString( Map m, Object key) {
		try {
			if(m == null) {
				return "";
			}
			Object r = m.get(key);
			if(r == null) {
				return "";
			}
			return r.toString();
		} catch (Exception e) {
			return "";
		}
	}


	public static final int getInt( Map m, Object key) {
		try {
			if(m == null) {
				return 0;
			}
			Object r = m.get(key);
			if(r == null) {
				return 0;
			}
			return Integer.parseInt(r.toString());
		} catch (Exception e) {
			return 0;
		}
	}

	public static final long getLong( Map m, Object key) {
		try {
			if(m == null) {
				return 0;
			}
			Object r = m.get(key);
			if(r == null) {
				return 0;
			}
			return Long.parseLong(r.toString());
		} catch (Exception e) {
			return 0;
		}
	}


	@SuppressWarnings("unchecked")
	public static final ArrayList<String> getList( Map m, Object key) {
		try {
			if(m == null) {
				return new ArrayList<String>();
			}
			Object r = m.get(key);
			if(r == null) {
				return new ArrayList<String>();
			}
			return (ArrayList<String>)r;
		} catch (Exception e) {
			return new ArrayList<String>();
		}
	}

}
