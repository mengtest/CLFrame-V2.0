// 引用单元
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection; // For BindingFlags
using System; // Activator
using System.Collections.Generic;
using System.IO; // File
using System.Text.RegularExpressions;
using System.Text;

using UnityEditor.RestService;

public class ExConfig
{
    public Dictionary<string, string> _cfgData;
    string _filename;
    Encoding _encode = Encoding.UTF8;
    public ExConfig() {
        _cfgData = new Dictionary<string,string>();
    }

    public bool Load(string fname)
    {
        _filename = fname;
        if (!File.Exists(fname))
        {
            Debug.Log("CFG>Missing file:'" + fname + "'\n");
            return false;
        }
        StreamReader reader = new StreamReader(fname, _encode);
        string line;
        int indx = 0;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.StartsWith(";") || string.IsNullOrEmpty(line))
                _cfgData.Add(";" + indx++, line);
            else
            {
                string[] key_value = line.Split('=');
                if (key_value.Length >= 2)
                    _cfgData.Add(key_value[0], key_value[1]);
                else
                    _cfgData.Add(";" + indx++, line);
            }
        }
        reader.Close();
        return true;
    }

    public string Get(string key, string defValue="")
    {
        if (_cfgData.Count <= 0)
            return defValue;
        else if(_cfgData.ContainsKey(key))
            return _cfgData[key].ToString();
        else
            return defValue;
    }

    public bool GetBoolean(string key) {
        string v = Get(key);
        v.ToLower();
        return (v == "true" || v == "yes");
    }

    public void SetBoolean(string key, bool v) {
        string txt = v ? "true" : "false";
        Set(key, txt);
    }

    public void Set(string key, string value)
    {
        if (_cfgData.ContainsKey(key))
            _cfgData[key] = value;
        else
            _cfgData.Add(key, value);
    }


    public void SetInt(string key, int value)
    {
        if (_cfgData.ContainsKey(key))
            _cfgData[key] = value.ToString();
        else
            _cfgData.Add(key, value.ToString());
    }

    public int GetInt(string key, int defValue=0)
    {
        if (_cfgData.Count <= 0)
            return defValue;
        else if(_cfgData.ContainsKey(key)) {
            int ret = 0;
            if ( int.TryParse( _cfgData[key].ToString(), out ret ) )
                return ret;
            return defValue;
        } else
            return defValue;
    }

    public void Save()
    {
        StreamWriter writer = new StreamWriter(_filename, false, _encode);
        IDictionaryEnumerator enu = _cfgData.GetEnumerator();
        while (enu.MoveNext())
        {
            if (enu.Key.ToString().StartsWith(";"))
                writer.WriteLine(enu.Value);
            else
                writer.WriteLine(enu.Key + "=" + enu.Value);
        }
        writer.Close();
    }
}

public class ExStackInfoPanel : EditorWindow {
    /// <summary>
    /// 面板配置文件存放位置，可根据需要进行修改
    /// </summary>
	string _configFile = "CoolapeFrameData/cfg/.StackInfoPanel.cfg";

    /// <summary>
    /// Lua编辑器选项列表
    /// </summary>
    string[] _luaEditorSelections = new string[]{"Sublime","Others"};

    /// <summary>
    /// Lua编辑器对应的APP路径已经相关参数，数量与 <see cref="_luaEditorSelections"/>  相同。
    /// </summary>
    /// <remarks>
    /// 注意在参数内容中 %FILE% 代表所打开的文件 %LINE% 为跳转的行号，不同的文
    /// 本编辑器在命令行的设置是不一样的。
    /// </remarks>
    string[][] _luaEditorExe = new string[][] {
        new string[]{
            "/Applications/Sublime Text 2.app/Contents/SharedSupport/bin/subl",
            "%FILE%:%LINE%"},
        
        new string[]{
            "/Applications/Unity/MonoDevelop.app/Contents/MacOS/monodevelop",
            "%FILE%;%LINE%;1"},
    };
    /// <summary>
    /// 用户当前所选择的Lua编辑器APP
    /// </summary>
    int _luaEditor = 0;

    /// <summary>
    /// 保存文件跳转的相关信息
    /// </summary>
	struct SourceEntity {
		public SourceEntity(string desc, string fname, int lno, int flag=0) { 
			displayDesc = desc;
			fileName = fname;
			lineNo = lno;
			fileType = "";
			int pos = fileName.LastIndexOf(".");
			if (pos != -1) {
				fileType = fileName.Substring( pos + 1 );
				fileType.ToLower();
			}
			tipFlag = flag;
		}
		public string displayDesc;
		public string fileName;
		public int lineNo;
		public string fileType;
		public int tipFlag;
	}
	List<SourceEntity> _stackInfos = new List<SourceEntity>();


