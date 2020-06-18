using System;

namespace ToolBuddy.CommentToTooltip
{
    /// <summary>
    /// The supported comment types for parsing.
    /// </summary>
    [Flags]
    public enum CommentTypes
    {
        /// <summary>
        /// No comments
        /// </summary>
        None = 0,
        /// <summary>
        /// Comments of type: \verbatim/// <summary> something </summary>\endverbatim
        /// More information can be found in the Annex E: "Documentation Comments" of the C# Language specification
        /// </summary>
        SingleLineDocumentation = 1 << 0,
        /// <summary>
        /// Comments of type: \verbatim/**<summary> Comment here </summary>*/ \endverbatim
        /// More information can be found in the Annex E: "Documentation Comments" of the C# Language specification
        /// </summary>
        DelimitedDocumentation = 1 << 1,
        /// <summary>
        /// Comments of type: \verbatim// Comment here \endverbatim
        SingleLine = 1 << 2,

        // <summary>
        // Comments of type: \verbatim/* Comment here */ \endverbatim
        // </summary>
        //Delimited = 1 << 3,

        /// <summary>
        /// All supported comments
        /// </summary>
        All = SingleLineDocumentation | DelimitedDocumentation | SingleLine //| Delimited
    };
}
