using System.Text.RegularExpressions;

namespace ToolBuddy.CommentToTooltip
{
    /*! \cond PRIVATE */
    /// <summary>
    /// Defines the parameters for processing (parsing) code to extract the comments to be put in the tooltips
    /// </summary>
    internal class CodeProcessingConfiguration
    {
        /// <summary>
        /// Regex that extracts from the code the documentation lines
        /// </summary>
        internal Regex Parser { get; private set; }

        /// <summary>
        /// Regex that transform documentation lines to comment lines.
        /// Is optional: If no documentation to comment transformation needed, set this to null
        /// </summary>
        internal Regex CommentExtractor { get; private set; }

        /// <summary>
        /// A list of file extensions - spearated with an ';' -  that will be considered in the code processing
        /// </summary>
        internal string CompatibleFileExtensions { get; private set; }

        /// <summary>
        /// The <see cref="CommentTypes"/> that are processed by this code processor
        /// </summary>
        internal CommentTypes CommentTypes { get; private set; }

        internal CodeProcessingConfiguration(Regex parser, Regex commentExtractor, string compatibleFileExtensions, CommentTypes commentType)
        {
            Parser = parser;
            CommentExtractor = commentExtractor;
            CompatibleFileExtensions = compatibleFileExtensions;
            CommentTypes = commentType;
        }
    }
    /*! \endcond */
}