    /// <summary>
    /// lua文件的存放根路径, 在面板上使用 _luaRootPathObj 存放。
    /// </summary>
	string _luaRootPath = "";


    /// <summary>
    /// lua根路径绝对路径，用来简化lua路径的显示。
    /// </summary>
    string _luaPathHead;

    /// <summary>
    /// lua文件的存放根路径，asset对象，方便通过拖放设置。与之对应的是变量
    /// _luaRootPath.
    /// </summary>
    UnityEngine.Object _luaRootPathObj;

    /// <summary>
    /// 获取lua存放根路径的成员函数, 封装了 _luaPathHead 的更新.
    /// </summary>
    string luaRootPath { get { return _luaRootPath; }
        set {
            if (!String.IsNullOrEmpty(value) )
            {
                string key = "Assets/";
                int pos = value.IndexOf(key) + key.Length;
                _luaPathHead = Application.dataPath + "/" + value.Substring(pos);
            }
            _luaRootPath = value;
        }
    }
    /// <summary>
    /// Lua 文件自动查找列表，部分对lua文件的加载不会自动匹配文件夹，这里需要设置，可以看到
    /// 发生这些问题的文件，都是绑定在GameObject上的。
    /// </summary>
    List<string> _searchList = new List<string>() { "", "ui/panel/", "ui/cell/" };

    /// <summary>
    /// 面板列表需要
    /// </summary>
	Vector2 _scrollpos = Vector2.zero;

    /// <summary>
    /// 提示信息，在面板中间，效果与日志不同。
    /// </summary>
	string _tipContent = "";

    readonly string[] _labels = new string[] { // _labels[10]
        "堆栈信息 Stack",
        "Lua编辑器路径",
        "编辑器参数",
        "开启监听",
        "关闭监听", // 0-4
        "设置 Setting",                 // 5
        "配置文件(只读)",                // 6
        "拖放LUA根目录的文件夹到该处:",   // 7
        "选择Lua编辑器",                 // 8
        "错误",                          // 9
        "请配置LUAROOT所在目录，建议采用拖放的操作来设置！",     //  
        "所指定的Lua 编辑器路径,请检查！\n  ",               //    11
        "保存当前的设置", // 12
        "加载配置文件", // 13
        "未实现功能", // 14
        "获取当前所选择日志的堆栈信息", // 15
        "",
    };

    /// <summary>
    /// 判断OnOpenAsset的操作是否由面板自身发起的
    /// </summary>
    UnityEngine.Object _configFileObj = null;

    /// <summary>
    /// 判断OnOpenAsset的操作是否由面板自身发起的
    /// </summary>
    public bool _inMonoScripting = false;

    /// <summary>
    /// 是否开启监控双击事件
    /// </summary>
    public bool _enableOpenCallback = true;

