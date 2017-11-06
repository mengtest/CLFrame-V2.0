-- 确认提示框
do
    local pName = nil;
    local csSelf = nil;
    local transform = nil;
    local gameObject = nil;
    local panelContent = nil;
    local table = nil;
    local LabelContent = nil;
    local ButtonCancel = nil;
    local ButtonOK = nil;
    local lbButtonCancel = nil;
    local lbButtonOK = nil;

    local onClickButtonCallback1 = nil;
    local onClickButtonCallback2 = nil;
    local datas = Stack(); -- 需要确认的消息椎
    local currData = nil;
    local panelContentDepth = 1;
    local buttonOkOrgPositon;
    local buttonCancelOrgPositon;

    PanelConfirm = {}
    function PanelConfirm.init(go)
        pName = go.name;
        csSelf = go;
        transform = csSelf.transform;
        gameObject = csSelf.gameObject;
        panelContent = getChild(transform, "content", "PanelContent");
        table = getChild(panelContent, "Table");
        LabelContent = getChild(table, "LabelContent");
        LabelContent = LabelContent:GetComponent("UILabel");
        table = table:GetComponent("UITable");
        ButtonCancel = getChild(transform, "content", "ButtonCancel");
        lbButtonCancel = getChild(ButtonCancel, "Label"):GetComponent("UILabel");
        ButtonOK = getChild(transform, "content", "ButtonOK");
        lbButtonOK = getChild(ButtonOK, "Label"):GetComponent("UILabel");
        panelContent = panelContent:GetComponent("UIPanel");
        panelContentDepth = panelContent.depth;

        buttonOkOrgPositon = ButtonOK.transform.localPosition;
        buttonCancelOrgPositon = ButtonCancel.transform.localPosition;
    end

    function PanelConfirm.setData(pars)
        if (pars == nil) then return end
        if (currData ~= nil and currData[0] == pars[0]) then return end -- 相同消息只弹出一次
        datas:Push(pars);
    end

    function PanelConfirm.show()
        csSelf.panel.depth = CLUIInit.self.PanelConfirmDepth;
        panelContent.depth = csSelf.panel.depth + panelContentDepth;

        currData = datas:Peek();
        if (currData == nil or currData.Count < 6) then
            PanelConfirm.checkLeftData();
            return;
        end
        local msg = currData[0];
        local isShowOneButton = currData[1];
        local lbbutton1 = currData[2];
        onClickButtonCallback1 = currData[3];
        local lbbutton2 = currData[4];
        onClickButtonCallback2 = currData[5];

        LabelContent.text = msg;
        -- 重新计算collider
        NGUITools.AddWidgetCollider(LabelContent.gameObject);
        table.repositionNow = true;
        -- table:Reposition();
        lbButtonOK.text = lbbutton1;
        if (isShowOneButton) then
            NGUITools.SetActive(ButtonCancel.gameObject, false);
            local pos = ButtonOK.localPosition
            pos.x = 0;
            ButtonOK.localPosition = pos;
        else
            ButtonCancel.localPosition = buttonOkOrgPositon
            ButtonOK.localPosition = buttonCancelOrgPositon
            NGUITools.SetActive(ButtonCancel.gameObject, true);
            lbButtonCancel.text = lbbutton2;
        end

        SoundEx.playSound2("Sounds/Alert", 1);
    end

    function PanelConfirm.hide()
    end

    function PanelConfirm.refresh()
    end

    function PanelConfirm.procNetwork(cmd, succ, msg, pars)
    end

    function PanelConfirm.OnClickOK(button)
        if (onClickButtonCallback1 ~= nil) then
            Utl.doCallback(onClickButtonCallback1, nil);
        end
        PanelConfirm.checkLeftData();
    end

    function PanelConfirm.OnClickCancel(button)
        if (onClickButtonCallback2 ~= nil) then
            Utl.doCallback(onClickButtonCallback2, nil);
        end
        PanelConfirm.checkLeftData();
    end

    function PanelConfirm.checkLeftData(...)
        currData = datas:Pop();
        if (currData ~= nil) then
            currData:Clear();
            currData = nil;
        end
        if (datas.Count > 0) then
            csSelf:show();
        else
            csSelf:hide();
        end
    end

    return PanelConfirm;
end
