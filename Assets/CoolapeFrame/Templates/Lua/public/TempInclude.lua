-- 需要先加载的部分
do
    -------------------------------------------------------
    -- 加载lua引用路径
    -- local i = 0;
    -- local count = CLPathCfg.self.luaPackgePath.Length;
    -- while(true) do
    -- if(i >= count) then break end;
    -- Util.AddLuaPath(CLPathCfg.luaBasePath .. CLPathCfg.self.luaPackgePath[i]);
    -- -- package.path = CLPathCfg.luaBasePath .. CLPathCfg.self.luaPackgePath[i] .. ";" .. package.path
    -- i = i + 1;
    -- end;


    -------------------------------------------------------
    -- 重新命名

    Vector2 = CS.UnityEngine.Vector2;
    Vector3 = CS.UnityEngine.Vector3;
    Vector4 = CS.UnityEngine.Vector4;
    Color = CS.UnityEngine.Color;
    Ray = CS.UnityEngine.Ray;
    Bounds = CS.UnityEngine.Bounds;
    Ray2D = CS.UnityEngine.Ray2D;
    Time = CS.UnityEngine.Time;
    GameObject = CS.UnityEngine.GameObject;
    Component = CS.UnityEngine.Component;
    Behaviour = CS.UnityEngine.Behaviour;
    Transform = CS.UnityEngine.Transform;
    Resources = CS.UnityEngine.Resources;
    TextAsset = CS.UnityEngine.TextAsset;
    AnimationCurve = CS.UnityEngine.AnimationCurve;
    AnimationClip = CS.UnityEngine.AnimationClip;
    MonoBehaviour = CS.UnityEngine.MonoBehaviour;
    ParticleSystem = CS.UnityEngine.ParticleSystem;
    SkinnedMeshRenderer = CS.UnityEngine.SkinnedMeshRenderer;
    Renderer = CS.UnityEngine.Renderer;
    WWW = CS.UnityEngine.WWW;
    Screen = CS.UnityEngine.Screen;
    Hashtable = CS.System.Collections.Hashtable;
    ArrayList = CS.System.Collections.ArrayList;
    Queue = CS.System.Collections.Queue
    Stack = CS.System.Collections.Stack
    GC = CS.System.GC
    File = CS.System.IO.File
    Directory = CS.System.IO.Directory
    Application = CS.UnityEngine.Application
    RaycastHit = CS.UnityEngine.RaycastHit
    Ray = CS.UnityEngine.Ray
    Rect = CS.UnityEngine.Rect

    UICamera = CS.UICamera;
    Localization = CS.Localization;
    NGUITools = CS.NGUITools;
    UIRect = CS.UIRect;
    UIWidget = CS.UIWidget;
    UIWidgetContainer = CS.UIWidgetContainer;
    UILabel = CS.UILabel;
    UIToggle = CS.UIToggle;
    UIBasicSprite = CS.UIBasicSprite;
    UITexture = CS.UITexture;
    UISprite = CS.UISprite;
    UIProgressBar = CS.UIProgressBar;
    UISlider = CS.UISlider;
    UIGrid = CS.UIGrid;
    UITable = CS.UITable;
    UIInput = CS.UIInput;
    UIScrollView = CS.UIScrollView;
    UITweener = CS.UITweener;
    TweenWidth = CS.TweenWidth;
    TweenRotation = CS.TweenRotation;
    TweenPosition = CS.TweenPosition;
    TweenScale = CS.TweenScale;
    TweenAlpha = CS.TweenAlpha;
    UICenterOnChild = CS.UICenterOnChild;
    UIAtlas = CS.UIAtlas;
    UILocalize = CS.UILocalize;
    UIPlayTween = CS.UIPlayTween;
    PlayerPrefs = CS.UnityEngine.PlayerPrefs
    SystemInfo = CS.UnityEngine.SystemInfo
    Shader = CS.UnityEngine.Shader
    Path = CS.System.IO.Path;
    MemoryStream = CS.System.IO.MemoryStream;

    CLAssetsManager = CS.Coolape.CLAssetsManager
    B2InputStream = CS.Coolape.B2InputStream
    B2OutputStream = CS.Coolape.B2OutputStream
    MapEx = CS.Coolape.MapEx
    NumEx2 = CS.Coolape.NumEx
    FileEx = CS.Coolape.FileEx
    JSON = CS.Coolape.JSON
    DateEx = CS.Coolape.DateEx
    PStr = CS.Coolape.PStr
    SoundEx = CS.Coolape.SoundEx
    CLBulletBase = CS.Coolape.CLBulletBase
    CLBulletPool = CS.Coolape.CLBulletPool
    CLEffect = CS.Coolape.CLEffect
    CLEffectPool = CS.Coolape.CLEffectPool
    CLMaterialPool = CS.Coolape.CLMaterialPool
    CLRolePool = CS.Coolape.CLRolePool
    CLSoundPool = CS.Coolape.CLSoundPool
    CLSharedAssets = CS.Coolape.CLSharedAssets
    CLMaterialInfor = CS.Coolape.CLSharedAssets.CLMaterialInfor
    CLTexturePool = CS.Coolape.CLTexturePool
    CLThingsPool = CS.Coolape.CLThingsPool
    CLBaseLua = CS.Coolape.CLBaseLua
    CLBehaviour4Lua = CS.Coolape.CLBehaviour4Lua
    CLUtlLua = CS.Coolape.CLUtlLua
    CLMainBase = CS.Coolape.CLMainBase
    Net = CS.Coolape.Net
    CLCfgBase = CS.Coolape.CLCfgBase
    CLPathCfg = CS.Coolape.CLPathCfg
    CLVerManager = CS.Coolape.CLVerManager
    CLAssetType = CS.Coolape.CLAssetType
    CLRoleAction = CS.Coolape.CLRoleAction
    CLRoleAvata = CS.Coolape.CLRoleAvata
    CLUnit = CS.Coolape.CLUnit
    BlockWordsTrie = CS.Coolape.BlockWordsTrie
    ColorEx = CS.Coolape.ColorEx
    DateEx = CS.Coolape.DateEx
    FileEx = CS.Coolape.FileEx
    HttpEx = CS.Coolape.HttpEx
    JSON = CS.Coolape.JSON
    ListEx = CS.Coolape.ListEx
    MapEx = CS.Coolape.MapEx
    MyMainCamera = CS.Coolape.MyMainCamera
    MyTween = CS.Coolape.MyTween
    NewList = CS.Coolape.NewList
    NewMap = CS.Coolape.NewMap
    SoundEx = CS.Coolape.SoundEx
    NumEx = CS.Coolape.NumEx
    ObjPool = CS.Coolape.ObjPool
    PStr = CS.Coolape.PStr
    SScreenShakes = CS.Coolape.SScreenShakes
    StrEx = CS.Coolape.StrEx
    Utl = CS.Coolape.Utl
    WWWEx = CS.Coolape.WWWEx
    ZipEx = CS.Coolape.ZipEx
    XXTEA = CS.Coolape.XXTEA
    CLButtonMsgLua = CS.Coolape.CLButtonMsgLua
    CLJoystick = CS.Coolape.CLJoystick
    CLUIDrag4World = CS.Coolape.CLUIDrag4World
    CLUILoopGrid = CS.Coolape.CLUILoopGrid
    CLUILoopTable = CS.Coolape.CLUILoopTable
    TweenSpriteFill = CS.Coolape.TweenSpriteFill
    UIDragPage4Lua = CS.Coolape.UIDragPage4Lua
    UIDragPageContents = CS.Coolape.UIDragPageContents
    UIGridPage = CS.Coolape.UIGridPage
    UIMoveToCell = CS.Coolape.UIMoveToCell
    UISlicedSprite = CS.Coolape.UISlicedSprite
    CLAlert = CS.Coolape.CLAlert
    CLCellBase = CS.Coolape.CLCellBase
    CLCellLua = CS.Coolape.CLCellLua
    CLPanelBase = CS.Coolape.CLPanelBase
    CLPanelLua = CS.Coolape.CLPanelLua
    CLPanelManager = CS.Coolape.CLPanelManager
    CLUIInit = CS.Coolape.CLUIInit
    CLUIOtherObjPool = CS.Coolape.CLUIOtherObjPool
    CLUIParticle = CS.Coolape.CLUIParticle
    CLUIUtl = CS.Coolape.CLUIUtl
    EffectNum = CS.Coolape.EffectNum
    EffectProgress = CS.Coolape.EffectNum
    B2Int = CS.Coolape.B2Int
    AngleEx = CS.Coolape.AngleEx
    CLMyGrid = CS.Coolape.CLMyGrid
    -------------------------------------------------------
    --

    -------------------------------------------------------
    -- require
    require("toolkit.LuaUtl");
    require("public.CLLPrefs");
    require("toolkit.CLLUpdateUpgrader");
    require("toolkit.CLLVerManager");
    -------------------------------------------------------
    -- 全局变量
    __version__ = Application.version; -- "1.0";
    -------------------------------------------------------
    bio2Int = NumEx.bio2Int
    int2Bio = NumEx.int2Bio
    bio2Long = NumEx.bio2Long
    long2Bio = NumEx.Long2Bio
    printe = Utl.printe
    printw = Utl.printw
    LGet = Localization.Get
    net = Net.self
    hideTopPanel = CLPanelManager.hideTopPanel
    getPanelAsy = CLPanelManager.getPanelAsy
    -------------------------------------------------------

    -- 模式
    GameMode = {
        city = 0,
        map = 1,
        battle = 2
    }

    -- 角色的状态
    RoleState = {
        walkAround = 1,
        idel = 2,
        beakBack = 3,
        searchTarget = 4,
        attack = 5,
        waitAttack = 6,
        dizzy = 7,
    }

    __httpBaseUrl = PStr.b():a("http://"):a(Net.self.gateHost):a(":"):a(tostring(Net.self.gatePort)):e()--192.168.0.18:8084"
end