    /// <summary>
    /// 是否需要从日志面板中重新解析堆栈数据。
    /// 说明：双击日志会影响 ConsoleWindow 的成员变量，直接在 OnOpenAsset 中调用的话
    ///      会出现异常，所以才采用在 OnGUI 进行日志解析的操作。
    /// </summary>
    bool _needRefresh = false;
    /// <summary>
    /// 启动入口
    /// </summary>
	void OnEnable()
    {
        string p = GetAssetPath(_configFile);
        _configFileObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(p);
        LoadConfig();
    }
    /// <summary>
    /// Raises the GU event.
    /// </summary>
    void OnGUI ()
    {
        if (_needRefresh) {
            _needRefresh = false;
            // 解析当前的日志
            DoRefreshLogStackInfo();
        }

        _scrollpos = EditorGUILayout.BeginScrollView (_scrollpos);

        if (NGUIEditorTools.DrawHeader(_labels[5] ))
        {
            EditorGUIUtility.labelWidth = 90;
            // 配置文件所在位置（只读）
            EditorGUILayout.ObjectField(_labels[6], _configFileObj, typeof(UnityEngine.Object), false);

            string tip = _labels[7];
            GUI.changed = false;
            GUILayout.Label(tip, GUILayout.MinWidth(100f));
            _luaRootPathObj = EditorGUILayout.ObjectField("Lua Root", 
                _luaRootPathObj, typeof(UnityEngine.Object), false) as UnityEngine.Object;

            if (GUI.changed) {
                GUI.changed = false;

                string atpath = AssetDatabase.GetAssetPath( _luaRootPathObj );
                if ( Directory.Exists(atpath) ) {
                    tip = "STACK>" + atpath + "(" + _luaRootPathObj.ToString() + ")\n";
                    luaRootPath = atpath;
                } else if ( File.Exists(atpath) ) {
                    atpath = Path.GetDirectoryName(atpath);
                    tip = "STACK>LUAPATH changed to '" + atpath + "'\n";
                    luaRootPath = atpath;
                } else {
                    tip = "STACK> not a folder:'" + atpath + "'\n";
                }
                _tipContent = tip;
            }

            // GUILayout.Label("LUA PATH:" + luaRootPath, GUILayout.MinWidth(100f));
            GUI.changed = false;
            int sel = DrawList("Lua编辑器选择", _luaEditorSelections, _luaEditor);
            if (GUI.changed) _luaEditor = sel;

            if (sel >= 0 && sel < _luaEditorExe.Length)
            {
                string[] info = _luaEditorExe[sel];
                GUI.changed = false;
                info[0] = EditorGUILayout.TextField(_labels[1], info[0], GUILayout.MinWidth(100f));
                info[1] = EditorGUILayout.TextField(_labels[2], info[1], GUILayout.MinWidth(100f));
            }

            GUILayout.BeginHorizontal ();
            if (GUILayout.Button (new GUIContent("Save", "保存配置信息"), GUILayout.Width (80))) {
                SaveConfig();
            }
            if (GUILayout.Button (new GUIContent("Load", "读取本面板的配置信息" ), GUILayout.Width (80))) {
                LoadConfig();
            }
            GUILayout.EndHorizontal ();
        }

        GUILayout.Label("." + _tipContent, GUILayout.MinWidth(100f));

        if (NGUIEditorTools.DrawHeader(_labels[0]))
        {
            GUILayout.BeginHorizontal ();
            {
                string enableLab = _enableOpenCallback ? _labels[4] : _labels[3];
                if (GUILayout.Button (new GUIContent(enableLab, "开启或关闭对双击打开文件的监控"), GUILayout.Width (80))) {
                    _enableOpenCallback = !_enableOpenCallback;
                    Repaint();
                }

                if (GUILayout.Button (new GUIContent("解析日志", "解析所选日志中的堆栈数据"), GUILayout.Width (80))) {
                    DoRefreshLogStackInfo();
                }

                if (GUILayout.Button (new GUIContent("关闭", "关闭窗口"), GUILayout.Width (80))) {
                    DoCloseWindow();
                }

//                if (GUILayout.Button (new GUIContent("Test", _labels[14]), GUILayout.Width (80))) {
//                    DoTest1();
//                }
            }
            GUILayout.EndHorizontal ();


            if (_stackInfos.Count>0) {
                for (int i = 0; i < _stackInfos.Count; ++i) {
                    SourceEntity se = _stackInfos[i];
                    string lab = "[" + se.lineNo + "]";
                    string desc = se.tipFlag + "|" + se.displayDesc;
                    GUILayout.BeginHorizontal ();
                    if (GUILayout.Button (new GUIContent(lab, se.fileName), GUILayout.Width (80))) {
                        OpenFileWithLineNo(se);
                    }
                    GUILayout.Label(desc, GUILayout.MinWidth(100f));
                    GUILayout.EndHorizontal ();
                }
            }
        }
        EditorGUILayout.EndScrollView ();
    }

    /// <summary>
    /// 绘制 ComboxButton, 返回序号
    /// </summary>
    /// <returns>The list.</returns>
    /// <param name="field">Field.</param>
    /// <param name="list">List.</param>
    /// <param name="selection">Selection.</param>
    /// <param name="options">Options.</param>
    int DrawList (string field, string[] list, int selection, params GUILayoutOption[] options)
    {
        if (list != null && list.Length > 0)
        {
            if (selection < 0 || selection >= list.Length)
                selection = 0;
            return EditorGUILayout.Popup(field, selection, list, options);
        }
        return 0;
    }
    /// <summary>
    /// 获取AssetDatabase 可以直接加载的路径名，以'Asset/'开头
    /// </summary>
    /// <returns>The asset path.</returns>
    /// <param name="path">Path.</param>
    string GetAssetPath(string path) {
        const string key = "Assets/";
        do {
            if (path.Length < 1 ) {
                break;
            }
            int pos = path.IndexOf(key);
            if (pos == -1) {
                path = key + path.TrimStart("/".ToCharArray());
                break;
            }
            path = path.Substring(pos);
        } while(false);
        return path;
    }
    /// <summary>
    /// 获取完整的绝对路径
    /// </summary>
    /// <returns>The full path.</returns>
    /// <param name="filename">Filename.</param>
    string GetFullPath(string filename) {
        string head = Application.dataPath + "/";
//        Debug.Log( "FILE=" + filename + "\n");
//        Debug.Log( "HEAD=" + head + "\n");
//        Debug.Log( "TEST=" + filename.IndexOf(head) + "\n");
        if (filename.IndexOf(head) != -1)
            return filename;
        return head + filename;
    }
    /// <summary>
    /// 获取文件名主体
    /// </summary>
    /// <returns>The file base.</returns>
    /// <param name="fileName">File name.</param>
    string GetFileBase(string fileName) {
        Match m = null;
        string regfname = @"^(?<fpath>(/)([\s\.\-\w]+/)*)(?<fname>[\w]+)(?<namext>(\.[\w]+)*)";
        m = Regex.Match(fileName, regfname);
        if (m.Success) {
            //          print("fpath=" + m.Result("${fpath}") );
            //          print("fname=" + m.Result("${fname}") );
            //          print("namext=" + m.Result("${namext}") );
            return m.Result("${fname}") + m.Result("${namext}");
        }
        return fileName;
    }

