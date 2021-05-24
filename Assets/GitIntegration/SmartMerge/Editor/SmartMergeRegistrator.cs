using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace GitIntegration
{
    [InitializeOnLoad]
    public class SmartMergeRegistrator
    {
        const int Version = 1;

#if UNITY_EDITOR_OSX
        private const string UnityyamlmergeFileName = "/UnityYAMLMerge";
#else
        private const string UnityyamlmergeFileName = "/UnityYAMLMerge.exe";
#endif

        private static string GetThisFilePath([CallerFilePath] string path = null) => path;

        private static string ResourcesPath = MakeResourcesPath();

        [MenuItem("Tools/Git/SmartMerge registration")]
        static void SmartMergeRegister()
        {
            try
            {
                var UnityYAMLMergePath = EditorApplication.applicationContentsPath + "/Tools" + UnityyamlmergeFileName;
                Utils.ExecuteGitWithParams("config merge.unityyamlmerge.name \"Unity SmartMerge (UnityYamlMerge)\"");
                Utils.ExecuteGitWithParams($"config merge.unityyamlmerge.driver \"\\\"{UnityYAMLMergePath}\\\" merge -h -p --force --fallback none %O %B %A %A\"");
                Utils.ExecuteGitWithParams("config merge.unityyamlmerge.recursive binary");
                Utils.WriteInstalledVersion(ResourcesPath, Version);
                Debug.Log($"Successfully registered UnityYAMLMerge with path {UnityYAMLMergePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Fail to register UnityYAMLMerge with error: {e}");
            }
        }

        //Unity calls the static constructor when the engine opens
        static SmartMergeRegistrator()
        {;
            if (Utils.GetInstalledVersion(ResourcesPath) != Version)
                SmartMergeRegister();
        }

        private static string MakeResourcesPath()
        {
            var ret = Path.Combine(Directory.GetParent(GetThisFilePath()).FullName, "Resources", "SmartMergeRegistrator.asset");
            var result = (new Uri(Application.dataPath)).MakeRelativeUri(new Uri(ret));
            return result.ToString();
        }
    }
}