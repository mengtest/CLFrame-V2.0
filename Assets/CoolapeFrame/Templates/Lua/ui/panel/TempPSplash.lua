--开始loading页面，处理资源更新、及相关初始化
do
    require("channel.KKChl");
    require("toolkit.KKWhiteList");

    ---@type Coolape.CLPanelLua
    local csSelf = nil;
    local transform = nil;
    local gameObject = nil;
    local progressBar = nil;
    local progressBarTotal = nil;
    local lbprogressBarTotal = nil;
    local upgradeUrl = nil; -- app更新url
    local LabelTip = nil;
    local LabelVer = nil;
    local isFinishLoadPanels = false;
    local loadedPanelCount = 0;
    --local panelIndex = 0;
    local lbCustServer = nil;

    local ButtonEntry;
    local selectedServer;
    local oldServerIdx = "";
    local currServer;
    local user;
    local bottom;

    local www4UpgradeCell = nil; -- 更新时单个单元的www

    -- 预先加载的页面(在热更新完成后，先把必要的公共页面先加载了，后面的处理可能会用到)
    local beforeLoadPanels = {
        "PanelHotWheel", -- 菊花
        "PanelBackplate", -- 背板遮罩
        "PanelConfirm", -- 确认提示页面
        "PanelMask4Panel", -- 遮挡
        "PanelWWWProgress", -- 显示网络请求资源的进度
    }

    CLLPSplash = {};

    function CLLPSplash.init(go)
        csSelf = go;
        transform = csSelf.transform;
        gameObject = csSelf.gameObject;
        bottom = getChild(transform, "Bottom")
        progressBar = getChild(transform, "Bottom", "Progress Bar");
        progressBar = progressBar:GetComponent("UISlider");
        NGUITools.SetActive(progressBar.gameObject, false);

        progressBarTotal = getChild(transform, "Bottom", "Progress BarTotal");
        lbprogressBarTotal = getChild(progressBarTotal, "Thumb", "Label"):GetComponent("UILabel");
        progressBarTotal = progressBarTotal:GetComponent("UISlider");
        NGUITools.SetActive(progressBarTotal.gameObject, false);

        LabelTip = getChild(transform, "Bottom", "LabelTip");
        LabelTip = LabelTip:GetComponent("UILabel");
        NGUITools.SetActive(LabelTip.gameObject, false);
        LabelVer = getChild(transform, "TopLeft", "LabelVer");
        LabelVer = LabelVer:GetComponent("UILabel");

        lbCustServer = getChild(transform, "TopLeft", "LabelCustomerServer"):GetComponent("UILabel");
        ButtonEntry = getChild(transform, "ButtonEntry").gameObject;
        currServer = getCC(transform, "TopLeft/ButtonServer", "CLCellLua")
    end

    --     function CLLPSplash.setData (pars)
    --     end

    function CLLPSplash.show()
        csSelf.panel.depth = 200;
        --SoundEx.playMainMusic("enterGame");
        --panelIndex = 0;
        loadedPanelCount = 0;
        SetActive(progressBar.gameObject, false);
        SetActive(progressBarTotal.gameObject, false);
        SetActive(ButtonEntry, false)
        SetActive(currServer.gameObject, false)

        -- load alert
        CLLPSplash.addAlertHud();

        -- 初始化需要提前加载的页面
        local count = #(beforeLoadPanels);
        loadedPanelCount = 0;
        for i = 1, count do
            CLPanelManager.getPanelAsy(
            beforeLoadPanels[i],
            CLLPSplash.onLoadPanelBefore);
        end

        -- Hide company panel
        csSelf:invoke4Lua(function()
            local p = CLPanelManager.getPanel(MyMain.self.firstPanel);
            if (p ~= nil) then
                CLPanelManager.hidePanel(p);
            end
        end, 1.5);
    end

    -- 取得系统账号
    function CLLPSplash.accountLogin()
        local onGetUid = function(uid, orgs)
            if isNilOrEmpty(uid) then
                -- 取得uid失败
                return;
            end
            local url = PStr.b():a(__httpBaseUrl):a("/KokAccount/AccountServlet"):e();
            local chnCfg = nil;
            local chlCode = getChlCode();
            local formData = Hashtable();
            formData:Add("accountKey", uid) -- 唯一String
            formData:Add("machineid", Utl.uuid); -- 机器码
            formData:Add("userName", "") -- 邮箱 （没有可以不填）	String
            formData:Add("passWord", "")--密码 （没有可以不填）	String
            formData:Add("type", 1)--登录类型  1;//机器码登陆  2;//邮箱登陆  Int
            formData:Add("channel", chlCode)--渠道号 	String
            if KKWhiteList.isWhiteName() then
                formData:Add("isMax", 0)--是否验证同一机子注册限制 0:不限制 1：限制最多5个
            else
                formData:Add("isMax", 1)--是否验证同一机子注册限制
            end

            local loginError = function(...)
                CLUIUtl.showConfirm(LGet("UIMsg001"), CLLPSplash.accountLogin);
            end
            WWWEx.newWWW(CLVerManager.self, Utl.urlAddTimes(url),
            formData,
            CLAssetType.text,
            5, 10, CLLPSplash.onAccountLogin,
            loginError,
            loginError, nil);
        end

        if isNilOrEmpty(KKChl.uid) then
            getPanelAsy("PanelLogin", onLoadedPanelTT, { onGetUid, nil })
        else
            onGetUid(KKChl.uid, nil);
        end
    end

    function CLLPSplash.onAccountLogin(content, orgs)
        Prefs.setUserInfor(content);
        local d = JSON.DecodeMap(content);
        user = d;
        local errorCode = MapEx.getInt(d, "errorCode");
        if errorCode == 1 then
            local accountStatus = MapEx.getString(d, "accountStatus")
            local lastServerid = MapEx.getString(d, "lastServerid")
            lastServerid = lastServerid and lastServerid or CLLPSplash.getLastServerId( )-- '1'
            if tostring(accountStatus) == "1" then
                local idx = MapEx.getString(d, "idx")
                -- 正常
                oldServerIdx = lastServerid;
                CLLPSplash.getCurrServer(lastServerid, CLLPSplash.onGetCurrServer)
                --CLLPSplash.getLastServerId( )
                pcall(KKWhiteList.init, idx)
            else
                -- 异常
                CLAlert.add(LGet("UIMsg003"), Color.red, 1);
            end
        else
            CLAlert.add(LGet("UIMsg002"), Color.red, 1);
        end
    end

    -- 取得当前服务器
    function CLLPSplash.getCurrServer(sid, callback, errorCallback)
        local url = PStr.b():a(__httpBaseUrl2):a("/KokDirServer/LoginServlet"):e();
        local formData = Hashtable();
        formData:Add("serverid", sid) -- 服务器ID	String

        local loginError = function(...)
            CLUIUtl.showConfirm(LGet("UIMsg004"), CLLPSplash.accountLogin);
        end
        WWWEx.newWWW(CLVerManager.self, Utl.urlAddTimes(url),
        formData,
        CLAssetType.text,
        5, 10,
        callback,
        errorCallback and errorCallback or loginError,
        errorCallback and errorCallback or loginError, nil);
    end

    function CLLPSplash.onGetCurrServer(content, orgs)
        Prefs.setCurrServer(content);
        local d = JSON.DecodeMap(content);
        selectedServer = d;

        SetActive(currServer.gameObject, true);
        currServer:init({ data = d, selected = d }, nil);
        SetActive(ButtonEntry.gameObject, true);
    end

    -- 关闭页面
    function CLLPSplash.hide()
        csSelf:cancelInvoke4Lua();

        ---@type Coolape.CLPanelLua
        local p = CLPanelManager.getPanel(MyMain.self.firstPanel);
        if (p ~= nil and p.gameObject.activeInHierarchy) then
            CLPanelManager.hidePanel(p);
        end
    end

    -- 刷新页面
    function CLLPSplash.refresh()
        LabelVer.text = joinStr(Localization.Get("Version"), __version__);
    end

    -- 处理热更新
    function CLLPSplash.updateRes()
        if CLCfgBase.self.isDirectEntry then
            -- 取得缓存的数据
            user = JSON.DecodeMap(Prefs.getUserInfor());

            --printe(Prefs.getCurrServer())
            selectedServer = JSON.DecodeMap(Prefs.getCurrServer());

            -- 直接进游戏
            CLLPSplash.checkHotUpgrade();
        else
            if not CLCfgBase.self.hotUpgrade4EachServer then
                -- 更新资源
                CLLVerManager.init(CLLPSplash.onProgress, CLLPSplash.onFinishResUpgrade, true, "");
            else
                --
                CLLPSplash.accountLogin();
            end
        end
    end

    function CLLPSplash.checkHotUpgrade()
        if CLCfgBase.self.hotUpgrade4EachServer then
            local resMd5 = "";
            if CLPathCfg.self.platform == "IOS" then
                resMd5 = MapEx.getString(selectedServer, "iosversion");
            else
                resMd5 = MapEx.getString(selectedServer, "androidversion");
            end
            -- 更新资源
            CLLVerManager.init(CLLPSplash.onProgress, CLLPSplash.onFinishResUpgrade, true, resMd5);
        else
            CLLPSplash.prepareStartGame()
        end
    end

    --设置进度条
    function CLLPSplash.onProgress(...)
        local args = { ... };
        local all = args[1]; -- 总量
        local v = args[2]; -- 当前值
        if (#(args) >= 3) then
            www4UpgradeCell = args[3];
        else
            www4UpgradeCell = nil;
        end

        if (progressBarTotal ~= nil) then
            NGUITools.SetActive(progressBarTotal.gameObject, true);
            NGUITools.SetActive(LabelTip.gameObject, true);
            if (type(all) == "number") then
                if (all > 0) then
                    local value = v / all;
                    progressBarTotal.value = value;
                    if (www4UpgradeCell ~= nil) then
                        -- 说明有单个资源
                        lbprogressBarTotal.text = joinStr(v, "/", all);
                    end
                    -- 单个资源的进度
                    CLLPSplash.onProgressCell();

                    -- 表明已经更新完成
                    if (value == 1) then
                        csSelf:cancelInvoke4Lua(CLLPSplash.onProgressCell);
                        NGUITools.SetActive(progressBarTotal.gameObject, false);
                        NGUITools.SetActive(LabelTip.gameObject, false);
                        NGUITools.SetActive(progressBar.gameObject, false);
                    end
                else
                    csSelf:cancelInvoke4Lua(CLLPSplash.onProgressCell);
                    progressBarTotal.value = 0;
                    NGUITools.SetActive(progressBarTotal.gameObject, false);
                    NGUITools.SetActive(LabelTip.gameObject, false);
                    NGUITools.SetActive(progressBar.gameObject, false);
                end
            else
                print(joinStr("all====", all));
            end
        end
    end

    -- 单个文件更新进度
    function CLLPSplash.onProgressCell(...)
        if (www4UpgradeCell ~= nil) then
            NGUITools.SetActive(progressBar.gameObject, true);
            progressBar.value = www4UpgradeCell.progress;
            csSelf:cancelInvoke4Lua(CLLPSplash.onProgressCell);
            csSelf:invoke4Lua(CLLPSplash.onProgressCell, 0.1);
        else
            NGUITools.SetActive(progressBar.gameObject, false);
            csSelf:cancelInvoke4Lua(CLLPSplash.onProgressCell);
        end
    end

    -- 资源更新完成
    function CLLPSplash.onFinishResUpgrade(upgradeProcSuccess)
        if (not upgradeProcSuccess) then
            print("UpgradeResFailed");
        else
            if (CLLVerManager.isHaveUpgrade()) then
                -- 说明有更新，重新启动
                if CLCfgBase.self.hotUpgrade4EachServer then
                    CLCfgBase.self.isDirectEntry = true;
                end
                csSelf:cancelInvoke4Lua();
                csSelf:invoke4Lua(CLLPSplash.reLoadGame, 0.1);
                return;
            end
        end

        if CLCfgBase.self.hotUpgrade4EachServer then
            -- 准备开始游戏
            CLLPSplash.prepareStartGame();
        else
            --SetActive(ButtonEntry, true)
            CLLPSplash.accountLogin();
        end
    end

    -- 重新启动lua
    function CLLPSplash.reLoadGame()
        --- 释放资源开始-------------------------------
        local cleanRes = function()
            -- 把主城删掉
            if KKPSceneManager and KKPSceneManager.mainCityObj and KKMainCity ~= nil and KKMainCity.transform ~= nil then
                KKMainCity.cleanRoles();
                --GameObject.DestroyImmediate (KKMainCity.transform.gameObject, true);
                CLThingsPool.returnObj(KKMainCity.transform.name, KKMainCity.transform.gameObject);
                SetActive(KKMainCity.transform.gameObject, false)
                KKPSceneManager.mainCityObj = nil;
            end

            -- 把战场删掉
            if KKBattle ~= nil and KKBattle.csSelf ~= nil then
                GameObject.DestroyImmediate(KKBattle.csSelf.gameObject, true);
            end

            if KKCarbon ~= nil and KKCarbon.csSelf ~= nil then
                GameObject.DestroyImmediate(KKCarbon.csSelf.gameObject, true);
            end

            if CLAlert ~= nil and CLAlert.csSelf ~= nil then
                GameObject.DestroyImmediate(CLAlert.csSelf.gameObject, true);
            end

            Net.self.luaTable = nil;
            MyCfg.self.worldMap.luaTable = nil;
            CLMaterialPool.materialTexRefCfg = nil; -- 重新把配置清空
            releaseRes4GC(true);
        end
        --- 释放资源结束-------------------------------
        pcall(cleanRes)
        local panel = CLPanelManager.getPanel(CLMainBase.self.firstPanel);
        if panel then
            CLPanelManager.showPanel(panel);
        end
        CLMainBase.self:reStart();
    end

    -- 准备开始游戏
    function CLLPSplash.prepareStartGame()
        CLLPSplash.checkSignCode();

        if (progressBar ~= nil) then
            csSelf:cancelInvoke4Lua(CLLPSplash.onProgressCell);
            NGUITools.SetActive(progressBar.gameObject, false);
            NGUITools.SetActive(progressBarTotal.gameObject, false);
            NGUITools.SetActive(LabelTip.gameObject, false);
        end

        -- 播放背景音乐---------------
        -- SoundEx.playMainMusic();
        ----------------------------
    end

    -- 加载hud alert
    function CLLPSplash.addAlertHud()
        local onGetObj = function(name, AlertRoot, orgs)
            AlertRoot.transform.parent = CLUIInit.self.uiPublicRoot;
            AlertRoot.transform.localPosition = Vector3.zero;
            AlertRoot.transform.localScale = Vector3.one;
            NGUITools.SetActive(AlertRoot, true);
        end
        CLUIOtherObjPool.borrowObjAsyn("AlertRoot", onGetObj);
    end

    function CLLPSplash.onLoadPanelBefore(p)
        p:init();
        loadedPanelCount = loadedPanelCount + 1;
        if (p.name == "PanelConfirm" or
        p.name == "PanelHotWheel" or
        p.name == "PanelMask4Panel" or
        p.name == "PanelWWWProgress") then
            p.transform.parent = CLUIInit.self.uiPublicRoot;
            p.transform.localScale = Vector3.one;
        end

        if (p.name == "PanelWWWProgress") then
            CLPanelManager.showPanel(p);
        end
        CLLPSplash.onProgress(#(beforeLoadPanels), loadedPanelCount);
        if (loadedPanelCount >= #(beforeLoadPanels)) then
            -- 页面已经加载完成
            -- 处理热更新

            if (not Application.isEditor) then
                CLLPSplash.checkNewVersion()
            else
                csSelf:invoke4Lua(CLLPSplash.updateRes, 0.2);
            end
        end
    end

    function CLLPSplash.checkSignCode()
        -- 把热更新及加载ui完了后，再做验证签名
        if (not CLLPSplash.isSignCodeValid()) then
            CLUIUtl.showConfirm(Localization.Get("MsgTheVerIsNotCorrect"), nil);
            -- CLUIUtl.showConfirm("亲爱的玩家你所下载的版本可能是非官方版本，请到xxx去下载。非常感谢！", nil);
            return;
        end

        CLLPSplash.goNext();
    end

    -- 签名是否有效(Only 4 android)
    function CLLPSplash.isSignCodeValid(...)
        if isNilOrEmpty(CLCfgBase.self.singinMd5Code) then
            return true;
        end
        -- 取得签名串
        local code = Utl.getSingInCodeAndroid();

        if (code ~= 0) then
            local md5Code = Utl.MD5Encrypt(code);
            if (md5Code ~= CLCfgBase.self.singinMd5Code) then
                return false;
            end
        end
        return true;
    end

    function CLLPSplash.onSelectServer(server)
        selectedServer = server;
        local jsonStr = JSON.JsonEncode(selectedServer)
        Prefs.setCurrServer(jsonStr);
        currServer:init({ data = selectedServer, selected = selectedServer }, nil);
    end

    function CLLPSplash.uiEventDelegate(go)
        local goName = go.name;
        if goName == "ButtonServer" then
            CLPanelManager.getPanelAsy("PanelServers", onLoadedPanelTT, { CLLPSplash.onSelectServer, selectedServer })
        elseif goName == "ButtonEntry" then
            --if oldServerIdx ~= selectedServer.idx then
            --    -- 说明换区了
            --    CLLPSplash.notifyServer();
            --end

            SetActive(ButtonEntry, false)
            CLLPSplash.getCurrServer(MapEx.getString(selectedServer, "idx"),
            function(content, orgs)
                Prefs.setCurrServer(content);
                local d = JSON.DecodeMap(content);
                selectedServer = d;
                currServer:init({ data = d, selected = d }, nil);
                -- 服务器状态
                local state = MapEx.getString(selectedServer, "serverstatus")
                if state ~= "1" and (KKWhiteList == nil or (not KKWhiteList.isWhiteName())) then
                    -- 服务器停服了
                    CLAlert.add(LGet("UIMsg005"), Color.red, 1);
                    SetActive(ButtonEntry, true)
                    return;
                end
                CLLPSplash.checkHotUpgrade();
            end,
            function()
                SetActive(ButtonEntry, true)
            end
            )

            --CLLPSplash.saveServerId(MapEx.getString(selectedServer, "idx"))
        elseif goName == "ButtonXieyi" then
            --CLPanelManager.getPanelAsy("PanelRobotTest", onLoadedPanelTT, { MapEx.getString(user, "idx"), selectedServer })
        elseif goName == "ButtonSetting" then
            getPanelAsy("PanelSetting", onLoadedPanelTT);
        end
    end

    function CLLPSplash.notifyServer()
        local url = PStr.b():a(__httpBaseUrl):a("/KokAccount/SaveServlet"):e();
        local formData = Hashtable();
        formData:Add("serverid", MapEx.getString(selectedServer, "idx"))
        formData:Add("uidx", MapEx.getString(user, "idx")) -- 邮箱 （没有可以不填）	String

        WWWEx.newWWW(CLVerManager.self, Utl.urlAddTimes(url),
        formData,
        CLAssetType.text,
        5, 10, nil,
        nil,
        nil, nil);
    end

    function CLLPSplash.goNext()
        if CLCfgBase.self.isDirectEntry then
            CLCfgBase.self.isDirectEntry = false;
        end
        CLPanelManager.getPanelAsy("PanelStart", onLoadedPanel, { MapEx.getString(user, "idx"), selectedServer });
    end

    local savePath = joinStr(Application.persistentDataPath, '/')
    local fname = 'loginserverid'
    --FileEx.CreateDirectory(savePath)

    function CLLPSplash.getLastServerId( )
        local str = FileEx.ReadAllText(joinStr(savePath, fname))
        if str and str:len() > 2 then
            local tb = json.decode(str)
            return tb.svid;
        end
    end

    function CLLPSplash.saveServerId(id)
        --print('save login server ', id)
        local str = json.encode({ svid = id })
        FileEx.WriteAllText(joinStr(savePath, fname), str)
    end

    --[[
    --{"ver":"1.0","force":true,"url":"http://"}
    --]]
    function CLLPSplash.checkNewVersion()
        local oldVer = __version__;
        local onGetVer = function(content, orgs)
            local map = JSON.DecodeMap(content);
            local newVer = MapEx.getString(map, "ver");
            --print(Utl.MapToString(map));
            --print(oldVer);
            if (tonumber(newVer) > tonumber(oldVer)) then
                local doUpgradeApp = function()
                    CLLPSplash.upgradeGame(MapEx.getString(map, "url"))
                end
                if MapEx.getBool(map, "force") then
                    CLUIUtl.showConfirm(LGet("UIMsg012"), true, LGet("UI057"), doUpgradeApp, "", nil);
                else
                    CLUIUtl.showConfirm(LGet("UIMsg012"), false, LGet("UI057"), doUpgradeApp, LGet("UI056"), CLLPSplash.updateRes);
                end
            else
                CLLPSplash.updateRes();
            end
        end

        local onGetVerError = function(msg, orgs)
            CLAlert.add(LGet("UIMsg013"), Color.white, 1)
            CLLPSplash.updateRes();
        end

        local chlCode = getChlCode();
        local url = Utl.urlAddTimes(joinStr(CLVerManager.self.baseUrl, "/appVer.", chlCode, ".json"));
        WWWEx.newWWW(CLVerManager.self, url, CLAssetType.text,
        5, 5, onGetVer,
        onGetVerError,
        onGetVerError, nil);
    end


    -- 更新安装游戏
    function CLLPSplash.upgradeGame(url)
        if not isNilOrEmpty(url ) then
            Application.OpenURL(url);
        end
    end

    ----------------------------------------------
    return CLLPSplash;
end