    /// <summary>
    ///  修正lua路径，主要是针对luac，其会产生绝对的文件路径，这里做个修正
    /// </summary>
    /// <returns>The lua filename.</returns>
    /// <param name="luafname">Luafname.</param>
    private string FixLuaFilename(string luafname) {
        int pos = -1;
        if ( (pos=luafname.IndexOf(_luaPathHead)) != -1 ) {
            luafname = luafname.Substring(pos + _luaPathHead.Length);
            Debug.Log("Fixed:" + luafname + "\n");
        }

        if (luafname.IndexOf(".lua") == -1 )
        {
            luafname = luafname.Replace(".", "/") + ".lua";
        }
        return luafname;
    }
    bool LoadConfig() {
        string fullpath = GetFullPath(_configFile);
        if (File.Exists(fullpath)) {
            ExConfig cfg = new ExConfig();
            if ( cfg.Load(fullpath) ) {
                luaRootPath = cfg.Get("LUAROOT", luaRootPath);
                _luaEditor = cfg.GetInt("LUAEDITOR", _luaEditor);

                int cnt = _luaEditorExe.Length;
                for (int i = 0; i < cnt; i++) {
                    string[] info = _luaEditorExe[i];
                    string exename = "LUAEDITOR_DIR" + i;
                    string paramname = "LUAEDITOR_PARAM" + i;
                    info[0] = cfg.Get(exename, info[0]);
                    info[1] = cfg.Get(paramname, info[1]);
                }

                if (Directory.Exists(luaRootPath) ) {
                    _luaRootPathObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(luaRootPath);
                }
                return true;
            }
        }
        return false;
    }

    void SaveConfig() {
        if (luaRootPath.Length < 1 || !Directory.Exists(luaRootPath)) {
            MsgBox(_labels[10], _labels[9]);
            return;
        }

        int edidx = _luaEditor;
        string edtor = _luaEditorExe[edidx][0];
        if (edtor.Length < 1 || !File.Exists(edtor)) {
            MsgBox(_labels[11] + edtor, _labels[9]);
            return;
        }

        string fullpath = GetFullPath(_configFile);
        ExConfig cfg = new ExConfig();
        cfg.Load(fullpath);
        cfg.Set("LUAROOT", luaRootPath);
        cfg.SetInt("LUAEDITOR", _luaEditor);

        int cnt = _luaEditorExe.Length;
        for (int i = 0; i < cnt; i++) {
            string[] info = _luaEditorExe[i];
            string exename = "LUAEDITOR_DIR" + i;
            string paramname = "LUAEDITOR_PARAM" + i;
            cfg.Set(exename, info[0]);
            cfg.Set(paramname, info[1]);
        }
        cfg.Save();
        Debug.Log("Save Stack panel config:'" + fullpath + "'\n");
    }

    void MsgBox(string msg, string title="INFO") {
        EditorUtility.DisplayDialog (title, msg, "OK");
    }

	void DoSomething()
	{
		string tip = "XLua.LuaException: error loading module toolkit.CLLVerManager from CustomLoader, toolkit.CLLVerManager:9: unfinished string near '')'";
		Debug.Log(tip +"\n");

        string p = GetAssetPath(_configFile);
        _configFileObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(p);
        Repaint();

        Debug.Log(p +"\n");
	}

