--- 网络下行数据调度器
do
--    require("CLLData");

    local lizGet = Localization.Get;

    cllNetDis = {}
    function cllNetDis.dispatchSend(map)
        CLLDataProc.procData(map);
    end

    function cllNetDis.dispatchHttp(map)
        if (map == nil) then return end
        PorotocolService.onCallNet.disp(map);
    end

    function cllNetDis.dispatchGate(map)
        if (map == nil) then return end
        PorotocolService.onCallNet.disp(map);
    end

    function cllNetDis.dispatchGame(map)
        if (map == nil) then return end
        GboGmSvBuilder.onCallNet.disp(map);
    end

    function cllNetDis.dispatch(...)
        local paras = { ... };
        local len = table.getn(paras);

        local cmd = paras[1]; -- 接口名
        local retvalue = paras[len]; -- 接口返回结果

        local data = {};
        if(len - 1 > 2) then
            for i = 2, len - 1 do
                -- data:Add(paras[i]);  -- 取得返回数据
                table.insert(data, paras[i]);
            end
        else
            data = paras[2];
        end

        -- 解密bio
        retvalue.succ = NumEx.bio2Int(retvalue.succ);
        local succ = retvalue.succ;
        local msg = retvalue.msg;
        if (succ ~= 0) then
            retvalue.msg = Localization.Get("Error_" .. succ);
            CLAlert.add(msg, Color.red, 1);
        else -- success
        cllNetDis.cacheData(cmd, data);
        end

        -- CLPanelManager.topPanel:onNetwork (cmd, retvalue.succ, nil);
        -- 因为c#调用时已经放在主线程里了，因此可以直接调用procNetwork
        -- CLPanelManager.topPanel:procNetwork (cmd, retvalue.succ, retvalue.msg, data);

        -- 通知所有显示的页面
        if (CLPanelManager.panelRetainLayer ~= nil and CLPanelManager.panelRetainLayer.Count > 0) then
            local showingPanels = CLPanelManager.panelRetainLayer:ToArray();
            for i = 0, showingPanels.Length - 1 do
                showingPanels[i]:procNetwork(cmd, succ, msg, data);
            end
            showingPanels = nil;
        else
            if (CLPanelManager.topPanel ~= nil) then
                CLPanelManager.topPanel:procNetwork(cmd, succ, msg, data);
            end
        end
    end

    function cllNetDis.cacheData(cmd, data)
        if (cmd == "getMapData") then
            if(data ~= nil and data.list ~= nil) then
                CLLData.onGetMapData(data.list);
            end
        elseif(cmd == "getMapDataOneScreen") then
            if(data ~= nil) then
                CLLData.onGetOneMapPageData(data);
            end
        end
    end

    -- 显示网关公告
    function cllNetDis.showGateNotice(paras)
        local data = paras[1];
        if (data ~= nil and data.list ~= nil) then
            local count = data.list.Count;
            local msg = "";
            local notice = nil;
            for i = 1, count do
                notice = data.list[i];
                msg = msg .. notice.title .. "\n" .. notice.cont .. "\n";
            end
            if (msg ~= "") then
                CLUIUtl.showConfirm(msg, nil);
            end
        end
    end

    return cllNetDis;
end

module("CLLNetDispatch", package.seeall)
