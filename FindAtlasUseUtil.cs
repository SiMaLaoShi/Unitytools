using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using OfficeOpenXml;
using Object = System.Object;
using UObject = UnityEngine.Object;

[CanEditMultipleObjects]
public class FindAtlasUseUtil : EditorWindow
{
    public static List<string> nameArray = new List<string>();

    private static FindAtlasUseUtil window;
    private UIAtlas atlas;
    private List<UIAtlas> atlasList; //存放所有图集，默认只选了common
    
    private readonly string[] atlasPath =
        {"Assets/Resources/UI/Atlas_New/Common", "Assets/Resources/UI/Atlas/UICommon"}; // TODO 指定项目里图集的父节点 需修改

    private readonly uint buttonHeight = 50;
    private List<GameObject> gameObjects;
    private Vector2 mScroll = Vector2.zero;
    private Transform[] selectTrans;
    private readonly uint split = 2;
    private List<UISprite> spriteList; //存放精灵
    private List<string> spritePath;
    private string sprName = ""; //筛选的名字
    private List<string> transName; //存放预设的名字
    public static int CompressQuality = 50;

    private List<SpriteReferenceData> spriteReferenceDatas;

    /// <summary>
    ///     UI预设的路径，也可用选中UI文件夹的方式获取
    /// </summary>
    private string[] uiPath =
    {
        "Assets/Resources/Prefabs/UI/Activity",
        "Assets/Resources/Prefabs/UI/Blunttower",
        "Assets/Resources/Prefabs/UI/Camp",
        "Assets/Resources/Prefabs/UI/CommonAction",
        "Assets/Resources/Prefabs/UI/Dart",
        "Assets/Resources/Prefabs/UI/DouFachang",
        "Assets/Resources/Prefabs/UI/Faction",
        "Assets/Resources/Prefabs/UI/Friend",
        "Assets/Resources/Prefabs/UI/Joystick",
        "Assets/Resources/Prefabs/UI/HeroChallenge"
    };

    #region NGUI Sprite 查找工具
    [MenuItem("Tools/查找NGUI图集引用")]
    private static void CreateWindow()
    {
        window = (FindAtlasUseUtil)GetWindow(typeof(FindAtlasUseUtil), false, "查找图集引用");
        window.Show();
    }


    private static List<string> prefabList = new List<string>();
    private static Dictionary<string, List<string>> bindNames;
    private static System.Timers.Timer timer;
    [MenuItem("Tools/生成模型骨骼绑点配置文件")]
    private static void CreateModelBonesConfig()
    {
        prefabList.Clear();
        bindNames = new Dictionary<string, List<string>>();
        string path = "Assets/Resources/prefabs/Actor/";
        string[] files = Directory.GetFiles(path);
        for (int i = 0; i < files.Length; ++i)
        {
            string file = files[i];
            if (file.EndsWith(".prefab"))
            { 
                prefabList.Add(file);
                //Debug.Log("file:"+file);
            }
        }
        while (prefabList.Count > 0)
        {
            string file = prefabList[prefabList.Count - 1];
            prefabList.RemoveAt(prefabList.Count - 1);
            GameObject prefabGo = AssetDatabase.LoadAssetAtPath(file, typeof(GameObject)) as GameObject;
            string fileName = Path.GetFileNameWithoutExtension(file);
            //Debug.Log("fileName:"+fileName);
            if (fileName == "gw_053" || fileName == "gw_054" || fileName == "gw_053_01" || fileName == "npc_035")
            {
                //镖车特殊处理
                List<string> binds = new List<string>();
                binds.Add("Dummy001");
                for (int i = 0; i < binds.Count; ++i)
                {
                    Transform ts = UnGfx.FindNode(prefabGo.transform, binds[i]);
                    List<string> list = GetListByName(fileName);
                    string p1 = GetParentPath(ts);
                    if (p1 != "")
                    {
                        list.Add(p1);
                    }
                }
            }
            else if (fileName.IndexOf("mount_") != -1)
            {
                //坐骑
                //petbind1
                //petbind2
                List<string> binds = new List<string>();
                binds.Add("petbind1");
                binds.Add("petbind2");
                for (int i = 0; i < binds.Count; ++i)
                {
                    Transform ts = UnGfx.FindNode(prefabGo.transform, binds[i]);
                    List<string> list = GetListByName(fileName);
                    string p1 = GetParentPath(ts);
                    if (p1 != "")
                    {
                        list.Add(p1);
                    }
                }
            }
            else if (fileName.IndexOf("mw_") != -1)
            {
            }
            else if (fileName.IndexOf("m1_h") != -1)
            {

            }
            else if (fileName.IndexOf("m") != -1)
            {
                //玩家
                //Hair
                //Back
                //weaponR
                //TopBip
                //CenterBip
                //ButtomBip
                //BackBip
                //WeaponBip
                //LeftBip
                //RightBip
                List<string> binds = new List<string>();
                binds.Add("Hair");
                binds.Add("Back");
                binds.Add("weaponR");
                binds.Add("TopBip");
                binds.Add("CenterBip");
                binds.Add("ButtomBip");
                binds.Add("BackBip");
                binds.Add("WeaponBip");
                binds.Add("LeftBip");
                binds.Add("RightBip");
                for(int i = 0; i < binds.Count; ++i)
                {
                    Transform ts = UnGfx.FindNode(prefabGo.transform, binds[i]);
                    List<string> list = GetListByName(fileName);
                    string p1 = GetParentPath(ts);
                    if (p1 != "")
                    {
                        list.Add(p1);
                    }
                }
            }
        }
        string luaPath = "Assets/Lua/Actor/ModelBindBonesConfig.lua";

        System.Text.StringBuilder sBd = new System.Text.StringBuilder();
        sBd.Append("local ModelBindBonesConfig = {\n");
        foreach(var item in bindNames)
        {
            if(item.Value.Count == 0 )
            {
                continue;
            }
           // Debug.Log("1==============================");
           // Debug.Log(item.Key);
            sBd.Append("\t['"+item.Key+"'] = ");
            sBd.Append("\n\t{\n");
            List<string> paths = item.Value;
            for(int i = 0; i < paths.Count;++i)
            {
                string[] pts = paths[i].Split('/');
                string key = pts[pts.Length - 1];
                sBd.Append("\t\t['"+key+"'] = ");
                sBd.Append("'");
                sBd.Append(paths[i]);
                sBd.Append("'");
                if(i < paths.Count-1)
                {
                    sBd.Append(",\n");
                }
                Debug.Log(paths[i]);
            }
            sBd.Append("\n\t},\n");
            //Debug.Log("2==============================");
        }
        sBd.Append("\n}\n");
        sBd.Append("return ModelBindBonesConfig");
        File.WriteAllText(luaPath, sBd.ToString());

        Debug.Log(sBd.ToString());

    }

