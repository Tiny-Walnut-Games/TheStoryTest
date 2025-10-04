#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TinyWalnutGames.StoryTest;
using TinyWalnutGames.StoryTest.Shared;

namespace TinyWalnutGames.StoryTest.Editor
{
    public static class StoryTestExportMenu
    {
        // MenuItem requires const, so we use default path that matches StoryTestSettings default
        private const string MenuPath = "Tiny Walnut Games/The Story Test/Run Story Test and Export Report";
        
        // � PLANNED FEATURE: Async sync-point validation and export
        // Will be implemented in Phase 3+ when StoryTestSyncPointValidator is available
        /*
        [MenuItem(MenuPath)]
        public static async void RunAndExportStoryTest()
        {
            var settings = StoryTestSettings.Instance;
            string exportPath = System.IO.Path.Combine(Application.dataPath, "..", settings.exportPath);
            Debug.Log($"Running story test and exporting to: {exportPath}");
            await StoryTestSyncPointValidator.QuickSyncPointTestAndExport(exportPath);
            EditorUtility.RevealInFinder(exportPath);
        }
        */
    }
}
#endif
