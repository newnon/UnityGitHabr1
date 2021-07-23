using UnityEngine;
using UnityEditor;
using System.IO;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System;

namespace GitIntegration
{
    [InitializeOnLoad]
    public class GitHooksInstaller
    {
        const string HooksFolder = "hooks~";
        const int Version = 2;

        private static string GetThisFilePath([CallerFilePath] string path = null) => path;

        private static string ResourcesPath = MakeResourcesPath();

        static bool IsParentOrSame(string dir1, string dir2)
        {
            DirectoryInfo di1 = new DirectoryInfo(dir1);
            DirectoryInfo di2 = new DirectoryInfo(dir2);
            bool isParent = false;
            while (di2 != null)
            {
                if (di2.FullName.Equals(di1.FullName))
                {
                    isParent = true;
                    break;
                }
                else di2 = di2.Parent;
            }
            return isParent;
        }

        [MenuItem("Tools/Git/Install Hooks")]
        public static void InstallHooks()
        {
            var filePath = GetThisFilePath();
            var hooksPath = Path.Combine(Path.GetDirectoryName(filePath), HooksFolder);

            var hooksDirectory = new DirectoryInfo(hooksPath);
            var hookFiles = hooksDirectory.GetFiles();
            var projectPath = new DirectoryInfo(Application.dataPath).Parent;

            var assetsPath = Path.Combine(projectPath.FullName, "Assets");

            var gitPath = Utils.FindGitFolder();
            List<string> submodules = new List<string>();

            try
            {
                var modulesStrings = File.ReadAllLines(Path.Combine(new DirectoryInfo(gitPath).Parent.FullName, ".gitmodules"));
                for (int i = 0; i < modulesStrings.Length; i += 3)
                {
                    var internalPath = modulesStrings[i].Substring(10, modulesStrings[i].Length - 11).Trim();
                    internalPath = internalPath.Substring(1, internalPath.Length - 2);
                    var externalPath = modulesStrings[i + 1].Split('=')[1].Trim();
                    if (IsParentOrSame(assetsPath, externalPath))
                        submodules.Add(internalPath);
                }
            }
            catch (System.Exception)
            {
            }

            foreach (var file in hookFiles)
            {
                if (!Path.GetExtension(file.FullName).Equals(".meta"))
                {
                    File.Copy(file.FullName, Path.Combine(gitPath, "hooks", file.Name), true);
                    foreach (var submodule in submodules)
                    {
                        File.Copy(file.FullName, Path.Combine(gitPath, "modules", submodule, "hooks", file.Name), true);
                    }
                }
            }
            Utils.WriteInstalledVersion(ResourcesPath, Version);
            Debug.Log("Git hooks installed");
        }

        //Unity calls the static constructor when the engine opens
        static GitHooksInstaller()
        {
            if (Utils.GetInstalledVersion(ResourcesPath) != Version)
                InstallHooks();
        }

        private static string MakeResourcesPath()
        {
            var ret = Path.Combine(Directory.GetParent(GetThisFilePath()).FullName, "Resources", "GitHooks.asset");
            var result = (new Uri(Application.dataPath)).MakeRelativeUri(new Uri(ret));
            return result.ToString();
        }
    }
}