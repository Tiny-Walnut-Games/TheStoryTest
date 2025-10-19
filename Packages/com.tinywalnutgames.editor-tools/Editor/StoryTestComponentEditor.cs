using UnityEditor;
using UnityEngine;
using TinyWalnutGames.StoryTest;

namespace TinyWalnutGames.EditorTools
{
    /// <summary>
    /// Custom Inspector for ProductionExcellenceStoryTest with manual validation trigger.
    /// </summary>
    [CustomEditor(typeof(ProductionExcellenceStoryTest))]
    public class StoryTestComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var component = (ProductionExcellenceStoryTest)target;

            EditorGUILayout.Space(10);

            // Big validation button at the top
            if (Application.isPlaying)
            {
                GUI.backgroundColor = component.IsValidating ? Color.yellow : Color.green;
                GUI.enabled = !component.IsValidating;

                if (GUILayout.Button(component.IsValidating ? "Validation Running..." : "▶ Run Validation", GUILayout.Height(40)))
                {
                    component.ValidateOnDemand();
                    Debug.Log("[Story Test] Manual validation triggered from Inspector");
                }

                GUI.enabled = true;
                GUI.backgroundColor = Color.white;

                if (component.IsValidating)
                {
                    EditorGUILayout.HelpBox("Validation in progress. Check Console for status updates.", MessageType.Info);
                }

                if (component.LastReport != null)
                {
                    EditorGUILayout.Space(5);
                    var violations = component.LastReport.StoryViolations.Count;
                    var messageType = violations == 0 ? MessageType.Info : MessageType.Warning;
                    EditorGUILayout.HelpBox($"Last run: {violations} violation(s) found", messageType);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Enter PlayMode to run validation", MessageType.Info);
            }

            EditorGUILayout.Space(10);

            // Show default inspector below
            DrawDefaultInspector();

            // Runtime-only controls
            if (Application.isPlaying)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);

                if (GUILayout.Button("Force Stop Validation"))
                {
                    component.StopAllCoroutines();
                    Debug.LogWarning("[Story Test] Validation forcefully stopped");
                }
            }
        }
    }
}