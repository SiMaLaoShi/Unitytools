using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using LitJson;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using UnityEditor;
using UnityEngine;
using Color = System.Drawing.Color;

public class UILabelTools : Editor
{
    

    #region field
    private const string extend = "txt";

    private static readonly string[] PrefabPath =
    {
        "Assets/Resources/Prefabs/UI"
    };

    private static TxtClass[] txtClasses;
    #endregion

    #region MenuItem Editor

    [MenuItem("Tools/海外版本工具/查找UILabel中文导出txt")]
    private static void FindUILabel()
    {
        
        var filePath = GetSavePath("UILabel中文分析报告", "txt");
        ReadJson();
        if (File.Exists(filePath)) File.Delete(filePath);
        var streamWriter = File.CreateText(filePath);
        var prefabs = AssetDatabase.FindAssets("t:Prefab", PrefabPath);
        for (var i = 0; i < prefabs.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(prefabs[i]);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var labels = go.transform.GetComponentsInChildren<UILabel>(true);

            streamWriter.WriteLine("//----------------------------" + go.name + "-----------------------------");
            for (var j = 0; j < labels.Length; j++)
                if (HasChinese(labels[j].text))
                    streamWriter.WriteLine("{0}\t{1}\t{2}\n", path, GetGameObjectPath(labels[j].gameObject),
                        labels[j].text);

            streamWriter.WriteLine("//----------------------------" + "Lua代码" + "-------------------------");
            foreach (var label in labels)
                if (HasChinese(label.text))
                    streamWriter.WriteLine(GetLuaComponent(label.gameObject, label.text, go));
            streamWriter.WriteLine("");

            EditorUtility.DisplayProgressBar("查找中", path, (float) i / (prefabs.Length - 1));
        }

        EditorUtility.ClearProgressBar();
        streamWriter.Flush();
        streamWriter.Close();
        txtClasses = null;
        Debug.Log("生成成功");
    }

    private static string GetSavePath(string file, string extend)
    {
        string saveDir = Environment.CurrentDirectory + "/海外版本";
        if (!Directory.Exists(saveDir))
            Directory.CreateDirectory(saveDir);
        return string.Format("{0}/{1}_{2}.{3}", saveDir, DateTime.Now.ToString("yyyyMMddHHmm"), file,
            extend);
    }

    private static string GetGameObjectPath(GameObject gameObject)
    {
        var path = "/" + gameObject.name;
        while (gameObject.transform.parent != null)
        {
            gameObject = gameObject.transform.parent.gameObject;
            path = "/" + gameObject.name + path;
        }

        return path;
    }

    private static string GetLuaComponent(GameObject gameObject, string txt, GameObject root)
    {
        var path = GetGameObjectPath(gameObject).Remove(0, 2 + root.name.Length);

        var lua = string.Format(
            "this.transform:Find(\"{0}\"):GetComponent(\"UILabel\").text = TextMgr.GetLanguageText({1})", path,
            RegexCn(txt));
        return lua;
    }

    /// <summary>
    ///     判断一个字符中是否包含中文
    /// </summary>
    /// <param name="str">源字符串</param>
    /// <returns>检测结果</returns>
    public static bool HasChinese(string str)
    {
        return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
    }

    private static void ReadJson()
    {
        txtClasses = JsonMapper.ToObject<TxtClass[]>(Resources.Load<TextAsset>("TxtTableJson").text);
    }


    private static int RegexCn(string str)
    {
        for (var i = 0; i < txtClasses.Length; i++)
            if (str.Equals(txtClasses[i].Cn))
                return txtClasses[i].ID;
        Debug.LogError("匹配出错在TxtTable里面匹配不到字符串(" + str + ")");
        return 10010;
    }

