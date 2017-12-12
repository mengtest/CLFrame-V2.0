--- - 管理数据配置
do
    require("cfg.DBCfgTool")
    pseudoMoneyItemIDBuild = 10000;--建筑升级用元宝
    pseudoMoneyItemIDTech = 20000;--科技升级用元宝
    pseudoMoneyItemIDMarch = 30000;--出征
    pseudoMoneyItemIDRes = 40000;--铁木粮
    pseudoMoneyItemIDCopper = 50000;--铜币
    pseudoMoneyItemIDAvoidWar = 60000;--免战
    pseudoMoneyItemIDMoveCity = 70000;--迁城
    local bio2Int = NumEx.bio2Int;
    local int2Bio = NumEx.int2Bio;
    local db = {} -- 经过处理后的数据
    -- 数据的路径
    local upgradeRes = "/upgradeRes"
    if (CLCfgBase.self.isEditMode) then
        upgradeRes = "/upgradeRes4Publish";
    end
    local priorityPath = PStr.b():a(CLPathCfg.persistentDataPath):a("/"):a(CLPathCfg.self.basePath):a(upgradeRes):a("/priority/"):e();
    local cfgBasePath = PStr.b():a(priorityPath):a("cfg/"):e();
    local cfgWorldBasePath = PStr.b():a(priorityPath):a("worldMap/"):e();

    -- 大地图
    local cfgMapPath = PStr.b():a(cfgWorldBasePath):e();

    -- 全局变量定义
    local cfgCfgPath = PStr.b():a(cfgBasePath):a("DBCFCfgData.cfg"):e();

    --local cfgGoodsPath = PStr.b():a(cfgBasePath):a("DBCFGoodsData.cfg"):e();

    DBCfg = {};

    -- 取得数据列表
    function DBCfg.getData(path)
        local dbMap = db[path];
        if (dbMap == nil) then
            --if (path == cfgRolePath) then
            --dbMap = DBCfgTool.getRoleData(cfgRolePath, cfgRoleLevPath);
            --elseif (path == cfgSkillPath) then
            --    dbMap = DBCfgTool.pubGetBaseAndLevData(cfgSkillPath, cfgSkillLevPath);
            if path == cfgMapCellPath then
                dbMap = DBCfgTool.pubGet4GIDLev(path);
            elseif path == cfgTalkingPath or path == cfgCarbonPath then
                local gidList;
                gidList, dbMap = DBCfgTool.pubGetList4GID(path);
				if path == cfgCarbonPath then
					gidList[0] = nil; -- 把新手剧情的移除
				end
                dbMap.list = gidList;
            else
                -- 其它没有特殊处理的都以ID为key（dbList:下标连续的列表, dbMap：以ID为key的luatable）
                local dbList = nil;
                dbList, dbMap = DBCfgTool.getDatas(path, true);

                if path == cfgGoodsPath then
                    -- 商品
                    local list = {};
                    local chlCode = getChlCode();
                    for i,v in ipairs(dbList) do
                        if true or v.Channel == chlCode then
                            table.insert(list, v);
                        end
                    end
                    table.sort(list, function(a, n)
                        return bio2Int(a.ListOrder) < bio2Int(n.ListOrder)
                    end)

                    dbList = list;
                end

                -- ====================================
                dbMap.list = dbList;
            end
            db[path] = dbMap;
        end
        return dbMap;
    end

    -- 取得常量配置
    function DBCfg.getConstCfg(...)
        local datas = DBCfg.getData(cfgCfgPath);
        if (datas == nil) then
            return nil
        end
        return datas[1];
    end

    -- 常量配置
    GConstCfg = DBCfg.getConstCfg();


    --function DBCfg.getGoodsList()
    --    local datas = DBCfg.getData(cfgGoodsPath);
    --    if (datas == nil) then
    --        return nil
    --    end
    --    return datas.list;
    --end
    --
    --function DBCfg.getGoodsByID(id)
    --    local datas = DBCfg.getData(cfgGoodsPath);
    --    if (datas == nil) then
    --        return nil
    --    end
    --    return datas[id]
    --end
    --------------------------------------------------
    return DBCfg;
end

--module("DBCfg", package.seeall)
