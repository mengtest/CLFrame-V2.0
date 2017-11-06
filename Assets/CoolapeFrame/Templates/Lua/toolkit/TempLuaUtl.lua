-- require 'CLLFunctions'
--- lua工具方法
do
    local smatch   = string.match
    local sfind    = string.find
    local pnlShade = nil;

    function showHotWheel(...)
        if pnlShade == nil then
            pnlShade = CLPanelManager.getPanel("PanelHotWheel");
        end
        local paras = { ... };
        if (# (paras) > 1) then
            local msg = paras[1];
            pnlShade:setData(msg);
        else
            pnlShade:setData("");
        end

        -- CLPanelManager.showPanel(pnlShade);
        pnlShade:show();
    end

    function hideHotWheel(...)
        if pnlShade == nil then
            pnlShade = CLPanelManager.getPanel("PanelHotWheel");
        end
        -- CLPanelManager.hidePanel(pnlShade);
        local func = pnlShade:getLuaFunction("hideSelf");
        if (func ~= nil) then
            func();
        end
    end

    function mapToColor(map)
        local color = Color.white;
        if (map == nil) then
            return color
        end
        color = Color(MapEx.getNumber(map, "r"),
        MapEx.getNumber(map, "g"),
        MapEx.getNumber(map, "b"),
        MapEx.getNumber(map, "a"));
        return color;
    end

    function mapToVector2(map)
        local v = Vector2.zero;
        if (map == nil) then
            return v
        end
        v = Vector2(MapEx.getNumber(map, "x"),
        MapEx.getNumber(map, "y"));
        return v;
    end

    function mapToVector3(map)
        local v = Vector3.zero;
        if (map == nil) then
            return v
        end
        v = Vector3(MapEx.getNumber(map, "x"),
        MapEx.getNumber(map, "y"),
        MapEx.getNumber(map, "z"));
        return v;
    end

    function mapToVector4(map)
        local v = Vector4.zero;
        if (map == nil) then
            return v
        end
        v = Vector4(MapEx.getNumber(map, "x"),
        MapEx.getNumber(map, "y"),
        MapEx.getNumber(map, "z"),
        MapEx.getNumber(map, "w"));
        return v;
    end

    function getChild(root, ...)
        local args = { ... }
        local path = "";
        if (# args > 1) then
            local str = PStr.b();
            for i, v in ipairs(args) do
                str:a(v):a("/")
            end
            path = str:e();
        else
            path = args[1];
        end
        return root:Find(path);
    end

    --获取路径
    function stripfilename(filename)
        return smatch(filename, "(.+)/[^/]*%.%w+$") --*nix system
        --return smatch(filename, “(.+)\\[^\\]*%.%w+$”) — windows
    end

    --获取文件名
    function strippath(filename)
        return smatch(filename, ".+/([^/]*%.%w+)$") -- *nix system
        --return smatch(filename, “.+\\([^\\]*%.%w+)$”) — *nix system
    end

    --去除扩展名
    function stripextension(filename)
        local idx = filename:match(".+()%.%w+$")
        if (idx) then
            return filename:sub(1, idx - 1)
        else
            return filename
        end
    end

    --获取扩展名
    function getextension(filename)
        return filename:match(".+%.(%w+)$")
    end

    function errMsg(msg)
        Utl.printe("error:" .. msg);
    end

    function warnMsg(msg)
        Utl.printw("warn:" .. msg);
    end

    function trim(s)
        -- return (s:gsub("^%s*(.-)%s*$", "%1"))
        return smatch(s, '^()%s*$') and '' or smatch(s, '^%s*(.*%S)') -- 性能略优
    end

    function startswith(str, substr)
        if str == nil or substr == nil then
            return nil, "the string or the sub-stirng parameter is nil"
        end
        if sfind(str, substr) ~= 1 then
            return false
        else
            return true
        end
    end

    function getAction(act)
        if (act == "idel") then
            --,       //0 空闲
            return 0;
        elseif (act == "idel2") then
            --,      //1 空闲
            actionValue = 1;
        elseif (act == "walk") then
            --,     //2 走
            actionValue = 2;
        elseif (act == "run") then
            --,        //3 跑
            actionValue = 3;
        elseif (act == "jump") then
            --,     //4 跳
            actionValue = 4;
        elseif (act == "slide") then
            --,      //5 滑行，滚动，闪避
            actionValue = 5;
        elseif (act == "drop") then
            --,     //6 下落
            actionValue = 6;
        elseif (act == "attack") then
            --,     //7 攻击
            actionValue = 7;
        elseif (act == "attack2") then
            --,    //8 攻击2
            actionValue = 8;
        elseif (act == "skill") then
            --,        //9 技能
            actionValue = 9;
        elseif (act == "skill2") then
            --,     //10 技能2
            actionValue = 10;
        elseif (act == "skill3") then
            --,     //11 技能3
            actionValue = 11;
        elseif (act == "skill4") then
            --,     //12 技能4
            actionValue = 12;
        elseif (act == "hit") then
            --,        //13 受击
            actionValue = 13;
        elseif (act == "dead") then
            --,     //14 死亡
            actionValue = 14;
        elseif (act == "happy") then
            --,      //15 高兴
            actionValue = 15;
        elseif (act == "sad") then
            --,        //16 悲伤
            actionValue = 16;
        elseif (act == "up") then
            --,        //17 起立
            actionValue = 17;
        elseif (act == "down") then
            --,        //18 倒下
            actionValue = 18;
        elseif (act == "biggestAK") then
            --,        //19 最大的大招
            actionValue = 19;
        elseif (act == "dizzy") then
            --,        //20 晕
            actionValue = 20;
        elseif (act == "stiff") then
            --,        //21 僵硬
            actionValue = 21;
        elseif (act == "idel3") then
            --,        //21 空闲
            actionValue = 22;
        else
            actionValue = 0;
        end
        return actionValue;
    end

    function getMonster(mcidInt)
        return CLLData.getMonsterByMcid(mcidInt);
    end

    function strSplit(inputstr, sep)
        if sep == nil then
            sep = "%s"
        end
        local t = {}; i = 1
        for str in string.gmatch(inputstr, "([^" .. sep .. "]+)") do
            t[i] = str
            i    = i + 1
        end
        return t;
    end

    -- 取得方法体
    function getLuaFunc(trace)
        if (trace == nil) then
            return nil
        end
        local list = strSplit(trace, ".");
        local func = nil;
        if (list ~= nil) then
            func = _G;
            for i, v in ipairs(list) do
                func = func[v];
            end
        end
        return func;
    end

    -- 动作回调toMap
    function ActCBtoList(...)
        local paras = { ... };
        if (# (paras) > 0) then
            local callbackInfor = ArrayList();
            for i, v in ipairs(paras) do
                callbackInfor:Add(v);
            end
            return callbackInfor;
        end
        return nil;
    end

    function onLoadedPanel(p, paras)
        p:setData(paras);
        CLPanelManager.showTopPanel(p);
    end

    function onLoadedPanelTT(p, paras)
        p:setData(paras);
        CLPanelManager.showTopPanel(p, true, true);
    end

    function SetActive(go, isActive)
        if (go == nil) then
            return
        end
        NGUITools.SetActive(go, isActive);
    end



    function getCC(transform, path, com)
        if not transform then return  end
        local tf = getChild(transform, path)
        if not tf then  return  end
        return tf:GetComponent(com)
    end


    function isNilOrEmpty(s)
        if s == nil or s == "" then
            return true;
        end
        return false;
    end

    function joinStr(...)
        local paras = {...}
        if paras == nil or #paras ==0 then
            return "";
        end
        local pstr = PStr.b();
        for i, v in ipairs(paras) do
            pstr:a(tostring(v))
        end
        return pstr:e();
    end
end

--module("LuaUtl", package.seeall)
