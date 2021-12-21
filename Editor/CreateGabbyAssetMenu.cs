using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace PotassiumK.GabbyDialogue
{
    public class CreateGabbyAssetMenu
    {
        [MenuItem("Assets/Create/Gabby Script", false, 2000)]
        private static void CreateGabbyScriptAsset(MenuCommand command)
        {
            // Create a new Gabby script
            string directory = "Assets/";
            if (Selection.activeObject != null)
            {
                directory = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (directory.Length == 0)
                {
                    return;
                }

                // If the selected item is not a directory, get the containing folder
                FileAttributes attr = File.GetAttributes(directory);
                if (!attr.HasFlag(FileAttributes.Directory))
                {
                    directory = Path.GetDirectoryName(directory);
                }
            }

            string filePath = Path.Combine(directory, "New Gabby Script.gab");
            if (File.Exists(filePath))
            {
                filePath = Path.Combine(directory, $"New Gabby Script ({DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}).gab");
                if (File.Exists(filePath))
                {
                    Debug.LogError("Unable to create file - file exists.");
                    return;
                }
            }

            // Create the file
            string[] fileContents =
            {
                "gabby 0.2",
                "language english",
                "",
                "// Write your dialogue here"
            };

            StringBuilder sb = new StringBuilder();
            foreach (string line in fileContents)
            {
                sb.Append(line);
                sb.Append("\n");
            }

            ProjectWindowUtil.CreateAssetWithContent(filePath, sb.ToString());
        }
    }
}
