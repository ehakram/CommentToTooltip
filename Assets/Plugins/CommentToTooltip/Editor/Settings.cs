using UnityEditor;

namespace ToolBuddy.CommentToTooltip.Editor
{
    public class Settings
    {
        private const string editorPreferencesKey1 = "C2T_SingleLineDocumentation";
        private const string editorPreferencesKey2 = "C2T_DelimitedDocumentation";
        private const string editorPreferencesKey3 = "C2T_SingleLine";

        public bool ParseSingleLineDocumentationComments { get; set; }
        public bool ParseDelimitedDocumentationComments { get; set; }
        public bool ParseSingleLineComments { get; set; }

        public void ReadFromEditorPreferences()
        {
            ParseSingleLineDocumentationComments = EditorPrefs.GetBool(editorPreferencesKey1, true);
            ParseDelimitedDocumentationComments = EditorPrefs.GetBool(editorPreferencesKey2, true);
            ParseSingleLineComments = EditorPrefs.GetBool(editorPreferencesKey3, true);
        }

        public void WriteToEditorPreferences()
        {
            EditorPrefs.SetBool(editorPreferencesKey1, ParseSingleLineDocumentationComments);
            EditorPrefs.SetBool(editorPreferencesKey2, ParseDelimitedDocumentationComments);
            EditorPrefs.SetBool(editorPreferencesKey3, ParseSingleLineComments);
        }

        public CommentTypes GetCommenTypes()
        {
            CommentTypes result = CommentTypes.None;
            if (ParseSingleLineDocumentationComments)
                result = result | CommentTypes.SingleLineDocumentation;
            if (ParseDelimitedDocumentationComments)
                result = result | CommentTypes.DelimitedDocumentation;
            if (ParseSingleLineComments)
                result = result | CommentTypes.SingleLine;
            return result;
        }
    }
}