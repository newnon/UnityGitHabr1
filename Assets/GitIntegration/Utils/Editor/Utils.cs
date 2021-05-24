using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GitIntegration
{
    public class Utils
    {
        public static int GetInstalledVersion(string path)
        {
            var oldManifest = AssetDatabase.LoadAssetAtPath<InstalledVersionManifest>(path);
            if (oldManifest)
                return oldManifest.Version;
            else
                return 0;
        }

        public static void WriteInstalledVersion(string path, int version)
        {
            var manifest = ScriptableObject.CreateInstance<InstalledVersionManifest>();
            manifest.Version = version;
            AssetDatabase.CreateAsset(manifest, path);
        }

        public class ExitCodeException : Exception
        {
            public ExitCodeException(string message) : base(message) {}
        }

        public static string ExecuteGitWithParams(string param)
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

            if (process.ExitCode != 0)
                throw new ExitCodeException("gir error: " + process.StandardError.ReadLine());

            return process.StandardOutput.ReadLine();
        }
        public static string FindGitFolder()
        {
            var dirInfo = new DirectoryInfo(Application.dataPath);
            while (dirInfo.Parent != null)
            {
                dirInfo = dirInfo.Parent;
                var gitDir = dirInfo.GetDirectories(".git", SearchOption.TopDirectoryOnly);
                if (gitDir.Length > 0)
                {
                    return gitDir[0].FullName;
                }
            }
            throw new Exception(".git folder cannot be found");
        }
    }
}