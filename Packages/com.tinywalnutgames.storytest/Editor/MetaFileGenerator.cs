using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace TinyWalnutGames.StoryTest.Editor
{
    /// <summary>
    /// Editor window and context menu for generating missing .meta files in the Unity project.
    /// </summary>
    public class MetaFileGenerator : EditorWindow
    {
        // Context menu integration for a Project window
        [MenuItem("Assets/Generate Meta Files for Folder", false, 2000)]
    public static void GenerateMetaFilesForSelectedFolder()
    {
        var folderPath = GetSelectedFolderPath();
        if (string.IsNullOrEmpty(folderPath))
        {
            EditorUtility.DisplayDialog("No Folder Selected", "Please select a folder in the Project window.", "OK");
            return;
        }
        GenerateMissingMetas(folderPath);
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Done", $"Meta files generated for all items in {folderPath}.", "OK");
    }

    // Validation: Only enable the menu if a folder is selected
    [MenuItem("Assets/Generate Meta Files for Folder", true)]
    public static bool ValidateGenerateMetaFilesForSelectedFolder()
    {
        var folderPath = GetSelectedFolderPath();
        return !string.IsNullOrEmpty(folderPath);
    }

    // Helper to get the selected folder path (Unity-relative)
    private static string GetSelectedFolderPath()
    {
        return Selection.GetFiltered(typeof(DefaultAsset), SelectionMode.Assets).Select(AssetDatabase.GetAssetPath).FirstOrDefault(path => Directory.Exists(Path.GetFullPath(path)));
    }
    [MenuItem("Tiny Walnut Games/Tools/Generate Missing Meta Files")]
    public static void ShowWindow()
    {
        GetWindow<MetaFileGenerator>("Meta Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Generate Missing Meta Files", EditorStyles.boldLabel);

        if (!GUILayout.Button("Scan & Generate")) return;
        var projectPath = Application.dataPath;
        var assetsDir = Path.GetDirectoryName(projectPath);

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

    private static void GenerateMissingMetas(string root)
    {
        var fullPath = Path.GetFullPath(root);

        // Process all directories (including root)
        foreach (var dir in Directory.GetDirectories(fullPath, "*", SearchOption.AllDirectories))
        {
            var metaPath = dir + ".meta";
            if (!File.Exists(metaPath))
            {
                WriteUnityMetaFile(metaPath);
            }
        }
        // Also process the root folder itself
        var rootMetaPath = fullPath + ".meta";
        if (!File.Exists(rootMetaPath))
        {
            WriteUnityMetaFile(rootMetaPath);
        }

        // Process all files
        foreach (var file in Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories))
        {
            if (file.EndsWith(".meta")) continue;
            var metaPath = file + ".meta";
            if (!File.Exists(metaPath))
            {
                WriteUnityMetaFile(metaPath);
            }
        }

        return;

        // Helper: Write a Unity-style meta file with a new GUID
        static void WriteUnityMetaFile(string metaPath)
        {
            var guid = System.Guid.NewGuid().ToString("N");
            var metaContent =
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
        var dirInfo = new DirectoryInfo(path);
        return dirInfo.Attributes.HasFlag(FileAttributes.ReadOnly);
    }
}
}