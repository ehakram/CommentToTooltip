using UnityEditor;
using UnityEngine;

namespace ToolBuddy.CommentToTooltip.Editor
{
    public class SettingsWindow : EditorWindow
    {
        private const string label = "Select the comment types to process";
        private const string toggle1Text = @"/// <summary> Comment here </summary>";
        private const string toggle2Text = @"/**<summary> Comment here </summary>*/";
        private const string toggle3Text = @"// Comment here";

        private const string enableLogsLabel = "Logging";
        private const string enableLogsToggleText = "Log all messages to Assets/Comment To Tooltip_logs.txt";
        private const string enableExceptionLogsToggleText = "Log all exceptions to Assets/Comment To Tooltip_logs.txt";

        public SettingsWindow()
        {
            minSize = new Vector2(350, 100);
        }

        public void OnGUI()
        {
            GUILayout.Label(label, EditorStyles.boldLabel);

            var bigLabelWidth = EditorGUIUtility.labelWidth;
            var smallLabelWidth = EditorGUIUtility.currentViewWidth * 0.9f;
            
            EditorGUIUtility.labelWidth = smallLabelWidth;

            Menu.Settings.ParseSingleLineDocumentationComments = EditorGUILayout.Toggle(toggle1Text, Menu.Settings.ParseSingleLineDocumentationComments);
            Menu.Settings.ParseDelimitedDocumentationComments = EditorGUILayout.Toggle(toggle2Text, Menu.Settings.ParseDelimitedDocumentationComments);
            Menu.Settings.ParseSingleLineComments = EditorGUILayout.Toggle(toggle3Text, Menu.Settings.ParseSingleLineComments);

            EditorGUIUtility.labelWidth = bigLabelWidth;
            
            GUILayout.Label(enableLogsLabel, EditorStyles.boldLabel);
            EditorGUIUtility.labelWidth = smallLabelWidth;

            Menu.Settings.EnableLoggingToFile = EditorGUILayout.Toggle(enableLogsToggleText, Menu.Settings.EnableLoggingToFile);
            Menu.Settings.EnableLoggingExceptionsToFile = EditorGUILayout.Toggle(
                enableExceptionLogsToggleText,
                Menu.Settings.EnableLoggingExceptionsToFile
            );


        }

        public void OnFocus()
        {
            Menu.Settings.ReadFromEditorPreferences();
        }
        public void OnLostFocus()
        {
            Menu.Settings.WriteToEditorPreferences();
        }
        public void OnDestroy()
        {
            Menu.Settings.WriteToEditorPreferences();
        }
    }
}