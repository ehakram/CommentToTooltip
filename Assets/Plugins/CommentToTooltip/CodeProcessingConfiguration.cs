using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using UnityEngine;

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
        [CanBeNull]
        internal Regex CommentExtractor { get; private set; }

        /// <summary>
        /// Read-only collection of strings indicating the file extensions that will be considered by the code processing.
        /// </summary>
        internal IReadOnlyCollection<string> CompatibleFileExtensions { get; private set; }

        /// <summary>
        /// The <see cref="CommentTypes"/> that are processed by this code processor
        /// </summary>
        internal CommentTypes CommentTypes { get; private set; }

        internal bool CanIProcessThisExt(string ext)
        {
            return CompatibleFileExtensions.Contains(ext.ToLowerInvariant());
        }
        

        /// <summary>
        /// refactored constructor
        /// </summary>
        /// <param name="parser">thing that extracts from the code documentation lines</param>
        /// <param name="commentExtractor">regex for extracting the summary from summary tags (optional) </param>
        /// <param name="commentTypes">supported comment types for parsing</param>
        /// <param name="compatibleFileExtensions">filetypes that this is compatible with</param>
        internal CodeProcessingConfiguration(
            Regex parser,
            [CanBeNull] Regex commentExtractor,
            CommentTypes commentTypes,
            params string[] compatibleFileExtensions
        )
        {
            Parser = parser;
            CommentExtractor = commentExtractor;
            CommentTypes = commentTypes;
            HashSet<string> exts = new HashSet<string>();
            foreach (var ext in compatibleFileExtensions)
            {
                exts.Add($".{ext.ToLowerInvariant()}");
            }
            exts.TrimExcess();
            CompatibleFileExtensions = exts;
        }
        
        
    }
    /*! \endcond */
}