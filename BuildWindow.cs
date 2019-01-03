using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BuildWindow : EditorWindow
{
    private static EditorWindow window;
    private bool bAbPackage;
    private bool bBuildAb;
    private bool bCopyRes;
    private bool bLoad;
    private List<BuildData> buildDatas;
    private Vector3 mScroll = Vector2.zero;

    [MenuItem("Tools/打包工具")]
    public static void OpenBuildWindow()
    {
        window = GetWindow(typeof(BuildWindow), false, "出包工具");
    }

    private void Load()
    {
        buildDatas = new List<BuildData>();
        foreach (var value in Enum.GetValues(typeof(ChannelConfig.ChannelType)))
        {
            var buildData = new BuildData(true, false, false, true,
                Enum.GetName(typeof(ChannelConfig.ChannelType), value));
            buildData.ChannelType = (ChannelConfig.ChannelType) value;
            buildDatas.Add(buildData);
        }

        bLoad = true;
    }

    private void OnGUI()
    {
        if (GUILayout.Button(new GUIContent("加载打包数据")))
            Load();

        if (bLoad)
            ShowAllBuildData();
    }

    private void OnDisable()
    {
        bLoad = false;
        buildDatas = null;
    }


    private void ShowAllBuildData()
    {
        var color = GUI.color;
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("渠道名", GUILayout.Width(50), GUILayout.MaxWidth(100));
            GUILayout.Label("打包类型", GUILayout.Width(50), GUILayout.MaxWidth(100));
            GUILayout.Label("拷贝资源", GUILayout.Width(50), GUILayout.MaxWidth(100));
            GUILayout.Label("生成AB(需要Ab包)", GUILayout.Width(50), GUILayout.MaxWidth(100));
            GUILayout.Label("一键打包是否包含", GUILayout.Width(50), GUILayout.MaxWidth(100));
            GUILayout.Label("Android平台", GUILayout.Width(50), GUILayout.MaxWidth(100));
#if UNITY_IOS
            GUILayout.Label("IOS平台", GUILayout.Width(50), GUILayout.MaxWidth(100));
#endif

        }
        GUILayout.EndHorizontal();

        GUILayout.BeginVertical();
        {
            mScroll = GUILayout.BeginScrollView(mScroll, GUILayout.MaxHeight(400));
            for (var i = 0; i < buildDatas.Count; i++)
            {
                var data = buildDatas[i];
                GUILayout.BeginHorizontal(GUIStyle.none);

                GUILayout.Label(data.ChannelName, GUILayout.Width(50), GUILayout.MaxWidth(100));
                data.BAb = GUILayout.Toggle(data.BAb, new GUIContent("Ab包"), GUILayout.Width(50),
                    GUILayout.MaxWidth(100));
                data.BCopyRes = GUILayout.Toggle(data.BCopyRes, new GUIContent("拷贝资源"), GUILayout.Width(50),
                    GUILayout.MaxWidth(100));
                data.BBuildAb = GUILayout.Toggle(data.BAb && data.BBuildAb, new GUIContent("生成Ab"), GUILayout.Width(50),
                    GUILayout.MaxWidth(100));
                data.BJoinBuildQueue = GUILayout.Toggle(data.BJoinBuildQueue, new GUIContent("加入构建队列"),
                    GUILayout.Width(50),
                    GUILayout.MaxWidth(100));

                if (GUILayout.Button("APK", GUILayout.Width(50), GUILayout.MaxWidth(100)))
                    BuildAPK(data);

#if UNITY_IOS
                 if (GUILayout.Button("IPA", GUILayout.Width(50), GUILayout.MaxWidth(100)))
                    BuildIPA(data);
#endif



                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
        GUILayout.EndVertical();

        GUI.color = Color.green;



        #region 一键出包以及一些版本信息

        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("所有渠道", GUILayout.Width(50), GUILayout.MaxWidth(100));
            bAbPackage = GUILayout.Toggle(bAbPackage, new GUIContent("Ab包"), GUILayout.Width(50),
                GUILayout.MaxWidth(100));
            bCopyRes = GUILayout.Toggle(bCopyRes, new GUIContent("拷贝资源"), GUILayout.Width(50), GUILayout.MaxWidth(100));
            bBuildAb = GUILayout.Toggle(bAbPackage && bBuildAb, new GUIContent("生成Ab"), GUILayout.Width(50),
                GUILayout.MaxWidth(100));

            if (GUILayout.Button("同步所有出包信息", GUILayout.Width(50), GUILayout.MaxWidth(100)))
            {
                // 把上面所有渠道的打包类型，拷贝资源，生成ab资源同步到我全部渠道的设置(不同步加入打包队列)
                foreach (var buildData in buildDatas)
                {
                    buildData.BAb = bAbPackage;
                    buildData.BBuildAb = bBuildAb;
                    buildData.BCopyRes = bCopyRes;
                }
            }


            if (GUILayout.Button("APK(all)", GUILayout.Width(50), GUILayout.MaxWidth(100)))
                BuildAllAPK();
#if UNITY_IOS
             if (GUILayout.Button("IPA(all)", GUILayout.Width(50), GUILayout.MaxWidth(100)))
                BuilsAllIPA();
#endif

        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("版本信息", GUILayout.Width(50), GUILayout.MaxWidth(100));
            GUILayout.Label("bundleVersion:" + PlayerSettings.bundleVersion, GUILayout.Width(100),
                GUILayout.MaxWidth(200));
#if UNITY_ANDROID
            GUILayout.Label("bundleVersionCode:" + PlayerSettings.Android.bundleVersionCode, GUILayout.Width(100),
                GUILayout.MaxWidth(200));
#elif UNITY_IOS
            GUILayout.Label("buildNumber:" + PlayerSettings.iOS.buildNumber, GUILayout.Width(50), GUILayout.MaxWidth(200));
#endif
            if (GUILayout.Button(new GUIContent("提升版本"), GUILayout.Width(50), GUILayout.MaxWidth(100)))
                BuildPackageBase.VersionIncrement();
        }
        GUILayout.EndHorizontal();

        #endregion

    }

    private void BuildAPK(BuildData buildData)
    {
        Debug.Log(buildData.ToString("Android"));
        ProjectBuild.AutoProjectBuildAndroid(GetResType(buildData), buildData.ChannelType);
    }

    private void BuildIPA(BuildData buildData)
    {
        Debug.Log(buildData.ToString("IOS"));
        ProjectBuild.AutoProjectBuildIOS(GetResType(buildData), buildData.ChannelType);
    }

    private ProjectBuild.ResType GetResType(BuildData data)
    {
        var type = ProjectBuild.ResType.NotRes;
        if (data.BAb && data.BBuildAb)
            type = ProjectBuild.ResType.BuildAB;
        else if (data.BAb && !data.BBuildAb)
            type = ProjectBuild.ResType.AB;
        else if (!data.BCopyRes && !data.BAb)
            type = ProjectBuild.ResType.NotRes;
        else if (!data.BAb && data.BCopyRes)
            type = ProjectBuild.ResType.Res;
        return type;
    }


    private void BuildAllAPK()
    {
        if (!EditorUtility.DisplayDialog("自动打加入了构建队列的包", "打包时间比较长，确定启动打包流程吗？", "确定", "取消"))
            return;
        foreach (var buildData in buildDatas)
        {
            if (!buildData.BJoinBuildQueue)
                continue;
            Debug.Log(buildData.ToString("Android"));
            ProjectBuild.AutoProjectBuildAll(false, GetResType(buildData), buildData.ChannelType);
        }
    }

    private void BuilsAllIPA()
    {
        if (!EditorUtility.DisplayDialog("自动打加入了构建队列的包", "打包时间比较长，确定启动打包流程吗？", "确定", "取消"))
            return;
        foreach (var buildData in buildDatas)
        {
            if (!buildData.BJoinBuildQueue)
                continue;
            Debug.Log(buildData.ToString("IOS"));
            ProjectBuild.AutoProjectBuildAll(true, GetResType(buildData), buildData.ChannelType);
        }
    }
}

internal class BuildData
{
    public BuildData(bool bAb, bool bBuildAb, bool bCopyRes, bool bJoinBuildQueue, string channelName)
    {
        BAb = bAb;
        BBuildAb = bBuildAb;
        BCopyRes = bCopyRes;
        BJoinBuildQueue = bJoinBuildQueue;
        ChannelName = channelName;
    }

    public bool BAb { set; get; }
    public bool BBuildAb { set; get; }
    public bool BCopyRes { set; get; }
    public bool BJoinBuildQueue { set; get; }
    public string ChannelName { set; get; }
    public ChannelConfig.ChannelType ChannelType { set; get; }

    public string ToString(string platform)
    {
        var sAb = BAb ? "Ab" : "非Ab";
        var sCpoy = BCopyRes ? "拷贝" : "不拷贝";
        var sBuilsAb = BBuildAb ? "生成" : "不生成";
        return string.Format("Build{0}渠道的{1}平台{2}资源的{3}包\t{4}Assetbundle", ChannelName, platform, sCpoy, sAb, sBuilsAb);
    }
}