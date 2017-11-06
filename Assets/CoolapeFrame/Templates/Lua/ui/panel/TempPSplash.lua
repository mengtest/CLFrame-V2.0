--开始loading页面，处理资源更新、及相关初始化
do

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
        SoundEx.playMainMusic("enterGame");
        --panelIndex = 0;
        loadedPanelCount = 0;
        SetActive(progressBar.gameObject, false);
        SetActive(progressBarTotal.gameObject, false);
        SetActive(ButtonEntry, false)
        SetActive(currServer.gameObject, false)

        -- 设置哪些页面时可以点击3D场景
        -- CLUIDrag4World.setCanClickPanel("PanelBattle");

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
        end, 0.2);
    end

    -- 取得系统账号
    function CLLPSplash.accountLogin()
        local url = PStr.b():a(__httpBaseUrl):a("/KokAccount/AccountServlet"):e();
        local chnCfg = nil;
        local chlCode = "0000";
        if not CLCfgBase.self.isEditMode then
            local fpath = "chnCfg.json"; -- 该文在打包时会自动放在streamingAssetsPath目录下，详细参见打包工具
            local content = FileEx.readNewAllText(fpath);
            if (content ~= nil) then
                chnCfg = JSON.DecodeMap(content);
                chlCode = chnCfg.SubChannel;
            end
        end
        local formData = Hashtable();
        formData:Add("accountKey", Utl.uuid) -- 唯一机器码	String
        formData:Add("email", "") -- 邮箱 （没有可以不填）	String
        formData:Add("pwd", "")--密码 （没有可以不填）	String
        formData:Add("type", 1)--登录类型  1;//机器码登陆  2;//邮箱登陆  Int
        formData:Add("channel", chlCode)--渠道号 	String

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

    function CLLPSplash.onAccountLogin(content, orgs)
        Prefs.setUserInfor(content);
        local d = JSON.DecodeMap(content);
        user = d;
        local errorCode = MapEx.getInt(d, "errorCode");
        if errorCode == 1 then
            --local idx           = MapEx.getString(d, "idx")
            local accountStatus = MapEx.getString(d, "accountStatus")
            local lastServerid = CLLPSplash.getLastServerId( ) or d.lastServerid
            print(' lastServerid ', lastServerid)
            if tostring(accountStatus) == "1" then
                -- 正常
                oldServerIdx = lastServerid;
                CLLPSplash.getCurrServer(lastServerid)
                --CLLPSplash.getLastServerId( )
            else
                -- 异常
                CLAlert.add(LGet("UIMsg003"), Color.red, 1);
            end
        else
            CLAlert.add(LGet("UIMsg002"), Color.red, 1);
        end
    end

    -- 取得当前服务器
    function CLLPSplash.getCurrServer(sid)
        local url = PStr.b():a(__httpBaseUrl):a("/KokDirServer/LoginServlet"):e();
        local formData = Hashtable();
        formData:Add("serverid", sid) -- 服务器ID	String

        local loginError = function(...)
            CLUIUtl.showConfirm(LGet("UIMsg004"), CLLPSplash.accountLogin);
        end
        WWWEx.newWWW(CLVerManager.self, Utl.urlAddTimes(url),
        formData,
        CLAssetType.text,
        5, 10, CLLPSplash.onGetCurrServer,
        loginError,
        loginError, nil);
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
    end

    -- 刷新页面
    function CLLPSplash.refresh()
        LabelVer.text = Localization.Get("Version") .. __version__;
    end

    -- 添加屏蔽字
    function CLLPSplash.addShieldWords()
        local onGetShieldWords = function(path, content, originals)
            if (content ~= nil) then
                BlockWordsTrie.getInstanse():init(content);
            end
        end;
        local path = CLPathCfg.self.basePath .. "/" .. CLPathCfg.upgradeRes .. "/priority/txt/shieldWords";
        CLVerManager.self:getNewestRes(path, CLAssetType.text, onGetShieldWords, nil);
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
            local resMd5 = selectedServer.version;
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
                        lbprogressBarTotal.text = v .. "/" .. all;
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
                print("all====" .. all);
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
                csSelf:invoke4Lua(CLLPSplash.reLoadGame, 0.3);
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
    function CLLPSplash.reLoadGame(...)
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

        -- 添加屏蔽字
        -- CLLPSplash.addShieldWords();
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
            csSelf:invoke4Lua(CLLPSplash.updateRes, 0.2);
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
        if (CLCfgBase.self.singinMd5Code == "" or CLCfgBase.self.singinMd5Code == nil) then
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
            CLPanelManager.getPanelAsy("PanelServers", onLoadedPanelTT, { CLLPSplash.onSelectServer , selectedServer })
        elseif goName == "ButtonEntry" then
            --if oldServerIdx ~= selectedServer.idx then
            --    -- 说明换区了
            --    CLLPSplash.notifyServer();
            --end
            -- 服务器状态
            if selectedServer.serverstatus ~= 1 then
                -- 服务器停服了
                CLAlert.add(LGet("UIMsg005"), Color.red, 1);
                return;
            end
            SetActive(ButtonEntry, false)
            CLLPSplash.checkHotUpgrade();

            CLLPSplash.saveServerId(selectedServer.idx)
        end
    end

    function CLLPSplash.notifyServer()
        local url = PStr.b():a(__httpBaseUrl):a("/KokAccount/SaveServlet"):e();
        local formData = Hashtable();
        formData:Add("serverid", selectedServer.idx)
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

    local savePath = Application.persistentDataPath ..'/'
    local fname = 'loginserverid'
    --FileEx.CreateDirectory(savePath)

    function CLLPSplash.getLastServerId( )
        local str = FileEx.ReadAllText(savePath..fname)
        if str and str:len() > 2 then
            local tb = json.decode(str)
            return tb.svid;
        end
    end

    function CLLPSplash.saveServerId(id)
        print('save login server ', id)
        local str = json.encode({svid=id})
        FileEx.WriteAllText(savePath..fname, str)
    end
    ----------------------------------------------
    return CLLPSplash;
end
