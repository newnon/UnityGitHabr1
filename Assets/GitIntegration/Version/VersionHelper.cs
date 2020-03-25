using UnityEngine;

namespace GitIntegration
{
    public static class VersionHelper
    {
        public const string UNDEFINED_VERSION = "undefined";
        public const string UNKNOWN_BRANCH = "unknown";
        public const string UNDEFINED_SHORT_VERSION = "0.0";
        public const string UNKNOWN_BUILD_TYPE = "unknown";

        public static string VersionString { get; }
        public static string ShortVersion { get; }
        public static string BranchName { get; }
        public static string BuildType { get; }

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
}