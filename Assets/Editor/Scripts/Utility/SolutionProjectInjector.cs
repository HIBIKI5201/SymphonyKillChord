using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.Utility
{
    /// <summary>
    ///     Unityがソリューションを再生成する際に、SinfoniaOperatorのプロジェクトを自動で追加するクラス。
    ///     これによりUnityのソリューションを開いたままBot側のコードを編集できる。
    /// </summary>
    public class SolutionProjectInjector : AssetPostprocessor
    {
        /// <summary> SDKスタイルC#プロジェクトのプロジェクトタイプGUID。 </summary>
        private const string SDK_PROJECT_TYPE_GUID = "9A19103F-16F7-4668-B9B4-C15CBF684C2B";

        /// <summary> ソリューションへ追加するプロジェクトの相対パス。 </summary>
        private static readonly string[] _projectPaths =
        {
            "SinfoniaOperator/SinfoniaOperator/SinfoniaOperator.csproj",
            "SinfoniaOperator/SinfoniaOperator.Core/SinfoniaOperator.Core.csproj",
        };

        /// <summary>
        ///     ソリューション生成時に呼ばれ、プロジェクトを注入した内容を返す。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private static string OnGeneratedSlnSolution(string path, string content)
        {
            try
            {
                // 既に追加済みなら何もしない。
                if (content.Contains("SinfoniaOperator.csproj")) { return content; }

                // slnx（XML形式）と従来のsln形式の両方に対応する。
                return content.TrimStart().StartsWith("<")
                    ? InjectToSlnx(content)
                    : InjectToSln(content);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{nameof(SolutionProjectInjector)}] ソリューションへのプロジェクト追加に失敗しました: {ex.Message}");
                return content;
            }
        }

        /// <summary>
        ///     slnx（XML形式）へプロジェクトを追加する。
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static string InjectToSlnx(string content)
        {
            StringBuilder entries = new();
            foreach (string projectPath in _projectPaths)
            {
                entries.AppendLine($"  <Project Path=\"{projectPath}\" />");
            }

            return content.Replace("</Solution>", $"{entries}</Solution>");
        }

        /// <summary>
        ///     従来のsln形式へプロジェクトを追加する。
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static string InjectToSln(string content)
        {
            string[] configs = GetSolutionConfigs(content);
            StringBuilder projectSb = new();
            StringBuilder configSb = new();

            foreach (string projectPath in _projectPaths)
            {
                string name = Path.GetFileNameWithoutExtension(projectPath);
                string windowsPath = projectPath.Replace('/', '\\');
                string guid = CreateDeterministicGuid(projectPath);

                projectSb.Append($"Project(\"{{{SDK_PROJECT_TYPE_GUID}}}\") = \"{name}\", \"{windowsPath}\", \"{{{guid}}}\"\r\nEndProject\r\n");

                foreach (string config in configs)
                {
                    configSb.Append($"\t\t{{{guid}}}.{config}.ActiveCfg = {config}\r\n");
                    configSb.Append($"\t\t{{{guid}}}.{config}.Build.0 = {config}\r\n");
                }
            }

            // Global行の直前にProject定義を挿入する。
            Match globalMatch = Regex.Match(content, @"^Global\r?$", RegexOptions.Multiline);
            if (!globalMatch.Success) { return content; }

            content = content.Insert(globalMatch.Index, projectSb.ToString());

            // ProjectConfigurationPlatformsセクションの先頭にビルド構成を挿入する。
            Match sectionMatch = Regex.Match(content, @"GlobalSection\(ProjectConfigurationPlatforms\)[^\r\n]*\r?\n");
            if (sectionMatch.Success)
            {
                content = content.Insert(sectionMatch.Index + sectionMatch.Length, configSb.ToString());
            }

            return content;
        }

        /// <summary>
        ///     ソリューションのビルド構成一覧を取得する。
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static string[] GetSolutionConfigs(string content)
        {
            Match match = Regex.Match(
                content,
                @"GlobalSection\(SolutionConfigurationPlatforms\)(.*?)EndGlobalSection",
                RegexOptions.Singleline);
            if (!match.Success) { return new[] { "Debug|Any CPU" }; }

            return match.Groups[1].Value
                .Split('\n')
                .Select(line => line.Split('=')[0].Trim())
                .Where(config => !string.IsNullOrEmpty(config) && config.Contains("|"))
                .Distinct()
                .ToArray();
        }

        /// <summary>
        ///     プロジェクトパスから決定的なGUIDを生成する。
        /// </summary>
        /// <param name="projectPath"></param>
        /// <returns></returns>
        private static string CreateDeterministicGuid(string projectPath)
        {
            using MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes($"SinfoniaOperator:{projectPath}"));
            return new Guid(hash).ToString().ToUpperInvariant();
        }
    }
}
