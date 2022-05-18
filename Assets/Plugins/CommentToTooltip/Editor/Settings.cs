using UnityEditor;

namespace ToolBuddy.CommentToTooltip.Editor
{
    public class Settings
    {
        private const string editorPreferencesKey1 = "C2T_SingleLineDocumentation";
        private const string editorPreferencesKey2 = "C2T_DelimitedDocumentation";
        private const string editorPreferencesKey3 = "C2T_SingleLine";
        private const string editorPreferencesKey4 = "C2T_LogFile";
        private const string editorPreferencesKey5 = "C2T_LogExceptionsFile";

        public bool ParseSingleLineDocumentationComments { get; set; }
        public bool ParseDelimitedDocumentationComments { get; set; }
        public bool ParseSingleLineComments { get; set; }
        
        /// <summary>
        /// allows the user to toggle whether or not they want to log stuff to 'Comment To Tooltips_log.txt'
        /// </summary>
        public bool EnableLoggingToFile { get; set; }
        
        /// <summary>
        /// allows exceptions to be logged to file regardless of whether or not everything else is logged.
        /// </summary>
        public bool EnableLoggingExceptionsToFile { get; set; }

        public void ReadFromEditorPreferences()
        {
            ParseSingleLineDocumentationComments = EditorPrefs.GetBool(editorPreferencesKey1, true);
            ParseDelimitedDocumentationComments = EditorPrefs.GetBool(editorPreferencesKey2, true);
            ParseSingleLineComments = EditorPrefs.GetBool(editorPreferencesKey3, true);
            EnableLoggingToFile = EditorPrefs.GetBool(editorPreferencesKey4, true);
            EnableLoggingExceptionsToFile = EditorPrefs.GetBool(editorPreferencesKey5, true);
        }

        public void WriteToEditorPreferences()
        {
            EditorPrefs.SetBool(editorPreferencesKey1, ParseSingleLineDocumentationComments);
            EditorPrefs.SetBool(editorPreferencesKey2, ParseDelimitedDocumentationComments);
            EditorPrefs.SetBool(editorPreferencesKey3, ParseSingleLineComments);
            EditorPrefs.SetBool(editorPreferencesKey4, EnableLoggingToFile);
            EditorPrefs.SetBool(editorPreferencesKey5, EnableLoggingToFile || EnableLoggingExceptionsToFile);
        }

        public CommentTypes GetCommenTypes()
        {
            CommentTypes result = CommentTypes.None;
            if (ParseSingleLineDocumentationComments)
                result |= CommentTypes.SingleLineDocumentation;
            if (ParseDelimitedDocumentationComments)
                result |= CommentTypes.DelimitedDocumentation;
            if (ParseSingleLineComments)
                result |= CommentTypes.SingleLine;
            return result;
        }
    }
}