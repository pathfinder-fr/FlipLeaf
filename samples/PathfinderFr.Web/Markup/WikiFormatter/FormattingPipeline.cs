namespace PathfinderFr.Markup.WikiFormatter
{
    internal static class FormattingPipeline
    {
        /// <summary>
        /// Performs the Phases 1 and 2 of the formatting process.
        /// </summary>
        /// <param name="raw">The raw WikiMarkup to format.</param>
        /// <param name="forIndexing">A value indicating whether the formatting is being done for content indexing.</param>
        /// <param name="context">The formatting context.</param>
        /// <param name="current">The current Page, if any.</param>
        /// <returns>The formatted content.</returns>
        public static string FormatWithPhase1And2(string raw, bool forIndexing, FormattingContext context, PageInfo current) 
            => FormatWithPhase1And2(raw, forIndexing, context, current, out var tempLinks);

        /// <summary>
        /// Performs the Phases 1 and 2 of the formatting process.
        /// </summary>
        /// <param name="raw">The raw WikiMarkup to format.</param>
        /// <param name="forIndexing">A value indicating whether the formatting is being done for content indexing.</param>
        /// <param name="context">The formatting context.</param>
        /// <param name="current">The current Page, if any.</param>
        /// <param name="linkedPages">The Pages linked by the current Page.</param>
        /// <returns>The formatted content.</returns>
        public static string FormatWithPhase1And2(string raw, bool forIndexing, FormattingContext context, PageInfo current, out IList<string> linkedPages)
        {
            throw new NotImplementedException();
            //return new Formatter().Format(raw, forIndexing, context, current, out linkedPages);
        }

        /// <summary>
        /// Performs the Phase 3 of the formatting process.
        /// </summary>
        /// <param name="raw">The raw WikiMarkup to format.</param>
        /// <param name="context">The formatting context.</param>
        /// <param name="current">The current Page, if any.</param>
        /// <returns>The formatted content.</returns>
        public static string FormatWithPhase3(string raw, FormattingContext context, PageInfo current)
        {
            throw new NotImplementedException();
            //return new Formatter().FormatPhase3(raw, context, current);
        }

        /// <summary>
        ///     Contains information about the Context of the page formatting.
        /// </summary>
        public class ContextInformation
        {
            /// <summary>
            ///     Initializes a new instance of the <b>FormatContext</b> class.
            /// </summary>
            /// <param name="forIndexing">A value indicating whether the formatting is being done for content indexing.</param>
            /// <param name="forWysiwyg">A value indicating whether the formatting is being done for display in the WYSIWYG editor.</param>
            /// <param name="context">The formatting context.</param>
            /// <param name="page">The Page Information, if any, <c>null</c> otherwise.</param>
            /// <param name="language">The current Thread's language (for example "en-US").</param>
            /// <param name="httpContext">The current HTTP Context object.</param>
            /// <param name="username">The current User's Username (or <c>null</c>).</param>
            /// <param name="groups">The groups the user is member of (or <c>null</c>).</param>
            public ContextInformation(bool forIndexing, bool forWysiwyg, FormattingContext context, PageInfo page,
                string language)
            {
                ForIndexing = forIndexing;
                ForWysiwyg = forWysiwyg;
                Context = context;
                Page = page;
                Language = language;
                Username = null;
                Groups = Enumerable.Empty<string>();
            }

            /// <summary>
            ///     Gets a value indicating whether the formatting is being done for content indexing.
            /// </summary>
            public bool ForIndexing { get; }

            /// <summary>
            ///     Gets a value indicating whether the formatting is being done for display in the WYSIWYG editor.
            /// </summary>
            public bool ForWysiwyg { get; }

            /// <summary>
            ///     Gets the formatting context.
            /// </summary>
            public FormattingContext Context { get; }

            /// <summary>
            ///     Gets the Page Information.
            /// </summary>
            public PageInfo Page { get; }

            /// <summary>
            ///     Gets the current Thread's Language (for example en-US).
            /// </summary>
            public string Language { get; }

            /// <summary>
            ///     Gets the Username of the current User (or <c>null</c>).
            /// </summary>
            /// <remarks>If the Username is not available, the return value is <c>null</c>.</remarks>
            public string Username { get; }

            /// <summary>
            ///     Gets the groups the user is member of (or <c>null</c>).
            /// </summary>
            public IEnumerable<string> Groups { get; }
        }

        /// <summary>
        ///     Enumerates formatting Phases.
        /// </summary>
        public enum FormattingPhase
        {
            /// <summary>
            ///     Phase 1, performed before the internal formatting step.
            /// </summary>
            Phase1,

            /// <summary>
            ///     Phase 2, performed after the internal formatting step.
            /// </summary>
            Phase2,

            /// <summary>
            ///     Phase 3, performed before sending the page content to the client.
            /// </summary>
            Phase3
        }
    }

}
