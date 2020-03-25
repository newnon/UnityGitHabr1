using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace GitIntegration
{
    static public class VersionGenerator
    {
        public static string VersionString { get; private set; } = VersionHelper.UNDEFINED_VERSION;
        public static string ShortVersion { get; private set; } = VersionHelper.UNDEFINED_SHORT_VERSION;
        public static string BranchName { get; private set; } = VersionHelper.UNKNOWN_BRANCH;
        public static string BuildType { get; private set; } = VersionHelper.UNKNOWN_BUILD_TYPE;

        private static string GetThisFilePath([CallerFilePath] string path = null) => path;

        private static string ManifestPath = MakeManifestPath();
        
        private static string MakeManifestPath()
        {
            var ret = Path.Combine(Directory.GetParent(GetThisFilePath()).Parent.FullName, "Resources", "VersionManifest.asset");
            var result = (new Uri(Application.dataPath)).MakeRelativeUri(new Uri(ret)); 
            return result.ToString();
        }

        static void SetVersions() 
        {
            try
            {
                BranchName = Utils.ExecuteGitWithParams("rev-parse --abbrev-ref HEAD");
            }
            catch (Exception e)
            {
                Debug.LogError($"Can't read branch name with error: {e}");
                return;
            }
            string latestTag;
            try
            {
                latestTag = Utils.ExecuteGitWithParams("describe --tags --match v* --abbrev=0");
            }
            catch (Exception e)
            {
                Debug.LogError($"Can't find tag with error: {e}");
                return;
            }

            string commitCount = Utils.ExecuteGitWithParams(string.Format("rev-list --no-merges --count --invert-grep --grep=@skip_version --all-match {0}", latestTag.Length == 0 ? "HEAD" : String.Format("{0}..", latestTag)));

            string versionTag = latestTag.Remove(0, 1);

            if (BranchName.StartsWith("release/"))
            {
                var temp = BranchName.Remove(0, "release/".Length);
                var version = Version.Parse(temp);
                ShortVersion = VersionString = string.Format("{0}.{1}.{2}", version.Major, version.Minor, commitCount);
                BuildType = "release";
            }
            else if (BranchName.StartsWith("feature/"))
            {
                var temp = BranchName.Remove(0, "feature/".Length);
                VersionString = string.Format("{1}-{0}.{2}", versionTag, temp, commitCount);
                ShortVersion = string.Format("{0}.{1}", versionTag, commitCount);
                BuildType = "feature";
            }
            else if (BranchName.StartsWith("hotfix/"))
            {
                var temp = BranchName.Remove(0, "hotfix/".Length);
                VersionString = string.Format("{1}-{0}.{2}", versionTag, temp, commitCount);
                ShortVersion = string.Format("{0}.{1}", versionTag, commitCount);
                BuildType = "hotfix";
            }
            else
            {
                VersionString = string.Format("{1}-{0}.{2}", versionTag, BranchName, commitCount);
                ShortVersion = string.Format("{0}.{1}", versionTag, commitCount);
                BuildType = "dev";
            }
        }

        [MenuItem("Tools/Git/Print version")]
        static void PrintBuildNumber()
        {
            Debug.Log($"VersionString: {VersionString}");
            Debug.Log($"BranchName: {BranchName}");
            Debug.Log($"ShortVersion: {ShortVersion}");
        }

        static void WriteVersionManifest()
        {
            var oldManifest = AssetDatabase.LoadAssetAtPath<VersionManifest>(ManifestPath);
            if (oldManifest && oldManifest.VersionString == VersionString)
                return;

            var manifest = ScriptableObject.CreateInstance<VersionManifest>();
            manifest.VersionString = VersionString;
            manifest.ShortVersion = ShortVersion;
            manifest.BranchName = BranchName;
            manifest.BuildType = BuildType; 
            AssetDatabase.CreateAsset(manifest, ManifestPath);
        }

        [InitializeOnLoadMethod]
        static void OnProjectLoadedInEditor()
        {
            SetVersions();
            WriteVersionManifest();
            PrintBuildNumber();
        }
    }
}