	void DoCloseWindow()
	{
		Close();
	}
    /*
    void DoTest1() {

        Assembly ueditorAsm = Assembly.GetAssembly(typeof(EditorWindow));  
        Type restreqType = ueditorAsm.GetType("UnityEditor.RestService.RestRequest");
        if (restreqType == null) {
            Debug.Log( ">restreqType == null" + "\n");
            return;
        }

        Type ses = ueditorAsm.GetType("UnityEditor.RestService.ScriptEditorSettings");
        if (ses == null) {
            Debug.Log( ">ScriptEditorSettings == null" + "\n");
            return;
        }
        PropertyInfo surl = ses.GetProperty("ServerURL", BindingFlags.Static | BindingFlags.NonPublic| BindingFlags.Public);
        string strurl = (string) surl.GetValue(null, new object[0]);
        Debug.Log( ">ServerURL:" + strurl + "\n");


        PropertyInfo fpath = ses.GetProperty("FilePath", BindingFlags.Static | BindingFlags.NonPublic| BindingFlags.Public);
        string str_fpath = (string) fpath.GetValue(null, new object[0]);
        Debug.Log( ">FilePath:" + str_fpath + "\n");

        MethodInfo mhSend = restreqType.GetMethod("Send", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic| BindingFlags.Public);
        if (mhSend == null) {
            Debug.Log( ">mhSend == null" + "\n");
            return;
        }

        string text = "/works/ex01.txt";
        int line = 4;
        string playload = string.Concat(new object[]
            {
                "{ \"file\" : \"",
                text,
                "\", \"line\" : ",
                line,
                " }"
            });

        // RestRequest.Send("/openfile", playload, 5000)
        // public static bool Send(string endpoint, string payload, int timeout)
        object ret = mhSend.Invoke(null, new object[3] { "/openfile", playload, (int)5000 } );

        Debug.Log("Is Successed! ret=" + ret.ToString() + "\n");
    }

*/
	void DoOutputString() {
		string text = "\t[string \"/CLMainLua.lua\"]:14: in main chunk";
		Debug.Log(text +"\n");
	}

    public void DoRefreshLogStackInfo()
    {
        int row = GetConsoleCurLine();
        if (row != -1) {

            _stackInfos.Clear();
            string context = GetSourceText(row);
            //          Debug.Log("--\n"+ context +"\n--");
            ParseStackLog( context );
            Repaint();
        }
    }

	string SearchLuaFile(string relative) {
        // 在加密等情况下会出现绝对路径，这里就需要处理一下了。
        if (relative.IndexOf(Application.dataPath ) != -1)
            return relative;

        string msg = "";
        string filePath;
		int count = _searchList.Count;
		for (int i = 0; i < count; i++) {
            filePath = _luaPathHead + "/" + _searchList[i] + relative;
            msg += "\n  [" + i + "] " + filePath;
			if (File.Exists(filePath)) {
				return filePath;
			}
		}
        Debug.Log("STACK>Not found, Check list:" + msg);
		return "";
	}

	void OpenLuaFileByLine(string fileName, int fileLineNumber)
	{
		string rfn = SearchLuaFile(fileName);
		if (rfn.Length < 1) {
			Debug.LogError("Not found lua:\n   " + fileName);
			return;
		}
        if(!File.Exists(rfn)) {
            Debug.LogError("Not found:" + rfn);
            return;
        }

		// 启用前需要设置软链接
		// ln -s "/Applications/Sublime Text 2.app/Contents/SharedSupport/bin/subl" ~/bin/subl
		// exprot PATH=$PATH:~/bin
		// subl /works/unity/example01/xlua_ex01/Assets/GalaxyEdge/upgradeResMedium/priority/lua/CLMainLua.lua:14
        // 软链接不起作用，还是直接使用绝对地址才有作用


        // /Applications/MacVim.app/Contents/MacOS/MacVim
        if (_luaEditor >= 0 && _luaEditor <= 2) {
            string[] exeinfo = _luaEditorExe[_luaEditor];
            string exe = exeinfo[0];
            string arguments = exeinfo[1];

            arguments = arguments.Replace("%FILE%", rfn );
            arguments = arguments.Replace("%LINE%", fileLineNumber.ToString() );

            System.Diagnostics.Process.Start(exe, arguments);

//            string arguments = rfn + ":" + fileLineNumber;
//            System.Diagnostics.Process.Start(_luaEditorExe, arguments);

        } else {
            Debug.Log("Config is wrong,please reset it.");
        }
	}