    [MenuItem("Tools/海外版本工具/查找UILabel中文导出Excel")]
    public static void WriteExcel()
    {
        var path = GetSavePath("UILabel中文分析报告", "xlsx");
        var file = new FileInfo(path);
        if (file.Exists)
        {
            file.Delete();
            file = new FileInfo(path);
        }

        ReadJson();
        //获取所有的Prefab的GUID
        var excelConfigs = new Dictionary<string, ExcelConfig>();
        var assets = AssetDatabase.FindAssets("t:Prefab", PrefabPath);
        for (var i = 0; i < assets.Length; i++)
        {
            //通过GUID获取Asset路径
            var guid = AssetDatabase.GUIDToAssetPath(assets[i]);
            //加载Asset通过路径
            var gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(guid);
            var labels = gameObject.GetComponentsInChildren<UILabel>();
            foreach (var uiLabel in labels)
                if (HasChinese(uiLabel.text))
                {
                    if (excelConfigs.ContainsKey(gameObject.name))
                    {
                        excelConfigs[gameObject.name].uiLabelCompnents.Add(uiLabel);
                    }
                    else
                    {
                        var config = new ExcelConfig();
                        config.prefabPath = guid;
                        config.componentPath = GetGameObjectPath(uiLabel.gameObject).Remove(0, 1);
                        config.uiLabelCompnents = new List<UILabel>();
                        config.uiLabelCompnents.Add(uiLabel);
                        excelConfigs.Add(gameObject.name, config);
                    }
                }

            EditorUtility.DisplayProgressBar("查找中", guid, (float) i / (assets.Length - 1));
        }

        var row = 1;
        using (var package = new ExcelPackage(file))
        {
            var worksheet = package.Workbook.Worksheets.Add("中文分析报告");
            //设置表头
            worksheet.Cells[row, 1].Value = "Prefab路径";
            worksheet.Cells[row, 2].Value = "组件层级路径";
            worksheet.Cells[row, 3].Value = "组件中文值";
            //设置表头为粗体
            using (var range = worksheet.Cells[row, 1, row, 3])
            {
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.AntiqueWhite);
                range.Style.Font.Bold = true;
            }

            row++;
            //row = 2
            var count = 0;

            foreach (var excelConfig in excelConfigs.Values)
            {
                worksheet.Cells[row, 1].Value =
                    AssetDatabase.LoadAssetAtPath<GameObject>(excelConfig.prefabPath).name;
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                using (var range = worksheet.Cells[row, 1, row, 3])
                {
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#DDEBF7"));
                }

                row++;
                for (var i = 0; i < excelConfig.uiLabelCompnents.Count; i++)
                {
                    var label = excelConfig.uiLabelCompnents[i];
                    worksheet.Cells[row, 1].Value = excelConfig.prefabPath;
                    worksheet.Cells[row, 2].Value = GetGameObjectPath(label.gameObject).Remove(0, 1);
                    worksheet.Cells[row, 3].Value = label.text;
                    row++;
                }

                count++;
                row++;
                EditorUtility.DisplayProgressBar("写入Excel中", excelConfig.prefabPath,
                    (float) count / (excelConfigs.Count - 1));
            }

            package.Save();
        }

        excelConfigs = null;

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    private static List<UILabel> GetUILabelsHasCN(UILabel[] uiLabels)
    {
        var labels = new List<UILabel>();
        for (var i = 0; i < uiLabels.Length; i++)
            if (HasChinese(uiLabels[i].text))
                labels.Add(uiLabels[i]);

        return labels;
    }

        #endregion

    #region GameObject Editor

    [MenuItem("GameObject/NGUI/检查子物体UILabel是否有中文", false, 10)]
    public static void CheckChinese()
    {
        foreach (var gameObject in Selection.gameObjects)
        {
            var uiLables = gameObject.GetComponentsInChildren<UILabel>();
            var count = 0;
            foreach (var item in uiLables)
                if (HasChinese(item.text))
                {
                    Debug.Log(item.gameObject.name + "\t" + item.text);
                    count++;
                }

            Debug.Log("有" + count + "个UILabel带有中文");
        }
    }


    [MenuItem("GameObject/NGUI/把UILabel赋值为空字符", false, 10)]
    public static void ClearUILabel()
    {
        foreach (var gameObject in Selection.gameObjects)
        {
            var uiLables = gameObject.GetComponentsInChildren<UILabel>();
            foreach (var item in uiLables)
                if (HasChinese(item.text))
                    item.text = string.Empty;
            ;
        }
    }

    #endregion

    #region Bean

    private class ConentConfig
    {
        public const string EXCEL_PREFAB_PATH_TITLE = "Prefab路径";
        public const string COMPONENT_PATH_TITLE = "组件层级路径";
        public const string COMPONENT_VALUE = "组件中文值";
        public const string TXT_EXTENSION = "txt";
        public const string EXCEL_EXTENSION = "xlsx";
        public const string FILE_SAVE_NAME = "组件中文分析报告";
    }

    private class ExcelConfig
    {
        public string prefabPath { get; set; }
        public string componentPath { get; set; }
        public List<UILabel> uiLabelCompnents { get; set; }
    }


    private class TxtClass
    {
        public int ID { get; set; }
        public string Cn { get; set; }
        public string En { get; set; }
    }

    #endregion
}