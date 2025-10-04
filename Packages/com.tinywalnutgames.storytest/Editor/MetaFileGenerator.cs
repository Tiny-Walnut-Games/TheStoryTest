using UnityEngine;
using UnityEditor;
using System.IO;

namespace TinyWalnutGames.StoryTest.Editor
{
    /// <summary>
    /// Editor window and context menu for generating missing .meta files in the Unity project.
    /// </summary>
    [InitializeOnLoad]
    public class MetaFileGenerator : EditorWindow
    {
        // Context menu integration for Project window
        [MenuItem("Assets/Generate Meta Files for Folder", false, 2000)]
    public static void GenerateMetaFilesForSelectedFolder()
    {
        string folderPath = GetSelectedFolderPath();
        if (string.IsNullOrEmpty(folderPath))
        {
            EditorUtility.DisplayDialog("No Folder Selected", "Please select a folder in the Project window.", "OK");
            return;
        }
        GenerateMissingMetas(folderPath);
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Done", $"Meta files generated for all items in {folderPath}.", "OK");
    }

    // Validation: Only enable menu if a folder is selected
    [MenuItem("Assets/Generate Meta Files for Folder", true)]
    public static bool ValidateGenerateMetaFilesForSelectedFolder()
    {
        string folderPath = GetSelectedFolderPath();
        return !string.IsNullOrEmpty(folderPath);
    }

    // Helper to get selected folder path (Unity-relative)
    private static string GetSelectedFolderPath()
    {
        foreach (var obj in Selection.GetFiltered(typeof(DefaultAsset), SelectionMode.Assets))
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (Directory.Exists(Path.GetFullPath(path)))
                return path;
        }
        return null;
    }
    [MenuItem("Tools/Generate Missing Meta Files")]
    public static void ShowWindow()
    {
        GetWindow<MetaFileGenerator>("Meta Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Generate Missing Meta Files", EditorStyles.boldLabel);

        if (GUILayout.Button("Scan & Generate"))
        {
            string projectPath = Application.dataPath;
            string assetsDir = Path.GetDirectoryName(projectPath);

            if (IsDirectoryReadOnly(assetsDir))
            {
                EditorUtility.DisplayDialog(
                    "Directory is Read-Only",
                    "The project directory is currently marked as read-only.\n\n" +
                    "Please make it writable in your OS file explorer, then click OK to continue.",
                    "OK"
                );
            }

            GenerateMissingMetas("Assets");
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Done", "Missing meta files have been generated.", "OK");
        }
    }

    private static void GenerateMissingMetas(string root)
    {
        string fullPath = Path.GetFullPath(root);

        // Process all directories (including root)
        foreach (string dir in Directory.GetDirectories(fullPath, "*", SearchOption.AllDirectories))
        {
            string metaPath = dir + ".meta";
            if (!File.Exists(metaPath))
            {
                WriteUnityMetaFile(metaPath);
            }
        }
        // Also process the root folder itself
        string rootMetaPath = fullPath + ".meta";
        if (!File.Exists(rootMetaPath))
        {
            WriteUnityMetaFile(rootMetaPath);
        }

        // Process all files
        foreach (string file in Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories))
        {
            if (file.EndsWith(".meta")) continue;
            string metaPath = file + ".meta";
            if (!File.Exists(metaPath))
            {
                WriteUnityMetaFile(metaPath);
            }
        }

        // Helper: Write a Unity-style meta file with a new GUID
        static void WriteUnityMetaFile(string metaPath)
        {
            string guid = System.Guid.NewGuid().ToString("N");
            string metaContent =
                "fileFormatVersion: 2\n" +
                "guid: " + guid + "\n" +
                "MonoImporter:\n" +
                "  externalObjects: {}\n" +
                "  serializedVersion: 2\n" +
                "  defaultReferences: []\n" +
                "  executionOrder: 0\n" +
                "  icon: {instanceID: 0}\n" +
                "  userData: \n" +
                "  assetBundleName: \n" +
                "  assetBundleVariant: \n";
            File.WriteAllText(metaPath, metaContent);
        }
    }

    private static bool IsDirectoryReadOnly(string path)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(path);
        return dirInfo.Attributes.HasFlag(FileAttributes.ReadOnly);
    }
}
}