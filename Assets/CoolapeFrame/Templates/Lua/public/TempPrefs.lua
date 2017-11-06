-- 本地存储
do
    local UserName = "UserName";
    local UserPsd = "UserPsd";
    local AutoFight = "AutoFight";
    local TestMode = "isTestMode";
    local TestMode = "isTestMode";
    local TestModeUrl = "TestModeUrl";
    local BuyCardTimes = "BuyCardTimes";
    local soundEffSwitch = "soundEffSwitch";
    local musicSwitch = "musicSwitch";

    Prefs = {};

    function Prefs.setUserName(v)
        PlayerPrefs.SetString(UserName, v);
    end

    function Prefs.getUserName()
        return PlayerPrefs.GetString(UserName, "");
    end

    function Prefs.setUserPsd(v)
        PlayerPrefs.SetString(UserPsd, v);
    end

    function Prefs.getUserPsd(...)
        return PlayerPrefs.GetString(UserPsd, "");
    end

    function Prefs.setAutoFight(v)
        PlayerPrefs.SetInt(AutoFight, v and 0 or 1);
    end

    function Prefs.getAutoFight(...)
        return (PlayerPrefs.GetInt(AutoFight, 0) == 0) and true or false;
    end

    function Prefs.setTestMode(v)
        PlayerPrefs.SetInt(TestMode, v and 0 or 1);
    end

    function Prefs.getTestMode(v)
        return (PlayerPrefs.GetInt(TestMode, 0) == 0) and true or false;
    end

    function Prefs.setTestModeUrl(v)
        PlayerPrefs.SetString(TestModeUrl, v);
    end

    function Prefs.getTestModeUrl()
        return PlayerPrefs.GetString(TestModeUrl, "");
    end

    -- 设置购买卡的次数
    function Prefs.setBuyCardTimes(v)
        PlayerPrefs.SetInt(BuyCardTimes, v);
    end

    -- 取得购买卡的次数
    function Prefs.getBuyCardTimes(...)
        return PlayerPrefs.GetInt(BuyCardTimes, 0);
    end


    function Prefs.getSoundEffSwitch()
        local f = PlayerPrefs.GetInt(soundEffSwitch, 0);
        return (f == 0 and true or false);
    end

    function Prefs.setSoundEffSwitch(v)
        local f = v and 0 or 1;
        PlayerPrefs.SetInt("soundEffSwitch", f);
    end

    function Prefs.getMusicSwitch()
        local f = PlayerPrefs.GetInt(musicSwitch, 0);
        return (f == 0 and true or false);
    end

    function Prefs.setMusicSwitch(v)
        local f = v and 0 or 1;
        PlayerPrefs.SetInt("musicSwitch", f);
    end
end
--module("CLLPrefs", package.seeall)
