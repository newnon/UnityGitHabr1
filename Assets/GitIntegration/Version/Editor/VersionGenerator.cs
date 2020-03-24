using System;
using UnityEditor;
using UnityEngine;

static public class VersionGenerator
{
    private const string RESOURCE_PATH = "Assets/Scripts/Version/Resources/VersionManifest.asset";
    private const string UNDEFINED_VERSION = "undefined";
    private const string UNKNOWN_BRANCH = "unknown";
    private const string UNDEFINED_SHORT_VERSION = "0.0";
    private const string UNKNOWN_BUILD_TYPE = "unknown";

    public static string VersionString { get; private set; } = UNDEFINED_VERSION;
    public static string ShortVersion { get; private set; } = UNDEFINED_SHORT_VERSION;
    public static string BranchName { get; private set; } = UNKNOWN_BRANCH;
    public static string BuildType { get; private set; } = UNKNOWN_BUILD_TYPE;

    static string ExecuteGitWithParams(string param)
    {
        try
        {
            var processInfo = new System.Diagnostics.ProcessStartInfo("git");

            processInfo.UseShellExecute = false;
            processInfo.WorkingDirectory = Environment.CurrentDirectory;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardError = true;
            processInfo.CreateNoWindow = true;

            var process = new System.Diagnostics.Process();
            process.StartInfo = processInfo;
            process.StartInfo.FileName = "git";
            process.StartInfo.Arguments = param;
            process.Start();
            process.WaitForExit();
            var outString = process.StandardOutput.ReadLine();
            return outString;
        }
        catch (Exception)
        { 
            return null; 
        } 
    }

    static void SetVersions()
    {
        string currentBranch = ExecuteGitWithParams("rev-parse --abbrev-ref HEAD");
        string latestTag = ExecuteGitWithParams("describe --tags --match v* --abbrev=0");

        if (string.IsNullOrEmpty(latestTag))
        {
            if (latestTag == null)
                Debug.LogError("Git client not installed!");
            else
                Debug.LogError("Git tag not found!");
            BranchName = currentBranch;
            ShortVersion = UNDEFINED_SHORT_VERSION;
            VersionString = UNDEFINED_VERSION;
            return;
        }

        
        string commitCount = ExecuteGitWithParams(string.Format("rev-list --no-merges --count --invert-grep --grep=@skip_version --all-match {0}", latestTag.Length == 0 ? "HEAD" : String.Format("{0}..", latestTag)));

        Debug.Log(latestTag);
        string versionTag = latestTag.Remove(0, 1);

        if (currentBranch.StartsWith("release/"))
        {
            BranchName = currentBranch.Remove(0, "release/".Length);
            var version = Version.Parse(BranchName);
            ShortVersion = VersionString = string.Format("{0}.{1}.{2}", version.Major, version.Minor, commitCount);
            BuildType = "release";
        }
        else if (currentBranch.StartsWith("feature/"))
        {
            BranchName = currentBranch.Remove(0, "feature/".Length);
            VersionString = string.Format("{1}-{0}.{2}", versionTag, BranchName, commitCount);
            ShortVersion = string.Format("{0}.{1}", versionTag, commitCount);
            BuildType = "feature";
        }
        else if (currentBranch.StartsWith("hotfix/"))
        {
            BranchName = currentBranch.Remove(0, "hotfix/".Length);
            VersionString = string.Format("{1}-{0}.{2}", versionTag, BranchName, commitCount);
            ShortVersion = string.Format("{0}.{1}", versionTag, commitCount);
            BuildType = "feature";
        }
        else
        {
            BranchName = currentBranch;
            VersionString = string.Format("{1}-{0}.{2}", versionTag, currentBranch, commitCount);
            ShortVersion = string.Format("{0}.{1}", versionTag, commitCount);
            BuildType = "dev";
        }
    }

    [MenuItem("Tools/Git/Print version")]
    static void PrintBuildNumber()
    {
        Debug.Log(VersionString);
        Debug.Log(BranchName);
        Debug.Log(ShortVersion);
    }

    static void WriteVersionManifest()
    {
        var oldManifest = AssetDatabase.LoadAssetAtPath<VersionManifest>(RESOURCE_PATH);
        if (oldManifest && oldManifest.VersionString == VersionString)
            return;

        var manifest = ScriptableObject.CreateInstance<VersionManifest>();
        manifest.VersionString = VersionString;
        manifest.ShortVersion = ShortVersion;
        manifest.BranchName = BranchName;
        manifest.BuildType = BuildType;
        AssetDatabase.CreateAsset(manifest, RESOURCE_PATH);
    }
    
    [InitializeOnLoadMethod]
    static void OnProjectLoadedInEditor()
    {
        SetVersions();
        WriteVersionManifest();
        PrintBuildNumber();
    }
}