	void OpenAssetByLine(string fileName, int fileLineNumber)
	{
		if (fileName != null)  
		{  
			_inMonoScripting = true;
			string fileAssetPath = fileName.Substring(fileName.IndexOf("Assets"));  
			bool ret = AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(fileAssetPath), fileLineNumber);  
			if (!ret) {
				Debug.Log("Open File failed:\n    " + fileAssetPath);
			}
		}  
	}

	void OpenFileWithLineNo(SourceEntity se)
	{
		if (se.fileType == "cs" ) {
			OpenAssetByLine(se.fileName, se.lineNo);
		}
		else if (se.fileType == "lua" ) {
			OpenLuaFileByLine(se.fileName, se.lineNo);
		}
		else {
			// DoSomething();
		}		
	}

    /// <summary>
    /// 解析日志，根据不同的正则表达式来生成文件跳转信息表
    /// </summary>
    /// <param name="context">Context.</param>
	private void ParseStackLog(string context)
	{
        if (_luaPathHead==null || String.IsNullOrEmpty(_luaPathHead) ) {
            LoadConfig();
        }

        Match m = null;
		string[] arraytext = context.Split(new char[]{'\n'}); // Regex.Split(context, "\n");
		int cnt = arraytext.Length;
        string luafname;
        string reg0;
		for (int i = 0; i <cnt; i++) {
			string txt = arraytext[i];

            // Example/upgradeRes/priority/lua/CLLMainLua.lua,XLua.LuaException: [string "chunk"]:30: attempt to concatenate a nil value (field 'mode')
            reg0 = "upgradeRes/priority/lua(.+),XLua\\.LuaException: \\[string \"chunk\"\\]:(\\d+): (.+)$";
			m = Regex.Match(txt, reg0);
			if (m.Groups.Count>3) {
                luafname = FixLuaFilename(m.Groups[1].Value);
                string msg = GetFileBase(luafname );
				msg += " | ";
				msg += m.Groups[2].Value;
				msg += " " + m.Groups[3].Value;
                _stackInfos.Add(new SourceEntity(msg, luafname, int.Parse( m.Groups[2].Value), 2));
				continue;
			}

			// XLua.LuaException: [string "/CLMainLua.lua"]:89: attempt to index a nil value (global 'CLPanelManager')
			// string reg2 = "XLua.LuaException: \\[string \"(.+)\"\\]:(\\d.+): (.+)$";
			string reg2 = "\\[string \"(.+)\"\\]:(\\d+): (.+)$";
			m = Regex.Match(txt, reg2);
			if (m.Groups.Count>3) {
                luafname = FixLuaFilename(m.Groups[1].Value);
                string msg = GetFileBase(luafname );
				msg += " | ";
				msg += m.Groups[2].Value;
				msg += " " + m.Groups[3].Value;
                _stackInfos.Add(new SourceEntity(msg, luafname, int.Parse( m.Groups[2].Value), 2));
				continue;
			}

			// XLua.LuaException: public.CLLIncludeEx:117: attempt to index a nil value (global 'SimpleAI')
			// XLua.LuaException: error loading module toolkit.CLLVerManager from CustomLoader, toolkit.CLLVerManager:9: unfinished string near '')'
			string reg3 = "(\\S+):(\\d+): (.+)$";
			m = Regex.Match(txt, reg3);
			if (m.Groups.Count>3) {
                luafname = FixLuaFilename(m.Groups[1].Value);
				string msg = luafname;
				msg += " | ";
				msg += m.Groups[2].Value;
				msg += " " + m.Groups[3].Value;
                _stackInfos.Add(new SourceEntity(msg, luafname, int.Parse( m.Groups[2].Value), 3));
				continue;
			}
			// lua 匹配
			string reg1 = "\"";
			m = Regex.Match(txt, "\\[string \"(.+.lua)" + reg1 + "\\]:(\\d+): (in.+)$");
			if (m != null && m.Groups.Count>3) {
                luafname = FixLuaFilename(m.Groups[1].Value);
				string msg = m.Groups[1].Value;
				msg += " | ";
				msg += m.Groups[2].Value;
				msg += " " + m.Groups[3].Value;
                _stackInfos.Add(new SourceEntity(msg, luafname, int.Parse( m.Groups[2].Value), 1));
				continue;
			} 


			// C# 匹配 
			// ExDebugPanel:GetConsoleCurLine() (at Assets/Editor/ExDebugPanel.cs:142)
			string regcshap5 = @"(.+) \(at (.+\.cs):(\d+)";
			m = Regex.Match(txt, regcshap5 );
			if (m!= null&& m.Groups.Count>3) {
				string msg = "";
				msg+= m.Groups[2].Value;
				msg+= " | ";
				msg+= m.Groups[3].Value;
				msg+= "[" + m.Groups[1].Value + "]";
				_stackInfos.Add(new SourceEntity(msg, m.Groups[2].Value, int.Parse( m.Groups[3].Value), 5));
				continue;
			}
			// C# 匹配
			// at XLua.LuaFunction.Call (System.Object[] args) [0x00000] in /works/unity/example01/xlua_ex01/Assets/XLua/Src/LuaFunction.cs:183
			string regcshap7 = @"at ([^\\(]+).+ in (.+\.cs):(\d+)";
			m = Regex.Match(txt, regcshap7 );
			if (m!= null&& m.Groups.Count>3) {
				string msg = "";
				msg+= GetFileBase(m.Groups[2].Value);
				msg+= " | ";
				msg+= m.Groups[3].Value;
				msg+= "[" + m.Groups[1].Value + "]";
				_stackInfos.Add(new SourceEntity(msg, m.Groups[2].Value, int.Parse( m.Groups[3].Value), 7));
				continue;
			}


			// lua 匹配
			//	public.CLLIncludeEx:117: in main chunk
			// toolkit.CLLUpdateUpgrader:7: in main chunk
			string reg4 = "\t(.+):(\\d+): (.+)$";
			m = Regex.Match(txt, reg4);
			if (m.Groups.Count>3) {
                luafname = FixLuaFilename( m.Groups[1].Value);
                string msg = GetFileBase(luafname);
				msg += " | ";
				msg += m.Groups[2].Value;
				msg += " " + m.Groups[3].Value;
				_stackInfos.Add(new SourceEntity(msg, luafname, int.Parse( m.Groups[2].Value), 4));
				continue;
			}

			// [C]: in method 'setData'
			string reg6 = "\\t\\[C\\]:(.+)$";
			m = Regex.Match(txt, reg6);
			if (m.Groups.Count>1) {
				luafname = "C";
				string msg = luafname;
				msg += " | ";
				msg += m.Groups[1].Value;
				_stackInfos.Add(new SourceEntity(msg, luafname, 1, 6));
				continue;
			}
		}

        if (_stackInfos.Count == 0 && !string.IsNullOrEmpty(_lastAssetScript) ) {
            _stackInfos.Add(new SourceEntity( GetFileBase(_lastAssetScript), _lastAssetScript, _lastAssetScriptLine, 100));
        }
	}
	
	public void SetNeedRefresh(bool refresh)
	{
		_needRefresh = refresh;
		Repaint();
	}

	public void SetTipText(string tiptext)
	{
		_tipContent = tiptext;
        Repaint();
	}

    string _lastAssetScript;
    int _lastAssetScriptLine;

    public void SetAssetOpenInfo(string name, int line)
    {
        _lastAssetScript = name;
        _lastAssetScriptLine = line;
    }

    private object _consoleWindow;  
    private object _logListView;  
    //  private static FieldInfo s_LogListViewTotalRows;  
    private FieldInfo _logListViewCurrentRow;  
    private MethodInfo _logEntriesGetEntry;  
    private object _logEntry;  
    // instanceId 非UnityEngine.Object的运行时 InstanceID 为零所以只能用 LogEntry.Condition 判断  
    //  private static FieldInfo s_LogEntryInstanceId;  
    //  private static FieldInfo s_LogEntryLine;  
    //  private static FieldInfo s_LogEntryCondition; 

    /// <summary>
    /// 获取Log日志中的堆栈信息，代码来自网络
    /// </summary>
    /// <returns>The source text.</returns>
    /// <param name="row">Row.</param>
	string GetSourceText(int row)
	{
		var LogEntriesType = typeof(EditorWindow).Assembly.GetType("UnityEditorInternal.LogEntries");
		var startGettingEntriesMethod = LogEntriesType.GetMethod("StartGettingEntries", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		var endGettingEntriesMethod = LogEntriesType.GetMethod("EndGettingEntries", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic); 

		startGettingEntriesMethod.Invoke(null, new object[0]);
		var GetEntryInternalMethod = LogEntriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		var logEntryType = typeof(EditorWindow).Assembly.GetType("UnityEditorInternal.LogEntry");

		var logEntry = Activator.CreateInstance(logEntryType);
		//Get detail debug info.
		GetEntryInternalMethod.Invoke(null, new object[2] { row, logEntry });
		//More info please search "UnityEditorInternal.LogEntry" class of ILSPY.
		var fieldInfo = logEntryType.GetField("condition", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		var result = fieldInfo.GetValue(logEntry).ToString();
		endGettingEntriesMethod.Invoke(null, new object[0]);

		return result;
	}

	int GetCount()
	{
		var debugType = typeof(EditorWindow).Assembly.GetType("UnityEditorInternal.LogEntries");
		var methodInfo = debugType.GetMethod("GetCount", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		return (int)methodInfo.Invoke(null, new object[0]);
	}

	private void GetConsoleWindowListView()  
	{  
		if (_logListView == null)  
		{
			Assembly unityEditorAssembly = Assembly.GetAssembly(typeof(EditorWindow));  
			Type consoleWindowType = unityEditorAssembly.GetType("UnityEditor.ConsoleWindow");  
			FieldInfo fieldInfo = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);  
			_consoleWindow = fieldInfo.GetValue(null);  
			FieldInfo listViewFieldInfo = consoleWindowType.GetField("m_ListView", BindingFlags.Instance | BindingFlags.NonPublic);  
			_logListView = listViewFieldInfo.GetValue(_consoleWindow);  
//			s_LogListViewTotalRows = listViewFieldInfo.FieldType.GetField("totalRows", BindingFlags.Instance | BindingFlags.Public);  
			_logListViewCurrentRow = listViewFieldInfo.FieldType.GetField("row", BindingFlags.Instance | BindingFlags.Public);  

//			//LogEntries  
//			Type logEntriesType = unityEditorAssembly.GetType("UnityEditorInternal.LogEntries");  
//			s_LogEntriesGetEntry = logEntriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public);  
//			Type logEntryType = unityEditorAssembly.GetType("UnityEditorInternal.LogEntry");  
//			s_LogEntry = Activator.CreateInstance(logEntryType);  
//			s_LogEntryInstanceId = logEntryType.GetField("instanceID", BindingFlags.Instance | BindingFlags.Public);  
//			s_LogEntryLine = logEntryType.GetField("line", BindingFlags.Instance | BindingFlags.Public);  
//			s_LogEntryCondition = logEntryType.GetField("condition", BindingFlags.Instance | BindingFlags.Public);  
		}
	}

	int GetConsoleCurLine()
	{
		if (_logListView == null)
			GetConsoleWindowListView();

		if (_logListViewCurrentRow != null ) {
			int num = (int)_logListViewCurrentRow.GetValue(_logListView);
//			Debug.Log("Current row=" + num);
			return num;
		} else {
			Debug.Log("No Console List View catched.");
		}
		return -1;
	}

    private static bool OpenScriptFile(int instanceID, int line)
    {
        
//        string text = Path.GetFullPath(Application.dataPath + "/../" + AssetDatabase.GetAssetPath(instanceID)).Replace('\\', '/');
//        string text2 = text.ToLower();
//        if (!text2.EndsWith(".cs") && !text2.EndsWith(".js") && !text2.EndsWith(".boo"))
//        {
//            return false;
//        }
//
//        if (!PairingRestHandler.IsScriptEditorRunning() || 
//            !RestRequest.Send("/openfile", string.Concat(new object[]
//            {
//                "{ \"file\" : \"",
//                text,
//                "\", \"line\" : ",
//                line,
//                " }"
//            }), 5000))
//        {
//            return false;
//        }
        return true;
    }

	// 双击日志的跳转处理重载
	[UnityEditor.Callbacks.OnOpenAssetAttribute(2)]  
	public static bool OnOpenAsset(int instanceID, int line)  
	{
		// 不是从控制台面板进入的直接忽略
		if (line == -1)
			return false;

		ExStackInfoPanel panel = EditorWindow.GetWindow<ExStackInfoPanel>();

        if (!panel._enableOpenCallback)
            return false;

		// 来自 Panel的调用也将忽略
		// AssetDatabase.OpenAsset 会调用重载的 OnOpenAsset，使用 inMonoScripting来避开
        if (panel._inMonoScripting)
		{
			panel._inMonoScripting = false;
			return false;
		}

        string assetName = Path.GetFullPath(Application.dataPath + "/../" + AssetDatabase.GetAssetPath(instanceID)).Replace('\\', '/');
		string name = EditorUtility.InstanceIDToObject(instanceID).name;
		int pid = panel.GetInstanceID();
		float numb = UnityEngine.Random.value;

        panel.SetAssetOpenInfo(assetName, line);
        panel.SetTipText("-- -- [" + name + "] instid=" + instanceID + " line="+line + " panelid="+pid);
        // 如果在此处调用内部函数来获取日志窗口的内容，会报异常，估计跟日志双击的事件的处理有关联
		panel.SetNeedRefresh(true);
		return true;

		// 参考: Unity日志工具——封装，跳转 http://dsqiu.iteye.com/blog/2263664
	} 


	[MenuItem("Coolape/Tools/Stack Info Console", false, 98)]
	static public void ShowStackInfoConsole()
	{
		EditorWindow.GetWindow<ExStackInfoPanel> (false, "ExDebugPanel", true);
	}

}
