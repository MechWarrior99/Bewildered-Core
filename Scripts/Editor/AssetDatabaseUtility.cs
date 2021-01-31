using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Bewildered.Editor
{
    public static class AssetDatabaseUtility
    {
        /// <summary>
        /// Create a text file asset in the current Project Window folder.
        /// </summary>
        /// <param name="fileName">The name of the file to create. Should not be a path, and must contain the file extention.</param>
        /// <param name="contents">The content to write to the newly created file.</param>
        /// <returns>The path reletive to the project where the file was created.</returns>
        public static string CreateFile(string fileName, string contents)
        {
            if (!Path.HasExtension(fileName))
                throw new System.ArgumentException("Does not contain file extention.", nameof(fileName));

            string path = GetProjectWindowPath();

            path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, fileName));
            File.WriteAllText(path, contents);
            AssetDatabase.ImportAsset(path);
            return path;
        }

        /// <summary>
        /// Returns the path reletive to the project of the project window.
        /// </summary>
        /// <returns>The asset path reletive to the project folder that the project window is currently open to, if none found returns "Assets".</returns>
        public static string GetProjectWindowPath()
        {
            foreach (Object obj in Selection.GetFiltered<Object>(SelectionMode.Assets))
            {
                var path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path))
                    continue;

                if (Directory.Exists(path))
                    return path;
                else if (File.Exists(path))
                    return Path.GetDirectoryName(path);
            }

            return "Assets";
        }

        /// <summary>
        /// Create any folders in a path that do not already exist in the asset database. Path must start with "Assets/", file names are ignored.
        /// </summary>
        /// <param name="path">The asset path to check for and create folders along.</param>
        public static void CreateFoldersAsNeeded(string path)
        {
            string fileName = Path.GetFileName(path);
            string folderPath = path.Replace(fileName, "");
            if (folderPath.EndsWith("/"))
                folderPath = folderPath.Remove(folderPath.Length - 1);

            if (AssetDatabase.IsValidFolder(folderPath))
                return;

            string[] splitPath = folderPath.Split('/');
            string completePathToPoint = "Assets";

            for (int i = 1; i < splitPath.Length; i++)
            {
                completePathToPoint += splitPath[i];
                if (AssetDatabase.IsValidFolder(completePathToPoint))
                    continue;

                AssetDatabase.CreateFolder(splitPath[i - 1], splitPath[i]);
            }
        }

        /// <summary>
        /// Set the icon for a script asset.
        /// </summary>
        /// <remarks>Useful for when generating <see cref="Object"/> scripts like <see cref="ScriptableObject"/>s or <see cref="Component"/>s.</remarks>
        /// <param name="script">The script asset to set the icon of.</param>
        /// <param name="icon">The icon to set.</param>
        public static void SetScriptIcon(MonoScript script, Texture2D icon)
        {
            MethodInfo setIconForObject = typeof(EditorGUIUtility).GetMethod("SetIconForObject", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo copyMonoScriptIconToImporters = typeof(MonoImporter).GetMethod("CopyMonoScriptIconToImporters", BindingFlags.Static | BindingFlags.NonPublic);

            setIconForObject.Invoke(null, new object[] { script, icon });
            copyMonoScriptIconToImporters.Invoke(null, new object[] { script });
        }
    } 
}
