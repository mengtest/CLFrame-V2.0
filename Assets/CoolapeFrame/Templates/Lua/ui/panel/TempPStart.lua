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
        "PanelChat",
        "PanelLossSource",
    }

    CLLPStart = {};

    function CLLPStart.init(go)
        csSelf = go;
        transform = csSelf.transform;
        gameObject = csSelf.gameObject;
        -- 加载一些必要的lua
        CLLPStart.setLuasAtBegainning();
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
        require("toolkit.KKPushMsg");
        require("channel.KKChlIAP");

        KKPushMsg.init(uidx);
        MyCfg.self.worldMap:setLua();
        --MyCfg.self.worldMap.luaTable._init();
        MyCfg.self.worldMap:invoke4Lua(MyCfg.self.worldMap.luaTable._init, 0.5);

        -- 资源释放时间
        if CLAssetsManager.self then
            CLAssetsManager.self.timeOutSec4Realse = 10;
        end

        if ReporterMessageReceiver.self and ReporterMessageReceiver.self.gameObject then
            if KKWhiteList.isWhiteName() then
                ReporterMessageReceiver.self.gameObject:SetActive(true)
            else
                ReporterMessageReceiver.self.gameObject:SetActive(false)
            end
        end

        CLPanelManager.self.mainPanelName = "PanelCityUi";
        -- 添加屏蔽字
        MyMain.self:invoke4Lua(CLLPStart.addShieldWords, 1);
    end

    function CLLPStart.show()
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
        if isNilOrEmpty(PorotocolService.callNet.__sessionid) or PorotocolService.callNet.__sessionid == 0 then
            CLLPStart.createPanel();
        end
    end

    -- 添加屏蔽字
    function CLLPStart.addShieldWords()
        local onGetShieldWords = function(path, content, originals)
            if (content ~= nil) then
                BlockWordsTrie.getInstanse():init(content);
            end
        end;
        local path = joinStr(CLPathCfg.self.basePath, "/", CLPathCfg.upgradeRes, "/priority/txt/shieldWords");
        CLVerManager.self:getNewestRes(path, CLAssetType.text, onGetShieldWords, nil);
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
                if CLCfgBase.self.isEditMode then
                    if not isNilOrEmpty(MyCfg.self.default_UID) then
                        uid = MyCfg.self.default_UID;
                    elseif (not isNilOrEmpty(__UUID__)) then
                        uid = __UUID__;
                    end
                else
                    if not isNilOrEmpty(__UUID__) then
                        uid = __UUID__;
                    end
                end

                PorotocolService.callNet.__sessionid = uid;
                Net.self:send(PorotocolService.callNet.login(uid))
                --CLLPStart.doEnterGame();
            elseif cmd == "login" then
                hideHotWheel();
                if msg == "-1" then
                    CLPanelManager.getPanelAsy("PanelSelectPlayer", onLoadedPanelTT, uidx)
                else
                    CLLPStart.doEnterGame();
                end

                -- IAP 登陆成功后再初化iap
                local rt, err = pcall(KKChlIAP.init);
                if not rt then
                    printe(err)
                end
            elseif cmd == "getUserTaskList" then
                if CLPanelManager.topPanel == csSelf then
                    CLLPStart.doEnterGame();
                end
            end
        else
            -- 接口返回不成功
            if (cmd == "outofNetConnect") then
                hideHotWheel()
                CLUIUtl.showConfirm(
                LGet("UIOutofConnect"),
                function()
                    csSelf:invoke4Lua(CLLPStart.connectGame, 0.5);
                end,
                function()
                    local p = CLPanelManager.getPanel("PanelSplash");
                    if (p ~= nil) then
                        CLPanelManager.showPanel(p);
                    end
                    hideTopPanel();
                end);
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
        if not isNilOrEmpty(upgradeUrl ) then
            Application.OpenURL(upgradeUrl);
        end
    end

    -- 点击返回键关闭自己（页面）
    function CLLPStart.hideSelfOnKeyBack()
        return false;
    end

    function CLLPStart.doEnterGame()
        local tasks = KKDBUser.getTasks()
        if tasks == nil then
            return
        end

        CLLPStart._EnterGame(KKDBUser.isNewPlayer());
    end

    function CLLPStart._EnterGame(isNewPlayer)
        if isNewPlayer then
            CLLPStart.guidNewPlayer();
        else
            CLPanelManager.getPanelAsy("PanelSceneManager",
            function(p)
                p:setData({ mode = GameMode.city });
                CLPanelManager.showTopPanel(p);

                local p2 = CLPanelManager.getPanel("PanelSplash");
                if (p2 ~= nil) then
                    CLPanelManager.hidePanel(p2);
                end
            end);
        end
    end

    function CLLPStart.guidNewPlayer()
        openGuidPanel(KKDBGuid.getCaptionList(1))
        local p2 = CLPanelManager.getPanel("PanelSplash");
        if (p2 ~= nil) then
            CLPanelManager.hidePanel(p2);
        end
    end

    ----------------------------------------------
    return CLLPStart;
end
