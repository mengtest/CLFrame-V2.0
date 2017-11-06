-- --[[
-- //                    ooOoo
-- //                   8888888
-- //                  88" . "88
-- //                  (| -_- |)
-- //                  O\  =  /O
-- //               ____/`---'\____
-- //             .'  \\|     |//  `.
-- //            /  \\|||  :  |||//  \
-- //           /  _||||| -:- |||||-  \
-- //           |   | \\\  -  /// |   |
-- //           | \_|  ''\---/''  |_/ |
-- //            \ .-\__  `-`  ___/-. /
-- //         ___`. .'  /--.--\  `. . ___
-- //      ."" '<  `.___\_<|>_/___.'  >' "".
-- //     | | : ` - \`.;`\ _ /`;.`/- ` : | |
-- //     \ \ `-.    \_ __\ /__ _/   .-` / /
-- //======`-.____`-.___\_____/___.-`____.-'======
-- //                   `=---='
-- //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
-- //           佛祖保佑       永无BUG
-- //           游戏大卖       公司腾飞
-- //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
-- --]]
do
    require("public.CLLInclude");

    #SCRIPTNAME# = {};


    --     UIAtlas.releaseSpriteTime = 30; -- 释放ui资源的时间（秒）

    -- 设置是否可以成多点触控
    -- CLCfgBase.self.uiCamera:GetComponent("UICamera").allowMultiTouch = false;

    -- if (SystemInfo.systemMemorySize < 2048) then
    --     CLCfgBase.self.isFullEffect = false;
    -- end

    --设置帧率
    Application.targetFrameRate = 30;

    -- 日志开关
    --CS.Debug.logger.logEnabled = false;

    -- 设置是否测试环境
    if (Prefs.getTestMode()) then
        local url = Prefs.getTestModeUrl();
        if (url ~= "") then
            CLAlert.add("Test...", Color.red, - 1, 1, false);
            CLVerManager.self.baseUrl = url;
        end
    end

    local fps = CLMainBase.self:GetComponent("CLFPS")
    fps.displayRect = Rect(10, 200, 640, 40);

    -- 当离线调用
    function #SCRIPTNAME#.onOffline(...)
        CLAlert.add("网络连接已经断开！", Color.red, 1);
    end

    -- 退出游戏确认
    function #SCRIPTNAME#.exitGmaeConfirm(...)
        if (CLCfgBase.self.isGuidMode) then
            return;
        end
        -- 退出确认
        if (CLPanelManager.topPanel == nil or
        (not CLPanelManager.topPanel:hideSelfOnKeyBack())) then
            CLUIUtl.showConfirm(Localization.Get("MsgExitGame"), #SCRIPTNAME#.doExitGmae, nil);
        end
    end

    -- 退出游戏
    function #SCRIPTNAME#.doExitGmae(...)
        Application.Quit();
    end

    -- 暂停游戏或恢复游戏
    function #SCRIPTNAME#.OnApplicationPause(isPause)
        if (isPause) then
            --设置帧率
            Application.targetFrameRate = 1;
            -- 内存释放
            GC.Collect();
        else
            -- 设置帧率
            Application.targetFrameRate = 30;
        end
    end

    function #SCRIPTNAME#.OnApplicationQuit(...)
    end
    --=========================================
    function #SCRIPTNAME#.showPanelStart()
        if (CLPanelManager.topPanel ~= nil and
        CLPanelManager.topPanel.name == "PanelStart") then
            CLPanelManager.topPanel:show();
        else
            --异步方式打开页面
            CLPanelManager.getPanelAsy("PanelSplash", #SCRIPTNAME#.showSplash);
        end
    end

    function #SCRIPTNAME#.showSplash(p)
        CLPanelManager.showPanel(p);
    end

    --------------------------------------------
    ---------- 验证热更新器是否需要更新------------
    --------------------------------------------
    function #SCRIPTNAME#.onCheckUpgrader(isHaveUpdated)
        if (isHaveUpdated) then
            -- 说明热更新器有更新，需要重新加载lua
            CLMainBase.self:reStart();
        else
            -- init sdk
            -- TODO: CLGboChn.getInstance():StartInit();

            if (CLCfgBase.self.isEditMode) then
                --主初始化完后，打开下一个页面
                CLMainBase.self:invoke4Lua(#SCRIPTNAME#.showPanelStart, 0.2);
            else
                -- 先执行一次热更新，注意isdoUpgrade=False,因为如果更新splash的atalse资源时，会用到
                CLLVerManager.init(nil,
                                   function()
                                       --主初始化完后，打开下一个页面
                                       CLMainBase.self:invoke4Lua(CLLMainLua.showPanelStart, 0.1);
                                   end,
                                   false, "");
            end
        end
    end

    -- 处理开始
    if (CLCfgBase.self.isEditMode) then
        #SCRIPTNAME#.onCheckUpgrader(false);
    else
        -- 更新热更新器
        CLLUpdateUpgrader.checkUpgrader(#SCRIPTNAME#.onCheckUpgrader);
    end
    --------------------------------------------
    --------------------------------------------

    return #SCRIPTNAME#;
end
