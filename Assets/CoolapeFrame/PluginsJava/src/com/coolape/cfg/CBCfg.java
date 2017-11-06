package com.coolape.cfg;

public class CBCfg {

	// 每隔多长时间更新一次，单位:毫秒
	public static final long Interval = 300000;// 300000;//五分钟
	// 推送配置信息保存的文件名
	public static final String PushCfgDataDir = ".cbPush";
	// 长时未登陆（1天），提示用户登陆
	public static final long pushOfflineTime = 86400000;// 12*60*60*1000;
	// 推送服务--需要配置的
	public static final String pushSericeClass = "com.coolape.pushmsg.CBPushServer"; 
	// 推送配置信息保存的文件名
	public static final String PushCfgDataFile = PushCfgDataDir +"/" + pushSericeClass + ".pushCfgData.cb";

}