    private static List<string> GetListByName(string fileName)
    {
        List<string> list;
        bindNames.TryGetValue(fileName,out list);
        if (list == null)
        {
            list = new List<string>();
            bindNames.Add(fileName,list);
        }
        return list;
    }
    private static string GetParentPath(Transform ts)
    {
        if (ts == null)
        {
            return "";
        }
        
        Transform parent = ts.parent;
        System.Text.StringBuilder sBd = new System.Text.StringBuilder();
        
        sBd.Insert(0, parent.name);
        
        while (parent.parent)
        {
            string nn = parent.parent.name;
            parent = parent.parent;
            if (parent.parent != null)
            {
                sBd.Insert(0, "/");
                sBd.Insert(0, nn);
            }
        }
        sBd.Append("/");
        sBd.Append(ts.name);
        return sBd.ToString();
    }
    private void OnEnable()
    {
        spriteList = new List<UISprite>();
        spriteReferenceDatas = new List<SpriteReferenceData>();
    }

    private void OnDisable()
    {
        spriteList = null;
        spriteReferenceDatas = null;
    }

    private bool bShowAtlas = false;

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("需要查询的atlas：", GUILayout.Width(200f));
        atlas = EditorGUILayout.ObjectField("", atlas, typeof(UIAtlas), true, GUILayout.Width(200f)) as UIAtlas;
        if (NGUIEditorTools.DrawPrefixButton("atlas"))
            ComponentSelector.Show<UIAtlas>(OnSelectAtlas);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("需要查询的sprite(空为全部查找)：", GUILayout.Width(200f));
        sprName = GUILayout.TextArea(sprName, 30, GUILayout.Width(200f));
        if (NGUIEditorTools.DrawPrefixButton("sprite"))
        {
            NGUISettings.atlas = atlas;
            SpriteSelector.Show(OnSelectSprite);
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("可多选“Hierarchy”、“Project”面板的预设体");

        
        bShowAtlas = GUILayout.Toggle(bShowAtlas, new GUIContent("显示图集所有图片"));
        if (GUILayout.Button("clear"))
        {
            SpriteReferenceData.spriteList = new List<string>();
            spriteList = new List<UISprite>();
            spriteReferenceDatas = new List<SpriteReferenceData>();/**/
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("查找引用选中对象是否引用", GUILayout.Width(Screen.width / split))) FindUse();

        if (GUILayout.Button("查找atlas被谁使用", GUILayout.Width(Screen.width / split)))
        {
            GetSystemUIPath();
            FindAtlasUseDatail();
        }

        

        GUILayout.EndHorizontal();

        if (spriteReferenceDatas.Count > 0 && atlas != null)
        {
            if (sprName == "")
                if (GUILayout.Button("导出信息"))
                    Export();
            ShowFindUse();
        }
    }

    void Export()
    {
        var saveDic = Environment.CurrentDirectory + "/图集引用信息";
        if (!Directory.Exists(saveDic))
            Directory.CreateDirectory(saveDic);

        string savePath = saveDic + "/" + atlas.name + ".xlsx";
        if (File.Exists(savePath))
            File.Delete(savePath);
        FileInfo info = new FileInfo(savePath);
        using (var package = new ExcelPackage(info))
        {
            var workSheet = package.Workbook.Worksheets.Add(atlas.name);
            workSheet.Cells[1, 1].Value = "美术资源名";
            workSheet.Cells[1, 2].Value = "引用路径";
            workSheet.Cells[1, 3].Value = "Prefab";
            workSheet.Cells[1, 4].Value = "Sprite";

            using (var space = workSheet.Cells[1,1,1,4])
            {
                space.Style.Font.Bold = true;
            }

            int count = 2;
            foreach (var data in spriteReferenceDatas)
            {
                workSheet.Cells[count, 1].Value = data.sprite.spriteName;
                workSheet.Cells[count, 2].Value = data.referencePath;
                workSheet.Cells[count, 3].Value = data.prefab.name;
                workSheet.Cells[count, 4].Value = data.sprite.name;
                count++;
                EditorUtility.DisplayProgressBar("配置导出excel", atlas.name, (float)(count - 2) / spriteReferenceDatas.Count);
            }
            EditorUtility.ClearProgressBar();
            package.Save();
        }
    }

    private void OnSelectAtlas(UObject obj)
    {
        atlas = obj as UIAtlas;
        sprName = "";
    }

    private void OnSelectSprite(string spriteName)
    {
        sprName = spriteName;
    }

    private void ChooseAtlas()
    {
        if (atlasList == null)
        {
            atlasList = new List<UIAtlas>();
            // 第二个参数为图集的目录位置数组，可以指定项目里存放图集的父节点。切记不要在Assets节点下查找，这样会遍历所有物体，会很卡的~~
            var guids = AssetDatabase.FindAssets("t:GameObject", atlasPath);
            var paths = new List<string>();
            guids.ToList().ForEach(m => paths.Add(AssetDatabase.GUIDToAssetPath(m)));
            paths.ForEach(p => atlasList.Add(AssetDatabase.LoadAssetAtPath(p, typeof(UIAtlas)) as UIAtlas));
            // 移除Null值
            for (var i = 0; i < atlasList.Count; i++)
                if (i < atlasList.Count && atlasList[i] == null)
                {
                    atlasList.Remove(atlasList[i]);
                    i--;
                }
        }
    }

    private void FindAtlasUseDatail()
    {
        if (atlas == null)
        {
            this.ShowNotification(new GUIContent("没有选择图集"));
            return;
        }

        var guid = AssetDatabase.FindAssets("t:Prefab", uiPath);

        spriteReferenceDatas = new List<SpriteReferenceData>();
        SpriteReferenceData.spriteList = new List<string>();

        for (var i = 0; i < guid.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid[i]);
            var go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            var sprites = go.transform.GetComponentsInChildren<UISprite>(true);
            for (var j = 0; j < sprites.Length; j++)
                if (sprites[j] != null && sprites[j].atlas == atlas)
                {
                    if (sprName != "" && sprName != sprites[j].spriteName)
                        continue;
                    SpriteReferenceData data = new SpriteReferenceData();
                    data.sprite = sprites[j];
                    data.prefab = go;
                    data.referencePath = GetCompletePath(sprites[j].transform);
                    spriteReferenceDatas.Add(data);
                    SpriteReferenceData.AddSprite(sprites[j].spriteName);
                }

            EditorUtility.DisplayProgressBar("查找预设体中", string.Format("{0}/{1}", i, guid.Length), (float)i / guid.Length);
        }

        EditorUtility.ClearProgressBar();

