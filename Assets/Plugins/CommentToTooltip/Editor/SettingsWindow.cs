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

        public SettingsWindow()
        {
            minSize = new Vector2(350, 100);
        }

        public void OnGUI()
        {
            GUILayout.Label(label, EditorStyles.boldLabel);

            EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth * 0.9f;

            Menu.Settings.ParseSingleLineDocumentationComments = EditorGUILayout.Toggle(toggle1Text, Menu.Settings.ParseSingleLineDocumentationComments);
            Menu.Settings.ParseDelimitedDocumentationComments = EditorGUILayout.Toggle(toggle2Text, Menu.Settings.ParseDelimitedDocumentationComments);
            Menu.Settings.ParseSingleLineComments = EditorGUILayout.Toggle(toggle3Text, Menu.Settings.ParseSingleLineComments);
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