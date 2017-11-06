--- - 管理数据配置
do
    local mdb = {} -- 原始数据
    local mMaps4ID = {}

    DBCfgTool = {};

    -- 把json数据转成对象
    function DBCfgTool.getDatas(cfgPath, isParseWithID)
        local list = mdb[cfgPath];
        local map4ID = {};
        if (list == nil) then
            list = {};
            local _list = Utl.fileToObj(cfgPath);
            if (_list == nil or _list.Count < 2) then
                mdb[cfgPath] = list;
                return list, map4ID;
            end

            local count = _list.Count;
            local n = 0;
            local keys = _list[0];
            local cellList = nil;
            local cell = nil;
            local value = 0;
            for i = 1, count - 1 do
                cellList = _list[i];
                n = cellList.Count;
                cell = {};
                for j = 0, n - 1 do
                    value = cellList[j];
                    if (type(value) == "number") then
                        cell[keys[j]] = int2Bio(value);
                    else
                        cell[keys[j]] = value;
                    end
                end
                if (isParseWithID) then
                    map4ID[bio2Int(cell.ID)] = cell;
                end
                table.insert(list, cell);
            end
            mdb[cfgPath] = list;
            mMaps4ID[cfgPath] = map4ID;
        else
            map4ID = mMaps4ID[cfgPath];
        end
        return list, map4ID;
    end

    -- 取得角色的数据
    function DBCfgTool.getRoleData(cfgRolePath, cfgRoleLevPath)
        local tmp, roleBaseData = DBCfgTool.getDatas(cfgRolePath, true);
        local roleLevData = DBCfgTool.getDatas(cfgRoleLevPath);
        local key = "";
        local gid = 0;
        local lev = 0;
        local heros = {};

        list = {}
        for i, v in pairs(roleLevData) do
            gid = bio2Int(v.GID);
            lev = bio2Int(v.Lev);
            key = gid .. "_" .. lev;
            local m = {}
            m.base = roleBaseData[gid];
            m.vals = v;
            list[key] = m;
            if (m.base.IsHero and lev == 1) then
                table.insert(heros, m);
            end
        end
        list.heros = heros;

        return list;
    end

    -- 通用取得有base数据和lev数据的表
    function DBCfgTool.pubGetBaseAndLevData(baseDataPath, levDataPath)
        local tmp, baseData = DBCfgTool.getDatas(baseDataPath, true);
        local levData = DBCfgTool.getDatas(levDataPath);
        local key = "";
        local gid = 0;
        local lev = 0;

        list = {}
        for i, v in pairs(levData) do
            gid = bio2Int(v.GID);
            lev = bio2Int(v.Lev);
            key = gid .. "_" .. lev;
            local m = {}
            m.base = baseData[gid];
            m.vals = v;
            list[key] = m;
        end
        return list;
    end

    function DBCfgTool.pubGetList4GID(dataPath)
        local datas = DBCfgTool.getDatas(dataPath);
        local gid;
        local m = {};
        local list = {}
        for i, v in pairs(datas) do
            gid = bio2Int(v.GID);
            list = m[gid];
            if(list == nil) then
                list = {};
            end
            table.insert(list, v);
            m[gid] = list;
        end
        return m;
    end

    return DBCfgTool;
end

--module("DBCfgTool", package.seeall)
