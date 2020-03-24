using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace EditorScripts.git_hooks
{
    /// <summary>
    /// Cleans all the empty folders
    /// </summary>
    public class GitDirCleanerEditor
    {
        /// <summary>
        /// Cleans empty folders and corresponding .meta files in the Assets folder
        /// </summary>
        [MenuItem("Tools/Git Helper/Clean Empty Folders")]
        public static void CleanEmptyFolders()
        {
            var directoryInfo = new DirectoryInfo(Application.dataPath).Parent;
            if (directoryInfo == null)
            {
                return;
            }

            var assetsPath = Path.Combine(directoryInfo.ToString(), "assets");
            Debug.Log($"Start cleaning: {assetsPath}");

            var found = true; // TRUE means that at least one directory was deleted during the DeleteIteration(...)  
            while (found)
            {
                found = CleanIteration(assetsPath);
            }

            Debug.Log($"Finish cleaning: {assetsPath}");
        }

        /// <summary>
        /// Deletes all the tail empty folders and corresponding metas
        /// </summary>
        /// <param name="rootFolderPath">Root folder</param>
        /// <returns>Returns true, if atl least one folder was deleted in the iteration</returns>
        private static bool CleanIteration(string rootFolderPath)
        {
            var pending = new Queue<string>();
            pending.Enqueue(rootFolderPath);

            var result = false;

            while (pending.Count > 0)
            {
                rootFolderPath = pending.Dequeue();
                string[] files;

                try
                {
                    files = Directory.GetFiles(rootFolderPath);
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }

                var foundCandidateDirectory =
                    files.Length == 0 || files.Length == 1 && files[0].ToLower().Contains(".meta");


                var subDirectories = Directory.GetDirectories(rootFolderPath);

                if (foundCandidateDirectory && subDirectories.Length == 0)
                {
                    result = true;
                    var directoryInfo = new DirectoryInfo(rootFolderPath);
                    var parentFolder = directoryInfo.Parent.FullName;

                    var siblings = Directory.GetFiles(parentFolder);
                    foreach (var sibling in siblings)
                    {
                        // Ignore all the files except the directory .meta
                        if (!sibling.Contains(new DirectoryInfo(rootFolderPath).Name + ".meta")) 
                        {
                            continue;
                        }


                        File.Delete(sibling);
                        Debug.LogWarning($"File {sibling} was deleted");
                    }

                    Debug.LogWarning($"Folder {rootFolderPath} was deleted");

                    Directory.Delete(rootFolderPath, true);
                }
                else // If the directory is not the tail one, add all the subirectories to the iteration
                {
                    foreach (var t in subDirectories)
                    {
                        pending.Enqueue(t);
                    }
                }
            }

            return result;
        }
    }
}