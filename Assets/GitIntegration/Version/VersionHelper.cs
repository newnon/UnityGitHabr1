using System;
using UnityEngine;

public static class VersionHelper
{
    private const string UNDEFINED_VERSION = "undefined";
    private const string UNKNOWN_BRANCH = "unknown";
    private const string UNDEFINED_SHORT_VERSION = "0.0";
    private const string UNKNOWN_BUILD_TYPE = "unknown";

    public static string VersionString { get; }
    public static string ShortVersion { get; }
    public static string BranchName { get; }
    public static string BuildType { get;  }

    static VersionHelper()
    {
        var versionManifest = Resources.Load<VersionManifest>("VersionManifest");
        if (versionManifest != null)
        {
            VersionString = versionManifest.VersionString;
            ShortVersion = versionManifest.ShortVersion;
            BranchName = versionManifest.BranchName;
            BuildType = versionManifest.BuildType;
        }
        else
        {
            VersionString = UNDEFINED_VERSION;
            ShortVersion = UNDEFINED_SHORT_VERSION;
            BranchName = UNKNOWN_BRANCH;
            BuildType = UNKNOWN_BUILD_TYPE;
        }

    }
}