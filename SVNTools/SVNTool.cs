using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SvnTools
{
    public static class SVNTool
    {
        /// <summary>
        /// SVN更新指定的路径
        /// 路径示例：Assets/1.png
        /// </summary>
        /// <param name="assetPaths"></param>
        public static void UpdateAtPath(string assetPath)
        {
            List<string> assetPaths = new List<string>();
            assetPaths.Add(assetPath);
            UpdateAtPaths(assetPaths);
        }

        /// <summary>
        /// SVN更新指定的路径
        /// 路径示例：Assets/1.png
        /// </summary>
        /// <param name="assetPaths"></param>
        public static void UpdateAtPaths(List<string> assetPaths)
        {
            if (assetPaths.Count == 0)
            {
                return;
            }

            string arg = "/command:update /closeonend:0 /path:\"";
            for (int i = 0; i < assetPaths.Count; i++)
            {
                var assetPath = assetPaths[i];
                if (i != 0)
                {
                    arg += "*";
                }
                arg += assetPath;
            }
            arg += "\"";
            SvnCommandRun(arg);
        }

        /// <summary>
        /// SVN提交指定的路径
        /// 路径示例：Assets/1.png
        /// </summary>
        /// <param name="assetPaths"></param>
        public static void CommitAtPaths(List<string> assetPaths, string logmsg = null)
        {
            if (assetPaths.Count == 0)
            {
                return;
            }

            string arg = "/command:commit /closeonend:0 /path:\"";
            for (int i = 0; i < assetPaths.Count; i++)
            {
                var assetPath = assetPaths[i];
                if (i != 0)
                {
                    arg += "*";
                }
                arg += assetPath;
            }
            arg += "\"";
            if (!string.IsNullOrEmpty(logmsg))
            {
                arg += " /logmsg:\"" + logmsg + "\"";
            }
            SvnCommandRun(arg);
        }

        public static void RevertAtPaths(List<string> assetPaths, string logmsg = null)
        {
            if (assetPaths.Count == 0)
            {
                return;
            }
            string arg = "/command:revert /closeonend:0 /path:\"";
            for (int i = 0; i < assetPaths.Count; i++)
            {
                var assetPath = assetPaths[i];
                if (i != 0)
                {
                    arg += "*";
                }
                arg += assetPath;
            }
            arg += "\"";
            if (!string.IsNullOrEmpty(logmsg))
            {
                arg += " /logmsg:\"" + logmsg + "\"";
            }
            SvnCommandRun(arg);
        }

        [MenuItem("Assets/SVN Tool/SVN 更新")]
        private static void SvnToolUpdate()
        {
            List<string> assetPaths = SelectionUtil.GetSelectionAssetPaths();
            UpdateAtPaths(assetPaths);
        }

        [MenuItem("Assets/SVN Tool/SVN 提交...")]
        private static void SvnToolCommit()
        {
            List<string> assetPaths = SelectionUtil.GetSelectionAssetPaths();
            CommitAtPaths(assetPaths);
        }

        [MenuItem("Assets/SVN Tool/SVN 还原")]
        private static void SvnToolRevert()
        {
            List<string> assetPaths = SelectionUtil.GetSelectionAssetPaths();
            RevertAtPaths(assetPaths);
        }

        [MenuItem("Assets/SVN Tool/显示日志")]
        private static void SvnToolLog()
        {
            List<string> assetPaths = SelectionUtil.GetSelectionAssetPaths();
            if (assetPaths.Count == 0)
            {
                return;
            }

            // 显示日志，只能对单一资产
            string arg = "/command:log /closeonend:0 /path:\"";
            arg += assetPaths[0];
            arg += "\"";
            SvnCommandRun(arg);
        }

        [MenuItem("Assets/SVN Tool/全部更新", false, 1100)]
        private static void SvnToolAllUpdate()
        {
            // 往上两级，包括数据配置文件
            string arg = "/command:update /closeonend:0 /path:\"";
            arg += "..";
            arg += "\"";
            SvnCommandRun(arg);
        }

        [MenuItem("Assets/SVN Tool/全部日志", false, 1101)]
        private static void SvnToolAllLog()
        {
            // 往上两级，包括数据配置文件
            string arg = "/command:log /closeonend:0 /path:\"";
            arg += "..";
            arg += "\"";
            SvnCommandRun(arg);
        }

        /// <summary>
        /// SVN命令运行
        /// </summary>
        /// <param name="arg"></param>
        private static void SvnCommandRun(string arg)
        {
            string workDirectory = Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets", StringComparison.Ordinal));
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = "TortoiseProc",
                Arguments = arg,
                WorkingDirectory = workDirectory
            });
        }
    }
}