--[[
-- 更新热更器处理
-- 判断热更新器本身是不是需要更新，同时判断渠道配置是否要更新
--]]
do
    local localVer = Hashtable();
    local serverVer = Hashtable();
    local serverVerStr = "";
    -- 热更新器的版本
    local upgraderVer = "upgraderVer.json";
    local localVerPath = upgraderVer;
    local upgraderName = PStr.b():a(CLPathCfg.self.basePath):a("/upgradeRes/priority/lua/toolkit/CLLVerManager.lua"):e();
    -- 控制渠道更新的
    local channelName = "channels.json";
    local finishCallback = nil; -- finishCallback(isHaveUpdated)

    local isUpdatedUpgrader = false; -- 是否更新的热更新器
    ----------------------------------
    CLLUpdateUpgrader = {};
    function CLLUpdateUpgrader.checkUpgrader(ifinishCallback)
        isUpdatedUpgrader = false;
        finishCallback = ifinishCallback;
        CLVerManager.self:StartCoroutine(FileEx.readNewAllTextAsyn(localVerPath, CLLUpdateUpgrader.onGetLocalUpgraderVer));
    end

    function CLLUpdateUpgrader.onGetLocalUpgraderVer(content)
        localVer = JSON.DecodeMap(content);
        if (localVer == nil) then
            localVer = Hashtable();
        end
        local url = PStr.b():a(CLVerManager.self.baseUrl):a("/"):a(upgraderVer):e();
        url = Utl.urlAddTimes(url);

        WWWEx.newWWW(CLVerManager.self, url, CLAssetType.text,
            3, 3, CLLUpdateUpgrader.onGetServerUpgraderVer,
            CLLUpdateUpgrader.onGetServerUpgraderVer,
            CLLUpdateUpgrader.onGetServerUpgraderVer, nil);
    end

    function CLLUpdateUpgrader.onGetServerUpgraderVer(content, orgs)
        serverVerStr = content;
        serverVer = JSON.DecodeMap(content);
        serverVer = serverVer == nil and Hashtable() or serverVer;
        -- print("MapEx.getInt(localVer, upgraderVer)==" .. MapEx.getInt(localVer, "upgraderVer"))
        -- print("MapEx.getInt(serverVer, upgraderVer)==" .. MapEx.getInt(serverVer, "upgraderVer"))
        if (MapEx.getString(localVer, "upgraderVer") ~= MapEx.getString(serverVer, "upgraderVer")) then
            CLLUpdateUpgrader.updateUpgrader();
        else
            CLLUpdateUpgrader.checkChannelVer(false);
        end
    end

    function CLLUpdateUpgrader.updateUpgrader(...)
        local url = "";
        local verVal = MapEx.getString(serverVer, "upgraderVer");
        url = PStr.b():a(CLVerManager.self.baseUrl):a("/"):a(upgraderName):a("."):a(verVal):e();
        WWWEx.newWWW(CLVerManager.self, url, CLAssetType.bytes,
            3, 5, CLLUpdateUpgrader.ongetNewestUpgrader,
            CLLUpdateUpgrader.ongetNewestUpgrader,
            CLLUpdateUpgrader.ongetNewestUpgrader, upgraderName);
    end

    function CLLUpdateUpgrader.ongetNewestUpgrader(content, orgs)
        if (content ~= nil) then
            local file = PStr.begin():a(CLPathCfg.persistentDataPath):a("/"):a(upgraderName):e();
            FileEx.CreateDirectory(Path.GetDirectoryName(file));
            File.WriteAllBytes(file, content);

            file = PStr.begin():a(CLPathCfg.persistentDataPath):a("/"):a(localVerPath):e();
            File.WriteAllText(file, serverVerStr);

            CLLUpdateUpgrader.checkChannelVer(true);
        else
            CLLUpdateUpgrader.checkChannelVer(false);
        end
    end

    -- 取得最新的渠道更新控制信息
    function CLLUpdateUpgrader.checkChannelVer(hadUpdatedUpgrader)
        isUpdatedUpgrader = hadUpdatedUpgrader;

        if (MapEx.getInt(localVer, "channelVer") < MapEx.getInt(serverVer, "channelVer")) then
            CLLUpdateUpgrader.getChannelInfor();
        else
            Utl.doCallback(finishCallback, isUpdatedUpgrader);
        end
    end


    function CLLUpdateUpgrader.getChannelInfor(...)
        local verVal = MapEx.getInt(serverVer, "channelVer");
        -- 注意是加了版本号的，会使用cdn
        local url = PStr.b():a(CLVerManager.self.baseUrl):a("/"):a(channelName):a("."):a(verVal):e();
        WWWEx.newWWW(CLVerManager.self, url, CLAssetType.text,
            3, 5, CLLUpdateUpgrader.onGetChannelInfor,
            CLLUpdateUpgrader.onGetChannelInfor,
            CLLUpdateUpgrader.onGetChannelInfor, channelName);
    end

    function CLLUpdateUpgrader.onGetChannelInfor(content, orgs)
        if (content ~= nil) then
            local file = PStr.b():a(CLPathCfg.persistentDataPath):a("/"):a(channelName):e();
            FileEx.CreateDirectory(Path.GetDirectoryName(file));
            File.WriteAllText(file, content);
        end

        Utl.doCallback(finishCallback, isUpdatedUpgrader);
    end
end

--module("CLLUpdateUpgrader", package.seeall)
