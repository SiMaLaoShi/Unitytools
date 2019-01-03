using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class UILabelTools : Editor
{
    private static readonly string[] PrefabPath =
    {
        "Assets/Resources/Prefabs/UI"
    };

    private static StringBuilder sb;

    [MenuItem("Tools/查找所有UILabel")]
    private static void FindUILabel()
    {
        var filePath = Application.dataPath + "/uilabel.txt";
        //todo 写入excel表
        if (File.Exists(filePath)) File.Delete(filePath);
        var streamWriter = File.CreateText(filePath);
        var prefabs = AssetDatabase.FindAssets("t:Prefab", PrefabPath);
        for (var i = 0; i < prefabs.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(prefabs[i]);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var labels = go.transform.GetComponentsInChildren<UILabel>(true);
            for (var j = 0; j < labels.Length; j++)
                if (HasChinese(labels[j].text))
                    streamWriter.WriteLine("{0}\t{1}\t{2}\n", path, GetGameObjectPath(labels[j].gameObject),
                        labels[j].text);
            EditorUtility.DisplayProgressBar("查找中", path, (float) i / (prefabs.Length - 1));
        }

        EditorUtility.ClearProgressBar();
        streamWriter.Flush();
        streamWriter.Close();
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

    /// 判断字符串中是否包含中文
    /// </summary>
    /// <param name="str">需要判断的字符串</param>
    /// <returns>判断结果</returns>
    public static bool HasChinese(string str)
    {
        return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
    }
}