        if (bShowAtlas)
        {
            foreach (var spriteData in atlas.spriteList)
            {
                if (!SpriteReferenceData.spriteList.Contains(spriteData.name))
                {
                    SpriteReferenceData data = new SpriteReferenceData();
                    data.sprite = new UISprite();
                    data.sprite.spriteName = spriteData.name;
                    data.prefab = null;
                    data.referencePath = "no use";
                    spriteReferenceDatas.Add(data);
                    SpriteReferenceData.AddSprite(spriteData.name);
                }
            }
        }
        spriteReferenceDatas.Sort((x, y) => { return string.Compare(x.sprite.spriteName, y.sprite.spriteName); });
    }

    private static string GetCompletePath(Transform node)
    {
        var path = "/" + node.name;
        while (node.parent != null)
        {
            node = node.parent;
            path = "/" + node.name + path;
        }
        
        Debug.Log("path:" + path.Remove(0,1));
        return path.Remove(0,1);
    }

    /// <summary>
    ///     获取UI下的所有文件夹并赋值给uipath
    /// </summary>
    private void GetSystemUIPath()
    {
        var path = "Assets/Resources/Prefabs/UI/";
        try
        {
            uiPath = Directory.GetDirectories(path);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    ///     根据指定的 Assets下的文件路径 返回这个路径下的所有文件名//
    /// </summary>
    /// <returns>文件名数组</returns>
    /// <param name="path">Assets下“一"级路径</param>
    /// <param name="pattern">筛选文件后缀名的条件.</param>
    /// <typeparam name="T">函数模板的类型名t</typeparam>
    private void GetObjectNameToArray<T>(string path, string pattern)
    {
        var objPath = Application.dataPath + "/" + path;
        string[] directoryEntries;
        try
        {
            //返回指定的目录中文件和子目录的名称的数组或空数组
            directoryEntries = Directory.GetFileSystemEntries(objPath);
            for (var i = 0; i < directoryEntries.Length; i++)
            {
                var p = directoryEntries[i];
                //得到要求目录下的文件或者文件夹（一级的）//
                var tempPaths = StringExtention.SplitWithString(p, "/Assets/" + path + "\\");

                //tempPaths 分割后的不可能为空,只要directoryEntries不为空//
                if (tempPaths[1].EndsWith(".meta"))
                    continue;
                if (tempPaths[1].EndsWith(".prefab"))
                    continue;

                var pathSplit = StringExtention.SplitWithString(tempPaths[1], ".");
                //文件
                if (pathSplit.Length > 1)
                    nameArray.Add(pathSplit[0]);
                //遍历子目录下 递归吧！
                else
                    GetObjectNameToArray<T>(path + "/" + pathSplit[0], "pattern");
            }
        }
        catch (DirectoryNotFoundException)
        {
            Debug.Log("The path encapsulated in the " + objPath + "Directory object does not exist.");
        }
    }

    private void FindUse()
    {
        selectTrans = Selection.GetTransforms(SelectionMode.TopLevel);

        if (selectTrans.Length <= 0)
        {
            ShowNotification(new GUIContent("没有选择预设体"));
            return;
        }

        spriteList = new List<UISprite>();
        transName = new List<string>();
        for (var i = 0; i < selectTrans.Length; i++)
        {
            var sprites = selectTrans[i].GetComponentsInChildren<UISprite>(true);
            for (var j = 0; j < sprites.Length; j++)
                if (sprites[j] != null && sprites[j].atlas == atlas)
                {
                    if (sprName == "")
                    {
                        spriteList.Add(sprites[j]);
                        transName.Add(selectTrans[i].gameObject.name);
                        continue;
                    }

                    if (sprites[j].spriteName == sprName)
                    {
                        spriteList.Add(sprites[j]);
                        transName.Add(selectTrans[i].gameObject.name);
                    }
                }
        }
    }



    private bool artSort = true;
    private bool refSort = false;
    private string up = "▲";
    private string down = "▼";
    /// <summary>
    ///     显示查找结果
    /// </summary>
    private void ShowFindUse()
    {
  
        GUILayout.BeginHorizontal();
        {
            GUI.color = Color.green;
            GUILayout.Label(" ", GUILayout.Width(14f));
            GUILayout.Label("美术资源名", GUILayout.Width(130f));
            if (GUILayout.Button(artSort ? up : down, GUILayout.Width(20f)))
            {
                artSort = !artSort;
                spriteReferenceDatas.Sort((x, y) =>
                {
                    return string.Compare(x.sprite.spriteName, y.sprite.spriteName) * (artSort ? 1 : -1);
                });
            }

            GUILayout.Label("引用路径", GUILayout.Width(400));
            //refSort = GUILayout.Button(refSort ? up : down, GUILayout.Width(20f));

            GUILayout.Label("对象信息(Prefab)", GUILayout.Width(150f));
            GUILayout.Label("图集信息(Sprite)", GUILayout.Width(150f));
            GUI.color = Color.white;
        }
        GUILayout.EndHorizontal();
        {
            GUILayout.BeginVertical();
            EditorGUILayout.Space();
            mScroll = GUILayout.BeginScrollView(mScroll,GUILayout.MaxHeight(400));
            for (var i = 0; i < spriteReferenceDatas.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(" ", GUILayout.Width(14f));
                GUILayout.Label(spriteReferenceDatas[i].sprite.spriteName, GUILayout.Width(150f));
                if (spriteReferenceDatas[i].referencePath == "no use")
                {
                    GUI.color = Color.red;
                    GUILayout.Label(spriteReferenceDatas[i].referencePath, GUILayout.Width(400f));
                    GUI.color = Color.white;;
                }
                else
                {
                    GUILayout.Label(spriteReferenceDatas[i].referencePath, GUILayout.Width(380f));
                    GUI.color = Color.green;
                    if (GUILayout.Button("C",GUILayout.Width(20f)))
                    {
                        GUIUtility.systemCopyBuffer = spriteReferenceDatas[i].referencePath;
                        ShowNotification(new GUIContent("拷贝成功，粘贴查看"));
                    }
                    GUI.color = Color.white;
                }
                    

                EditorGUILayout.ObjectField("", spriteReferenceDatas[i].prefab, typeof(GameObject), true, GUILayout.Width(150f));
                EditorGUILayout.ObjectField("", spriteReferenceDatas[i].sprite, typeof(UISprite), true, GUILayout.Width(150f));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }
        GUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    [MenuItem("Assets/AtlasTools/导出UISprite和图集不匹配的预设")]
    static void ExportPrefabErrorSprite()
    {
        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources/Prefabs/UI" });
        List<SpriteReferenceData> spriteReferenceDatas = new List<SpriteReferenceData>();

        for (int i = 0; i < guids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[i]);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var sprites = go.GetComponentsInChildren<UISprite>(true);
            foreach (UISprite sprite in sprites)
            {
                if (sprite.atlas != null)
                {
                    if (sprite.atlas.GetSprite(sprite.spriteName) == null)
                    {
                        SpriteReferenceData data = new SpriteReferenceData();
                        data.sprite = sprite;
                        data.prefab = go;
                        data.referencePath =  GetCompletePath(sprite.transform);
                        spriteReferenceDatas.Add(data);
                    }
                }
            }

            EditorUtility.DisplayProgressBar("find progress", go.name, (float) i / (guids.Length - 1));
        }

        string savDic = Environment.CurrentDirectory + "/图集引用信息";
        string savePath = savDic + "/图集错误.xlsx";
        FileInfo info = new FileInfo(savePath);
        if (File.Exists(savePath))
            File.Delete(savePath);
        using (ExcelPackage package = new ExcelPackage(info))
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("图集信息");
            worksheet.Cells[1, 1].Value = "Prefab";
            worksheet.Cells[1, 2].Value = "Ref";
            worksheet.Cells[1, 3].Value = "Name";
            worksheet.Cells[1, 4].Value = "SpriteName";
            worksheet.Cells[1, 5].Value = "Atlas";

            int count = 2;
            foreach (var data in spriteReferenceDatas)
            {
                worksheet.Cells[count, 1].Value = data.prefab.name;
                worksheet.Cells[count, 2].Value = data.referencePath;
                worksheet.Cells[count, 3].Value = data.sprite.name;
                worksheet.Cells[count, 4].Value = data.sprite.spriteName;
                worksheet.Cells[count, 5].Value = data.sprite.atlas.name;

                EditorUtility.DisplayProgressBar("export progress", data.prefab.name, (float)count / (spriteReferenceDatas.Count - 2));
                count++;
            }
            package.Save();
        }
        EditorUtility.ClearProgressBar();
    }


    class SpriteReferenceData
    {
        public UISprite sprite;
        public string referencePath;
        public GameObject prefab;
        public static List<string> spriteList = new List<string>();

        public static void AddSprite(string name)
        {
            if (!spriteList.Contains(name))
                spriteList.Add(name);
        }

    }
    #endregion
}

public class PrefabReplaceAtlasWindow : EditorWindow
{
    private static PrefabReplaceAtlasWindow window;

    [MenuItem("Tools/预设替换工具")]
    private static void OpenWindow()
    {
        window = (PrefabReplaceAtlasWindow) GetWindow(typeof(PrefabReplaceAtlasWindow), false, "预设替换图集窗口");
        window.Show();
    }

    public GameObject prefab;
    private Dictionary<string,ReplaceAtlas> atlases;
    private Vector2 scroll;
    private UISprite[] spriteArray;

    void OnEnable()
    {
        ResetCache();
    }

    void OnDisable()
    {
        ResetCache();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        prefab = EditorGUILayout.ObjectField("预设", prefab, typeof(GameObject),false) as GameObject;
        if (GUILayout.Button("查找"))
            ShowResult();
        if (GUILayout.Button("替换"))
            Replace();
        if (atlases.Count > 0)
            DrawResult();
        EditorGUILayout.EndVertical();
    }

    void Replace()
    {
        for (int i = 0; i < spriteArray.Length; i++)
        {
            if (atlases[spriteArray[i].atlas.name].newAtlas != null)
                spriteArray[i].atlas = atlases[spriteArray[i].atlas.name].newAtlas;
            else
            {
                ShowNotification(new GUIContent(spriteArray[i].atlas.name + "图集没有对应的新图集"));
                Debug.Log(spriteArray[i].atlas.name + "图集没有对应的新图集");
            }
            EditorUtility.DisplayProgressBar("替换图集中", i + 1 + "/" + spriteArray.Length,
                (float)i / spriteArray.Length - 1);
        }
        EditorUtility.ClearProgressBar();
        Apply();
        ResetCache();
        ShowNotification(new GUIContent("替换完成"));
    }

    void Apply()
    {
        GameObject temp = GameObject.Instantiate(prefab);
        PrefabUtility.ReplacePrefab(temp, prefab, ReplacePrefabOptions.ConnectToPrefab);
        DestroyImmediate(temp);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    void ShowResult()
    {
        if (prefab == null)
        {
            ShowNotification(new GUIContent("没有选择预设体"));
            return;
        }
        ResetCache();
        spriteArray = prefab.GetComponentsInChildren<UISprite>(true);

        List<UISprite> temp = new List<UISprite>();
        foreach (UISprite uiSprite in spriteArray)
            if (uiSprite.atlas != null)
                temp.Add(uiSprite);
        spriteArray = temp.ToArray();

        for (int i = 0; i < spriteArray.Length; i++)
        {
            if (!atlases.ContainsKey(spriteArray[i].atlas.name))
            {
                ReplaceAtlas atlas = new ReplaceAtlas();
                atlas.oldAtlas = spriteArray[i].atlas;
                atlas.newAtlas = null;
                atlases.Add(spriteArray[i].atlas.name, atlas);
            }
            EditorUtility.DisplayProgressBar("查找图集中", i + 1 + "/" + spriteArray.Length,
                (float) i / spriteArray.Length - 1);
        }
        EditorUtility.ClearProgressBar();
    }

    void ResetCache()
    {
        atlases = new Dictionary<string, ReplaceAtlas>();
    }

    private string curKey = string.Empty;
    void DrawResult()
    {
        scroll = GUILayout.BeginScrollView(scroll, GUILayout.MaxHeight(400));
        foreach (var atlas in atlases)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField("旧图集", atlas.Value.oldAtlas, typeof(UIAtlas),false);
            atlas.Value.newAtlas = EditorGUILayout.ObjectField("新图集", atlas.Value.newAtlas, typeof(UIAtlas),false) as UIAtlas;
            if (NGUIEditorTools.DrawPrefixButton("Atlas"))
            {
                curKey = atlas.Key;
                ComponentSelector.Show<UIAtlas>(FillNewAtlas);
            }
                
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        GUILayout.EndScrollView();
    }

    void FillNewAtlas(Object atlas)
    {
        if(atlases.ContainsKey(curKey))
            atlases[curKey].newAtlas = atlas as UIAtlas;
    }

    class ReplaceAtlas
    {
        public UIAtlas oldAtlas;
        public UIAtlas newAtlas;
    }
}

public class PrefabReplaceAtlasWindowDic : EditorWindow
{

    private static List<GameObject> prefabs;
    private static PrefabReplaceAtlasWindowDic window;
    private Vector2 scroll;

    public static void OpenWindow()
    {
        window.Show();
    }

    private Dictionary<string, AtlasRef> atlasRefs;
    private List<UISprite> sprites;

    [MenuItem("Assets/AtlasTools/替换选中的prefab图集")]
    static void SelectPrefabs()
    {
        AssetDatabase.SaveAssets();
        window = GetWindow<PrefabReplaceAtlasWindowDic>(false, "图集替换", true);
        window.maxSize = new Vector2(1280, 720);
        prefabs = new List<GameObject>();
        UnityEngine.Object[] objects = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        if (objects.Length > 20)
        {
            Debug.LogError("选着的预设太多请重新选着");
            return;
        }
        int progress = 0;
        foreach (var o in objects)
        {
            string path = AssetDatabase.GetAssetPath(o);
            if (path.EndsWith(".prefab"))
            {
                var gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                prefabs.Add(gameObject);
            }

            progress++;
            EditorUtility.DisplayProgressBar("填充预设中", path, (float)progress / objects.Length);
        }
        EditorUtility.ClearProgressBar();

        if (prefabs.Count > 0)
            window.Show();
        else
            Debug.Log("没有选中预设体");
    }


    void OnEnable()
    {
        sprites = new List<UISprite>();
        atlasRefs = new Dictionary<string, AtlasRef>();
    }

    void OnDisable()
    {
        sprites = new List<UISprite>();
        atlasRefs = new Dictionary<string, AtlasRef>();
    }

    private bool fillCommon = false;

    void OnGUI()
    {
        //scroll = EditorGUILayout.BeginScrollView(scroll);
        GUILayout.BeginVertical();
        if (GUILayout.Button("查找"))
            Find();
        fillCommon = GUILayout.Toggle(fillCommon,"空图集填充Common");

        GUILayout.EndVertical();
        if (atlasRefs.Count > 0)
        {
            if (GUILayout.Button("替换"))
                Replace();
            DrawResult();
        }
    }

    void Reset()
    {
        prefabs = new List<GameObject>();
        atlasRefs = new Dictionary<string, AtlasRef>();
    }

    void Replace()
    {
        int progress = 0;
        foreach (GameObject gameObject in prefabs)
        {
            var spriteArray = gameObject.GetComponentsInChildren<UISprite>(true);
            List<UISprite> temp = new List<UISprite>();
            foreach (UISprite uiSprite in spriteArray)
                if (uiSprite.atlas != null)
                    temp.Add(uiSprite);
            Debug.Log("剔除" + (spriteArray.Length - temp.Count) + "个UISprite空图集");
            bool apply = false;
            foreach (UISprite sprite in temp)
            {
                if (GetNewAtlas(sprite.atlas.name) == null)
                    this.ShowNotification(new GUIContent(sprite.atlas.name + "没有对应的新图集"));
                else
                {
                    if (sprite.atlas != null && GetNewAtlas(sprite.atlas.name).name == sprite.atlas.name)
                        continue;
                    sprite.atlas = GetNewAtlas(sprite.atlas.name);
                    apply = true;
                }
                    
            }

            if (apply)
                Apply(gameObject);
            progress++;
            EditorUtility.DisplayProgressBar("", "批量替换中", (float)progress / prefabs.Count);
        }
        EditorUtility.ClearProgressBar();
        Reset();
        this.ShowNotification(new GUIContent("替换完成"));
        Close();
    }

    void Apply(GameObject prefab)
    {
        
        Debug.Log(PrefabUtility.GetPrefabType(prefab));
        GameObject temp = GameObject.Instantiate(prefab);
        PrefabUtility.ReplacePrefab(temp, prefab, ReplacePrefabOptions.ConnectToPrefab);
        DestroyImmediate(temp);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    UIAtlas GetNewAtlas(string key)
    {
        if (atlasRefs.ContainsKey(key))
            return atlasRefs[key].newAtlas;
        return null;
    }

    private string curKey = string.Empty;
    void DrawResult()
    {
        scroll = GUILayout.BeginScrollView(scroll, GUILayout.MaxHeight(400));
        foreach (var atlas in atlasRefs)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField("旧图集", atlas.Value.oldAtlas, typeof(UIAtlas), false);
            atlas.Value.newAtlas = EditorGUILayout.ObjectField("新图集", atlas.Value.newAtlas, typeof(UIAtlas), false) as UIAtlas;
            if (NGUIEditorTools.DrawPrefixButton("Atlas"))
            {
                curKey = atlas.Key;
                ComponentSelector.Show<UIAtlas>(FillNewAtlas);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        GUILayout.EndScrollView();
    }

    void FillNewAtlas(Object atlas)
    {
        if (atlasRefs.ContainsKey(curKey))
            atlasRefs[curKey].newAtlas = atlas as UIAtlas;
    }
    void Find()
    {
        atlasRefs = new Dictionary<string, AtlasRef>();
        foreach (GameObject gameObject in prefabs)
        {
            var spriteArray = gameObject.GetComponentsInChildren<UISprite>(true);
            List<UISprite> temp = new List<UISprite>();
            foreach (UISprite uiSprite in spriteArray)
                if (uiSprite.atlas != null)
                    temp.Add(uiSprite);
                else
                	if (fillCommon)
                    	uiSprite.atlas = Resources.Load<GameObject>("UI/Atlas/Common/Common_Atlas").GetComponent<UIAtlas>();

            foreach (var uiSprite in temp)
            {
                if (!atlasRefs.ContainsKey(uiSprite.atlas.name))
                {
                    AtlasRef atlasRef = new AtlasRef();
                    atlasRef.oldAtlas = uiSprite.atlas;
                    atlasRef.newAtlas = null;
                    atlasRefs.Add(uiSprite.atlas.name, atlasRef);
                }
            }
        }
    }

    class AtlasRef
    {
        public UIAtlas oldAtlas;
        public UIAtlas newAtlas;
    }


}

public class AtlasSplitDataReplace : EditorWindow
{

    private static AtlasSplitDataReplace window;
    [MenuItem("Assets/AtlasTools/图集替换宫格信息窗口")]
    static void OpenWindow()
    {
        window = GetWindow<AtlasSplitDataReplace>(false, "图集九宫格信息替换窗口", true);
        window.maxSize = new Vector2(1280, 720);
        window.Show();
    }

    private UIAtlas oldAtlas;
    private UIAtlas newAtlas;
    private Dictionary<string, SplitData> splitDatas;

    void OnEnable()
    {
        splitDatas = new Dictionary<string, SplitData>();
    }

    void OnDisable()
    {
        splitDatas = new Dictionary<string, SplitData>();
    }

    void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        oldAtlas = EditorGUILayout.ObjectField("旧图集", oldAtlas, typeof(UIAtlas), false) as UIAtlas;
        newAtlas = EditorGUILayout.ObjectField("新图集", newAtlas, typeof(UIAtlas), false) as UIAtlas;
        GUILayout.EndHorizontal();

        if (GUILayout.Button("输出九宫格信息"))
            FillSplitData();

        if (splitDatas.Count > 0)
        {
            if (GUILayout.Button("一键替换九宫格信息"))
                ReplaceSplitData();
            DrawSplitData();

        }
        GUILayout.EndVertical();
    }

    void ReplaceSplitData()
    {
        int index = 0;
        foreach (UISpriteData data in newAtlas.spriteList)
        {
            foreach (var splitData in splitDatas)
            {
                if (splitData.Key == data.name )
                {
                    var value = splitData.Value;
                    data.SetBorder(value.left, value.bottom, value.right, value.top);
/*                    data.borderLeft = splitData.Value.left;
                    data.borderRight = splitData.Value.right;
                    data.borderBottom = splitData.Value.bottom;
                    data.borderTop = splitData.Value.top;*/
                }
            }

            EditorUtility.DisplayProgressBar("批量替换中", data.name, (float)index / newAtlas.spriteList.Count);
        }
        EditorUtility.ClearProgressBar();
        Apply(newAtlas.gameObject);
        this.ShowNotification(new GUIContent("替换完成进入游戏查看效果"));
    }

    void Apply(GameObject prefab)
    {
        //GameObject temp = GameObject.Instantiate(prefab);
        //PrefabUtility.ReplacePrefab(temp, prefab, ReplacePrefabOptions.ConnectToPrefab);
        //DestroyImmediate(temp);
        prefab.transform.position = Vector3.one;
        prefab.transform.position = Vector3.zero;
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    void FillSplitData()
    {
        if (oldAtlas == null)
        {
            this.ShowNotification(new GUIContent("没有选择旧图集"));
            return;
        }

        if (newAtlas == null)
        {
            this.ShowNotification(new GUIContent("没有选择新图集"));
            return;
        }

        splitDatas = new Dictionary<string, SplitData>();

        foreach (UISpriteData spriteData in oldAtlas.spriteList)
        {
            if (spriteData.borderLeft != 0 || spriteData.borderRight != 0 || spriteData.borderBottom != 0 || spriteData.borderTop != 0)
            {
                SplitData data = new SplitData();
                data.name = spriteData.name;
                data.bottom = spriteData.borderBottom;
                data.top = spriteData.borderTop;
                data.left = spriteData.borderLeft;
                data.right = spriteData.borderRight;
                splitDatas.Add(spriteData.name, data);
            }
           
        }
    }

    private Vector2 scroll;

    void DrawSplitData()
    {
        scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(400));
        foreach (var splitData in splitDatas)
        {
            var value = splitData.Value;
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label(value.name, GUILayout.MaxWidth(200), GUILayout.MinWidth(200));
            NGUIEditorTools.IntPair(null, "Left", "Right", value.left, value.right);
            NGUIEditorTools.IntPair(null, "Bottom", "Top", value.bottom, value.top);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        GUILayout.EndScrollView();
    }

    class SplitData
    {
        public string name;
        public int left;
        public int right;
        public int bottom;
        public int top;
    }

    [MenuItem("Assets/AtlasTools/输入图集九宫格信息")]
    static void PrintSplitData()
    {
        UObject[] objects = Selection.GetFiltered(typeof(UObject), SelectionMode.DeepAssets);
        foreach (UObject o in objects)
        {
            var path = AssetDatabase.GetAssetPath(o);
            if (path.EndsWith(".prefab"))
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var atlas = go.GetComponent<UIAtlas>();
                if (atlas != null)
                {
                    foreach (UISpriteData spriteData in atlas.spriteList)
                    {
                        Debug.Log(string.Format("{0}\tleft:{1}\tright:{2}\tbottom:{3}\ttop:{4}", spriteData.name, spriteData.borderLeft, spriteData.borderRight, spriteData.borderBottom,
                            spriteData.borderTop));
                    }
                }
            }
        }
    }



}

public class SpriteReplaceWindow : EditorWindow
{
    private static SpriteReplaceWindow window;

    [MenuItem("Assets/AtlasTools/图集替换窗口")]
    static void OpenWindow()
    {
        window = GetWindow<SpriteReplaceWindow>(false, "图集替换窗口", false);
        window.maxSize = new Vector2(1280, 720);
        window.Show();
    }

    void OnEnable()
    {
        spriteArray = new UISprite[0];
    }

    void OnDisable()
    {
        spriteArray = new UISprite[0];
    }

    public GameObject prefab;
    public UISprite[] spriteArray;
    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        prefab = EditorGUILayout.ObjectField("预设体",prefab,typeof(GameObject),false) as GameObject;
        if(GUILayout.Button("查找"))
            Fill();
        if (spriteArray.Length > 0)
        {
            if (GUILayout.Button("Apply"))
            {
                GameObject temp = GameObject.Instantiate(prefab);
                PrefabUtility.ReplacePrefab(temp, prefab, ReplacePrefabOptions.ConnectToPrefab);
                DestroyImmediate(temp);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            DrawResult();
        }
            

        EditorGUILayout.EndVertical();
    }


    void Fill()
    {
        if (prefab == null)
        {
            this.ShowNotification(new GUIContent("没有选择预设体"));
            return;
        }

        spriteArray = prefab.GetComponentsInChildren<UISprite>(true);
        Array.Sort(spriteArray, (x, y) =>
        {
            if (x.atlas == null || y.atlas == null)
                return 0;
            return string.Compare(x.atlas.name, y.atlas.name) * (atlasSort ? 1 : -1);
        });
    }

    private Vector2 scroll;
    private int selectIndex;
    private string up = "▲";
    private string down = "▼";
    private bool atlasSort = false;
    void DrawTitle()
    {
        GUILayout.BeginHorizontal();
        GUI.color = Color.green;
        GUILayout.Label("控件名", GUILayout.Width(120));
        GUILayout.Label("美术名", GUILayout.Width(170f));
        GUILayout.Label("Sprite", GUILayout.Width(150f + 76f + 20f));
        GUILayout.Label("Atlas", GUILayout.MaxWidth(150f + 76f));
        if (GUILayout.Button(atlasSort ? up : down,GUILayout.Width(20f)))
        {
            atlasSort = !atlasSort;
            Array.Sort(spriteArray, (x, y) =>
            {
                if (x.atlas == null || y.atlas == null)
                    return 0;
                return string.Compare(x.atlas.name, y.atlas.name) * (atlasSort ? 1 : -1);
            });
        }
        GUILayout.EndHorizontal();
        GUI.color = Color.white;
    }


    void DrawResult()
    {
        DrawTitle();
        scroll = GUILayout.BeginScrollView(scroll, GUILayout.MaxHeight(400f));
        for (int i = 0; i < spriteArray.Length; i++)
        {
            if (spriteArray[i] == null)
                continue;
            GUILayout.BeginHorizontal();
            GUILayout.Label(spriteArray[i].name, GUILayout.Width(100f));

            GUI.color = Color.red;
            if (GUILayout.Button("C", GUILayout.Width(20f)))
                MyTools.CopyStr(spriteArray[i].name, this);
            GUI.color = Color.white;

            GUILayout.Label(spriteArray[i].spriteName, GUILayout.Width(150f));

            GUI.color = Color.red;
            if (GUILayout.Button("C", GUILayout.Width(20f)))
                MyTools.CopyStr(spriteArray[i].spriteName,this);
            GUI.color = Color.white;

            if (NGUIEditorTools.DrawPrefixButton("Sprite"))
            {
                NGUISettings.atlas = spriteArray[i].atlas;
                selectIndex = i;
                SpriteSelector.Show(OnSelectSprite);
            }
            spriteArray[i] = EditorGUILayout.ObjectField("", spriteArray[i], typeof(UISprite), false,GUILayout.MaxWidth(110f)) as UISprite;
            if (GUILayout.Button("Edit", GUILayout.Width(40f)))
            {
                if (spriteArray[i].atlas != null)
                {
                    UIAtlas atl = spriteArray[i].atlas;
                    NGUISettings.atlas = atl;
                    NGUISettings.selectedSprite = spriteArray[i].name;
                    if (atl != null) NGUIEditorTools.Select(atl.gameObject);
                }
            }

            GUILayout.Space(10f);

            if (NGUIEditorTools.DrawPrefixButton("Atlas"))
            {
                ComponentSelector.Show<UIAtlas>(OnSelectAtlas);
                selectIndex = i;
            }
            spriteArray[i].atlas = EditorGUILayout.ObjectField("",spriteArray[i].atlas,typeof(UIAtlas),false,GUILayout.MaxWidth(150f)) as UIAtlas;
            if (GUILayout.Button("Edit", GUILayout.Width(40f)))
            {
                if (spriteArray[i].atlas != null)
                {
                    UIAtlas atl = spriteArray[i].atlas;
                    NGUISettings.selectedSprite = spriteArray[i].name;
                    NGUISettings.atlas = atl;
                    if (atl != null) NGUIEditorTools.Select(atl.gameObject);
                }
            }

            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
    }

    void OnSelectSprite(string name)
    {
        spriteArray[selectIndex].spriteName = name;
    }
    

    void OnSelectAtlas(Object atlas)
    {
        spriteArray[selectIndex].atlas = atlas as UIAtlas;
    }
}

public class AtlasReplaceOneKey : EditorWindow
{
    private static AtlasReplaceOneKey window;

    
    static void OpenWindow()
    {

    }

    private UIAtlas _oldAtlas;
    private UIAtlas _newAtlas;

    void OnEnable()
    {
        prefabAtlases = new Dictionary<string, PrefabAtlas>();
    }

   
    //public Vector2 sroll =;
    void OnGUI()
    {
        _oldAtlas = EditorGUILayout.ObjectField("旧图集", _oldAtlas, typeof(UIAtlas), false) as UIAtlas;
        _newAtlas = EditorGUILayout.ObjectField("新图集", _newAtlas, typeof(UIAtlas), false) as UIAtlas;
        if (GUILayout.Button("查找替换"))
        {
            FindAtlas();
        }

        if (prefabAtlases.Count > 0)
        {
            //GUILayout.BeginScrollView(s)
        }

    }

    private Dictionary<string, PrefabAtlas> prefabAtlases;

    void FindAtlas()
    {
        prefabAtlases = new Dictionary<string, PrefabAtlas>();
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] {"Assets/Resources/Prefabs/UI"});
        for (int i = 0; i < guids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[i]);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var sprites = go.GetComponentsInChildren<UISprite>(true);
            foreach (UISprite sprite in sprites)
            {
                if (sprite.atlas.name == _oldAtlas.name)
                {
                    sprite.atlas = _newAtlas;
                    if (!prefabAtlases.ContainsKey(guids[i]))
                    {
                        PrefabAtlas prefabAtlas = new PrefabAtlas();
                        prefabAtlas.prefab = go;
                        prefabAtlas.bReplace = true;
                        prefabAtlases.Add(guids[i],prefabAtlas);
                    }
                }
            }

        }

        foreach (var prefabAtlase in prefabAtlases)
        {
            //prefabAtlase.va
        }
        
    }

    class PrefabAtlas
    {
        public GameObject prefab;
        public bool bReplace = false;
    }
}


internal class AtlasTools
{
    /// <summary>
    ///     保存图集中的图片
    /// </summary>
    /// <param name="_atlas">图片所在的图集</param>
    /// <param name="_sprite">图集中要保存的图片名字</param>
    /// <param name="_path">要保存的路径</param>
    public static void SaveSpriteAsTexture(UIAtlas _atlas, string _sprite, string _path)
    {
        var se = UIAtlasMaker.ExtractSprite(_atlas, _sprite);

        if (se != null)
        {
            var bytes = se.tex.EncodeToPNG();
            File.WriteAllBytes(_path, bytes);
            AssetDatabase.ImportAsset(_path);
            if (se.temporaryTexture)
                UObject.DestroyImmediate(se.tex);
        }
    }

    /// <summary>
    ///     将图集中的所有图片拆分并保存
    /// </summary>
    /// <param name="_atlas">图集</param>
    /// <param name="_refresh">是否立即更新</param>
    public static void CutTextures(UIAtlas _atlas, bool _refresh = true)
    {
        // 验证参数有效性
        if (!_atlas) return;

        // 创建路径
        if (!Directory.Exists(Application.dataPath + "/AtlasTextures"))
            AssetDatabase.CreateFolder("Assets", "AtlasTextures");

        if (!Directory.Exists(Application.dataPath + "/AtlasTextures/" + _atlas.name))
            AssetDatabase.CreateFolder("Assets/AtlasTextures", _atlas.name);

        var count = 0;
        // 开始
        var sprites = _atlas.GetListOfSprites();
        foreach (var spriteName in sprites)
        {
            var path = Application.dataPath + "/AtlasTextures/" + _atlas.name + "/" + spriteName + ".png";
            EditorUtility.DisplayProgressBar("拆包中", path, (float) count / sprites.size);
            SaveSpriteAsTexture(_atlas, spriteName, path);
            count++;
        }

        EditorUtility.ClearProgressBar();

        if (_refresh)
            AssetDatabase.Refresh();
    }

    [MenuItem("Tools/NGUI图集拆包")]
    private static void Do()
    {
        var selectedAssets = Selection.assetGUIDs;
        if (selectedAssets == null || selectedAssets.Length == 0) return;
        foreach (var guid in selectedAssets)
        {
            var atlas = LoadAssetByGUID<UIAtlas>(guid);
            CutTextures(atlas, false);
        }

        AssetDatabase.Refresh();
    }

    /// <summary>
    ///     根据GUID加载资源
    /// </summary>
    /// <typeparam name="T">要加载的资源的类型，该类型必须继承MonoBehaviour</typeparam>
    /// <param name="_guid">资源的GUID</param>
    /// <returns>返回加载的资源，如果加载失败将返回null</returns>
    public static T LoadAssetByGUID<T>(string _guid) where T : MonoBehaviour
    {
        var path = AssetDatabase.GUIDToAssetPath(_guid);
        var resum = AssetDatabase.LoadAssetAtPath(path, typeof(T)) as T;
        return resum;
    }
}

class UITextureWindow : EditorWindow
{
    private static UITextureWindow window;
    public static void OpenWindow()
    {
        window = (UITextureWindow)GetWindow(typeof(UITextureWindow), true, "纹理引用详情窗口");
        window.Show();
    }

    private Vector2 scroll = Vector2.zero;
    void OnGUI()
    {
        GUILayout.Label("窗口");
    }
}

internal class MyTools : Editor
{
    public static void CopyStr(string str, EditorWindow window, string tips = "拷贝成功Ctrl + V 查看")
    {
        GUIUtility.systemCopyBuffer = str;
        window.ShowNotification(new GUIContent(tips));
    }
}
internal class TextureTools
{
    [MenuItem("Assets/纹理工具/查找纹理在UI上引用情况")]
    static void FindTextureUse()
    {
        UObject[] objects= Selection.GetFiltered(typeof(Texture2D), SelectionMode.TopLevel);
        var texturePath = AssetDatabase.GetAssetPath(objects[0]);
        if (!IsTextureFile(texturePath))
        {
            Debug.LogError("没有选中一张纹理");
            return;
        }

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] {"Assets/Resources/Prefabs/UI"});
        for (int i = 0; i < guids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[i]);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var components = go.GetComponentsInChildren<UITexture>();
            for (int j = 0; j < components.Length; j++)
            {
                if (components[j].mainTexture != null)
                {
                    if (components[j].mainTexture.name.Equals(texture.name))
                    {
                        Debug.Log(go.name);
                    }
                }
               
            }
            EditorUtility.DisplayProgressBar("查找中",AssetDatabase.GUIDToAssetPath(guids[i]),(float) i / guids.Length);
        }
        UITextureWindow.OpenWindow();
        EditorUtility.ClearProgressBar();
    }

  

    #region 设置纹理格式

    [MenuItem("Tools/批量设置Texture格式")]
    private static void SetTextureType()
    {
        UObject[] objs = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
        int count = 0;
        foreach (UObject ob in objs)
        {
            string path = AssetDatabase.GetAssetPath(ob);
            if (String.IsNullOrEmpty(path) || !IsTextureFile(path))
            {
                Debug.LogError("未选中对象或者选择的对象不是图片");
                return;
            }

            if (IsTextureFile(path))
            {
                TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                if (textureImporter == null) return;
                textureImporter.textureType = TextureImporterType.Advanced;
                textureImporter.spriteImportMode = SpriteImportMode.None;
                textureImporter.mipmapEnabled = false;
                textureImporter.isReadable = false;
                textureImporter.alphaIsTransparency = true;

                int androidMaxTextureSize = 0;
                TextureImporterFormat androidTextureFormat;
                textureImporter.GetPlatformTextureSettings("Android", out androidMaxTextureSize, out androidTextureFormat);
                androidTextureFormat = TextureImporterFormat.ETC2_RGBA8;
                textureImporter.SetPlatformTextureSettings("Android", GetValidSize(androidMaxTextureSize), androidTextureFormat, FindAtlasUseUtil.CompressQuality, false);

                //int iphoneMaxTextureSize = 0;
                //textureImporter.GetPlatformTextureSettings("iPhone", out iphoneMaxTextureSize, out iphoneTextureFormat);
                //TextureImporterFormat iphoneTextureFormat = UnityEditor.TextureImporterFormat.PVRTC_RGBA4;
                //textureImporter.SetPlatformTextureSettings("iPhone", GetValidSize(iphoneMaxTextureSize), iphoneTextureFormat, CompressQuality, false);

                textureImporter.SaveAndReimport();
            }

            count++;
            ShowProgress(path, (float)count / objs.Length);
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static int GetValidSize(int size)
    {
        int result = 0;
        if (size <= 48)
        {
            result = 32;
        }
        else if (size <= 96)
        {
            result = 64;
        }
        else if (size <= 192)
        {
            result = 128;
        }
        else if (size <= 384)
        {
            result = 256;
        }
        else if (size <= 768)
        {
            result = 512;
        }
        else if (size <= 1536)
        {
            result = 1024;
        }
        else if (size <= 3072)
        {
            result = 2048;
        }
        else if (size <= 6144)
        {
            result = 4096;
        }
        else if (size <= 12288)
        {
            result = 8192;
        }

        return result;
    }

    #endregion

    /// <summary>
    /// 显示进度条
    /// <summary>
    /// 显示进度条
    /// </summary>
    /// <param name="path"></param>
    /// <param name="val"></param>
    static public void ShowProgress(string path, float val)
    {
        EditorUtility.DisplayProgressBar("批量处理中...", String.Format("Please wait...  Path:{0}", path), val);
    }

    /// <summary>
    /// 判断是否是图片格式
    /// </summary>
    /// <param name="_path"></param>
    /// <returns></returns>
    private static bool IsTextureFile(string _path)
    {
        string path = _path.ToLower();
        return path.EndsWith(".psd") || path.EndsWith(".tga") || path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".dds") || path.EndsWith(".bmp") || path.EndsWith(".tif") || path.EndsWith(".gif");
    }
}

/// <summary>
///     自定义字符串分割方法
/// </summary>
public class StringExtention
{
    public static string[] SplitWithString(string sourceString, string splitString)
    {
        var tempSourceString = sourceString;
        var arrayList = new List<string>();
        var s = string.Empty;
        while (sourceString.IndexOf(splitString) > -1) //分割
        {
            s = sourceString.Substring(0, sourceString.IndexOf(splitString));
            sourceString = sourceString.Substring(sourceString.IndexOf(splitString) + splitString.Length);
            arrayList.Add(s);
        }

        arrayList.Add(sourceString);
        return arrayList.ToArray();
    }
}