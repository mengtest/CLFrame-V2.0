--- - 管理数据配置
do
    require("cfg.DBCfgTool")
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

    -- 全局变量定义
    local cfgCfgPath = PStr.b():a(cfgBasePath):a("DBCFCfgData.cfg"):e();
    -- 角色base属性
    local cfgRolePath = PStr.b():a(cfgBasePath):a("DBCFRoleData.cfg"):e();
    -- 角色等级属性
    local cfgRoleLevPath = PStr.b():a(cfgBasePath):a("DBCFRoleLevData.cfg"):e();
    -- 技能base属性
    local cfgSkillPath = PStr.b():a(cfgBasePath):a("DBCFSkillData.cfg"):e();
    -- 技能等级属性
    local cfgSkillLevPath = PStr.b():a(cfgBasePath):a("DBCFSkillLevData.cfg"):e();
    -- 子弹
    local cfgBulletPath = PStr.b():a(cfgBasePath):a("DBCFBulletData.cfg"):e();
    -- 大地图中的物件属性
    local cfgMapCellPath = PStr.b():a(cfgBasePath):a("DBCFMapCellData.cfg"):e();
    -- 剧情对话
    local cfgPlotPath = PStr.b():a(cfgBasePath):a("DBCFPlotData.cfg"):e();
    -- buff
    local cfgBuffPath = PStr.b():a(cfgBasePath):a("DBCFBuffData.cfg"):e();
    -- 物品
    local cfgThingPath = PStr.b():a(cfgBasePath):a("DBCFThingData.cfg"):e();

    DBCfg = {};

    -- 取得数据列表
    function DBCfg.getData(path)
        local dbMap = db[path];
        if (dbMap == nil) then
            if (path == cfgRolePath) then
                dbMap = DBCfgTool.getRoleData(cfgRolePath, cfgRoleLevPath);
            elseif (path == cfgSkillPath) then
                dbMap = DBCfgTool.pubGetBaseAndLevData(cfgSkillPath, cfgSkillLevPath);
            elseif (path == cfgPlotPath) then
                dbMap = DBCfgTool.pubGetList4GID(cfgPlotPath);
            else
                -- 其它没有特殊处理的都以ID为key（因些在配置数据时，ID列必须是以1开始且连续）
                local tmp = nil;
                tmp, dbMap = DBCfgTool.getDatas(path, true);
            end
            db[path] = dbMap;
        end
        return dbMap;
    end


    -- 取得常量配置
    function DBCfg.getConstCfg(...)
        local datas = DBCfg.getData(cfgCfgPath);
        if (datas == nil) then return nil end
        return datas[1];
    end

    -- 常量配置
    GConstCfg = DBCfg.getConstCfg();

    -- 取得角色的数据
    function DBCfg.getRoleByIDAndLev(id, lev)
        local datas = DBCfg.getData(cfgRolePath);
        if (datas == nil) then return nil end
        return datas[id .. "_" .. lev];
    end

    -- 取得英雄列表
    function DBCfg.getHeroList()
        local datas = DBCfg.getData(cfgRolePath);
        if (datas == nil) then return nil end
        return datas.heros;
    end

    -- 取得技能的数据
    function DBCfg.getSkillByIDAndLev(id, lev)
        local datas = DBCfg.getData(cfgSkillPath);
        if (datas == nil) then return nil end
        return datas[id .. "_" .. lev];
    end

    -- 取得子弹属性
    function DBCfg.getBulletByID(id)
        local datas = DBCfg.getData(cfgBulletPath);
        if (datas == nil) then return nil end
        return datas[id];
    end

    -- 取得大地图物件
    function DBCfg.getMapCellByID(id)
        local datas = DBCfg.getData(cfgMapCellPath);
        if (datas == nil) then return nil end
        return datas[id];
    end

    -- 取得大地图数据
    function DBCfg.getMapByMapID(id)
        local path = PStr.b():a(cfgMapPath):a(id):a(".json"):e();
        local data = db[path];
        if (data == nil) then
            local centent = File.ReadAllText(path);
            data = cjson.decode(centent);
            db[path] = data;
        end
        return data;
    end

    -- 取得对话
    function DBCfg.getPlotsByGID(gid)
        local datas = DBCfg.getData(cfgPlotPath);
        if (datas == nil) then return nil end
        return datas[gid];
    end

    -- 取得buff
    function DBCfg.getBuffByID(id)
        local datas = DBCfg.getData(cfgBuffPath);
        if (datas == nil) then return nil end
        return datas[id];
    end


    -- 取得物品
    function DBCfg.getThingByID(id)
        local datas = DBCfg.getData(cfgThingPath);
        if (datas == nil) then return nil end
        return datas[id];
    end
    --------------------------------------------------
    return DBCfg;
end

--module("DBCfg", package.seeall)
