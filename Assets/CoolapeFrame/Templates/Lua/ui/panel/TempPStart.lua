--开始游戏
do
    local csSelf = nil;
    local transform = nil;
    local gameObject = nil;
    local upgradeUrl;
    local uidx;
    local selectedServer;
    local panelIndex = 0;

    -- 放在后面加载的页面
    local lateLoadPanels = {
        "PanelSceneManager", -- 切换场景页面
        "PanelChat"
    }

    CLLPStart = {};

    function CLLPStart.init(go)
        csSelf = go;
        transform = csSelf.transform;
        gameObject = csSelf.gameObject;
    end

    function CLLPStart.setData(pars)
        uidx = pars[1]
        selectedServer = pars[2];
    end

    -- 加载一些必要的lua
    function CLLPStart.setLuasAtBegainning()
        -- 取得数据配置
        require("cfg.DBCfg");
        -- 网络
        Net.self:setLua();
        --TODO:other lua scripts
        require("net.PorotocolService");

        CallNet = PorotocolService.callNet
        require("db.KKDBRoot");
		require("db.KKConstant");
        require("public.KKCameraMgr");
        require("public.KKFormula");
        MyCfg.self.worldMap:setLua();
        MyCfg.self.worldMap.luaTable._init();
    end

    function CLLPStart.show()
        -- 加载一些必要的lua
        CLLPStart.setLuasAtBegainning();

        local p = CLPanelManager.getPanel("PanelSplash");
        if (p ~= nil) then
            CLPanelManager.hidePanel(p);
        end

        CLLPStart.createPanel();
    end


    -- 创建ui
    function CLLPStart.createPanel()
        panelIndex = 0;
        local count = #(lateLoadPanels);
        if (count > 0) then
            for i = 1, count do
                local name = lateLoadPanels[i];
                CLPanelManager.getPanelAsy(name, CLLPStart.onLoadPanelAfter);
            end
        else
            CLLPStart.connectServer();
        end
    end

    function CLLPStart.onLoadPanelAfter(p)
        p:init();
        panelIndex = panelIndex + 1;
        local count = #(lateLoadPanels);
        if (panelIndex >= count) then
            --已经加载完
            CLLPStart.connectServer();
        end
    end


    -- 关闭页面
    function CLLPStart.hide()
        csSelf:cancelInvoke4Lua();
    end

    -- 刷新页面
    function CLLPStart.refresh()
    end

    -- 连接服务器相关处理
    function CLLPStart.connectServer()
        CLLPStart.connectGame();
        --         TODO:
        --[[
        -- 走到此处，说明热更部分及初始化部已经完成，接下来可以处理渠道SDK初始化、连接网关（CLLPStart.connectGate）、版本更新、处理公告、封号处理等等
        --]]

    end

    -- 连接网关
    function CLLPStart.connectGame(...)
        showHotWheel();
        Net.self:connectGame(MapEx.getString(selectedServer, "serverip"), MapEx.getInt(selectedServer, "serverport"))
    end


    -- 处理网络接口
    function CLLPStart.procNetwork(cmd, succ, msg, pars)
        if (succ == 1) then
            -- 接口处理成功
            if (cmd == "connectCallback") then
                if (pars == Net.self.gateTcp) then
                    -- 网关连接成功
                    -- 取得服务器版本号
                    --Net.self:sendGate(CallNet.verifyVer(CLCfgBase.Channel, __version__));
                    --Net.self:sendGate(PorotocolService.callNet.getMapList(0));
                end
                hideHotWheel();
                showHotWheel();

                local uid = uidx;
                --uid= '9215be325c6649bead13c2b9def749d9'
                --uid="8962bdf6c39d4d0c9e41a00bb0089173"
                if CLCfgBase.self.isEditMode then
                    if MyCfg.self.default_UID ~= "" then
                        uid = MyCfg.self.default_UID;
                    end
                end
                Net.self:send(PorotocolService.callNet.login(uid))
                --CLLPStart.doEnterGame();
            elseif cmd == "login" then
                hideHotWheel();
                CLLPStart.doEnterGame();
                --elseif (cmd == "verifyVer") then
                --    if (CLCfgBase.self.isNetMode) then
                --        showHotWheel();
                --        -- 取得公告(注意是在消息分发的地方处理的显示)
                --        Net.self:sendGate(CallNet.getNotices(CLCfgBase.Channel, __version__));
                --
                --        -- send to server
                --        Net.self:sendGate(CallNet.lgRegUser(Utl.uuid(), "", Utl.uuid(), CLCfgBase.Channel,
                --                                            SystemInfo.deviceModel, __version__));
                --    else
                --        local p = CLPanelManager.getPanel("PanelEnterGame");
                --        CLPanelManager.showTopPanel(p, true, true);
                --    end
                --elseif (cmd == "registUser"
                --or cmd == "lgUser"
                --or cmd == "lgRegUser") then
                --    local data = pars;
                --    local user = data[0];
                --    -- local lastNsv = data[1];
                --    -- 判断用户是否已经被封号
                --    if (NumEx.bio2Int(user.status) == GboConstant.PubAttr.Type_User_Close) then
                --        CLUIUtl.showConfirm(Localization.Get("MsgUserIsColsed"), nil);
                --        return;
                --    end
                --    local p = CLPanelManager.getPanel("PanelEnterGame");
                --    p:setData(data);
                --    CLPanelManager.showTopPanel(p, true, true);
                --
                --elseif (cmd == "entryGame") then
                --    -- 进入游戏
                --    -- CLLPStart.enterGame(nil, nil);
                --elseif (cmd == "lastLoginSv") then
                --    Net.self.gateTcp:stop(); -- 关掉网关连接
            end
        else
            -- 接口返回不成功
            if (cmd == "outofNetConnect") then
                hideHotWheel()
                CLUIUtl.showConfirm(
                LGet("UIOutofConnect"),
                function()
                    csSelf:invoke4Lua(CLLPStart.connectGame, 0.5);
                end );
            elseif (cmd == "verifyVer") then
                -- 版本号验证失败
                if (succ ~= GboConstant.ResultStatus.R_User_ChnFalse) then
                    local data = pars;
                    upgradeUrl = data.nstr;
                    CLUIUtl.showConfirm(Localization.Get("MsgHaveNewVersion"),
                    CLLPStart.upgradeGame);
                else
                    CLAlert.add(msg, Color.red, 1);
                end
            else
                CLAlert.add(msg, Color.red, 1);
            end
        end
    end

    -- 更新安装游戏
    function CLLPStart.upgradeGame(...)
        if (upgradeUrl ~= nil and upgradeUrl ~= "") then
            Application.OpenURL(upgradeUrl);
        end
    end

    -- 点击返回键关闭自己（页面）
    function CLLPStart.hideSelfOnKeyBack()
        return false;
    end

    function CLLPStart.doEnterGame()
        --CLPanelManager.getPanelAsy("mainCity", onLoadedPanel)
        CLPanelManager.getPanelAsy("PanelSceneManager", function(p)
            p:setData({ mode = GameMode.city });
            CLPanelManager.showTopPanel(p);
        end);

        --csSelf:invoke4Lua(function()
        --    CLPanelManager.hidePanel(csSelf);
        --end, 0.2);
    end

    ----------------------------------------------
    return CLLPStart;
end
