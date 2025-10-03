#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TinyWalnutGames.StoryTest;

namespace TinyWalnutGames.StoryTest.Editor
{
    public static class StoryTestExportMenu
    {
        [MenuItem("Tiny Walnut Games/The Story Test/Run Story Test and Export Report")]
        public static async void RunAndExportStoryTest()
        {
            string exportPath = System.IO.Path.Combine(Application.dataPath, "../.debug/storytest_report.txt");
            Debug.Log($"Running story test and exporting to: {exportPath}");
            await StoryTestSyncPointValidator.QuickSyncPointTestAndExport(exportPath);
            EditorUtility.RevealInFinder(exportPath);
        }
    }
}
#endif
